# SheetClean 1.0

A Windows CSV cleaning utility by Allan Jae Eparwa, developed with AI assistance. It previews original data, trims outer whitespace, flags full-row duplicates and missing required values, optionally removes duplicate/blank rows, and exports a separate cleaned copy with a JSON change log.

## Install and start
Windows 11 x64 and .NET Framework 4.8+ are required. Run SheetClean-Setup.exe for a per-user installation and Start menu shortcut, or extract the portable ZIP and open SheetClean.exe with its config file alongside it. No administrator access, API key, account, network, Python, or Node runtime is required by the app.

The release is unsigned. Windows Smart App Control or organizational policies can block it. Trusted code signing remains a distribution follow-up; do not disable workplace security controls to use it.

## Typical workflow
1. Choose Open CSV, or Load sample for fictional client records.
2. Before opening a file, expand Import options if you need an explicit delimiter, Windows-1252 decoding, or no-header mode. Auto supports comma, semicolon, tab, and pipe, and rejects ambiguous detection rather than guessing. A one-column file with no separators defaults to comma. Options apply to the next import.
3. Review Original. Values stay text: IDs like 001, date strings, and capitalization are preserved.
4. Choose cleaning rules. Trim changes only outer whitespace. Duplicate matching uses every column after trimming, compares case-sensitively, and retains the first row. Removing duplicates or fully blank rows is optional and off by default. Header labels are not trimmed or renamed.
5. Select required columns by clicking or pressing Space. Missing/whitespace-only values are flagged; rows are retained and values are never invented.
6. Analyze changes. Inspect Cleaned, Issues, and Changes. Changing a rule makes the result stale and disables export until you analyze again.
7. Export cleaned copy, then optionally Save change log. CSV/TSV retains the imported delimiter and uses UTF-8 BOM. The log includes original and changed values, so it can contain client data. Neither output is sent anywhere by SheetClean.

Native keyboard access keys: Alt+O open, Alt+S sample, Alt+A analyze, Alt+E export, Alt+L log, Alt+C cancel when an operation is running. Tab/arrows/Space navigate native controls. Close or Clear may ask before discarding an unexported completed analysis. Source and exported files are unaffected by clearing the workspace.

## Supported files and deliberate limits
- .csv and .tsv only; XLSX, macros, workbook formatting, and cloud sheets are unsupported.
- Strict UTF-8 by default; UTF-8 and UTF-16 LE/BE BOM detection. Windows-1252 is an explicit option. UTF-32 and invalid byte sequences are rejected.
- Quoted delimiters, double-quote escaping, multiline quoted fields, CRLF/LF/CR record endings, and empty fields are supported. Records must have the same number of fields. A physically empty line in a multicolumn file is an uneven record; express a blank row with the correct number of delimiters. Malformed quotes and spaces after closing quotes are rejected with an explanation.
- Local regular files only. Network paths/drives, alternate data streams, symlinks, and junctions are rejected; copy data to a normal local folder first.
- Maximum 10 MiB bytes, 10 MiB decoded characters, 50,000 data rows, 100 columns, 250,000 cells including headers, and 32,768 characters per field. Split larger data into files.
- Preview: first 200 data rows, first 40 columns, 120 characters per data cell. Issues/changes: first 500 entries, 200 characters per value. Full analysis and exports include all imported data. Tables scroll horizontally by design. Preview copying copies displayed/truncated text, so use export for complete data.
- Source record numbers count CSV records, not physical lines; a quoted multiline record counts once. With headers, record 1 is the header.
- Column descriptions in issue/change records include a one-based column number and at most 120 header characters; exported CSV headers retain their complete text. Export quoting, UTF-8 encoding, and text prefixes can make an output larger than its input; an output at a size boundary may exceed the import limits when reopened.
- Headers-only input is valid. All data rows can be removed by selected rules; a no-header export with no remaining records is an empty UTF-8 BOM file. It cannot be reimported as a nonempty dataset.

## Spreadsheet text protection
Default protection prefixes formula-like cells with an apostrophe, including headers, values beginning with = + - @, fullwidth variants, initial tabs/newlines, and such markers after leading whitespace/format characters. The preview and change log show these changes. Negative-looking numbers are included deliberately. Existing apostrophe-prefixed values are not prefixed twice.

This is a mitigation, not a universal guarantee across spreadsheet programs or repeated CSV resaves. It changes literal data. If protection is disabled, formula-like cells are flagged and their exact text is exported; use that mode only with an appropriate destination/import process. SheetClean itself never evaluates formulas. Import ID/date columns as text in Excel or another spreadsheet program to prevent that program from converting them.

## Privacy and output safety
No telemetry, backend, saved history, API credentials, automatic uploads, clipboard monitoring, or background service. Data lives in process memory until exported. Logs contain values but not an automatic full source path. Windows controls access to local files using your existing user permissions.

Exports cannot target the imported source path or a detected hard-link alias. Atomic writes preserve an old destination when writing or replacing fails. Cancellation is checked during parsing, analysis, and export before commit. The app cannot guarantee cancellation of a stuck physical storage driver or survival of an abrupt power loss.

Uninstall through Windows Installed apps. Only the app's named files, Start menu shortcut, and uninstall registration are removed; client files and exports are kept. A small cached maintenance executable is retained because it may be running during uninstall.

## Build and tests
From the extracted source directory run `powershell -NoProfile -File .\build.ps1`. This uses Windows' 64-bit .NET Framework C# compiler. No NuGet packages are used. Executable and installer appear in build/.

Developer checks: `SheetClean.exe --self-test <output-directory>` and `SheetClean.exe --ui-audit <output-directory>`. Use Start-Process -Wait -PassThru to inspect exit codes for this GUI executable. Test outputs use fictional fixtures. test-install.ps1 performs real per-user install/uninstall/reinstall tests; close SheetClean first. See AUDIT.md and PROJECT-FLOW.md for coverage and limitations.

Source map: Core.cs (parser/data/cleaning), Files.cs (encoding/local I/O/export), App.cs/Main.xaml (WPF workflow), Tests.cs/UiAudit.cs (checks), Setup.cs (installer). Source license: MIT. No third-party runtime packages or media are bundled.

Optional independent parser check: with Python 3 available, run `powershell -NoProfile -File .\test-oracle.ps1 -Python python`. This compiles the developer-only CsvOracle harness and checks 100 Python-generated CSV fixtures. If native-dialog export fixtures are present in ui-results/, it also verifies those CSV/JSON files.
