# SheetClean 1.0 — detailed project flow

## 1. Define the first release
A local Windows desktop utility for VAs, administrators, and support staff who need to clean a CSV without losing leading zeros, changing dates, or uploading client data. Build with C#, WPF, and .NET Framework 4.8+, targeting Windows 11 x64. Provide an installer, portable ZIP, source ZIP, sample data, guide, and audit evidence.

The first release supports delimited text (.csv/.tsv), not XLSX workbooks, formulas, macros, formatting, cloud sheets, or databases. All values remain text. No accounts, API keys, subscriptions, network calls, or automatic uploads. The installer runs for the current Windows account.

## 2. User workflow
1. Open the app and either load the built-in sample or choose a local CSV/TSV file.
2. Choose delimiter (Auto, comma, semicolon, tab, pipe), text encoding (UTF-8/BOM auto or explicit Windows-1252), and whether the first record contains column names. Display detected delimiter, encoding, row count, and column count after import. Changing import options affects the next import, not the already-loaded dataset.
3. Parse strictly. Support quoted delimiters, escaped quotes, empty fields, CRLF/LF/CR record separators, and newlines within quoted values. Reject malformed quotes and uneven record widths with a record/line explanation. Do not silently pad or discard malformed rows.
4. Display original data in a bounded, virtualized table preview. Explain preview limits clearly; analysis and export cover every imported row and column.
5. Configure cleaning: trim outer whitespace; optionally remove fully blank rows; optionally remove duplicate rows, keeping the first. Duplicate matching uses the complete row after trimming and is case-sensitive. Select which columns are required; rows with missing required values are flagged and retained.
6. Analyze asynchronously. Disable conflicting actions, show a busy state, provide Cancel, and keep the last completed result on failure. Changing cleaning rules marks results stale and disables export until analysis runs again.
7. Review Cleaned preview, Issues, and Changes. Show counts for kept/removed rows, changed cells, duplicate rows, blank rows, and missing required fields. Maintain original CSV record numbers for traceability. Do not equate a successful operation with proof of overall data correctness.
8. Export a new CSV/TSV with the chosen delimiter and UTF-8 BOM. Prevent saving over the source, including file aliases where identifiable. Default spreadsheet-safety mode prefixes formula-like cells with an apostrophe; explicitly show that this changes exported values. If disabled, warn that spreadsheet applications may interpret those values. The preview and change log use the same selected safety mode.
9. Optionally export a JSON change log containing rules, counts, source record numbers, changed values, and removal reasons. Explain that logs can contain client data. Never transmit logs.
10. Clear the workspace or close. Nothing is persisted automatically. Exported copies survive uninstall. Prompt before discarding completed unexported analysis.

## 3. Data contracts and safety boundaries
- Import bounds: 10 MiB file bytes, 50,000 data rows, 100 columns, 250,000 total cells, 32,768 characters per field. Enforce during parsing as well as at entry. Limit messages explain how to split the file.
- Default text decoding: strict UTF-8, with UTF-8/UTF-16 BOM detection. Invalid byte sequences produce a useful error; legacy Windows-1252 decoding is an explicit user choice.
- Read only local files; reject UNC/network paths for this release. Lock against concurrent writers during reading. Keep source path/file identity only in process memory.
- Preserve empty cells, column names, leading zeros, date strings, case, inner whitespace, and input ordering. Generate display labels for blank/repeated headers without changing exported header values.
- Headers are never silently renamed or trimmed. Spreadsheet-safety export protection also applies to headers.
- Duplicate keys must be structural and collision-safe, not a delimiter-joined approximation.
- Missing means whitespace-only after the selected trim rule; required fields are flagged, never guessed or filled.
- Use cancellation checks in parsing and cleaning loops. Bound UI previews (200 rows / 40 columns / 120 displayed characters per cell); exact counts and exports cover all data. Long-value full text remains available through issue/change records and exports.
- Atomic writes use a temporary sibling file and replace/move only on successful completion. Native dialogs handle overwrite confirmation. On failure preserve the prior destination and clean temporary files.

## 4. Implementation phases
Phase A: CSV parser/serializer, bounded local file import, BOM/encoding detection, and explicit delimiter detection rules.
Phase B: immutable input dataset, rule options, duplicate equality, required-column validation, cleanup plan, exported-value protection, and traceable change/issue records.
Phase C: export service for CSV/TSV and JSON change log, source-file protection and atomic save. Cancellation before commit leaves the old file intact.
Phase D: WPF workspace with Import, Original, Cleaned, Issues, Changes, and Guide areas. Use native controls, visible labels, focus indicators, textual statuses, and flexible layouts. Data grids may scroll horizontally by design; surrounding controls must remain reachable.
Phase E: per-user installer and portable/source packaging. Reuse the already-tested installer pattern with SheetClean-specific paths and an exact uninstall file allowlist.
Phase F: regression tests, file round trips, adversarial fixtures, UI interactions/layout checks, real dialogs, installation lifecycle tests, source rebuild, and release audit documentation.

## 5. Audit matrix
Parsing: quoted delimiters, escaped quotes, multiline cells, empty cells/files, headers-only files, no-header mode, each delimiter, ambiguous detection, BOMs, strict bad UTF-8, legacy decoding, missing/extra fields, whitespace after closing quotes, control characters, and each resource limit.

Cleaning: identity/no-op, leading zeros/date preservation, outer-only trimming, full-row duplicates after trimming, case sensitivity, delimiter-key collision attempts, duplicate headers, required fields, blank-row options, all rows removed, header preservation, precise source-row mapping, stale-analysis invalidation, cancellation, repeatability, and input immutability.

Export: parser round trips, BOM output, delimiter and header parity, formula-like text (including headers, whitespace prefixes, tabs, fullwidth markers, negative values), safe/explicit-raw modes, JSON escaping, original-file protection, locked/read-only destinations, cancellation, overwrite, long/Unicode paths, and temporary-file cleanup.

UI: every tab and action; initial/loading/loaded/analyzed/stale/error/empty states; repeated operations, import failure preserving old data, clean-result clearing, preview-limit labels, keyboard access, accessible names, high-contrast resource mapping, window resizing, and simulated 100/150/200% content scaling. Physical device and assistive-technology coverage must be reported honestly.

Security: no stored secrets, API clients, remote pages, telemetry, database/access-control surface, formula execution, macros, or automatic client-data storage. Local file permissions remain the Windows user's permissions. Input text must never become code or shell arguments.

Distribution: standard-user installation, payload equality, repeat installation, locked-payload rollback, in-use handling, shortcut/Installed apps registration, removal preserving unrelated files, portable execution, and independent source rebuild from an archive in a path containing spaces.

## 6. Release decision
Fix discovered defects and rerun affected checks. Ship only after the recorded checks pass. Document unsupported formats, deliberate limits, unsigned binaries, untested physical devices/policies, and residual risk. An AI audit cannot guarantee every possible bug or spreadsheet application's interpretation.

## References
- CSV record and quoting conventions: https://www.rfc-editor.org/rfc/rfc4180
- Spreadsheet formula injection risks: https://owasp.org/www-community/attacks/CSV_Injection
