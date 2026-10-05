# TODO

Everything left to do on the mod, by milestone. Requirement IDs refer to [`docs/REQUIREMENTS.md`](docs/REQUIREMENTS.md).
Tasks marked **(owner)** need the project owner (play-testing, decisions, extracting sheets). Everything else can be done in a cloud session.
Keep this file current: tick tasks as they merge, add tasks as they are discovered, and move finished milestones to the bottom.

## M1: Isaac exists and shoots tears

Exit criterion: Isaac is selectable in the Breach and can clear floor 1 with no gun pickups, no dodge roll and no blanks.

### Character (CHR-1 to CHR-4, CHR-9 to CHR-12)
- [ ] Register Isaac through Alexandria CharacterAPI on the Bullet base prefab (CHR-1, CHR-2); extend `PlayableCharacters` with the mod GUID.
- [ ] `characterdata.txt` with name, short name, nickname, stats (health 3, movement speed, curse 0) and loadout: tears gun `infinite` + bomb (CHR-3, CHR-4).
- [ ] Placeholder sprite set: generate flat coloured squares for every `playerAnimInfo` clip, the hand, foyer card, loadout icons, minimap icon, face card, boss card, win pics, co-op death page (CHR-9); script in `tools/make_placeholders.py` so the set can be regenerated.
- [ ] Punch-out sprites placeholder set (`punchout/`) so the Resourceful Rat fight does not crash.
- [ ] Character strings: name, nickname, select-card text, Ammonomicon entry (CHR-10).
- [ ] Verify Isaac survives the Breach stand, character select, elevator mid-game save and reload (CHR-11, CHR-12).
- [ ] **(owner)** Confirm Isaac appears in the Breach and selecting him starts a run (F2 shows the load line).

### Tears weapon (CHR-5 to CHR-8, STA-1, STA-3, STA-4)
- [ ] Build the Tears gun with `ItemBuilder.BuildGun`, quality `EXCLUDED`, infinite ammo, undroppable, persists on death, volley modifications prevented; clone the projectile module from the vanilla Tear Jerker (CHR-5).
- [ ] Invisible gun: 1x1 transparent idle sprite plus `.jtk2d` hand points; `ForceHandless` on the player if the hand still shows (CHR-6).
- [ ] Hide or replace the ammo HUD with Alexandria `CustomAmmoDisplay` (CHR-6).
- [ ] Tear projectile: TBOI arc (spawn height, fall so the tear lands at Range), size scaling with damage, knockback from shot speed, no rotation (CHR-7).
- [ ] Fire rate from the TBOI tear-delay formula in `Core/TearFormulas` driving the module cooldown; ETG `RateOfFire` multipliers still apply (STA-1).
- [ ] Damage from `Core/TearFormulas.Damage` times a global scaling constant, exposed as a tunable (STA-3).
- [ ] Range and shot speed mapping (`RangeMultiplier`, `ProjectileSpeed`) with a tunable base range for ETG room sizes (STA-4).
- [ ] Tear impact VFX: recoloured vanilla impact pool (CHR-7).
- [ ] **(owner)** Play-test: tears feel like Isaac; adjust the damage constant and base range.

### Gun lock, no roll, no blanks (CHR-8, MOD-13, HLT-5)
- [ ] Harmony prefix on `Gun.Pickup` and `LootEngine.GivePrefabToPlayer`: for Isaac, convert the gun into an Isaac item drop at the same position (CHR-8).
- [ ] `GunInventory.GunLocked.SetOverride("isaac", true)` while Isaac is the player (CHR-8).
- [ ] Blanks: start at 0, no floor restock, blank pickups never spawn for Isaac (MOD-13).
- [ ] Dodge roll disabled for Isaac via `CustomDodgeRollAPI` or input interception (MOD-13, HLT-5).
- [ ] Armor's free blank on break disabled for Isaac (HLT-1).
- [ ] **(owner)** Play-test: no gun can be picked up from chests, pedestals or shops; roll and blanks are gone.

### Stats plumbing (STA-2, ITM-4)
- [ ] Custom StatAPI stats: `tears`, `luck`, `tearHeight` (STA-2).
- [ ] `IsaacStats` component on the player that recomputes tear delay and damage when stats change.
- [ ] Unit tests for every formula added to `Core/`.

### Sheet converter (CHR-14)
- [ ] `tools/anm2_to_frames.py`: read a TBOI `.anm2` plus its sheets, compose head and body layers per frame, write ETG clip folders into `Resources/Characters/Isaac/newspritesetup/` at 16 px per unit scale; mapping table from TBOI animation names to Alexandria clip names.
- [ ] `tools/README.md` with the extraction steps.
- [ ] **(owner)** Extract the player `.anm2` and the Isaac costume sheets from the game and run the converter.
- [ ] Credit line for the sprites in README and the Thunderstore description (§8).

### Debug and config (UI-3, UI-2)
- [ ] F2 console group `isaac`: `mode`, `stats`, `give <item>`, `hearts <n>`.
- [ ] Gunfig soft dependency with an empty "Isaac Mode" page, ready for toggles.

## M2: Isaac mode loot and economy

Exit criterion: a full Isaac run yields zero guns, TBOI-style drops and income, and the adaptation prompt works and persists.

### Loot replacement (MOD-1 to MOD-7)
- [ ] `IsaacModeActive` set in `CustomActions.OnRunStart` from the character identity, restored on mid-game load (MOD-1).
- [ ] Build `IsaacItemsTable` with `LootUtility`: Isaac items plus allowed vanilla items at their qualities (MOD-2, MOD-8).
- [ ] Harmony prefix on `RewardManager.GetItemForPlayer` and `LootData.GetItemForPlayer` swapping the gun and item tables for the Isaac table (MOD-2, MOD-3).
- [ ] Boss pedestal override through `CustomActions.OnRewardPedestalDetermineContents`; a "Boss pool" subset of stat-ups (MOD-4).
- [ ] Shops: Bello covered by the table swap; sub-shop gun stock replaced in `OnShopItemStarted` or a `DoSetup` prefix; ammo consumables replaced by TBOI pickups (MOD-5).
- [ ] Room-clear rewards via `RoomRewardAPI` using `Core/RoomClearRewards` (MOD-6).
- [ ] Safety net: any `Gun` that reaches the floor becomes an Isaac item; seeded runs covered by patching `GetItemForSeededRun` (MOD-7).
- [ ] Exclusion list of gun-only and blank/roll vanilla items (MOD-8), kept in a data file with a unit test that it parses.
- [ ] Blessed mode made a no-op for Isaac; Rainbow, Turbo, Challenge, Boss Rush, shortcuts verified not to crash (MOD-12).
- [ ] **(owner)** Play-test a full run: count guns seen (must be zero), note drop feel.

### Economy (ECO-1, ECO-2)
- [ ] No shells from enemies in Isaac runs (ECO-1).
- [ ] Coin pickups from the room-clear table and chests; TBOI coin subtypes (penny, nickel, dime) as `CurrencyPickup` clones with placeholder sprites (ITM-11).
- [ ] Expected-income model in `Core/` (coins per floor from the drop table) with a unit test; price multiplier derived from it (ECO-2).
- [ ] Apply the multiplier to Bello and sub-shops for Isaac runs; verify Cursula's curse pricing still works (ECO-2).
- [ ] **(owner)** Play-test the economy: can you afford about one shop item per floor as in TBOI?

### Adaptation prompt (MOD-14)
- [ ] **(owner)** Decide the substitute items granted for the lost roll and blanks (Q-adapt).
- [ ] Verify in source whether ETG's confirmation dialog can be reused for a yes/no prompt; fall back to a shrine-style interactable in the first room.
- [ ] Show the prompt on first-floor load of an Isaac run; grant items on yes; store the answer in run data so it survives save and load (CHR-12).
- [ ] Gunfig toggle to skip the prompt and always/never adapt (UI-2).

## M3: Items v0.1 (ITM-1 to ITM-12)

Exit criterion: about 30 items drop with the right tiers and effects; the D6 rerolls.

- [ ] Item framework: `IsaacPassive` and `IsaacActive` base classes over Alexandria's `PassiveItem` and `PlayerItem`; registration helper that removes each item from the vanilla tables and adds it to the Isaac table at its mapped quality (ITM-1 to ITM-3).
- [ ] Per-room active charging with +2 for large rooms; batteries as charge pickups (MOD-9, ITM-7).
- [ ] Stat-ups: Sad Onion, Pentagram, Magic Mushroom, Cricket's Head, Blood of the Martyr, Lunch, Dinner, Belt, Growth Hormones, Jesus Juice, Steven (ITM-4, ITM-10); the "x1.5 items do not stack" rule in `Core/` with a test (STA-3).
- [ ] Tear flags: Spoon Bender (homing), Cupid's Arrow (piercing), Ouija Board (spectral), Rubber Cement (bouncing), Polyphemus (ITM-5).
- [ ] Multishot: 20/20, The Inner Eye, Mutant Spider, with the fire-rate penalties and the 16-tear cap; synergy that removes the penalty when combined (ITM-6, ITM-9).
- [ ] Health items: Yum Heart (active), Raw Liver (ITM-10).
- [ ] Actives: The D6 (reroll pedestal and chest items in the room), Book of Belial, The Bible, Bomb (ITM-7).
- [ ] Pickups: red half and full hearts, keys, bombs, batteries as clones with placeholder sprites (ITM-11).
- [ ] Ammonomicon entries, names and descriptions for every item; item tips when ItemTips is installed (ITM-12).
- [ ] Add Isaac items to the exclusion from vanilla loot and verify none leak into a Marine run (ITM-2).
- [ ] Unit tests for every piece of item math in `Core/`.
- [ ] **(owner)** Play-test: tiers feel right, nothing is useless, nothing crashes.

## M4: Health and HUD (HLT-1 to HLT-4, UI-1)

Exit criterion: the HUD reads as Isaac hearts through a full run.

- [ ] Armor re-skinned as blue soul hearts via a `GameUIHeartController.UpdateHealth` postfix (HLT-1).
- [ ] Soul heart pickups that grant armor (ITM-11).
- [ ] 12-heart cap and HUD layout check above 10 hearts (HLT-4).
- [ ] Isaac stat readout (tears, damage, range, shot speed, luck) on the HUD or pause menu (UI-1).
- [ ] Run-data carrier item for mode flags, prompt answer and health state (CHR-12).

## M5: Art and audio

Exit criterion: no placeholder squares remain.

- [ ] **(owner)** Extract sheets: player animations, item icons, pickups, tears, UI hearts; run the converter.
- [ ] Replace every placeholder; adjust bounds and offsets per clip; hand sprite.
- [ ] Punch-out sprites from the sheets or drawn.
- [ ] Original or synthesised SFX for tear fire, tear splash, hurt, death, item pickup; `.bnk` via `gen-gungeon-audio-bank.py`; gun switch group wiring (CHR-13). No TBOI audio.
- [ ] Real 256x256 Thunderstore icon.

## M6: Release 1.0

- [ ] Compatibility pass with GungeonCraft, Once More Into The Breach, Planetside, Modular, Custom Characters Mod, Jolly Co-op (NFR-1).
- [ ] Co-op: Isaac as player 2 does not crash (Q9).
- [ ] Gunfig options page complete (UI-2).
- [ ] README: features, item list, install, credits, non-affiliation line; CHANGELOG (NFR-6).
- [ ] **(owner)** Decide the public name and nickname (Q8).
- [ ] Version bump in the three places; `scripts/package.sh`; upload to Thunderstore (NFR-4).

## Later (not scheduled)

- [ ] Proper soul, black, eternal, bone and rotten hearts with a custom counter and HUD (HLT-3).
- [ ] Brimstone and Technology as charged and beam module swaps (ITM-6).
- [ ] Mom's Knife.
- [ ] Devil and Angel rooms through DungeonAPI; Devil deal pricing in heart containers (ITM-8).
- [ ] Trinkets, cards, runes and pills as a new slot type.
- [ ] Familiars.
- [ ] Alternate costume; Breach unlocks and prerequisites.
- [ ] Localisation beyond English.
- [ ] CI: restrict the push trigger to `main` so PR branches run once per push.

## Done

### M0: Skeleton (merged in PR #1)
- [x] SDK-style `net35` project with NuGet reference assemblies, plugin stub, `Core/` formulas, xunit tests.
- [x] CI building, testing and uploading the DLL and Thunderstore zip.
- [x] Packaging script, manifest, placeholder icon, licence, changelog.
- [x] Cloud session hook and permission rules; `CLAUDE.md`; `docs/WORKFLOW.md`.
- [x] Requirements document and research notes.

### Decisions (PR #2)
- [x] Owner's design decisions recorded (section 9); precedent research on crossover assets; asset policy adopted.
