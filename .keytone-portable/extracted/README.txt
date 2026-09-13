KEYTONE 1.2 — Mechanical keyboard sounds, tuned to you


BLUE / RED / BROWN SWITCHES (BROWN ADDED IN 1.2)
The Blue Switch, Red Switch and Brown Switch cards select the keyboard default and preview it.
Existing individual-key overrides stay in place when selecting a card.
All keys blue / All keys red / All keys brown replaces ALL individual sounds, imported samples,
mutes and pitch/tone overrides in the current profile. Master volume is kept.
Undo whole-keyboard change restores the previous sounds and tuning during this
session. It is available only in the profile where the change was made.
To mix switches: click any key, then Blue switch, Red switch or Brown switch in the inspector.
These buttons reset that key's imported sounds and tuning to the recorded switch.
The Sound dropdown offers the same choices. Pick Default to follow the keyboard.
Blue/red/brown key colors and tooltips show the effective switch; white border marks
the selected key. A dot marks an individual override.

The recorded packs cover 105 physical codes with separate key-down/key-up
segments. Real key recordings preserve differences between letters, larger keys,
spacebar and numpad. Some modifiers use equivalent recorded keys; unsupported
keys fall back to A. Natural variation adds subtle pitch/volume variation.
Blue is clicky; red is linear; brown is tactile with a mellow bottom-out clack. These are recordings of
one keyboard setup, not a guarantee of the sound of every blue/red switch board.
The five original synthesized styles are under More sound styles.
INSTALL
Run KeyTone-Setup.exe and select Install & launch. Installs only for your Windows account and adds a Start menu shortcut. Requires Windows 11 x64 with its built-in .NET Framework 4.8 or newer. No administrator access or internet connection is needed. This initial build is unsigned.

USE
Choose a switch card or expand More sound styles to change the global keyboard sound and hear a preview. Type in any normal desktop app to hear sounds. Click a key on the visual keyboard to edit pitch, tone, volume, or its preset. A dot marks customized keys. Use Select by typing to select keys not on the ANSI diagram, including numpad and international keys. Escape cancels key selection.

Pitch changes playback speed, so duration changes too. Tone adjusts brightness. Silent mutes one key. Default follows the global preset. Reset this key removes all its overrides, including custom sounds.

Import press / release accepts PCM 16-bit WAV, mono or stereo, 8–192 kHz, up to 2 seconds and 1 MB. Custom audio is embedded in profiles for portability. Global release sounds must be enabled to hear custom release sounds while typing. Play this key deliberately previews even while background sound is muted.

Profiles & settings lets you rename, duplicate, export and import profiles, select or reconnect the output, enable key-repeat sounds, and opt into launch at sign-in. Changes save automatically. Imported profile names are independent of their filenames.

BACKGROUND
Closing the window hides KeyTone to the tray. Right-click its K icon for mute, presets, open, or Exit KeyTone. Ctrl+Alt+K toggles background sounds unless another app owns that shortcut. Starting KeyTone a second time opens the existing window. Start at sign-in is off initially.

SOUNDS AND PRIVACY
Five additional original procedural presets: Deep thock, Crisp click, Soft tactile, Smooth linear, Muted. They evoke keyboard styles; they are not recordings or emulations of specific branded keyboards. Spacebar and larger keys have distinct resonances. Three sample variations and separate releases are generated locally. Blue Switch, Red Switch and Brown Switch use recorded Cherry MX switches with ABS keycaps from Mechvibes (MIT). See THIRD-PARTY-NOTICES.txt.
No network code, telemetry, account, microphone capture, or stored typing history. Only physical key identifiers are processed transiently. Text in the typing room is not saved. Profile JSON stores key customizations, not typed text.

FILES
App: %LOCALAPPDATA%\Programs\KeyTone
Profiles: %LOCALAPPDATA%\KeyTone\settings.json (plus backup)
Cached maintenance installer: %LOCALAPPDATA%\KeyTone\Installer
Uninstall from Windows Settings > Apps > Installed apps > KeyTone. Exit from the tray first. Uninstall preserves profiles and the cached installer. Do not delete profiles if you want to retain custom imported sounds.

LIMITATIONS
Initial Windows-only release. Native waveOut audio engine with three 5 ms buffers; actual audible latency depends on your audio driver and output. Prefer wired headphones. Bluetooth may lag. Output changes are recovered on device/resume notifications; use Refresh & reconnect output if needed. Does not operate on Windows secure screens. Some games, remote sessions or input software may handle keyboard events differently. Hardware latency and compatibility across other machines have not been certified.

SOURCE / BUILD
Source package includes C# and XAML, build.ps1, tests, manifest, and icon.
Build on Windows with the built-in .NET Framework compiler:
  powershell -ExecutionPolicy Bypass -File .\build.ps1
Run deterministic sound, profile and native playback checks:
  .\build\KeyTone.exe --self-test <absolute-test-output-directory>
The test emits brief sounds and writes test-results.txt and sample WAVs.
Use --data-dir <directory> for isolated manual app testing.
Installer test: KeyTone-Setup.exe --quiet --target <empty-test-directory>
Uninstall test: KeyTone-Setup.exe --quiet --uninstall --target <same-directory>
These target overrides avoid shortcuts and registry changes. Normal --quiet installation installs for the current user without launching. Installer has no autoupdater.

Version 1.2.0, September 2026.

