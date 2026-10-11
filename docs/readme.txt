COMP4976 Assignment 1 - Municipal Elections Management System

1. Team members
   - Minsu Kim (A01424659)
   - Justice Plunkett

2. Contributions
   - Minsu Kim:
   - Justice Plunkett:

3. What has not been completed
   - TODO

4. Major challenges encountered
   - TODO

5. Special instructions for testing
   - Requirements: .NET 10 SDK
   - Copy web/appsettings.Development.example.json to web/appsettings.Development.json
   - Set Jwt:Key in web/appsettings.Development.json (any random string of 32+ characters)
   - Run: dotnet run --project web --launch-profile http   (site at http://localhost:5276)
     The SQLite database is created and seeded on the first run (3 municipalities, 9 positions, 18 candidates)
   - Seeded users (password P@$$w0rd): super (Super Admin), pm (Pitt Meadows Admin)
     Log in at /Identity/Account/Login with the user name (an email address works too)
   - The Web API for the iOS app (COMP4977) is part of this project: /api/... and Swagger at /swagger
   - TODO (public site and Admin area testing steps)

6. Deployed application URL (Azure)
   - https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net
     (MVC site at /, Web API at /api/..., Swagger UI at /swagger)
   - The free Azure plan can take 10-30 seconds to answer the first request after a quiet period.

7. GitHub repository
   - https://github.com/mskim9097/comp4976_assignment1
