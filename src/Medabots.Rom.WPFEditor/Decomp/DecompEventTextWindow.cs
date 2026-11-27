using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Medabots.Rom.Text;
using Medabots.Rom.WPFEditor.Dialogs;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Controls.ListBox;
using ListBoxItem = System.Windows.Controls.ListBoxItem;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using TextBox = System.Windows.Controls.TextBox;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using SystemColors = System.Windows.SystemColors;

namespace Medabots.Rom.WPFEditor.Decomp;

/// <summary>
/// Event scripts of a decomp project as text (data/events/NNNN.evt, tools/events_text.py; plan M6). Script list,
/// the text with one command per line, the opcode list (double click inserts a line), who calls the script (NPCs of
/// data/maps, Goto_Event of other scripts) and the line of the message under the caret. Save writes the file and
/// assembles it (events_text.py --check-one); New makes the next free id; Delete removes the last new script
/// (events_text.py --delete: never a game script, never one still called); Check runs tools/story_check.py. Each save,
/// new or delete tells the main window, so its Events tab and Maps panel show the same script.
/// </summary>
public sealed partial class DecompEventTextWindow : Window
{
    private readonly string _root;
    private readonly string _events;
    private readonly string? _romPath;
    private readonly ListBox _list = new() { MinWidth = 170 };
    private readonly TextBox _filter = new() { Margin = new Thickness(0, 0, 0, 4) };
    private readonly TextBox _text = new()
    {
        FontFamily = new FontFamily("Consolas"), FontSize = 13, AcceptsReturn = true, AcceptsTab = true,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        TextWrapping = TextWrapping.NoWrap,
    };
    private readonly ListBox _ops = new() { FontFamily = new FontFamily("Consolas"), FontSize = 11 };
    private readonly ListBox _callers = new() { FontSize = 11, MaxHeight = 160 };
    private readonly TextBlock _header = new() { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) };
    private readonly TextBlock _preview = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
    private List<(int Id, string Label)> _all = [];
    private int _current = -1;
    private string _currentFile = "";
    private bool _dirty;
    private byte[]? _rom;
    private Dictionary<string, int>? _symbols;

    public DecompEventTextWindow(string root, string? baseRomPath, int? openId = null)
    {
        _root = root;
        _events = Path.Combine(root, "data", "events");
        _romPath = baseRomPath;
        Title = "Event scripts (.evt) - " + root;
        Width = 1150;
        Height = 720;
        Content = BuildLayout();
        LoadOps();
        LoadList();
        _filter.TextChanged += (_, _) => ApplyFilter();
        _list.SelectionChanged += (_, _) =>
        {
            if (_list.SelectedItem is ListBoxItem { Tag: int id } && id != _current)
            {
                Open(id);
            }
        };
        _text.TextChanged += (_, _) => _dirty = true;
        _text.SelectionChanged += (_, _) => UpdatePreview();
        _ops.MouseDoubleClick += (_, _) => InsertOp();
        Closing += (_, e) =>
        {
            if (_dirty && MessageBox.Show(this, "Discard the unsaved changes?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        };
        Open(openId ?? 0);
    }

    private UIElement BuildLayout()
    {
        var grid = new Grid { Margin = new Thickness(8) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });

        var left = new DockPanel { Margin = new Thickness(0, 0, 8, 0) };
        var newButton = new Button { Content = "New script", Padding = new Thickness(6, 2, 6, 2) };
        newButton.Click += async (_, _) => await NewScriptAsync();
        var deleteButton = new Button
        {
            Content = "Delete script", Padding = new Thickness(6, 2, 6, 2), Margin = new Thickness(6, 0, 0, 0),
            ToolTip = "Deletes the script shown. Only a new script (not one of the game), only the last id, and only when nothing calls it.",
        };
        deleteButton.Click += async (_, _) => await DeleteScriptAsync();
        var leftButtons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
        leftButtons.Children.Add(newButton);
        leftButtons.Children.Add(deleteButton);
        DockPanel.SetDock(_filter, Dock.Top);
        DockPanel.SetDock(leftButtons, Dock.Bottom);
        left.Children.Add(_filter);
        left.Children.Add(leftButtons);
        left.Children.Add(_list);
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        var center = new DockPanel();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
        var save = new Button { Content = "Save", Padding = new Thickness(14, 2, 14, 2) };
        save.Click += async (_, _) => await SaveAsync();
        var check = new Button { Content = "Check storyline", Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(6, 0, 0, 0) };
        check.Click += async (_, _) => await CheckStoryAsync();
        buttons.Children.Add(save);
        buttons.Children.Add(check);
        var bottom = new StackPanel();
        bottom.Children.Add(buttons);
        bottom.Children.Add(_status);
        DockPanel.SetDock(_header, Dock.Top);
        DockPanel.SetDock(bottom, Dock.Bottom);
        center.Children.Add(_header);
        center.Children.Add(bottom);
        center.Children.Add(_text);
        Grid.SetColumn(center, 1);
        grid.Children.Add(center);

        var right = new DockPanel { Margin = new Thickness(8, 0, 0, 0) };
        var callersHead = new TextBlock { Text = "Called by", FontWeight = FontWeights.SemiBold };
        var previewHead = new TextBlock { Text = "Message under the caret", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) };
        var opsHead = new TextBlock { Text = "Commands (double click inserts)", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) };
        foreach (var e in new UIElement[] { callersHead, _callers, previewHead, _preview, opsHead })
        {
            DockPanel.SetDock(e, Dock.Top);
            right.Children.Add(e);
        }

        right.Children.Add(_ops);
        Grid.SetColumn(right, 2);
        grid.Children.Add(right);
        return grid;
    }

    private static JsonElement ReadJson(string path) => JsonDocument.Parse(File.ReadAllText(path)).RootElement.Clone();

    private void LoadOps()
    {
        var doc = ReadJson(Path.Combine(_events, "_opcodes.json"));
        foreach (var op in doc.GetProperty("opcodes").EnumerateObject())
        {
            var name = op.Value.GetProperty("name").GetString()!;
            var args = op.Value.GetProperty("args").EnumerateArray()
                .Select(a => a.GetProperty("name").GetString() + "=" + (a.GetProperty("name").GetString() == "jump" ? "@L_" : "0")).ToArray();
            var line = name == "Switch_Event_Counter" ? "Switch_Event_Counter @L_a, @L_b" : (name + (args.Length > 0 ? " " + string.Join(", ", args) : ""));
            _ops.Items.Add(new ListBoxItem { Content = $"{op.Name} {name}", Tag = line });
        }
    }

    private void LoadList()
    {
        var all = new List<(int, string)>();
        for (var id = 0; File.Exists(Path.Combine(_events, $"{id:D4}.json")); id++)
        {
            var doc = ReadJson(Path.Combine(_events, $"{id:D4}.json"));
            var label = $"{id:D4}";
            if (doc.TryGetProperty("name", out var n))
            {
                label += "  " + n.GetString();
            }

            if (doc.TryGetProperty("shared_with", out var s))
            {
                label += $"  (= {s.GetInt32():D4})";
            }

            all.Add((id, label));
        }

        _all = all;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var f = _filter.Text.Trim();
        _list.Items.Clear();
        foreach (var (id, label) in _all.Where(x => f.Length == 0 || x.Label.Contains(f, StringComparison.OrdinalIgnoreCase)))
        {
            var item = new ListBoxItem { Content = label, Tag = id };
            _list.Items.Add(item);
            if (id == _current)
            {
                _list.SelectedItem = item;
            }
        }
    }

    private void Open(int id)
    {
        if (_dirty && _current >= 0 &&
            MessageBox.Show(this, $"Discard the changes to script {_current}?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            Select(_current);
            return;
        }

        var doc = ReadJson(Path.Combine(_events, $"{id:D4}.json"));
        var owner = id;
        var shared = doc.TryGetProperty("shared_with", out var s);
        if (shared)
        {
            owner = s.GetInt32();
            doc = ReadJson(Path.Combine(_events, $"{owner:D4}.json"));
        }

        var users = _all.Count(x => x.Id != owner && ReadSharedWith(x.Id) == owner);
        _current = id;
        _currentFile = Path.Combine(_events, doc.GetProperty("script").GetString()!);
        _text.Text = File.ReadAllText(_currentFile);
        var first = Regex.Match(_text.Text, @"^[^;\r\n]", RegexOptions.Multiline);   // caret on the first command
        _text.CaretIndex = first.Success ? first.Index : 0;
        _dirty = false;
        _header.Text = $"Script {id}" + (shared ? $": same script as {owner}" : "") +
                       (users > 0 ? $" ({users} other ids use this script too; saving changes them all)" : "") +
                       "   -  " + Path.GetFileName(_currentFile);
        _status.Text = "";
        Select(id);
        LoadCallers(id);
        UpdatePreview();
    }

    private int? ReadSharedWith(int id)
    {
        var doc = ReadJson(Path.Combine(_events, $"{id:D4}.json"));
        return doc.TryGetProperty("shared_with", out var s) ? s.GetInt32() : null;
    }

    private void Select(int id)
    {
        foreach (ListBoxItem item in _list.Items)
        {
            if (item.Tag is int t && t == id)
            {
                _list.SelectedItem = item;
                _list.ScrollIntoView(item);
                return;
            }
        }
    }

    private void LoadCallers(int id)
    {
        _callers.Items.Clear();
        var maps = Path.Combine(_root, "data", "maps");
        for (var area = 0; Directory.Exists(Path.Combine(maps, $"area_{area:D3}")); area++)
        {
            var a = ReadJson(Path.Combine(maps, $"area_{area:D3}", "area.json"));
            var npcs = ReadJson(Path.Combine(maps, a.GetProperty("npcs").GetString()!, "npcs.json"));
            foreach (var n in npcs.GetProperty("npcs").EnumerateArray().Where(n => n.GetProperty("event").GetInt32() == id))
            {
                _callers.Items.Add($"NPC  area {area}  ({n.GetProperty("x").GetInt32()},{n.GetProperty("y").GetInt32()})");
            }
        }

        var gotoLine = new Regex($@"^Goto_Event\s+event={id}\s*$", RegexOptions.Multiline);
        foreach (var evt in Directory.EnumerateFiles(_events, "*.evt"))
        {
            if (gotoLine.IsMatch(File.ReadAllText(evt)))
            {
                _callers.Items.Add($"Goto_Event in {Path.GetFileNameWithoutExtension(evt)}");
            }
        }

        if (_callers.Items.Count == 0)
        {
            _callers.Items.Add("(nobody in data/maps or data/events)");
        }
    }

    private void InsertOp()
    {
        if (_ops.SelectedItem is not ListBoxItem { Tag: string line })
        {
            return;
        }

        var at = _text.CaretIndex;
        var lineStart = at == 0 ? 0 : _text.Text.LastIndexOf('\n', at - 1) + 1;
        _text.Text = _text.Text.Insert(lineStart, line + Environment.NewLine);
        _text.CaretIndex = lineStart + line.Length;
        _text.Focus();
    }

    private static readonly Regex MessageLine = new(@"Show_Message_[AB]\s+bank=(-?\d+),\s*id=(\d+)");

    private void UpdatePreview()
    {
        var at = Math.Min(_text.CaretIndex, _text.Text.Length);
        var start = at == 0 ? 0 : _text.Text.LastIndexOf('\n', at - 1) + 1;
        var end = _text.Text.IndexOf('\n', at);
        var line = _text.Text[start..(end < 0 ? _text.Text.Length : end)];
        var m = MessageLine.Match(line);
        if (!m.Success)
        {
            _preview.Text = "(put the caret on a Show_Message line)";
            return;
        }

        var bank = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        var id = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
        _preview.Text = ProjectMessage(bank, id) ?? GameMessage(bank, id) ?? $"bank {bank}, id {id}: not found";
    }

    /// <summary>A line of the project's own banks (data/story/text/bank_NN.json), as written there.</summary>
    private string? ProjectMessage(int bank, int id)
    {
        var file = Path.Combine(_root, "data", "story", "text", $"bank_{bank}.json");
        if (!File.Exists(file))
        {
            return null;
        }

        var msgs = ReadJson(file).GetProperty("messages");
        return id < msgs.GetArrayLength() ? "(project) " + msgs[id].GetString()!.Replace("||", "\n\n").Replace("|", "\n") : null;
    }

    /// <summary>A line of the game, read from the build's base ROM through its .map (gTextBankTables / gTextTable).</summary>
    private string? GameMessage(int bank, int id)
    {
        try
        {
            if (_romPath is null || !File.Exists(_romPath))
            {
                return null;
            }

            _rom ??= File.ReadAllBytes(_romPath);
            _symbols ??= ReadMap(Path.ChangeExtension(_romPath, ".map"));
            uint U32(int o) => BitConverter.ToUInt32(_rom, o);
            int table;
            if (bank < 0)
            {
                table = _symbols["gTextTable"];
            }
            else
            {
                table = (int)(U32(_symbols["gTextBankTables"] + 4 * bank) - 0x08000000);
            }

            var ptr = (int)(U32(table + 4 * id) - 0x08000000);
            return new MedabotsTextCodec().DecodeMessage(_rom, ptr);
        }
        catch (Exception ex) when (ex is KeyNotFoundException or ArgumentException or IndexOutOfRangeException or IOException)
        {
            return null;
        }
    }

    private static Dictionary<string, int> ReadMap(string mapFile)
    {
        var symbols = new Dictionary<string, int>();
        var re = new Regex(@"^\s+0x([0-9a-fA-F]{8})\s+(gTextBankTables|gTextTable)\s*$");
        foreach (var line in File.ReadLines(mapFile))
        {
            var m = re.Match(line);
            if (m.Success)
            {
                symbols[m.Groups[2].Value] = int.Parse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture) - 0x08000000;
            }
        }

        return symbols;
    }

    private async Task SaveAsync()
    {
        if (_current < 0)
        {
            return;
        }

        var before = File.ReadAllText(_currentFile);
        File.WriteAllText(_currentFile, _text.Text);
        var (code, output) = await DecompBuild.RunToolAsync(_root, DecompProjectWindow.SavedPython(), "events_text.py",
            "--check-one", _current.ToString(CultureInfo.InvariantCulture));
        if (code != 0)
        {
            File.WriteAllText(_currentFile, before);
            _status.Foreground = Brushes.IndianRed;
            _status.Text = "Not saved: " + output;
            return;
        }

        _dirty = false;
        _status.Foreground = Brushes.SeaGreen;
        _status.Text = "Saved. " + output + "  (Build ROM puts it in the game.)";
        TellMainWindow(_current);
    }

    /// <summary>The main window rereads this script (Events tab, Maps panel).</summary>
    private void TellMainWindow(int id)
    {
        if (Owner is MainWindow main)
        {
            main.ReloadDecompEvent(id);
        }
    }

    /// <summary>
    /// The main window changed a script on disk (Events tab edit, Maps "Add event here"): the list is read again, and
    /// the script shown is reloaded when it is that one and has no unsaved typing.
    /// </summary>
    public void ReloadFromDisk(int id)
    {
        LoadList();
        if (id == _current && !_dirty)
        {
            Open(id);
            _status.Foreground = SystemColors.ControlTextBrush;
            _status.Text = "Reloaded: changed in the Events tab.";
        }
    }

    private async Task DeleteScriptAsync()
    {
        if (_current < 0)
        {
            return;
        }

        var id = _current;
        if (MessageBox.Show(this, $"Delete script {id} ({id:D4}.evt and {id:D4}.json)?", Title, MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        var (code, output) = await DecompBuild.RunToolAsync(_root, DecompProjectWindow.SavedPython(), "events_text.py",
            "--delete", id.ToString(CultureInfo.InvariantCulture));
        if (code != 0)
        {
            _status.Foreground = Brushes.IndianRed;
            _status.Text = "Not deleted: " + output;
            return;
        }

        _dirty = false;
        LoadList();
        Open(Math.Max(0, id - 1));
        _status.Foreground = Brushes.SeaGreen;
        _status.Text = output;
        TellMainWindow(id);
    }

    private async Task NewScriptAsync()
    {
        var (code, output) = await DecompBuild.RunToolAsync(_root, DecompProjectWindow.SavedPython(), "events_text.py", "--new");
        var m = Regex.Match(output, @"roteiro novo: (\d+)");
        if (code != 0 || !m.Success)
        {
            _status.Text = output;
            return;
        }

        LoadList();
        Open(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture));
        _status.Text = $"New script {_current}. Put its id in an NPC (Maps tab) or a Goto_Event to call it.";
        TellMainWindow(_current);
    }

    private async Task CheckStoryAsync()
    {
        var (_, output) = await DecompBuild.RunToolAsync(_root, DecompProjectWindow.SavedPython(), "story_check.py");
        _status.Foreground = output.Contains("ERRO", StringComparison.Ordinal) ? Brushes.IndianRed : SystemColors.ControlTextBrush;
        _status.Text = output;
    }
}
