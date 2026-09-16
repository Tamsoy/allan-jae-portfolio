# FormFlow development flow

## 1. Define the problem

Forms often live in documents, email threads, and task lists. FormFlow creates one local place to see the form purpose, its fields, owner, workflow state, due date, and next action.

## 2. Define the v1 scope

- Form records with title, owner, email, workflow status, due date, field list, notes, and priority flag.
- Search and status/priority filters.
- New, edit, save, duplicate, delete, and copy-summary actions.
- JSON import/export with validation and atomic writes.
- Light/dark mode, responsive scrolling, accessible labels, and loading feedback.
- Per-user Windows installer and portable ZIP.

## 3. Choose the architecture

Use a small native WPF application targeting .NET Framework 4.8. Keep the data model and validation in `Core.cs`, safe file operations in `Files.cs`, UI orchestration in `App.cs`, and audit coverage in `Tests.cs` and `UiAudit.cs`. Avoid network services and runtime dependencies so the app is portable and privacy-friendly.

## 4. Design the interface

Use a minimal glassmorphism system: deep indigo canvas, translucent cards, cyan primary actions, coral accents, rounded 18px cards, soft background orbs, and an icon-only theme toggle. The FormFlow logo combines a form sheet, connected workflow nodes, route arrows, and a completion checkmark.

## 5. Implement the data layer

Validate required titles, email shape, status values, date input, text limits, unique IDs, and maximum record/file counts. Filter case-insensitively across all useful fields. Clone records before import or edit commits so failed operations cannot partially replace the current workspace.

## 6. Implement the UI layer

Build a two-column workspace: searchable form list on the left and an editor plus summary preview on the right. Use dynamic theme resources for both modes. Provide hover, pressed, disabled, accessible, and loading states, an animated theme flash, an indeterminate progress bar, and a subtle busy pulse during file operations.

## 7. Package the app

Compile the WPF executable and a self-contained per-user installer with no administrator requirement. Include the config, README, license, portable ZIP, source ZIP, logo assets, checksums, and audit evidence.

## 8. Verify before launch

Run engine tests for validation, search, cloning, JSON round trips, malformed files, limits, cancellation, and path safety. Run UI tests for cold start, theme switching, accessible names, sample loading, filters, editing, copy/export/import behavior, invalid input, and 500–1440px layouts. Run the installer test for repeat install, locked-file rollback, in-use protection, uninstall preservation, and final reinstall.

## 9. Post-launch maintenance

Keep the schema versioned, preserve import compatibility, review audit results before each release, and add future integrations behind explicit user actions and documented permissions.
