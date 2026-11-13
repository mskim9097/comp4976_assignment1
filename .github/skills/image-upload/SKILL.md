---
name: image-upload
description: How candidate photos and municipality logos are uploaded safely. Use when you add or change a file upload.
---
# Image upload

All uploads go through `src/MEMS.Data/Services/ImageFileStore.cs` (the services call it; pages and controllers only pass the stream).

- `SaveAsync(folder, fileName, contentType, length, stream)` checks: extension (`.png .jpg .jpeg .gif .webp`), declared size, content type matching the extension, the first bytes of the file, and the real size while copying (limit 5 MB, `Limits.ImageMaxBytes`). It throws `RuleViolationException` with a friendly message and removes a partly written file.
- Stored as `<guid>.<ext>` below `Uploads:Path/images/candidates` or `images/logos`; only the file name is saved in `PhotoFileName` / `LogoFileName`.
- `DeleteUploaded` only deletes files with a GUID name, so the seed images in `MEMS.Api/wwwroot/images` are never removed. Old files are deleted after the database change succeeded.
- The Admin app and the Api share the folder through `Uploads__Path` (set by the AppHost). The Api serves it under `/images/...` next to its own `wwwroot`.
- Forms use `InputFile` with `accept`, show the limits, and catch the size exception of `OpenReadStream`.
