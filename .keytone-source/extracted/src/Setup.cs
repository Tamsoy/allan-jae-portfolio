using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KeyToneSetup {
    static class Program {
        static string productDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "KeyTone");
        static string maintenanceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyTone", "Installer");
        static string uninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyTone";
        static string shortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "KeyTone.lnk");
        static string[] files = { "KeyTone.exe", "KeyTone.exe.config", "README.txt", "LICENSE.txt", "THIRD-PARTY-NOTICES.txt" };
        static bool test;
        [STAThread] static int Main(string[] args) {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            int at = Array.IndexOf(args, "--target"); if (at >= 0) { if (at+1 >= args.Length) return 2; productDir = Path.GetFullPath(args[at+1]); test = true; }
            bool remove = args.Contains("--uninstall"), quiet = args.Contains("--quiet");
            if (quiet) { try { if (remove) Uninstall(); else Install(); return 0; } catch (Exception e) { File.WriteAllText(Path.Combine(Path.GetTempPath(), "KeyTone-setup-error.txt"), e.ToString()); return 1; } }
            var form = new Form { Text = remove ? "Uninstall KeyTone" : "Install KeyTone", ClientSize = new Size(520,370), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, StartPosition = FormStartPosition.CenterScreen, BackColor = Color.FromArgb(16,19,24), ForeColor = Color.White, Font = new Font("Segoe UI",10) };
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("KeyTone.ico")) form.Icon = new Icon(s);
            form.Controls.Add(new Label { Text = remove ? "Goodbye, for now." : "A better sound for every key.", Font = new Font("Segoe UI",22,FontStyle.Bold), Location = new Point(28,27), Size = new Size(470,75) });
            form.Controls.Add(new Label { Text = remove ? "Remove KeyTone and its Start menu shortcut.\nYour profiles and cached installer will be kept." : "KeyTone 1.2 for Windows\nRecorded blue / red / brown switches. Per-key tuning.\nWorks offline. No account or extra downloads.", ForeColor = Color.FromArgb(190,202,218), Location = new Point(30,111), Size = new Size(460,82) });
            form.Controls.Add(new Label { Text = (remove ? "Remove from:\n" : "Install for your Windows account:\n") + productDir, ForeColor = Color.FromArgb(190,202,218), Location = new Point(30,202), Size = new Size(460,64) });
            var action = new Button { Text = remove ? "Uninstall KeyTone" : "Install & launch", Location = new Point(28,285), Size = new Size(285,48), BackColor = Color.FromArgb(143,225,194), ForeColor = Color.FromArgb(18,34,30), FlatStyle = FlatStyle.Flat };
            var cancel = new Button { Text = "Cancel", Location = new Point(327,285), Size = new Size(160,48), FlatStyle = FlatStyle.Flat }; cancel.Click += delegate { form.Close(); }; form.Controls.Add(action); form.Controls.Add(cancel);
            action.Click += delegate { action.Enabled = false; try { if (remove) { Uninstall(); MessageBox.Show("KeyTone was removed. Your profiles have been kept.", "KeyTone"); } else { Install(); Process.Start(Path.Combine(productDir,"KeyTone.exe")); } form.Close(); } catch (Exception e) { MessageBox.Show(form, e.Message, "Setup could not finish", MessageBoxButtons.OK, MessageBoxIcon.Error); action.Enabled = true; } };
            Application.Run(form); return 0;
        }
        static void EnsureNotRunning() {
            foreach (var p in Process.GetProcessesByName("KeyTone")) { try { if (String.Equals(Path.GetFullPath(p.MainModule.FileName), Path.Combine(productDir,"KeyTone.exe"), StringComparison.OrdinalIgnoreCase)) throw new IOException("Please exit KeyTone from its tray menu, then try again."); } catch (System.ComponentModel.Win32Exception) { } finally { p.Dispose(); } }
        }
        static void Install() {
            EnsureNotRunning(); Directory.CreateDirectory(productDir);
            foreach (string file in files) {
                using (var embedded = Assembly.GetExecutingAssembly().GetManifestResourceStream(file + ".gz")) using (var gzip = new GZipStream(embedded, CompressionMode.Decompress)) using (var output = File.Create(Path.Combine(productDir,file))) gzip.CopyTo(output);
            }
            if (test) return;
            Directory.CreateDirectory(maintenanceDir); string maintenance = Path.Combine(maintenanceDir,"KeyTone-Setup.exe");
            if (!String.Equals(Assembly.GetExecutingAssembly().Location,maintenance,StringComparison.OrdinalIgnoreCase)) File.Copy(Assembly.GetExecutingAssembly().Location, maintenance, true);
            dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")); dynamic link = shell.CreateShortcut(shortcut); link.TargetPath = Path.Combine(productDir,"KeyTone.exe"); link.WorkingDirectory = productDir; link.Description = "Mechanical keyboard sounds, tuned to you"; link.Save();
            using (var key = Registry.CurrentUser.CreateSubKey(uninstallKey)) { key.SetValue("DisplayName", "KeyTone"); key.SetValue("DisplayVersion","1.2.0"); key.SetValue("Publisher","KeyTone"); key.SetValue("DisplayIcon",Path.Combine(productDir,"KeyTone.exe")); key.SetValue("InstallLocation",productDir); key.SetValue("UninstallString","\""+maintenance+"\" --uninstall"); key.SetValue("NoModify",1); key.SetValue("NoRepair",1); key.SetValue("EstimatedSize",(int)(new FileInfo(Path.Combine(productDir,"KeyTone.exe")).Length/1024+100)); }
        }
        static void Uninstall() {
            EnsureNotRunning();
            foreach (string name in files) { string path = Path.Combine(productDir,name); if (File.Exists(path)) File.Delete(path); }
            if (Directory.Exists(productDir) && !Directory.EnumerateFileSystemEntries(productDir).Any()) Directory.Delete(productDir);
            if (test) return;
            if (File.Exists(shortcut)) File.Delete(shortcut);
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",true)) { if (key != null) key.DeleteValue("KeyTone",false); }
            Registry.CurrentUser.DeleteSubKeyTree(uninstallKey,false);
        }
    }
}

