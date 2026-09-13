# 🚀 WLauncher

![WLauncher Banner](https://i.pinimg.com/originals/df/64/a0/df64a03a777dc9f9a060ef6b286773b3.gif)

**Your library of native game recompilations and PC ports, in one launcher.**

WLauncher helps you discover projects, download supported GitHub releases, manage installed versions, and launch your games. Built with **C# and Avalonia**, it offers a desktop library with gamepad navigation, custom artwork, and command-line controls.

[Download WLauncher](https://github.com/Spicywonda/WLauncher/releases) · [Report an issue](https://github.com/Spicywonda/WLauncher/issues) · [Catalog notes](docs/catalog.md)

## What you can do

- **Browse an organized catalog.** Games are grouped by category and sorted alphabetically. Duplicate repositories are combined, including known repository moves.
- **Install and update supported releases.** Choose a download from GitHub, switch versions, or skip an update. Files matching your selected platform appear first in download menus.
- **Keep your own library.** Add repositories, link existing installations, choose an installation directory, and hide entries you do not need.
- **Make it yours.** Switch between grid and list layouts, customize artwork and colors, and use fullscreen or gamepad navigation.
- **Use external projects too.** Entries marked **Manual download** open the project's download page in your browser. Extract the files into the folder shown by WLauncher, then refresh the library.
- **Browse when the network fails.** A failed catalog refresh uses the last valid cached copy. On a fresh installation, bundled entries remain available without a remote catalog.

## Get started

1. Download the archive for your system from [Releases](https://github.com/Spicywonda/WLauncher/releases).
2. Extract the **whole archive** into a writable folder of your choice. Keep the included files together.
3. Open WLauncher, enter the app browser, and add a project to your library.
4. Choose a supported download for your operating system. For external entries, follow the manual installation instructions.
5. Supply the original game's data as required by that project's documentation, then launch it from your library.

WLauncher does not provide ROMs or commercial game data. Each port has its own requirements; a recompilation and a port based on a decompilation may need different versions of the original game.

### Platforms

The release workflow builds these targets:

| System | Architecture | Package |
| --- | --- | --- |
| Windows | x64 | ZIP |
| Linux | x64, ARM64 | TAR.GZ |
| macOS | Intel x64 | ZIP containing an `.app` bundle |

Individual games may support fewer platforms than WLauncher. There is currently no native Apple Silicon release target for the launcher. On Linux, an extracted executable may need executable permission (`chmod +x WLauncher`).

### Download formats

The installer handles `.zip`, `.tar.gz`, `.rar`, `.exe`, and `.AppImage` files. RAR extraction requires a `tar` implementation with RAR support. Packages such as `.dmg`, `.tar.xz`, `.msi`, and Flatpak are not installed automatically; follow the project's instructions for those formats and link the resulting installation. Unsupported packages are not marked as successfully installed.

## Catalog

WLauncher combines the configured [community catalog](https://github.com/SirDiabo/GHLAppList) with a small [bundled additions list](Catalog/additions.json). Your selected games are stored separately in `apps.json`.

Recent additions and corrections include:

| Project | Category | Installation |
| --- | --- | --- |
| [Extreme-G Recompiled](https://gitlab.com/sonicdcer/ExtremeGRecomp/-/releases) | N64 | Manual download from GitLab |
| [Star Fox 64 Recompiled](https://gitlab.com/sonicdcer/Starfox64Recomp/-/releases) | N64 | Updated GitLab location; manual download |
| [Mario Kart 64 Recompiled](https://gitlab.com/sonicdcer/MarioKart64Recomp/-/releases) | N64 | Updated GitLab location; manual download |
| [Duke Nukem: Zero Hour Recompiled](https://gitlab.com/sonicdcer/DNZHRecomp/-/releases) | N64 | Updated GitLab location; manual download |
| [Sonic Mania](https://github.com/RSDKModding/Sonic-Mania-Decompilation/releases) | 2D ports | Supported GitHub release files |
| [DevilutionX — Diablo / Hellfire](https://github.com/diasurgical/DevilutionX/releases) | Other ports | Supported GitHub release files |

The existing Nocturne, Silent Hill: Downpour, Pokémon, and TimeSplitters Rewind additions remain available. Pokémon now points to `bryanthaboi/gen1recomp`. Availability of a release does not establish that every feature or platform works; consult each project's release notes.

See [catalog maintenance and verification notes](docs/catalog.md) to add a project or review the checked releases.

## Your files

Keep backups of these files and folders before moving your installation:

| Location | Purpose |
| --- | --- |
| `apps.json` | Your library, linked paths, artwork URLs, and version preferences |
| `settings.json` | Launcher preferences and optional GitHub API token; keep this file private |
| `Apps/` or your configured folder | Installed games and their local data |
| `Cache/` | Cached artwork and GitHub API data |
| `app_catalog_snapshot.json` | Last valid remote catalog, its source, and its version |

If `apps.json` cannot be read, WLauncher reports the error and preserves the file. Repair it or restore a backup before saving changes.

## Command line

```sh
WLauncher --help
WLauncher --list
WLauncher --run "Game Name"
WLauncher --download "Game Name"
WLauncher --update
WLauncher --update-launcher
```

On Linux, use `./WLauncher`. On Windows, use `WLauncher.exe`. Game commands refer to projects already added to your library.

## Build and test

Install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0), then:

```sh
git clone https://github.com/Spicywonda/WLauncher.git
cd WLauncher
dotnet restore WLauncher.sln
dotnet build WLauncher.sln -c Release
dotnet run --project tests/WLauncher.Tests -c Release
dotnet run --project WLauncher.csproj
```

The regression runner checks catalog organization, repository moves, cache recovery, app-list preservation, and unsupported download handling. It returns a nonzero exit code if a check fails and runs in CI for each host platform.

To publish a self-contained Windows build:

```sh
dotnet publish WLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o publish/win-x64
```

Use the matching host OS for publishing: some dependencies and compile-time features depend on the build host. See [GitHub Actions](.github/workflows) for the other targets.

## Contributing

Bug reports are most useful with your OS, WLauncher version, project repository, and steps to reproduce. For a catalog addition, include its official repository or download page, available platforms, and original-data requirements. Please do not attach tokens or original game files.

WLauncher depends on [Avalonia](https://github.com/AvaloniaUI/Avalonia) and the work of the port and recompilation communities. Game engines, assets, and third-party components retain their respective licenses; the bundled core's license is available in [WLauncher.Core](lib/WLauncher.Core/LICENSE).
