---
description: Review Admin area code for authorization, anti-forgery and validation gaps
agent: agent
---
Review the files I name (default: everything under `web/Areas/Admin/`) as a security reviewer for this assignment.

Check each action for:
- `[Authorize]` / role requirement on the controller; `[ValidateAntiForgeryToken]` on every POST.
- Municipality scoping: queries go through `AdminAccess`; ids from the URL or form are re-checked; other municipalities' rows return 404, not data. A Super Admin sees all; a municipality admin only the municipality of their admin role; an iOS app user (no role) sees nothing.
- Over-posting: fields such as `AdminRoleName`, ids and file names are not bound from the form.
- Validation on the server (not only in the browser); uploads checked by `ImageUploadStorage`.
- Error handling: database errors show a message on the form; uploaded files are removed when saving fails.

Report findings as a short list with file and line, severity and a suggested fix. Do not change code unless I ask.
