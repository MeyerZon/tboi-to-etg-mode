# Where we stopped (session of 2026-10-05 / 06)

Read this first when picking the work up again. Update or replace it at the end of each session.

## State of the branches

| Branch | Content | State |
|---|---|---|
| `chore/supply-chain-hardening` | NuGet package source mapping, GitHub Actions pinned to commit SHAs, narrower pre-approved commands in `.claude/settings.json` | Pushed, PR open against `main`. Not merged. |
| `m1/isaac-and-tears` | Everything below. Built on top of the hardening branch. | Pushed, PR open against the hardening branch. Not merged. |

Merge order: hardening first, then M1. Nothing is merged without the owner saying so.

## What M1 has so far

| Piece | Requirement | Checked in game? |
|---|---|---|
| Isaac registered on the Bullet base, Breach stand at (12.3, 21.3) | CHR-1 to CHR-4 | Yes |
| Tears weapon: hidden gun, infinite, undroppable, fire rate / damage / range / speed from TBOI base stats | CHR-5, CHR-6, STA-1, STA-3, STA-4 | Yes |
| No other guns, no dodge roll, no blanks, no blank on armor loss | CHR-8, MOD-13, HLT-5 | Yes |
| Tear arc (level for 80% of range, then falls), size from damage, knockback 14 | CHR-7 | Yes, after one tuning pass |
| Custom stats `tears`, `luck`, `damageups`, `flatdamage`, `tearheight` driving the Tears | STA-1 to STA-3, ITM-4 | Yes |
| Tear icon in the HUD gun box | CHR-6 | Yes (centred after one fix) |
| F2 console group `isaac`: `mode`, `stats`, `addstat <stat> <amount>` | UI-3 | Yes |
| **Real Isaac sprites** converted from the game by `tools/anm2_to_frames.py` | CHR-14 | **No. This is the next thing to check.** |

## Next: check-in 6 (the converted sprites)

Build (`dotnet build IsaacMode.sln -c Release` copies the DLL into the game through `local.props`), start the game, and check:

1. **Size**: Isaac is about 28 x 33 px, kept 1:1, so he is bigger than a Gungeoneer. Acceptable, or scale down?
2. **Position**: centred on his shadow, feet on the ground. The mod shifts each frame by `(16 px - width) / 2`; this is the least certain part.
3. **Facing**: side head aiming sideways, front aiming down, back aiming up; mirrored when aiming left.
4. **Walking** animates in every direction.
5. **Hits** land where expected; the hitbox is still the Bullet's.
6. **Special clips**: item pickup, pit fall, death, table flip, Breach stand idle and select. These mappings are guesses; see `CLIPS` in `tools/anm2_to_frames.py`.
7. **HUD portrait** shows his face.

If something fails, the first error in `<Enter the Gungeon>/BepInEx/LogOutput.log` is what to paste.

## Left in M1 after that

- Punch-out placeholder sprites, so the Resourceful Rat fight does not crash (CHR-9).
- Mid-game save and reload at the elevator (CHR-11, CHR-12).
- Still placeholders: ghost clips, hand, minimap icon, boss card.
- Gunfig soft dependency with an empty options page (UI-2).
- `GunInventory.GunLocked` override was skipped: with one gun and the two pickup patches it has nothing to do.
- Blank and gun pickups still spawn for Isaac; a blocked gun stays on the floor and a gun bought in a shop costs shells for nothing. Both are fixed by the M2 loot table (MOD-2, MOD-7).
- TODO.md has not been ticked; tick M1 tasks when the PR merges.

## Things learned the hard way

- `characterdata.txt` needs an `<altguns>` block even when empty, or Alexandria 0.5.10 throws and leaves a live half-built player in the scene (camera breaks for every character).
- Alexandria tags audio on frames 2 and 5 of run clips without a bounds check: run clips need 1, 3, 4 or at least 6 frames; dodge clips must not have exactly 5.
- `select_idle` and `select_choose` are not in Alexandria's clip table; they go under `newspritesetup/custom/`.
- A boss card (`bosscard_001.png`) is required: the boss intro indexes `BosscardSprites[0]`.
- Custom StatAPI stats cannot go into `ownerlessStatModifiers` (vanilla loop indexes by stat number and throws). Items are fine; the debug command raises the base value instead.
- The HUD gun box is laid out around the gun's own sprite; the tear icon is shifted to that sprite's bounds centre.
- `make_placeholders.py` wipes `newspritesetup/`; run it before `anm2_to_frames.py`, never after.

## Local setup on the owner's PC (not in the repository)

- Enter the Gungeon: `C:\Program Files (x86)\Steam\steamapps\common\Enter the Gungeon`, with BepInExPack_EtG 5.4.2101, Mod the Gungeon API 1.9.2 and Alexandria 0.5.10 installed by hand from Thunderstore.
- `local.props` (git-ignored) points `EtgPluginsDir` at that install's `BepInEx\plugins`.
- The Binding of Isaac: `C:\Program Files (x86)\Steam\steamapps\common\The Binding of Isaac Rebirth`. The extracted resources lived in a temporary session folder and are gone; `tools/README.md` has the one command to extract them again.

## Tunables waiting for more play-testing

`Core/GungeonScale.cs`: `DamageScale` 2.0, `WorldScale` 2.0 (range 13, speed 15). `Tears/TearArc.cs`: `BaseForce` 14, `FallHeight` 0.75. `Core/TearFormulas.cs`: `ArcLevelFraction` 0.8.
