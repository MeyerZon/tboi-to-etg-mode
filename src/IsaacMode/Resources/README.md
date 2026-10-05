# Embedded resources

Everything under this folder with the extensions `.png`, `.txt`, `.json`, `.jtk2d` or `.bnk`
is embedded in `IsaacMode.dll` and addressed from code as `IsaacMode/Resources/<folder>/<file>`.

Planned layout (see docs/REQUIREMENTS.md, CHR-9 and ENV-4):

- `Characters/Isaac/` : `characterdata.txt`, `newspritesetup/<clip>/*.png`, `foyercard/`, `punchout/`, `icon.png`, `facecard.png`, ...
- `Items/` : Ammonomicon sprites for Isaac items (at most 30x30 px, 16 px = 1 world unit).
- `Guns/Tears/` : the invisible Tears gun sprite and its `.jtk2d` metadata, tear projectile sprites.
- `Sounds/` : `.bnk` banks generated with `gen-gungeon-audio-bank.py`.

All art and audio must be original. Nothing from The Binding of Isaac may be copied here.
