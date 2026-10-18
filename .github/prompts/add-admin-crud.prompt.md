---
description: Add create/edit/delete (CRUD) screens for an entity in the Admin area, with municipality scoping and validation
agent: agent
---
Add CRUD for the entity `${input:entity}` to the Admin area of `web/`.

Follow the existing pattern in `web/Areas/Admin/Controllers/MunicipalityAdminController.cs` and `web/Areas/Admin/Views/MunicipalityAdmin/`:

1. Add a form view model in `web/Areas/Admin/Models/` or `web/Models/` with data annotations (required fields, `StringLength`, format attributes) and friendly `ErrorMessage` texts.
2. Add GET and POST actions for create, edit and delete. Every POST has `[ValidateAntiForgeryToken]`; check `ModelState.IsValid`; trim text input.
3. Scope every query to the municipalities the signed-in admin manages (`AdminAccess.GetManagedMunicipalitiesAsync`). Return `NotFound()` for rows outside that scope. Do not trust ids or foreign keys from the form: re-check them on the server.
4. Add the views (form + list/details), include `_ValidationScriptsPartial`, use Bootstrap classes that work in dark mode.
5. Handle database exceptions (duplicates, foreign keys) with a message on the form instead of an error page.
6. If the entity changes the schema, tell the team first, add one migration, and check the Web API DTOs and `docs/erd.png`.
7. Add the new counts or links to the Admin dashboard when it makes sense.

Finish by listing the manual checks to run: a municipality admin cannot edit another municipality's rows, invalid input is rejected on client and server, anonymous users are redirected to login.
