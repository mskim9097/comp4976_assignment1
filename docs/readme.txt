COMP4976 / COMP4977 - Municipal Elections Management System

1. Project setup
   - The web application is a multi-project .NET 10 solution orchestrated by .NET Aspire.
   - MEMS.Api provides the API and applies database migrations and seed data at startup.
   - MEMS.Admin is the administrative application; MEMS.Public is the public application.
   - MEMS.Data owns the SQL Server EF Core model; MEMS.Shared contains shared API contracts.
   - The SwiftUI client for COMP4977 is in ios/.

2. Requirements
   - .NET 10 SDK, .NET Aspire CLI, and Docker Desktop.
   - Xcode is required to build and run the iOS app.

3. Build and run
   - From the repository root: dotnet build web\MEMS.sln
   - Change directory to web and run: aspire run
   - Use the Aspire dashboard for the application URLs and API documentation.
   - SQL Server data is stored in the mems-sql-data-v2 Docker volume.
   - If prompted, set the SQL password with .NET User Secrets as described in README.md.

4. Seeded administrator accounts
   - Password for all seeded accounts: P@$$w0rd
   - super: Super Admin
   - pm: Pitt Meadows administrator
   - burnabyadmin, surreyadmin, richmondadmin: municipality administrators

5. Deployment
   - The previous Azure App Service deployment workflow has been retired.
   - No deployment is configured for the Aspire application.

6. GitHub repository
   https://github.com/mskim9097/comp4976_assignment1
