# KeyTone 1.3.1 audio repair — 16 September 2026

## Findings and changes

The prior native mixer passed byte counts to the Int16 overload of Marshal.Copy. This could fail with an out-of-range copy as soon as the driver completed a buffer. It also passed the header size plus the audio payload size to waveOutWrite; that API requires only sizeof(WAVEHDR). Both sizes are corrected.

The former SoundPlayer fallback could interrupt previous keystrokes, and delayed Stop calls could stop a newer sound. It counted nonzero source samples before confirming output. That fallback is removed. A bounded 64-voice mixer now sends overlapping keystrokes to one persistent PCM stream. Unavailable output produces a visible error and automatic retries instead of a false healthy status.

The mixer uses three 480-frame buffers (10 ms each at 48 kHz), reuses PCM arrays, and resamples 48 kHz source sounds when the driver negotiates 44.1 kHz. Shutdown joins the worker before releasing native memory. A stalled driver is detected after two seconds. Device arrival/removal, resume, and polling of the default Windows endpoint trigger reconnection; failed startup output is retried every two seconds.

No Windows sound volume, audio drivers, security controls, or stored keyboard profiles were changed. Typed text remains excluded from saved settings.

## Verification of installed build

- Engine and sound/settings regression suite: 83 checks passed.
- Installed native UI suite: 53 passed, zero failed.
- Installer suite: 18 passed, zero failed. Upgrade preserved the existing settings file byte-for-byte.
- Blue, red, and brown samples completed nonzero buffers on System default, Speakers (AB13X USB Audio), and Speakers (Realtek(R) Audio).
- Sustained rapid playback, overlapping voice stress, mute drain, close/reopen, malformed WAV rejection, profile persistence, themes and layout checks passed.
- Independent Windows endpoint-meter check: idle baseline 0.000000, playback peaks 0.139587 on default/USB and 0.136915 on Realtek. No microphone recording was used. This verifies a signal reached the Windows render endpoints; it is not a physical listening measurement.

Earlier fallback-based results in the 1.3 audit did not establish overlapping native playback and must not be treated as proof of this fix. The 1.3.1 reports replace those conclusions.

## Practical limits

After installation, the user tested typing with speakers and earphones and confirmed: "Both work correctly." The disconnected BD86 Bluetooth device was not tested. All driver versions and other PCs were not tested. Keyboard callbacks were exercised in the UI suite; arbitrary physical typing in other applications was not independently simulated. No claim of zero bugs or universal hardware compatibility is made.

## Reproduce

Run build.ps1, then run build/KeyTone.exe --self-test followed by an output directory, and --ui-audit followed by a separate output directory. Start-Process -Wait should be used from PowerShell because this is a GUI executable. Run test-install.ps1 after exiting KeyTone from the tray. AudioRouteCheck.cs is an independent meter test; compile it with the .NET Framework C# compiler referencing build/KeyTone.exe, and place the helper executable beside KeyTone.exe.

API references: https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.copy and https://learn.microsoft.com/en-us/windows/win32/api/mmeapi/nf-mmeapi-waveoutwrite . The process-wide interruption behavior of the removed fallback is documented at https://learn.microsoft.com/en-us/previous-versions/dd743680(v=vs.85) .
