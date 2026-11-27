using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Medabots.Rom.Animation;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Medabots.Rom.WPFEditor.Dialogs;

/// <summary>
/// Plays and edits the battle pose scripts of a decomp project (data/anim/poses/NN.json) on a Medabot skeleton
/// read from a built ROM + its .map (default: the male Tinpet, i.e. the bare frame with every mold). The drawing
/// is a port of tools/anim_render.py, checked against the game in mGBA.
/// </summary>
public partial class PoseViewerWindow : Window
{
    private const int ViewW = 240, ViewH = 160, RootX = 120, RootY = 112;

    private readonly string _root;
    private SkeletonRig? _rig;
    private readonly Workspace? _workspace;
    private PoseRows? _poseRows; // pose_rows.json of the built ROM (Navi molds 50+)
    private PoseLibrary? _lib;
    private PoseScript? _script;
    private List<(PoseState State, int Key)> _frames = [];
    private List<SpriteRef>[]? _lists;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1 / 60.0) };
    private readonly WriteableBitmap _bitmap = new(ViewW, ViewH, 96, 96, PixelFormats.Bgra32, null);
    private readonly uint[] _pixels = new uint[ViewW * ViewH];
    private bool _busy;

    private UIElement? _hostedContent;

    /// <summary>The window is hosted in the main window's Bots tab (never shown): dialogs are owned by the main window.</summary>
    private Window Host => _hostedContent is null ? this : (Window.GetWindow(_hostedContent) ?? this);

    public PoseViewerWindow(string projectRoot, Workspace? workspace = null)
    {
        InitializeComponent();
        _root = projectRoot;
        _workspace = workspace;
        Preview.Source = _bitmap;
        _timer.Tick += OnTick;
        Loaded += (_, _) => Init();
        Closing += (_, e) =>
        {
            _timer.Stop();
            if (_lib?.Scripts.Values.Any(s => s.Dirty) == true &&
                MessageBox.Show(Host, "Discard the unsaved pose edits?", "Pose Viewer", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                e.Cancel = true;
        };
    }

    /// <summary>Hosted in the Bots tab: the window is never shown, so Loaded never fires; this does the same work.</summary>
    public void InitHosted(UIElement hostedContent)
    {
        _hostedContent = hostedContent;
        CloseWindowButton.Visibility = Visibility.Collapsed;
        Init();
    }

    private sealed record ScriptItem(PoseScript Script, string Display);

    private sealed record BotItem(string Name, int Head, int RightArm, int LeftArm, int Legs)
    {
        public override string ToString() => Name;
    }

    public sealed class NodeRow
    {
        public int Node { get; init; }
        public string Part { get; init; } = "";
        public double Degrees { get; set; }
        public int Down { get; set; }
        public int Right { get; set; }
    }

    // ---------- loading ----------

    private void Init()
    {
        try
        {
            _lib = PoseLibrary.Load(_root);
        }
        catch (Exception ex)
        {
            StatusText.Text = $"data/anim/poses: {ex.Message} (python tools/anim_poses.py --export)";
            return;
        }
        ScriptList.ItemsSource = _lib.Scripts.Values.Select(s => new ScriptItem(s, Describe(s))).ToList();
        var rom = NewestBuild(_root);
        if (rom is not null)
            LoadRom(rom);
        else
            StatusText.Text = "No built ROM with a .map under build/scratch: Build ROM first, or Browse.";
        // first script that moves (some are empty)
        ScriptList.SelectedItem = ScriptList.Items.Cast<ScriptItem>().FirstOrDefault(i => i.Script.KeyCount > 0);
    }

    private static string Describe(PoseScript s)
    {
        var frames = s.Ends.Sum();
        var shared = s.AnglesFrom is { } a ? $" (angles of {a:00})" : "";
        return $"{s.Id:00}  {s.KeyCount} keys, {frames} frames{shared}";
    }

    // Newest build/scratch/*/medabots_rokusho.gba that has its .map next to it, preferring builds with the bot list
    // (bot_ids.json, written by tools/decomp_expand.py) over the plain game tree (decomp_rok) rebuilt for a compare.
    internal static string? NewestBuild(string root) => Workspace.FindBuild(root)?.RomPath;

    private void LoadRom(string romPath)
    {
        try
        {
            var map = Path.ChangeExtension(romPath, ".map");
            _rig = _workspace?.RigFor(romPath) ?? new SkeletonRig(File.ReadAllBytes(romPath), File.ReadAllText(map));
            _poseRows = PoseRows.Load(romPath);
            RomPathBox.Text = romPath;
            var tinpet = TinpetId(romPath);
            var keysById = BotLabels.KeysById(romPath);
            var names = BotLabels.DecompNames(_root);
            BotBox.ItemsSource = new List<BotItem>
            {
                new(BotLabels.Of(tinpet, "Tinpet (male)"), tinpet, tinpet, tinpet, tinpet),
                new(BotLabels.Of(tinpet + 1, "Tinpet (female)"), tinpet + 1, tinpet + 1, tinpet + 1, tinpet + 1),
            }.Concat(Enumerable.Range(0, tinpet).Select(i => new BotItem(BotLabels.Row(i, keysById.GetValueOrDefault(i), names), i, i, i, i))).ToList();
            BotBox.SelectedIndex = 0;
            StatusText.Text = $"Skeleton from {Path.GetFileName(Path.GetDirectoryName(romPath))}";
        }
        catch (Exception ex)
        {
            _rig = null;
            StatusText.Text = $"Could not read {romPath}: {ex.Message}";
        }
    }

    // The Tinpet rows follow the bots (NUM_MEDABOTS = N: male N, female N + 1); the build writes N in the layout.
    internal static int TinpetId(string romPath)
    {
        foreach (var f in new[] { Path.ChangeExtension(romPath, ".layout.json"), romPath + ".layout.json" })
            if (File.Exists(f) && JsonNode.Parse(File.ReadAllText(f)) is JsonObject o &&
                o["tinpet_id"] is { } t)
                return t.GetValue<int>();
        return 120;
    }

    // ---------- playback ----------

    private void Rebuild(bool keepFrame = false)
    {
        if (_script is null)
            return;
        var frame = keepFrame ? (int)FrameSlider.Value : 0;
        _frames = SkeletonRig.PlayScript(_script, new PoseState(), _poseRows);
        _busy = true;
        FrameSlider.Maximum = Math.Max(0, _frames.Count - 1);
        FrameSlider.Value = Math.Min(frame, (int)FrameSlider.Maximum);
        _busy = false;
        Draw();
    }

    private void Draw()
    {
        Array.Fill(_pixels, 0xFF303040u);
        var frame = (int)FrameSlider.Value;
        var key = _frames.Count > 0 ? _frames[Math.Min(frame, _frames.Count - 1)].Key : 0;
        FrameText.Text = _frames.Count > 0 ? $"frame {frame + 1}/{_frames.Count}  key {key + 1}" : "";
        if (_rig is not null && _lists is not null && _frames.Count > 0)
        {
            var state = _frames[Math.Min(frame, _frames.Count - 1)].State;
            var side = SideBox.SelectedIndex == 1 ? 1 : 0;
            try
            {
                var (sprites, pals) = _rig.Render(_lists, state, RootX, RootY, side: side);
                SkeletonRig.Rasterize(sprites, pals, _pixels, ViewW, ViewH);
                if (NodeGrid.SelectedItem is NodeRow row && sprites.FirstOrDefault(s => s.Node == row.Node) is { } sel)
                    Cross(sel.JointX, sel.JointY);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Render: {ex.Message}";
            }
        }
        _bitmap.WritePixels(new Int32Rect(0, 0, ViewW, ViewH), _pixels, ViewW * 4, 0);
    }

    private void Cross(int x, int y)
    {
        for (var d = -3; d <= 3; d++)
        {
            Put(x + d, y);
            Put(x, y + d);
        }
    }

    private void Put(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < ViewW && y < ViewH)
            _pixels[y * ViewW + x] = 0xFFFFE040;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_frames.Count == 0)
            return;
        var next = (int)FrameSlider.Value + 1;
        if (next >= _frames.Count)
        {
            if (LoopBox.IsChecked != true)
            {
                Stop();
                return;
            }
            next = 0;
        }
        FrameSlider.Value = next;
    }

    private void Stop()
    {
        _timer.Stop();
        PlayButton.Content = "Play";
    }

    private void OnPlayClicked(object sender, RoutedEventArgs e)
    {
        if (_timer.IsEnabled)
            Stop();
        else
        {
            _timer.Start();
            PlayButton.Content = "Pause";
        }
    }

    private void OnFrameChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_busy)
            Draw();
    }

    private void OnViewOptionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_rig is null || BotBox.SelectedItem is not BotItem bot)
            return;
        try
        {
            _lists = _rig.PartLists(bot.Head, bot.RightArm, bot.LeftArm, bot.Legs);
        }
        catch (Exception ex)
        {
            _lists = null;
            StatusText.Text = $"Parts: {ex.Message}";
        }
        FillNodes();
        Draw();
    }

    // ---------- editing ----------

    private void OnScriptSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_busy || ScriptList.SelectedItem is not ScriptItem item)
            return;
        _script = item.Script;
        Rebuild(); // frames first: picking the keyframe below jumps to its last frame
        KeyBox.ItemsSource = Enumerable.Range(0, _script.KeyCount)
            .Select(k => $"{k + 1}  ({_script.Ends[k]} frames)").ToList();
        KeyBox.SelectedIndex = _script.KeyCount > 0 ? 0 : -1;
        var owner = _script.AngleOwner ?? _script;
        var users = _lib!.SharingAngles(owner).Select(s => s.Id).ToList();
        ShareText.Text = users.Count > 1
            ? $"Angles shared by scripts {string.Join(", ", users.Select(u => u.ToString("00")))}: an edit changes all of them."
            : "";
    }

    private void OnKeySelected(object sender, SelectionChangedEventArgs e)
    {
        FillNodes();
        if (_script is null || KeyBox.SelectedIndex < 0)
            return;
        // jump to the last frame of this keyframe (where the pose reaches the values in the table)
        var key = KeyBox.SelectedIndex;
        var last = _frames.FindLastIndex(f => f.Key == key);
        if (last >= 0)
            FrameSlider.Value = last;
        KeyInfo.Text = $"reached at frame {last + 1}";
        EndBox.Text = _script.Ends[key].ToString();
    }

    private void FillNodes()
    {
        if (_script is null || _rig is null || KeyBox.SelectedIndex < 0)
        {
            NodeGrid.ItemsSource = null;
            return;
        }
        var key = KeyBox.SelectedIndex;
        var owner = new Dictionary<int, string>();
        string[] parts = ["head", "right arm", "left arm", "legs"];
        if (_lists is not null)
            for (var p = 0; p < 4; p++)
                foreach (var r in _lists[p])
                    owner.TryAdd(r.Node, parts[p]);
        foreach (var (n, d) in _rig.Core)
            if (d.Layer is not null)
                owner.TryAdd(n, d.Layer switch
                {
                    "gHeadPartLayer" => "head layer",
                    "gRightArmPartLayer" => "right arm layer",
                    "gLeftArmPartLayer" => "left arm layer",
                    _ => "legs layer",
                });
        var nodes = _rig.Core.Keys.Union(_script.Angles.Keys).Where(n => n > 0).Order().ToList();
        NodeGrid.ItemsSource = nodes.Select(n =>
        {
            var (v, h) = _script.Offset(n, key);
            return new NodeRow
            {
                Node = n, Part = owner.GetValueOrDefault(n, n == 1 ? "root" : ""),
                Degrees = Math.Round(_script.Angle(n, key) * 360.0 / 65536, 1), Down = v, Right = h,
            };
        }).ToList();
    }

    private void OnNodeSelected(object sender, SelectionChangedEventArgs e) => Draw();

    private void OnNodeCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit || _script is null || e.Row.Item is not NodeRow row ||
            e.EditingElement is not TextBox box)
            return;
        var key = KeyBox.SelectedIndex;
        var header = e.Column.Header?.ToString() ?? "";
        if (!double.TryParse(box.Text.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            return;
        PoseScript changed;
        if (header.StartsWith("Angle"))
        {
            var raw = (int)Math.Round(value * 65536 / 360.0);
            raw = ((raw + 0x8000) & 0xFFFF) - 0x8000;
            changed = _script.SetAngle(row.Node, key, raw);
        }
        else
        {
            var (v, h) = _script.Offset(row.Node, key);
            var n = Math.Clamp((int)Math.Round(value), -128, 127);
            changed = header == "Down" ? _script.SetOffset(row.Node, key, n, h) : _script.SetOffset(row.Node, key, v, n);
        }
        StatusText.Text = $"Edited script {changed.Id:00} (unsaved)";
        Dispatcher.BeginInvoke(() =>
        {
            Rebuild(keepFrame: true);
            var last = _frames.FindLastIndex(f => f.Key == key);
            if (last >= 0)
                FrameSlider.Value = last;
        }, DispatcherPriority.Background);
    }

    private void OnEndKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            OnEndEdited(sender, e);
    }

    private void OnEndEdited(object sender, RoutedEventArgs e)
    {
        if (_script is null || KeyBox.SelectedIndex < 0 || !int.TryParse(EndBox.Text.Trim(), out var frames))
            return;
        var key = KeyBox.SelectedIndex;
        if (frames == _script.Ends[key])
            return;
        _script.SetEnd(key, frames);
        EndBox.Text = _script.Ends[key].ToString();
        RefreshScriptList();
        StatusText.Text = $"Edited script {_script.Id:00} (unsaved)";
        Rebuild(keepFrame: true);
        var last = _frames.FindLastIndex(f => f.Key == key);
        if (last >= 0)
            FrameSlider.Value = last;
        KeyInfo.Text = $"reached at frame {last + 1}";
    }

    private void RefreshScriptList()
    {
        var selected = _script;
        var keyIndex = KeyBox.SelectedIndex;
        var items = _lib!.Scripts.Values.Select(s => new ScriptItem(s, Describe(s))).ToList();
        _busy = true;
        ScriptList.ItemsSource = items;
        ScriptList.SelectedItem = items.FirstOrDefault(i => i.Script == selected);
        _busy = false;
        if (keyIndex >= 0 && keyIndex < KeyBox.Items.Count)
            KeyBox.SelectedIndex = keyIndex;
    }

    private void OnNewPoseClicked(object sender, RoutedEventArgs e)
    {
        if (_lib is null || _script is null)
            return;
        var created = PoseScript.CreateCopy(_lib, _script);
        var items = _lib.Scripts.Values.Select(s => new ScriptItem(s, Describe(s))).ToList();
        ScriptList.ItemsSource = items;
        ScriptList.SelectedItem = items.First(i => i.Script == created);
        ScriptList.ScrollIntoView(ScriptList.SelectedItem);
        StatusText.Text = $"Created {created.Id:00}.json (copy of {_script.Id:00}). Use it from data/anim/pose_anims.json.";
    }

    private void OnSaveClicked(object sender, RoutedEventArgs e) => SaveChanges();

    /// <summary>Hosted: the main window asks about these edits on close (its Closing never reaches the hosted window).</summary>
    public bool HasUnsavedChanges => _lib?.Scripts.Values.Any(s => s.Dirty) == true;

    /// <summary>Saves the changed pose scripts (the Save button's work). False if saving failed.</summary>
    public bool SaveAllChanges()
    {
        try
        {
            return SaveChanges();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Save: " + ex.Message;
            return false;
        }
    }

    /// <summary>Stops the Play timer (a hosted window that is dropped keeps no animation running).</summary>
    public void StopPlayback() => _timer.Stop();

    private bool SaveChanges()
    {
        if (_lib is null)
            return true;
        var dirty = _lib.Scripts.Values.Where(s => s.Dirty).ToList();
        foreach (var s in dirty)
            s.Save();
        StatusText.Text = dirty.Count == 0
            ? "Nothing to save."
            : $"Saved {string.Join(", ", dirty.Select(s => $"{s.Id:00}.json"))}. Build ROM in Decomp Project to see it in game.";
        return true;
    }

    private void OnBrowseRomClicked(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "GBA ROM (*.gba)|*.gba", Title = "Built ROM (needs its .map)" };
        if (dlg.ShowDialog(Host) != true)
            return;
        if (!File.Exists(Path.ChangeExtension(dlg.FileName, ".map")))
        {
            MessageBox.Show(Host, "That ROM has no .map next to it (the decomp build writes medabots_rokusho.map).",
                "Pose Viewer", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        LoadRom(dlg.FileName);
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
