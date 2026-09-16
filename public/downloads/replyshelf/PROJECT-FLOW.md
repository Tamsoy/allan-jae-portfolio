# ReplyShelf 1.0 — detailed development flow

## Product definition
ReplyShelf is a local Windows desktop library for reusable support, VA, and administrative replies. It lets a user find a saved response quickly, edit it safely, replace placeholders such as `{{name}}`, mark responses as favorites, copy a prepared message to the clipboard, and move a library between computers with JSON export/import.

The first release is offline and account-free. It uses C# WPF and .NET Framework 4.8+, targets Windows 11 x64, and does not call a server, require an API key, or upload message content. The product deliberately does not send email, connect to a help desk, or automatically paste into another app.

## User journey
1. Launch into a clear empty or sample-library state. The sample contains fictional support replies.
2. Search by title, category, shortcut, body, or variables. Search updates with a short debounce-free local filter and reports result counts.
3. Narrow by category and Favorites. Select a result to inspect its title, shortcut, body, variables, and last-edited state.
4. Replace visible placeholders in the variable form. Variables are plain text substitutions, never evaluated code. Copy the final message with one action and receive a visible status.
5. Create or edit a response. Require a non-empty title and body, trim only the title/category/shortcut fields, and preserve meaningful line breaks in the body. Save updates in memory and return focus to the library.
6. Toggle favorite, duplicate, or delete. Delete requires a confirmation; an empty library remains usable.
7. Export the library to a new JSON file, or import a JSON file after strict validation. Import replaces the in-memory library only after the complete file is valid.
8. Close without automatic persistence. The interface explains that unsaved in-memory edits disappear, while exported files remain untouched.

## Data contract
`ResponseItem` contains a stable GUID, title, category, shortcut, body, variables (`name`, `value`), favorite flag, and UTC edited timestamp. IDs are regenerated only when a duplicate is created; import preserves valid IDs. Variable names are limited to letters, digits, underscore and hyphen, and values are bounded. Body text is plain text with a 32,768-character limit. Libraries contain at most 500 responses and 20 variables per response.

Search and filtering never mutate records. Copying performs exact placeholder replacement for `{{variable}}`; unknown placeholders remain visible so a user cannot silently send an incomplete reply. JSON is escaped by the framework serializer. No response text enters a shell, HTML renderer, dynamic expression, or network request.

## Implementation phases

Phase A — model and persistence: response model, JSON schema, bounds, validation, clone semantics, import/export with UTF-8 BOM and atomic destination writes.

Phase B — editing behavior: search, category filtering, favorites, selection, variable replacement, copy, new/save/duplicate/delete, and stale-editor protection.

Phase C — WPF interface: teal/orange flat design system, responsive wrapping, two-column desktop layout that stacks at narrow widths, visible labels, keyboard access keys, focus states, live status text, and list/table scroll behavior.

Phase D — distribution: per-user installer, Start menu shortcut, portable package, source archive, README, license, sample library, screenshots, and SHA256 checksums.

Phase E — audit: model tests, import/export round trips, malformed JSON and limit checks, editor state transitions, clipboard behavior, keyboard automation, layout at 500/768/1140/1440 widths and 100/150/200% scale, install/update/uninstall/reinstall, source rebuild from a path containing spaces, and secret/API-key review.

## Audit matrix

- Search: case-insensitive title/category/body/shortcut/variable matching, empty search, Unicode, no results, result count, favorite filter, category filter, and reset.
- Editing: required title/body, long text, multiline text, variable validation, unknown placeholders, save/new/duplicate/delete, cancel edits, selection retention, favorite toggle, and immutable source records.
- Import/export: valid JSON, BOM, escaped text, duplicate IDs, malformed JSON, wrong types, unknown fields, limits, missing fields, atomic writes, locked/read-only destinations, source path protection, cancellation, and Unicode paths.
- UI: cold start, sample load, loading/disabled actions, status messages, editor navigation, clipboard success, delete confirmation, empty library, narrow layout, high scale, keyboard access, accessible names, focus visibility, and every tab/section.
- Security: no network clients, no embedded secrets/API keys, no server/access-control surface, plain text only, local Windows file permissions, no automatic persistence, and no shell/process execution except the per-user installer launching the installed app.
- Distribution: payload equality, repeat install, locked-payload rollback, in-use handling, shortcut/Installed Apps registration, safe uninstall preserving unrelated files, portable execution, and source rebuild.

## Release policy
Fix every discovered defect and rerun the affected checks before packaging. Document the app's local-only boundary, unsigned status, unsupported cloud integrations, and physical-device/assistive-technology limits. An AI audit can reduce risk, but cannot prove that every possible Windows policy, screen reader, or future OS update will behave identically.
