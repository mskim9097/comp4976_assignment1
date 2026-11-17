---
name: image-upload
description: How to accept candidate photos and municipality logos safely. Use when adding or changing any file upload.
---
# Image upload

All uploads go through `web/Services/ImageUploadStorage.cs`.

- `SaveAsync(file, webRootPath, "candidates" | "logos")` validates and stores the file and returns the generated file name. It throws `InvalidDataException` with a user-friendly message; catch it and add it to `ModelState` for the file field.
- Checks: extension in `.png .jpg .jpeg .gif .webp`; size at most 5 MB; content type matches the extension; the first bytes match the format (so a text file renamed to `.png` is rejected).
- The stored name is a new GUID plus the extension. The original file name is never used. Files are written to `wwwroot/images/candidates` or `wwwroot/images/logos`; only the file name is stored in `PhotoFileName` / `LogoFileName`.
- Delete old files with `DeleteUploaded(...)` after the database change succeeded (on edit and delete), and delete the new file if saving to the database fails (`try/finally` pattern in `MunicipalityAdminController`).
- Views: file inputs use `accept=".png,.jpg,.jpeg,.gif,.webp,..."`, a form with `enctype="multipart/form-data"`, and show the limits in a help text.
- Uploaded files have GUID names and are git-ignored; the seed images (`c01.png`...`c18.png`, the three logos) are tracked. `app.UseStaticFiles()` serves files added at runtime.
- Known limitation: on Azure the files live in `wwwroot`; a redeployment can remove files uploaded after the last deployment. A persistent folder or Blob Storage would fix that.
