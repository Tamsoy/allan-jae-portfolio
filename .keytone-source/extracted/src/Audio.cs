using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyTone {
    public sealed class AudioEngine : IDisposable {
        [StructLayout(LayoutKind.Sequential, Pack = 2)] struct WaveFormat { public ushort tag, channels; public uint rate, average; public ushort align, bits, extra; }
        [StructLayout(LayoutKind.Sequential)] struct Header { public IntPtr data; public uint length, recorded; public IntPtr user; public uint flags, loops; public IntPtr next, reserved; }
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] struct Caps { public ushort mid, pid; public uint version; [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string name; public uint formats; public ushort channels, reserved; public uint support; }
        [DllImport("winmm.dll")] static extern uint waveOutOpen(out IntPtr h, uint id, ref WaveFormat f, IntPtr callback, IntPtr instance, uint flags);
        [DllImport("winmm.dll")] static extern uint waveOutPrepareHeader(IntPtr h, IntPtr p, uint size);
        [DllImport("winmm.dll")] static extern uint waveOutUnprepareHeader(IntPtr h, IntPtr p, uint size);
        [DllImport("winmm.dll")] static extern uint waveOutWrite(IntPtr h, IntPtr p, uint size);
        [DllImport("winmm.dll")] static extern uint waveOutReset(IntPtr h);
        [DllImport("winmm.dll")] static extern uint waveOutClose(IntPtr h);
        [DllImport("winmm.dll")] static extern uint waveOutGetNumDevs();
        [DllImport("winmm.dll", CharSet=CharSet.Unicode)] static extern uint waveOutGetDevCapsW(UIntPtr id, out Caps caps, uint size);
        public static List<string> Devices() { var names = new List<string> { "System default" }; for (uint i = 0; i < waveOutGetNumDevs(); i++) { Caps c; if (waveOutGetDevCapsW(new UIntPtr(i), out c, (uint)Marshal.SizeOf(typeof(Caps))) == 0) names.Add(c.name); else names.Add("Output " + (i+1)); } return names; }
        class Voice { public float[] Samples; public int Position; public float Gain; }
        readonly object gate = new object(); readonly List<Voice> voices = new List<Voice>(64);
        IntPtr handle; IntPtr[] headers = new IntPtr[3], buffers = new IntPtr[3];
        readonly int headerSize = Marshal.SizeOf(typeof(Header));
        const int Frames = 240; Thread thread; volatile bool running;
        public volatile string Error;
        public long Played, NonzeroFrames, Blocks;
        public AudioEngine(int device) {
            var f = new WaveFormat { tag = 1, channels = 1, rate = 48000, average = 96000, align = 2, bits = 16 };
            uint result = waveOutOpen(out handle, device < 0 ? UInt32.MaxValue : (uint)device, ref f, IntPtr.Zero, IntPtr.Zero, 0);
            if (result != 0) throw new InvalidOperationException("Audio output could not open (Windows code " + result + "). Choose another output and reconnect.");
            try {
                for (int i = 0; i < 3; i++) {
                    buffers[i] = Marshal.AllocHGlobal(Frames * 2); Marshal.Copy(new byte[Frames * 2], 0, buffers[i], Frames * 2);
                    headers[i] = Marshal.AllocHGlobal(headerSize); Marshal.StructureToPtr(new Header { data = buffers[i], length = Frames * 2 }, headers[i], false);
                    Check(waveOutPrepareHeader(handle, headers[i], (uint)headerSize)); Check(waveOutWrite(handle, headers[i], (uint)headerSize));
                }
                running = true; thread = new Thread(Pump) { IsBackground = true, Name = "KeyTone audio", Priority = ThreadPriority.AboveNormal }; thread.Start();
            } catch { Dispose(); throw; }
        }
        static void Check(uint n) { if (n != 0) throw new InvalidOperationException("Audio device error " + n + ". Reconnect the output."); }
        public void Play(float[] samples, float gain) {
            if (samples == null || gain <= 0 || !running) return;
            lock (gate) { if (voices.Count == 64) voices.RemoveAt(0); voices.Add(new Voice { Samples = samples, Gain = gain }); Played++; }
        }
        public void Clear() { lock (gate) voices.Clear(); }
        void Pump() {
            short[] pcm = new short[Frames]; float[] mix = new float[Frames]; int next = 0;
            try {
                while (running) {
                    Header h = (Header)Marshal.PtrToStructure(headers[next], typeof(Header));
                    if ((h.flags & 1) == 0) { Thread.Sleep(2); continue; }
                    Array.Clear(mix, 0, mix.Length);
                    lock (gate) {
                        for (int v = voices.Count - 1; v >= 0; v--) {
                            Voice voice = voices[v]; int count = Math.Min(Frames, voice.Samples.Length - voice.Position);
                            for (int i = 0; i < count; i++) mix[i] += voice.Samples[voice.Position++] * voice.Gain;
                            if (voice.Position >= voice.Samples.Length) voices.RemoveAt(v);
                        }
                    }
                    for (int i = 0; i < Frames; i++) { float x = mix[i]; pcm[i] = (short)(Math.Max(-.98, Math.Min(.98, x)) * 32767); if (pcm[i] != 0) NonzeroFrames++; }
                    Marshal.Copy(pcm, 0, buffers[next], Frames); Check(waveOutWrite(handle, headers[next], (uint)headerSize)); Blocks++; next = (next + 1) % 3;
                }
            } catch (Exception e) { Error = e.Message; running = false; }
        }
        public void Dispose() {
            running = false; if (thread != null && thread != Thread.CurrentThread) thread.Join();
            if (handle != IntPtr.Zero) waveOutReset(handle);
            for (int i = 0; i < 3; i++) { if (headers[i] != IntPtr.Zero) { if (handle != IntPtr.Zero) waveOutUnprepareHeader(handle, headers[i], (uint)headerSize); Marshal.FreeHGlobal(headers[i]); headers[i] = IntPtr.Zero; } if (buffers[i] != IntPtr.Zero) { Marshal.FreeHGlobal(buffers[i]); buffers[i] = IntPtr.Zero; } }
            if (handle != IntPtr.Zero) { waveOutClose(handle); handle = IntPtr.Zero; }
        }
    }
}
