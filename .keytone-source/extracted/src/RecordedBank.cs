using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KeyTone {
    // PCM segments derived from Mechvibes' recorded Cherry MX ABS sound packs.
    // See THIRD-PARTY-NOTICES.txt and assets/prepare_switches.py for provenance.
    public static class RecordedBank {
        static readonly Dictionary<string, Dictionary<int, float[][]>> packs = new Dictionary<string, Dictionary<int, float[][]>>();
        public static bool IsSwitch(string preset) { return preset == "Blue Switch" || preset == "Red Switch" || preset == "Brown Switch"; }
        public static float[] Get(string preset, int code, bool release, int variation) {
            Dictionary<int, float[][]> pack;
            if (!packs.TryGetValue(preset, out pack)) {
                pack = new Dictionary<int, float[][]>();
                if (!IsSwitch(preset)) throw new ArgumentException("Unknown recorded switch.");
                string resource = preset == "Blue Switch" ? "blue.bin" : preset == "Red Switch" ? "red.bin" : "brown.bin";
                using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)) {
                    if (s == null) throw new InvalidDataException("Missing switch sound pack: " + resource);
                    using (var r = new BinaryReader(s)) {
                        if (r.ReadInt32() != 0x3153544B) throw new InvalidDataException("Invalid switch sound pack.");
                        int count = r.ReadInt32();
                        for (int k = 0; k < count; k++) {
                            int key = r.ReadInt32(); float[][] events = new float[2][];
                            for (int e = 0; e < 2; e++) {
                                int length = r.ReadInt32(); events[e] = new float[length];
                                for (int i = 0; i < length; i++) events[e][i] = r.ReadInt16() / 32768f;
                            }
                            pack[key] = events;
                        }
                    }
                }
                packs[preset] = pack;
            }
            float[][] sound;
            // Unrecorded international/media keys inherit a normal key, never silence.
            if (!pack.TryGetValue(code, out sound)) sound = pack[30];
            float[] original = sound[release ? 1 : 0];
            if (variation == 1) return original;
            return SoundBank.Tune(original, (variation - 1) * .12, 0, 1 + (variation - 1) * .025);
        }
        public static int[] Codes(string preset) { Get(preset,30,false,1); var result = new int[packs[preset].Count]; packs[preset].Keys.CopyTo(result,0); return result; }
    }
}
