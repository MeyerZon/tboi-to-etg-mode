# tboi-to-etg-mode
Mod for etg, that will add Isaac, and Isaac gamemode into the game (standard tboi, but all guns and other incompatible rewards are cut) + Isaac items are added

## Status

Milestone M0 (project skeleton): builds and tests run, no gameplay yet.

- [Requirements](docs/REQUIREMENTS.md): what the mod must do, the technical constraints, open decisions and a roadmap.
- [Research notes](docs/research/): source-linked notes on the ETG modding toolchain, the Alexandria / Mod the Gungeon API patterns, and the TBOI and ETG mechanics and IP constraints the mod depends on.

## Stack

Enter the Gungeon v2.1.9 (PC), BepInEx 5.4.21, Mod the Gungeon API 1.9.2, Alexandria 0.5.10, C# targeting .NET Framework 3.5.

## Building

Requires the .NET 8 SDK on any OS. Reference assemblies are pulled from NuGet; no game files are needed to compile.

```bash
dotnet build IsaacMode.sln -c Release   # -> src/IsaacMode/bin/Release/net35/IsaacMode.dll
dotnet test tests/IsaacMode.Tests       # unit tests for the game-agnostic logic
scripts/package.sh                      # Thunderstore zip in artifacts/
```

To test in game, copy `IsaacMode.dll` to `<Enter the Gungeon>/BepInEx/plugins/IsaacMode/` with BepInEx, Mod the Gungeon API and Alexandria installed, then check the F2 console for the load message.

## Installing (players)

Not released yet. The mod will be published on Thunderstore for installation with r2modman, Gale or Thunderstore Mod Manager.

Unofficial fan project. Not affiliated with or endorsed by Nicalis, Inc., Edmund McMillen, Dodge Roll or Devolver Digital.
