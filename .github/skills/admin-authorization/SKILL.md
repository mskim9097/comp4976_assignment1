---
name: admin-authorization
description: How administrators are authorized in this project (Super Admin vs municipality admin groups). Use when writing or reviewing Admin area actions.
---
# Admin authorization

## Model
- Groups are ASP.NET Core Identity roles. `Super Admin` (`DbSeeder.SuperAdminRole`) manages everything. Each municipality has its own role named in `Municipality.AdminRoleName`, for example `Pitt Meadows Admin` (built by `DbSeeder.AdminRoleNameFor`).
- Seeded users: `super` (Super Admin) and `pm` (Pitt Meadows Admin), password `P@$$w0rd`. Creating a municipality creates its role; the Super Admin creates municipality admin accounts in `SuperAdminController.CreateAdmin`.
- People who register in the iOS app are normal Identity users without these roles and must get nothing in the Admin area.

## Rules
1. `SuperAdminController` is `[Authorize(Roles = DbSeeder.SuperAdminRole)]`. `DashboardController` and `MunicipalityAdminController` are `[Authorize]` and then check scope.
2. The set of municipalities an administrator manages comes from `AdminAccess.GetManagedMunicipalitiesAsync(User)`. Use it in every query and check. Do not copy the rule into controllers.
3. Find rows through scoped helpers such as `FindManagedPositionAsync` / `FindManagedCandidateAsync`; return `NotFound()` when the row is outside the scope (do not reveal that it exists).
4. For create/edit, re-validate foreign keys from the form (`MunicipalityId`, `PositionId`) against the managed set. Never trust hidden fields.
5. Do not bind `AdminRoleName`, ids or file names from forms (`[BindNever]`).
6. Every POST needs `[ValidateAntiForgeryToken]`.

## Tests to run after changes
- Log in as `pm`: only Pitt Meadows appears; editing or deleting a Maple Ridge position/candidate returns 404; creating a position for another municipality is refused.
- Log in as `super`: everything is visible. Anonymous users are redirected to the login page. An iOS-registered user gets an access-denied redirect on `/Admin`.
