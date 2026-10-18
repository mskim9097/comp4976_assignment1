COMP4976 Assignment 1 - Municipal Elections Management System
The same ASP.NET project is also submitted for COMP4977 (that zip also contains the iOS app in ios/).

1. Team members
   - Minsu Kim (A01424659)
   - Justice Plunkett (A01405284)

2. Contributions
   Minsu Kim
   - Repository and solution setup: one ASP.NET project and one SQLite database for the MVC site and the Web API
   - Database design: entities, ERD, EF Core migration, Identity, roles and seed data
   - Web API (JWT, Swagger) and the SwiftUI iOS app
   - Admin dashboard, candidate details page, positions page, dark mode
   - Azure deployment, GitHub Actions, Copilot files, documentation
   Justice Plunkett
   - Public site: home, candidate directory (search and filters), preferred candidates list, printable voting guide
   - Admin screens for Super Admin and municipality admins, authorization per municipality
   - Image upload and form validation

3. Not completed
   - Bonus features except dark mode (pagination, PDF export, candidate comparison, Azure Blob Storage)
   - Uploaded images are stored in wwwroot, so a new Azure deployment can remove images uploaded after the last deployment
     (the seeded sample images are part of the project and are not affected)
   - A candidate saved by a mobile app user cannot be deleted (the screen shows a message)

4. Major challenges
   - The two assignment specs have different schemas. After asking the instructor we merged them into one schema,
     one project and one database; the Web API returns the JSON the iOS app expects.
   - The seeded users are "super" and "pm", not emails, so we replaced the default Identity login page with one that
     accepts a user name or an email.
   - After the first deployment Azure showed its default welcome page, because two *.runtimeconfig.json files were on
     the server. Fixed with the startup command "dotnet MunicipalElections.Web.dll".
   - SQLite on Azure: the database is kept in /home/data/app.db so it survives restarts.
   - Working on a Mac and a Windows computer and merging our Git branches.

5. Testing instructions
   Requires the .NET 10 SDK.
   a) Run locally
      - Copy web/appsettings.Development.example.json to web/appsettings.Development.json
      - In that file set "Jwt:Key" to any random text of 32+ characters
      - dotnet run --project web --launch-profile http   (http://localhost:5276)
      - The database is created and seeded on the first run: 3 municipalities, 9 positions, 18 candidates
   b) Seeded users (password P@$$w0rd): super (Super Admin), pm (Pitt Meadows Admin).
      Log in at /Identity/Account/Login with the user name or the email.
   c) Public site (no login)
      - /: municipalities. /Positions: positions and seats.
      - /CandidateDirectory: search, filter by municipality and position, "View profile" for details.
      - "Add to preferred candidates" saves a list in the browser; "Print guide" prints it (use print preview).
      - "Dark mode" button in the top bar.
   d) Admin area (/Admin, "Admin" link after login)
      - super: all counts, municipalities with logo upload, municipality admin accounts, all positions and candidates.
      - pm: only Pitt Meadows. Other municipalities cannot be opened or changed
        (try /Admin/MunicipalityAdmin/EditPosition/4: not found).
      - Photos must be PNG, JPEG, GIF or WebP, max 5 MB.
   e) Web API: /swagger. Log in with POST /api/auth/login, press Authorize and paste the token.
   f) iOS app (COMP4977 zip only): open ios/MunicipalElectionsCompanion in Xcode and run on a simulator.
      It uses the Azure server by default. To use a local server set the environment variable API_BASE_URL
      (for example http://localhost:5276). Create an account on the first screen, then log in.

6. Azure URL
   https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net
   (site at /, admin at /Admin, API at /api, Swagger at /swagger).
   The free plan can take 10-30 seconds to answer the first request.

7. GitHub repository
   https://github.com/mskim9097/comp4976_assignment1

8. GitHub Copilot files
   - .github/copilot-instructions.md
   - .github/prompts/*.prompt.md (add-admin-crud, add-public-page, change-entity-and-migrate, review-authorization)
   - .github/skills/*/SKILL.md (admin-authorization, image-upload, api-contract-ios)
