# Project Aura

**Project Aura** is a graph-based thought management system. It provides a robust, SQL-backed structure to manage relationships between thought nodes and constellations, accessed via a fast Minimal API and visualized through a high-fidelity 3D Unity client.

## Architecture

The repository is organized into a mono-repo containing both the server and client applications:

- **`Aura.Core` & `Aura.Api`**: The backend solution. Built with C# and **.NET 7 LTS**. Uses **Entity Framework Core (SQLite)** for persisting the node graph and relationship entities.
- **`Aura.Unity`**: The visual layer. A Unity project (Universal Render Pipeline) that acts as the "Neural Interface" to fluidly navigate and manipulate the node structures.

## Getting Started

### Prerequisites
To build and run Project Aura locally, you will need:
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Unity 2022.3 LTS](https://unity.com/releases/editor/archive) (or newer)
- PowerShell (for the setup script)
- Git

### 1. Setup the Codebases (Important!)
Because the Unity Client needs access to the Core Data Transfer Objects (DTOs) without copying files and risking version drift, we use a NTFS Junction (Symlink) managed by a script.

After cloning the repository, **you must run the setup script** to link the projects together. Git does not check-out junctions by default.

Open PowerShell as Administrator (or ensure Developer Mode is enabled on Windows), navigate to the root directory, and run:
```powershell
.\Link-Dto.ps1
```

> **Note:** This will create a local folder link at `Aura.Unity\Assets\Plugins\Aura.Shared\` that points to `Aura.Core\DTOs\`. This folder is ignored by the Unity `.gitignore` to prevent duplicate tracking.

### 2. Backend Setup (`Aura.Api`)
Next, you need to set up the SQLite database and start the API server.

1. Open a terminal in the root directory.
2. Apply the initial Entity Framework Migrations (if not already applied):
    ```bash
    dotnet tool install --global dotnet-ef  # if not installed
    dotnet build
    dotnet ef database update --project Aura.Core --startup-project Aura.Api
    ```
3. Run the API:
    ```bash
    dotnet run --project Aura.Api
    ```
The API will be available on `localhost` (check `Properties/launchSettings.json` for the exact port).

### 3. Unity Setup (`Aura.Unity`)
With the symlink created and the API running:

1. Open **Unity Hub**.
2. Click **Add** and select the `Aura.Unity` folder.
3. Open the project. Unity will import the link to `Aura.Shared` and treat the `.cs` files natively.
4. Ensure the Universal Render Pipeline (URP) assets are assigned in Project Settings.
5. In the entry scene, verify that the `GraphClient` uses the correct `localhost` port matching your `Aura.Api` runtime.

## Contributing
- **Core Changes**: If you modify or add any models in `Aura.Core/DTOs`, they will instantly appear in the Unity project. 
- **Database Changes**: Remember to add a new Entity Framework migration using `dotnet ef migrations add <Name>` if you alter the database models.
- **Git Hygiene**: `*.db` (SQLite databases) and the `Aura.Shared` Unity proxy directories are intentionally ignored in `.gitignore`. Please keep it that way to avoid binary merge conflicts.
