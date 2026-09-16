# ClientHub pre-launch audit

## Scope

The audit covers permissions, local data handling, input validation, import/export safety, UI states, theme switching, responsive widths, installation, uninstall behavior, and runtime dependencies.

## Security and permissions

- Runs as the signed-in Windows user with `asInvoker` privileges.
- No administrator elevation, account, API key, telemetry, network request, or background service.
- Import accepts only validated UTF-8 JSON under 10 MiB and rejects network/reparse paths.
- Notes and names are length-limited; email and status values are validated.
- Export writes through a temporary file and atomic replace.

## UI and interaction

- Both light and dark semantic palettes are available through an icon-only labeled toggle.
- Theme changes include a short fade feedback; file operations show progress and disable conflicting controls.
- ComboBox fields use an explicit glass template so closed values, arrows, dropdown surfaces, hover, and selected items keep readable contrast in dark mode.
- Search, status, priority filters, editor fields, and summary copy action have accessible names.
- Layout uses a scrollable adaptive two-column workspace and was exercised at 500, 768, 1140, and 1440px widths.

## Evidence

Run `ClientHub.exe --self-test <folder>`, `ClientHub.exe --ui-audit <folder>`, and `test-install.ps1`. The release package includes the resulting text reports and screenshots.

## Release results

- Engine audit: **53 passed, 0 failed**.
- UI audit: **21 passed, 0 failed**.
- Installer lifecycle audit: **21 passed, 0 failed**.
- Installed UI smoke audit: **21 passed, 0 failed**.
- Portable package runtime audit: **53 passed, 0 failed**.
- 21st.dev review: **0 findings** across the XAML and C# UI entry points.

## Post-launch checks

- Cold start opens with an empty workspace and disables editor actions until a client is selected.
- Sample loading, search, status filtering, priority filtering, duplicate, save, delete, copy-summary, export, import, and malformed-import recovery were exercised.
- Theme switching updates semantic resources and accessible toggle labels, with a fade feedback animation.
- Widths of 500, 768, 1140, and 1440px were exercised through the scrollable workspace.
- Reinstall, update, locked-file rollback, in-use protection, uninstall preservation, and reinstall were exercised by the installer test.

## Release verdict

ClientHub is ready for a local Windows 11 x64 release. The app intentionally stays offline and local-only; cloud sync, authentication, email delivery, and multi-device collaboration are outside v1 and would require a new permissions and privacy review.

## September 16 icon and contrast update

Executable and installer now include native icon resources; the application window assigns its icon for taskbar/Alt+Tab display. ClientHub, FormFlow and ReplyShelf use themed closed selection, dropdown and list-item surfaces. UI checks verify actual selected text rendering and 4.5:1 text/token contrast in both themes.

Latest installed checks:
- engine-results.txt: RESULT: 53 passed; 0 failed.
- ui-results.txt: RESULT: 33 passed; 0 failed.
- install-results.txt: RESULT: 21 passed; 0 failed.

Tests ran on this Windows computer. These results do not certify all hardware, DPI combinations, assistive technologies or future inputs. Historic audit sections above retain their original scope; the raw logs in audit are the latest run.

