# Research note: TBOI mechanics, ETG mechanics and IP constraints (as of 2026-10-05)

Tags: **[V]** read directly from the cited page or source, **[S]** from a search snippet of the cited page, **[I]** inferred/synthesis. The fandom wikis were unreachable; citations use the `wiki.gg` mirrors maintained by the same editor communities.

---

## Part A: TBOI (Repentance) mechanics to translate

### A1. Isaac's base stats and starting item

| Stat | Value | Tag |
|---|---|---|
| Health | 3 red heart containers | [V] |
| Speed | 1.00 | [V] |
| Tears | 2.73 tears/s (tear delay 10; 30/(10+1)) | [V] https://hyphen-ated.github.io/rebirthDps/ |
| Damage | 3.50 | [V] |
| Range | 6.50 tiles | [V] |
| Shot speed | 1.00 | [V] |
| Luck | 0 | [V] |
| Start items | 1 Bomb; The D6 once unlocked | [V] https://bindingofisaacrebirth.wiki.gg/wiki/Isaac |

**Tear delay and fire rate** [V] https://bindingofisaacrebirth.wiki.gg/wiki/Tears
- `FireRate = 30 / (TearDelay + 1)`; HUD shows `(30/(TearDelay+1) + S) × P` with S = flat additions and P = multipliers (Soy Milk ×5.5, Inner Eye ×0.51, Mutant Spider ×0.42).
- Internal tears stat T (sum of tears-ups, base 0) → `TearDelay = 16 − 6·sqrt(T·1.3 + 1)` for 0 ≤ T ≤ about 1.816, where delay reaches 5 → the **5 tears/s cap** from stat-ups. For −0.77 < T < 0: `16 − 6·sqrt(T·1.3+1) − 6·T`; for T ≤ −0.77: `16 − 6·T`. Multipliers and flat additions bypass the cap; engine floor is delay 1 (15 shots/s).

**Damage** [V] https://bindingofisaacrebirth.wiki.gg/wiki/Damage
- `Damage = (Base × sqrt(DmgUps × 1.2 + 1) + FlatUps) × Multipliers`.
- ×1.5 sources (Magic Mushroom, Cricket's Head, Blood of the Martyr + Book of Belial) do not stack with each other; Polyphemus ×2; Eve's Mascara ×2; Sacred Heart ×2.3; 20/20 ×0.8; Soy Milk about ×0.3; most others stack multiplicatively. Flat ups (Ipecac, Curved Horn) add after the sqrt.
- Tear size grows with damage, cosmetically. [S]

**Range** [V] https://bindingofisaacrebirth.wiki.gg/wiki/Range — tiles travelled; floor 1.0, no ceiling; tear falling speed is derived from range, shot speed and height so the tear lands at Range. [V] https://bindingofisaacrebirth.wiki.gg/wiki/Hidden_Attributes

**Shot speed** [V] https://bindingofisaacrebirth.wiki.gg/wiki/Shot_Speed — 1.0 = one tile per 8 frames = 7.5 tiles/s; floor 0.6; higher shot speed increases knockback.

**Speed** — base 1.0; stat-screen max 2.0. [V]

### A2. Health
[V] https://bindingofisaacrebirth.wiki.gg/wiki/Health , https://bindingofisaacrebirth.wiki.gg/wiki/Hearts
- Red hearts (containers, half-heart granularity), soul hearts (consumed first, no container), black hearts (Necronomicon effect on depletion), eternal (two on a floor or carried over → +1 container), bone (3 hits, refillable), rotten (half heart, drained by any hit, spawn flies), gold (drop coins), broken (dead slots), blended.
- Cap 12 hearts (18 with Magdalene + Birthright). Drain order right-to-left, soul/black/bone before red.
- Damage size: chapters 1–3 mostly half-heart; **chapter 4 onward all half-heart hits become a full heart** (except with The Wafer). Sacrifice spikes and bombs deal a full heart.
- Invincibility frames about 1 s in Repentance; Blind Rage doubles. [S]

### A3. Item system
[V] https://bindingofisaacrebirth.wiki.gg/wiki/Items , /Item_Pool , /Item_Quality , /Trinkets , /Cards_and_Runes , /Pills
- Counts: 719 items total, 171 active (about 548 passive incl. familiars), 188 trinkets, 66 cards + 28 runes + 3 others, 50 pill effects.
- Actives: one slot (Schoolbag → 2). Recharge types: room-clear bars 1–6, timed (2–15 s), one-use, unlimited, resource-cost. Room clear = +1 charge; 2×2 and L rooms = +2; only if the room had enemies. Batteries: Lil' 6, Micro 2, Mega full ×2, Golden 6 + respawn. The Battery allows one extra full use.
- Trinkets: 1 slot (2 with Mom's Purse / Belly Button); 2% golden; smelting makes them permanent.
- Pocket consumables: cards/runes/pills share one slot; random card roll: tarot 66.66%, reverse 11.22%, runes 3.6%, soul stones 5.95%.
- Pools: Treasure (largest, default fallback), Shop, Boss (passives only), Devil, Angel, Secret, Library, Golden Chest, Red Chest, Curse, Beggar, Devil Beggar, Key Master, Bomb Bum, Battery Bum, Planetarium, Ultra Secret, Crane Game, Old/Wooden/Mom's Chest, Baby Shop, Shell Game. Nearly all items weight 1; rares 0.1–0.5. Seen items are removed from most pools (100%), partially from others (50% / 40% / 10%). Touching removes from all pools. Empty pool → Treasure → Breakfast.
- Quality 0–4 (hidden), used by Bag of Crafting, Sacred Orb, Tainted Lost, NO!, Poker Chip, Abyss, Birthright.

### A4. Pickups
[V] https://bindingofisaacrebirth.wiki.gg/wiki/Coins , /Bombs , /Keys , /Batteries , /Hearts , /Sacks , /Chests
- Coins: Penny 91.7%, Nickel 4.9%, Dime 1.0%, Lucky Penny 0.93%, Sticky Nickel 0.94%, Golden Penny 0.5%. Max 99.
- Bombs: Bomb 74.84%, Double 12.47%, Troll 9.9%, plus Mega Troll, Golden, Giga. Bomb damage 100 to enemies, 1 heart to Isaac.
- Keys: Key 96.08%, Golden 1.97%, Charged 1.96%.
- Hearts: Full 39.98%, Half 39.99%, Soul 6.73%, Half soul 2.24%, Eternal 1.8%, Rotten 1.71%, Blended 0.87%, Scared 0.87%, Gold 0.63%, Black 0.5%, Bone 0.47%.
- Sacks: Grab Bag 1–4 pickups (35% key, 27% penny, 26% bomb, 7% card/rune, 6% battery). Black Sack 2–3 (35% pills, 35% bombs, 20% black heart, 10% bone heart).
- Chests: Normal (free: 78% pickups, 10% trinket, 10% pill), Golden/Locked (key: 20% Golden-Chest item, 58% pickups, 10% trinket, 10% card/rune), Red, Bomb/Stone, Spiked, Mimic, Eternal, Old, Wooden, Mega, Haunted, Mom's.

### A5. Room and run structure
[V] https://bindingofisaacrebirth.wiki.gg/wiki/Rooms , /Chapters , /Treasure_Room , /Shop , /Secret_Room , /Devil_Room , /Angel_Room , /Sacrifice_Room , /Curse_Room , /Boss_Room , /Room_Clear_Awards
- Floors: Ch.1 Basement/Cellar/Burning Basement; Ch.2 Caves/Catacombs/Flooded Caves; Ch.3 Depths/Necropolis/Dank Depths; Ch.4 Womb/Utero/Scarred Womb; Ch.5 Sheol | Cathedral; Ch.6 Dark Room | Chest; plus Blue Womb, Void, Home, alt path. About 8–11 floors per run.
- Room types: Boss, Mini-Boss, Treasure, Planetarium, Shop, Arcade, Challenge, Boss Challenge, Curse, Sacrifice, Secret, Super Secret, Ultra Secret, Library, Devil, Angel, Vault, Dice, Bedrooms, Crawl Space, Black Market, Angel Shop, Secret Shop, Red Room, I AM ERROR, Boss Rush, Mega Satan.
- Treasure room: 1 per floor on Ch.1–3; free on Basement 1, key from floor 2; one item (two with More Options / alt path).
- Shop: key required; items 15¢ base, pickups 3–5¢; Donation Machine levels raise stock.
- Secret rooms: bombed open, Secret pool.
- Devil deal: base 1% after boss on floors 2–8; +35% no red-heart damage vs boss, +99% none on floor, item bonuses, Goat Head guaranteed; ×0.25 if seen last floor. Costs 1–2 containers or 3 soul hearts. Angel room replaces devil at 50% base (+bonuses), free items; taking a paid devil deal disables angel rooms.
- Sacrifice room: full-heart spikes, reward ladder by count.
- Boss room: Boss-pool item + hearts, trapdoor, devil/angel roll.
- **Room clear awards**: `value = rnd × clamp(Luck,0,10) × 0.1 + rnd` (both rnd in 0–1). < 0.22 nothing (22%); 0.22–0.30 card/pill/trinket (8%); 0.30–0.45 coin (15%); 0.45–0.60 heart (15%); 0.60–0.80 key (20%); 0.80–0.95 bomb (15%); > 0.95 chest (5%). Lucky Foot / Lucky Toe bias; hard mode: 33% of heart rewards become nothing.

### A6. Luck
[V] https://bindingofisaacrebirth.wiki.gg/wiki/Luck — base 0 (Keeper −2, Lazarus/Esau −1, Jacob +1). For room drops luck is clamped 0–10. Tear-effect chances scale to 100% at per-item thresholds (Fire Mind 13, Ball of Tar 18, Common Cold 12, Loki's Horns 15). Luck does not affect machines, chest contents or devil-room contents.

### A7. Tear projectile properties and tear replacers
[V] https://bindingofisaacrebirth.wiki.gg/wiki/Tear_Effects , /Hidden_Attributes , /Mutant_Spider , /The_Inner_Eye , /20/20 , /Brimstone , /Technology , /Mom%27s_Knife
- Flags: Homing, Piercing (through enemies), Spectral (through obstacles), Bouncing. Status tears are luck-rolled. Tears spawn at height 23.75 and fall to land at Range.
- Knockback: `CollisionKnockback = Vp × 1.7 × Mp / (10 × Mt)`; damage does not affect knockback, shot speed does.
- Multi-shot: 20/20 → 2 tears ×0.8 damage; Inner Eye → 3 tears, fire rate ×0.51; Mutant Spider → 4 tears, fire rate ×0.42; combinations remove penalties; hard cap 16 tears per volley.
- Brimstone: charged piercing+spectral beam; charge time = 130 / ceil(TearDelay); damage per tick = Damage × FireRate / 5, 9 ticks; ≥15 fire rate → continuous.
- Technology: fire-rate-based piercing laser, infinite range. Technology 2 continuous beam.
- Mom's Knife: held/thrown knife, 20 hits/s contact; charged throw ×6 damage.
- Design note [I]: TBOI items are behaviour swaps on one projectile source; in ETG the analogous mechanisms are gun-level (projectile module, charge projectiles, `BeamController` beams). One "Tears" gun whose module/projectile gets flags is the natural mapping.

---

## Part B: ETG mechanics the mod interacts with

### B1. Player health
[V] https://enterthegungeon.wiki.gg/wiki/Health , /Armor , /Jammed , /Curse , /The_Robot , /Dodge_Roll_(Move)
- Health in 0.5 increments; Gungeoneers start at 3 (Robot 0); max HP cannot drop below 1 except Robot. No vanilla way to show a half-heart max.
- Regular bullets and contact = half heart; **Jammed enemies deal a full heart**. Armor absorbs exactly one hit of any size and triggers a free blank when destroyed. Robot: 6 armor, Master Rounds → +1 armor, heart containers → coins.
- I-frames: `HealthHaver.TriggerInvulnerabilityPeriod(float)` exists; default duration not confirmed. Dodge roll about 0.7 s, first half invulnerable to bullets.
- Master Rounds: flawless boss → +1 max health (+1 armor on Robot).

### B2. Economy, chests, shops, rewards
[V] https://enterthegungeon.wiki.gg/wiki/Pickups , /Chests , /Shops , /Breach , /Hegemony_Credit , /Blanks , /Coolness , /Curse , /Bosses
- Pickups: shells 1/5/50; keys; blanks (restocked to 2 at floor start); half/full hearts; armor; Full Ammo / Spread Ammo.
- Room-clear rewards: base about 20% (30% co-op); dynamic chance about `(1 + coolness − curse)%` rising about 9% per cleared room without a reward, capped 80–85%. Curse lowers reward chance, raises ammo drops, mimic and fuse odds; Coolness lowers fuse odds and active cooldowns.
- "Magnificence" (hidden per-room score for rare drops) is community-reported; verify in `LootEngine`/`RewardManager`. [S]
- Chests: D brown / C blue / B green / A red / S black + Rainbow, Synergy, Glitched, Truth, Rat. Treasure-room chests always locked; **per floor one is a gun and the other an item**. Floor 1 about 35% brown → 4% black; floor 5 0% brown → 12.5% black. Fuses; destroying a chest: 0.6 junk, 0.2 half-heart, 0.1 explosion, 0.1 item/gun.
- Boss pedestal: gun if no gun was picked up this floor; else 80% / 70% / 37.5% gun depending on gun count. Hegemony Credits 1–5 per boss, doubled flawless.
- Shops: Bello (price ×1.0 → ×1.6 by floor; D items from 10, S items 120 base), Cursula (−50%, +2.5 curse per buy), Flynt (keys), Trorc (military, −20%), Old Red (blanks), Goopton (goop, −20%), Doug, Ser Manuel. Breach: Ox & Cadence, Trorc, Goopton, Doug, Sorceress (Blessed), Daisuke (Challenge), Tonic (Turbo), Bowler (Rainbow), Winchester, Frifle & Grey Mauser, Ledge Goblin.

### B3. `PlayerStats.StatType` (31 members, source order)
[V] decompiled `PlayerStats.cs` https://github.com/FlowSand/Re-ETG/blob/main/ETG_BACKUP_Task02/PlayerStats.cs
`MovementSpeed, RateOfFire, Accuracy, Health, Coolness, Damage, ProjectileSpeed, AdditionalGunCapacity, AdditionalItemCapacity, AmmoCapacityMultiplier, ReloadSpeed, AdditionalShotPiercing, KnockbackMultiplier, GlobalPriceMultiplier, Curse, PlayerBulletScale, AdditionalClipCapacityMultiplier, AdditionalShotBounces, AdditionalBlanksPerFloor, ShadowBulletChance, ThrownGunDamage, DodgeRollDamage, DamageToBosses, EnemyProjectileSpeedMultiplier, ExtremeShadowBulletChance, ChargeAmountMultiplier, RangeMultiplier, DodgeRollDistanceMultiplier, DodgeRollSpeedMultiplier, TarnisherClipCapacityMultiplier, MoneyMultiplierFromEnemies`
Stacking: most stat items multiply; movement speed adds.

### B4. Characters, alternate characters, active items
[V] https://enterthegungeon.wiki.gg/wiki/Gungeoneers , /The_Pilot , /The_Robot , /The_Paradox , /The_Gunslinger , /Items
- Marine, Convict, Pilot (2 active slots), Hunter, Bullet, Robot (armor only), Cultist (co-op), Paradox (random loadout), Gunslinger. Alternate skins via the Bullet's shrine. Characters are `PlayerController` prefabs with starting gun/item ID lists.
- Active items recharge by damage dealt (requirement scales ×1.0 → ×2.1 by floor), timed, per-room, per-floor, or consumable. 271 items total. Coolness reduces cooldowns. TBOI room charge ≈ ETG per-room; TBOI timed ≈ ETG timed; TBOI one-use ≈ ETG consumable. [I]

### B5. Existing modes and entry points
[V] https://enterthegungeon.wiki.gg/wiki/Boss_Rush , /Turbo_Mode , /Challenge_Mode , /Rainbow_Mode , /Sorceress , /Breach
- Boss Rush (elevator, 3 credits); Turbo (Tonic, free toggle, speed/bullet multipliers); Challenge (Daisuke, room modifiers); Rainbow (Bowler: one rainbow chest per floor, all other chests, shop guns and pedestals removed, Master Rounds still drop); Blessed (Sorceress: gun cycles, no gun pickups).
- Pattern [I]: an "Isaac mode" can be a Breach NPC/shrine toggle (Tonic/Bowler) or tied to a character (Paradox). A character needs no new NPC and reuses character select; Rainbow mode shows reward-manager overrides are feasible.

### B6. Compatibility issues to call out [I]
- Projectile model: TBOI tears arc and die at Range; ETG rooms are larger; a faithful 6.5-tile range will feel cramped.
- Enemies shoot bullets: ETG bullet density plus half-heart hits vs TBOI contact-heavy design; TBOI-style HP with no dodge would be punishing; blanks are ETG's safety valve. Blanks and dodge roll have no TBOI analog (closest: Holy Mantle, flight).
- Damage scaling: TBOI Ch.4+ full-heart hits ≈ ETG Jammed hits.
- Keys/coins economy differs; prices differ by an order of magnitude.
- Luck vs Coolness: Coolness also cuts active cooldowns; a custom luck stat is cleaner.
- Active charge: ETG default is damage-based; use per-room for Isaac actives.
- Gun pickups/boss gun rewards conflict with a character whose weapon is his body; Rainbow/Blessed show reward-manager overrides are feasible.
- 12-heart cap vs ETG HUD built for about 10; no vanilla half-heart max.
- Item count: TBOI 719 vs ETG 271; port a curated subset.

---

## Part C: IP and asset considerations

### C1. TBOI: McMillen / Nicalis
- *The Binding of Isaac: Rebirth* and expansions are developed/published by Nicalis, Inc.; Edmund McMillen is creator/designer. McMillen publicly cut ties with Nicalis in 2019. [V] https://en.wikipedia.org/wiki/Nicalis
- Positive stance on mods inside their game: Afterbirth+ Lua API and Workshop; Booster Packs from community mods; Antibirth became Repentance. [V] https://en.wikipedia.org/wiki/The_Binding_of_Isaac:_Repentance . Nicalis sample Workshop mods invite reuse for "your own original creations". [V] https://steamcommunity.com/sharedfiles/filedetails?id=835650828 . The wiki states modding "is not officially supported or endorsed by Nicalis". [V]
- **No published policy on reusing TBOI sprites/music/sounds in other games' mods** was found. Steam Subscriber Agreement §6 covers Workshop uploads only. [V]
- Enforcement precedent: Nicalis DMCA against Cave Story Engine 2 and forks (Nov 2020). [V] https://gameluster.com/nicalis-under-fire-for-cave-story-dmca/ . No Isaac-specific cross-game takedown found, but tolerance is not a license. [I]
- Prior art: "The Child" (ModWorkshop 25290) author drew their own sprites; "TBOI GUNGEON EDITION" on the Isaac Workshop redistributes ETG audio and has stayed up, still not a license. [V/S]

### C2. ETG: Dodge Roll / Devolver
- Dodge Roll helped with ETGMod code and hangs out in the Gungeon Discord (ETGMod README, MIT). [V] https://github.com/ModTheGungeon/ETGMod . The ETG wiki carries a disclaimer that mods are not official or reviewed. [V]
- No ETG-specific EULA restricting mods found. Do not redistribute `Assembly-CSharp.dll` or other game files. [V/I]

### C3. Thunderstore policy
[V] https://wiki.thunderstore.io/moderation/global-rules — follow copyright and licensing; do not reupload packages or assets by other authors without permission; do not distribute game files such as `Assembly-CSharp.dll`.

### C4. Practical guidance [I unless cited]
1. Draw original Isaac-style sprites; do not rip TBOI sprites, music or SFX. Avoid TBOI music entirely.
2. Naming: avoid "The Binding of Isaac", "Rebirth/Repentance" and publisher marks in the package name/icon; add a non-affiliation disclaimer; "Isaac" as a plain character reference is acceptable.
3. License: MIT for code (ecosystem norm, ETGMod is MIT); art/audio under a separate notice excluding third-party IP.
4. Distribution hygiene: ship only the mod DLL and own assets; declare BepInExPack, MtG API and Alexandria as Thunderstore dependencies; never include game files.
5. Mechanics (stats, formulas, tables, behaviours) are not copyrightable; names and icons are the sensitive layer.

## Open items not verified
ETG player post-hit invulnerability duration; exact "magnificence" formula; Alexandria's LICENSE file text; Ser Manuel / Doug shop details; TBOI per-quality item counts.
