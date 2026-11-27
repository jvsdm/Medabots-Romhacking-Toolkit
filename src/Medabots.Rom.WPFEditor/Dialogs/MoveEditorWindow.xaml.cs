using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Medabots.Rom.Animation;
using Medabots.Rom.Metadata;
using Medabots.Rom.WPFEditor.Decomp;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Medabots.Rom.WPFEditor.Dialogs;

/// <summary>
/// Edits the moves of a decomp project: the battle sequence scripts (data/anim/seq), the animation lines that pick a
/// pose per leg type (data/anim/pose_anims.json) and which part plays which move ("anim" / "emit" of a part, or
/// data/anim/base_bots.json for the 120 bots of the base game). Works with or without Navi bots in the project.
/// The pose of the selected command plays on the Tinpet skeleton of the latest build.
/// </summary>
public partial class MoveEditorWindow : Window
{
    private const int ViewW = 240, ViewH = 160;

    private readonly string _root;
    private MoveLibrary _moves = null!;
    private PoseAnimTable _anims = null!;
    private PoseLibrary _poses = null!;
    private BaseBotMoves _baseMoves = null!;
    private DecompBotStore _store = null!;
    private SkeletonRig? _rig;
    private readonly Workspace? _workspace;
    private PoseRows? _poseRows; // pose_rows.json of the built ROM (Navi molds 50+)
    private List<SpriteRef>[]? _lists;
    private MoveScript? _move;
    private readonly ObservableCollection<OpRow> _rows = [];
    private readonly Dictionary<string, string[]> _operandNames = [];
    private bool _loading;
    private bool _baseDirty;
    private List<(PoseState State, int Key)> _frames = [];
    private int _frame;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1 / 60.0) };
    private readonly WriteableBitmap _bitmap = new(ViewW, ViewH, 96, 96, PixelFormats.Bgra32, null);
    private readonly uint[] _pixels = new uint[ViewW * ViewH];

    private readonly int? _initialMove;

    private UIElement? _hostedContent;

    /// <summary>The window is hosted in the main window's Bots tab (never shown): dialogs are owned by the main window.</summary>
    private Window Host => _hostedContent is null ? this : (Window.GetWindow(_hostedContent) ?? this);

    // Pronto para desenhar: a janela aberta ou, hospedada numa aba da janela principal (MainWindow.BotsHost.cs), o
    // conteudo carregado. A janela hospedada nunca abre, entao IsLoaded sozinho ficava falso e o Play nao tinha quadros.
    private bool Ready => IsLoaded || (_hostedContent as FrameworkElement)?.IsLoaded == true;

    /// <summary>Set by the main window: opens the Moves / Skeleton / Poses subtab instead of a separate window.</summary>
    public Action<string>? OpenSubtab { get; set; }

    public MoveEditorWindow(string projectRoot, int? initialMove = null, Workspace? workspace = null)
    {
        InitializeComponent();
        _root = projectRoot;
        _workspace = workspace;
        _initialMove = initialMove;
        Preview.Source = _bitmap;
        OpGrid.ItemsSource = _rows;
        CommandColumn.ItemsSource = MoveScript.Commands.Select(c => c.Name).Append("label").ToList();
        PoseColBox.ItemsSource = Enumerable.Range(0, PoseAnimTable.Columns).ToList();
        PoseColBox.SelectedIndex = 2; // the column most bots use (gLegPartPoseSets: 62 of the 122 legs)
        _timer.Tick += OnTick;
        Loaded += (_, _) => Init();
        Closing += OnClosing;
    }

    /// <summary>Hosted in the Bots tab: the window is never shown, so Loaded never fires; this does the same work.</summary>
    public void InitHosted(UIElement hostedContent)
    {
        _hostedContent = hostedContent;
        CloseWindowButton.Visibility = Visibility.Collapsed;
        Init();
    }

    /// <summary>One command in the grid.</summary>
    public sealed class OpRow(MoveOp op) : INotifyPropertyChanged
    {
        public MoveOp Op { get; } = op;
        public int Index { get; set; }

        public string Name
        {
            get => Op.Name;
            set => Op.Name = value;
        }

        public string ArgsText
        {
            get => Op.ArgsText;
            set => Op.ArgsText = value;
        }

        /// <summary>Operand names per command (data/anim/seq_operands.json), set by the window.</summary>
        public IReadOnlyDictionary<string, string[]> OperandNames { get; set; } = new Dictionary<string, string[]>();

        /// <summary>"name=value" for each operand, e.g. "move delay=54, x=124". Empty when the command has no names.</summary>
        public string Meaning
        {
            get
            {
                if (!OperandNames.TryGetValue(Name, out var names))
                    return "";
                return string.Join(", ", Op.Args.Select((v, i) => $"{(i < names.Length ? names[i] : "?")}={v}"));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    private int _tinpet = 120;
    private string _buildName = "";
    private readonly Dictionary<string, int> _botIds = [];

    private sealed record Target(string Display, int? BaseId, DecompBot? Bot)
    {
        public override string ToString() => Display;
    }

    // ---------- loading ----------

    private void Init()
    {
        try
        {
            _moves = MoveLibrary.Load(_root);
            _anims = PoseAnimTable.Load(_root);
            _poses = PoseLibrary.Load(_root);
            _baseMoves = BaseBotMoves.Load(_root);
            _store = new DecompBotStore(_root);
            _store.Load();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"data/anim: {ex.Message} (python tools/anim_seq.py --export; python tools/anim_poses.py --export)";
            return;
        }
        LoadOperandNames();
        var rom = PoseViewerWindow.NewestBuild(_root);
        if (rom is not null)
        {
            try
            {
                _rig = _workspace?.RigFor(rom) ?? new SkeletonRig(File.ReadAllBytes(rom), File.ReadAllText(Path.ChangeExtension(rom, ".map")));
                _poseRows = PoseRows.Load(rom);
                _tinpet = PoseViewerWindow.TinpetId(rom);
                _lists = _rig.PartLists(_tinpet, _tinpet, _tinpet, _tinpet);
                // ids of the list's bots in this build (tools/decomp_expand.py writes bot_ids.json next to the ROM)
                var idsFile = Path.Combine(Path.GetDirectoryName(rom)!, "bot_ids.json");
                if (File.Exists(idsFile) && System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(idsFile)) is System.Text.Json.Nodes.JsonObject ids)
                    foreach (var (k, v) in ids)
                        _botIds[k] = v!.GetValue<int>();
                _buildName = Path.GetFileName(Path.GetDirectoryName(rom)) ?? "";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Skeleton: {ex.Message}";
            }
        }
        else
            StatusText.Text = "No built ROM with a .map under build/scratch: the pose preview needs one (Build ROM first).";

        var meta = MedabotsMetadata.Default;
        // base game ids get the "core" pack only when this build has a bot list (bot_ids.json)
        var basePack = _botIds.Count > 0 ? "core" : null;
        var targets = Enumerable.Range(0, 120).Select(i => new Target(BotLabels.Of(i, meta.GetBotName(i), basePack), i, null)).ToList();
        // _botIds is filled above (bot_ids.json of the loaded build), so the ids are known here
        var listBots = _store.Bots.Where(b => b.InList)
            .Select(b => (Bot: b, Id: _botIds.TryGetValue(b.Key, out var id) ? (int?)id : null))
            .OrderBy(x => x.Id.HasValue ? 0 : 1).ThenBy(x => x.Id ?? 0).ThenBy(x => x.Bot.Name, StringComparer.Ordinal)
            .Select(x => new Target(BotLabels.Of(x.Id, x.Bot.Name, x.Bot.Pack), null, x.Bot));
        targets.AddRange(listBots);
        BotBox.ItemsSource = targets;
        BotBox.SelectedIndex = 0;
        RefreshMoveList(null);
    }

    private void RefreshMoveList(MoveScript? select)
    {
        _loading = true;
        var items = _moves.Scripts.Values.ToList();
        MoveList.ItemsSource = null;
        MoveList.DisplayMemberPath = "Display";
        MoveList.ItemsSource = items;
        MoveList.SelectedItem = select ?? _move ?? items.FirstOrDefault(i => i.Id == (_initialMove ?? 2)) ?? items.FirstOrDefault();
        _loading = false;
        OnMoveSelected(MoveList, null!);
    }

    // ---------- move scripts ----------

    private void OnMoveSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || MoveList.SelectedItem is not MoveScript m)
            return;
        _move = m;
        _loading = true;
        MoveNameBox.Text = m.Name;
        var shared = m.SameAs is not null;
        DetachButton.Visibility = shared ? Visibility.Visible : Visibility.Collapsed;
        SharedText.Text = shared ? $"points at {m.SameAs:000} in the game" : "";
        OpGrid.IsReadOnly = shared;
        _loading = false;
        LoadRows();
    }

    // data/anim/seq_operands.json (tools/anim_seq.py, from battle_sequence.c): the name of each operand of each command.
    // "_doc" and other non-array keys are skipped. A missing file just leaves the Meaning column empty.
    private void LoadOperandNames()
    {
        _operandNames.Clear();
        var file = Path.Combine(_root, "data", "anim", "seq_operands.json");
        if (!File.Exists(file))
            return;
        try
        {
            if (System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(file)) is System.Text.Json.Nodes.JsonObject names)
                foreach (var (cmd, list) in names)
                    if (list is System.Text.Json.Nodes.JsonArray arr)
                        _operandNames[cmd] = arr.Select(n => n!.GetValue<string>()).ToArray();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"seq_operands.json: {ex.Message}";
        }
    }

    private OpRow NewRow(MoveOp op) => new(op) { OperandNames = _operandNames };

    private void LoadRows()
    {
        if (_move is null)
            return;
        _rows.Clear();
        foreach (var op in _moves.Resolve(_move).Ops)
            _rows.Add(NewRow(op));
        Reindex();
        ShowProblems();
        var first = _rows.FirstOrDefault(r => IsPoseOp(r.Op));
        OpGrid.SelectedItem = first;
        if (first is null)
            UpdatePoseFromSelection();
    }

    private void Reindex()
    {
        for (var i = 0; i < _rows.Count; i++)
        {
            _rows[i].Index = i + 1;
            _rows[i].Refresh();
        }
    }

    private void MarkDirty()
    {
        if (_move is null)
            return;
        _moves.Resolve(_move).Dirty = true;
        StatusText.Text = $"Edited {_moves.Resolve(_move).Id:000} (unsaved)";
        ShowProblems();
    }

    private void ShowProblems()
    {
        var p = _move is null ? [] : _moves.Resolve(_move).Validate().ToList();
        ProblemsText.Text = string.Join("\n", p);
    }

    private void OnMoveNameEdited(object sender, RoutedEventArgs e)
    {
        if (_loading || _move is null || MoveNameBox.Text == _move.Name)
            return;
        _move.Name = MoveNameBox.Text.Trim();
        _move.Dirty = true;
        StatusText.Text = $"Edited {_move.Id:000} (unsaved)";
        RefreshMoveList(_move);
    }

    private void OnDetachClicked(object sender, RoutedEventArgs e)
    {
        if (_move is null)
            return;
        _move.Detach(_moves);
        RefreshMoveList(_move);
    }

    private void OnNewMoveClicked(object sender, RoutedEventArgs e)
    {
        if (_move is null)
            return;
        try
        {
            var created = _moves.CreateCopy(_move, $"{_moves.Resolve(_move).Name}_2".Replace(' ', '_'));
            StatusText.Text = $"Created {created.Id:000}.json. Point a part at it under 'Who plays this move'.";
            RefreshMoveList(created);
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }

    private void OnAddOpClicked(object sender, RoutedEventArgs e)
    {
        if (_move is null || _move.SameAs is not null)
            return;
        var ops = _move.Ops;
        var at = OpGrid.SelectedIndex >= 0 ? OpGrid.SelectedIndex + 1 : Math.Max(0, ops.Count - 1);
        var op = new MoveOp { Name = "wait_frames", Args = [1] };
        ops.Insert(at, op);
        _rows.Insert(at, NewRow(op));
        Reindex();
        OpGrid.SelectedIndex = at;
        MarkDirty();
    }

    private void OnDeleteOpClicked(object sender, RoutedEventArgs e)
    {
        var i = OpGrid.SelectedIndex;
        if (_move is null || _move.SameAs is not null || i < 0)
            return;
        _move.Ops.RemoveAt(i);
        _rows.RemoveAt(i);
        Reindex();
        MarkDirty();
    }

    private void OnMoveUpClicked(object sender, RoutedEventArgs e) => Shift(-1);

    private void OnMoveDownClicked(object sender, RoutedEventArgs e) => Shift(1);

    private void Shift(int d)
    {
        var i = OpGrid.SelectedIndex;
        var j = i + d;
        if (_move is null || _move.SameAs is not null || i < 0 || j < 0 || j >= _rows.Count)
            return;
        (_move.Ops[i], _move.Ops[j]) = (_move.Ops[j], _move.Ops[i]);
        _rows.Move(i, j);
        Reindex();
        OpGrid.SelectedIndex = j;
        MarkDirty();
    }

    private void OnOpCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit || e.Row.Item is not OpRow row)
            return;
        var byName = e.Column == CommandColumn;
        Dispatcher.BeginInvoke(() =>
        {
            if (byName)
            {
                // a new command: fit the operand list to what it takes
                var n = row.Name == "label" ? 1 : MoveScript.OperandCount(row.Name);
                if (n >= 0 && row.Op.Args.Count != n)
                {
                    row.Op.Args = row.Op.Args.Take(n).Concat(Enumerable.Repeat(
                        row.Name == "label" ? MoveScript.SafeLabels[0] : 0, Math.Max(0, n - row.Op.Args.Count))).ToList();
                }
            }
            row.Refresh();
            MarkDirty();
            UpdatePoseFromSelection();
        }, DispatcherPriority.Background);
    }

    // ---------- pose preview ----------

    private static bool IsPoseOp(MoveOp op) => op.Name is "actor_pose" or "target_pose" or "actor_pose_loop" or "target_pose_loop"
                                               && op.Args.Count == 7;

    private void OnOpSelected(object sender, SelectionChangedEventArgs e) => UpdatePoseFromSelection();

    private void OnPoseViewChanged(object sender, SelectionChangedEventArgs e) => UpdatePoseFromSelection();

    private int? SelectedLine()
    {
        if (OpGrid.SelectedItem is not OpRow row || !IsPoseOp(row.Op))
            return null;
        var idx = 1 + PoseSideBox.SelectedIndex * 3 + PosePartBox.SelectedIndex;
        return row.Op.Args[idx];
    }

    private void UpdatePoseFromSelection()
    {
        if (!Ready || PoseColBox.SelectedItem is not int col)
            return;
        _timer.Stop();
        PlayButton.Content = "Play";
        _frames = [];
        LineBox.Text = "";
        var line = SelectedLine();
        if (line is null)
        {
            PoseInfo.Text = "Select an actor_pose / target_pose command to see its pose.";
            Draw();
            return;
        }
        if (line < 0 || line >= _anims.Rows.Count)
        {
            PoseInfo.Text = $"Line {line} does not exist in pose_anims.json ({_anims.Rows.Count} lines).";
            Draw();
            return;
        }
        LineBox.Text = string.Join(", ", _anims.Rows[line.Value]);
        var pose = _anims.Rows[line.Value][col];
        if (!_poses.Scripts.TryGetValue(pose, out var script))
        {
            PoseInfo.Text = $"Line {line} → pose {pose}, which has no data/anim/poses/{pose:00}.json.";
            Draw();
            return;
        }
        PoseInfo.Text = $"Line {line} → pose {pose:00} ({script.KeyCount} keys, {script.Ends.Sum()} frames) for legs column {col}.";
        _frames = SkeletonRig.PlayScript(script, new PoseState(), _poseRows);
        _frame = Math.Max(0, _frames.Count - 1);
        Draw();
    }

    private void OnPlayClicked(object sender, RoutedEventArgs e)
    {
        if (_frames.Count == 0)
            return;
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            PlayButton.Content = "Play";
            return;
        }
        _frame = 0;
        _timer.Start();
        PlayButton.Content = "Pause";
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_frames.Count == 0)
            return;
        _frame++;
        if (_frame >= _frames.Count)
            _frame = 0;
        Draw();
    }

    // The bot stands at (60, 80) on the left of the close-up (root node; measured in the game) and the shot scripts
    // use absolute screen positions (x, y) for the effect sprite, so both draw in the same 240x160 view.
    private const int BotX = 60, BotY = 80;

    private void Draw()
    {
        Array.Fill(_pixels, 0xFF303040u);
        if (_rig is not null && _lists is not null)
        {
            try
            {
                var state = _frames.Count > 0 ? _frames[Math.Min(_frame, _frames.Count - 1)].State : new PoseState();
                var (sprites, pals) = _rig.Render(_lists, state, BotX, BotY);
                SkeletonRig.Rasterize(sprites, pals, _pixels, ViewW, ViewH);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Render: {ex.Message}";
            }
        }
        DrawShotMarker();
        _bitmap.WritePixels(new Int32Rect(0, 0, ViewW, ViewH), _pixels, ViewW * 4, 0);
    }

    // Where the move's first effect sprite is created (the shot leaving the barrel): operands 1 and 2 of the first
    // spawn_effect / _wave / _tiles / spawn_orbit before the first test_missed (the same rule the game uses for emit).
    private (int X, int Y)? ShotOrigin()
    {
        if (_move is null)
            return null;
        foreach (var op in _moves.Resolve(_move).Ops)
        {
            if (op.Name == "test_missed")
                return null;
            if (op.Name is "spawn_effect" or "spawn_effect_wave" or "spawn_effect_tiles" or "spawn_orbit" && op.Args.Count >= 3)
                return (op.Args[1], op.Args[2]);
        }
        return null;
    }

    private void DrawShotMarker()
    {
        if (ShotOrigin() is not { } o)
            return;
        _ = int.TryParse(EmitXBox.Text.Trim(), out var dx);
        _ = int.TryParse(EmitYBox.Text.Trim(), out var dy);
        int x = o.X + dx, y = o.Y + dy;
        DrawImportedEffect(x, y);
        for (var i = 0; i <= 12; i++) // dotted line from the script's point to the moved one
        {
            if (i % 2 == 0)
                PutPixel(o.X + (x - o.X) * i / 12, o.Y + (y - o.Y) * i / 12, 0xFFFFFFFF);
        }
        Cross(o.X, o.Y, 0xFF9090A0, 4);
        Cross(x, y, 0xFF40E0FF, 6);
    }

    // ---------- effects imported from PNG (data/anim/effects; tools/anim_effects.py builds them) ----------

    private const int EffectGfxBase = 24, EffectAnimBase = 118; // the game has 24 effect graphics, 118 sprite anims

    private sealed record EffectInfo(int Slot, string Name, string Png, int FrameW, int FrameH, int Frames);

    private string EffectsDir => Path.Combine(_root, "data", "anim", "effects");

    private List<EffectInfo> LoadEffects()
    {
        var list = new List<EffectInfo>();
        if (!Directory.Exists(EffectsDir))
            return list;
        foreach (var f in Directory.GetFiles(EffectsDir, "*.json"))
        {
            try
            {
                var j = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(f))!;
                list.Add(new EffectInfo(j["slot"]!.GetValue<int>(), j["name"]!.GetValue<string>(),
                    Path.Combine(EffectsDir, j["png"]!.GetValue<string>()), j["frame_w"]!.GetValue<int>(),
                    j["frame_h"]!.GetValue<int>(), j["frames"]!.GetValue<int>()));
            }
            catch
            {
                // a broken file is reported by the build; the editor just skips it
            }
        }
        return list.OrderBy(e => e.Slot).ToList();
    }

    private readonly Dictionary<string, (uint[] Px, int W, int H)> _effectImages = [];

    // The first spawn command's sprite anim (operand 8): if it is an imported effect, draw its first frame where the
    // shot is created (the frame is centred on that point, as the build makes it).
    private void DrawImportedEffect(int x, int y)
    {
        if (_move is null)
            return;
        var spawn = _moves.Resolve(_move).Ops.TakeWhile(op => op.Name != "test_missed")
            .FirstOrDefault(op => op.Name is "spawn_effect" or "spawn_effect_wave" or "spawn_effect_tiles" or "spawn_orbit" && op.Args.Count == 8);
        if (spawn is null || spawn.Args[7] < EffectAnimBase)
            return;
        var eff = LoadEffects().FirstOrDefault(e => e.Slot == spawn.Args[7] - EffectAnimBase);
        if (eff is null || !File.Exists(eff.Png))
            return;
        if (!_effectImages.TryGetValue(eff.Png, out var img))
        {
            var src = new FormatConvertedBitmap(new BitmapImage(new Uri(eff.Png)), PixelFormats.Bgra32, null, 0);
            var px = new uint[src.PixelWidth * src.PixelHeight];
            src.CopyPixels(px, src.PixelWidth * 4, 0);
            _effectImages[eff.Png] = img = (px, src.PixelWidth, src.PixelHeight);
        }
        for (var yy = 0; yy < eff.FrameH && yy < img.H; yy++)
            for (var xx = 0; xx < eff.FrameW && xx < img.W; xx++)
            {
                var c = img.Px[yy * img.W + xx];
                if (c >> 24 >= 128)
                    PutPixel(x - eff.FrameW / 2 + xx, y - eff.FrameH / 2 + yy, c | 0xFF000000);
            }
    }

    private static readonly (int W, int H)[] ObjSizes =
        [(8, 8), (16, 16), (32, 32), (64, 64), (16, 8), (32, 8), (32, 16), (64, 32), (8, 16), (8, 32), (16, 32), (32, 64)];

    private void OnImportEffectClicked(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "PNG image (*.png)|*.png", Title = "Effect image: a strip of frames side by side" };
        if (dlg.ShowDialog(Host) != true)
            return;
        var bmp = new BitmapImage(new Uri(dlg.FileName));
        int w = bmp.PixelWidth, h = bmp.PixelHeight;
        var guess = w % h == 0 ? w / h : 1;
        var ask = new InputDialog("Import effect", $"The image is {w}x{h}. How many frames side by side?", "Next", "Cancel",
            "frame width x height must be an OBJ size: 8, 16, 32, 64 (8x8 ... 64x64, 32x16, 16x32...)", guess.ToString()) { Owner = Host };
        if (ask.ShowDialog() != true || !int.TryParse(ask.ResponseText.Trim(), out var frames) || frames < 1 || w % frames != 0)
            return;
        var fw = w / frames;
        if (!ObjSizes.Contains((fw, h)))
        {
            MessageBox.Show(Host, $"A frame would be {fw}x{h}, which the GBA cannot draw as one sprite. Valid: " +
                string.Join(", ", ObjSizes.Select(s => $"{s.W}x{s.H}")), "Import effect", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (fw / 8 * (h / 8) * frames > 256)
        {
            MessageBox.Show(Host, "Too big: the frames need more than 256 tiles of 8x8 (fewer frames or a smaller size).",
                "Import effect", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var tickAsk = new InputDialog("Import effect", "Frames (1/60 s) each picture stays on screen:", "Import", "Cancel", "4 is the game's bullets", "4") { Owner = Host };
        if (tickAsk.ShowDialog() != true || !int.TryParse(tickAsk.ResponseText.Trim(), out var ticks) || ticks is < 1 or > 250)
            return;
        var name = new string(Path.GetFileNameWithoutExtension(dlg.FileName).ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        Directory.CreateDirectory(EffectsDir);
        var existing = LoadEffects();
        if (existing.Any(x => x.Name == name))
            name += "_" + existing.Count;
        var slot = existing.Count;
        File.Copy(dlg.FileName, Path.Combine(EffectsDir, name + ".png"), overwrite: true);
        File.WriteAllText(Path.Combine(EffectsDir, name + ".json"),
            $"{{\"slot\": {slot}, \"name\": \"{name}\", \"png\": \"{name}.png\", \"frame_w\": {fw}, \"frame_h\": {h}, " +
            $"\"frames\": {frames}, \"ticks\": {ticks}, \"loop\": true}}\n", new System.Text.UTF8Encoding(false));
        int gfx = EffectGfxBase + slot, anim = EffectAnimBase + slot;
        StatusText.Text = $"Effect '{name}': load_effect_gfx {gfx}, sprite anim {anim} (spawn_effect operand 8).";
        if (_move is { SameAs: null } m && MessageBox.Show(Host,
                $"Use '{name}' as the shot of move {m.Id:000}?\n\nSets the first load_effect_gfx to {gfx} and the first spawn command's sprite anim to {anim}.\n" +
                "Note: the effect palette is shared by everything the move draws with that graphic.",
                "Import effect", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            var ops = m.Ops.TakeWhile(op => op.Name != "test_missed").ToList();
            if (ops.FirstOrDefault(op => op.Name == "load_effect_gfx") is { } load)
                load.Args[0] = gfx;
            if (ops.FirstOrDefault(op => op.Name is "spawn_effect" or "spawn_effect_wave" or "spawn_effect_tiles" or "spawn_orbit" && op.Args.Count == 8) is { } sp)
                sp.Args[7] = anim;
            foreach (var r in _rows)
                r.Refresh();
            MarkDirty();
        }
        Draw();
    }

    private void Cross(int x, int y, uint color, int arm)
    {
        for (var d = -arm; d <= arm; d++)
        {
            PutPixel(x + d, y, color);
            PutPixel(x, y + d, color);
        }
    }

    private void PutPixel(int x, int y, uint color)
    {
        if (x >= 0 && y >= 0 && x < ViewW && y < ViewH)
            _pixels[y * ViewW + x] = color;
    }

    private void OnEmitChanged(object sender, TextChangedEventArgs e)
    {
        if (Ready)
            Draw();
    }

    // Click / drag on the preview: the blue cross goes under the pointer and Fires from becomes pointer - the
    // script's shot point (the bot stands on the left of the close-up, so forward = right).
    private void OnPreviewDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!ApplyPartButton.IsEnabled || ShotOrigin() is null)
            return;
        Preview.CaptureMouse();
        SetEmitFromPointer(e.GetPosition(Preview));
    }

    private void OnPreviewMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (Preview.IsMouseCaptured && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            SetEmitFromPointer(e.GetPosition(Preview));
    }

    private void OnPreviewUp(object sender, System.Windows.Input.MouseButtonEventArgs e) => Preview.ReleaseMouseCapture();

    private void SetEmitFromPointer(System.Windows.Point p)
    {
        if (ShotOrigin() is not { } o)
            return;
        EmitXBox.Text = Math.Clamp((int)Math.Round(p.X) - o.X, -128, 127).ToString();
        EmitYBox.Text = Math.Clamp((int)Math.Round(p.Y) - o.Y, -128, 127).ToString();
        StatusText.Text = "Fires from set on the preview: Apply to this part to keep it.";
    }

    // Up / Down nudge the number (Shift: by 5); the cross follows through OnEmitChanged.
    private void OnEmitKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is not TextBox box || e.Key is not (System.Windows.Input.Key.Up or System.Windows.Input.Key.Down))
            return;
        _ = int.TryParse(box.Text.Trim(), out var v);
        var step = System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift) ? 5 : 1;
        v = Math.Clamp(v + (e.Key == System.Windows.Input.Key.Up ? step : -step), -128, 127);
        box.Text = v.ToString();
        box.CaretIndex = box.Text.Length;
        e.Handled = true;
    }

    // pose_anims.json

    private void OnLineEdited(object sender, RoutedEventArgs e)
    {
        var line = SelectedLine();
        if (line is null || line < 0 || line >= _anims.Rows.Count)
            return;
        try
        {
            var vals = LineBox.Text.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            if (vals.Length != PoseAnimTable.Columns || vals.Any(v => !_poses.Scripts.ContainsKey(v)))
                throw new FormatException($"{PoseAnimTable.Columns} existing pose script ids");
            if (vals.SequenceEqual(_anims.Rows[line.Value]))
                return;
            _anims.Rows[line.Value] = vals;
            _anims.Dirty = true;
            StatusText.Text = $"Edited pose line {line} (unsaved)";
            UpdatePoseFromSelection();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Line poses: {ex.Message}";
            LineBox.Text = string.Join(", ", _anims.Rows[line.Value]);
        }
    }

    private void OnNewLineClicked(object sender, RoutedEventArgs e)
    {
        if (OpGrid.SelectedItem is not OpRow row || !IsPoseOp(row.Op) || _move?.SameAs is not null)
        {
            StatusText.Text = "Select an editable actor_pose / target_pose command first.";
            return;
        }
        var dlg = new InputDialog("New animation line", "Pose script for the new line (all 7 leg columns):", "Add", "Cancel",
            "0-" + _poses.Scripts.Keys.Max() + "; make a new one in the Pose Viewer first", SelectedLine() is { } l && l < _anims.Rows.Count
                ? _anims.Rows[l][(int)PoseColBox.SelectedItem].ToString() : "") { Owner = Host };
        if (dlg.ShowDialog() != true || !int.TryParse(dlg.ResponseText.Trim(), out var pose) || !_poses.Scripts.ContainsKey(pose))
            return;
        var newLine = _anims.AddRow(pose);
        row.Op.Args[1 + PoseSideBox.SelectedIndex * 3 + PosePartBox.SelectedIndex] = newLine;
        row.Refresh();
        MarkDirty();
        StatusText.Text = $"Line {newLine} added: pose {pose:00} on every leg type (unsaved)";
        UpdatePoseFromSelection();
    }

    // ---------- who plays it ----------

    private void OnTargetChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BotBox.SelectedItem is not Target t || SlotBox.SelectedIndex < 0)
            return;
        var sel = SlotBox.SelectedIndex;
        var noForm = false;
        if (sel >= 4) // Drive A / B / C of the bot's Medachange form
            noForm = !ShowDrive(t, "ABC"[sel - 4]);
        else
        {
            ApplyPartButton.IsEnabled = true;
            var slot = BaseBotMoves.Slots[sel];
            PartMove pm;
            if (t.BaseId is { } id)
                pm = _baseMoves.Get(id, slot);
            else
            {
                var part = t.Bot!.Part(slot);
                var emit = part["emit"] as System.Text.Json.Nodes.JsonArray;
                pm = new PartMove(part["anim"]?.GetValue<int>(), emit?[0]?.GetValue<int>(), emit?[1]?.GetValue<int>());
            }
            PlaysBox.Text = pm.Anim?.ToString() ?? "";
            EmitXBox.Text = (pm.EmitX ?? 0).ToString();
            EmitYBox.Text = (pm.EmitY ?? 0).ToString();
            PlaysName.Text = pm.Anim is { } a && _moves.Scripts.TryGetValue(a, out var m) ? m.Name : "";
        }
        // the pose this part plays: head / right arm / left arm (Drive A / B / C = the same three)
        var poseSlot = sel >= 4 ? sel - 4 : sel;
        if (poseSlot < PosePartBox.Items.Count && PosePartBox.SelectedIndex != poseSlot)
            PosePartBox.SelectedIndex = poseSlot;
        ShowBot(t);
        if (noForm)
            StatusText.Text = "This bot has no Medachange form.";
    }

    // A Drive of the Medachange form: "anim" / "emit" like a part, and the technique it plays when there is no "anim".
    // Returns false when the bot has no form (the boxes are cleared and Apply is disabled).
    private bool ShowDrive(Target t, char drive)
    {
        if (t.Bot?.Medachange is null)
        {
            PlaysBox.Text = "";
            EmitXBox.Text = "";
            EmitYBox.Text = "";
            PlaysName.Text = "";
            ApplyPartButton.IsEnabled = false;
            return false;
        }
        ApplyPartButton.IsEnabled = true;
        var d = DriveOf(t.Bot!, drive, create: false);
        var emit = d?["emit"] as System.Text.Json.Nodes.JsonArray;
        var anim = d?["anim"]?.GetValue<int>();
        var technique = d?["technique"]?.GetValue<int>();
        PlaysBox.Text = anim?.ToString() ?? "";
        EmitXBox.Text = (emit?[0]?.GetValue<int>() ?? 0).ToString();
        EmitYBox.Text = (emit?[1]?.GetValue<int>() ?? 0).ToString();
        if (anim is { } a && _moves.Scripts.TryGetValue(a, out var m))
            PlaysName.Text = m.Name;
        else if (technique is { } tech && _moves.Scripts.TryGetValue(tech, out var tm))
            PlaysName.Text = $"{tm.Name} (technique)";
        else
            PlaysName.Text = "";
        return true;
    }

    // The Drive object of the bot's Medachange form ("drives" -> "A" / "B" / "C"); created on demand when asked.
    private static System.Text.Json.Nodes.JsonObject? DriveOf(DecompBot bot, char drive, bool create)
    {
        var form = bot.Medachange;
        if (form is null)
            return null;
        if (form["drives"] is not System.Text.Json.Nodes.JsonObject drives)
        {
            if (!create)
                return null;
            drives = new System.Text.Json.Nodes.JsonObject();
            form["drives"] = drives;
        }
        var key = drive.ToString();
        if (drives[key] is System.Text.Json.Nodes.JsonObject found)
            return found;
        if (!create)
            return null;
        var fresh = new System.Text.Json.Nodes.JsonObject();
        drives[key] = fresh;
        return fresh;
    }

    // The preview draws the chosen bot (its id in the loaded build) on its own legs column; the Tinpet when the bot is
    // not in that build. Drive A / B / C draw the bot's Medachange form, as the game does: the four parts become the
    // id MEDACHANGE_FORM_ID(k) = Tinpet + 1 + k, with k = gNaviMedachange[bot] (battle_hud.c).
    private void ShowBot(Target t)
    {
        if (_rig is null)
            return;
        int? id = t.BaseId ?? (t.Bot is { } b && _botIds.TryGetValue(b.Key, out var v) ? v : null);
        if (id is null)
            StatusText.Text = $"{t.Bot?.Key} is not in the loaded build ({_buildName}): the preview shows the Tinpet. Build ROM with it in the list.";
        var draw = id ?? _tinpet;
        if (SlotBox.SelectedIndex >= 4 && id is { } formBot)
        {
            var k = MedachangeIndex(formBot);
            if (k is > 0)
                draw = _tinpet + 1 + k.Value;
            else
                StatusText.Text = $"The Medachange form of {t.Display} is not in this build (no gNaviMedachange entry): the preview shows the normal bot.";
        }
        try
        {
            _lists = _rig.PartLists(draw, draw, draw, draw);
            var col = (int)_rig.Rom[_rig.Off("gLegPartPoseSets") + draw];
            if (PoseColBox.Items.Contains(col) && !Equals(PoseColBox.SelectedItem, col))
                PoseColBox.SelectedItem = col; // replays the pose for this bot's legs
            else
                Draw();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Preview: {ex.Message}";
        }
    }

    // k of gNaviMedachange[botId] (0 = the bot has no form); null when this build has no such table.
    private int? MedachangeIndex(int botId)
    {
        if (_rig is null || !_rig.Symbols.ContainsKey("gNaviMedachange"))
            return null;
        return _rig.Rom[_rig.Off("gNaviMedachange") + botId];
    }

    private void OnApplyPartClicked(object sender, RoutedEventArgs e)
    {
        if (BotBox.SelectedItem is not Target t || SlotBox.SelectedIndex < 0)
            return;
        var sel = SlotBox.SelectedIndex;
        var drive = sel >= 4 ? "ABC"[sel - 4] : '\0';
        var slot = sel < 4 ? BaseBotMoves.Slots[sel] : $"drive {drive}";
        int? anim = null;
        if (!string.IsNullOrWhiteSpace(PlaysBox.Text))
        {
            if (!int.TryParse(PlaysBox.Text.Trim(), out var a) || !_moves.Scripts.ContainsKey(a) || a > 254)
            {
                StatusText.Text = $"'{PlaysBox.Text}' is not a move script id (0-254 that exists in data/anim/seq).";
                return;
            }
            anim = a;
        }
        if (!int.TryParse(EmitXBox.Text.Trim(), out var ex) || !int.TryParse(EmitYBox.Text.Trim(), out var ey) ||
            ex is < -128 or > 127 || ey is < -128 or > 127)
        {
            StatusText.Text = "Fires from: whole pixels from -128 to 127.";
            return;
        }
        if (sel >= 4)
        {
            if (t.Bot?.Medachange is null)
            {
                StatusText.Text = "This bot has no Medachange form.";
                return;
            }
            var dd = DriveOf(t.Bot!, drive, create: true)!;
            if (anim is { } da)
                dd["anim"] = da;
            else
                dd.Remove("anim");
            if (ex != 0 || ey != 0)
                dd["emit"] = new System.Text.Json.Nodes.JsonArray(ex, ey);
            else
                dd.Remove("emit");
            t.Bot!.Dirty = true;
        }
        else if (t.BaseId is { } id)
        {
            _baseMoves.Set(id, slot, new PartMove(anim, ex, ey));
            _baseDirty = true;
        }
        else
        {
            var part = t.Bot!.Part(slot);
            if (anim is { } a)
                part["anim"] = a;
            else
                part.Remove("anim");
            if (ex != 0 || ey != 0)
                part["emit"] = new System.Text.Json.Nodes.JsonArray(ex, ey);
            else
                part.Remove("emit");
            t.Bot.Dirty = true;
        }
        if (sel >= 4)
            ShowDrive(t, drive); // the technique's name again when the Drive has no "anim"
        else
            PlaysName.Text = anim is { } n && _moves.Scripts.TryGetValue(n, out var mv) ? mv.Name : "";
        StatusText.Text = $"{t.Display.Split("  (")[0]}, {slot}: " + (anim is null ? "its skill's own move" : $"plays move {anim:000}") + " (unsaved)";
    }

    // ---------- saving ----------

    private bool AnyDirty => _moves.Scripts.Values.Any(s => s.Dirty) || _anims.Dirty || _baseDirty || _store.Bots.Any(b => b.Dirty);

    private void OnSaveClicked(object sender, RoutedEventArgs e) => SaveChanges(askAboutBad: true);

    /// <summary>Hosted: the main window asks about these edits on close (its Closing never reaches the hosted window).</summary>
    public bool HasUnsavedChanges => _moves is not null && AnyDirty;

    /// <summary>Saves everything dirty (the Save button's work, without asking about bad scripts). False if not saved.</summary>
    public bool SaveAllChanges()
    {
        try
        {
            return SaveChanges(askAboutBad: false);
        }
        catch (Exception ex)
        {
            StatusText.Text = "Save: " + ex.Message;
            return false;
        }
    }

    /// <summary>Stops the Play timer (a hosted window that is dropped keeps no animation running).</summary>
    public void StopPlayback() => _timer.Stop();

    private bool SaveChanges(bool askAboutBad)
    {
        var bad = _moves.Scripts.Values.Where(s => s.Dirty && s.Validate().Any()).ToList();
        if (bad.Count > 0)
        {
            var list = string.Join(", ", bad.Select(s => s.Id.ToString("000")));
            if (!askAboutBad)
            {
                StatusText.Text = $"Not saved: scripts with problems ({list}). The generator will refuse them.";
                return false;
            }

            if (MessageBox.Show(Host,
                    $"Scripts with problems: {list}. The generator will refuse them. Save anyway?",
                    "Move Editor", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return true;
        }
        var saved = new List<string>();
        foreach (var s in _moves.Scripts.Values.Where(s => s.Dirty).ToList())
        {
            s.Save();
            saved.Add($"seq/{s.Id:000}");
        }
        if (_anims.Dirty)
        {
            _anims.Save();
            saved.Add("pose_anims");
        }
        if (_baseDirty)
        {
            _baseMoves.Save();
            _baseDirty = false;
            saved.Add("base_bots");
        }
        foreach (var b in _store.Bots.Where(b => b.Dirty).ToList())
        {
            b.Save();
            saved.Add(b.Key);
        }
        StatusText.Text = saved.Count == 0 ? "Nothing to save." : $"Saved {string.Join(", ", saved)}. Build ROM in Decomp Project to play it.";
        return true;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        _timer.Stop();
        if (_moves is not null && AnyDirty && MessageBox.Show(Host, "Discard the unsaved edits?", "Move Editor",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            e.Cancel = true;
    }

    private void OnPoseViewerClicked(object sender, RoutedEventArgs e)
    {
        if (OpenSubtab is { } openSubtab)
        {
            openSubtab("Poses");
            return;
        }

        new PoseViewerWindow(_root) { Owner = Host }.Show();
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
