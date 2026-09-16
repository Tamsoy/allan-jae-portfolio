# SheetClean 1.0 — release audit

Date: 16 September 2026 (Asia/Singapore). Review performed with AI assistance on the available Windows computer. Scope: the Windows x64 desktop application, its installer, portable distribution, and source build. This is an engineering test report, not independent security certification or a guarantee that every bug has been eliminated.

## Recorded results

| Suite | Passed | Failed | Evidence |
|---|---:|---:|---|
| Engine, parser, cleaning, encoding, file safety | 119 | 0 | audit/engine-results.txt |
| WPF workflow, theme animation, accessibility names, layout | 100 | 0 | audit/ui-results.txt |
| Actual per-user install/update/uninstall/reinstall | 21 | 0 | audit/install-results.txt |
| Independent Python CSV oracle and native export verification | 102 | 0 | audit/oracle-results.txt |
| Total distinct recorded checks | 342 | 0 | Repeated runs are not counted again |

The engine, UI, and installer suites were rerun for the 16 September icon fix. The independent CSV oracle evidence remains from 15 September; CSV processing code is unchanged.

The engine suite additionally exercises 300 generated serializer/parser round trips under one recorded assertion. The independent suite supplies 100 different CSV fixtures produced by Python's csv module, rather than relying on SheetClean's own serializer. These cover all four delimiters, quoted and minimally quoted records, header/no-header mode, Unicode, empty values, embedded separators, quotes, CR/LF/CRLF, and text-like dates/IDs.

The WPF suite includes 72 layout combinations: six tabs, four window widths (500, 768, 1140, 1440), and three content scales (100%, 150%, 200%). It checks the surrounding controls for horizontal clipping while allowing intentional scrolling inside tables. Content transforms simulate scale pressure; they are not tests on 12 physical monitor configurations.

## Defects found and addressed

1. The import expander's long label exceeded the available width at narrow/high-scale sizes. Shortened the label; surrounding explanations wrap. All 72 layout combinations subsequently passed.
2. Alternate data stream paths were blocked by the framework with an inconsistent exception before the app's path validation. Added an early explicit path check so these inputs receive the intended local-file error.
3. Long header names could be copied into every issue/change label, greatly increasing memory and log output. Cache bounded display labels with a one-based column index and at most 120 header characters. Actual exported headers remain complete. Added a regression using a maximum-length header.
4. Increased the per-record JSON serializer limit to accommodate a maximally escaped removed-row entry within the input limits. The exporter still streams entries and checks cancellation.
5. Installer failure rollback now restores prior shortcut and uninstall metadata along with changed payload files. The locked-payload rollback path passed; injected registry-ACL failure was not tested.
6. Corrected installer-specific descriptions and quoting in the install test script so developer test paths may contain spaces.
7. Visual inspection found that fixed-height table rows clipped multiline cell previews. Changed rows to grow with their content while retaining a minimum readable height. Rechecked the screenshot, UI suite, and installed payload.

## Active workflow and data correctness

Verified cold start, no-file state, sample import, file loading, rule selection, required-column selection, analysis, all four review tables, stale previews, blocked stale exports, successful CSV/JSON saves, failed imports retaining prior data, failed exports recovering controls, cancellation, duplicate-operation protection, workspace clearing, bounded previews with full analysis, and closing during an active operation.

Parser checks include all documented boundaries: byte/character size, row count, column count, cell budget, field length, malformed quoting, missing/extra fields, invalid delimiter, ambiguous auto detection, empty input, header-only input, no-header mode, BOM handling, bad Unicode, Windows-1252, UTF-32 rejection, and control characters. Errors explain the problem instead of silently repairing an uneven record.

Cleaning checks include outer-only whitespace trimming, source immutability, complete-row duplicate comparison, case sensitivity, first-record retention, structured keys, optional blank removal, required values retained and flagged, original record numbers across multiline input, header preservation, rule snapshots, and cancellation. Duplicate/blank/missing counts reflect the documented rule order: removed blank rows do not also count as duplicates, and missing required values are counted on retained rows. Trim events can precede removal of the same row.

Spreadsheet protection covers leading = + - @ markers, fullwidth variants, leading whitespace/format characters, initial tabs/newlines, negative-looking numbers, headers, already-prefixed values, and explicit raw mode. SheetClean never evaluates any formula. Apostrophe prefixing is a mitigation that changes literal text; it is not a guarantee about Excel or another program after import/resave.

## Native Windows interaction

Launched the installed executable and used real keyboard/mouse input to:

- Open the Windows file picker with Alt+O and import the fictional-client CSV.
- Enable duplicate and blank-row removal and select Email as required.
- Run Analyze with Alt+A and verify five retained rows, two removed rows, two trims, one missing email, and one formula-like value prefixed as text.
- Use Alt+E to save a separate CSV through the native Save dialog.
- Use Alt+L to save a JSON change log through its native dialog.
- Attempt replacement of an unexported analysis; confirm the default No response keeps the result.
- Close the test workspace with the explicit discard confirmation.

Python independently parsed the native CSV and JSON outputs. It verified IDs, trimmed names, retained missing email, formula prefix, multiline text, row counts, selected rules, all five change events, and three issue entries. Only fictional fixtures were used. Native dialog evidence is described here; rendered WPF screenshots in screenshots/ contain the fictional sample.

## Permissions, access rules, and attack surface

| Area requested | Result and boundary |
|---|---|
| Administrative permissions | Tested with a non-elevated Windows token. Both executables use asInvoker. Installation writes current-user app files, Start menu shortcut, and HKCU uninstall metadata. |
| File access | Existing Windows user permissions apply. The app imports only a selected regular local file and writes only an explicitly chosen export. It does not scan client folders. |
| Source preservation | Original-path and hard-link-alias rejection tested. Source bytes remain unchanged after cleaning/export. |
| Output failures | Locked/read-only targets, missing output directory, cancelled writes, injected mid-write failure, overwrite, and orphan temporary-file cleanup tested. Atomic replacement is used after a complete successful write. |
| Input handling | Strict bounded CSV parser. Data is rendered as text and JSON-escaped. CSV values never enter a shell, command line, HTML renderer, or dynamic expression. |
| API keys and secrets | Source review found no embedded credentials, API clients, or external service integration. No API key is required. |
| Accounts and access rules | No account, backend, roles, database, cloud storage, or server endpoints exist. Server authorization/RLS/CORS tests therefore do not apply. Local Windows permissions are the access boundary. |
| Network | App source has no network client, telemetry, upload, or embedded browser. UNC/mapped-network paths and linked paths are rejected. Cloud-sync software independently watching a chosen local folder is outside SheetClean's control. |
| Pages and loading | Native desktop tabs replace web pages/routes. All tabs render; loading, cancellation, empty and failure states are exercised. No remote page or API dependency can fail at startup. |
| Runtime/dependencies | Framework-only C# and WPF; no NuGet runtime libraries, JavaScript packages, Python runtime, or third-party media are shipped in the app. OS/.NET patching remains the device owner's responsibility. |
| Installer | Exact payload allowlist; in-use install/uninstall rejected; updates restore payload on tested failure; unrelated sentinel file survived uninstall. No service, startup task, browser change, firewall change, or security-setting change. |
| Signing | Application and installer are NotSigned. There is no trusted publisher certificate bundled. |

## UI polish pass

The final UI pass applied UI/UX Pro Max guidance for readable contrast, visible native focus behavior, 44px primary controls (38px compact theme button), explicit status copy, semantic native controls, and an 8px spacing rhythm. A 21st.dev search for `glassmorphism data cleaning dashboard` was used for surface and hierarchy inspiration; no web component code was copied into the WPF app. The updated CSV logo remains embedded in the header. A user-controlled night mode, soft background glow, launch fade, and restrained accent pulse were added; the right-aligned compact circular icon-only theme control exposes dynamic accessible names and tooltips. The audit verifies both theme palettes and completed with 100 passed and 0 failed checks, including the installed executable. The 16 September fix centers outlined reference-style icons, animates palette colors, preserves rapid-click intent, checks mid-transition values, and prevents obsolete animation callbacks from overriding a later selection. The nonanimated path was tested without changing Windows settings.

## Release evidence and reproducibility

The delivered source contains build.ps1, test-install.ps1, test-oracle.ps1, test-oracle.py, CsvOracle.cs, and the in-app --self-test / --ui-audit harnesses. Tests write fictional data into explicit audit directories. test-install.ps1 intentionally installs, removes, and reinstalls SheetClean for the current account; close the app first.

Distribution archive integrity and embedded file hashes are recorded separately in audit/package-results.txt. SHA256SUMS.txt identifies the final distribution files. Source rebuilds need Windows' .NET Framework compiler; Python is optional and needed only for the independent oracle suite.

## Known limits and release decision

No failing check remains in the recorded suites. This release is suitable for a local portfolio demonstration and controlled use within the documented CSV scope. It has not been publicly deployed or submitted to an app store.

Remaining validation before broad public distribution:

- Test on additional physical Windows devices, real mixed-DPI monitors, keyboard-only sessions, screen readers, and actual high-contrast themes. High-contrast resource selection is implemented and reviewed, but a full assistive-technology audit was not performed.
- Test a clean Windows account/image and organizational security policies. Unsigned executables may be blocked by Smart App Control or enterprise policy. Trusted code signing is a distribution follow-up.
- Test representative users' spreadsheet import/resave workflows. Spreadsheet programs may convert text IDs/dates or interpret formulas differently. XLSX, macOS, Linux, phones, and tablets are unsupported in v1.0.
- File operations cannot guarantee cancellation of a stuck storage driver or recovery from power loss. Input bounds are intentional; quoting, encoding, or prefixing may produce an export that exceeds the same import limits when reopened.
- No server monitoring, auto updater, background telemetry, or automatic crash upload is included. Reproduce reported failures with a redacted sample, rule selection, encoding/delimiter, app version, and error text; add a regression before releasing a fix.

An AI audit reduces risk through explicit checks and fixes. It cannot prove the absence of all possible defects or replace broader user/device testing.

## September 16 icon and contrast update

Executable and installer now include native icon resources; the application window assigns its icon for taskbar/Alt+Tab display. ClientHub, FormFlow and ReplyShelf use themed closed selection, dropdown and list-item surfaces. UI checks verify actual selected text rendering and 4.5:1 text/token contrast in both themes.

Latest installed checks:
- engine-results.txt: RESULT: 119 passed; 0 failed.
- ui-results.txt: RESULT: 100 passed; 0 failed.
- install-results.txt: RESULT: 21 passed; 0 failed.

Tests ran on this Windows computer. These results do not certify all hardware, DPI combinations, assistive technologies or future inputs. Historic audit sections above retain their original scope; the raw logs in audit are the latest run.

