using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace KeyTone {
    static class Program {
        [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern IntPtr FindWindow(string cls, string title);
        [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h, int msg, IntPtr w, IntPtr l);
        [STAThread] static int Main(string[] args) {
            if (args.Contains("--self-test")) return SelfTest.Run(args.Last());
            string data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyTone");
            int dataArg = Array.IndexOf(args, "--data-dir"); if (dataArg >= 0 && dataArg + 1 < args.Length) data = Path.GetFullPath(args[dataArg+1]);
            bool first; using (var mutex = new Mutex(true, "Local\\KeyTone.Desktop.v1" + (dataArg >= 0 ? ".test" : ""), out first)) {
                if (!first) { PostMessage(FindWindow(null, "KeyTone"), 0x8001, IntPtr.Zero, IntPtr.Zero); return 0; }
                try {
                    var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                    application.DispatcherUnhandledException += delegate(object sender, DispatcherUnhandledExceptionEventArgs e) { MessageBox.Show(e.Exception.Message, "KeyTone", MessageBoxButton.OK, MessageBoxImage.Error); e.Handled = true; };
                    var controller = new Studio(data, args.Contains("--minimized"));
                    application.Run(); GC.KeepAlive(controller); return 0;
                } catch (Exception e) { Directory.CreateDirectory(data); File.WriteAllText(Path.Combine(data, "startup-error.txt"), e.ToString()); MessageBox.Show("KeyTone could not start: " + e.Message, "KeyTone"); return 1; }
                finally { mutex.ReleaseMutex(); }
            }
        }
    }
    public sealed class Studio {
        public readonly Window Window;
        Settings settings; Profile Current { get { return settings.Profiles[settings.Active]; } }
        string dataDir, settingsFile; bool loading, quitting, capture; int selected = 57;
        AudioEngine audio; KeyboardInput input; Forms.NotifyIcon tray;
        readonly Dictionary<int, Button> keys = new Dictionary<int, Button>(); readonly Dictionary<int,string> labels = new Dictionary<int,string>();
        readonly Dictionary<string, Button> presetButtons = new Dictionary<string,Button>();
        readonly Dictionary<string, float[]> cache = new Dictionary<string,float[]>(); Random random = new Random();
        DispatcherTimer saveTimer, healthTimer; DateTime statusUntil; bool recoveryPending;
        Profile switchUndo, switchUndoTarget; int switchUndoIndex = -1;
        T C<T>(string name) where T:class { return Window.FindName(name) as T; }
        static Brush Color(string value) { return (Brush)new BrushConverter().ConvertFromString(value); }
        public Studio(string directory, bool minimized) {
            dataDir = directory; settingsFile = Path.Combine(directory, "settings.json"); string warning;
            settings = Store.Read(settingsFile, out warning);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Main.xaml")) Window = (Window)XamlReader.Load(stream);
            try { using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("KeyTone.ico")) Window.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(s); } catch { }
            BuildPresets(); BuildKeyboard();
            C<ComboBox>("KeyPreset").ItemsSource = new [] { "Default" }.Concat(SoundBank.Presets).Concat(new [] { "Silent" }).ToArray();
            C<Button>("EnableButton").Click += delegate { Toggle(); };
            C<Button>("TrayButton").Click += delegate { Hide(); };
            C<Button>("SettingsButton").Click += delegate { ShowSettings(); };
            C<Button>("CaptureButton").Click += delegate { capture = true; C<Button>("CaptureButton").Content = "Press any key…"; SetStatus("Press the physical key you want to customize. Escape cancels."); };
            C<Button>("PreviewKey").Click += delegate { Preview(selected); };
            C<Button>("AllBlue").Click += delegate { AllSwitches("Blue Switch"); };
            C<Button>("AllRed").Click += delegate { AllSwitches("Red Switch"); };
            C<Button>("AllBrown").Click += delegate { AllSwitches("Brown Switch"); };
            C<Button>("KeyBlue").Click += delegate { SetSwitch("Blue Switch"); };
            C<Button>("KeyRed").Click += delegate { SetSwitch("Red Switch"); };
            C<Button>("KeyBrown").Click += delegate { SetSwitch("Brown Switch"); };
            C<Button>("UndoSwitch").Click += delegate {
                if (switchUndo == null || switchUndoIndex != settings.Active) return;
                settings.Profiles[settings.Active] = switchUndo; switchUndo = null;
                C<Button>("UndoSwitch").IsEnabled = false; Changed(); LoadProfile(); SetStatus("Previous keyboard sounds and tuning restored.");
            };
            C<Button>("ClearTest").Click += delegate { C<TextBox>("TypingTest").Clear(); };
            C<Button>("ResetKey").Click += delegate { Current.Keys.Remove(selected.ToString()); Changed(); LoadKey(); PaintKeys(); SetStatus("Key restored to the keyboard preset."); };
            C<Button>("ImportPress").Click += delegate { Import(false); }; C<Button>("ImportRelease").Click += delegate { Import(true); };
            C<Slider>("MasterVolume").ValueChanged += delegate { if (!loading) { Current.Volume = C<Slider>("MasterVolume").Value; MasterLabels(); ScheduleSave(); } };
            foreach (string name in new [] { "Pitch", "Tone", "KeyVolume" }) C<Slider>(name).ValueChanged += delegate { EditKey(); };
            C<ComboBox>("KeyPreset").SelectionChanged += delegate { if (!loading && RecordedBank.IsSwitch(C<ComboBox>("KeyPreset").SelectedItem as string)) SetSwitch(C<ComboBox>("KeyPreset").SelectedItem as string); else EditKey(); };
            C<CheckBox>("ReleaseToggle").Click += delegate { if (!loading) { Current.Release = C<CheckBox>("ReleaseToggle").IsChecked == true; ScheduleSave(); } };
            C<CheckBox>("VariationToggle").Click += delegate { if (!loading) { Current.Variation = C<CheckBox>("VariationToggle").IsChecked == true; ScheduleSave(); } };
            saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) }; saveTimer.Tick += delegate { saveTimer.Stop(); Save(); };
            Window.SourceInitialized += delegate {
                input = new KeyboardInput((HwndSource)PresentationSource.FromVisual(Window)); input.Key = OnKey; input.Toggle = Toggle; input.Activate = Show; input.Repeat = Current.Repeat;
                input.DeviceChanged = delegate { recoveryPending = true; };
                if (!input.HotkeyAvailable) C<TextBlock>("ShortcutHint").Text = "Shortcut in use · use tray to mute";
                Microsoft.Win32.SystemEvents.PowerModeChanged += PowerChanged;
                OpenAudio();
            };
            Window.Closing += delegate(object s, System.ComponentModel.CancelEventArgs e) { if (!quitting) { e.Cancel = true; Hide(); } };
            CreateTray(); LoadProfile(); WarmCache();
            healthTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            healthTimer.Tick += delegate { if (recoveryPending || (audio != null && audio.Error != null)) { recoveryPending = false; OpenAudio(); } else if (DateTime.UtcNow > statusUntil) UpdateStatus(); };
            healthTimer.Start(); Window.Show(); if (minimized) Window.Hide(); if (warning != null) SetStatus(warning);
        }
        void PowerChanged(object sender, PowerModeChangedEventArgs e) { if (e.Mode == PowerModes.Resume) Window.Dispatcher.BeginInvoke(new Action(delegate { if (input != null) input.Reset(); OpenAudio(); })); }
        void BuildPresets() {
            string[] descriptions = { "Low, rounded & resonant", "Bright, sharp & clicky", "Gentle, textured feedback", "Clean, light & fluid", "Soft, cushioned taps", "Recorded MX Blue · clicky press & release", "Recorded MX Red · smooth linear clack", "Recorded MX Brown · tactile, mellow clack" };
            for (int i = 0; i < SoundBank.Presets.Length; i++) {
                string name = SoundBank.Presets[i]; var panel = new StackPanel();
                var bars = new StackPanel { Orientation = Orientation.Horizontal, Height = 25, Margin = new Thickness(0,0,0,12) };
                for (int j = 0; j < 12; j++) bars.Children.Add(new Border { Background = Color(i==7 ? "#DCB185" : i==5 ? "#85BFFF" : i==6 ? "#FFADB6" : i==0 ? "#8FE1C2" : i==1 ? "#F0C38B" : i==2 ? "#B5B2F2" : i==3 ? "#8ACBDD" : "#A7B6BA"), Width = 4, Height = 5 + ((j*7+i*3)%19), CornerRadius = new CornerRadius(2), Margin = new Thickness(0,0,4,0), VerticalAlignment = VerticalAlignment.Center });
                panel.Children.Add(bars); panel.Children.Add(new TextBlock { Text = name, FontSize = 16, FontWeight = FontWeights.SemiBold }); panel.Children.Add(new TextBlock { Text = descriptions[i], FontSize = 11, Foreground = Color("#B8C3D2"), Margin = new Thickness(0,5,0,0), TextWrapping = TextWrapping.Wrap });
                var button = new Button { Content = panel, Padding = new Thickness(15), Margin = new Thickness(0,0,10,0), HorizontalContentAlignment = HorizontalAlignment.Left, Background = Color("#191F28") };
                if (RecordedBank.IsSwitch(name)) { bars.Height = 12; bars.Margin = new Thickness(0,0,0,6); button.Padding = new Thickness(12,8,12,8); }
                AutomationProperties.SetName(button, name + " keyboard preset"); button.Click += delegate { Current.Preset = name; Changed(); PaintPresets(); PaintKeys(); LoadKey(); PreviewPreset(name); }; presetButtons[name] = button; C<System.Windows.Controls.Primitives.UniformGrid>(RecordedBank.IsSwitch(name) ? "SwitchPresets" : "Presets").Children.Add(button);
            }
        }
        void BuildKeyboard() {
            AddRow("Esc:1 F1:59 F2:60 F3:61 F4:62 F5:63 F6:64 F7:65 F8:66 F9:67 F10:68 F11:87 F12:88 Del:339", 1);
            AddRow("`:41 1:2 2:3 3:4 4:5 5:6 6:7 7:8 8:9 9:10 0:11 -:12 =:13 Back:14:1.7", 1);
            AddRow("Tab:15:1.5 Q:16 W:17 E:18 R:19 T:20 Y:21 U:22 I:23 O:24 P:25 [:26 ]:27 \\:43:1.2", 1);
            AddRow("Caps:58:1.8 A:30 S:31 D:32 F:33 G:34 H:35 J:36 K:37 L:38 ;:39 ':40 Enter:28:1.9", 1);
            AddRow("Shift:42:2.3 Z:44 X:45 C:46 V:47 B:48 N:49 M:50 ,:51 .:52 /:53 Shift:54:2.4", 1);
            AddRow("Ctrl:29:1.3 Win:347:1.2 Alt:56:1.2 Space:57:5.5 Alt:312:1.2 Ctrl:285:1.3 ←:331 ↑:328 ↓:336 →:333", 1);
        }
        void AddRow(string specification, double scale) {
            var grid = new Grid { Margin = new Thickness(0,0,0,6) }; int index = 0;
            foreach (string item in specification.Split(' ')) {
                string[] parts = item.Split(':'); int code = Int32.Parse(parts[1]); double width = parts.Length > 2 ? Double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture) : 1;
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });
                var button = new Button { Content = parts[0], Height = 36, MinHeight=0, Padding = new Thickness(2), Margin = new Thickness(0,0,5,0), FontSize = 12, Background = Color("#28313D"), Tag = code };
                labels[code] = parts[0]; keys[code] = button; AutomationProperties.SetName(button, parts[0] + " key"); button.ToolTip = "Customize " + parts[0];
                button.Click += delegate { selected = code; LoadKey(); PaintKeys(); }; Grid.SetColumn(button, index++); grid.Children.Add(button);
            }
            C<StackPanel>("Keyboard").Children.Add(grid);
        }
        void LoadProfile() {
            if (!Object.ReferenceEquals(Current,switchUndoTarget)) { switchUndo = null; C<Button>("UndoSwitch").IsEnabled = false; }
            loading = true; C<Slider>("MasterVolume").Value = Current.Volume; C<CheckBox>("ReleaseToggle").IsChecked = Current.Release; C<CheckBox>("VariationToggle").IsChecked = Current.Variation; C<TextBlock>("ProfileLabel").Text = Current.Name; loading = false;
            if (input != null) { input.Repeat = Current.Repeat; input.Reset(); } MasterLabels(); PaintPresets(); LoadKey(); PaintKeys(); UpdateToggle();
        }
        void MasterLabels() { C<TextBlock>("MasterValue").Text = Math.Round(Current.Volume*100) + "%"; }
        void AllSwitches(string preset) {
            switchUndo = Store.Json().Deserialize<Profile>(Store.Json().Serialize(Current)); switchUndoIndex = settings.Active; switchUndoTarget = Current;
            Current.ApplySwitchToAll(preset); Changed(); LoadProfile(); C<Button>("UndoSwitch").IsEnabled = true;
            PreviewPreset(preset); SetStatus("Every key now uses " + preset + ". Undo is available above.");
        }
        void SetSwitch(string preset) {
            if (loading) return; Current.SetKeySwitch(selected,preset); Changed(); LoadKey(); PaintKeys(); Preview(selected);
        }
        void PreviewPreset(string preset) {
            if (audio == null) { SetStatus("No audio output. Open settings and reconnect."); return; }
            // Preview the requested switch even if A has a per-key override.
            audio.Play(SoundBank.Generate(preset,0,false,1),(float)Current.Volume);
            if (Current.Release) {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(110) };
                timer.Tick += delegate { timer.Stop(); if (audio != null) audio.Play(SoundBank.Generate(preset,0,true,1),(float)Current.Volume); }; timer.Start();
            }
            SetStatus(preset + " is the default. Individual overrides are kept; use All keys to replace them.");
        }
        void PaintPresets() { foreach (var pair in presetButtons) { bool active = pair.Key == Current.Preset; pair.Value.BorderBrush = Color(active ? "#8FE1C2" : "#465368"); pair.Value.BorderThickness = new Thickness(active ? 2 : 1); pair.Value.Background = Color(active ? "#223C36" : "#191F28"); AutomationProperties.SetHelpText(pair.Value, active ? "Selected" : "Choose and preview this preset"); } }
        void PaintKeys() { foreach (var pair in keys) {
            string preset = Current.EffectivePreset(pair.Key); bool blue = preset == "Blue Switch", red = preset == "Red Switch", brown = preset == "Brown Switch";
            pair.Value.Content = labels[pair.Key] + (Current.Keys.ContainsKey(pair.Key.ToString()) ? " ·" : "");
            pair.Value.Background = Color(blue ? "#234E7A" : red ? "#793C46" : brown ? "#684D35" : pair.Key == selected ? "#8FE1C2" : "#28313D");
            pair.Value.Foreground = Color(pair.Key == selected && !blue && !red && !brown ? "#12221E" : "#F2F4F7");
            pair.Value.BorderBrush = Color(pair.Key == selected ? "#FFFFFF" : blue ? "#7FB5EF" : red ? "#E69FA7" : brown ? "#DCB185" : "#546176");
            pair.Value.BorderThickness = new Thickness(pair.Key == selected ? 2 : 1);
            pair.Value.ToolTip = labels[pair.Key] + " — " + preset; AutomationProperties.SetHelpText(pair.Value,preset);
        } }
        void LoadKey() {
            loading = true; KeySound k = Current.Get(selected); string label;
            C<TextBlock>("SelectedKey").Text = labels.TryGetValue(selected, out label) ? label : "Key " + selected;
            C<ComboBox>("KeyPreset").SelectedItem = k.Preset; C<Slider>("Pitch").Value = k.Pitch; C<Slider>("Tone").Value = k.Tone; C<Slider>("KeyVolume").Value = k.Volume;
            loading = false; KeyLabels(k);
        }
        void KeyLabels(KeySound k) {
            C<TextBlock>("PitchValue").Text = (k.Pitch > 0 ? "+" : "") + k.Pitch.ToString("0") + " st";
            C<TextBlock>("ToneValue").Text = Math.Abs(k.Tone) < .05 ? "Neutral" : k.Tone < 0 ? "Darker" : "Brighter";
            C<TextBlock>("KeyVolumeValue").Text = Math.Round(k.Volume*100) + "%";
            C<TextBlock>("KeyMode").Text = Current.EffectivePreset(selected) + (Current.Keys.ContainsKey(selected.ToString()) ? " · custom key" : " · keyboard default");
            C<TextBlock>("CustomLabel").Text = (String.IsNullOrEmpty(k.PressWav) ? "" : "Custom press loaded. ") + (String.IsNullOrEmpty(k.ReleaseWav) ? "" : "Custom release loaded. ");
            if (C<TextBlock>("CustomLabel").Text == "") C<TextBlock>("CustomLabel").Text = "Optional: PCM 16-bit WAV, up to 2 sec.";
        }
        void EditKey() {
            if (loading) return; KeySound k = Current.Get(selected);
            k.Preset = C<ComboBox>("KeyPreset").SelectedItem as string ?? "Default"; k.Pitch = C<Slider>("Pitch").Value; k.Tone = C<Slider>("Tone").Value; k.Volume = C<Slider>("KeyVolume").Value;
            Current.Keys[selected.ToString()] = k; KeyLabels(k); Changed(); PaintKeys();
        }
        void Changed() { cache.Clear(); WarmCache(); ScheduleSave(); }
        void WarmCache() {
            var codes = new HashSet<int>(new [] {30,28,57}.Concat(Current.Keys.Keys.Select(Int32.Parse)));
            if (RecordedBank.IsSwitch(Current.Preset)) codes.UnionWith(RecordedBank.Codes(Current.Preset));
            foreach (int code in codes) for (int v = 0; v < 3; v++) { Sample(code, false, v); Sample(code, true, v); }
        }
        float[] Sample(int code, bool release, int variant) {
            KeySound k = Current.Get(code); if (k.Preset == "Silent") return null;
            string wav = release ? k.ReleaseWav : k.PressWav; int category = SoundBank.Category(code);
            string preset = Current.EffectivePreset(code);
            string id = (Current.Keys.ContainsKey(code.ToString()) || RecordedBank.IsSwitch(preset) ? "key" + code : "cat" + category) + ":" + release + ":" + variant;
            float[] sample; if (cache.TryGetValue(id, out sample)) return sample;
            float[] raw = String.IsNullOrEmpty(wav) ? (RecordedBank.IsSwitch(preset) ? RecordedBank.Get(preset,code,release,variant) : SoundBank.Generate(preset, category, release, variant)) : SoundBank.ReadWav(Convert.FromBase64String(wav));
            sample = SoundBank.Tune(raw, k.Pitch, k.Tone, k.Volume); cache[id] = sample; return sample;
        }
        void OnKey(int code, bool release) {
            if (capture) { if (!release) { capture = false; C<Button>("CaptureButton").Content = "Select by typing"; if (code != 1) { selected = code; LoadKey(); PaintKeys(); } } return; }
            if (settings.Enabled && (!release || Current.Release) && audio != null) audio.Play(Sample(code, release, Current.Variation ? random.Next(3) : 1), (float)Current.Volume);
        }
        void Preview(int code) {
            if (audio == null) { SetStatus("No audio output. Open settings and reconnect."); return; }
            audio.Play(Sample(code, false, 1), (float)Current.Volume);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(85) }; Profile profile = Current;
            timer.Tick += delegate { timer.Stop(); if (audio != null && Current == profile && Current.Release) audio.Play(Sample(code, true, 1), (float)Current.Volume); }; timer.Start();
            SetStatus("Previewing " + (labels.ContainsKey(code) ? labels[code] : "selected key") + ". Preview also works when background sound is muted.");
        }
        void Import(bool release) {
            var dialog = new OpenFileDialog { Title = "Import " + (release ? "release" : "press") + " sound", Filter = "PCM 16-bit WAV|*.wav" };
            if (dialog.ShowDialog(Window) != true) return;
            try { if (new FileInfo(dialog.FileName).Length > 1024 * 1024) throw new InvalidDataException("Use a WAV file under 1 MB."); byte[] bytes = File.ReadAllBytes(dialog.FileName); SoundBank.ReadWav(bytes); KeySound k = Current.Get(selected); if (release) k.ReleaseWav = Convert.ToBase64String(bytes); else k.PressWav = Convert.ToBase64String(bytes); if (k.Preset == "Silent") k.Preset = "Default"; Current.Keys[selected.ToString()] = k; Changed(); LoadKey(); PaintKeys(); Preview(selected); }
            catch (Exception e) { MessageBox.Show(Window, e.Message, "Could not import sound"); }
        }
        void OpenAudio() {
            if (audio != null) { audio.Dispose(); audio = null; }
            try { var devices = AudioEngine.Devices(); int index = devices.IndexOf(settings.Output); if (index < 0) { settings.Output = "System default"; index = 0; } audio = new AudioEngine(index - 1); UpdateStatus(); }
            catch (Exception e) { SetStatus(e.Message); }
        }
        void Toggle() { settings.Enabled = !settings.Enabled; if (audio != null) audio.Clear(); if (input != null) input.Reset(); UpdateToggle(); ScheduleSave(); UpdateStatus(); }
        void UpdateToggle() { C<Button>("EnableButton").Content = settings.Enabled ? "Sound on" : "Sound muted"; C<Button>("EnableButton").Background = Color(settings.Enabled ? "#8FE1C2" : "#394453"); C<Button>("EnableButton").Foreground = Color(settings.Enabled ? "#12221E" : "#F2F4F7"); if (tray != null) tray.Text = settings.Enabled ? "KeyTone — sound on" : "KeyTone — muted"; }
        void ScheduleSave() { if (saveTimer != null) { saveTimer.Stop(); saveTimer.Start(); } }
        void Save() { try { Store.Write(settingsFile, settings); } catch (Exception e) { SetStatus("Could not save settings: " + e.Message); } }
        void UpdateStatus() { C<TextBlock>("Status").Text = audio == null ? "Audio unavailable · open settings to reconnect" : (settings.Enabled ? "Listening locally" : "Background sounds muted") + "  ·  " + settings.Output; }
        void SetStatus(string text) { C<TextBlock>("Status").Text = text; statusUntil = DateTime.UtcNow.AddSeconds(9); }
        void CreateTray() {
            tray = new Forms.NotifyIcon { Text = "KeyTone", Visible = true };
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KeyTone.ico")) tray.Icon = new System.Drawing.Icon(stream);
            var menu = new Forms.ContextMenuStrip(); menu.Items.Add("Open KeyTone", null, delegate { Show(); }); menu.Items.Add("Mute / unmute", null, delegate { Toggle(); }); menu.Items.Add(new Forms.ToolStripSeparator());
            foreach (string preset in SoundBank.Presets) { string name = preset; menu.Items.Add(name, null, delegate { Current.Preset = name; Changed(); LoadProfile(); }); }
            menu.Items.Add(new Forms.ToolStripSeparator()); menu.Items.Add("Exit KeyTone", null, delegate { Quit(); }); tray.ContextMenuStrip = menu; tray.DoubleClick += delegate { Show(); };
        }
        void Show() { Window.Show(); Window.WindowState = WindowState.Normal; Window.Activate(); }
        void Hide() { Window.Hide(); tray.ShowBalloonTip(1800, "KeyTone is in your system tray", "Typing sounds stay on when enabled. Right-click the KeyTone icon to mute or exit.", Forms.ToolTipIcon.Info); }
        void Quit() { quitting = true; Save(); healthTimer.Stop(); saveTimer.Stop(); Microsoft.Win32.SystemEvents.PowerModeChanged -= PowerChanged; if (input != null) input.Dispose(); if (audio != null) audio.Dispose(); tray.Visible = false; tray.Dispose(); Window.Close(); Application.Current.Shutdown(); }

        void ShowSettings() {
            var dialog = new Window { Title = "KeyTone — Profiles & settings", Owner = Window, Width = 550, Height = 735, MinHeight = 600, WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = Color("#101318"), Foreground = Color("#F2F4F7"), FontFamily = Window.FontFamily, FontSize = 14, Resources = Window.Resources };
            var stack = new StackPanel { Margin = new Thickness(26) }; dialog.Content = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            Action<string> heading = delegate(string text) { stack.Children.Add(new TextBlock { Text = text, FontSize = 20, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,10,0,12) }); };
            heading("Profiles");
            var profiles = new ComboBox { ItemsSource = settings.Profiles.Select(p => p.Name).ToArray(), SelectedIndex = settings.Active, Margin = new Thickness(0,0,0,10) }; AutomationProperties.SetName(profiles, "Active profile"); stack.Children.Add(profiles);
            var nameLabel = new TextBlock { Text = "Profile name", Margin = new Thickness(0,0,0,4) }; stack.Children.Add(nameLabel);
            var nameBox = new TextBox { Text = Current.Name, MaxLength = 60 }; AutomationProperties.SetName(nameBox, "Profile name"); stack.Children.Add(nameBox);
            bool refresh = false;
            Action updateList = delegate { refresh = true; profiles.ItemsSource = settings.Profiles.Select(p => p.Name).ToArray(); profiles.SelectedIndex = settings.Active; nameBox.Text = Current.Name; refresh = false; };
            profiles.SelectionChanged += delegate { if (refresh || profiles.SelectedIndex < 0) return; settings.Active = profiles.SelectedIndex; nameBox.Text = Current.Name; Changed(); LoadProfile(); };
            var row = new WrapPanel { Margin = new Thickness(0,10,0,8) }; stack.Children.Add(row);
            Action<string, Action> add = delegate(string text, Action action) { var b = new Button { Content = text, Margin = new Thickness(0,0,8,8), Padding = new Thickness(12,7,12,7) }; b.Click += delegate { try { action(); } catch (Exception e) { MessageBox.Show(dialog, e.Message, "KeyTone"); } }; row.Children.Add(b); };
            add("Rename", delegate { string n = nameBox.Text.Trim(); if (n.Length == 0) throw new Exception("Enter a profile name."); Current.Name = n; updateList(); LoadProfile(); ScheduleSave(); });
            add("Duplicate", delegate { if (settings.Profiles.Count >= 100) throw new Exception("Maximum 100 profiles."); var p = Store.Json().Deserialize<Profile>(Store.Json().Serialize(Current)); p.Name = "Copy of " + Current.Name; if (p.Name.Length > 60) p.Name = p.Name.Substring(0,60); settings.Profiles.Add(p); settings.Active = settings.Profiles.Count-1; updateList(); LoadProfile(); ScheduleSave(); });
            add("Delete", delegate { if (settings.Profiles.Count == 1) throw new Exception("Keep at least one profile."); if (MessageBox.Show(dialog, "Delete profile “" + Current.Name + "” and its custom sounds?", "Delete profile", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return; settings.Profiles.RemoveAt(settings.Active); settings.Active = Math.Min(settings.Active,settings.Profiles.Count-1); updateList(); Changed(); LoadProfile(); });
            add("Export", delegate { var d = new SaveFileDialog { Filter = "KeyTone profile|*.keytone.json", FileName = "My-keyboard.keytone.json" }; if (d.ShowDialog(dialog) == true) File.WriteAllText(d.FileName, Store.Json().Serialize(Current)); });
            add("Import", delegate { var d = new OpenFileDialog { Filter = "KeyTone profile|*.json" }; if (d.ShowDialog(dialog) != true) return; if (settings.Profiles.Count >= 100 || new FileInfo(d.FileName).Length > 12 * 1024 * 1024) throw new Exception("Profile too large or profile limit reached."); var p = Store.Json().Deserialize<Profile>(File.ReadAllText(d.FileName)); if (p == null) throw new Exception("Empty profile."); p.Validate(); settings.Profiles.Add(p); settings.Active = settings.Profiles.Count-1; updateList(); Changed(); LoadProfile(); });
            heading("Audio output"); var devices = new ComboBox { ItemsSource = AudioEngine.Devices(), SelectedItem = settings.Output }; AutomationProperties.SetName(devices, "Audio output device"); stack.Children.Add(devices);
            devices.SelectionChanged += delegate { if (devices.SelectedItem == null) return; settings.Output = devices.SelectedItem.ToString(); OpenAudio(); ScheduleSave(); };
            var reconnect = new Button { Content = "Refresh & reconnect output", Margin = new Thickness(0,10,0,4), HorizontalAlignment = HorizontalAlignment.Left }; stack.Children.Add(reconnect); reconnect.Click += delegate { devices.ItemsSource = AudioEngine.Devices(); devices.SelectedItem = settings.Output; OpenAudio(); };
            stack.Children.Add(new TextBlock { Text = "Wired headphones give the tightest response. Bluetooth can add delay.", Foreground = Color("#ABB5C4"), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,4,0,10) });
            heading("Background behavior");
            var repeat = new CheckBox { Content = "Play repeated sounds while holding a key", IsChecked = Current.Repeat }; stack.Children.Add(repeat); repeat.Click += delegate { Current.Repeat = repeat.IsChecked == true; if (input != null) input.Repeat = Current.Repeat; ScheduleSave(); };
            profiles.SelectionChanged += delegate { repeat.IsChecked = Current.Repeat; };
            var startup = new CheckBox { Content = "Start KeyTone when I sign in", IsChecked = StartupEnabled() }; stack.Children.Add(startup); startup.Click += delegate { try { using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")) { if (startup.IsChecked == true) key.SetValue("KeyTone", "\"" + Assembly.GetExecutingAssembly().Location + "\" --minimized"); else key.DeleteValue("KeyTone", false); } } catch (Exception e) { startup.IsChecked = StartupEnabled(); MessageBox.Show(dialog, e.Message, "Startup setting"); } };
            stack.Children.Add(new TextBlock { Text = "Closing the window hides it to the tray. To stop the app, use Exit KeyTone. Ctrl + Alt + K toggles background sounds.", Foreground = Color("#ABB5C4"), TextWrapping = TextWrapping.Wrap, FontSize = 12, Margin = new Thickness(0,10,0,12) });
            stack.Children.Add(new TextBlock { Text = "KeyTone 1.2 · Recorded blue / red / brown switches\nWorks offline. Typed content is never saved or sent.\nPitch changes also shorten or lengthen the sound.", Foreground = Color("#ABB5C4"), FontSize = 12, TextWrapping = TextWrapping.Wrap });
            var done = new Button { Content = "Done", Background = Color("#8FE1C2"), Foreground = Color("#12221E"), Margin = new Thickness(0,20,0,0), IsCancel = true }; done.Click += delegate { dialog.Close(); }; stack.Children.Add(done);
            var exit = new Button { Content = "Exit KeyTone", Margin = new Thickness(0,8,0,0) }; exit.Click += delegate { dialog.Close(); Quit(); }; stack.Children.Add(exit);
            dialog.ShowDialog();
        }
        bool StartupEnabled() { using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")) return key != null && key.GetValue("KeyTone") != null; }
    }
}


