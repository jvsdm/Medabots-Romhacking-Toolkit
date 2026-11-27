using Medabots.Rom.Metadata;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Medabots.Rom.WPFEditor.Decomp;
using Binding = System.Windows.Data.Binding;
using CheckBox = System.Windows.Controls.CheckBox;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using TextBox = System.Windows.Controls.TextBox;
using Brush = System.Windows.Media.Brush;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Medabots.Rom.WPFEditor.Dialogs;

/// <summary>
/// Edits the bots of a decompilation project (data/bots, data/navi/bots JSON: names, the 16 bytes of each part,
/// the Medachange form legs and Drives) and builds a ROM with tools/decomp_build.py. Never touches a ROM directly:
/// the JSON files are the source of truth, the generator turns them into the ROM.
/// </summary>
public partial class DecompProjectWindow : Window
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medabots-Romhacking-Toolkit",
        "decomp-project.json");

    private static readonly string[] ArtMethods = ["auto", "liq", "fusao", "maxcov", "mediancut"];

    // tools/export_template.py ("auto") reads the pictures from the newest ROM of tools/decomp_build.py, with its .map
    private bool HasBuiltRom()
    {
        var scratch = Path.Combine(_store!.Root, "build", "scratch");
        return Directory.Exists(scratch) && Directory.EnumerateDirectories(scratch, "decomp_*")
            .Any(d => File.Exists(Path.Combine(d, "medabots_rokusho.gba")) && File.Exists(Path.Combine(d, "medabots_rokusho.map")));
    }

    private readonly Func<string, Task>? _openRom;
    private readonly Dictionary<DecompBot, JsonNode> _parkedForms = [];
    private DecompBotStore? _store;
    private DecompBot? _current;
    private FieldCatalog _fields = FieldCatalog.For(null);
    private bool _building;

    private UIElement? _hostedContent;

    /// <summary>The window is hosted in the main window's Bots tab (never shown): dialogs are owned by the main window.</summary>
    private Window Host => _hostedContent is null ? this : (Window.GetWindow(_hostedContent) ?? this);

    /// <summary>Set by the main window: opens the Moves / Skeleton / Poses subtab instead of a separate window.</summary>
    public Action<string>? OpenSubtab { get; set; }

    /// <summary>Raised after a project folder loads (also the first time). The main window follows it in Skeleton / Moves / Poses.</summary>
    public event Action<string>? ProjectRootChanged;

    private string? _loadedRoot;

    /// <summary>Bot edits not saved yet (the main window asks about them on close).</summary>
    public bool HasUnsavedChanges => _store?.Bots.Any(b => b.Dirty) == true;

    public DecompProjectWindow(string? suggestedProject, Func<string, Task>? openRom)
    {
        InitializeComponent();
        _openRom = openRom;
        var settings = LoadSettings();
        ProjectPathBox.Text = DecompBotStore.LooksLikeProject(suggestedProject) ? suggestedProject! : settings.Root ?? "";
        PythonBox.Text = settings.Python ?? "python";
        Loaded += (_, _) => LoadProject();
        Closing += OnWindowClosing;
    }

    /// <summary>Hosted in the Bots tab: the window is never shown, so Loaded never fires; this does the same work.</summary>
    public void InitHosted(UIElement hostedContent)
    {
        _hostedContent = hostedContent;
        CloseWindowButton.Visibility = Visibility.Collapsed;
        // The main window's project bar (folder, Build ROM, Open build, log) and the Move/Skeleton/Pose buttons (now subtabs) replace these.
        ProjectBar.Visibility = Visibility.Collapsed;
        MoveEditorButton.Visibility = Visibility.Collapsed;
        SkeletonEditorButton.Visibility = Visibility.Collapsed;
        PoseViewerButton.Visibility = Visibility.Collapsed;
        BuildButton.Visibility = Visibility.Collapsed;
        LogBox.Visibility = Visibility.Collapsed;
        if (hostedContent is Grid { RowDefinitions.Count: 4 } grid)
        {
            grid.RowDefinitions[3].Height = new GridLength(0);
        }

        LoadProject();
    }

    /// <summary>Hosted: the main window's log receives the lines (the LogBox is hidden).</summary>
    public Action<string>? LogSink { get; set; }

    /// <summary>The project folder in the window now (the main window compares it with the one that applies).</summary>
    public string ProjectFolder => ProjectPathBox.Text.Trim();

    /// <summary>Hosted: loads another project folder (the caller has already asked about unsaved edits).</summary>
    public void ShowProject(string root)
    {
        ProjectPathBox.Text = root;
        LoadProject();
    }

    // ---------- project ----------

    private void LoadProject()
    {
        var root = ProjectPathBox.Text.Trim();
        if (!DecompBotStore.LooksLikeProject(root))
        {
            StatusText.Text = $"Pick the Medabots project folder (the one with {DecompBotStore.BotListFile}).";
            BotList.ItemsSource = null;
            return;
        }

        var loaded = false;
        try
        {
            _store = new DecompBotStore(root);
            _store.Load();
            // ids of the newest build (bot_ids.json next to its ROM), so the bot list shows NNN like every other screen
            var rom = PoseViewerWindow.NewestBuild(root);
            var keysById = rom is null ? new Dictionary<int, string>() : BotLabels.KeysById(rom);
            var idByKey = keysById.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.Ordinal);
            foreach (var bot in _store.Bots)
            {
                bot.Id = idByKey.TryGetValue(bot.Key, out var id) ? id : null;
            }

            _fields = FieldCatalog.For(root);
            _parkedForms.Clear();
            ApplyFilter();
            StatusText.Text = $"{_store.Bots.Count} bots ({_store.Bots.Count(b => b.InList)} in the bot list)";
            SaveSettings();
            loaded = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = "Could not load the project: " + ex.Message;
        }

        if (loaded && root != _loadedRoot)
        {
            _loadedRoot = root;
            ProjectRootChanged?.Invoke(root);
        }
    }

    private void ApplyFilter()
    {
        if (_store is null)
        {
            return;
        }

        var filter = FilterBox.Text.Trim();
        var selected = _current;
        BotList.ItemsSource = _store.Bots
            .Where(b => filter.Length == 0 || b.Key.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        b.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (selected is not null)
        {
            BotList.SelectedItem = selected;
        }
    }

    private void RefreshList()
    {
        var selected = BotList.SelectedItem;
        BotList.Items.Refresh();
        BotList.SelectedItem = selected;
        var dirty = _store?.Bots.Count(b => b.Dirty) ?? 0;
        StatusText.Text = dirty == 0 ? "No unsaved changes" : $"{dirty} bot(s) with unsaved changes";
    }

    // ---------- editor ----------

    private void OnBotSelected(object sender, SelectionChangedEventArgs e)
    {
        _current = BotList.SelectedItem as DecompBot;
        PartTabs.Items.Clear();
        if (_current is null)
        {
            NameBox.Text = "";
            BotInfoText.Text = "";
            return;
        }

        var bot = _current;
        NameBox.Text = bot.Name;
        BotInfoText.Text = $"{Path.GetFileName(bot.FilePath)}   {(bot.InList ? "in the bot list" : "NOT in the bot list")}";
        string[] titles = ["Head", "Right arm", "Left arm", "Legs"];
        for (var k = 0; k < 4; k++)
        {
            PartTabs.Items.Add(new TabItem { Header = titles[k], Content = PartEditor(bot, DecompBot.Slots[k]) });
        }

        if (bot.Medachange is not null || _parkedForms.ContainsKey(bot) || OwnArtPack(bot))
        {
            PartTabs.Items.Add(new TabItem { Header = "Medachange", Content = MedachangeEditor(bot) });
        }

        PartTabs.Items.Add(new TabItem { Header = "Art", Content = ArtEditor(bot) });

        PartTabs.SelectedIndex = 0;
    }

    private UIElement PartEditor(DecompBot bot, string slot)
    {
        var part = bot.Part(slot);
        var panel = new DockPanel { Margin = new Thickness(6) };
        var top = new StackPanel { Margin = new Thickness(0, 0, 0, 6) };
        var title = new TextBlock { FontSize = 14, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
        string TitleText() => $"{part["name"]?.GetValue<string>() ?? ""}  ({PartSlotTitle(slot)})";
        title.Text = TitleText();
        top.Children.Add(title);
        var nameRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
        nameRow.Children.Add(new TextBlock { Text = "Part name:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
        var name = new TextBox { Width = 220, Text = part["name"]?.GetValue<string>() ?? "" };
        name.LostFocus += (_, _) =>
        {
            if (name.Text != (part["name"]?.GetValue<string>() ?? ""))
            {
                part["name"] = name.Text;
                title.Text = TitleText();
                Changed(bot);
            }
        };
        nameRow.Children.Add(name);
        if (part["name_jp"] is { } jp)
        {
            nameRow.Children.Add(new TextBlock { Text = "  " + jp.GetValue<string>(), VerticalAlignment = VerticalAlignment.Center });
        }

        top.Children.Add(nameRow);
        DockPanel.SetDock(top, Dock.Top);
        panel.Children.Add(top);

        // The same card layout as the Parts tab: groups in the Parts order, each field a label, its control and one
        // muted explanation line. Values are written through FieldRow, so the JSON and its validation are unchanged.
        var stats = (JsonObject)part["stats"]!;
        var groups = new Dictionary<PartGroup, List<FrameworkElement>>();
        foreach (var key in stats.Select(kv => kv.Key).ToList())
        {
            var info = PartFieldInfo(key, slot);
            var row = new FieldRow(stats, key, () => Changed(bot), Describe, Choices);
            if (!groups.TryGetValue(info.Group, out var cards))
            {
                groups[info.Group] = cards = [];
            }

            cards.Add(FieldCard(row, key, info.Label, info.Note));
        }

        var content = new StackPanel();
        foreach (var (group, header) in new[]
                 {
                     (PartGroup.Core, "Core Fields"),
                     (PartGroup.Kind, "Kind-Specific Stats"),
                     (PartGroup.Derived, "Derived / Scaling Fields"),
                 })
        {
            if (!groups.TryGetValue(group, out var cards))
            {
                continue;
            }

            content.Children.Add(new TextBlock { Margin = new Thickness(0, 14, 0, 0), Text = header, FontWeight = FontWeights.SemiBold });
            content.Children.Add(TwoColumns(cards, new Thickness(0, 8, 0, 0)));
        }

        if (groups.TryGetValue(PartGroup.Raw, out var raw))
        {
            content.Children.Add(new Expander
            {
                Margin = new Thickness(0, 14, 0, 0),
                Header = "Raw bytes",
                IsExpanded = false,
                Content = TwoColumns(raw, new Thickness(0, 8, 0, 0)),
            });
        }

        panel.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = content });
        return panel;
    }

    private enum PartGroup { Core, Kind, Derived, Raw }

    private static string PartSlotTitle(string slot) => slot switch
    {
        "head" => "Head",
        "right_arm" => "Right arm",
        "left_arm" => "Left arm",
        _ => "Legs",
    };

    // Labels and explanation lines are the Parts tab's (MainWindow.xaml.cs RefreshPartEditorHelp) for the same fields.
    private static (PartGroup Group, string Label, string Note) PartFieldInfo(string key, string slot) => key switch
    {
        "medal_compat" => (PartGroup.Core, "Compatibility Attribute",
            "Matching this against the bot's medal class contributes the medal-part compatibility bonus."),
        "speciality" => (PartGroup.Core, "Speciality", "Speciality grouping."),
        "female" => (PartGroup.Core, "Gender Restriction", "Gender restriction: 0 = male / default, 1 = female / alternate."),
        "armor" => (PartGroup.Core, "Armor", "Armor is the part HP value contributed in battle."),
        "leg_type" => (PartGroup.Kind, "Leg Type",
            "Leg type is the movement/chassis family used by movement formulas."),
        "technique" => (PartGroup.Kind, "Technique", "Technique selects the action family."),
        "success" => (PartGroup.Kind, "Rate of Success", "Rate of Success is the base accuracy / reliability stat for the action."),
        "power" => (PartGroup.Kind, "Power", "Power is the base damage or effect strength."),
        "chain_reaction" => (PartGroup.Kind, "Chain Reaction",
            "Chain Reaction is a boolean toggle stored in the shared slot-specific stat byte at record offset +7."),
        "uses_or_charge" => slot == "head"
            ? (PartGroup.Kind, "Uses", "Uses is the head ammo / remaining-use count. Battle code clamps head consumption against this byte.")
            : (PartGroup.Kind, "Charge", "Charge is the arm action charge value. This shares the same raw record byte that heads use for Uses."),
        "propulsion" => (PartGroup.Kind, "Propulsion", "Propulsion controls movement performance and speed-related handling."),
        "evasion" => (PartGroup.Kind, "Evasion", "Evasion affects how well the Medabot avoids incoming attacks."),
        "defense" => (PartGroup.Kind, "Defense", "Defense affects how well the legs absorb or mitigate hits."),
        "proximity" => (PartGroup.Kind, "Proximity", "Proximity is the legs-derived bonus used for close-range specialities."),
        "remoteness" => (PartGroup.Kind, "Remoteness", "Remoteness is the legs-derived bonus used for ranged specialities."),
        "radiation" => (PartGroup.Derived, "Radiation", "Radiation is a per-part scalar used by combat logic for radiation-type interactions."),
        "buy_price" => (PartGroup.Derived, "Price", "Price is the shop price in whole units (11 is shown in-game as 11,00)."),
        _ => (PartGroup.Raw, key, "Raw record byte; its gameplay meaning is not resolved yet."),
    };

    // One field: bold label, its control (dropdown, checkbox or number box) and one muted line: the explanation, then
    // Describe(field, value) in brackets when there is one. Each change goes through FieldRow (validation, JSON write).
    private FrameworkElement FieldCard(FieldRow row, string key, string label, string note)
    {
        var card = new StackPanel();
        card.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold });
        var hint = new TextBlock { Margin = new Thickness(0, 4, 0, 0), TextWrapping = TextWrapping.Wrap };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "Theme.TextMuted");

        void ShowHint(string? error = null)
        {
            var meaning = Describe(key, row.Value);
            var text = error ?? note;
            hint.Text = meaning.Length == 0 || error is not null ? text : $"{text}  ({meaning})";
        }

        if (key == "chain_reaction")
        {
            var check = new CheckBox { Content = "Enabled", Margin = new Thickness(0, 4, 0, 0), IsChecked = row.Value != 0 };
            check.Checked += (_, _) => { row.Value = 1; ShowHint(); };
            check.Unchecked += (_, _) => { row.Value = 0; ShowHint(); };
            card.Children.Add(check);
        }
        else if (row.Choices is { } choices)
        {
            var box = new ComboBox
            {
                Margin = new Thickness(0, 4, 0, 0),
                ItemsSource = choices,
                DisplayMemberPath = nameof(Choice.Text),
                SelectedValuePath = nameof(Choice.Id),
                SelectedValue = row.Value,
                MaxDropDownHeight = 420,
            };
            box.SelectionChanged += (_, _) =>
            {
                if (box.SelectedValue is int id && id != row.Value)
                {
                    row.Value = id;
                    ShowHint();
                }
            };
            card.Children.Add(box);
        }
        else
        {
            var entry = new TextBox { Margin = new Thickness(0, 4, 0, 0), Text = row.Value.ToString() };
            void Commit()
            {
                if (int.TryParse(entry.Text.Trim(), out var value) && value is >= 0 and <= 255)
                {
                    row.Value = value;
                    ShowHint();
                }
                else
                {
                    entry.Text = row.Value.ToString();
                    ShowHint("Enter a number from 0 to 255.");
                }
            }

            entry.LostFocus += (_, _) => Commit();
            entry.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    Commit();
                }
            };
            card.Children.Add(entry);
        }

        ShowHint();
        card.Children.Add(hint);
        return card;
    }

    // Two columns like the Parts tab: fields go left, right, left, ...
    private static Grid TwoColumns(List<FrameworkElement> cards, Thickness firstRowMargin)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < cards.Count; i += 2)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var row = i / 2;
            card.Margin = row == 0 ? firstRowMargin : new Thickness(0, 8, 0, 0);
            Grid.SetRow(card, row);
            Grid.SetColumn(card, i % 2 == 0 ? 0 : 2);
            grid.Children.Add(card);
        }

        return grid;
    }

    private UIElement MedachangeEditor(DecompBot bot)
    {
        var panel = new DockPanel { Margin = new Thickness(6) };
        var hasForm = new CheckBox
        {
            Content = "This bot transforms (unchecked = \"medachange\": null, the bot has no form in the ROM)",
            IsChecked = bot.Medachange is not null,
            Margin = new Thickness(0, 0, 0, 6),
        };
        DockPanel.SetDock(hasForm, Dock.Top);
        panel.Children.Add(hasForm);
        var body = new StackPanel();
        panel.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = body });

        // Cards like the part form: Core (the form's legs), then one card per Drive (A, B, C), then the pictures.
        void Fill()
        {
            body.Children.Clear();
            if (bot.Medachange is not { } mc)
            {
                body.Children.Add(new TextBlock { Text = "No form.", Foreground = SystemParameters.HighContrast ? null : (System.Windows.Media.Brush)FindResource("Theme.TextMuted") });
                return;
            }

            body.Children.Add(CardTitle("Core (form legs; leg_type 100-106)"));
            var legs = (JsonObject)mc["legs"]!;
            var legCards = new List<FrameworkElement>();
            foreach (var key in legs.Select(kv => kv.Key).ToList())
            {
                var info = PartFieldInfo(key, "legs");
                legCards.Add(FieldCard(new FieldRow(legs, key, () => Changed(bot), Describe, Choices), key, info.Label, info.Note));
            }

            body.Children.Add(TwoColumns(legCards, new Thickness(0, 8, 0, 0)));

            var drives = (JsonObject)mc["drives"]!;
            var driveObjs = drives.Select(kv => (JsonObject)kv.Value!).ToList();
            var techniques = DriveChoices(Choices("technique")?.ToList() ?? new List<Choice>(), driveObjs.Select(d => d["technique"]?.GetValue<int>() ?? 0));
            techniques.Add(new Choice(255, "255  (none: part stays)"));
            var medals = DriveChoices(Choices("medal_compat")?.ToList() ?? new List<Choice>(), driveObjs.Select(d => d["medal_compat"]?.GetValue<int>() ?? 255).Where(v => v != 255));
            medals.Add(new Choice(255, "255  (the part's own)"));
            var specialities = DriveChoices(Choices("speciality")?.ToList() ?? new List<Choice>(), driveObjs.Select(d => d["speciality"]?.GetValue<int>() ?? 0));
            foreach (var kv in drives)
            {
                var drive = new DriveRow((JsonObject)kv.Value!, kv.Key, () => Changed(bot));
                body.Children.Add(CardTitle($"Drive {drive.Drive}  (the form's head / right arm / left arm)"));
                body.Children.Add(TwoColumns(DriveCards(drive, techniques, medals, specialities), new Thickness(0, 8, 0, 0)));
            }

            if (OwnArtPack(bot))
                body.Children.Add(FormPictures(bot, mc));
        }

        void OnHasForm(object? _s, RoutedEventArgs _e)
        {
            if (hasForm.IsChecked == true && bot.Medachange is null && _parkedForms.Remove(bot, out var form))
            {
                bot.Json["medachange"] = form;
            }
            else if (hasForm.IsChecked == true && bot.Medachange is null && OwnArtPack(bot))
            {
                bot.Json["medachange"] = DefaultForm(bot);
            }
            else if (hasForm.IsChecked != true && bot.Medachange is { } mc)
            {
                _parkedForms[bot] = mc.DeepClone();
                bot.Json["medachange"] = null;
            }

            Changed(bot);
            Fill();
        }
        // Checked/Unchecked (e nao Click): valem para mouse, teclado e automacao
        hasForm.Checked += OnHasForm;
        hasForm.Unchecked += OnHasForm;
        Fill();
        return panel;
    }

    private static TextBlock CardTitle(string text) =>
        new() { Text = text, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 12, 0, 0), TextWrapping = TextWrapping.Wrap };

    // One card: bold label, its control, one muted explanation line (the same look as FieldCard).
    private static StackPanel Card(string label, string note, FrameworkElement control)
    {
        var card = new StackPanel();
        card.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold });
        control.Margin = new Thickness(0, 4, 0, 0);
        card.Children.Add(control);
        var hint = new TextBlock { Margin = new Thickness(0, 4, 0, 0), TextWrapping = TextWrapping.Wrap, Text = note };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "Theme.TextMuted");
        card.Children.Add(hint);
        return card;
    }

    private static StackPanel ChoiceCard(string label, string note, IEnumerable<Choice> items, Func<int> get, Action<int> set)
    {
        var box = new ComboBox
        {
            ItemsSource = items.ToList(), DisplayMemberPath = nameof(Choice.Text), SelectedValuePath = nameof(Choice.Id),
            SelectedValue = get(), MaxDropDownHeight = 420,
        };
        box.SelectionChanged += (_, _) =>
        {
            if (box.SelectedValue is int id && id != get())
            {
                set(id);
            }
        };
        return Card(label, note, box);
    }

    private static StackPanel NumberCard(string label, string note, Func<int> get, Action<int> set)
    {
        var entry = new TextBox { Text = get().ToString() };
        void Commit()
        {
            if (int.TryParse(entry.Text.Trim(), out var value) && value is >= 0 and <= 255 && value != get())
            {
                set(value);
            }

            entry.Text = get().ToString();
        }

        entry.LostFocus += (_, _) => Commit();
        entry.KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Commit();
            }
        };
        return Card(label, note, entry);
    }

    // The fields of one Drive, in the order of the part form. Each change goes through DriveRow (same JSON keys).
    private List<FrameworkElement> DriveCards(DriveRow d, List<Choice> techniques, List<Choice> medals, List<Choice> specialities)
    {
        var effect = new ComboBox { ItemsSource = _fields.Effects, SelectedItem = d.Effect, MaxDropDownHeight = 420 };
        effect.SelectionChanged += (_, _) =>
        {
            if (effect.SelectedItem is string s && s != d.Effect)
            {
                d.Effect = s;
            }
        };
        var chain = new CheckBox { IsThreeState = true, Content = "Enabled", IsChecked = d.Chain };
        chain.Checked += (_, _) => d.Chain = true;
        chain.Unchecked += (_, _) => d.Chain = false;
        chain.Indeterminate += (_, _) => d.Chain = null;
        return
        [
            ChoiceCard("Technique", "The action the drive plays.", techniques, () => d.Technique, v => d.Technique = v),
            ChoiceCard("Medal class", "Class of the part while in the form; 255 = the part's own class.", medals, () => d.Medal, v => d.Medal = v),
            ChoiceCard("Speciality", "Speciality of the drive's action.", specialities, () => d.Speciality, v => d.Speciality = v),
            NumberCard("Rate of Success", "Base accuracy / reliability of the drive's action.", () => d.Success, v => d.Success = v),
            NumberCard("Power", "Base damage or effect strength of the drive.", () => d.Power, v => d.Power = v),
            Card("Effect", "Special effect of the drive; none = no effect.", effect),
            Card("Chain Reaction (◎)", "Indeterminate = the Navi's own value; checked / unchecked overrides it.", chain),
        ];
    }

    // Fields with a known list of values show a dropdown with names (the JSON keeps the number).
    private IReadOnlyList<Choice>? Choices(string field)
    {
        var meta = Medabots.Rom.Metadata.MedabotsMetadata.Default;
        return field switch
        {
            // the part's medal class: a medal of the same class gets the part bonus (re/functions/R1b.md)
            "medal_compat" => Enumerable.Range(0, FieldCatalog.MedalClassCount)
                .Select(id => new Choice(id, $"{id:000}  {_fields.MedalClassLabel(id)}")).ToList(),
            "technique" => Enumerable.Range(0, _fields.TechniqueCount)
                .Select(id => new Choice(id, $"{id:000}  {_fields.TechniqueName(id)}")).ToList(),
            "speciality" => Enumerable.Range(0, _fields.SpecialityCount)
                .Select(id => new Choice(id, $"{id:000}  {_fields.SpecialityName(id)}")).ToList(),
            "leg_type" => _fields.LegTypeIds.Select(id => new Choice(id, $"{id:000}  {_fields.LegTypeName(id)}")).ToList(),
            "female" => [new Choice(0, $"000  {_fields.GenderName(0)}"), new Choice(1, $"001  {_fields.GenderName(1)}")],
            "chain_reaction" => [new Choice(0, "0  No"), new Choice(1, "1  Chain Reaction (◎)")],
            _ => null,
        };
    }

    // A Drive's dropdown: the list, plus a "(?)" entry for any stored value that is not in it (so the box never shows blank).
    private static List<Choice> DriveChoices(List<Choice> list, IEnumerable<int> stored)
    {
        foreach (var v in stored.Distinct())
            if (list.All(c => c.Id != v))
                list.Add(new Choice(v, $"{v:000}  (?)"));
        return list;
    }

    public sealed record Choice(int Id, string Text)
    {
        public override string ToString() => Text;
    }

    private static readonly DataTemplate NumberCell = (DataTemplate)System.Windows.Markup.XamlReader.Parse(
        "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
        "<TextBlock Text='{Binding Value}' VerticalAlignment='Center' Margin='4,0' /></DataTemplate>");

    private static readonly DataTemplate NumberEdit = (DataTemplate)System.Windows.Markup.XamlReader.Parse(
        "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
        "<TextBox Text='{Binding Value, UpdateSourceTrigger=LostFocus, ValidatesOnExceptions=True}' /></DataTemplate>");

    private static readonly DataTemplate ChoiceCell = (DataTemplate)System.Windows.Markup.XamlReader.Parse(
        "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
        "<ComboBox ItemsSource='{Binding Choices}' SelectedValuePath='Id' MaxDropDownHeight='420' " +
        "SelectedValue='{Binding Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}' /></DataTemplate>");

    // Three states: unchecked = false, checked = true, indeterminate (null) = key removed, the Navi's own value
    private static readonly DataTemplate ChainCell = (DataTemplate)System.Windows.Markup.XamlReader.Parse(
        "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
        "<CheckBox IsThreeState='True' HorizontalAlignment='Center' VerticalAlignment='Center' " +
        "IsChecked='{Binding Chain, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}' /></DataTemplate>");

    private sealed class ValueTemplates(bool editing) : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) =>
            item is FieldRow { Choices: not null } ? ChoiceCell : editing ? NumberEdit : NumberCell;
    }

    private static DataGrid FieldGrid(List<FieldRow> rows)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserSortColumns = false,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            ItemsSource = rows,
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Field", Binding = new Binding(nameof(FieldRow.Field)), IsReadOnly = true, Width = 160 });
        grid.Columns.Add(new DataGridTemplateColumn
        {
            Header = "Value", Width = 330, CellTemplateSelector = new ValueTemplates(false), CellEditingTemplateSelector = new ValueTemplates(true),
        });
        grid.Columns.Add(new DataGridTextColumn { Header = "Meaning", Binding = new Binding(nameof(FieldRow.Meaning)), IsReadOnly = true, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        return grid;
    }

    private string Describe(string field, int value) => field switch
    {
        "technique" => value < _fields.TechniqueCount ? _fields.TechniqueName(value) : "",
        "speciality" => value < _fields.SpecialityCount ? _fields.SpecialityName(value) : "",
        "leg_type" => value is >= 100 and <= 106 ? _fields.LegTypeName(value) : "",
        "chain_reaction" => value != 0 ? "Chain Reaction (◎)" : "",
        "female" => value != 0 ? "female" : "male",
        // byte 11: the shop shows byte x 100 in cents (28 -> 28.00); selling pays half (shop_list.c: total /= 2)
        "buy_price" or "u11" => $"buy {value}.00   sell_price {value / 2}.{(value % 2 == 0 ? "00" : "50")}",
        _ => "",
    };

    private void Changed(DecompBot bot)
    {
        bot.Dirty = true;
        RefreshList();
    }

    private void OnNameEdited(object sender, RoutedEventArgs e)
    {
        if (_current is { } bot && NameBox.Text != bot.Name)
        {
            bot.Name = NameBox.Text;
            RefreshList();
        }
    }

    // ---------- save / build ----------

    private int SaveAll()
    {
        var saved = 0;
        foreach (var bot in _store?.Bots.Where(b => b.Dirty).ToList() ?? [])
        {
            bot.Save();
            saved++;
        }

        RefreshList();
        return saved;
    }

    /// <summary>Saves the changed bots (the Save button's work). False if saving failed.</summary>
    public bool SaveAllChanges()
    {
        try
        {
            SaveAll();
            return true;
        }
        catch (Exception ex)
        {
            Log("save failed: " + ex.Message);
            return false;
        }
    }

    private void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var n = SaveAll();
            Log($"saved {n} file(s)");
        }
        catch (Exception ex)
        {
            Log("save failed: " + ex.Message);
        }
    }

    private async void OnBuildClicked(object sender, RoutedEventArgs e)
    {
        if (_store is null || _building)
        {
            return;
        }

        try
        {
            var n = SaveAll();
            if (n > 0)
            {
                Log($"saved {n} file(s) before building");
            }
        }
        catch (Exception ex)
        {
            Log("save failed, build cancelled: " + ex.Message);
            return;
        }

        _building = true;
        BuildButton.IsEnabled = false;
        try
        {
            var (exitCode, built) = await DecompBuild.RunAsync(_store.Root, PythonBox.Text.Trim(), Log);
            // back on the UI thread explicitly (the continuation of the await may run elsewhere)
            await Dispatcher.InvokeAsync(async () =>
            {
                Log(exitCode == 0 ? "build finished" : $"build FAILED (exit {exitCode})");
                if (built is not null && _openRom is not null &&
                    MessageBox.Show(Host, $"ROM built:{Environment.NewLine}{built}{Environment.NewLine}{Environment.NewLine}Open it in the editor?", "Build ROM",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    await _openRom(built);
                }
            }).Task.Unwrap();
        }
        catch (Exception ex)
        {
            Log("build error: " + ex.Message);
        }
        finally
        {
            Dispatcher.Invoke(() =>
            {
                _building = false;
                BuildButton.IsEnabled = true;
            });
        }
    }

    // ---------- art (own pictures of a data/bots bot: tools/export_template.py, tools/check_template.py) ----------

    private static bool OwnArtPack(DecompBot bot) => bot.Pack is "m9" or "shingata" or "binario";

    private string ArtFolderOf(DecompBot bot) =>
        Path.Combine("art", "template_" + Path.GetFileNameWithoutExtension(bot.FilePath)[4..]).Replace('\\', '/');

    // "art" of the bot without the cleaned-copy suffix: the folder the user draws in.
    private static string? ArtFolderInJson(DecompBot bot)
    {
        var art = bot.Json["art"]?.GetValue<string>();
        return art is null ? null : art.EndsWith("/_limpo", StringComparison.Ordinal) ? art[..^7] : art;
    }

    private UIElement ArtEditor(DecompBot bot)
    {
        var panel = new DockPanel { Margin = new Thickness(6) };
        var top = new StackPanel();
        DockPanel.SetDock(top, Dock.Top);
        panel.Children.Add(top);
        // every bot takes own pictures (Navi too: tools/decomp_navi.py reads "art"); Start from template only for data/bots
        var own = true;
        var folder = ArtFolderInJson(bot);
        var from = bot.Json["graphics_from"]?.GetValue<int>() ?? 43;
        var buildRom = _store is null ? null : PoseViewerWindow.NewestBuild(_store.Root);
        // pieces as the newest build draws them (tools/export_template.py on that ROM; cache in build/scratch/art_cache)
        var cache = buildRom is null || bot.Id is not { } bid || _store is null ? null
            : Path.Combine(_store.Root, "build", "scratch", "art_cache",
                $"{Path.GetFileName(Path.GetDirectoryName(buildRom))}_{bid:D3}");
        var info = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) };
        info.Text = folder is not null
            ? $"Own pictures: {folder}. The build uses the cleaned copy ({folder}/_limpo)."
            : cache is null
                ? "Not in the newest build (no id): build the ROM with this bot in the list to see its pictures."
                : $"Pictures of the build {Path.GetFileName(Path.GetDirectoryName(buildRom))} (as the game draws the bot). " +
                  $"Import on a piece makes own pictures for this bot in {ArtFolderOf(bot)} (the other pieces stay as they are).";
        top.Children.Add(CardTitle("Pieces (large parts and mini)"));
        top.Children.Add(info);

        var buttons = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        var start = new Button { Content = "Start from template", Padding = new Thickness(10, 2, 10, 2), Margin = new Thickness(0, 0, 6, 0),
            IsEnabled = OwnArtPack(bot),
            ToolTip = $"Exports bot {from}'s parts (large pictures and mini) as indexed 16-colour PNGs into {ArtFolderOf(bot)}" };
        var open = new Button { Content = "Open folder", Padding = new Thickness(10, 2, 10, 2), Margin = new Thickness(0, 0, 6, 0),
            IsEnabled = folder is not null || cache is not null };
        var method = new ComboBox { Width = 110, ItemsSource = ArtMethods, SelectedIndex = 0, Margin = new Thickness(12, 0, 6, 0),
            ToolTip = "Colour reduction when a part has more than 15 colours: auto = liq (libimagequant, closest to the drawing); " +
                      "fusao = merge the least used colours; maxcov / mediancut = other Pillow quantizers" };
        var check = new Button { Content = "Check and clean", Padding = new Thickness(10, 2, 10, 2), Margin = new Thickness(0, 0, 6, 0),
            IsEnabled = folder is not null,
            ToolTip = "Checks sizes and colours (tools/check_template.py) and writes the cleaned copy _limpo that the build uses" };
        var remove = new Button { Content = "Use bot's pictures again", Padding = new Thickness(10, 2, 10, 2), Margin = new Thickness(0, 0, 6, 0),
            IsEnabled = folder is not null, ToolTip = "Takes \"art\" out of the JSON (the folder stays on disk)" };
        var exportAll = new Button { Content = "Export all...", Padding = new Thickness(10, 2, 10, 2),
            IsEnabled = folder is not null || cache is not null, ToolTip = "Copies every piece PNG to a folder you pick, to draw over them elsewhere" };
        buttons.Children.Add(start);
        buttons.Children.Add(open);
        buttons.Children.Add(new TextBlock { Text = "Colours:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) });
        buttons.Children.Add(method);
        buttons.Children.Add(check);
        buttons.Children.Add(remove);
        buttons.Children.Add(exportAll);
        top.Children.Add(buttons);
        top.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6),
            Foreground = (Brush)FindResource("Theme.TextMuted"),
            Text = "Rules: keep each PNG's file name; index 0 / transparent = background. A piece drawn at another GBA sprite size " +
                   "(8..64, e.g. 64x64) gets an own mold of that size in the build (pieces with poses keep the mold's size); " +
                   "place it with the Skeleton Editor. Each large part " +
                   "(all its pieces together) and the mini (its 8 frames together) get 15 colours. Paint with a hard brush at 100% " +
                   "opacity (or the image in indexed mode): soft brushes and colourize tools make one tone per pixel, and the " +
                   "reduction has to merge them.",
        });

        // one card per piece PNG of the drawing folder: preview (cleaned copy when there is one), Import replaces just that
        // piece, Export saves it to draw over elsewhere
        var pieces = new WrapPanel();
        panel.Children.Add(new ScrollViewer { Content = pieces, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        static BitmapImage? LoadPng(string path)
        {
            if (!File.Exists(path))
                return null;
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;            // no file lock: Import overwrites the file
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.UriSource = new Uri(path);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        // folder the cards come from: the bot's own drawing folder, else the pictures of the newest build
        string? SourceDir() => _store is null ? null
            : own && ArtFolderInJson(bot) is { } f ? Path.Combine(_store.Root, f) : cache;
        string[] PieceFiles(string dir) => Directory.Exists(dir)
            ? Directory.GetFiles(dir, "*.png").Where(x => !Path.GetFileName(x).StartsWith('_'))
                .OrderBy(x => Path.GetFileName(x).StartsWith("mini", StringComparison.Ordinal) ? 1 : 0)
                .ThenBy(x => x, StringComparer.Ordinal).ToArray()
            : [];
        var exporting = false;
        async void ShowSheet()
        {
            pieces.Children.Clear();
            if (SourceDir() is not { } dir || _store is null)
                return;
            var fromBuild = dir == cache;
            if (fromBuild && !File.Exists(Path.Combine(dir, "_folha.png")))
            {
                if (exporting || buildRom is null || bot.Id is not { } id)
                    return;
                exporting = true;
                pieces.Children.Add(new TextBlock { Text = "Reading the pictures from the build...", Margin = new Thickness(0, 6, 0, 0) });
                var code = await RunPython("export_template.py", id.ToString(), buildRom, dir);
                exporting = false;
                pieces.Children.Clear();
                if (code != 0 || !File.Exists(Path.Combine(dir, "_folha.png")))
                {
                    pieces.Children.Add(new TextBlock { Text = "Could not read the pictures from the build (see the log)." });
                    return;
                }
            }
            foreach (var file in PieceFiles(dir))
            {
                var name = Path.GetFileName(file);
                var shown = (fromBuild ? null : LoadPng(Path.Combine(dir, "_limpo", name))) ?? LoadPng(file);
                pieces.Children.Add(PieceCard(Path.GetFileNameWithoutExtension(name), name, shown, file,
                    own ? $"Replaces only {name} with a PNG you pick, then runs Check and clean"
                        : "Own pictures for Medarot Navi bots are not supported by the build yet",
                    () => ImportPiece(file, shown), own));
            }
            // Medachange on: the form's two pictures (one picture on the legs, the 32x24 field mini) are pieces too
            if (bot.Json["medachange"] is JsonObject mc)
            {
                foreach (var (key, kind, title) in new[] { ("picture", "picture", "form (picture)"), ("mini", "mini", "form (mini 32x24)") })
                {
                    var rel = mc[key]?.GetValue<string>();
                    var file = rel is null ? null : Path.Combine(_store.Root, rel);
                    pieces.Children.Add(PieceCard(title, $"form_{key}.png", file is null ? null : LoadPng(file), file,
                        kind == "mini" ? "32x24 field sprite of the form (top 8x8 corners stay empty), reduced to 15 colours"
                                       : "The bot in the form: one picture up to 64x64 on the legs (Medachange tab: Legs from bot), 15 colours",
                        async () => { if (await ImportFormPicture(bot, mc, key, kind)) ShowSheet(); }));
                }
            }
        }
        Border PieceCard(string title, string name, BitmapImage? shown, string? file, string importTip, Func<Task> onImport,
            bool canImport = true)
        {
                var card = new StackPanel { Width = 150 };
                card.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = name });
                var img = new Image { Source = shown, Height = 128, Width = 128, Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.Both, Margin = new Thickness(0, 4, 0, 4), HorizontalAlignment = HorizontalAlignment.Left };
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
                card.Children.Add(img);
                card.Children.Add(new TextBlock { Text = shown is null ? "" : $"{shown.PixelWidth}x{shown.PixelHeight}",
                    Foreground = (Brush)FindResource("Theme.TextMuted") });
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
                var imp = new Button { Content = "Import...", Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(0, 0, 6, 0),
                    ToolTip = importTip, IsEnabled = canImport };
                ToolTipService.SetShowOnDisabled(imp, true);
                var exp = new Button { Content = "Export...", Padding = new Thickness(8, 2, 8, 2), ToolTip = $"Saves {name} to draw over it",
                    IsEnabled = file is not null && File.Exists(file) };
                System.Windows.Automation.AutomationProperties.SetName(imp, $"Import {name}");
                System.Windows.Automation.AutomationProperties.SetName(exp, $"Export {name}");
                row.Children.Add(imp);
                row.Children.Add(exp);
                card.Children.Add(row);
                imp.Click += async (_, _) => await onImport();
                exp.Click += (_, _) =>
                {
                    var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PNG image (*.png)|*.png", FileName = name, Title = $"Export {name}" };
                    if (file is not null && dlg.ShowDialog(Host) == true)
                    {
                        File.Copy(file, dlg.FileName, true);
                        Log($"{bot.Key}: {name} exported to {dlg.FileName}");
                    }
                };
                return new Border { Child = card, Padding = new Thickness(8), Margin = new Thickness(0, 0, 8, 8),
                    CornerRadius = new CornerRadius(4), BorderThickness = new Thickness(1), BorderBrush = (Brush)FindResource("Theme.TextMuted") };
        }
        async Task ImportPiece(string file, BitmapImage? current)
        {
            var name = Path.GetFileName(file);
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "PNG image (*.png)|*.png", Title = $"Import {name}" };
            if (dlg.ShowDialog(Host) != true)
                return;
            var picked = LoadPng(dlg.FileName);
            if (picked is null)
                return;
            if (current is not null && (picked.PixelWidth != current.PixelWidth || picked.PixelHeight != current.PixelHeight) &&
                MessageBox.Show(Host, $"{name} is {current.PixelWidth}x{current.PixelHeight}; the picked image is " +
                    $"{picked.PixelWidth}x{picked.PixelHeight}.\n\nA large piece at another GBA sprite size (8..64) gets an own mold " +
                    "of that size (place it with the Skeleton Editor). Use it anyway?", "Import", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            var started = false;
            if (_store is not null && ArtFolderInJson(bot) is null && Path.GetDirectoryName(file) == cache)
            {
                started = true;
                // first own drawing of this bot: the build's pictures become its folder (art/template_<bot>), then this piece
                var rel = ArtFolderOf(bot);
                var full = Path.Combine(_store.Root, rel);
                if (Directory.Exists(full) && Directory.EnumerateFileSystemEntries(full).Any())
                {
                    MessageBox.Show(Host, $"{rel} already has files: not overwriting a drawing. Point the bot at it with Open folder / " +
                        "Check and clean, or delete it first.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                Directory.CreateDirectory(full);
                foreach (var x in Directory.GetFiles(cache!))
                    File.Copy(x, Path.Combine(full, Path.GetFileName(x)));
                bot.Json["art"] = rel + "/_limpo";
                Changed(bot);
                file = Path.Combine(full, name);
                Log($"{bot.Key}: own pictures started in {rel} (copy of the build's)");
            }
            File.Copy(dlg.FileName, file, true);
            Log($"{bot.Key}: {name} replaced by {dlg.FileName}");
            await RunCheck();
            if (started)
            {
                OnBotSelected(this, null!);     // own folder now: rebuild the tab (texts, Check and clean, Use bot's pictures)
                SelectTab("Art");
            }
            else
                ShowSheet();
        }
        ShowSheet();

        start.Click += async (_, _) =>
        {
            if (_store is null)
                return;
            var rel = ArtFolderOf(bot);
            var full = Path.Combine(_store.Root, rel);
            if (Directory.Exists(full) && Directory.EnumerateFileSystemEntries(full).Any())
            {
                MessageBox.Show(Host, $"{rel} already has files: not overwriting a drawing. Use Open folder, or delete it first.",
                    "Art", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (!HasBuiltRom())
            {
                MessageBox.Show(Host, "tools/export_template.py reads the pictures from a ROM built by this project. Press Build ROM first.", "Art",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (await RunPython("export_template.py", from.ToString(), "auto", rel) != 0)
                return;
            bot.Json["art"] = rel + "/_limpo";
            Changed(bot);
            OnBotSelected(this, null!);
            SelectTab("Art");
            Log($"{bot.Key}: draw over the PNGs in {rel}, then Check and clean");
        };
        open.Click += (_, _) =>
        {
            if (SourceDir() is { } dir && Directory.Exists(dir))
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{Path.GetFullPath(dir)}\"") { UseShellExecute = true });
        };
        check.Click += async (_, _) => await RunCheck();
        exportAll.Click += (_, _) =>
        {
            if (SourceDir() is not { } dir)
                return;
            var dlg = new Microsoft.Win32.OpenFolderDialog { Title = "Export the pieces to" };
            if (dlg.ShowDialog(Host) != true)
                return;
            var files = PieceFiles(dir);
            foreach (var x in files)
                File.Copy(x, Path.Combine(dlg.FolderName, Path.GetFileName(x)), true);
            Log($"{bot.Key}: {files.Length} pieces exported to {dlg.FolderName} (Import each one back when drawn)");
        };
        async Task RunCheck()
        {
            if (ArtFolderInJson(bot) is not { } f)
                return;
            var lines = new List<string>();
            var code = await RunPython("check_template.py", lines, f, "--limpar", $"--metodo={method.SelectedItem}");
            ShowSheet();
            var sizes = lines.Where(l => l.Contains("tamanho", StringComparison.Ordinal)).ToList();
            if (code != 0 || sizes.Count > 0)
                MessageBox.Show(Host, (code != 0 ? "check_template.py failed (see the log).\n" : "") + string.Join("\n", sizes) +
                    (sizes.Count > 0 ? "\n\nThe build needs each PNG at the mold's size." : ""), "Check and clean",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                bot.Json["art"] = f.Replace('\\', '/') + "/_limpo";
                Changed(bot);
                Log($"{bot.Key}: cleaned copy ready ({f}/_limpo); Save changes, then Build ROM");
            }
        }
        remove.Click += (_, _) =>
        {
            bot.Json.Remove("art");
            Changed(bot);
            OnBotSelected(this, null!);
            SelectTab("Art");
        };
        return panel;
    }

    // A first form for a data/bots bot: legs from its legs, Drives A/B/C from its head and arms (edit them after).
    private static JsonObject DefaultForm(DecompBot bot)
    {
        var legs = (JsonObject)bot.Part("legs")["stats"]!;
        int L(string k, int def = 0) => legs[k]?.GetValue<int>() ?? def;
        JsonObject Drive(string slot)
        {
            var st = (JsonObject)bot.Part(slot)["stats"]!;
            int S(string k) => st[k]?.GetValue<int>() ?? 0;
            return new JsonObject
            {
                ["technique"] = S("technique"), ["medal_compat"] = S("medal_compat"), ["speciality"] = S("speciality"),
                ["success"] = S("success"), ["power"] = S("power"), ["effect"] = "none",
                ["chain_reaction"] = S("chain_reaction"),
            };
        }
        return new JsonObject
        {
            ["legs"] = new JsonObject
            {
                ["leg_type"] = L("leg_type", 103), ["propulsion"] = L("propulsion"), ["evasion"] = L("evasion"),
                ["defense"] = L("defense"), ["proximity"] = L("proximity"), ["remoteness"] = L("remoteness"),
            },
            ["drives"] = new JsonObject { ["A"] = Drive("head"), ["B"] = Drive("right_arm"), ["C"] = Drive("left_arm") },
            ["legs_from"] = 89,
        };
    }

    // Pictures of the form of a data/bots bot: the big one (one piece on the legs, an own mold) and the 32x24 mini.
    private UIElement FormPictures(DecompBot bot, JsonObject mc)
    {
        var box = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        box.Children.Add(new TextBlock { Text = "Form pictures", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
        box.Children.Add(new TextBlock
        {
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6), Foreground = (Brush)FindResource("Theme.TextMuted"),
            Text = "In the form the bot is one picture on the legs (head and arms empty). Picture: any GBA sprite size up to " +
                   "64x64, reduced to 15 colours. Legs from: a game bot whose one-piece legs the picture hangs on (89 Tankar = " +
                   "mold 30, 64x64). Mini: 32x24 (the field sprite in the form), top 8x8 corners stay empty.",
        });
        var legsRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
        legsRow.Children.Add(new TextBlock { Text = "Legs from bot:", VerticalAlignment = VerticalAlignment.Center, Width = 110 });
        var legsFrom = new TextBox { Width = 100, Text = (mc["legs_from"]?.GetValue<int>() ?? 89).ToString() };
        legsFrom.LostFocus += (_, _) =>
        {
            if (int.TryParse(legsFrom.Text.Trim(), out var v) && v is >= 0 and < 120 && v != (mc["legs_from"]?.GetValue<int>() ?? 89))
            {
                mc["legs_from"] = v;
                Changed(bot);
            }
            legsFrom.Text = (mc["legs_from"]?.GetValue<int>() ?? 89).ToString();
        };
        legsRow.Children.Add(legsFrom);
        box.Children.Add(legsRow);
        box.Children.Add(new TextBlock { Text = "The form's picture and mini are in the Art tab (with the parts' pieces).",
            Foreground = (Brush)FindResource("Theme.TextMuted") });
        return box;
    }

    // Picks a PNG for the form's picture or mini (tools/form_art.py reduces and checks it) and points the JSON at it.
    private async Task<bool> ImportFormPicture(DecompBot bot, JsonObject mc, string key, string kind)
    {
        if (_store is null)
            return false;
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "PNG image (*.png)|*.png", Title = $"Form {key}" };
        if (dlg.ShowDialog(Host) != true)
            return false;
        var rel = $"{ArtFolderOf(bot)}/forma/{key}.png";
        var lines = new List<string>();
        if (await RunPython("form_art.py", lines, kind, dlg.FileName, rel) != 0 || !lines.Any(l => l.StartsWith("ok ")))
        {
            MessageBox.Show(Host, string.Join("\n", lines.Where(l => !l.StartsWith(">"))), $"Form {key}",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        mc[key] = rel;
        Changed(bot);
        return true;
    }

    private void SelectTab(string header)
    {
        foreach (TabItem t in PartTabs.Items)
            if ((t.Header as string) == header)
                PartTabs.SelectedItem = t;
    }

    private Task<int> RunPython(string tool, params string[] args) => RunPython(tool, null, args);

    // Runs tools/<tool> with the project's Python in the project folder, output to the log (and to `lines`).
    private async Task<int> RunPython(string tool, List<string>? lines, params string[] args)
    {
        if (_store is null)
            return -1;
        try
        {
            var (exe, prefixArgs) = DecompBuild.SplitCommand(PythonBox.Text.Trim());
            var psi = new ProcessStartInfo(exe)
            {
                WorkingDirectory = _store.Root, RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false, CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            };
            foreach (var a in prefixArgs)
                psi.ArgumentList.Add(a);
            psi.ArgumentList.Add(Path.Combine(_store.Root, "tools", tool));
            foreach (var a in args)
                psi.ArgumentList.Add(a);
            psi.Environment["PYTHONIOENCODING"] = "utf-8";
            Log($"> {exe} tools/{tool} {string.Join(' ', args)}");
            using var process = Process.Start(psi) ?? throw new InvalidOperationException("could not start Python");
            void Line(string? s)
            {
                if (s is null)
                    return;
                lock (psi)
                    lines?.Add(s);
                Log(s);
            }
            process.OutputDataReceived += (_, a) => Line(a.Data);
            process.ErrorDataReceived += (_, a) => Line(a.Data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Log($"tools/{tool}: {ex.Message}");
            return -1;
        }
    }

    // ---------- new bot (data/bots, pack binario) ----------

    private void OnNewBotClicked(object sender, RoutedEventArgs e)
    {
        if (_store is null)
            return;
        var dlg = new Window
        {
            Title = "New bot", Width = 420, SizeToContent = SizeToContent.Height, Owner = Host,
            WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize,
            Background = (Brush)FindResource("Theme.WindowBackground"), Foreground = (Brush)FindResource("Theme.TextPrimary"),
        };
        var grid = new StackPanel { Margin = new Thickness(12) };
        var name = new TextBox { Margin = new Thickness(0, 2, 0, 8), MaxLength = 13 };
        var baseBox = new ComboBox { Margin = new Thickness(0, 2, 0, 8), IsEditable = false };
        var core = Directory.GetFiles(Path.Combine(_store.Root, "data", "bots"), "*.json").Order(StringComparer.Ordinal)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Where(s => s.Length > 4 && int.TryParse(s[..3], out var id) && id < 120).ToList();
        baseBox.ItemsSource = core;
        baseBox.SelectedItem = core.FirstOrDefault(s => s.StartsWith("043_", StringComparison.Ordinal)) ?? core.FirstOrDefault();
        var inList = new CheckBox { Content = "Add to data/expansion_bots.txt", IsChecked = true, Margin = new Thickness(0, 0, 0, 10) };
        grid.Children.Add(new TextBlock { Text = "Name (up to 13 letters):" });
        grid.Children.Add(name);
        grid.Children.Add(new TextBlock { Text = "Start from (stats and pictures of):" });
        grid.Children.Add(baseBox);
        grid.Children.Add(inList);
        var ok = new Button { Content = "Create", Padding = new Thickness(12, 3, 12, 3), IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
        ok.Click += (_, _) => dlg.DialogResult = true;
        grid.Children.Add(ok);
        dlg.Content = grid;
        if (dlg.ShowDialog() != true || name.Text.Trim().Length == 0 || baseBox.SelectedItem is not string baseStem)
            return;

        var slug = System.Text.RegularExpressions.Regex.Replace(name.Text.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
        var dir = Path.Combine(_store.Root, "data", "bots");
        var next = Directory.GetFiles(dir, "*.json").Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(s => int.TryParse(s[..3], out var id) ? id : 0).DefaultIfEmpty(181).Max() + 1;
        if (_store.Bots.Any(b => b.Key.EndsWith("/" + slug, StringComparison.Ordinal)))
        {
            MessageBox.Show(Host, $"There is already a bot called {slug}.", "New bot", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var src = JsonNode.Parse(File.ReadAllText(Path.Combine(dir, baseStem + ".json"))) as JsonObject ?? [];
        var baseId = int.Parse(baseStem[..3]);
        var json = new JsonObject
        {
            ["bot_id"] = next,
            ["name"] = name.Text.Trim().ToUpperInvariant(),
            ["notes"] = $"Made in the Decomp Project window from {baseStem} (stats and pictures); edit the parts and the Art tab.",
            ["graphics_from"] = baseId,
            ["parts"] = src["parts"]?.DeepClone(),
        };
        var path = Path.Combine(dir, $"{next:000}_{slug}.json");
        File.WriteAllText(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }).Replace("\r\n", "\n") + "\n",
            new UTF8Encoding(false));
        if (inList.IsChecked == true)
            File.AppendAllText(Path.Combine(_store.Root, DecompBotStore.BotListFile), $"binario/{slug}\n");
        Log($"new bot {Path.GetFileName(path)} (binario/{slug}){(inList.IsChecked == true ? ", added to the bot list" : "")}");
        LoadProject();
        BotList.SelectedItem = _store?.Bots.FirstOrDefault(b => b.FilePath == path);
    }

    private void Log(string line)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => Log(line));
            return;
        }

        if (_hostedContent is not null)
        {
            LogSink?.Invoke(line);
            return;
        }

        LogBox.AppendText(line + Environment.NewLine);
        LogBox.ScrollToEnd();
    }

    // ---------- buttons / window ----------

    private void OnBrowseClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Medabots project folder (with data/expansion_bots.txt)" };
        if (dialog.ShowDialog(Host) == true)
        {
            ProjectPathBox.Text = dialog.FolderName;
            LoadProject();
        }
    }

    private void OnReloadClicked(object sender, RoutedEventArgs e)
    {
        if (_store?.Bots.Any(b => b.Dirty) == true &&
            MessageBox.Show(Host, "Discard unsaved changes and reload?", "Reload", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        LoadProject();
    }

    private void OnFilterChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();

    private void OnMoveEditorClicked(object sender, RoutedEventArgs e)
    {
        if (OpenSubtab is { } openSubtab)
        {
            openSubtab("Moves");
            return;
        }

        if (_store is null)
        {
            StatusText.Text = "Open a project first.";
            return;
        }
        new MoveEditorWindow(_store.Root) { Owner = Host }.Show();
    }

    private void OnSkeletonEditorClicked(object sender, RoutedEventArgs e)
    {
        if (OpenSubtab is { } openSubtab)
        {
            openSubtab("Skeleton");
            return;
        }

        if (_store is null)
        {
            StatusText.Text = "Open a project first.";
            return;
        }
        new SkeletonEditorWindow(_store.Root) { Owner = Host }.Show();
    }

    private void OnPoseViewerClicked(object sender, RoutedEventArgs e)
    {
        if (OpenSubtab is { } openSubtab)
        {
            openSubtab("Poses");
            return;
        }

        if (_store is null)
        {
            StatusText.Text = "Open a project first.";
            return;
        }
        new PoseViewerWindow(_store.Root) { Owner = Host }.Show();
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_store?.Bots.Any(b => b.Dirty) != true)
        {
            return;
        }

        var answer = MessageBox.Show(Host, "Save the changed bots before closing?", "Decomp Project",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if (answer == MessageBoxResult.Cancel)
        {
            e.Cancel = true;
        }
        else if (answer == MessageBoxResult.Yes)
        {
            SaveAll();
        }
    }

    // ---------- settings ----------

    private sealed record Settings(string? Root, string? Python);

    /// <summary>The project folder saved in the settings (or null), used by the Skeleton / Moves / Poses subtabs.</summary>
    internal static string? SavedProjectRoot() => LoadSettings().Root;

    /// <summary>The Python command saved in the settings ("python" when none).</summary>
    internal static string SavedPython()
    {
        var python = LoadSettings().Python;
        return string.IsNullOrWhiteSpace(python) ? "python" : python.Trim();
    }

    /// <summary>Remembers the project folder (the Python command is kept); the main window's Open Decomp Project uses it.</summary>
    internal static void SaveProjectRoot(string root)
    {
        var python = LoadSettings().Python;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new Settings(root, python)));
        }
        catch
        {
            // settings are a convenience only
        }
    }

    private static Settings LoadSettings()
    {
        try
        {
            return File.Exists(SettingsPath)
                ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath)) ?? new Settings(null, null)
                : new Settings(null, null);
        }
        catch
        {
            return new Settings(null, null);
        }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new Settings(ProjectPathBox.Text.Trim(), PythonBox.Text.Trim())));
        }
        catch
        {
            // settings are a convenience only
        }
    }

    // ---------- rows ----------

    private sealed class FieldRow(JsonObject owner, string fieldName, Action changed, Func<string, int, string> describe,
        Func<string, IReadOnlyList<Choice>?> choices)
    {
        public string Field => fieldName == "female" ? "gender" : fieldName; // JSON key stays "female" (0 male, 1 female)

        private readonly IReadOnlyList<Choice>? _choices = choices(fieldName);

        // the list, plus the stored value when it is not one of them (so the dropdown never shows blank)
        public IReadOnlyList<Choice>? Choices => _choices is null || _choices.Any(c => c.Id == Value)
            ? _choices
            : [.. _choices, new Choice(Value, $"{Value:000}  (?)")];

        public int Value
        {
            get => owner[fieldName]?.GetValue<int>() ?? 0;
            set
            {
                if (value is < 0 or > 255)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "0-255");
                }

                if (value != Value)
                {
                    owner[fieldName] = value;
                    changed();
                }
            }
        }

        public string Meaning => Choices is null ? describe(fieldName, Value) : "";
    }

    private sealed class DriveRow(JsonObject drive, string name, Action changed)
    {
        public string Drive => name;

        public int Technique { get => Get("technique"); set => Set("technique", value); }

        // Class (medal_compat) of the part while in the form; 255 = key absent = the part's own class
        public int Medal
        {
            get => drive["medal_compat"]?.GetValue<int>() ?? 255;
            set
            {
                if (value == Medal)
                    return;
                if (value == 255)
                    drive.Remove("medal_compat");
                else
                    drive["medal_compat"] = value;
                changed();
            }
        }

        public int Speciality { get => Get("speciality"); set => Set("speciality", value, 7); }

        public int Success { get => Get("success"); set => Set("success", value); }

        public int Power { get => Get("power"); set => Set("power", value); }

        public string Effect
        {
            get => drive["effect"]?.GetValue<string>() ?? "none";
            set
            {
                if (value != Effect)
                {
                    drive["effect"] = value;
                    changed();
                }
            }
        }

        // Chain Reaction (◎): null = key absent = the Navi's own value; true/false override it
        public bool? Chain
        {
            get => drive["chain_reaction"] is JsonValue v
                ? (v.TryGetValue<bool>(out var b) ? b : v.TryGetValue<int>(out var n) ? n != 0 : null)
                : null;
            set
            {
                if (value == Chain)
                    return;
                if (value is { } v)
                    drive["chain_reaction"] = v;
                else
                    drive.Remove("chain_reaction");
                changed();
            }
        }

        private int Get(string key) => drive[key]?.GetValue<int>() ?? 0;

        private void Set(string key, int value, int max = 255)
        {
            if (value < 0 || value > max)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"0-{max}");
            }

            if (value != Get(key))
            {
                drive[key] = value;
                changed();
            }
        }
    }
}
