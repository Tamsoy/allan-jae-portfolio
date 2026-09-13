using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace KeyTone {
    public static class SelfTest {
        static List<string> results = new List<string>();
        static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); results.Add("PASS: " + name); }
        public static int Run(string directory) {
            Directory.CreateDirectory(directory);
            try {
                var distinct = new List<double>();
                foreach (string p in SoundBank.Presets) {
                    float[] press = SoundBank.Generate(p, 0, false, 1), release = SoundBank.Generate(p, 0, true, 1);
                    Check(press.All(x => !Single.IsNaN(x) && Math.Abs(x) <= 1) && press.Any(x => Math.Abs(x) > .05), p + " valid audible press");
                    Check(!release.SequenceEqual(press) && release.Any(x => Math.Abs(x) > .001), p + " independent release");
                    Check(!press.SequenceEqual(SoundBank.Generate(p, 2, false, 1)), p + " spacebar distinction");
                    Check(!press.SequenceEqual(SoundBank.Generate(p, 0, false, 2)), p + " variation");
                    distinct.Add(press.Sum(x => (double)x*x)); SoundBank.WriteWav(Path.Combine(directory, p.Replace(" ","-") + ".wav"), press);
                }
                Check(distinct.Distinct().Count() == SoundBank.Presets.Length, "all eight presets are distinct");
                foreach (string preset in new[] {"Blue Switch","Red Switch","Brown Switch"}) {
                    int[] codes = RecordedBank.Codes(preset);
                    Check(codes.Length >= 100, preset + " full physical-key sample coverage");
                    foreach (int code in codes) foreach (bool up in new[] {false,true}) {
                        float[] a = RecordedBank.Get(preset,code,up,1);
                        if (a.Length == 0 || a.Length > 96000 || !a.All(x => !Single.IsNaN(x) && Math.Abs(x) < 1) || !a.Any(x => Math.Abs(x) > .001)) throw new Exception("Invalid recording: " + preset + " " + code + " " + up);
                    }
                    Check(true,preset + " every recorded press/release is finite, audible and bounded");
                    Check(!RecordedBank.Get(preset,30,false,1).SequenceEqual(RecordedBank.Get(preset,31,false,1)),preset + " A and S use distinct key recordings");
                    Check(RecordedBank.Get(preset,767,false,1).Any(x=>x!=0),preset + " unrecorded keys have audible fallback");
                }
                var mixed = new Profile { Preset = "Blue Switch", Volume = .37 };
                mixed.SetKeySwitch(30,"Red Switch"); mixed.SetKeySwitch(57,"Blue Switch");
                Check(mixed.EffectivePreset(30)=="Red Switch" && mixed.EffectivePreset(31)=="Blue Switch" && mixed.EffectivePreset(57)=="Blue Switch", "mix red A with blue remaining keys");
                mixed.Get(30).PressWav = "old imported sample"; mixed.Get(30).Volume = 0;
                mixed.SetKeySwitch(30,"Blue Switch");
                Check(mixed.Get(30).PressWav==null && mixed.Get(30).Volume==1, "choosing a switch replaces a key's old WAV and mute");
                mixed.Keys["31"]=new KeySound {Preset="Silent",Pitch=4};
                var before = Store.Json().Serialize(mixed);
                mixed.ApplySwitchToAll("Red Switch");
                Check(mixed.Keys.Count==0 && Enumerable.Range(1,767).All(code=>mixed.EffectivePreset(code)=="Red Switch") && mixed.Volume==.37, "all red overrides every key including silent keys and preserves master volume");
                mixed.ApplySwitchToAll("Blue Switch");
                Check(Enumerable.Range(1,767).All(code=>mixed.EffectivePreset(code)=="Blue Switch"), "all blue overrides every key");
                mixed=Store.Json().Deserialize<Profile>(before); mixed.Validate();
                Check(mixed.Get(31).Preset=="Silent" && mixed.Get(31).Pitch==4, "whole-keyboard undo snapshot restores old overrides");
                string mixedPath=Path.Combine(directory,"mixed-profile.json"); Store.Write(mixedPath,mixed);
                var restored = Store.Json().Deserialize<Profile>(File.ReadAllText(mixedPath)); restored.Validate();
                Check(restored.EffectivePreset(30)=="Blue Switch" && restored.EffectivePreset(31)=="Silent", "recorded switch profile round trip");
                restored.SetKeySwitch(57,"Brown Switch"); restored.SetKeySwitch(32,"Red Switch");
                Check(restored.EffectivePreset(30)=="Blue Switch" && restored.EffectivePreset(32)=="Red Switch" && restored.EffectivePreset(57)=="Brown Switch", "mix blue, red and brown in one profile");
                string brownSnapshot=Store.Json().Serialize(restored);
                var savedBrown=Store.Json().Deserialize<Profile>(brownSnapshot); savedBrown.Validate();
                Check(savedBrown.EffectivePreset(57)=="Brown Switch", "brown per-key assignment survives profile serialization");
                restored.Get(57).PressWav="replaced sample"; restored.Get(57).ReleaseWav="replaced release"; restored.Get(57).Volume=0;
                restored.SetKeySwitch(57,"Brown Switch");
                Check(restored.Get(57).PressWav==null && restored.Get(57).ReleaseWav==null && restored.Get(57).Volume==1, "brown assignment replaces imported sounds and mute");
                restored.ApplySwitchToAll("Brown Switch"); restored.Validate();
                Check(restored.Keys.Count==0 && Enumerable.Range(1,767).All(code=>restored.EffectivePreset(code)=="Brown Switch") && restored.Volume==.37, "all brown replaces every key and preserves master volume");
                restored=Store.Json().Deserialize<Profile>(brownSnapshot);
                Check(restored.EffectivePreset(30)=="Blue Switch" && restored.EffectivePreset(32)=="Red Switch" && restored.EffectivePreset(57)=="Brown Switch", "undo snapshot restores three-way mixed switches");
                float[] sample = SoundBank.Generate("Deep thock", 0, false, 1);
                Check(SoundBank.Tune(sample, 12, 0, 1).Length == sample.Length / 2, "octave pitch changes sample duration");
                Check(SoundBank.Tune(sample, 0, 0, 0).All(x => x == 0), "zero key volume is silent");
                Check(!sample.SequenceEqual(SoundBank.Tune(sample, 0, -1, 1)), "tone filter alters sample");
                string wav = Path.Combine(directory, "Deep-thock.wav"); float[] decoded = SoundBank.ReadWav(File.ReadAllBytes(wav));
                Check(decoded.Length == sample.Length && decoded.Zip(sample, (a,b) => Math.Abs(a-b)).Max() < .0001, "WAV round trip");
                bool rejected = false; try { SoundBank.ReadWav(new byte[50]); } catch { rejected = true; } Check(rejected, "reject malformed WAV");
                var settings = new Settings(); settings.Profiles[0].Keys["57"] = new KeySound { Preset = "Crisp click", Pitch = 4, Tone = -.5, Volume = .8, PressWav = Convert.ToBase64String(File.ReadAllBytes(wav)) };
                settings.Profiles[0].Validate(); string path = Path.Combine(directory, "settings.json"); Store.Write(path, settings); string warning; var read = Store.Read(path, out warning);
                Check(read.Profiles[0].Get(57).Pitch == 4 && read.Profiles[0].Get(57).PressWav != null, "profile and embedded custom WAV persistence");
                Check(read.Profiles[0].Get(30).Preset == "Default", "unmodified key inherits global preset");
                Store.Write(path, settings); File.WriteAllText(path, "broken"); read = Store.Read(path, out warning); Check(warning != null && read.Profiles[0].Get(57).Pitch == 4, "corrupt settings recover from backup");
                rejected = false; read.Profiles[0].Keys["57"].Pitch = 100; try { read.Profiles[0].Validate(); } catch { rejected = true; } Check(rejected, "reject invalid profile tuning");
                using (var engine = new AudioEngine(-1)) {
                    for (int i = 0; i < 100; i++) engine.Play(sample, .15f);
                    Thread.Sleep(350); Check(engine.Played == 100 && engine.NonzeroFrames > 0 && engine.Blocks > 0 && engine.Error == null, "native device playback, overlap and bounded voice stress");
                    engine.Clear(); Thread.Sleep(100);
                }
                using (var engine = new AudioEngine(-1)) { engine.Play(sample, .2f); Thread.Sleep(150); Check(engine.NonzeroFrames > 0, "audio device close and reopen"); }
                File.WriteAllLines(Path.Combine(directory, "test-results.txt"), results.Concat(new[] { "ALL " + results.Count + " CHECKS PASSED" })); return 0;
            } catch (Exception e) { results.Add(e.ToString()); File.WriteAllLines(Path.Combine(directory, "test-results.txt"), results); return 1; }
        }
    }
}
