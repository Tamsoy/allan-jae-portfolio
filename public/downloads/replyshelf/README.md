# ReplyShelf 1.0

ReplyShelf is an offline Windows library for reusable support and VA responses. Search saved replies, filter by category or favorites, edit variables, copy a prepared message, and export/import the library as JSON.

## Install

Run `ReplyShelf-Setup.exe` for a per-user install and Start menu shortcut. No administrator access, account, API key, network, Python, or Node runtime is required. Windows 11 x64 and .NET Framework 4.8+ are required. The binary is unsigned and may be blocked by Smart App Control or workplace policy; do not disable security controls.

`ReplyShelf-Portable.zip` contains the executable and config for extraction. Keep the config beside the executable.

## Workflow

1. Load the fictional sample library or create a response.
2. Search and filter the left shelf. Select a response.
3. Replace variables in the right preview, then copy the prepared message.
4. Edit fields and save, duplicate, favorite, or delete a response.
5. Export a JSON backup. Import validates the complete file before replacing the in-memory library.

Placeholders use `{{name}}`. Unknown placeholders remain visible. Variable substitution is plain text and never runs code. Response content stays in memory until you explicitly export it; there is no automatic cloud sync or telemetry.

## Deliberate limits

Up to 500 responses, 20 variables per response, 32,768 body characters, 160 title/category/shortcut characters, and 10 MiB JSON import size. JSON must contain the ReplyShelf schema. HTML, Markdown rendering, email sending, help-desk APIs, login, and team permissions are outside v1.0.

## Build and test

Run `powershell -NoProfile -File .\build.ps1` on Windows with .NET Framework 4.8 developer tools available. Run `ReplyShelf.exe --self-test <folder>` for the engine suite and `ReplyShelf.exe --ui-audit <folder>` for WPF checks. `test-install.ps1` performs a real per-user install/update/uninstall/reinstall cycle; close ReplyShelf first.

Source map: `Core.cs` model/search/templating, `Files.cs` JSON I/O, `App.cs` workflow, `Main.xaml` layout, `Tests.cs` regression suite, `UiAudit.cs` UI audit, `Setup.cs` installer. MIT license. No third-party runtime packages.
