COMP4976 Assignment 1 - Municipal Elections Management System
(the same ASP.NET project is also submitted for COMP4977, together with the iOS app in ios/)

1. Team members
   - Minsu Kim (A01424659)
   - Justice Plunkett

2. Contributions
   - Minsu Kim:
     * Repository and solution structure, one ASP.NET project with one SQLite database for the MVC site and the Web API
     * Database design: entities, ERD, EF Core Code First migration, ASP.NET Core Identity, roles ("groups") and the seed data
       (users super and pm, municipalities, positions, candidates)
     * Web API with JWT, Swagger, and the SwiftUI iOS app (COMP4977)
     * Admin area structure and dashboard (counts), candidate details page, positions page, dark mode
     * Azure App Service deployment and GitHub Actions workflow, GitHub Copilot instructions/prompts/skills, documentation
   - Justice Plunkett:
     * Public site: home page with municipalities, candidate directory (search, filter by municipality and position)
     * "Preferred candidates" list (no login) and the printable voting guide
     * Admin screens: Super Admin (municipalities with logo upload, municipality administrator accounts) and
       municipality admin (positions and candidates with photo upload), authorization per municipality
     * Image upload validation and form validation

3. What has not been completed
   - Bonus features other than dark mode (for example pagination, PDF export, candidate comparison, Azure Blob Storage)
   - Images uploaded through the Admin screens are stored in wwwroot; on Azure a new deployment can remove images that were
     uploaded after the last deployment (the seeded sample images are part of the project and are not affected)
   - A candidate that a mobile-app user has saved to their voting guide cannot be deleted (the screen shows a message)
   - TODO (team: add anything else that is missing at submission time)

4. Major challenges encountered
   - The COMP4976 and COMP4977 specifications describe different schemas. After asking the instructor we merged them into one
     schema, one project and one SQLite database; the Web API returns the JSON shape the iOS app expects.
   - The seeded users are "super" and "pm" (user names, not emails), so the default Identity login page (email only) was replaced
     by a login page that accepts a user name or an email address.
   - Azure showed its default welcome page after a deployment because the server still contained files of an older project name
     (two *.runtimeconfig.json files); fixed with the startup command "dotnet MunicipalElections.Web.dll".
   - Keeping the SQLite database on Azure: the connection string points to /home/data/app.db, a folder that survives restarts.
   - TODO (team: add personal challenges, for example Git merge conflicts or working on Mac and Windows)

5. Special instructions for testing
   Requirements: .NET 10 SDK.
   a) Setup
      - Copy web/appsettings.Development.example.json to web/appsettings.Development.json
      - Set "Jwt:Key" in that file to any random string of 32+ characters (needed by the Web API)
      - Run: dotnet run --project web --launch-profile http   (site at http://localhost:5276)
      - The SQLite database is created and seeded on the first run: 3 municipalities, 9 positions, 18 candidates
   b) Seeded users (password P@$$w0rd): super (Super Admin), pm (Pitt Meadows Admin)
      Log in at /Identity/Account/Login with the user name (an email address works too)
   c) Public site (no login)
      - Home (/): municipalities with logo, election date and candidate count
      - Positions (/Positions): positions and seats per municipality, link to the candidates
      - Candidates (/CandidateDirectory): search by name, filter by municipality and position; "View profile" opens the
        candidate details page; "Add to preferred candidates" builds your list (kept in the browser, no login)
      - "Print guide" on the candidate directory prints the selected candidates grouped by municipality (election date,
        position, optional photo, print date); use the browser print preview
      - "Dark mode" button in the top bar switches the theme (the choice is remembered; printing is always light)
   d) Admin area (/Admin, link "Admin" in the top bar after login)
      - Log in as super: dashboard shows all counts; "Municipalities and administrators" lets you add/edit/delete municipalities
        (logo upload) and create municipality administrator accounts; "Positions and candidates" manages everything
      - Log in as pm: the dashboard and lists only show Pitt Meadows; positions and candidates of other municipalities
        cannot be opened, edited or deleted (try /Admin/MunicipalityAdmin/EditPosition/4: not found)
      - Candidate form: name required, valid email and website URL, biography at most 2000 characters, photo must be a
        PNG/JPEG/GIF/WebP image of at most 5 MB (the file picker is limited to images; the type, size and file
        contents are fully checked on the server). Name, email and URL are also checked in the browser.
   e) Web API and Swagger: /swagger (log in with POST /api/auth/login, then use the Authorize button)

6. Deployed application URL (Azure)
   - https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net
     (MVC site at /, Admin area at /Admin, Web API at /api/..., Swagger UI at /swagger)
   - The free Azure plan can take 10-30 seconds to answer the first request after a quiet period.

7. GitHub repository
   - https://github.com/mskim9097/comp4976_assignment1

8. GitHub Copilot files (in the repository)
   - .github/copilot-instructions.md
   - .github/prompts/*.prompt.md (add-admin-crud, add-public-page, change-entity-and-migrate, review-authorization)
   - .github/skills/*/SKILL.md (admin-authorization, image-upload, api-contract-ios)
