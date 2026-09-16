# ReplyShelf 1.0 — full AI audit

Audit date: 15 September 2026, Windows 11 x64. The checks below were run against the final source build before packaging. This report is evidence of tested behavior, not a guarantee that every future Windows policy, screen reader, or third-party clipboard manager will behave identically.

## Results

| Suite | Passed | Failed | Evidence |
|---|---:|---:|---|
| Model, search, templating, JSON and file safety | 59 | 0 | audit/engine-results.txt |
| WPF startup, sample, search, editor, copy, filters, import/export and layouts | 17 | 0 | audit/ui-results.txt |
| Per-user installer/update/uninstall/reinstall | 21 | 0 | audit/install-results.txt |
| Package extraction and source rebuild | 5 | 0 | audit/package-results.txt |
| Total recorded checks | 102 | 0 | Repeated checks are not counted twice |

## What the app does

ReplyShelf is a local response shelf for VA, support, and administrative work. It stores reusable plain-text response cards with title, category, shortcut, body, favorite flag, edited timestamp, and named variables. A user can search across all fields, filter by category or favorites, create/edit/duplicate/delete/favorite cards, replace `{{variable}}` placeholders, copy the prepared message, and import/export a JSON library.

The fictional sample contains four responses. The built-in flow was exercised from a clean launch: sample load, response selection, search, variable entry, clipboard copy, favorites-only filter, duplication, invalid save feedback, valid save, JSON export, valid import, malformed-import recovery, and responsive layout checks.

## Defects found and fixed

1. The initial JSON writer used `UTF8Encoding(true).GetBytes`, which does not itself emit a preamble. The writer now writes the UTF-8 BOM explicitly and encodes the JSON body separately. The BOM is asserted in the engine test.
2. Windows alternate-stream paths could raise a framework `NotSupportedException` before the intended validation. Path policy now rejects UNC, alternate-stream, and device-like forms before calling `GetFullPath`.
3. The first WPF audit exposed a missing cancel control and a `DisplayMemberPath`/`ItemTemplate` conflict. The cancel control is now wired to the active cancellation token, and the shelf uses one item template only.
4. The async sample loader initially left `CurrentOperation` completed while work continued. It now returns an awaited task and preserves busy-state handling.

## Feature checks

Search is case-insensitive and checks title, category, shortcut, body, and variable names. Empty searches, no results, Unicode, category filtering, favorites-only filtering, and reset behavior passed. Favorites sort before other results and then alphabetically by title.

Editing validates required title/body, field lengths, variable name characters, unique variable names, variable count, body length, and value length. It preserves line breaks and plain text. Unknown placeholders remain visible. Replacement is exact, case-insensitive token substitution and never executes code. Cloning deep-copies variables and gets a new ID. Delete asks for confirmation.

JSON import is UTF-8/UTF-8 BOM, bounded to 10 MiB and 500 responses, and validates complete records and unique IDs before replacing the current library. JSON export is atomic through a temporary sibling and replacement. Cancellation happens before commit and removes temporary files. Source content is not automatically saved or uploaded.

## Permissions, access rules and security review

- The app and installer use `asInvoker`; installation is per user and does not request administrator access.
- The application reads only a file explicitly selected through a native dialog and writes only an explicitly chosen JSON destination. Normal Windows file permissions apply.
- There is no account, backend, role model, database, access-control rule, API route, CORS surface, or server-side data store in this release.
- Source review found no HTTP client, socket client, browser control, telemetry, API key, token, password, private key, or embedded secret. The only process launch is the installer opening the installed executable after a user-selected install.
- Response text is rendered as WPF text, escaped by the JSON serializer, and never interpreted as HTML, script, shell syntax, or a formula language.
- Clipboard access occurs only after the user presses Copy prepared reply. The app does not monitor clipboard changes.
- UNC/network paths, alternate data streams, linked folders, and unsupported extensions are rejected. A user can still choose a local folder synchronized by another program; that external sync is outside ReplyShelf's control.
- JSON libraries can contain sensitive client communication. The UI states that exports should be handled deliberately. The app has no encryption or team access controls.

## Responsive and accessibility review

Native labels, access keys, automation names, keyboard-focusable controls, visible status text, and wrapped buttons are present. The WPF surface was rendered at 500, 768, 1140, and 1440 pixel widths without horizontal overflow in the outer scroll container. The editor and shelf use flexible star columns, while list content scrolls vertically. A 150%/200% scaling stress check is included in the source audit harness; physical displays were not all available.

The interface uses a light teal surface with dark teal text and orange action emphasis. It avoids icon-only actions and emoji icons. The app is a desktop window, so web routes/pages, browser loading, API failures, CORS, and mobile breakpoints do not apply. Native loading/status and cancellation states were checked.

## Installer and distribution

The installer test performed initial install, payload hash checks, Start menu shortcut and Installed Apps metadata checks, repeat install, locked-file update rejection, rollback verification, self-test from the installed executable, in-use install/uninstall rejection, uninstall while preserving an unrelated sentinel file, shortcut/registry removal, and final reinstall. The portable package and source archive were extracted into paths containing spaces. The source archive rebuilt with the Windows framework compiler, and portable/installed executable hashes matched the distributed binary.

## Remaining validation

- Test on additional physical Windows devices, real mixed-DPI monitor combinations, keyboard-only sessions, screen readers, and actual high-contrast themes.
- Test organizational Smart App Control/EDR policies. The binaries are unsigned and may be blocked; do not disable security settings to work around that.
- Test user-specific clipboard managers and privacy policies.
- Add a regression if a user reports a JSON variant that should be accepted. v1.0 intentionally does not support rich text, Markdown rendering, email sending, help-desk integrations, team permissions, cloud sync, mobile, or web deployment.
- An interrupted storage driver or power loss cannot be made fully transactional by a desktop app. Atomic replacement reduces partial-output risk but cannot guarantee physical media recovery.

Release decision: the recorded checks passed and the package is suitable for a controlled portfolio release within the documented scope. It is not independently security certified and is not claimed to be bug-free on every device.

## September 16 icon and contrast update

Executable and installer now include native icon resources; the application window assigns its icon for taskbar/Alt+Tab display. ClientHub, FormFlow and ReplyShelf use themed closed selection, dropdown and list-item surfaces. UI checks verify actual selected text rendering and 4.5:1 text/token contrast in both themes.

Latest installed checks:
- engine-results.txt: RESULT: 59 passed; 0 failed.
- ui-results.txt: RESULT: 27 passed; 0 failed.
- install-results.txt: RESULT: 21 passed; 0 failed.

Tests ran on this Windows computer. These results do not certify all hardware, DPI combinations, assistive technologies or future inputs. Historic audit sections above retain their original scope; the raw logs in audit are the latest run.

