using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Expansion;
using Medabots.Rom.Layout;
using Medabots.Rom.Text;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ComboBoxItem = System.Windows.Controls.ComboBoxItem;
using GroupBox = System.Windows.Controls.GroupBox;
using Grid = System.Windows.Controls.Grid;
using Image = System.Windows.Controls.Image;
using ListBoxItem = System.Windows.Controls.ListBoxItem;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using Separator = System.Windows.Controls.Separator;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = System.Windows.Controls.TextBlock;
using TextBox = System.Windows.Controls.TextBox;
using WrapPanel = System.Windows.Controls.WrapPanel;

namespace Medabots.Rom.WPFEditor.Dialogs;

/// <summary>
/// Builds new Medabots on top of existing bots' bodies: names and stats start with the base bot's
/// values, and each drawable piece shows the base picture until a PNG replaces it. The bots form a
/// list that can be saved and reopened (bot list file, pictures in a folder next to it) and are added
/// together, in list order, to one copy of the expanded ROM. Single bots import/export as data/bots JSON.
/// </summary>
public partial class NewBotWindow : Window
{
    private const string JsonFilter = "Medabot definition (*.json)|*.json|All files (*.*)|*.*";
    private const string ListFilter = "Medabot bot list (*.json)|*.json|All files (*.*)|*.*";
    private const string PngFilter = "PNG picture (*.png)|*.png";
    private const string Caption = "New Medabots";
    private const int Zoom = 3;

    // Bases most people start from; every bot of the ROM is listed after them.
    private static readonly int[] CommonBases = [43, 42, 73, 8, 14, 40];

    private static readonly Dictionary<string, string> PartTitles = new()
    {
        ["cabeca"] = "Head", ["braco_dir"] = "Right arm", ["braco_esq"] = "Left arm", ["pernas"] = "Legs", ["mini"] = "Mini sprite (battle)",
    };

    private static readonly string[] PartTitlesByKind = ["Head", "Right arm", "Left arm", "Legs"];

    // Same names as the part editor of the main window (MainWindow.ResetPartEditorLabels), by record byte.
    private static readonly int[] AdvancedBytes = [10, 13, 14, 15];
    private static readonly Brush Checker = CreateChecker();

    private readonly string _romPath;
    private readonly byte[] _rom;
    private readonly RomLayout _layout;
    private readonly MedabotsTextCodec _codec = new();
    private readonly TextBox[] _partNames = new TextBox[4];
    private readonly System.Windows.Controls.Control[][] _stats = new System.Windows.Controls.Control[4][];
    private readonly Dictionary<int, IReadOnlyList<ArtSlot>> _slotsByBase = [];
    private readonly List<BotDraft> _drafts = [];
    private BotDraft _current = null!;
    private IReadOnlyList<ArtSlot> _slots = [];
    private string? _listPath;
    private string _savedSnapshot = "";
    private bool _switching;

    public NewBotWindow(string romPath, byte[] rom)
    {
        InitializeComponent();
        _romPath = romPath;
        _rom = rom;
        _layout = RomLayout.Resolve(rom);
        BuildPartEditors();

        foreach (var id in CommonBases.Where(id => id < _layout.BotCount))
        {
            BaseComboBox.Items.Add(BaseItem(id));
        }

        BaseComboBox.Items.Add(new Separator());
        for (var id = 0; id < _layout.BotCount; id++)
        {
            BaseComboBox.Items.Add(BaseItem(id));
        }

        AddDraft(NewDraft(CommonBases[0]));
        _savedSnapshot = Snapshot();
        Closing += OnWindowClosing;
    }

    /// <summary>The ROM written by "Add all to ROM", when the window closes with OK.</summary>
    public string? OutputPath { get; private set; }

    public string AddedSummary { get; private set; } = "";

    // The editor works on the selected bot's own pictures.
    private Dictionary<string, ArtImage> _art => _current.Art;

    private ComboBoxItem BaseItem(int id) =>
        new() { Content = $"{id:000}  {_layout.ReadBotName(_rom, id, _codec)}", Tag = id };

    /// <summary>One bot of the list: everything the editor shows for it.</summary>
    private sealed class BotDraft
    {
        public int BaseId { get; set; }

        public string Name { get; set; } = "";

        public string[] PartNames { get; } = new string[4];

        public byte[][] Stats { get; } = [new byte[16], new byte[16], new byte[16], new byte[16]];

        public Dictionary<string, ArtImage> Art { get; } = [];

        public IReadOnlyDictionary<string, string>? Recolor { get; set; }

        public BotDraft Clone()
        {
            var copy = new BotDraft { BaseId = BaseId, Name = Name, Recolor = Recolor };
            PartNames.CopyTo(copy.PartNames, 0);
            for (var k = 0; k < 4; k++)
            {
                Stats[k].CopyTo(copy.Stats[k], 0);
            }

            foreach (var (name, picture) in Art)
            {
                copy.Art[name] = picture;
            }

            return copy;
        }
    }

    // ---- the bot list ----

    private BotDraft NewDraft(int baseId)
    {
        var draft = new BotDraft();
        FillFromBase(draft, baseId);
        return draft;
    }

    private void FillFromBase(BotDraft draft, int baseId)
    {
        draft.BaseId = baseId;
        draft.Name = _layout.ReadBotName(_rom, baseId, _codec) ?? "";
        draft.Recolor = null;
        draft.Art.Clear();
        for (var kind = 0; kind < 4; kind++)
        {
            draft.PartNames[kind] = _layout.ReadPartName(_rom, baseId * 4 + kind, _codec) ?? "";
            _rom.AsSpan(_layout.PartTableOffset + (baseId * 4 + kind) * 16, 16).CopyTo(draft.Stats[kind]);
        }
    }

    private BotDraft DraftFrom(BotDefinition definition)
    {
        if (definition.GraphicsFrom < 0 || definition.GraphicsFrom >= _layout.BotCount)
        {
            throw new InvalidDataException($"{definition.Name}: graphics_from {definition.GraphicsFrom} is not a bot of this ROM.");
        }

        var draft = new BotDraft { BaseId = definition.GraphicsFrom, Name = definition.Name, Recolor = definition.Recolor };
        for (var kind = 0; kind < 4; kind++)
        {
            draft.PartNames[kind] = definition.Parts[kind].Name;
            definition.Parts[kind].Stats.CopyTo(draft.Stats[kind], 0);
        }

        return draft;
    }

    private IReadOnlyList<ArtSlot> SlotsFor(int baseId) =>
        _slotsByBase.TryGetValue(baseId, out var slots) ? slots : _slotsByBase[baseId] = BotArt.Slots(_rom, baseId);

    private void AddDraft(BotDraft draft, int? at = null)
    {
        _drafts.Insert(at ?? _drafts.Count, draft);
        ShowDraft(draft);
        RefreshDraftList();
    }

    private void RefreshDraftList()
    {
        _switching = true;
        DraftsListBox.Items.Clear();
        for (var i = 0; i < _drafts.Count; i++)
        {
            var draft = _drafts[i];
            var art = draft.Art.Count > 0 ? $", {draft.Art.Count} drawing(s)" : "";
            DraftsListBox.Items.Add(new ListBoxItem
            {
                Content = $"{_layout.BotCount + i:000}  {(draft.Name.Length > 0 ? draft.Name : "(no name)")}\n        body {draft.BaseId:000}{art}",
                Tag = draft,
                Padding = new Thickness(4, 3, 4, 3),
            });
        }

        DraftsListBox.SelectedIndex = _drafts.IndexOf(_current);
        _switching = false;
        int first = _layout.BotCount, last = _layout.BotCount + _drafts.Count - 1;
        StatusTextBlock.Text = $"{(_listPath is null ? "Unsaved bot list" : Path.GetFileName(_listPath))}: {_drafts.Count} bot(s), " +
                               (first == last ? $"will be bot {first}" : $"will be bots {first}-{last}") +
                               $" in {Path.GetFileName(_romPath)} (has {_layout.BotCount} bots).";
    }

    // Puts a bot in the editor (store the previous one first, see StoreEditor).
    private void ShowDraft(BotDraft draft)
    {
        _current = draft;
        _switching = true;
        BaseComboBox.SelectedItem = BaseComboBox.Items.OfType<ComboBoxItem>().First(item => (int)item.Tag == draft.BaseId);
        _switching = false;
        _slots = SlotsFor(draft.BaseId);
        BotNameTextBox.Text = draft.Name;
        for (var kind = 0; kind < 4; kind++)
        {
            _partNames[kind].Text = draft.PartNames[kind];
            for (var i = 0; i < 16; i++)
            {
                SetStat(_stats[kind][i], draft.Stats[kind][i]);
            }
        }

        RefreshSlots();
    }

    // Copies the editor fields into the selected bot; false (with a message) when a field is invalid.
    private bool StoreEditor()
    {
        try
        {
            ReadEditorInto(_current);
            return true;
        }
        catch (InvalidDataException ex)
        {
            MessageBox.Show(this, ex.Message, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }

    private void ReadEditorInto(BotDraft draft)
    {
        for (var kind = 0; kind < 4; kind++)
        {
            var fields = StatLabels(kind);
            for (var i = 0; i < 16; i++)
            {
                var text = StatText(_stats[kind][i]);
                if (!byte.TryParse(text, out var value))
                {
                    throw new InvalidDataException($"{BotNameTextBox.Text}: {PartTitlesByKind[kind]} {fields[i]}: '{text}' is not a number from 0 to 255.");
                }

                draft.Stats[kind][i] = value;
            }

            draft.PartNames[kind] = _partNames[kind].Text.Trim();
        }

        draft.Name = BotNameTextBox.Text.Trim();
    }

    private void OnDraftSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_switching || DraftsListBox.SelectedItem is not ListBoxItem { Tag: BotDraft draft } || draft == _current)
        {
            return;
        }

        if (!StoreEditor())
        {
            _switching = true;
            DraftsListBox.SelectedIndex = _drafts.IndexOf(_current);
            _switching = false;
            return;
        }

        ShowDraft(draft);
        RefreshDraftList();
    }

    private void OnNewDraftClicked(object sender, RoutedEventArgs e)
    {
        if (!StoreEditor())
        {
            return;
        }

        if (_layout.BotCount + _drafts.Count >= BotAdder.MaxBots)
        {
            MessageBox.Show(this, $"The list already fills the ROM up to bot {BotAdder.MaxBots - 1}.", Caption);
            return;
        }

        AddDraft(NewDraft(_current.BaseId));
    }

    private void OnDuplicateDraftClicked(object sender, RoutedEventArgs e)
    {
        if (StoreEditor() && _layout.BotCount + _drafts.Count < BotAdder.MaxBots)
        {
            AddDraft(_current.Clone(), _drafts.IndexOf(_current) + 1);
        }
    }

    private void OnRemoveDraftClicked(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, $"Remove {_current.Name} from the list?", Caption, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        if (_drafts.Count == 1)
        {
            FillFromBase(_current, _current.BaseId);       // the list always keeps one bot to edit
            ShowDraft(_current);
            RefreshDraftList();
            return;
        }

        var index = _drafts.IndexOf(_current);
        _drafts.RemoveAt(index);
        ShowDraft(_drafts[Math.Min(index, _drafts.Count - 1)]);
        RefreshDraftList();
    }

    private void OnMoveDraftUpClicked(object sender, RoutedEventArgs e) => MoveDraft(-1);

    private void OnMoveDraftDownClicked(object sender, RoutedEventArgs e) => MoveDraft(+1);

    private void MoveDraft(int step)
    {
        var index = _drafts.IndexOf(_current);
        if (index + step < 0 || index + step >= _drafts.Count || !StoreEditor())
        {
            return;
        }

        _drafts.RemoveAt(index);
        _drafts.Insert(index + step, _current);
        RefreshDraftList();
    }

    // ---- base bot ----

    private void OnBaseChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_switching || BaseComboBox.SelectedItem is not ComboBoxItem { Tag: int id } || id == _current.BaseId)
        {
            return;
        }

        if (_art.Count > 0 && MessageBox.Show(this,
                $"Changing the body type discards the {_art.Count} loaded picture(s) (other bodies have other pieces). Continue?",
                Caption, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            _switching = true;
            BaseComboBox.SelectedItem = BaseComboBox.Items.OfType<ComboBoxItem>().First(item => (int)item.Tag == _current.BaseId);
            _switching = false;
            return;
        }

        FillFromBase(_current, id);
        ShowDraft(_current);
        RefreshDraftList();
    }

    // ---- names and stats ----

    private void BuildPartEditors()
    {
        for (var kind = 0; kind < 4; kind++)
        {
            var grid = new Grid { Margin = new Thickness(4) };
            grid.ColumnDefinitions.Add(new() { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
            var advanced = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            advanced.ColumnDefinitions.Add(new() { Width = new GridLength(146) });
            advanced.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
            var labels = StatLabels(kind);
            _partNames[kind] = new TextBox { Padding = new Thickness(3, 2, 3, 2) };
            AddRow(grid, "Name", _partNames[kind]);
            _stats[kind] = new System.Windows.Controls.Control[16];
            for (var i = 0; i < 16; i++)
            {
                _stats[kind][i] = StatEditor(kind, i);
                AddRow(AdvancedBytes.Contains(i) ? advanced : grid, labels[i], _stats[kind][i]);
            }

            var expander = new System.Windows.Controls.Expander
            {
                Header = "Advanced / Unresolved Fields",
                Content = advanced,
                Margin = new Thickness(0, 6, 0, 0),
                ToolTip = "These bytes are preserved on save, but their full gameplay meaning is still being reverse engineered.",
            };
            Grid.SetRow(expander, grid.RowDefinitions.Count);
            Grid.SetColumnSpan(expander, 2);
            grid.RowDefinitions.Add(new() { Height = GridLength.Auto });
            grid.Children.Add(expander);
            PartsPanel.Children.Add(new GroupBox { Header = PartTitlesByKind[kind], Content = grid, Margin = new Thickness(0, 0, 0, 8) });
        }
    }

    private static string[] StatLabels(int kind) => kind == 3
        ? ["Compatibility Attribute", "Leg Type", "Speciality", "Gender Restriction", "Armor", "Propulsion", "Evasion", "Defense",
           "Proximity", "Remoteness", "Bot Shared Value", "Price", "Attack Scalar", "Reserved Tail A", "Reserved Tail B", "Reserved Tail C"]
        : ["Compatibility Attribute", "Technique", "Speciality", "Gender Restriction", "Armor", "Rate of Success", "Power", "Chain Reaction",
           kind == 0 ? "Uses" : "Charge", "Radiation", "Bot Shared Value", "Price", "Attack Scalar", "Reserved Tail A", "Reserved Tail B", "Reserved Tail C"];

    // Dropdowns use the main window's catalog and "000  Name" format; Chain Reaction is a check box there too.
    private static System.Windows.Controls.Control StatEditor(int kind, int index)
    {
        var metadata = Medabots.Rom.Metadata.MedabotsMetadata.Default;
        IEnumerable<(int Value, string Label)>? choices = index switch
        {
            0 => Enumerable.Range(0, 27).Select(id => (id, id < 26 ? $"{metadata.GetPartAttributeName(id)}  ({metadata.GetMedalName(id)} Medal)" : metadata.GetPartAttributeName(id))),
            1 when kind == 3 => Enumerable.Range(100, 7).Select(id => (id, metadata.GetLegTypeName(id))),
            1 => Enumerable.Range(0, metadata.Catalog.Techniques.Count).Select(id => (id, metadata.GetTechniqueName(id))),
            2 => Enumerable.Range(0, metadata.Catalog.Specialities.Count).Select(id => (id, metadata.GetSpecialityName(id))),
            3 => [(0, "Male / Default"), (1, "Female / Alternate")],
            _ => null,
        };

        if (index == 7 && kind != 3)
        {
            return new System.Windows.Controls.CheckBox { Content = "Enabled", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 3, 0, 3) };
        }

        if (choices is null)
        {
            return new TextBox { Width = 60, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Padding = new Thickness(3, 1, 3, 1) };
        }

        var combo = new System.Windows.Controls.ComboBox { MaxDropDownHeight = 420 };
        foreach (var (value, label) in choices)
        {
            combo.Items.Add(new ComboBoxItem { Content = $"{value:D3}  {label}", Tag = value });
        }

        return combo;
    }

    private static void SetStat(System.Windows.Controls.Control editor, int value)
    {
        switch (editor)
        {
            case TextBox box:
                box.Text = value.ToString();
                return;
            case System.Windows.Controls.CheckBox check:
                check.IsChecked = value != 0;
                return;
        }

        var combo = (System.Windows.Controls.ComboBox)editor;
        var item = combo.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == value);
        if (item is null)
        {
            // A value without a catalog name (kept as is).
            item = new ComboBoxItem { Content = $"{value:D3}  Unknown #{value}", Tag = value };
            combo.Items.Add(item);
        }

        combo.SelectedItem = item;
    }

    private static string StatText(System.Windows.Controls.Control editor) => editor switch
    {
        TextBox box => box.Text.Trim(),
        System.Windows.Controls.CheckBox check => check.IsChecked == true ? "1" : "0",
        System.Windows.Controls.ComboBox { SelectedItem: ComboBoxItem { Tag: int value } } => value.ToString(),
        _ => "",
    };

    private static void AddRow(Grid grid, string label, UIElement editor)
    {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 1, 0, 1) };
        text.SetResourceReference(TextBlock.ForegroundProperty, "Theme.TextMuted");
        Grid.SetRow(text, row);
        Grid.SetRow(editor, row);
        Grid.SetColumn(editor, 1);
        grid.Children.Add(text);
        grid.Children.Add(editor);
    }

    private static BotDefinition ToDefinition(BotDraft draft)
    {
        if (draft.Name.Length == 0)
        {
            throw new InvalidDataException("Every bot needs a name.");
        }

        return new BotDefinition
        {
            Name = draft.Name,
            Parts = Enumerable.Range(0, 4).Select(k => (draft.PartNames[k], (byte[])draft.Stats[k].Clone())).ToList(),
            GraphicsFrom = draft.BaseId,
            Recolor = draft.Art.Count == 0 ? draft.Recolor : null,
            Art = draft.Art.Count > 0 ? new Dictionary<string, ArtImage>(draft.Art) : null,
        };
    }

    private List<BotDefinition> AllDefinitions()
    {
        ReadEditorInto(_current);
        return _drafts.Select(ToDefinition).ToList();
    }

    // ---- pieces ----

    private void RefreshSlots()
    {
        SlotsPanel.Children.Clear();
        foreach (var group in _slots.GroupBy(slot => slot.Part))
        {
            var header = PartTitles.GetValueOrDefault(group.Key, group.Key);
            var custom = group.Count(slot => _art.ContainsKey(slot.Name));
            if (custom > 0)
            {
                var colours = BotArt.BuildPalette(group.Select(Picture).ToList()).SourceColors;
                header += $"  -  {custom} of {group.Count()} redrawn, {colours} colours" +
                          (colours > BotArt.MaxColors ? $" (will be merged to {BotArt.MaxColors})" : "");
            }

            SlotsPanel.Children.Add(new TextBlock { Text = header, FontWeight = FontWeights.SemiBold, FontSize = 14, Margin = new Thickness(0, 8, 0, 4) });
            var wrap = new WrapPanel();
            foreach (var slot in group)
            {
                wrap.Children.Add(SlotCard(slot));
            }

            SlotsPanel.Children.Add(wrap);
        }

        if (_current.Recolor is { Count: > 0 } recolor && _art.Count == 0)
        {
            var note = new TextBlock
            {
                Text = $"This bot recolours the base drawings ({recolor.Count} colour swaps); this applies when adding, unless pictures are loaded.",
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 0),
            };
            note.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Notice");
            SlotsPanel.Children.Add(note);
        }
    }

    // After any picture change: the slots and the list label (drawing count).
    private void PicturesChanged()
    {
        RefreshSlots();
        RefreshDraftList();
    }

    private ArtImage Picture(ArtSlot slot) => _art.GetValueOrDefault(slot.Name) ?? slot.Current;

    private System.Windows.Controls.Border SlotCard(ArtSlot slot)
    {
        var custom = _art.ContainsKey(slot.Name);
        var image = new Image { Source = ToBitmap(Picture(slot)), Width = slot.Width * Zoom, Height = slot.Height * Zoom, Stretch = Stretch.Fill };
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
        var load = new Button { Content = "Load PNG...", Padding = new Thickness(6, 2, 6, 2) };
        load.Click += (_, _) => LoadSlotPng(slot);
        var reset = new Button { Content = "Reset", Padding = new Thickness(6, 2, 6, 2), Margin = new Thickness(4, 0, 0, 0), IsEnabled = custom };
        reset.Click += (_, _) =>
        {
            _art.Remove(slot.Name);
            PicturesChanged();
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
        buttons.Children.Add(load);
        buttons.Children.Add(reset);
        var card = new StackPanel();
        card.Children.Add(new System.Windows.Controls.Border { Background = Checker, Child = image, HorizontalAlignment = System.Windows.HorizontalAlignment.Left });
        card.Children.Add(new TextBlock { Text = slot.Name, FontSize = 11, Margin = new Thickness(0, 4, 0, 0) });
        var state = new TextBlock { Text = $"{slot.Width}x{slot.Height}  {(custom ? "your picture" : "base picture")}", FontSize = 11 };
        state.SetResourceReference(TextBlock.ForegroundProperty, custom ? "Theme.Success" : "Theme.TextMuted");
        card.Children.Add(state);
        card.Children.Add(buttons);
        var border = new System.Windows.Controls.Border
        {
            Child = card, Padding = new Thickness(6), Margin = new Thickness(0, 0, 8, 8), CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(custom ? 2 : 1), MinWidth = 150, VerticalAlignment = VerticalAlignment.Top,
        };
        border.SetResourceReference(System.Windows.Controls.Border.BorderBrushProperty, custom ? "Theme.Success" : "Theme.Border");
        return border;
    }

    private void LoadSlotPng(ArtSlot slot)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Title = $"Picture for {slot.Name} ({slot.Width}x{slot.Height})", Filter = PngFilter };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        if (TryReadPicture(dialog.FileName, slot, out var picture, out var problem))
        {
            _art[slot.Name] = picture;
            PicturesChanged();
        }
        else
        {
            MessageBox.Show(this, problem, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OnLoadFolderClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Folder with the pieces' PNGs (names as in the templates)" };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var (loaded, problems) = LoadArt(dialog.FolderName, _slots, _art);
        PicturesChanged();
        StatusTextBlock.Text = $"{Path.GetFileName(dialog.FolderName)}: {loaded} of {_slots.Count} pieces found, {_art.Count} differ from the base.";
        if (loaded == 0)
        {
            problems.Insert(0, "No PNG in this folder matches the pieces of this body type (export the templates to see the names).");
        }

        Warn(problems);
    }

    // Reads every "<piece name>.png" of the folder; pictures equal to the base piece are not changes.
    private static (int Loaded, List<string> Problems) LoadArt(string folder, IReadOnlyList<ArtSlot> slots, Dictionary<string, ArtImage> art)
    {
        var problems = new List<string>();
        var loaded = 0;
        foreach (var slot in slots)
        {
            var path = Path.Combine(folder, slot.Name + ".png");
            if (!File.Exists(path))
            {
                continue;
            }

            if (TryReadPicture(path, slot, out var picture, out var problem))
            {
                if (!picture.Argb.SequenceEqual(slot.Current.Argb))
                {
                    art[slot.Name] = picture;
                }

                loaded++;
            }
            else
            {
                problems.Add(problem);
            }
        }

        return (loaded, problems);
    }

    private void OnResetAllClicked(object sender, RoutedEventArgs e)
    {
        _art.Clear();
        PicturesChanged();
    }

    private void OnExportTemplatesClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Folder for the templates (one PNG per piece)" };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        foreach (var slot in _slots)
        {
            SavePng(Picture(slot), Path.Combine(dialog.FolderName, slot.Name + ".png"));
        }

        StatusTextBlock.Text = $"{_slots.Count} templates written to {dialog.FolderName}. Draw over them (same size), then 'Load pictures from folder'.";
    }

    private void Warn(List<string> problems)
    {
        if (problems.Count > 0)
        {
            MessageBox.Show(this, string.Join("\n", problems), Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ---- files ----

    // A bot JSON or a bot list: its bots are appended to the list.
    private void OnImportJsonClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Title = "Medabot definition or bot list", Filter = JsonFilter };
        if (dialog.ShowDialog(this) != true || !StoreEditor())
        {
            return;
        }

        try
        {
            // The untouched starter bot is replaced instead of kept.
            var untouched = _drafts.Count == 1 && _listPath is null && Snapshot() == _savedSnapshot;
            var (drafts, problems) = ReadBots(dialog.FileName, untouched ? 0 : _drafts.Count);
            if (untouched)
            {
                _drafts.Clear();
            }

            _drafts.AddRange(drafts);
            ShowDraft(drafts[0]);
            RefreshDraftList();
            Warn(problems);
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or System.Text.Json.JsonException or KeyNotFoundException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Import", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private (List<BotDraft> Drafts, List<string> Problems) ReadBots(string path, int alreadyListed)
    {
        var definitions = BotProject.Parse(File.ReadAllText(path));
        if (definitions.Count == 0)
        {
            throw new InvalidDataException($"{Path.GetFileName(path)} has no bots.");
        }

        if (_layout.BotCount + alreadyListed + definitions.Count > BotAdder.MaxBots)
        {
            throw new InvalidDataException($"{definitions.Count} bot(s) would pass bot {BotAdder.MaxBots - 1}, the last one the expansion has room for.");
        }

        var drafts = new List<BotDraft>();
        var problems = new List<string>();
        foreach (var definition in definitions)
        {
            var draft = DraftFrom(definition);
            if (definition.ArtFolder is { } art)
            {
                var folder = FindArtFolder(Path.GetDirectoryName(Path.GetFullPath(path))!, art);
                if (folder is null)
                {
                    problems.Add($"{draft.Name}: pictures folder \"{art}\" not found; the base pictures are used.");
                }
                else
                {
                    problems.AddRange(LoadArt(folder, SlotsFor(draft.BaseId), draft.Art).Problems.Select(p => $"{draft.Name}: {p}"));
                }
            }

            drafts.Add(draft);
        }

        return (drafts, problems);
    }

    // "art" is relative to the JSON in the editor and to the project root in tools/build.py: try both.
    private static string? FindArtFolder(string jsonFolder, string art)
    {
        for (var dir = new DirectoryInfo(jsonFolder); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, art);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    // The selected bot alone, in the data/bots format (tools/build.py reads it too).
    private void OnSaveJsonClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            ReadEditorInto(_current);
            var definition = ToDefinition(_current);
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export the selected Medabot", Filter = JsonFilter, FileName = $"{SafeFileName(definition.Name)}.json",
            };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            string? artFolder = null;
            if (_art.Count > 0)
            {
                artFolder = Path.GetFileNameWithoutExtension(dialog.FileName) + "_art";
                WriteArt(_current, Path.Combine(Path.GetDirectoryName(dialog.FileName)!, artFolder));
            }

            File.WriteAllText(dialog.FileName, definition.ToJson(artFolder));
            StatusTextBlock.Text = $"Exported {Path.GetFileName(dialog.FileName)}" + (artFolder is null ? "." : $" and {_art.Count} picture(s) in {artFolder}\\.");
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, "Export bot JSON", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static void WriteArt(BotDraft draft, string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (var old in Directory.GetFiles(folder, "*.png").Where(f => !draft.Art.ContainsKey(Path.GetFileNameWithoutExtension(f))))
        {
            File.Delete(old);       // a piece that went back to the base picture
        }

        foreach (var (name, picture) in draft.Art)
        {
            SavePng(picture, Path.Combine(folder, name + ".png"));
        }
    }

    private void OnOpenListClicked(object sender, RoutedEventArgs e)
    {
        if (!StoreEditor() || !ConfirmDiscard())
        {
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog { Title = "Open a bot list", Filter = ListFilter };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var (drafts, problems) = ReadBots(dialog.FileName, 0);
            _drafts.Clear();
            _drafts.AddRange(drafts);
            _listPath = dialog.FileName;
            ShowDraft(drafts[0]);
            RefreshDraftList();
            _savedSnapshot = Snapshot();
            Warn(problems);
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or System.Text.Json.JsonException or KeyNotFoundException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Open bot list", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OnSaveListClicked(object sender, RoutedEventArgs e) => SaveList(_listPath);

    private void OnSaveListAsClicked(object sender, RoutedEventArgs e) => SaveList(null);

    private bool SaveList(string? path)
    {
        try
        {
            var definitions = AllDefinitions();
            if (path is null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save the bot list", Filter = ListFilter,
                    FileName = _listPath is null ? "my_medabots.json" : Path.GetFileName(_listPath),
                    InitialDirectory = Path.GetDirectoryName(_listPath ?? _romPath),
                };
                if (dialog.ShowDialog(this) != true)
                {
                    return false;
                }

                path = dialog.FileName;
            }

            // Pictures: <list>_art/<NN>_<name>/<piece>.png, NN = position in the list.
            var artRoot = Path.GetFileNameWithoutExtension(path) + "_art";
            var entries = new List<(BotDefinition, string?)>();
            for (var i = 0; i < _drafts.Count; i++)
            {
                string? art = null;
                if (_drafts[i].Art.Count > 0)
                {
                    art = $"{artRoot}/{i + 1:00}_{SafeFileName(_drafts[i].Name)}";
                    WriteArt(_drafts[i], Path.Combine(Path.GetDirectoryName(path)!, art));
                }

                entries.Add((definitions[i], art));
            }

            File.WriteAllText(path, BotProject.ToJson(entries));
            _listPath = path;
            _savedSnapshot = Snapshot();
            RefreshDraftList();
            StatusTextBlock.Text = $"Saved {Path.GetFileName(path)}: {_drafts.Count} bot(s).";
            return true;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, "Save bot list", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }

    // Everything the list holds, to tell whether it changed since the last save.
    private string Snapshot()
    {
        var current = _current.Clone();
        try
        {
            ReadEditorInto(current);
        }
        catch (InvalidDataException)
        {
            return "(invalid field)";
        }

        return string.Join("|", _drafts.Select(d => d == _current ? current : d).Select(d =>
            $"{d.BaseId};{d.Name};{string.Join(",", d.PartNames)};{string.Join(",", d.Stats.SelectMany(s => s))};" +
            string.Join(",", d.Art.OrderBy(a => a.Key).Select(a => $"{a.Key}:{a.Value.Argb.Aggregate(17, (h, v) => unchecked(h * 31 + (int)v))}"))));
    }

    // True when nothing is unsaved, or the user saved it or chose to discard it.
    private bool ConfirmDiscard()
    {
        if (Snapshot() == _savedSnapshot)
        {
            return true;
        }

        var answer = MessageBox.Show(this, "The bot list has unsaved changes. Save them?", Caption,
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        return answer == MessageBoxResult.No || (answer == MessageBoxResult.Yes && SaveList(_listPath));
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (OutputPath is null && !ConfirmDiscard())
        {
            e.Cancel = true;
        }
    }

    // ---- add ----

    private async void OnAddClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var definitions = AllDefinitions();
            if (Snapshot() != _savedSnapshot && MessageBox.Show(this,
                    "Save the bot list first, so it can be reopened later to add more?", Caption,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && !SaveList(_listPath))
            {
                return;
            }

            var label = definitions.Count == 1 ? SafeFileName(definitions[0].Name) : $"{definitions.Count}bots";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = $"Save the ROM with the {definitions.Count} new bot(s) as",
                Filter = "Game Boy Advance ROM (*.gba)|*.gba|All files (*.*)|*.*",
                InitialDirectory = Path.GetDirectoryName(_romPath),
                FileName = $"{Path.GetFileNameWithoutExtension(_romPath)}+{label}.gba",
            };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            AddButton.IsEnabled = false;
            StatusTextBlock.Text = $"Adding {definitions.Count} bot(s)...";
            var result = await Task.Run(() => BotAdder.AddAll(_rom, definitions));
            await File.WriteAllBytesAsync(dialog.FileName, result.Rom);
            OutputPath = dialog.FileName;
            AddedSummary = string.Join("\n", result.Log);
            DialogResult = true;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            AddButton.IsEnabled = true;
            RefreshDraftList();
            MessageBox.Show(this, ex.Message, "Add all to ROM", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ---- pictures ----

    private static bool TryReadPicture(string path, ArtSlot slot, out ArtImage picture, out string problem)
    {
        picture = null!;
        try
        {
            using var stream = File.OpenRead(path);
            var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
            if (frame.PixelWidth != slot.Width || frame.PixelHeight != slot.Height)
            {
                problem = $"{Path.GetFileName(path)}: {frame.PixelWidth}x{frame.PixelHeight}, the piece {slot.Name} is {slot.Width}x{slot.Height}.";
                return false;
            }

            var bgra = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
            var bytes = new byte[slot.Width * slot.Height * 4];
            bgra.CopyPixels(bytes, slot.Width * 4, 0);
            var argb = new uint[slot.Width * slot.Height];
            Buffer.BlockCopy(bytes, 0, argb, 0, bytes.Length);
            picture = new ArtImage(slot.Width, slot.Height, argb);
            problem = "";
            return true;
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or FileFormatException or UnauthorizedAccessException)
        {
            problem = $"{Path.GetFileName(path)}: {ex.Message}";
            return false;
        }
    }

    private static BitmapSource ToBitmap(ArtImage picture)
    {
        var bytes = new byte[picture.Argb.Length * 4];
        Buffer.BlockCopy(picture.Argb, 0, bytes, 0, bytes.Length);
        var bitmap = BitmapSource.Create(picture.Width, picture.Height, 96, 96, PixelFormats.Bgra32, null, bytes, picture.Width * 4);
        bitmap.Freeze();
        return bitmap;
    }

    private static void SavePng(ArtImage picture, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(ToBitmap(picture)));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static Brush CreateChecker()
    {
        var light = new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 8, 8)));
        var dark = new GeometryDrawing(new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)), null,
            new GeometryGroup { Children = { new RectangleGeometry(new Rect(0, 0, 4, 4)), new RectangleGeometry(new Rect(4, 4, 4, 4)) } });
        var brush = new DrawingBrush(new DrawingGroup { Children = { light, dark } })
        {
            TileMode = TileMode.Tile, Viewport = new Rect(0, 0, 8, 8), ViewportUnits = BrushMappingMode.Absolute,
        };
        brush.Freeze();
        return brush;
    }

    private static string SafeFileName(string name) =>
        string.Concat(name.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
}
