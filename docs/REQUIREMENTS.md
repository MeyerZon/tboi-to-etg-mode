# Requirements: Isaac character, Isaac mode and Isaac items for Enter the Gungeon

Status: draft v0.2 (decisions from the project owner recorded on 2026-10-05; see section 9)
Scope source: the repository description

> Mod for etg, that will add Isaac, and Isaac gamemode into the game (standard tboi, but all guns and other incompatible rewards are cut) + Isaac items are added

This document turns that sentence into concrete, checkable requirements, records the technical facts the mod depends on, and lists the decisions still open. Detailed, source-linked research notes live in [`docs/research/`](research/):

- [Toolchain and ecosystem](research/toolchain.md)
- [API patterns for characters, guns, items, loot and health](research/api-patterns.md)
- [Game mechanics (TBOI and ETG) and intellectual-property constraints](research/mechanics-and-ip.md)

Legend for verification tags used below: **[V]** verified against a primary source (game source, library source, official wiki, package registry), **[I]** inferred design conclusion, **[?]** open question for the project owner.

---

## 1. Product definition

### 1.1 Three deliverables

| ID | Deliverable | One-line definition |
|----|-------------|---------------------|
| D1 | **Isaac, a playable Gungeoneer** | A new character selectable in the Breach. He fires tears from his body instead of using a gun, has Isaac-like stats and starting loadout, and his own sprites, HUD portrait, select card and punch-out sprites. |
| D2 | **Isaac mode, a run modifier** | While Isaac is the active character the run follows TBOI rules where they fit ETG: every loot source (chests, shops, boss pedestals, room-clear rewards) stops producing guns and gun-related rewards and produces Isaac items and Isaac-style pickups instead. Everything else in the Gungeon (floors, enemies, bosses, secrets) stays vanilla. |
| D3 | **Isaac items** | A library of TBOI passive and active items re-implemented on ETG's item system, with original art, so Isaac mode has something to drop. |

### 1.2 Interpretation of the description (assumptions, see §9 for the open points)

- "Standard tboi" means TBOI *rules* applied to ETG *content*: the Gungeon's floors and enemies remain, only the player, the item economy and the reward system change. The mod does not recreate TBOI's floors or enemies. [I]
- "All guns ... are cut" means no gun may be obtained from any source during an Isaac run, and no slot that would normally hold a gun stays empty. Those slots become Isaac items or TBOI-style pickups. [I]
- "Other incompatible rewards" means rewards that only make sense with guns: ammo pickups (full ammo, spread ammo), gun-only passives (for example clip-size, reload-speed, ammo-capacity items, ammolets that depend on blanks if blanks are removed), gun-specific synergies, and gun merchants' gun stock. [I]
- "Isaac items are added" means the items are registered with the game and drop **only** in Isaac runs. They never appear for other Gungeoneers. Vanilla ETG items that still make sense without guns keep dropping in Isaac runs alongside them (decided, Q2 and Q5).

---

## 2. Platform and technical constraints

| Constraint | Value | Tag |
|------------|-------|-----|
| Game | Enter the Gungeon v2.1.9 ("A Farewell to Arms", May 2019). The game is frozen; no newer patch exists. PC only (Steam, Epic, GOG). Consoles and mobile cannot be modded. | [V] |
| Engine / runtime | Unity 2017.4.x, Mono scripting backend. IL2CPP is not involved. | [V] |
| Mod loader | BepInEx **5.4.21**, distributed as Thunderstore package `BepInEx-BepInExPack_EtG` 5.4.2101 (ships unstripped Unity DLLs and MonoMod.Loader). BepInEx 6 is not used by the ETG community. | [V] |
| Required libraries | `MtG_API-Mod_the_Gungeon_API` 1.9.2 (plugin GUID `etgmodding.etg.mtgapi`, MIT) and `Alexandria-Alexandria` 0.5.10 (GUID `alexandria.etgmod.alexandria`, MIT, still actively updated as of Aug 2026). | [V] |
| Target framework | .NET Framework **3.5** (`net35`). Modern C# syntax is fine via `LangVersion latest`, but no BCL newer than 3.5 (no `Task`, no tuples without polyfill, limited LINQ). | [V] |
| Reference assemblies | NuGet: `EtG.GameLibs` 2.1.9.1 from `https://nuget.bepinex.dev/v3/index.json` (already-publicized `Assembly-CSharp.dll`), plus `EtG.Alexandria`, `EtG.ModTheGungeonAPI`, `EtG.UnityEngine`, `BepInEx.BaseLib`/`BepInEx.Core` 5.4.21, `HarmonyX` 2.7.0, `MonoMod.RuntimeDetour` 21.12.13.1 from nuget.org. The game's own DLLs must never be committed or redistributed. | [V] |
| Hooking | HarmonyX (`Harmony.PatchAll`) is primary; MonoMod `Hook`/`ILHook` for the few cases Harmony handles badly (coroutines, IL edits). | [V] |
| Deprecated path | Classic "Mod the Gungeon" (ETGMod) is officially deprecated and must not be targeted. | [V] |
| Distribution | Thunderstore community `enter-the-gungeon`, installed through r2modman, Gale or Thunderstore Mod Manager. There is no Steam Workshop for ETG. | [V] |

---

## 3. Development environment requirements

| ID | Requirement | Priority |
|----|-------------|----------|
| ENV-1 | C# project targeting `net35`, producing a single plugin DLL with `[BepInPlugin]`, `[BepInDependency(ETGModMainBehaviour.GUID)]` and `[BepInDependency(Alexandria.Alexandria.GUID)]`. Start from `SpecialAPI/BepInExExampleModItems` (old-style csproj + `packages.config` + `nuget.config` with both feeds). Plugin GUID follows the `creator.etg.modname` convention. | Must |
| ENV-2 | All game-start initialisation runs in a callback registered with `ETGModMainBehaviour.WaitForGameManagerStart`, not in `Awake`. | Must |
| ENV-3 | Reference assemblies come only from NuGet (ENV table above) or from a developer-local `EtG_Data/Managed` path configured through an untracked props file. `.gitignore` excludes `bin/`, `obj/`, `packages/`, `*.user`, local props. | Must |
| ENV-4 | Assets (PNG sprites, `characterdata.txt`, gun `.jtk2d` metadata, `.bnk` sound banks) are shipped as **EmbeddedResource** inside the DLL. Resource folder layout follows Alexandria's `Namespace/Folder/file` convention (slashes become dots). | Must |
| ENV-5 | Tooling available to contributors: ILSpy (or dnSpy) for reading `Assembly-CSharp`, GAER (gun animation / jtk2d editor) and `gen-gungeon-audio-bank.py` from `pcrain/gungeon-modding-tools`, an pixel-art editor exporting per-animation PNG frame folders (Aseprite workflow), optionally Unity 2017.4.40 for prebaked asset bundles. | Should |
| ENV-6 | A build on a non-Windows machine (CI) is desirable. Options to evaluate: `dotnet build` with `Microsoft.NETFramework.ReferenceAssemblies.net35` on an SDK-style csproj, or Mono `msbuild`. The community itself builds with Visual Studio 2022 on Windows with the .NET 3.5 feature enabled; CI is not a blocker for v0.1. | Could |
| ENV-7 | Local test loop: copy DLL to `<Gungeon>/BepInEx/plugins/<ModFolder>/`, verify in the F2 console. Provide debug console commands (`isaac mode`, `isaac give <item>`, `isaac hearts ...`) through `ETGModConsole.Commands`. | Should |
| ENV-8 | Repository license: MIT for code (ecosystem norm; MtG API, Alexandria, ETGMod are MIT). Art and audio under a separate notice (fan work, not covered by the code license). See §8. | Must |

---

## 4. Functional requirements

### 4.1 D1: Isaac the Gungeoneer

| ID | Requirement | Tag | Priority |
|----|-------------|-----|----------|
| CHR-1 | Isaac is registered through Alexandria CharacterAPI (`Loader.BuildCharacter` with an embedded resource folder containing `characterdata.txt`, or the "manual" `CharacterBuilder` route GungeonCraft uses). He gets his own `PlayableCharacters` enum value via `ETGModCompatibility.ExtendEnum`. | [V] | Must |
| CHR-2 | Base prefab to clone is chosen for closest body shape/animation set (candidates: `bullet` or `convict`; Robot only if armor-only health is chosen, see HLT-x). | [I] | Must |
| CHR-3 | Starting loadout: the Tears "gun" (CHR-5) flagged `infinite`, one Isaac-style bomb active (TBOI: Isaac starts with 1 bomb) and optionally The D6 as a Could-have unlock. No vanilla gun, no vanilla passive. | [V] TBOI stats | Must |
| CHR-4 | Base stats translated from TBOI Repentance Isaac: 3 red hearts (`Health` 3), speed 1.0 (ETG `MovementSpeed` tuned to feel like Isaac, exact value to be playtested), tears 2.73/s (`RateOfFire` on the Tears gun), damage 3.5 (`Damage` scaled to ETG enemy health, see MEC-2), range 6.5 tiles (`RangeMultiplier`), shot speed 1.0 (`ProjectileSpeed`), luck 0 (custom stat, STA-2). | [V] source values, [I] scaling | Must |
| CHR-5 | **Tears weapon**: an ETG `Gun` built with `ItemBuilder.BuildGun`, quality `EXCLUDED`, `InfiniteAmmo`, `CanBeDropped=false`, `PreventStartingOwnerFromDropping`, `PersistsOnDeath`, `preventVolleyModifications` as appropriate. Fires a tear projectile; fire mode `Automatic` with cooldown derived from tear delay. It never appears in loot. | [V] | Must |
| CHR-6 | The gun sprite is invisible (1x1 transparent idle sprite plus matching hand points) or the player is set handless so tears visibly come from Isaac. Ammo HUD is hidden or replaced via Alexandria `CustomAmmoDisplay`. | [I] no API exists | Must |
| CHR-7 | Tear projectile: full TBOI behaviour (decided, Q-tears). Tears spawn at tear height, fall so they land at `Range`, size scales with damage (`PlayerBulletScale`), knockback from shot speed, `shouldRotate=false`. Arc implemented with `Projectile` velocity modifiers or a custom component. Hit VFX reuse a vanilla `ProjectileImpactVFXPool` recoloured until tear-splash art exists. | [V] components, [I] arc | Must |
| CHR-8 | Isaac cannot acquire guns: prefix patch on `Gun.Pickup` (Modular's pattern) and on `LootEngine.GivePrefabToPlayer`, plus `GunInventory.GunLocked.SetOverride("isaac", true)`. Any gun that still reaches the floor is converted to an Isaac item drop (safety net; MOD-7). | [V] hooks | Must |
| CHR-9 | Full animation set delivered as PNG frame folders under `newspritesetup/` named exactly like Alexandria's `playerAnimInfo` clips (`idle`, `run_*`, `dodge*`, `death*`, `doorway`, `item_get`, `pitfall*`, `slide_*`, `spinfall`, `ghost_*`, `jetpack_*`, `pet`, `tablekick*`, `chest_recover`, `select_*` and their `_hand`/`_twohands` variants). Plus `foyercard/`, `loadoutsprites/`, `punchout/`, `icon.png`, `facecard.png`, `bosscard_*.png`, `win_pic*.png`, `coop_page_death.png`. Until real art exists every sprite is a flat coloured square (decided, Q7). | [V] | Must (placeholders first, see §10) |
| CHR-14 | **Asset source switch**: all sprites load through one asset loader that first looks in an external folder next to the DLL (`BepInEx/plugins/IsaacMode/assets/`, same layout as `Resources/`) and falls back to the embedded placeholders. The external folder is where the owner places TBOI sprite sheets extracted from their own copy of the game. Those files are never committed and never shipped in the Thunderstore package (see §8). A converter tool (`tools/`, Python) turns TBOI `.anm2` + sheet exports into ETG frame folders. | [I] | Must (M1) |
| CHR-10 | Strings: name, short name, nickname ("The Child"? to decide) registered through CharacterAPI string keys; Ammonomicon entries for Tears and items via MtG API `StringDB`. English first; other languages optional through `GungeonSupportedLanguages` overloads. | [V] | Must / Could |
| CHR-11 | Breach presence: character select stand with prerequisites empty (always unlocked) in v0.1; alt costume (`newaltspritesetup/`) is a Could. Co-op: Isaac must at least not crash when chosen by player 2; parity for the Cultist slot is a Could. | [V] | Must / Could |
| CHR-12 | Mid-game save and elevator reload must restore Isaac correctly (CharacterAPI handles identity; custom health state from HLT-x must be serialised via a run-data carrier item, GungeonCraft `CwaffRunData` pattern). | [V] pattern | Must |
| CHR-13 | Voice/SFX: original tear-fire, tear-splash, hurt and death sounds in a `.bnk` generated with `gen-gungeon-audio-bank.py`, loaded by `SoundManager.LoadSoundbanksFromAssembly`, wired through gun switch groups. No TBOI audio. | [V] pipeline | Should |

### 4.2 D2: Isaac mode

| ID | Requirement | Tag | Priority |
|----|-------------|-----|----------|
| MOD-1 | Activation: Isaac mode is **on** whenever the primary player's `characterIdentity` is Isaac, decided once per run in Alexandria `CustomActions.OnRunStart` and re-checked on reload. A Gunfig toggle may later allow Isaac mode for other characters or disable it for Isaac. | [V] hook, [?] toggle | Must |
| MOD-2 | **Single loot-table swap**: Harmony prefix on `RewardManager.GetItemForPlayer` and on `LootData.GetItemForPlayer` that replaces `GunsLootTable`/`ItemsLootTable` with the mod's `IsaacItemsTable` (a `GenericLootTable` built with `LootUtility`). This covers chests, shop group-2 slots, boss pedestals, `SpawnTotallyRandomItem` and most other mods' calls. | [V] | Must |
| MOD-3 | Chests: per-floor treasure-room chest pair (vanilla: one gun chest + one item chest) both yield Isaac items. Chest tiers D–S keep working because Isaac items carry ETG qualities (ITM-3). Rainbow/synergy/glitch chests are handled (rainbow: `GameStatsManager.IsRainbowRun` already filters via the swapped table). | [V] | Must |
| MOD-4 | Boss reward pedestal: `CustomActions.OnRewardPedestalDetermineContents` fills `overrideItemPool` from the Isaac table, also neutralising `IsBossRewardForcedGun`. TBOI flavour: boss drops come from a "Boss pool" subset (stat-ups) when available. | [V] hook, [I] pool | Must |
| MOD-5 | Shops: Bello's group-2 slots are covered by MOD-2. Sub-shops (Trorc, Flynt, Cursula, Goopton, Old Red) draw from their own tables: replace gun stock in `CustomActions.OnShopItemStarted` (force out of stock + spawn replacement) or prefix `BaseShopController.DoSetup` to swap `shopItems`. Gun-only consumables (ammo) are replaced by TBOI pickups (hearts, keys, bombs, batteries). | [V] hooks | Must |
| MOD-6 | Room-clear rewards: `RoomRewardAPI.OnRoomRewardDetermineContents` replaces the vanilla pickup roll with the TBOI room-clear table (nothing 22%, card/pill/trinket 8%, coin 15%, heart 15%, key 20%, bomb 15%, chest 5%, luck-weighted). Ammo and blank drops are removed. | [V] table | Must |
| MOD-7 | Safety net: any `Gun` instance that still spawns on the floor (other mods, edge paths, seeded runs) is swapped for an Isaac item at the same position via `LootEngine.SpawnItem`; seeded runs patch `RewardManager.GetItemForSeededRun` too. | [V] | Must |
| MOD-8 | Vanilla ETG items **do** drop in Isaac runs, mixed into the Isaac table at their ETG qualities (decided, Q5), except a maintained exclusion list of items that only affect guns or removed systems: clip/reload/ammo/gun-capacity items, gun-switching items, Ammolets and other blank items, dodge-roll items, and items that grant blanks. | [I] | Must |
| MOD-9 | Active item charging: Isaac actives use ETG `PerRoom` cooldown type (TBOI room-clear charge), with +2 charge for large rooms as a Could. ETG `Damage`-based charging is not used for Isaac items. | [V] types | Must |
| MOD-10 | TBOI-style item pedestals and room rewards may be realised with `overrideFunctionPool` to spawn pedestal items directly instead of chests (Could; chests are an acceptable ETG-flavoured substitute). | [V] | Could |
| MOD-11 | Nothing in Isaac mode may alter save flags, unlocks or Hegemony credit flow for other characters. Mode state is per run only. | [I] | Must |
| MOD-12 | Mode must not break Boss Rush, shortcuts, Turbo, Challenge, Rainbow or Blessed modes. Blessed mode (gun cycling) is incompatible with Isaac and is disabled or made a no-op for him. | [I] | Should |
| MOD-13 | **No blanks, no dodge roll** for Isaac (decided, Q3). Blanks start at 0, are never restocked, and blank pickups are removed from Isaac loot; the dodge-roll input is disabled or replaced through Alexandria `CustomDodgeRollAPI`. | [V] APIs | Must |
| MOD-14 | **Adaptation prompt**: on entering the first floor of an Isaac run, a yes/no prompt asks whether to "adapt to the Gungeon". Yes grants a fixed set of TBOI-style substitute items chosen to compensate for the lost roll and blanks (the exact items are the owner's call, to be decided; candidates include a Holy Mantle style per-room shield and a Book of Shadows style active). No grants nothing. The answer is stored in run data (CHR-12) and never asked again that run. Implementation candidates to verify in source: ETG's existing confirmation dialog UI; fallback is a shrine-style interactable in the starting room offering the items. | [I] | Must (M2) |
| ECO-1 | **Isaac economy**: enemies drop no shells in Isaac runs (`MoneyMultiplierFromEnemies` 0 or a drop patch). Coins come only from the room-clear table, chests, and shop stock, as in TBOI. | [I] | Must (M2) |
| ECO-2 | **Shop prices rescaled** to the Isaac income rate (decided, Q5): a global price multiplier for Isaac runs so a treasure-tier item costs about what a TBOI shop item costs relative to income (15 coins against roughly one coin drop per 6 rooms). Tuned from a spreadsheet of expected coins per floor; applies to Bello and the sub-shops. | [I] | Must (M2) |

### 4.3 D3: Isaac items

| ID | Requirement | Tag | Priority |
|----|-------------|-----|----------|
| ITM-1 | Items are built with Alexandria `ItemBuilder.BuildItem<T>` (passives derive from `PassiveItem`, actives from `PlayerItem`), with original Ammonomicon sprites (rule of thumb up to 30x30 px, 16 px = 1 world unit, no pure black at edges). | [V] | Must |
| ITM-2 | Items are **removed from the vanilla loot tables** after registration (`LootUtility.RemovePickupFromLootTables`) or registered as `EXCLUDED`, then added only to the mod's Isaac table. MtG API otherwise auto-adds every new item to `ItemsLootTable` with weight 1. | [V] | Must |
| ITM-3 | TBOI item quality 0–4 maps to ETG quality D/C/B/A/S (0→D, 1→C, 2→B, 3→A, 4→S) so ETG's floor-based chest tier odds keep working. | [I] | Must |
| ITM-4 | Stat items use `AddPassiveStatModifier` with the TBOI→ETG stat mapping in §6; stats with no vanilla equivalent (Luck, tear height) use Alexandria StatAPI custom stats. | [V] | Must |
| ITM-5 | Tear-effect items map to vanilla projectile components: homing (`HomingModifier`), piercing (`PierceProjModifier` / `AdditionalShotPiercing`), bouncing (`BounceProjModifier` / `AdditionalShotBounces`), spectral (pass through obstacles; needs custom collision flags), status tears (poison, slow, fear, burn, freeze, charm) via ETG `GameActorEffect`s with luck-scaled proc chance. | [V] components, [I] mapping | Must |
| ITM-6 | Multi-shot items (20/20, The Inner Eye, Mutant Spider) modify the Tears gun's `ProjectileModule` (projectile count, spread, fire-rate multiplier, 16-tear cap). Brimstone and Technology become module swaps to `Charged`/`Beam` shoot styles using ETG's `BeamController`. Mom's Knife is a Could. | [V] fields, [I] design | Should |
| ITM-7 | Actives: signature set (The D6 → reroll pedestal/chest items in the room; Book of Belial; Yum Heart; The Bible; Bomb) using `PerRoom` or `Timed` cooldowns; batteries as pickups that add charge. | [I] | Should |
| ITM-8 | Pools: metadata per item lists its TBOI pools (Treasure, Shop, Boss, Devil, Angel, Secret, ...). v0.1 uses Treasure/Shop/Boss only; Devil and Angel deals require new rooms and are out of scope until DungeonAPI work is planned. | [I] | Should |
| ITM-9 | Synergies between Isaac items use `CustomSynergies.Add` (for example Inner Eye + 20/20 removes the fire-rate penalty). Vanilla gun synergies never fire in Isaac mode. | [V] | Could |
| ITM-10 | Initial curated set for v0.1 (about 25–40 items): stat-ups (Sad Onion, Pentagram, Magic Mushroom, Cricket's Head, Blood of the Martyr, Lunch, Dinner, Belt, Growth Hormones, Jesus Juice, Steven), tear flags (Spoon Bender, Cupid's Arrow, Ouija Board, Rubber Cement, Polyphemus), multishot (20/20, Inner Eye, Mutant Spider), health (Yum Heart, Raw Liver), actives (D6, Book of Belial, The Bible, Bomb). Familiars and room-generation-dependent items are deferred. | [I] | Must |
| ITM-11 | Pickups: TBOI-style hearts (red half/full) reuse cloned vanilla `HealthPickup` prefabs with new sprites; keys and coins reuse `KeyBulletPickup`/`CurrencyPickup` with Isaac art; bombs are a stackable consumable active; batteries charge the active. Soul/black hearts depend on HLT-3. | [V] classes | Should |
| ITM-12 | Item names: reuse TBOI item names as plain references (weakly protected), never TBOI icons or sprites (§8). Item tips via `AddItemTip` when the ItemTips mod is present. | [V] | Must |

### 4.4 Health system

| ID | Requirement | Tag | Priority |
|----|-------------|-----|----------|
| HLT-1 | v0.1 uses vanilla ETG health: red hearts = `HealthHaver` half-heart health, armor = soul hearts (armor already absorbs exactly one hit and is lost first). HUD armor sprites are re-skinned blue via a `GameUIHeartController.UpdateHealth` postfix. Armor's free blank on break is disabled for Isaac if blanks are removed. | [V] mechanics, [I] mapping | Must |
| HLT-2 | TBOI damage sizes: regular hits = half heart (same as ETG). Jammed enemies deal a full heart in ETG, which mirrors TBOI chapter 4+ full-heart damage; no change needed. | [V] | Must |
| HLT-3 | Later: faithful soul/black/eternal/bone hearts through a per-player counter component, `HealthHaver.ModifyDamage` interception and a custom HUD (`ToolsCharApi.AddUISprite`). No existing ETG mod implements this; it is new work and must be serialised for mid-game saves. | [I] | Could |
| HLT-4 | Max health cap 12 hearts as in TBOI; ETG HUD layout must be checked above 10 hearts. | [V] TBOI cap | Should |
| HLT-5 | Invulnerability frames stay ETG default. The dodge roll is removed for Isaac (MOD-13); compensation comes only through the adaptation prompt (MOD-14). | decided | Must |

### 4.5 Stats and formulas

| ID | Requirement | Tag | Priority |
|----|-------------|-----|----------|
| STA-1 | Tears: fire rate = 30 / (tearDelay + 1); tear delay from tears stat T: 16 − 6·sqrt(T·1.3 + 1), capped at delay 5 (5 tears/s) from stat-ups; multipliers bypass the cap. Implemented by computing the Tears gun cooldown from a custom "tears" stat, then letting ETG `RateOfFire` multipliers apply. | [V] formula | Must |
| STA-2 | Luck: custom StatAPI stat, clamped 0–10 for room-drop weighting, unclamped for tear-effect proc chance. Not mapped to Coolness (Coolness also cuts active cooldowns). | [V] | Must |
| STA-3 | Damage: (3.5 · sqrt(dmgUps·1.2 + 1) + flat) · multipliers; the ×1.5 items (Magic Mushroom, Cricket's Head, Blood of the Martyr with Belial) do not stack with each other. Final value multiplied by a global ETG scaling constant to be tuned (ETG enemies have much more HP than TBOI's). | [V] formula, [I] scaling | Must |
| STA-4 | Range in tiles → `RangeMultiplier`; ETG rooms are larger than TBOI's, so a base range above 6.5 ETG units is expected after playtesting. Shot speed 1.0 = 7.5 tiles/s → `ProjectileSpeed`. | [V] values, [I] tuning | Must |

### 4.6 UI, configuration and debugging

| ID | Requirement | Tag | Priority |
|----|-------------|-----|----------|
| UI-1 | Optional Isaac-style stat readout (tears, damage, range, shot speed, luck) on the HUD or in the pause menu. Prior art: `Morphious86-Reloaded_HUD`. | [V] prior art | Could |
| UI-2 | Config menu through `CaptainPretzel-Gunfig` (soft dependency): mode toggles, vanilla-items-in-Isaac-mode, blanks on/off, dodge roll on/off, Isaac items for other characters. | [V] | Should |
| UI-3 | F2 console command group `isaac` for debugging (toggle mode, give item, set hearts, reroll). | [V] | Should |

---

## 5. Non-functional requirements

| ID | Requirement |
|----|-------------|
| NFR-1 | **Compatibility**: patches must be Harmony prefix/postfix where possible, keyed on "is Isaac mode active" so other characters are untouched. Known mods to test against: GungeonCraft, Once More Into The Breach, Planetside of Gunymede, Modular (also patches `Gun.Pickup`), Custom Characters Mod, Jolly Co-op. |
| NFR-2 | **Performance**: all sprite packing and item registration happens once at game start; no per-frame reflection; tear projectiles reuse ETG pooling. |
| NFR-3 | **Stability**: no exceptions in the F2 console on load, character select, floor transition, mid-game save/load, co-op join, death, victory. |
| NFR-4 | **Packaging**: Thunderstore zip with `manifest.json` (`name` without spaces, semver `version_number`, `description` ≤ 250 chars, `dependencies` `MtG_API-Mod_the_Gungeon_API-1.9.2`, `Alexandria-Alexandria-0.5.10`, optionally `BepInEx-BepInExPack_EtG-5.4.2101`), `README.md`, `CHANGELOG.md`, `icon.png` exactly 256×256. Only the mod's own DLL and assets are shipped. |
| NFR-5 | **Legal hygiene**: no TBOI sprites, music or sound effects; no game DLLs; disclaimer of non-affiliation with Nicalis, Edmund McMillen, Dodge Roll and Devolver Digital; package name avoids the "The Binding of Isaac" trademark (character name "Isaac" as plain reference is acceptable). |
| NFR-6 | **Documentation**: README with install, dependencies, feature list, item list and credits; CHANGELOG per release; this requirements document kept in sync with scope changes. |

---

## 6. TBOI → ETG mapping reference

### 6.1 Stats

| TBOI | ETG `PlayerStats.StatType` | Notes |
|------|----------------------------|-------|
| Speed | `MovementSpeed` | additive in ETG |
| Tears | `RateOfFire` (on Tears gun) | via STA-1 |
| Damage | `Damage` | plus global scaling constant |
| Range | `RangeMultiplier` | |
| Shot speed | `ProjectileSpeed` | affects knockback |
| Luck | custom StatAPI stat | no vanilla equivalent |
| Tear size | `PlayerBulletScale` | cosmetic in TBOI |
| Piercing / bouncing | `AdditionalShotPiercing` / `AdditionalShotBounces` | or projectile components |
| Schoolbag (2 actives) | `AdditionalItemCapacity` | Pilot already has 2 slots |
| Devil-deal / "evil" items | `Curse` | ETG Jammed odds, flavour only |
| Price items (Steam Sale) | `GlobalPriceMultiplier` | |
| Boss damage (e.g. Hornfel-like) | `DamageToBosses` | |

The full verified `StatType` list (31 members) is in [api-patterns.md](research/api-patterns.md) and [mechanics-and-ip.md](research/mechanics-and-ip.md).

### 6.2 Systems

| TBOI concept | ETG counterpart used | Fidelity |
|--------------|---------------------|----------|
| Red hearts | `HealthHaver` health (0.5 steps) | exact |
| Soul hearts | Armor (v0.1), custom counter (later) | approximate → exact |
| Black / eternal / bone / rotten hearts | custom (HLT-3) | later |
| Room-clear charge | `PlayerItem` `PerRoom` cooldown | exact |
| Timed active | `Timed` cooldown | exact |
| One-use active | consumable `PlayerItem` | exact |
| Treasure room item | locked treasure-room chest (one per floor) | approximate (TBOI: free pedestal on floor 1, key from floor 2) |
| Shop (15¢ items) | Bello with price multiplier; coins = shells | approximate |
| Boss item (Boss pool) | boss `RewardPedestal` | exact shape |
| Room-clear drop table | `RoomRewardAPI` override | exact table |
| Keys / bombs / coins | ETG keys / new consumable / shells | approximate |
| Blanks, dodge roll | none in TBOI | keep (config) |
| Devil / Angel rooms, secret rooms, curse rooms | none in v0.1 | out of scope (needs DungeonAPI rooms) |
| Trinkets, cards, pills | none in v0.1 | out of scope (new slot type) |

---

## 7. Prior art

- No Isaac-content ETG mod exists on BepInEx/Thunderstore (458 packages scanned). [V]
- Classic Mod the Gungeon era (abandoned, closed source, 2017): "The Child (Isaac custom character) WIP" on ModWorkshop (author drew their own sprites; starter Makarov as placeholder) and "Isaac Gamemodes" by Kyle (console commands `isaac`, `eden`, `thelost`). [V]
- Adjacent: `KyleTheScientist-Static_Camera_Mod` (room-locked camera "like Isaac"), `Morphious86-Reloaded_HUD` (Isaac-style stats HUD). [V]
- Best code references: `Some-Bunny/Modular` (character that cannot pick up guns), `pcrain/GungeonCraft` (characters, items, run data, Hecked mode, Gunfig), `Nevernamed22/OnceMoreIntoTheBreach` (CharacterAPI usage), `appletree3232/BetterShop` (shop `DoSetup` patches). [V]

---

## 8. Intellectual property and asset policy

1. **Art**: all sprites are original "in the style of" pixel art. Ripping TBOI sprites is unlicensed copying of Nicalis-owned assets and breaches Thunderstore's global rule against reuploading others' assets; Nicalis has a DMCA record (Cave Story Engine 2, 2020). [V]
2. **Audio**: no TBOI music or SFX (Ridiculon soundtrack is separately owned). Original or CC0 audio only. [V]
3. **Names**: item and character names may be referenced; the package/mod name must not contain "The Binding of Isaac", "Rebirth", "Repentance" or publisher marks. README carries a non-affiliation disclaimer. [I]
4. **Game files**: never commit or ship `Assembly-CSharp.dll` or any ETG file; reference assemblies come from NuGet. Thunderstore forbids distributing game files. [V]
5. **Licenses**: MIT for code; art/audio under a separate notice excluding third-party IP from the grant. Alexandria and MtG API are MIT; ETGMod is MIT. [V]
6. **Mechanics** (formulas, drop tables, item behaviours) are not copyrightable and may be re-implemented. [I]
7. **Owner's decision on sprites (2026-10-05)**: the owner intends to use sprite sheets extracted from their own copy of TBOI for the character and items, once working outside the cloud. To keep points 1 to 4 intact, those files live only in the external asset folder on the owner's machine (CHR-14): they are git-ignored, never pushed to this public repository, and never included in the Thunderstore zip. The published mod ships placeholders (or original art if it is ever made) and loads the owner-supplied sheets when present. Publishing ripped sheets would be redistribution of Nicalis-owned assets and a Thunderstore rule violation; this split keeps the public artefacts clean.

---

## 9. Open decisions for the project owner

Decided by the project owner on 2026-10-05 unless marked open.

| # | Question | Decision |
|---|----------|----------|
| Q1 | Is Isaac mode tied to playing Isaac, or a separate toggle usable by any Gungeoneer? | **Tied to Isaac.** No separate toggle. |
| Q-tears | TBOI arc with range drop-off, or straight ETG-style bullets? | **Full TBOI reimplementation** of tear behaviour (arc, range, size, knockback). |
| Q2 | Do Isaac items ever appear for other characters? | **No.** Isaac items exist only in Isaac runs. |
| Q3 | Keep ETG blanks and dodge roll in Isaac mode? | **Remove both.** Replace with TBOI-analogue items (which ones: open), granted only if the player accepts the adaptation prompt at the start of the run (MOD-13, MOD-14). |
| Q4 | Health model for v0.1? | **Vanilla hearts + armor re-skinned as soul hearts**; custom heart types later. |
| Q5 | Keep useful vanilla ETG items in Isaac drops? | **Yes**, mixed with Isaac items; gun-only items excluded (MOD-8). Shop prices rescaled to Isaac income (ECO-1, ECO-2). |
| Q6 | Which TBOI version is the reference? | Repentance (v1.7.x) values. |
| Q-items | Is the v0.1 item list (ITM-10) acceptable? | **Yes for now**; to be adjusted later. |
| Q7 | Character sprite source? | **Placeholders (flat coloured squares) for everything now.** Later, sprite sheets extracted from the owner's copy of TBOI, loaded from the external asset folder (CHR-14, §8 point 7), wired in from the start of M1. |
| Q8 | Isaac's nickname and the mod's public name. | Open. Working title "Isaac Mode"; nickname candidate "The Child". |
| Q9 | Co-op support level. | Open. Must not crash; parity later. |
| Q-order | Milestone order M1 character, M2 loot, M3 items? | **Agreed.** |
| Q-adapt | Which TBOI-analogue items compensate for the lost roll and blanks? | **Open**, owner to decide before MOD-14 is implemented. |

---

## 10. Proposed roadmap

| Milestone | Content | Exit criterion |
|-----------|---------|----------------|
| M0 Skeleton | Project builds against NuGet refs, loads in game, logs in F2, Thunderstore manifest, CI if feasible, LICENSE, .gitignore | DLL loads with no errors |
| M1 Tears + Isaac (placeholder art) | Character registered on a vanilla base with square placeholder sprites, asset loader with external-folder override (CHR-14), Tears gun with hidden sprite and full TBOI tear behaviour, stat mapping, gun lock, no blanks, no dodge roll | Isaac playable through floor 1 with no gun pickups, no roll, no blanks |
| M2 Isaac mode loot and economy | Table swap, pedestal override, shop replacement, room-clear table, safety net, seeded runs, no enemy shells, shop price rescale, adaptation prompt with substitute items | A full run yields zero guns, TBOI-style drops and income, and the prompt works |
| M3 Items v0.1 | 25–40 items from ITM-10, pickups, D6, synergies for multishot | Items drop with correct tiers and effects |
| M4 Health and HUD | Armor-as-soul-hearts re-skin, 12-heart cap check, stat readout | HUD readable through a full run |
| M5 Art and audio | Full original animation set, punch-out, foyer card, SFX bank | No placeholder art remains |
| M6 Release 1.0 | Compatibility pass with major mods, Gunfig options, README/CHANGELOG, Thunderstore upload | Published |
| Later | Soul/black hearts proper, Devil/Angel rooms via DungeonAPI, trinkets/cards/pills, Brimstone/Technology beams, alt costume, Breach unlocks | |

---

## 11. Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Alexandria has no formal documentation; signatures change between versions (`BuildCharacter` parameter list has changed) | Rework | Pin NuGet versions; read source; join the Mod the Gungeon Discord |
| Full character animation set is a large art task (about 30 clips plus punch-out) | Schedule | Ship over a vanilla base first; prioritise idle/run/dodge/death |
| Balance: 3 hearts and TBOI damage numbers do not fit ETG enemy HP and bullet density | Fun | Global damage scaling constant; keep blanks and dodge roll; playtest early |
| Gun-biased assumptions baked into vanilla (`IsBossRewardForcedGun`, `GunVersusItemPercentChance`, treasure chest gun/item pairing) | Edge cases, log spam | Table swap plus safety net; test every reward source |
| Other mods patching `Gun.Pickup` or loot (Modular, GungeonCraft) | Conflicts | Guard patches on Isaac mode; test against them |
| IP takedown if any ripped asset slips in | Mod removed | Asset review before each release; original art only |
| ETG 2 (announced 2025) is a different codebase | None for this mod | Target stays ETG 1 v2.1.9 |
