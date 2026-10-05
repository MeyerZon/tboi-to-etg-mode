# Isaac Mode: project guide for coding sessions

Enter the Gungeon (ETG) mod that adds Isaac from The Binding of Isaac (TBOI) as a Gungeoneer who
shoots tears, an Isaac run mode that removes guns from all loot, and Isaac-style items.
Read `docs/REQUIREMENTS.md` before implementing a feature; requirement IDs (CHR-x, MOD-x, ITM-x,
HLT-x, STA-x) are referenced in code comments and commit messages.

## Stack (do not drift from these versions without updating docs/REQUIREMENTS.md)

- ETG v2.1.9, Unity 2017.4 Mono. Mod targets **.NET Framework 3.5** (`net35`).
- BepInEx 5.4.21, Mod the Gungeon API 1.9.2, Alexandria 0.5.10. Hooking with HarmonyX (prefix/postfix) first, MonoMod `Hook` only when needed.
- Reference assemblies come only from NuGet (`nuget.config` has nuget.org and the BepInEx feed). Never commit or download game DLLs.

## Commands

```bash
dotnet build IsaacMode.sln -c Release          # builds src/IsaacMode -> src/IsaacMode/bin/Release/net35/IsaacMode.dll
dotnet test tests/IsaacMode.Tests              # xunit tests for src/IsaacMode/Core (pure logic)
scripts/package.sh                             # Release build + Thunderstore zip in artifacts/
```

The cloud session installs the .NET 8 SDK through `.claude/hooks/session-start.sh`. The game itself
cannot run here; in-game testing happens on the owner's PC (copy the DLL to `BepInEx/plugins/IsaacMode/`).

## Layout

- `src/IsaacMode/Plugin.cs`: BepInEx entry point. Game init goes in `GMStart`, never in `Awake`/`Start`.
- `src/IsaacMode/Core/`: game-agnostic logic (TBOI formulas, drop tables). **No Unity, BepInEx or game types here**; the test project compiles these files directly on .NET 8.
- `src/IsaacMode/Resources/`: embedded assets, addressed as `IsaacMode/Resources/<folder>/<file>`. Original art only.
- `tests/IsaacMode.Tests/`: xunit.
- `thunderstore/`: `manifest.json` and `icon.png` (256x256). Version must match `ModVersion` in `Directory.Build.props`.
- `docs/`: requirements and research notes.

## Conventions

- Keep all patches guarded on `Plugin.IsaacModeActive` (or the Isaac character identity) so other Gungeoneers are untouched.
- Put TBOI-rule math in `Core/` with a unit test; put ETG wiring (Harmony patches, Alexandria calls) outside `Core/`.
- `.NET 3.5` BCL only in the mod project: no `Task`, no value tuples, no `string.Join(IEnumerable)`, `System.Linq` is available.
- No TBOI sprites, music or sounds anywhere in the repository. Names may be referenced; assets may not be copied.
- Bump `ModVersion` in `Directory.Build.props`, `Plugin.VERSION` and `thunderstore/manifest.json` together; add a `CHANGELOG.md` entry.
