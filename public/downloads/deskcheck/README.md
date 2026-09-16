# DeskCheck 1.0

DeskCheck is a Windows desktop diagnostic utility by Allan Jae Eparwa, developed with AI assistance. It gathers a snapshot of system details, memory availability, local disk space, and network adapter configuration. Optional connectivity tests help investigate a specific destination. Reports explain findings and next steps in plain language.

## Install and use
1. On Windows 11 x64 with .NET Framework 4.8 or newer, run DeskCheck-Setup.exe. It installs for your Windows account with a Start menu shortcut. The portable ZIP can instead be extracted to a writable folder; run DeskCheck.exe alongside its config file.
2. Open DeskCheck and select Run checks. No outbound probes run by default. Review Overview, System, Storage, and Network tabs.
3. Optional: enable connectivity tests, enter one hostname or IP and a TCP port, then run again. Default example.com:443 is a public example destination. These tests contact your configured DNS resolver and the destination, whose operators may see your source IP. A failed ping does not prove loss of Internet access.
4. Open Report, add optional issue notes, and review the preview. Computer names, adapter names, addresses, and probe targets are omitted by default. The inclusion checkbox adds them. Notes are user-supplied and are not automatically redacted.
5. Save TXT, save static HTML, or copy the preview. DeskCheck never sends a report for you. Save cancellation leaves the app unchanged; if saving fails choose a writable local directory.
6. Close the app normally. Remove it through Windows Settings > Apps > Installed apps > DeskCheck. Uninstall preserves exported reports and unrelated files. A small cached maintenance executable is retained under LocalAppData/DeskCheck/Installer.

## Reading results
- Pass: this specific check met its condition, not a whole-PC health certificate.
- Attention: disk space below 10% OR 5 GiB; memory use at least 90%; or no active non-loopback adapter. Review the accompanying explanation.
- Unknown: a device/API could not provide reliable data, a probe timed out, or access was restricted.
- Info: scope/context, or a test that was not requested.

The app checks fixed local drives only. It does not inspect SMART disk health, remove files, repair Windows, scan for malware, measure CPU load, test TLS, or guarantee service availability. TCP checks try one resolved address and one port. Kernel uptime may span shutdowns when Fast Startup is enabled.

## Privacy and permissions
The app runs as the current user and requests no elevation. It does not use API keys, accounts, servers, telemetry, clipboard monitoring, keyboard hooks, or autostart. Results live in process memory until you explicitly save/copy them. Host and port are validated and passed to networking APIs, never a command shell. Connectivity probes are opt-in for each app session. The local scan does not actively probe external hosts; Windows/network drivers may perform their own normal background traffic.

This release is unsigned. Smart App Control or enterprise policies may block it. A trusted signed release is a distribution follow-up; do not disable organizational security controls to use the app.

## Build from source
Run `powershell -NoProfile -File .\build.ps1` from the source directory. The script uses the Windows .NET Framework 64-bit C# compiler. No NuGet restore, Python, Node, API key, or paid service is required. `build/DeskCheck.exe` and `build/DeskCheck-Setup.exe` are produced.

Automated checks: run `DeskCheck.exe --self-test <output-directory>`. UI/layout checks: run `DeskCheck.exe --ui-audit <output-directory>`. Both are developer commands that create local test artifacts. The UI audit uses labeled sample data and temporary test windows. Use PowerShell Start-Process with -Wait -PassThru to inspect exit codes for this GUI executable.

Optional development-only scripts: test-install.ps1 exercises the actual per-user install, uninstall, and reinstall lifecycle; it requires that DeskCheck be closed. test-report.cjs requires Node.js, Playwright, and Microsoft Edge, and reads tests/sample-report.html created by the engine suite. Resolve Playwright through node_modules or set DESKCHECK_PLAYWRIGHT to its installed module directory. None of these tools are required by the shipped app. package.ps1 expects the completed test evidence folders from this development workflow; it is not needed to build or use the application.

`--collect` emits a JSON snapshot to redirected standard output and is used by the app's bounded worker. Adding a validated hostname and port explicitly enables network probes. Developer worker delay/failure modes are only for regression tests.

## Source map
Core.cs: data models, input validation, thresholds, report encoding and atomic writes.
Diagnostics.cs: local data collection, optional probes, child-process timeout/cancellation.
App.cs / Main.xaml: accessible WPF interface and scan/report state.
Setup.cs: per-user installation and allowlisted removal.
SelfTest.cs / UiAudit.cs: regression, integration, and layout checks.
PROJECT-FLOW.md: development plan and acceptance matrix.
AUDIT.md: verification results and remaining coverage gaps (in the release package).

The app has no third-party runtime packages or embedded third-party media. Windows and .NET remain Microsoft components. Source is provided under the included MIT license.
