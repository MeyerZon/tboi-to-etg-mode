# Tools

Developer scripts. Python 3, standard library only; nothing to install.

## Sprites: from the game to the mod (CHR-14)

Isaac's sprites come from the owner's own copy of The Binding of Isaac: Rebirth (docs/REQUIREMENTS.md, section 8).
Only sprites are taken. **Never copy anything from the extracted `music` or `sfx` folders into this repository.**

1. Extract the game's resources with the extractor that ships with the game. Give it an output folder
   outside the game and outside this repository, so the install stays untouched:

   ```
   "<Isaac>/tools/ResourceExtractor/ResourceExtractor.exe" "<Isaac>" "<output folder>"
   ```

   `<Isaac>` is the install folder, for example
   `C:/Program Files (x86)/Steam/steamapps/common/The Binding of Isaac Rebirth`. The result is about 1.2 GB.
   Run with no arguments, the extractor unpacks into the game's own `resources` folder instead.

2. Generate the placeholder set first (it wipes `newspritesetup/`), then convert the real animations over it:

   ```
   python tools/make_placeholders.py
   python tools/anm2_to_frames.py "<output folder>/resources"
   ```

3. Build. The frames are embedded in the DLL like every other resource.

## `anm2_to_frames.py`

Reads `gfx/001.000_player.anm2` and its sheets, overlays the body and head layers, and writes one PNG per
frame into `src/IsaacMode/Resources/Characters/Isaac/newspritesetup/<clip>/`, plus `facecard.png`.

- The table `CLIPS` maps each Alexandria clip name to TBOI animations. Standing and walking clips overlay a
  body animation (`WalkDown`, `WalkRight`, `WalkUp`) with a head animation (`HeadDown`, `HeadRight`, `HeadUp`);
  whole-body clips (`Death`, `Pickup`, `Happy`, `FallIn`, `JumpOut`, `Jump`) are sampled down to a few frames.
- Pixels are kept 1:1 (16 px = 1 Gungeon unit). Each clip gets one canvas, symmetric around Isaac's position
  with his feet on the bottom row; the mod centres every frame on the player from its width.
- Alexandria constraints: a run clip must have 1, 3, 4 or at least 6 frames, and a dodge clip must not have
  exactly 5.
- Not converted: the ghost clips, the hand, the minimap icon and the boss card keep their placeholders.

## `make_placeholders.py`

Writes a flat-coloured stand-in for every clip Alexandria looks up, the loose character sprites and the
Tears HUD icon. Run it before the converter, never after.
