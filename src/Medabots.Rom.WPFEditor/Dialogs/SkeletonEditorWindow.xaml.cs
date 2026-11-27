using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Animation;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace Medabots.Rom.WPFEditor.Dialogs;

/// <summary>
/// Edits the battle skeleton of a decomp project: each mold's joint (where it hangs from its parent), pivot (the
/// point of its picture on that joint), depth and child / sibling links. Edits go to data/anim/molds.json
/// (tools/anim_molds.py puts them in the build). Drawing: the same port of tools/anim_render.py as the Pose Viewer.
/// </summary>
public partial class SkeletonEditorWindow : Window
{
    private const int ViewW = 240, ViewH = 160, RootX = 120, RootY = 112;
    private static readonly string[] PartNames = ["head", "right arm", "left arm", "legs"];

    private readonly string _root;
    private SkeletonRig? _rig;
    private PoseRows? _poseRows; // pose_rows.json of the built ROM (Navi molds 50+)
    private MoldEdits? _edits;
    private PoseLibrary? _lib;
    private readonly Dictionary<(bool Navi, int Mold), MoldFields> _romValues = [];
    private readonly Dictionary<(bool Navi, int Mold), int> _usage = [];
    private List<SpriteRef>[]? _lists;
    private List<(PoseState State, int Key)> _frames = [new(new PoseState(), 0)];
    private List<RenderedSprite> _sprites = [];
    private (bool Navi, int Mold)? _sel;
    private readonly WriteableBitmap _bitmap = new(ViewW, ViewH, 96, 96, PixelFormats.Bgra32, null);
    private readonly uint[] _pixels = new uint[ViewW * ViewH];
    private bool _busy;
    private Point? _dragFrom;
    private MoldFields? _dragStart;

    private readonly int? _initialMold;
    private Dictionary<string, int>? _botIds;        // bot_ids.json of the loaded build (key -> id)
    private readonly Dictionary<int, string> _botKeys = []; // id -> key

    private UIElement? _hostedContent;

    /// <summary>The window is hosted in the main window's Bots tab (never shown): dialogs are owned by the main window.</summary>
    private Window Host => _hostedContent is null ? this : (Window.GetWindow(_hostedContent) ?? this);

    public SkeletonEditorWindow(string projectRoot, int? initialMold = null)
    {
        InitializeComponent();
        _root = projectRoot;
        _initialMold = initialMold;
        Preview.Source = _bitmap;
        Loaded += (_, _) => Init();
        Closing += (_, e) =>
        {
            if (_edits?.Dirty == true &&
                MessageBox.Show(Host, "Discard the unsaved skeleton edits?", "Skeleton Editor", MessageBoxButton.YesNo,
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

    private sealed record BotItem(string Name, int Id)
    {
        public override string ToString() => Name;
    }

    private sealed record PoseItem(PoseScript? Script, string Name)
    {
        public override string ToString() => Name;
    }

    // ---------- loading ----------

    private void Init()
    {
        try
        {
            _lib = PoseLibrary.Load(_root);
        }
        catch
        {
            _lib = null; // poses are only a backdrop here
        }
        PoseBox.ItemsSource = new[] { new PoseItem(null, "Neutral (bind pose)") }
            .Concat(_lib?.Scripts.Values.Where(s => s.KeyCount > 0).Select(s => new PoseItem(s, $"Pose {s.Id:00}")) ?? [])
            .ToList();
        _busy = true;
        PoseBox.SelectedIndex = 0;
        _busy = false;
        var rom = PoseViewerWindow.NewestBuild(_root);
        if (rom is not null)
        {
            LoadRom(rom);
            if (_initialMold is { } m && _rig is not null)
            {
                var key = (IsNavi(m), m);
                if (Desc(key) is not null)
                {
                    _sel = key;
                    FillTree();
                    FillFields();
                    Draw();
                }
            }
        }
        else
            StatusText.Text = "No built ROM with a .map under build/scratch: Build ROM first, or Browse.";
    }

    private void LoadRom(string romPath)
    {
        try
        {
            _rig = new SkeletonRig(File.ReadAllBytes(romPath), File.ReadAllText(Path.ChangeExtension(romPath, ".map")));
            _poseRows = PoseRows.Load(romPath);
        }
        catch (Exception ex)
        {
            _rig = null;
            StatusText.Text = $"Could not read {romPath}: {ex.Message}";
            return;
        }
        RomPathBox.Text = romPath;
        _botIds = null;
        _botKeys.Clear();
        var idsFile = Path.Combine(Path.GetDirectoryName(romPath)!, "bot_ids.json");
        if (File.Exists(idsFile))
            try
            {
                _botIds = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(idsFile));
                foreach (var (k, v) in _botIds ?? [])
                    _botKeys.TryAdd(v, k);
            }
            catch
            {
                _botIds = null;
            }
        _romValues.Clear();
        foreach (var (m, d) in _rig.Core)
            _romValues[(false, m)] = MoldFields.From(d, MoldEdits.LayerNames.ContainsKey(m));
        foreach (var m in _rig.NaviOwn)
            _romValues[(true, m)] = MoldFields.From(_rig.NaviMolds![m], false);

        var note = "";
        try
        {
            _edits = MoldEdits.Load(_root);
            var skipped = _edits.ApplyTo(_rig);
            skipped.AddRange(_edits.ApplyCustom(_rig, _botIds));
            if (skipped.Count > 0)
                note = $" Not in this build: {string.Join(", ", skipped)}.";
            var bad = MoldEdits.CheckTree(_rig.Core) ?? (_rig.NaviMolds is { } n ? MoldEdits.CheckTree(n) : null);
            if (bad is not null)
                note += $" molds.json breaks the tree: {bad}.";
        }
        catch (Exception ex)
        {
            _edits = null;
            note = $" data/anim/molds.json: {ex.Message}";
        }

        var tinpet = PoseViewerWindow.TinpetId(romPath);
        CountUsage(tinpet);
        _busy = true;
        var names = BotLabels.DecompNames(_root);
        BotBox.ItemsSource = new List<BotItem> { new(BotLabels.Of(tinpet, "Tinpet (male)"), tinpet), new(BotLabels.Of(tinpet + 1, "Tinpet (female)"), tinpet + 1) }
            .Concat(Enumerable.Range(0, tinpet).Select(i => new BotItem(
                BotLabels.Row(i, _botKeys.GetValueOrDefault(i), names), i))).ToList();
        BotBox.SelectedIndex = 0;
        _busy = false;
        OnViewOptionChanged(this, null!);
        StatusText.Text = $"Skeleton from {Path.GetFileName(Path.GetDirectoryName(romPath))}" +
                          (_rig.NaviMolds is null ? "" : " (Navi build)") + "." + note;
    }

    // How many parts (head / arm / arm / legs of every bot) draw with each mold of each table.
    private void CountUsage(int tinpet)
    {
        _usage.Clear();
        for (var id = 0; id < tinpet + 2; id++)
        {
            List<SpriteRef>[] lists;
            try
            {
                lists = _rig!.PartLists(id, id, id, id);
            }
            catch
            {
                continue;
            }
            foreach (var l in lists)
                foreach (var r in l.DistinctBy(r => r.Node))
                {
                    var key = (r.Navi && _rig.NaviOwn.Contains(r.Node), r.Node);
                    _usage[key] = _usage.GetValueOrDefault(key) + 1;
                }
        }
    }

    // ---------- the tree ----------

    private void OnViewOptionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_busy || _rig is null || BotBox.SelectedItem is not BotItem bot)
            return;
        try
        {
            _lists = _rig.PartLists(bot.Id, bot.Id, bot.Id, bot.Id);
        }
        catch (Exception ex)
        {
            _lists = null;
            StatusText.Text = $"Parts: {ex.Message}";
        }
        FillTree();
        Draw();
    }

    private SpriteRef? Entry(int node) =>
        _lists?.SelectMany(l => l).Cast<SpriteRef?>().FirstOrDefault(r => r!.Value.Node == node);

    // Which table draws this node for the shown bot (its part entry has the Navi bit and the mold a Navi record).
    private bool IsNavi(int node) => _rig?.NaviMolds is not null && Entry(node) is { Navi: true } && _rig.NaviOwn.Contains(node);

    // Own mold of the shown bot on this node (data/anim/molds.json "custom" / "use"); 0 = the shared mold.
    private int CustomOf(int node) => Entry(node)?.Custom ?? 0;

    private MoldDesc? Desc((bool Navi, int Mold) key) =>
        CustomOf(key.Mold) is > 0 and var c && _rig?.Customs.GetValueOrDefault(c) is { } own ? own
        : key.Navi ? _rig?.NaviMolds?.GetValueOrDefault(key.Mold) : _rig?.Core.GetValueOrDefault(key.Mold);

    private int PartOf(int node)
    {
        if (_lists is not null)
            for (var p = 0; p < 4; p++)
                if (_lists[p].Any(r => r.Node == node))
                    return p;
        return -1;
    }

    private string? BotKey => BotBox.SelectedItem is BotItem b ? _botKeys.GetValueOrDefault(b.Id) : null;

    private string Label(int node)
    {
        if (MoldEdits.LayerNames.TryGetValue(node, out var layer))
            return layer;
        if (node == 1)
            return "root";
        if (node == 2)
            return "body";
        if (_lists is not null)
            for (var p = 0; p < 4; p++)
                if (_lists[p].Any(r => r.Node == node))
                    return PartNames[p];
        return "";
    }

    private bool Edited((bool Navi, int Mold) key) =>
        _edits is not null && CustomOf(key.Mold) == 0 && (key.Navi ? _edits.Navi : _edits.Core).ContainsKey(key.Mold);

    private string Header((bool Navi, int Mold) key)
    {
        var d = Desc(key);
        var size = d is null || d.Layer is not null || key.Mold <= 2 ? "" : $"  {d.Height}x{d.Width}";
        var own = CustomOf(key.Mold) is > 0 and var c ? $"  [own #{c}]" : "";
        return $"{key.Mold:00}  {Label(key.Mold)}{size}{(key.Navi ? "  [Navi]" : "")}{own}{(Edited(key) ? "  *" : "")}";
    }

    private void FillTree()
    {
        MoldTree.Items.Clear();
        if (_rig is null)
            return;
        var seen = new HashSet<int>();
        TreeViewItem? Make(int node)
        {
            if (node == 0 || !seen.Add(node))
                return null;
            var key = (IsNavi(node), node);
            var d = Desc(key);
            if (d is null)
                return null;
            var item = new TreeViewItem { Header = Header(key), Tag = key, IsExpanded = true };
            var child = d.Layer is not null && _lists is not null
                ? (_lists[d.Layer switch { "gHeadPartLayer" => 0, "gRightArmPartLayer" => 1, "gLeftArmPartLayer" => 2, _ => 3 }]
                    is { Count: > 0 } l ? l[0].Node : 0)
                : d.Child;
            for (var c = child; c != 0;)
            {
                if (Make(c) is { } sub)
                    item.Items.Add(sub);
                var cd = Desc((IsNavi(c), c));
                c = cd is null || seen.Contains(cd.Sibling) ? 0 : cd.Sibling;
            }
            if (_sel == key)
                item.IsSelected = true;
            return item;
        }
        if (Make(1) is { } top)
            MoldTree.Items.Add(top);
    }

    private void RefreshHeaders()
    {
        void Walk(ItemCollection items)
        {
            foreach (TreeViewItem it in items)
            {
                it.Header = Header(((bool, int))it.Tag);
                Walk(it.Items);
            }
        }
        Walk(MoldTree.Items);
    }

    private void OnMoldSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (MoldTree.SelectedItem is TreeViewItem { Tag: ValueTuple<bool, int> key })
            Select(key);
    }

    private void Select((bool Navi, int Mold) key)
    {
        _sel = key;
        FillFields();
        Draw();
    }

    private void FillFields()
    {
        var d = _sel is { } k ? Desc(k) : null;
        FieldGrid.IsEnabled = ResetButton.IsEnabled = d is not null;
        OwnButton.IsEnabled = SharedButton.IsEnabled = false;
        if (d is null || _sel is not { } key)
        {
            MoldTitle.Text = "No mold selected";
            MoldInfo.Text = "";
            return;
        }
        var ownId = CustomOf(key.Mold);
        OwnButton.IsEnabled = ownId == 0 && key.Mold >= 7 && PartOf(key.Mold) >= 0 && BotKey is not null && _edits is not null;
        SharedButton.IsEnabled = ownId != 0 && BotKey is not null && _edits is not null;
        var layer = MoldEdits.LayerNames.ContainsKey(key.Mold) && !key.Navi;
        MoldTitle.Text = $"Mold {key.Mold:00}  {Label(key.Mold)}";
        var table = key.Navi ? "Navi record" : "core (game) table";
        var used = _usage.GetValueOrDefault(key);
        if (ownId != 0)
        {
            var cm = _edits?.Custom.GetValueOrDefault(ownId);
            table = $"Own mold #{ownId} of this bot (copy of {cm?.Base ?? "?"})";
        }
        MoldInfo.Text = ownId != 0
            ? $"{table}, {d.Height}x{d.Width} picture. Only this bot's {Label(key.Mold)} changes; the tree and the motion stay the node's."
            : layer
            ? $"{table}. Part layer: every bot's {Label(key.Mold)[..^6]} hangs from it, so moving its joint moves that part on all bots. Its child is the part's first mold, set in game."
            : key.Mold <= 2
                ? $"{table}. No picture: the parts hang from it."
                : $"{table}, {d.Height}x{d.Width} picture. Drawn for {used} part{(used == 1 ? "" : "s")} of the bots in this ROM." +
                  (Edited(key) ? " Edited (molds.json)." : "");
        _busy = true;
        JointDownBox.Text = d.JointV.ToString();
        JointRightBox.Text = d.JointH.ToString();
        PivotDownBox.Text = d.PivotV.ToString();
        PivotRightBox.Text = d.PivotH.ToString();
        DepthBox.Text = d.Depth.ToString();
        ChildBox.Text = d.Child.ToString();
        SiblingBox.Text = d.Sibling.ToString();
        PivotDownBox.IsEnabled = PivotRightBox.IsEnabled = !layer;
        ChildBox.IsEnabled = SiblingBox.IsEnabled = !layer && ownId == 0; // an own mold keeps the node's tree
        _busy = false;
    }

    // ---------- editing ----------

    private bool SetFields((bool Navi, int Mold) key, MoldFields f, bool checkTree)
    {
        var d = Desc(key);
        if (d is null || _rig is null || _edits is null)
            return false;
        f.JointDown = Math.Clamp(f.JointDown, -128, 127);
        f.JointRight = Math.Clamp(f.JointRight, -128, 127);
        f.Depth = Math.Clamp(f.Depth, -128, 127);
        f.PivotDown = Math.Clamp(f.PivotDown, -4096, 4096);
        f.PivotRight = Math.Clamp(f.PivotRight, -4096, 4096);
        f.Child = Math.Clamp(f.Child, 0, MoldEdits.Nodes - 1);
        f.Sibling = Math.Clamp(f.Sibling, 0, MoldEdits.Nodes - 1);
        if (CustomOf(key.Mold) is > 0 and var own)
        {
            (f.Child, f.Sibling) = (d.Child, d.Sibling);
            f.ApplyTo(d);
            _edits.Custom[own].Fields = f;
            _edits.Dirty = true;
            StatusText.Text = $"Edited own mold #{own} (only this bot), unsaved.";
            return true;
        }
        if (checkTree)
        {
            var table = new Dictionary<int, MoldDesc>(key.Navi ? _rig.NaviMolds! : _rig.Core);
            var trial = d.Clone();
            f.ApplyTo(trial);
            table[key.Mold] = trial;
            if (MoldEdits.CheckTree(table) is { } why)
            {
                StatusText.Text = $"Not applied: {why}.";
                return false;
            }
        }
        f.ApplyTo(d);
        (key.Navi ? _edits.Navi : _edits.Core)[key.Mold] = f;
        _edits.Dirty = true;
        StatusText.Text = $"Edited mold {key.Mold:00} ({(key.Navi ? "Navi" : "core")}), unsaved.";
        return true;
    }

    private MoldFields Current((bool Navi, int Mold) key) =>
        MoldFields.From(Desc(key)!, MoldEdits.LayerNames.ContainsKey(key.Mold) && !key.Navi);

    private void OnFieldKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OnFieldEdited(sender, e);
    }

    private void OnFieldEdited(object sender, RoutedEventArgs e)
    {
        if (_busy || _sel is not { } key || Desc(key) is null || sender is not TextBox box)
            return;
        var f = Current(key);
        if (!int.TryParse(box.Text.Trim(), out var v))
        {
            FillFields();
            return;
        }
        var links = false;
        switch (box.Tag as string)
        {
            case "jd": f.JointDown = v; break;
            case "jr": f.JointRight = v; break;
            case "pd": f.PivotDown = v; break;
            case "pr": f.PivotRight = v; break;
            case "dp": f.Depth = v; break;
            case "ch": f.Child = v; links = true; break;
            case "sb": f.Sibling = v; links = true; break;
        }
        if (MoldFields.From(Desc(key)!, f.Layer) is var old && Same(old, f))
            return;
        if (SetFields(key, f, links) && links)
            FillTree();
        else
            RefreshHeaders();
        FillFields();
        Draw();
    }

    private static bool Same(MoldFields a, MoldFields b) =>
        a.JointDown == b.JointDown && a.JointRight == b.JointRight && a.PivotDown == b.PivotDown &&
        a.PivotRight == b.PivotRight && a.Depth == b.Depth && a.Child == b.Child && a.Sibling == b.Sibling;

    private void OnResetClicked(object sender, RoutedEventArgs e)
    {
        if (_sel is { } sk && CustomOf(sk.Mold) is > 0 and var own && _edits is not null && _rig is not null &&
            Desc(sk) is { } od && _edits.Custom.GetValueOrDefault(own) is { } cm)
        {
            var b = cm.BaseNavi ? _rig.NaviMolds?.GetValueOrDefault(cm.BaseMold) : _rig.Core.GetValueOrDefault(cm.BaseMold);
            if (b is null)
                return;
            var f = MoldFields.From(b, false);
            (f.Child, f.Sibling) = (od.Child, od.Sibling);
            f.ApplyTo(od);
            cm.Fields = f;
            _edits.Dirty = true;
            StatusText.Text = $"Own mold #{own} back to the values of {cm.Base}.";
            RefreshHeaders();
            FillFields();
            Draw();
            return;
        }
        if (_sel is not { } key || _edits is null || Desc(key) is not { } d || !_romValues.TryGetValue(key, out var rom))
            return;
        var table = new Dictionary<int, MoldDesc>(key.Navi ? _rig!.NaviMolds! : _rig!.Core);
        var trial = d.Clone();
        rom.ApplyTo(trial);
        table[key.Mold] = trial;
        if (MoldEdits.CheckTree(table) is { } why)
        {
            StatusText.Text = $"Reset would break the tree ({why}): fix the other molds first.";
            return;
        }
        rom.ApplyTo(d);
        if ((key.Navi ? _edits.Navi : _edits.Core).Remove(key.Mold))
            _edits.Dirty = true;
        StatusText.Text = $"Mold {key.Mold:00} back to the loaded ROM's value (if that ROM was built with the edit, Build ROM to see the game's own).";
        FillTree();
        FillFields();
        Draw();
    }

    // Moves the joint (or, with Shift, the picture around its pivot) by a screen step. Mirrored side: right is left.
    private void Nudge(MoldFields from, int down, int right, bool picture)
    {
        if (_sel is not { } key)
            return;
        if (SideBox.SelectedIndex == 1)
            right = -right;
        var f = Current(key);
        if (picture && !f.Layer)
            (f.PivotDown, f.PivotRight) = (from.PivotDown - down, from.PivotRight - right);
        else
            (f.JointDown, f.JointRight) = (from.JointDown + down, from.JointRight + right);
        if (!Same(f, Current(key)) && SetFields(key, f, false))
        {
            RefreshHeaders();
            FillFields();
            Draw();
        }
    }

    private void OnPreviewDown(object sender, MouseButtonEventArgs e)
    {
        Preview.Focus();
        var p = e.GetPosition(Preview);
        var hit = PieceAt((int)p.X, (int)p.Y);
        if (hit is not null)
        {
            var key = (IsNavi(hit.Node), hit.Node);
            if (_sel != key)
            {
                _sel = key;
                FillTree();
                FillFields();
                Draw();
            }
        }
        if (_sel is { } k && Desc(k) is not null)
        {
            _dragFrom = p;
            _dragStart = Current(k);
            Preview.CaptureMouse();
        }
    }

    private void OnPreviewMove(object sender, MouseEventArgs e)
    {
        if (_dragFrom is not { } from || _dragStart is null || e.LeftButton != MouseButtonState.Pressed)
            return;
        var p = e.GetPosition(Preview);
        int dx = (int)Math.Round(p.X - from.X), dy = (int)Math.Round(p.Y - from.Y);
        Nudge(_dragStart, dy, dx, Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
    }

    private void OnPreviewUp(object sender, MouseButtonEventArgs e)
    {
        _dragFrom = null;
        _dragStart = null;
        Preview.ReleaseMouseCapture();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_sel is not { } key || Desc(key) is null)
            return;
        var (down, right) = e.Key switch
        {
            Key.Up => (-1, 0), Key.Down => (1, 0), Key.Left => (0, -1), Key.Right => (0, 1), _ => (0, 0),
        };
        if (down == 0 && right == 0)
            return;
        Nudge(Current(key), down, right, Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        e.Handled = true;
    }

    // Front-most picture whose box holds the point.
    private RenderedSprite? PieceAt(int x, int y) =>
        _sprites.FirstOrDefault(s =>
        {
            int w = s.Double ? s.W * 2 : s.W, h = s.Double ? s.H * 2 : s.H;
            return x >= s.ScreenX && y >= s.ScreenY && x < s.ScreenX + w && y < s.ScreenY + h;
        });

    // ---------- pose and drawing ----------

    private void OnPoseSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_busy)
            return;
        _frames = PoseBox.SelectedItem is PoseItem { Script: { } s }
            ? SkeletonRig.PlayScript(s, new PoseState(), _poseRows)
            : [(new PoseState(), 0)];
        _busy = true;
        FrameSlider.Maximum = Math.Max(0, _frames.Count - 1);
        FrameSlider.Value = FrameSlider.Maximum; // the pose as reached
        _busy = false;
        Draw();
    }

    private void OnFrameChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_busy)
            Draw();
    }

    private void Draw()
    {
        Array.Fill(_pixels, 0xFF303040u);
        var frame = Math.Min((int)FrameSlider.Value, _frames.Count - 1);
        FrameText.Text = _frames.Count > 1 ? $"frame {frame + 1}/{_frames.Count}" : "";
        _sprites = [];
        if (_rig is not null && _lists is not null)
        {
            try
            {
                var (sprites, pals) = _rig.Render(_lists, _frames[frame].State, RootX, RootY, side: SideBox.SelectedIndex == 1 ? 1 : 0);
                _sprites = sprites;
                SkeletonRig.Rasterize(sprites, pals, _pixels, ViewW, ViewH);
                if (_sel is { } key)
                    foreach (var s in sprites.Where(s => s.Node == key.Mold))
                    {
                        Box(s);
                        Cross(s.JointX, s.JointY, 0xFFFFE040);
                    }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Render: {ex.Message}";
            }
        }
        _bitmap.WritePixels(new Int32Rect(0, 0, ViewW, ViewH), _pixels, ViewW * 4, 0);
    }

    private void Box(RenderedSprite s)
    {
        int w = s.Double ? s.W * 2 : s.W, h = s.Double ? s.H * 2 : s.H;
        for (var i = 0; i < w; i += 2)
        {
            Put(s.ScreenX + i, s.ScreenY, 0xFFE0E0E0);
            Put(s.ScreenX + i, s.ScreenY + h - 1, 0xFFE0E0E0);
        }
        for (var i = 0; i < h; i += 2)
        {
            Put(s.ScreenX, s.ScreenY + i, 0xFFE0E0E0);
            Put(s.ScreenX + w - 1, s.ScreenY + i, 0xFFE0E0E0);
        }
    }

    private void Cross(int x, int y, uint color)
    {
        for (var d = -3; d <= 3; d++)
        {
            Put(x + d, y, color);
            Put(x, y + d, color);
        }
    }

    private void Put(int x, int y, uint color)
    {
        if (x >= 0 && y >= 0 && x < ViewW && y < ViewH)
            _pixels[y * ViewW + x] = color;
    }

    // ---------- own molds (EXP_CUSTOM_MOLDS) ----------

    // A private copy of the selected mold for the shown bot: from here on, edits move only this bot.
    private void OnOwnClicked(object sender, RoutedEventArgs e)
    {
        if (_sel is not { } key || _edits is null || _rig is null || BotKey is not { } bot || PartOf(key.Mold) is var part && part < 0)
            return;
        int c;
        try
        {
            c = _edits.NextCustom();
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            return;
        }
        var f = MoldFields.From(Desc(key)!, false);
        _edits.Custom[c] = new CustomMold { Base = $"{(key.Navi ? "navi" : "core")}:{key.Mold}", Fields = f };
        if (!_edits.Use.TryGetValue(bot, out var parts))
            _edits.Use[bot] = parts = new SortedDictionary<string, SortedDictionary<int, int>>(StringComparer.Ordinal);
        var pk = MoldEdits.PartKeys[part];
        if (!parts.TryGetValue(pk, out var nodes))
            parts[pk] = nodes = [];
        nodes[key.Mold] = c;
        _edits.Dirty = true;
        ReloadCustom();
        StatusText.Text = $"Own mold #{c} for {bot} ({pk}, node {key.Mold}), a copy of {_edits.Custom[c].Base}. Unsaved.";
    }

    // Back to the shared mold: the bot stops using its own copy (the copy goes when no bot uses it).
    private void OnSharedClicked(object sender, RoutedEventArgs e)
    {
        if (_sel is not { } key || _edits is null || BotKey is not { } bot)
            return;
        var c = CustomOf(key.Mold);
        if (c <= 0)
            return;
        if (_edits.Use.TryGetValue(bot, out var parts))
        {
            foreach (var (pk, nodes) in parts.ToList())
            {
                nodes.Remove(key.Mold);
                if (nodes.Count == 0)
                    parts.Remove(pk);
            }
            if (parts.Count == 0)
                _edits.Use.Remove(bot);
        }
        if (!_edits.Use.Values.Any(p => p.Values.Any(n => n.ContainsValue(c))))
            _edits.Custom.Remove(c);
        _edits.Dirty = true;
        ReloadCustom();
        StatusText.Text = $"{bot} back to the shared mold {key.Mold:00}. Unsaved.";
    }

    private void ReloadCustom()
    {
        if (_rig is null || _edits is null)
            return;
        _rig.Customs.Clear();
        _edits.ApplyCustom(_rig, _botIds);
        var keep = _sel;
        OnViewOptionChanged(this, null!);
        if (keep is { } k)
            _sel = (IsNavi(k.Mold), k.Mold);
        FillTree();
        FillFields();
        Draw();
    }

    // ---------- buttons ----------

    private void OnSaveClicked(object sender, RoutedEventArgs e) => SaveChanges();

    /// <summary>Hosted: the main window asks about these edits on close (its Closing never reaches the hosted window).</summary>
    public bool HasUnsavedChanges => _edits?.Dirty == true;

    /// <summary>Saves the skeleton edits (the Save button's work). False if they were not saved.</summary>
    public bool SaveAllChanges() => SaveChanges();

    private bool SaveChanges()
    {
        if (_edits is null || _rig is null)
            return true;
        var bad = MoldEdits.CheckTree(_rig.Core) ?? (_rig.NaviMolds is { } n ? MoldEdits.CheckTree(n) : null);
        if (bad is not null)
        {
            StatusText.Text = $"Not saved: {bad}.";
            return false;
        }
        try
        {
            _edits.Save();
            StatusText.Text = $"Saved data/anim/molds.json ({_edits.Core.Count} core, {_edits.Navi.Count} Navi, {_edits.Custom.Count} own). Build ROM in Decomp Project to see it in game.";
            return true;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Save: {ex.Message}";
            return false;
        }
    }

    private void OnBrowseRomClicked(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "GBA ROM (*.gba)|*.gba", Title = "Built ROM (needs its .map)" };
        if (dlg.ShowDialog(Host) != true)
            return;
        if (!File.Exists(Path.ChangeExtension(dlg.FileName, ".map")))
        {
            MessageBox.Show(Host, "That ROM has no .map next to it (the decomp build writes medabots_rokusho.map).",
                "Skeleton Editor", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_edits?.Dirty == true &&
            MessageBox.Show(Host, "Loading another ROM drops the unsaved edits. Go on?", "Skeleton Editor",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        _sel = null;
        LoadRom(dlg.FileName);
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
