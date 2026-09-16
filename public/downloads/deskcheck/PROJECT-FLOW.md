# DeskCheck 1.0 — development and release flow

## Product and scope
An installable Windows utility for help-desk information gathering. The user runs a check, reads understandable findings, and exports a support report. This is a point-in-time diagnostic snapshot, not a repair tool, benchmark, antivirus, or guarantee of overall PC health.

Target: Windows 11 x64, .NET Framework 4.8+. Native WPF and C# with no NuGet dependencies, backend, account, paid API, or secret keys. Use the framework compiler available on Windows to make source builds reproducible.

## User journey
1. Install for the current Windows account or extract the portable ZIP.
2. Open a ready screen explaining exactly what is checked. No automatic outgoing probes.
3. Optionally enable connectivity tests and enter one hostname/IP and TCP port. Defaults: example.com and 443. Explain DNS lookup, one ICMP echo, and one TCP connection; no browsing, uploads, port ranges, or repairs.
4. Select Run checks. Disable duplicate runs, show progress, allow cancellation. Run collection in an isolated child process so blocked Windows calls cannot freeze the UI. Impose a total deadline and terminate that specific worker on cancellation, timeout, or exit.
5. Show a timestamped summary and detailed System, Storage, Network, and Report tabs. Each finding has an explicit status, explanation, and suggested next step. Missing data means Unknown; an unsuccessful ping alone never means the Internet is down.
6. Preview the exact plain-text support report. Device and network identifiers are omitted by default; including them requires an explicit checkbox. Freeform issue notes are optional and visibly identified as user-supplied content.
7. Save a UTF-8 TXT or static HTML report through a native save dialog, or copy the displayed text. HTML output must escape every dynamic field and contain no script, remote resources, or executable links.
8. Close normally. No tray agent, autostart, background service, stored report history, or telemetry. Uninstall from Windows Installed apps; exported reports are preserved.

## Architecture and implementation sequence
Phase 1 — Contracts: define report, facts, findings, statuses, sensitive-field markers, input limits, and disk/memory threshold rules. Write pure evaluators and serializers first.
Phase 2 — Collection: read OS/CPU from fixed registry locations; physical memory through GlobalMemoryStatusEx; uptime via GetTickCount64; inspect fixed local drives only; enumerate network adapters and IPv4/IPv6 addresses. Catch unavailable/denied sources independently.
Phase 3 — Network: strictly parse a single ASCII DNS hostname or IP literal and port 1–65535; reject URLs, shell syntax, ranges, newlines, and malformed numeric addresses. Optional DNS/ICMP/TCP use managed APIs directly, with bounded timeouts. Never concatenate input into a shell command.
Phase 4 — Isolation: launch only the current signed-or-unsigned executable path as a worker, redirect structured output, validate its result, cap output size, and provide total timeout/cancellation. No durable scan file is needed.
Phase 5 — Interface: neutral light workspace, strong navy type, teal actions, clear textual statuses; native controls and focus indicators. Flexible wrapping, vertical scrolling, and a compact layout for smaller Windows screens. No web pages or routes.
Phase 6 — Reports: use one report builder for both preview and export; apply identifier filtering before formatting. Atomic local writes preserve existing reports if a write fails. Use native overwrite confirmation and explicit errors.
Phase 7 — Packaging: build executable/config, portable ZIP, complete source ZIP, and per-user installer with Start menu and Installed apps entries. Never request elevation or alter security settings. Use an exact allowlist for uninstall; preserve unrelated files.
Phase 8 — Audit and fixes: execute tests below, inspect layouts/screenshots, fix failures, rerun affected checks, then package the exact audited build with hashes and an audit report.

## Audit matrix and acceptance criteria
- Core: healthy/warning/unknown boundaries; zero and invalid totals; huge values; no adapters; multiple drives; permission failures; offline operation; no false whole-PC health claims.
- Inputs: empty/long strings, whitespace, URL/shell/HTML payloads, Unicode, IPv4/IPv6, invalid port, report notes length and escaping.
- Runtime: cold start, duplicate scan prevention, cancellation, total deadline, child cleanup, worker failure, malformed worker output, retry after failure, close during scan, disabled controls restored.
- Report: default identifier omission, opt-in inclusion, preview/export parity, text and HTML escaping, Unicode, overwrite, read-only/invalid destination, atomic failure, clipboard failure feedback.
- UI: every tab/action; empty/loading/complete/error states; narrow and wide windows; simulated 100/150/200% scaling; text wrapping, keyboard access, visible focus, textual status, disabled states, contrast.
- Packaging: normal-user manifest, install and payload equality, Start menu/registry entries, rerun install, in-use handling, uninstall preserving unrelated files, portable run, source rebuild.
- Security/privacy: no keys/secrets, no remote dependencies/telemetry, no authentication/database/access rules required, local account filesystem permissions, controlled network opt-in, no shell injection, no machine identifier in default exports, no security setting changes.
- Release: record real test results and remaining risks. Physical multiple-device, screen-reader, and enterprise-policy testing must not be claimed if unavailable. No audit can guarantee every future bug is eliminated.

## Deliverables
DeskCheck-Setup.exe; DeskCheck-Portable.zip; DeskCheck-Source.zip; this flow; user guide; audit results; screenshots; sample redacted reports; SHA256 checksums; build instructions and source license.
