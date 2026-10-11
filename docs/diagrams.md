# Diagrams

The diagrams are Mermaid (`erd.mmd`, `architecture.mmd`); the PNG files are the exported
images for the project document. GitHub renders the blocks below directly.

## Database ERD

See [erd.md](erd.md) (merged schema for the MVC site and the Web API).

## Architecture

![Architecture](architecture.png)

```mermaid
flowchart LR
    subgraph Browser["Web browser"]
        Site["Public site and Admin area<br/>(MVC views, cookie login)"]
    end

    subgraph Phone["iPhone / iOS Simulator"]
        direction TB
        Views["SwiftUI Views<br/>(Login, Register, Municipalities,<br/>Candidates, Details, Voting Guide, Profile)"]
        VM["ViewModels (MVVM)<br/>@Observable, async/await"]
        Svc["API client<br/>URLSession + Codable"]
        KC[("Keychain<br/>JWT token")]
        Views --> VM --> Svc
        Svc --- KC
    end

    subgraph Azure["Azure App Service (Linux, .NET 10, Canada Central): one ASP.NET Core app"]
        direction TB
        MVC["MVC controllers + views<br/>Admin area (roles: Super Admin,<br/>municipality admin groups)"]
        Auth["JWT Bearer authentication<br/>(/api endpoints)"]
        Ctl["Web API controllers<br/>Auth, Municipalities, Positions,<br/>Candidates, SavedCandidates + Swagger"]
        EF["EF Core (Code First)<br/>ASP.NET Core Identity"]
        DB[("One SQLite database<br/>/home/data/app.db")]
        Img["Static images<br/>wwwroot/images"]
        MVC --> EF
        Auth --> Ctl --> EF
        EF --> DB
    end

    Site -- "HTTPS" --> MVC
    Svc -- "HTTPS + JSON<br/>Authorization: Bearer token" --> Auth
    Svc -. "HTTPS image URLs" .-> Img

    subgraph CI["GitHub"]
        Repo["Repository<br/>web/"] --> Actions["GitHub Actions<br/>dotnet publish + deploy"]
    end
    Actions -- "deploy on push to main" --> Azure
```

The administrators use the MVC site in a browser (cookie login). The iOS app calls the REST API of the same
app over HTTPS: login returns a JWT that the app keeps in the Keychain and sends as a Bearer token for the
voting-guide endpoints. Both parts use the same EF Core model and one SQLite database, and the app also serves
the candidate and logo images. Pushes to `main` that change `web/` are built and deployed by GitHub Actions.

Re-export the PNGs after editing a `.mmd` file (needs Node and Chrome):
`npx @mermaid-js/mermaid-cli -i architecture.mmd -o architecture.png -s 2 -b white`
