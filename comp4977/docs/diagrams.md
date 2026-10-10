# COMP4977 - Diagrams

Both diagrams are Mermaid (`erd.mmd`, `architecture.mmd`); the PNG files are the exported
images for the project document. GitHub renders the blocks below directly.

## Database ERD

![ERD](erd.png)

```mermaid
erDiagram
    AspNetUsers {
        string Id PK
        string UserName
        string Email
        string PasswordHash
        string Name
    }
    Municipality {
        int MunicipalityId PK
        string Name
        string Province
        date ElectionDate
        string Description
        string LogoUrl
    }
    Position {
        int PositionId PK
        string Name
        int SeatsAvailable
        int MunicipalityId FK
    }
    Candidate {
        int CandidateId PK
        string Name
        string Biography
        string PhotoUrl
        string Website
        string Email
        int PositionId FK
        int MunicipalityId FK
    }
    SavedCandidate {
        int SavedCandidateId PK
        string UserId FK
        int CandidateId FK
        datetime DateSaved
    }

    Municipality ||--o{ Position : "has"
    Municipality ||--o{ Candidate : "has"
    Position ||--o{ Candidate : "has"
    AspNetUsers ||--o{ SavedCandidate : "saves"
    Candidate ||--o{ SavedCandidate : "is saved as"
```

Relationships and constraints:
- A municipality has many positions and many candidates; a position has many candidates.
- Deleting a position deletes its candidates (cascade). `Candidate.MunicipalityId` is
  restricted (no second cascade path) and always matches the municipality of the position.
- `SavedCandidate` links a user to a candidate (the voting guide). Deleting the user or the
  candidate removes the saved rows. `(UserId, CandidateId)` is unique, so a candidate can be
  saved only once per user.
- `AspNetUsers` is the ASP.NET Core Identity user table (extended with `Name`); the other
  Identity tables (roles, claims, logins, tokens) are created by the Identity migration.
- Database table names are plural (`Municipalities`, `Positions`, `Candidates`,
  `SavedCandidates`); the diagram uses the entity names from the assignment.

## Architecture

![Architecture](architecture.png)

```mermaid
flowchart LR
    subgraph Phone["iPhone / iOS Simulator"]
        direction TB
        Views["SwiftUI Views<br/>(Login, Register, Municipalities,<br/>Candidates, Details, Voting Guide, Profile)"]
        VM["ViewModels (MVVM)<br/>@Observable, async/await"]
        Svc["API client<br/>URLSession + Codable"]
        KC[("Keychain<br/>JWT token")]
        Views --> VM --> Svc
        Svc --- KC
    end

    subgraph Azure["Azure App Service (Linux, .NET 10, Canada Central)"]
        direction TB
        Auth["JWT Bearer authentication<br/>ASP.NET Core Identity"]
        Ctl["Web API controllers<br/>Auth, Municipalities, Positions,<br/>Candidates, SavedCandidates"]
        EF["EF Core (Code First)"]
        DB[("SQLite<br/>/home/data/api.db")]
        Img["Static images<br/>wwwroot/images"]
        Auth --> Ctl --> EF --> DB
    end

    Svc -- "HTTPS + JSON<br/>Authorization: Bearer token" --> Auth
    Svc -. "HTTPS image URLs" .-> Img

    subgraph CI["GitHub"]
        Repo["Repository<br/>comp4977/api"] --> Actions["GitHub Actions<br/>dotnet publish + deploy"]
    end
    Actions -- "deploy on push to main" --> Azure
```

The iOS app (left) calls the REST API over HTTPS. Login returns a JWT that the app keeps in
the Keychain and sends as a Bearer token for the voting-guide endpoints. The API
(right) reads and writes SQLite through EF Core and serves the candidate and logo images.
Pushes to `main` that change `comp4977/api` are built and deployed by GitHub Actions.

Re-export the PNGs after editing a `.mmd` file (needs Node and Chrome):
`npx @mermaid-js/mermaid-cli -i erd.mmd -o erd.png -s 2 -b white`
