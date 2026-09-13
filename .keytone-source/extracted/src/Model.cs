using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace KeyTone {
    public class KeySound {
        public string Preset { get; set; }
        public double Pitch { get; set; }
        public double Tone { get; set; }
        public double Volume { get; set; }
        public string PressWav { get; set; }
        public string ReleaseWav { get; set; }
        public KeySound() { Preset = "Default"; Volume = 1; }
    }
    public class Profile {
        public string Name { get; set; }
        public string Preset { get; set; }
        public double Volume { get; set; }
        public bool Release { get; set; }
        public bool Variation { get; set; }
        public bool Repeat { get; set; }
        public Dictionary<string, KeySound> Keys { get; set; }
        public Profile() { Name = "My keyboard"; Preset = "Deep thock"; Volume = .55; Release = true; Variation = true; Keys = new Dictionary<string, KeySound>(); }
        public KeySound Get(int code) { KeySound k; return Keys.TryGetValue(code.ToString(), out k) ? k : new KeySound(); }
        public string EffectivePreset(int code) { var k = Get(code); return k.Preset == "Default" ? Preset : k.Preset; }
        public void SetKeySwitch(int code, string preset) {
            if (!RecordedBank.IsSwitch(preset)) throw new ArgumentException("Choose Blue, Red or Brown Switch.");
            // A deliberate switch selection replaces imported audio and old tuning for this key.
            Keys[code.ToString()] = new KeySound { Preset = preset };
        }
        public void ApplySwitchToAll(string preset) {
            if (!RecordedBank.IsSwitch(preset)) throw new ArgumentException("Choose Blue, Red or Brown Switch.");
            Preset = preset; Keys.Clear();
        }
        public void Validate() {
            if (String.IsNullOrWhiteSpace(Name) || Name.Length > 60) throw new InvalidDataException("Profile name must contain 1–60 characters.");
            if (!SoundBank.Presets.Contains(Preset)) throw new InvalidDataException("Unknown keyboard preset.");
            if (Double.IsNaN(Volume) || Volume < 0 || Volume > 1) throw new InvalidDataException("Invalid master volume.");
            if (Keys == null || Keys.Count > 768) throw new InvalidDataException("Invalid key settings.");
            foreach (var pair in Keys) {
                int code; KeySound k = pair.Value;
                if (!Int32.TryParse(pair.Key, out code) || code < 1 || code > 767 || k == null) throw new InvalidDataException("Invalid key identifier.");
                if (k.Preset != "Default" && k.Preset != "Silent" && !SoundBank.Presets.Contains(k.Preset)) throw new InvalidDataException("Unknown key preset.");
                if (Double.IsNaN(k.Pitch) || Math.Abs(k.Pitch) > 12 || Double.IsNaN(k.Tone) || Math.Abs(k.Tone) > 1 || Double.IsNaN(k.Volume) || k.Volume < 0 || k.Volume > 1.5) throw new InvalidDataException("Key settings are out of range.");
                if (!String.IsNullOrEmpty(k.PressWav)) SoundBank.ReadWav(Convert.FromBase64String(k.PressWav));
                if (!String.IsNullOrEmpty(k.ReleaseWav)) SoundBank.ReadWav(Convert.FromBase64String(k.ReleaseWav));
            }
        }
    }
    public class Settings {
        public int Version { get; set; }
        public int Active { get; set; }
        public bool Enabled { get; set; }
        public string Output { get; set; }
        public List<Profile> Profiles { get; set; }
        public Settings() { Version = 1; Enabled = true; Output = "System default"; Profiles = new List<Profile> { new Profile() }; }
    }
    public static class Store {
        public static JavaScriptSerializer Json() { return new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024, RecursionLimit = 32 }; }
        public static void Write(string path, object value) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            string json = Json().Serialize(value);
            if (System.Text.Encoding.UTF8.GetByteCount(json) > 16 * 1024 * 1024) throw new InvalidDataException("Profile storage exceeds 16 MB. Remove unused profiles or custom WAV sounds.");
            File.WriteAllText(temporary, json);
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak"); else File.Move(temporary, path);
        }
        public static Settings Read(string path, out string warning) {
            warning = null;
            if (!File.Exists(path)) return new Settings();
            foreach (string candidate in new [] { path, path + ".bak" }) {
                try {
                    if (new FileInfo(candidate).Length > 16 * 1024 * 1024) throw new InvalidDataException();
                    var s = Json().Deserialize<Settings>(File.ReadAllText(candidate));
                    if (s == null || s.Version != 1 || s.Profiles == null || s.Profiles.Count < 1 || s.Profiles.Count > 100 || s.Active < 0 || s.Active >= s.Profiles.Count) throw new InvalidDataException();
                    foreach (var p in s.Profiles) p.Validate();
                    if (candidate != path) warning = "Recovered settings from the backup.";
                    return s;
                } catch { warning = "Settings could not be read. Defaults loaded; original saved as settings.recovery.json."; }
            }
            File.Copy(path, Path.Combine(Path.GetDirectoryName(path), "settings.recovery.json"), true);
            return new Settings();
        }
    }
    public static class SoundBank {
        public static readonly string[] Presets = { "Deep thock", "Crisp click", "Soft tactile", "Smooth linear", "Muted", "Blue Switch", "Red Switch", "Brown Switch" };
        public const int Rate = 48000;
        public static float[] Generate(string preset, int category, bool release, int variation) {
            if (RecordedBank.IsSwitch(preset)) return RecordedBank.Get(preset, category == 2 ? 57 : category == 1 ? 28 : 30, release, variation);
            int p = Array.IndexOf(Presets, preset); if (p < 0) p = 0;
            double[] freq = { 185, 1150, 410, 285, 165 };
            double[] decay = { .024, .012, .018, .014, .010 };
            double f = freq[p] * (category == 2 ? .68 : category == 1 ? .82 : 1) * (1 + (variation - 1) * .024);
            double d = decay[p] * (category == 2 ? 1.45 : 1);
            if (release) { f *= 1.35; d *= .55; }
            int n = (int)(Rate * (release ? .085 : .18));
            float[] data = new float[n]; Random r = new Random(1337 + p * 111 + category * 17 + variation + (release ? 701 : 0));
            double low = 0;
            for (int i = 0; i < n; i++) {
                double t = i / (double)Rate;
                double noise = r.NextDouble() * 2 - 1; low += .19 * (noise - low);
                double attack = Math.Min(1, t / .0007);
                double body = (Math.Sin(2 * Math.PI * f * t) + .40 * Math.Sin(2 * Math.PI * f * 2.71 * t) + .17 * Math.Sin(2 * Math.PI * f * 4.1 * t)) * Math.Exp(-t / d);
                double snap = (p == 1 ? noise : low) * Math.Exp(-t / (p == 1 ? .004 : .006)) * (p == 4 ? .25 : .7);
                double click = p == 1 && t > .006 ? Math.Sin(2 * Math.PI * 2600 * (t - .006)) * Math.Exp(-(t - .006) / .003) * .5 : 0;
                double rattle = category > 0 ? low * Math.Exp(-Math.Max(0, t - .011) / .017) * (t > .011 ? .18 : 0) : 0;
                data[i] = (float)((body * .45 + snap * .45 + click + rattle) * attack * (release ? .34 : .85) * (p == 4 ? .65 : 1) * Math.Min(1, (n - i - 1) / 120.0));
            }
            return data;
        }
        public static int Category(int code) { return code == 57 ? 2 : new[] {14,15,28,284,42,54,58,29,285,56,312}.Contains(code) ? 1 : 0; }
        public static float[] Tune(float[] source, double pitch, double tone, double volume) {
            double step = Math.Pow(2, pitch / 12); int n = Math.Max(1, (int)(source.Length / step));
            float[] result = new float[n]; double low = 0;
            for (int i = 0; i < n; i++) {
                double pos = i * step; int j = (int)pos; double frac = pos - j;
                double v = source[j] * (1 - frac) + source[Math.Min(j + 1, source.Length - 1)] * frac;
                low += .12 * (v - low);
                v = tone < 0 ? v * (1 + tone) + low * -tone : v + tone * .8 * (v - low);
                result[i] = (float)(v * volume);
            }
            return result;
        }
        public static float[] ReadWav(byte[] bytes) {
            if (bytes.Length < 44 || bytes.Length > 1024 * 1024) throw new InvalidDataException("Use a PCM 16-bit WAV under 1 MB and 2 seconds.");
            using (var s = new MemoryStream(bytes)) using (var r = new BinaryReader(s)) {
                if (new string(r.ReadChars(4)) != "RIFF") throw new InvalidDataException("Not a WAV file.");
                r.ReadUInt32(); if (new string(r.ReadChars(4)) != "WAVE") throw new InvalidDataException("Not a WAV file.");
                int format = 0, channels = 0, rate = 0, bits = 0; byte[] pcm = null;
                while (s.Position + 8 <= s.Length) {
                    string id = new string(r.ReadChars(4)); uint size = r.ReadUInt32(); long end = s.Position + size;
                    if (end > s.Length) throw new InvalidDataException("Truncated WAV file.");
                    if (id == "fmt " && size >= 16) { format = r.ReadUInt16(); channels = r.ReadUInt16(); rate = r.ReadInt32(); r.ReadInt32(); r.ReadUInt16(); bits = r.ReadUInt16(); }
                    if (id == "data") pcm = r.ReadBytes((int)size);
                    s.Position = end + (size % 2);
                }
                if (format != 1 || bits != 16 || (channels != 1 && channels != 2) || rate < 8000 || rate > 192000 || pcm == null || pcm.Length == 0 || pcm.Length % (2 * channels) != 0) throw new InvalidDataException("Use PCM 16-bit WAV, mono or stereo, 8–192 kHz.");
                int frames = pcm.Length / (2 * channels); if (frames > rate * 2) throw new InvalidDataException("Sound must be 2 seconds or shorter.");
                float[] original = new float[frames];
                for (int i = 0; i < frames; i++) { double value = 0; for (int c = 0; c < channels; c++) value += BitConverter.ToInt16(pcm, (i * channels + c) * 2) / 32768.0; original[i] = (float)(value / channels); }
                float[] converted = new float[(int)((long)frames * Rate / rate)];
                for (int i = 0; i < converted.Length; i++) { double at = i * rate / (double)Rate; int j = (int)at; converted[i] = (float)(original[j] * (1 - (at - j)) + original[Math.Min(j + 1, frames - 1)] * (at - j)); }
                return converted;
            }
        }
        public static void WriteWav(string path, float[] data) {
            using (var w = new BinaryWriter(File.Create(path))) {
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + data.Length * 2); w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(Rate); w.Write(Rate * 2); w.Write((short)2); w.Write((short)16); w.Write(System.Text.Encoding.ASCII.GetBytes("data")); w.Write(data.Length * 2);
                foreach (float x in data) w.Write((short)(Math.Max(-1, Math.Min(1, x)) * 32767));
            }
        }
    }
}
