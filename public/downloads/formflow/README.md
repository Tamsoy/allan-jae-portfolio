# FormFlow 1.0

FormFlow is an offline Windows workspace for creating, reviewing, and tracking forms. It keeps form titles, owners, email contacts, workflow status, due dates, field lists, and notes together so the next step is always clear.

## Install

Run `FormFlow-Setup.exe` for a per-user install and Start menu shortcut, or extract `FormFlow-Portable.zip` and open `FormFlow.exe`. Windows 11 x64 and .NET Framework 4.8+ are required. The unsigned app runs with normal user permissions.

## Workflow

1. Load the fictional sample workspace or create a form.
2. Search by title, owner, email, status, due date, fields, or notes.
3. Filter by Draft, In Review, Approved, Archived, or priority forms.
4. Edit form fields and workflow details, then save them in memory.
5. Copy a clean form summary for a brief, review, or handoff.
6. Export a JSON backup when you choose; import validates the complete file before replacing the current workspace.

FormFlow has no login, API keys, telemetry, automatic sync, email sending, or background service. Data remains in memory until you explicitly export it.

## Build and test

Run `powershell -NoProfile -File .\\build.ps1` on Windows with .NET Framework 4.8 developer tools. Run `FormFlow.exe --self-test <folder>` for engine tests and `FormFlow.exe --ui-audit <folder>` for UI tests. `test-install.ps1` performs a real install, update, uninstall, and reinstall lifecycle.
