using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace KeyTone {
    public sealed class KeyboardInput : IDisposable {
        [StructLayout(LayoutKind.Sequential)] struct Device { public ushort page, usage; public uint flags; public IntPtr target; }
        [DllImport("user32.dll", SetLastError=true)] static extern bool RegisterRawInputDevices(Device[] devices, uint count, uint size);
        [DllImport("user32.dll")] static extern uint GetRawInputData(IntPtr raw, uint command, IntPtr data, ref uint size, uint header);
        [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr h, int id, uint modifiers, uint key);
        [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr h, int id);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("wtsapi32.dll")] static extern bool WTSRegisterSessionNotification(IntPtr h, uint flags);
        [DllImport("wtsapi32.dll")] static extern bool WTSUnRegisterSessionNotification(IntPtr h);
        readonly HwndSource source; readonly HashSet<string> held = new HashSet<string>();
        readonly IntPtr buffer = Marshal.AllocHGlobal(128); bool locked;
        public bool Repeat, HotkeyAvailable;
        public Action<int, bool> Key;
        public Action Toggle, Activate, DeviceChanged;
        public long EventCount;
        public KeyboardInput(HwndSource window) {
            source = window; source.AddHook(Hook);
            var devices = new [] { new Device { page = 1, usage = 6, flags = 0x100 | 0x2000, target = source.Handle } };
            if (!RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(Device)))) throw new InvalidOperationException("Windows keyboard input registration failed.");
            HotkeyAvailable = RegisterHotKey(source.Handle, 817, 0x0002 | 0x0001 | 0x4000, 0x4B);
            WTSRegisterSessionNotification(source.Handle, 0);
        }
        public void Reset() { held.Clear(); }
        IntPtr Hook(IntPtr hwnd, int msg, IntPtr w, IntPtr l, ref bool handled) {
            if (msg == 0x0312 && w.ToInt32() == 817) { if (Toggle != null) Toggle(); handled = true; }
            if (msg == 0x8001) { if (Activate != null) Activate(); handled = true; }
            if (msg == 0x02B1) { held.Clear(); locked = w.ToInt32() == 7; }
            if (msg == 0x0218 || msg == 0x00FE) held.Clear();
            if (msg == 0x0219) { held.Clear(); if (DeviceChanged != null) DeviceChanged(); }
            if (msg == 0x00FF && !locked) {
                uint size = 128, header = (uint)(8 + IntPtr.Size * 2);
                uint got = GetRawInputData(l, 0x10000003, buffer, ref size, header);
                if (got == UInt32.MaxValue || got < header + 16 || Marshal.ReadInt32(buffer) != 1) return IntPtr.Zero;
                int offset = (int)header;
                int scan = (ushort)Marshal.ReadInt16(buffer, offset), flags = (ushort)Marshal.ReadInt16(buffer, offset + 2), vk = (ushort)Marshal.ReadInt16(buffer, offset + 6);
                if (scan == 0 || scan == 255 || vk == 255) return IntPtr.Zero;
                int code = scan | ((flags & 2) != 0 ? 256 : 0) | ((flags & 4) != 0 ? 512 : 0);
                string identity = Marshal.ReadIntPtr(buffer, 8).ToInt64().ToString() + ":" + code;
                bool release = (flags & 1) != 0;
                if (release) { if (!held.Remove(identity)) return IntPtr.Zero; }
                else if (!held.Add(identity) && !Repeat) return IntPtr.Zero;
                EventCount++; if (Key != null) Key(code, release);
            }
            return IntPtr.Zero;
        }
        public void Dispose() {
            source.RemoveHook(Hook); UnregisterHotKey(source.Handle, 817); WTSUnRegisterSessionNotification(source.Handle);
            RegisterRawInputDevices(new[] { new Device { page = 1, usage = 6, flags = 1, target = IntPtr.Zero } }, 1, (uint)Marshal.SizeOf(typeof(Device)));
            Marshal.FreeHGlobal(buffer);
        }
    }
}
