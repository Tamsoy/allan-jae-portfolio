# ClientHub development flow

## 1. Define the problem

Client work is often spread across notes, inboxes, and task lists. ClientHub creates one local place to see the relationship, the current status, the next follow-up date, and the context needed for the next action.

## 2. Define the v1 scope

- Client records with name, company, email, status, follow-up date, notes, and priority flag.
- Search and status/priority filters.
- New, edit, save, duplicate, delete, and copy-summary actions.
- JSON import/export with validation and atomic writes.
- Light/dark mode, responsive scrolling, accessible labels, and loading feedback.
- Per-user Windows installer and portable ZIP.

## 3. Choose the architecture

Use a small native WPF application targeting .NET Framework 4.8. Keep the data model and validation in `Core.cs`, safe file operations in `Files.cs`, UI orchestration in `App.cs`, and audit coverage in `Tests.cs` and `UiAudit.cs`. Avoid network services and runtime dependencies so the app is portable and privacy-friendly.

## 4. Design the interface

Use a minimal glassmorphism system: deep indigo canvas, translucent cards, cyan primary actions, coral status accents, 8px spacing rhythm, rounded 18px cards, and a compact icon-only theme toggle. The ClientHub logo uses a central client profile connected to request, deadline, notes, and completion nodes.

## 5. Implement the data layer

Validate required names, email shape, status values, date input, text limits, unique IDs, and maximum record/file counts. Filter case-insensitively across all useful fields. Clone records before import or edit commits so failed operations cannot partially replace the current workspace.

## 6. Implement the UI layer

Build a two-column workspace: searchable client list on the left and an editor plus summary preview on the right. Use dynamic theme resources for both modes. Provide hover/pressed/disabled states, an animated theme flash, an indeterminate progress bar, and a subtle busy pulse during file operations.

## 7. Package the app

Compile the WPF executable and a self-contained per-user installer with no administrator requirement. Include the config, README, license, portable ZIP, source ZIP, logo assets, sample data, checksums, and audit evidence.

## 8. Verify before launch

Run engine tests for validation, search, cloning, JSON round trips, malformed files, limits, cancellation, and path safety. Run UI tests for cold start, theme switching, accessible names, sample loading, filters, editing, copy/export/import behavior, invalid input, and 500–1440px layouts. Run the installer test for repeat install, locked-file rollback, in-use protection, uninstall preservation, and final reinstall.

## 9. Post-launch maintenance

Keep the schema versioned, preserve import compatibility, review audit results before each release, and add any future integrations behind explicit user actions and documented permissions.
