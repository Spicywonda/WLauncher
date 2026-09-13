# Catalog maintenance

WLauncher keeps the browsable catalog separate from the user's installed library:

- `Catalog/additions.json` is embedded in the launcher and contains WLauncher-specific additions.
- The configured GitHub repository supplies the community catalog through a release asset named `apps.json`.
- The writable `apps.json` next to the launcher is the user's library. Do not fill this file with catalog entries in a release.

## Adding a project

Add an entry to a category array in `Catalog/additions.json`:

```json
{
  "N64 Ports": [
    {
      "name": "Project name",
      "repository": "owner/repository",
      "folderName": "ProjectFolder",
      "appIconUrl": ""
    }
  ]
}
```

Use `owner/repository` for GitHub release installation, or an HTTPS download page for the existing manual-download flow. A URL does not enable automatic GitLab updates. `folderName` must be a single directory name, with no separators or traversal components. Artwork is optional; local artwork uses `/Assets/...` paths.

Before adding a project, check its official README, release files, platform support, and original-data requirements. A repository with source code but no downloadable build is not an installable release. Check the community catalog first to avoid duplicating existing projects.

`CatalogService` validates the data, merges category aliases, deduplicates repositories, and sorts categories and titles without regard to case. `WLauncherCatalog` combines bundled additions with remote entries; bundled metadata wins on duplicates. Known repository moves are resolved before deduplication and when loading the library. Existing installation folder names and linked paths are preserved.

The upstream `AI` category distinction is retained as upstream metadata, not converted into an unsupported claim about stability. `Rexglue - Xbox 360` and `Xbox 360 (Rexglue)` share one category. System Emulation continues to be excluded.

## Release checks — 2026-09-12

These versions were checked through the official GitHub/GitLab release APIs. This is a record of availability, not a claim that the games were play-tested.

| Project | Release checked | Result |
| --- | --- | --- |
| [Extreme-G](https://gitlab.com/sonicdcer/ExtremeGRecomp/-/releases/v1.0.0) | `v1.0.0` | Windows, macOS, Linux x64/ARM64 links; new manual entry |
| [Star Fox 64](https://gitlab.com/sonicdcer/Starfox64Recomp/-/releases/v1.0.3) | `v1.0.3` | Release links available; replace obsolete GitHub location |
| [Mario Kart 64](https://gitlab.com/sonicdcer/MarioKart64Recomp/-/releases/v0.9.2) | `v0.9.2` | Release links available; replace obsolete GitHub location |
| [Duke Nukem: Zero Hour](https://gitlab.com/sonicdcer/DNZHRecomp/-/releases/0.0.3) | `0.0.3` | Release links available; replace obsolete GitHub location |
| [Sonic Mania](https://github.com/RSDKModding/Sonic-Mania-Decompilation/releases/tag/v1.1.1) | `v1.1.1` | Windows and Linux ZIP files; requires original game data |
| [DevilutionX](https://github.com/diasurgical/DevilutionX/releases/tag/1.5.5) | `1.5.5` | Windows ZIP and Linux x64 AppImage supported by the installer; other formats may require manual installation |
| [Nocturne](https://github.com/birabittoh/NocturneRecomp/releases/tag/v1.4.5) | `v1.4.5` | Existing entry retained; Windows/Linux release files |
| [Downpour](https://github.com/LittleBitUA/DownpourRecomp/releases/tag/v1.1.8) | `v1.1.8` | Existing entry retained; ZIP release |
| [Gen1Recomp++](https://github.com/bryanthaboi/gen1recomp/releases/tag/v0.2.59) | `v0.2.59` | Updated repository for the previous Pokémon entry |

Banjo Recompiled, Diddy Kong Racing Recompiled, and DK64 Rekongpiled were already in the [community catalog](https://github.com/SirDiabo/GHLAppList/blob/main/apps.json) at review time. They are not counted as new additions. [Kirby 64](https://github.com/Kirby64Ret/Kirby64Recomp) was not added because its README says there are no downloads yet. TimeSplitters Rewind retains its previous manual download link; no new release is claimed here.

## Cache and tests

A remote refresh must pass validation before replacing `app_catalog_snapshot.json`. Source, version, and JSON are replaced together through a temporary file in the same directory. A failed refresh keeps the previous valid snapshot; a different configured repository cannot receive that snapshot. With no usable snapshot, the UI displays the bundled entries and explains that the remote catalog is unavailable. During an offline upgrade using the default catalog setting, a valid legacy `app_catalog_cache.json` can still be displayed. Because old cache files contain no source metadata, this fallback is explicitly labeled as unverified and is never persisted as a source-verified snapshot; an online refresh replaces it. Custom catalog settings do not use the legacy fallback.

Run the deterministic regression checks:

```sh
dotnet run --project tests/WLauncher.Tests -c Release
```

Optionally pass a downloaded community catalog file to validate current upstream compatibility:

```sh
dotnet run --project tests/WLauncher.Tests -c Release -- /path/to/catalog.json
```
