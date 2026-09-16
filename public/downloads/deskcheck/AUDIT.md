# DeskCheck 1.0 — release audit

Completed September 15, 2026 (Asia/Singapore). Conducted by the AI development assistant through source review, executable tests, UI/layout tests, browser rendering, and hands-on Windows interaction. This is a development audit, not an independent security certification.

## Release result
The release build compiled with warnings treated as errors, installed under a standard non-elevated Windows user token, opened successfully, and passed the recorded checks. No failing checks remain in the final test logs. The app is suitable for a personal-project preview on the tested Windows environment; broad compatibility has not been established.

| Suite | Passing | Failing | Evidence |
|---|---:|---:|---|
| Engine, validation, reporting, worker integration | 92 | 0 | test-results.txt |
| WPF interface and layout | 91 | 0 | ui-results.txt |
| Per-user installation, rollback, removal | 21 | 0 | install-results.txt |
| Static report browser rendering and contrast | 15 | 0 | browser-results.txt |
| Total automated assertions | 219 | 0 | Individual logs above |

Repeated assertions across the layout matrix are included in this total; it is not a count of 219 distinct features. Repeated runs of the same suite are not added again. Hands-on checks below are separate from the count.

The portable ZIP was extracted into a path containing spaces, its executable hash matched the audited build, and its 92-check engine suite passed. The source ZIP was independently extracted into a path containing spaces, rebuilt with the documented build script, and its rebuilt executable passed the same 92-check suite. These reruns are retained as separate evidence files and are not added to the 219 total.

## UI/UX redesign audit

The final build includes the minimal glassmorphism redesign documented in DESIGN-UPDATE.md. The 21st.dev inspiration search identified “Glassmorphism Minimal Metrics” as the closest reference; only the visual hierarchy was adapted because that catalog component is React/shadcn and DeskCheck is native WPF. The redesigned build was rebuilt with warnings as errors, then rerun through the 92 engine checks and 91 WPF checks. The layout matrix passed at 480, 768, 1140, and 1440 pixel widths with 100%, 150%, and 200% simulated content scale. The updated PC diagnostic logo is embedded as a resolution-independent WPF vector in the header, with the source SVGs included in the source package. Native Windows interaction remains local, read-only, and permission-preserving.

## Features and failure handling
System information, memory availability, fixed local disk space, and network adapter enumeration ran on the development PC. Disk and RAM threshold boundaries, impossible totals, zero capacities, and very large values were tested. Simulated collection access/device errors become Unknown and do not leak exception paths into default reports. The no-active-adapter decision was tested without disconnecting the user's network.

DNS success was exercised with localhost, TCP success with a loopback listener, connection refusal with a closed local port, and DNS failure with a reserved .invalid hostname. A failed ping is explicitly inconclusive. No port range, subnet scan, HTTP request, TLS validation, or application sign-in is performed.

Worker tests cover successful collection, pre-cancellation, cancellation during execution, deadline expiry, nonzero exit, and successful retry after failure. The UI prevents duplicate runs, restores controls, preserves the previous snapshot after cancellation, and waits for worker cancellation during normal window closing. The total parent-enforced collection deadline is 25 seconds, with bounded output and short stream/termination cleanup waits.

Reports passed tests for default omission and explicit inclusion of identifier fields, Unicode, note limits, control characters, exact preview generation, HTML escaping, JSON validation, empty/malformed/oversized outputs, atomic overwrite, locked/read-only destinations, invalid extensions, and missing directories. Failed writes preserve the previous file and clean their temporary file.

## Hands-on installed-app checks
- Opened the installed DeskCheck through its real WPF window.
- Used the Run checks keyboard access key and observed a completed real scan.
- Opened the Report tab and inspected its generated preview.
- Used Save TXT and completed the native Windows save dialog; verified the file was created.
- Used Save HTML and completed its native Windows save dialog; verified the file was created.
- Used Copy report and verified the clipboard exactly matched the saved TXT preview, without printing unrelated clipboard data.
- Opened a save dialog and cancelled it with Escape; the app retained the report and prior status.

The Windows automation helper occasionally returned stale focus/index metadata for the common file dialog. Screenshot-grounded input completed the checks. This was an automation-tool issue, not recorded as a passing app check until the resulting files/status were verified.

## Permissions, access, API keys, and privacy
- Manifest requests asInvoker with uiAccess=false; test token was not elevated. No UAC request or security-setting change was needed.
- Main app reads fixed registry locations and Windows APIs. Installer writes only per-user application files, a Start menu shortcut, and the current user's uninstall registration.
- No API keys, private keys, access tokens, login forms, databases, server endpoints, or authentication rules are required. Source scan found no common embedded-secret patterns. These web/backend audit categories are not applicable to this native local app.
- Reports remain in process memory until explicitly saved or copied. No scan history, telemetry, network upload, keyboard hook, microphone capture, or clipboard monitor is implemented.
- Default UI scan performs local collection only. Outbound DNS/ICMP/TCP probes require the connectivity option. Direct developer --collect host port invocation also explicitly requests probes.
- Hostname/IP/port validation rejects URLs, newlines, shell syntax, ranges, malformed numeric addresses, bracketed/scoped IPs, and overlong input. Networking uses managed APIs. The worker launches the current executable with UseShellExecute=false; user text is never executed as shell code.
- Default exported facts exclude computer names, adapter names, IP/gateway/DNS addresses, and probe destinations. Notes are not automatically redacted, which is stated beside the field and in reports.
- Static HTML encodes all report content, embeds only local CSS, and sets a restrictive Content Security Policy. Browser tests observed no external requests or runtime errors.

## Responsiveness and accessibility
All six tabs were exercised at window widths 480, 768, 1100, and 1440 device-independent units, each with simulated content scales 1.0, 1.5, and 2.0. Tests check both scroll extent and individual visible control bounds. The connectivity inputs were expanded during the matrix. Headers, controls, tab rows, descriptions, and report text wrap; vertical scrolling retains access to content.

These are layout simulations on one PC, not physical DPI/device certification. HTML reports additionally rendered in Edge at 375, 768, and 1440 pixel widths with no horizontal overflow. Main text/button palette contrast measurements range from 5.88:1 to 13.27:1 for the tested pairs. Native controls retain keyboard focus behavior and expose accessible input/action names. The theme maps to Windows system brushes when High Contrast is active; actual high-contrast desktop and screen-reader interaction remain unverified. No decorative animation is used.

## Installer audit
Per-user installation, payload hash equality, Start menu shortcut creation, Installed apps metadata, repeat installation, locked-payload failure, executable rollback, staging cleanup, installed-engine execution, real window startup, in-use install/uninstall rejection, removal, unrelated-file preservation, and final reinstallation all passed. Uninstall deletes an exact file allowlist and removes the directory only when empty; it does not recursively delete the install tree.

If restoring a previous payload fails, installer recovery copies are retained rather than discarded. A small cached maintenance executable remains after uninstall because it may be the running uninstaller. Exported reports are not part of the uninstall allowlist.

## Issues found and corrected during development
1. Added the missing framework UI Automation reference required by accessibility tests.
2. Fixed the screenshot surface background and inspected the resulting native renderings.
3. Replaced a fixed-width connectivity field layout with wrapping/stretching controls and strengthened tests to detect clipped descendants.
4. Added normal-close coordination so the GUI waits for worker cancellation.
5. Bounded redirected worker output during reading, rather than only checking its final size.
6. Moved native save-dialog invocation inside export error handling.
7. Preserved installer recovery copies if rollback itself fails.
8. Corrected the installation test's startup timing and repeated-run sentinel naming. The corrected harness passed with the app opening normally.

## Known limits and remaining pre-public-release work
- Windows 11 x64 on this development PC is the tested platform. Other hardware, ARM emulation, Windows Server, multiple physical monitors with mixed DPI, Narrator, and enterprise policies need a separate test matrix.
- The executable and installer are unsigned (verified as NotSigned). Smart App Control or organizational controls may block them. Obtaining a trusted code-signing certificate is a future distribution step; this release does not bypass those controls.
- Real unplugged devices, a full network outage, a hanging device driver, disk-full hardware, and abrupt power loss were not reproduced. Related validation, error paths, and worker timeout behavior were tested where possible.
- Installer metadata failure recovery and sudden interruption during installation are not a transactional MSI guarantee. Payload rollback was tested; shortcut/registry failures under enterprise policy may require rerunning setup.
- Direct --collect developer mode has no parent watchdog. The normal UI launches it under a bounded parent worker. Force-killing the GUI is not the same as a normal close.
- Clipboard busy errors are handled but were not fault-injected; the normal copy path was verified.
- Diagnostics are observations only: no SMART/drive-health checks, RAM stress test, malware scan, hardware certification, TLS/service validation, benchmark, or automatic repair.
- No audit can guarantee the absence of every bug or future regression. Preserve the source and test scripts and rerun relevant checks after changes.

## Technical references used
- [Microsoft: Ping.SendPingAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping.sendpingasync)
- [Microsoft: TcpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient)
- [Microsoft: .NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)

## September 16 icon and contrast update

Executable and installer now include native icon resources; the application window assigns its icon for taskbar/Alt+Tab display. ClientHub, FormFlow and ReplyShelf use themed closed selection, dropdown and list-item surfaces. UI checks verify actual selected text rendering and 4.5:1 text/token contrast in both themes.

Latest installed checks:
- engine-results.txt: RESULT: 92 passed; 0 failed.
- ui-results.txt: RESULT: 95 passed; 0 failed.
- install-results.txt: RESULT: 21 passed; 0 failed.

Tests ran on this Windows computer. These results do not certify all hardware, DPI combinations, assistive technologies or future inputs. Historic audit sections above retain their original scope; the raw logs in audit are the latest run.

