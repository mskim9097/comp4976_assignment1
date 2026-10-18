---
description: Add a public (no login) read-only page with search/filter, in the style of the course MVC examples
agent: agent
---
Add a public page that shows `${input:what}`.

- Use the controller/view pattern of `comp4976/week03/RazorSportsSQLiteMVC` (`Index`, `Details`, `asp-action`/`asp-route-id` links) and of `web/Controllers/CandidateDirectoryController.cs` and `PositionsController.cs`.
- Mark the actions `[AllowAnonymous]` and `[HttpGet]`. Use `AsNoTracking()` and `Include(...)` for related data. Return `NotFound()` when an id does not exist.
- Escape LIKE wildcards in search text (see `CandidateDirectoryController`). Keep filters in the query string so links can be shared.
- Show a clear empty state ("No candidates match these filters").
- Use Bootstrap grid and cards so the page works on phones, and theme-aware classes so it works in dark mode.
- Add a nav link in `Views/Shared/_Layout.cshtml` only if the page is a main section.
- Never put private data (user accounts, other users' saved lists) on a public page.
