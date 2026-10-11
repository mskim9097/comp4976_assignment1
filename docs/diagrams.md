# Diagrams

The diagrams are Mermaid (`erd.mmd`, `architecture.mmd`); the PNG files are the exported
images for the project document. GitHub renders the blocks below directly.

## Database ERD

See [erd.md](erd.md) (merged schema for the MVC site and the Web API).

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
        Repo["Repository<br/>api/"] --> Actions["GitHub Actions<br/>dotnet publish + deploy"]
    end
    Actions -- "deploy on push to main" --> Azure
```

The iOS app (left) calls the REST API over HTTPS. Login returns a JWT that the app keeps in
the Keychain and sends as a Bearer token for the voting-guide endpoints. The API
(right) reads and writes SQLite through EF Core and serves the candidate and logo images.
Pushes to `main` that change `api/` are built and deployed by GitHub Actions.

Re-export the PNGs after editing a `.mmd` file (needs Node and Chrome):
`npx @mermaid-js/mermaid-cli -i erd.mmd -o erd.png -s 2 -b white`
