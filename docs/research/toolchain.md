# Research note: Enter the Gungeon modding toolchain (as of 2026-10-05)

Tags: **[V]** verified against a primary source (fetched page, raw file, registry metadata), **[P]** partially verified, **[U]** unverified (search snippet or secondary source only).

## 1. Loader and library stack

- **BepInEx is the standard loader.** The official wiki states mods are installed with BepInEx via r2modman or Thunderstore Mod Manager. [V] https://enterthegungeon.wiki.gg/wiki/Modding
- **Version: BepInEx 5.4.21**, Thunderstore package `BepInEx-BepInExPack_EtG` v5.4.2101 (2022). Includes unstripped Unity DLLs, a preconfigured console and MonoMod.Loader. [V] https://thunderstore.io/c/enter-the-gungeon/p/BepInEx/BepInExPack_EtG/
  - Every ETG mod/library csproj inspected references `BepInEx.BaseLib 5.4.21` / `BepInEx.Core 5.4.21`. [V]
  - BepInEx 6 (IL2CPP/.NET focus, pre-release) is not used for ETG. [V] https://github.com/BepInEx/BepInEx/releases
- **Mod the Gungeon classic (ETGMod) is officially deprecated.** [V] https://modthegungeon.github.io/ ; old repo https://github.com/ModTheGungeon/ETGMod . `SpecialAPI-Emulate_the_Gungeon` runs most classic mods under BepInEx. [V]
- **Three-layer stack** (verified on the Thunderstore API dump of 458 packages):
  1. `BepInEx-BepInExPack_EtG` 5.4.2101.
  2. **Mod the Gungeon API** `MtG_API-Mod_the_Gungeon_API` v1.9.2 (updated 2025-01-21, about 380 dependants). Provides loot-table injection, string/translation changes, texture replacement, F2 debug console, F3 log, F1 plugin menu, `ETGModMainBehaviour.WaitForGameManagerStart`, `ETGModConsole.Log`. Author SpecialAPI. **MIT**. Plugin GUID `etgmodding.etg.mtgapi`. Repo https://github.com/SpecialAPI/ModTheGungeonAPI ; Thunderstore https://thunderstore.io/c/enter-the-gungeon/p/MtG_API/Mod_the_Gungeon_API/
  3. **Alexandria** `Alexandria-Alexandria` v0.5.10 (updated 2026-08-17, about 88 dependants), depends on MtG API. Repo https://github.com/Nevernamed22/Alexandria (active branch `main`; `master` is stale). Thunderstore https://thunderstore.io/c/enter-the-gungeon/p/Alexandria/Alexandria/ . GUID `alexandria.etgmod.alexandria`. **MIT** per NuGet metadata; no LICENSE file found at repo root. [V]
- **Maintainers**: Alexandria repo owner Nevernamed22; 2026 commits mostly by pcrain (Captain Pretzel). SpecialAPI maintains MtG API. Some Bunny maintains Planetside, Ammonomicon API, Room Architect Tool. Apache Thunder maintains Expand the Gungeon. KyleTheScientist maintains Custom Characters / Custom Rooms. [V]
- Older "GungeonAPI" / "GungeonItems" (KyleTheScientist, MtG-classic era) were absorbed into Alexandria (DungeonAPI is "formerly GungeonAPI"). [V]

## 2. Alexandria modules

- Repo folders (`main`): `Assetbundle, CharApi, CustomDodgeRollAPI, DebuggingTools, DungeonAPI, EnemyAPI, Integrations, ItemAPI, Misc, NPCAPI, NativeResources, NuGet, PrefabAPI, SoundAPI, StatAPI, Testing, VisualAPI, _Deprecated, cAPI` plus `Module.cs`, `Alexandria.csproj`, `Alexandria.nuspec`, `packages.config`, `config.ps1`. [V]
- Thunderstore README lists: SoundAPI, CharacterAPI, DungeonAPI, EnemyAPI, ShopAPI/NPCAPI, ItemAPI, PrefabAPI, TranslationAPI, BreakableAPI, ChestAPI, LightAPI, CustomDodgeRollAPI, AssetbundleAPI. [V]
- ItemAPI files: `ItemBuilder.cs, GunTools.cs, BeamAPI.cs, CompanionBuilder.cs, GenericItemAPIHooks.cs, OrbitalUtility.cs, SlashDoer.cs, ThrowableAPI.cs, TrailAPI.cs, AnimationUtility.cs, AlexandriaTags.cs` + `AdvancedClasses, BasicProjectileComponents, Fakeprefab, ItemInterfaces, SpriteTools, SynergyTools`. [V]
- CharApi: `CharacterBuilding/{CharacterBuilder.cs, CustomCharacter.cs, FoyerCharacterHandler.cs, Loader.cs, SpriteHandler.cs, StringHandler.cs}`, `Tools/`, `CharacterSwitcher.cs`, `CustomCharacterBlackSmithHandler.cs`. [V]
- Misc: 32 utilities incl. `CustomActions.cs, EasyEnumExtender.cs, ILTools.cs, LightAPI.cs, LootUtility.cs, PlayerOverrides.cs (Jul 2026), ChamberGunAPI.cs, RoomRewardAPI.cs, GoopUtility.cs, DebuffUtility.cs, Commands.cs, ExtendedPlayerComponent.cs, ProjectileUtility.cs`. [V]
- **Still updated**: commits Jan–Aug 2026 (0.5.5 → 0.5.10). NuGet `EtG.Alexandria` 0.5.10 published 2026-08-17. [V]
- **No GitHub wiki** for Alexandria; README is one line. De-facto docs are the community GitBook and the source. [V]
- Adjacent libraries: `CaptainPretzel-Gunfig` 1.1.11 (config menus, https://github.com/pcrain/Gunfig), `HellfireJune-JuneLib`, `TeamPlanetside-Ammonomicon_API`, `KyleTheScientist-Custom_Characters_Mod` 2.2.8 (runtime loader for character packs, https://github.com/KyleTheScientist/GungeonCharacters), `KyleTheScientist-Custom_Rooms`, `Alexandria-RoomArchitectTool`. GungeonCraft is a content mod, not an API. There is no competing API library in active use. [V]

## 3. Project setup

- **Engine/runtime**: Unity 2017.4.x, Mono scripting backend (crash dialogs quote 2017.4.3f1; BepInEx 5 Mono-only works). [V/P]
- **Target framework: .NET Framework 3.5 (`net35`)** in Alexandria.csproj, GungeonCraft and the SpecialAPI example mods; all NuGet packages target net35. `LangVersion` up to 13 is used. Old-style csproj + `packages.config` is the community norm. GitBook recommends VS 2022 with the .NET 3.5 Windows feature. [V] https://mtgmodders.gitbook.io/etg-modding-guide/getting-started/setting-up-visual-studio.md
- **Reference assemblies via NuGet**: [V]
  - BepInEx feed `https://nuget.bepinex.dev/v3/index.json`: `EtG.GameLibs` 2.1.9.1 (`lib/net35/Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`, `PlayMaker.dll`; already publicized).
  - nuget.org: `EtG.Alexandria` 0.5.10, `EtG.ModTheGungeonAPI` 1.9.2, `EtG.UnityEngine` 1.0.0 (stripped Unity DLLs), `BepInEx.BaseLib`/`BepInEx.Core` 5.4.21, `HarmonyX` 2.7.0, `MonoMod.RuntimeDetour` 21.12.13.1, `MonoMod.Utils` 21.12.13.1, `Mono.Cecil` 0.10.4, `Newtonsoft.Json` 13.0.1.
  - Alternative: MtG API repo ships publicized/stripped DLLs in `Dependencies/`; Alexandria supports a local `EtG_Data/Managed` path through `config.ps1` → `local.props`.
  - No ETG project found using Krafs.Publicizer or BepInEx.AssemblyPublicizer; publicization is pre-baked. [U/negative]
- **Templates**: https://github.com/SpecialAPI/BepInExExampleMod and https://github.com/SpecialAPI/BepInExExampleModItems (with an Alexandria `ExamplePassive.cs`). Plugin skeleton: [V]
  ```csharp
  [BepInDependency(Alexandria.Alexandria.GUID)]
  [BepInDependency(ETGModMainBehaviour.GUID)]
  [BepInPlugin(GUID, NAME, VERSION)]
  public class Plugin : BaseUnityPlugin {
      public const string GUID = "creator.etg.modname"; public const string NAME = "MOD NAME"; public const string VERSION = "0.0.0";
      public void Start() { ETGModMainBehaviour.WaitForGameManagerStart(GMStart); }
      public void GMStart(GameManager g) { ExamplePassive.Register(); ETGModConsole.Log(...); }
  }
  ```
  Conventions: GUID `creator.etg.modname` lowercase; semver; game-start logic in `GMStart`, not `Awake`. Deploy to `<Gungeon>/BepInEx/plugins/<YourModFolder>/`. [V]
- **Tools**: ILSpy (GitBook "Using IlSpy"), GAER (Gun Animation Editor, JSON/JTK2D), Python Audio Bank Generator (Wwise bypass), Room Architect Tool, MtG API F2 console. UnityExplorer `BepInEx5.Mono` build should work. [V/P]

## 4. Hooking

- ETG is Unity Mono; BepInEx 5 supports Mono only. [V]
- **Alexandria uses HarmonyX**: `Module.cs` does `new Harmony(GUID).PatchAll(Assembly.GetExecutingAssembly())`. Changelog mentions porting hooks to Harmony for cross-mod stability. [V/U]
- **MonoMod RuntimeDetour `Hook`** still taught: GitBook "How to create a hook" (hook method must be static; `Func<>` for return values; iterate the enumerator for coroutines). [V] https://mtgmodders.gitbook.io/etg-modding-guide/misc/how-to-create-a-hook.md
- Guidance: prefer `[HarmonyPatch]` prefix/postfix/transpiler; fall back to MonoMod `Hook`/`ILHook`. Alexandria exposes `EasyEnumExtender.ExtendEnumsInAssembly(GUID)` and `ILTools`. Gotchas: .NET 3.5 BCL, old Mono JIT, `WaitForGameManagerStart` required. [P]

## 5. Distribution

- Thunderstore community https://thunderstore.io/c/enter-the-gungeon/ (458 packages). Mod managers: r2modman 3.2.20, Gale 1.23.1, Thunderstore Mod Manager. [V]
- `manifest.json`: `name` (a-zA-Z0-9_ only), `version_number` (semver), `website_url` (may be empty), `description` (≤250 chars), `dependencies` (`Team-Package-Version`). Zip must contain `manifest.json`, `README.md`, `icon.png` exactly 256×256; `CHANGELOG.md` optional. [V] https://wiki.thunderstore.io/mods/creating-a-package
- Typical dependency strings: `BepInEx-BepInExPack_EtG-5.4.2101`, `MtG_API-Mod_the_Gungeon_API-1.9.2`, `Alexandria-Alexandria-0.5.10`. [V]
- No Steam Workshop for ETG; classic installer path deprecated; consoles cannot be modded. [V]

## 6. Game version and platforms

- Current PC version **v2.1.9** (May 16, 2019). "A Farewell to Arms" was the final content update. `EtG.GameLibs` is versioned 2.1.9.1. [V] https://enterthegungeon.wiki.gg/wiki/V2.1.9
- Enter the Gungeon 2 announced April 2025 (different engine); mobile ports Aug 2025. ETG 1 modding is a stable, frozen target. [V]
- Modding is PC only (Windows; Linux/macOS via BepInEx scripts). [V]

## 7. Prior art: Isaac content in ETG

- Thunderstore scan (regex isaac|tears|binding|basement|repentance|afterbirth|tboi): **no mod adds Isaac as a character or Isaac items on BepInEx**. [V]
- `KyleTheScientist-Static_Camera_Mod` 1.0.1 (room-locked camera "like Isaac"). [V]
- `Morphious86-Reloaded_HUD` 1.1.1 (Isaac-style stats HUD). [V]
- `Nevernamed-Greg_The_Weird_Egg` (reskin referencing an Isaac mod). [V]
- ModWorkshop (classic MtG, abandoned, no source): "The Child (Isaac custom character) WIP" https://modworkshop.net/mod/25290 ; "Isaac Gamemodes" by Kyle https://modworkshop.net/mod/23694 (console commands `isaac`, `eden`, `thelost`). [V]
- Reverse direction (Gungeon content in Isaac) exists on the Steam Workshop. [V]

## 8. Community resources and learning mods

- Discord: "Mod the Gungeon" https://discord.gg/uT7AwbcpyC (about 2,000 members); main game server https://discord.gg/etg has a #modding channel. [V]
- Docs: EtG Modding Guide (GitBook) https://mtgmodders.gitbook.io/etg-modding-guide (installing, creating a mod, uploading, ILSpy, sprites, passives/actives/guns, synergies, custom character, floors, text boxes, audio, asset bundles, hooks, commands, actions, enemies, shaders, ID lists). Official wiki modding page https://enterthegungeon.wiki.gg/wiki/Modding . MtG API wiki (installation, BepInEx migration, enum extension, JSON→JTK2D). [V]
- Open-source mods to learn from (BepInEx, net35; most have no LICENSE file, so all rights reserved by default):
  - GungeonCraft https://github.com/pcrain/GungeonCraft (best modern reference for items, guns, characters, shops, run data).
  - Planetside of Gunymede https://github.com/Some-Bunny/Planetside
  - Once More Into The Breach https://github.com/Nevernamed22/OnceMoreIntoTheBreach
  - Children of Kaliber https://github.com/UnstableStrafe/ChildrenOfKaliber
  - Knife to a Gunfight https://github.com/Skilotar/Knife_to_a_Gunfight
  - Frost and Gunfire https://github.com/Neighborin0/FrostAndGunfire
  - Expand the Gungeon (GPL-3.0) https://github.com/ApacheThunder/ExpandTheGungeon
  - Modular https://github.com/Some-Bunny/Modular (character that cannot use other guns)
  - Libraries with source: Alexandria (MIT), MtG API (MIT), Gunfig, JuneLib, Ammonomicon API, Custom Characters.

## Key takeaways

1. Stack: BepInEx 5.4.21 → MtG API 1.9.2 → Alexandria 0.5.10; Mono/net35; NuGet from nuget.bepinex.dev and nuget.org.
2. Start from `SpecialAPI/BepInExExampleModItems`; GUID `creator.etg.modname`; init in `GMStart`.
3. Hook with HarmonyX primarily; MonoMod `Hook` as needed.
4. Ship on Thunderstore with the three dependencies, a 256×256 icon, README and CHANGELOG.
5. Game is frozen at v2.1.9; no Isaac-content mod exists on BepInEx.
6. Alexandria has no formal docs; rely on the GitBook, the source and the Discord.
