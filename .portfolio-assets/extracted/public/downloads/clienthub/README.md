# ClientHub 1.0

ClientHub is an offline Windows workspace for client contacts, requests, follow-up dates, priorities, and notes. It helps a VA, admin assistant, or support coordinator keep the next action visible without a cloud account.

## Install

Run `ClientHub-Setup.exe` for a per-user install and Start menu shortcut, or extract `ClientHub-Portable.zip` and open `ClientHub.exe`. Windows 11 x64 and .NET Framework 4.8+ are required. The unsigned app runs with normal user permissions.

## Workflow

1. Load the fictional sample workspace or create a client.
2. Search by name, company, email, status, follow-up date, or notes.
3. Filter by Lead, Active, Paused, Closed, or priority clients.
4. Edit client details and save them in memory.
5. Copy a clean client summary for a ticket, brief, or handoff.
6. Export a JSON backup when you choose; import validates the complete file before replacing the current workspace.

ClientHub has no login, API keys, telemetry, automatic sync, email sending, or background service. Data remains in memory until you explicitly export it.

## Build and test

Run `powershell -NoProfile -File .\\build.ps1` on Windows with .NET Framework 4.8 developer tools. Run `ClientHub.exe --self-test <folder>` for engine tests and `ClientHub.exe --ui-audit <folder>` for UI tests. `test-install.ps1` performs a real install, update, uninstall, and reinstall lifecycle.
