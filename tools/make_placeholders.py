#!/usr/bin/env python3
"""Generate the placeholder sprite set for Isaac (CHR-9).

Every sprite is a flat coloured rectangle, with two eye pixels on the clips that face the camera
so the facing direction can be read in game. Standard library only, so it runs anywhere:

    python tools/make_placeholders.py

Output goes to src/IsaacMode/Resources/Characters/Isaac/ (plus the Tears HUD icon under
Resources/Guns/Tears/) and replaces what is there. The folder
and file names are the ones Alexandria's CharacterAPI looks for (docs/research/api-patterns.md, 1.3).
Once the real sheets are converted (CHR-14) this script is no longer needed for those clips.
"""
import os
import shutil
import struct
import zlib

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..",
                    "src", "IsaacMode", "Resources", "Characters", "Isaac")

BODY_W, BODY_H = 16, 20

SKIN = (242, 198, 180, 255)
SKIN_BACK = (214, 168, 150, 255)
SKIN_SIDE = (228, 184, 166, 255)
GHOST = (200, 220, 255, 170)
DEAD = (150, 120, 120, 255)
EYE = (40, 30, 30, 255)

# Clip names from Alexandria's SpriteHandler.playerAnimInfo (v0.5.10).
CLIPS = [
    "chest_recover", "death", "death_coop", "death_shot",
    "dodge", "dodge_bw", "dodge_left", "dodge_left_bw",
    "doorway",
    "ghost_idle_back", "ghost_idle_back_left", "ghost_idle_back_right",
    "ghost_idle_front", "ghost_idle_left", "ghost_idle_right",
    "ghost_sneeze_left", "ghost_sneeze_right",
    "idle", "idle_backward", "idle_backward_hand", "idle_backward_twohands",
    "idle_bw", "idle_bw_twohands",
    "idle_forward", "idle_forward_hand", "idle_forward_twohands",
    "idle_hand", "idle_twohands",
    "item_get",
    "jetpack_down", "jetpack_down_hand", "jetpack_right", "jetpack_right_bw",
    "jetpack_right_hand", "jetpack_up",
    "pet",
    "pitfall", "pitfall_down", "pitfall_return",
    "run_down", "run_down_hand", "run_down_twohands",
    "run_right", "run_right_hand", "run_right_twohands",
    "run_right_bw", "run_right_bw_twohands",
    "run_up", "run_up_hand", "run_up_twohands",
    "slide_right", "slide_up", "slide_down",
    "spinfall", "spit_out",
    "tablekick_down", "tablekick_down_hand", "tablekick_right", "tablekick_right_hand",
    "tablekick_up",
    "timefall",
]

# Not in playerAnimInfo: the Breach stand looks these up by name, so they go under custom/.
CUSTOM_CLIPS = ["select_idle", "select_choose"]

# Alexandria tags audio events on frames 2 and 5 of every run clip without a bounds check,
# so a run clip must have exactly 1, 3, 4 or at least 6 frames.
RUN_FRAMES = 6


def write_png(path, width, height, pixel_at):
    raw = bytearray()
    for y in range(height):
        raw.append(0)
        for x in range(width):
            raw.extend(pixel_at(x, y))

    def chunk(tag, data):
        body = tag + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)))
        f.write(chunk(b"IDAT", zlib.compress(bytes(raw), 9)))
        f.write(chunk(b"IEND", b""))


def flat(path, width, height, colour):
    write_png(path, width, height, lambda x, y: colour)


def facing(clip):
    """front, back or side, from the clip name."""
    if clip.startswith("ghost_idle_back") or clip.endswith(("_bw", "_up")) \
            or "_bw_" in clip or "_up_" in clip or "backward" in clip:
        return "back"
    if "right" in clip or "left" in clip:
        return "side"
    return "front"


def body(path, clip, frame=0):
    if clip.startswith("ghost"):
        colour = GHOST
    elif clip.startswith("death"):
        colour = DEAD
    else:
        colour = {"front": SKIN, "back": SKIN_BACK, "side": SKIN_SIDE}[facing(clip)]
    side = facing(clip)
    # Run clips bob by one pixel on alternate frames; the canvas size never changes.
    bob = frame % 2

    def pixel_at(x, y):
        if y < bob:
            return (0, 0, 0, 0)
        eye_row = 6 + bob
        if side == "front" and y == eye_row and x in (4, 11):
            return EYE
        if side == "side" and y == eye_row and x == 11:
            return EYE
        return colour

    write_png(path, BODY_W, BODY_H, pixel_at)


def tear_icon(path, size=15):
    """A light blue disc with a highlight: the HUD icon for the Tears weapon."""
    centre = (size - 1) / 2.0
    radius = size / 2.0

    def pixel_at(x, y):
        dx, dy = x - centre, y - centre
        if dx * dx + dy * dy > radius * radius:
            return (0, 0, 0, 0)
        hx, hy = x - size * 0.32, y - size * 0.32
        if hx * hx + hy * hy <= (size * 0.14) ** 2:
            return (235, 248, 255, 255)
        return (120, 190, 240, 255)

    write_png(path, size, size, pixel_at)


def main():
    root = os.path.normpath(ROOT)
    tear_icon(os.path.join(root, "..", "..", "Guns", "Tears", "tear_icon.png"))
    for stale in ("newspritesetup", "icon.png", "facecard.png", "bosscard_001.png"):
        target = os.path.join(root, stale)
        if os.path.isdir(target):
            shutil.rmtree(target)
        elif os.path.exists(target):
            os.remove(target)

    sprites = os.path.join(root, "newspritesetup")
    count = 0
    for clip in CLIPS:
        frames = RUN_FRAMES if clip.startswith("run_") else 1
        for i in range(frames):
            body(os.path.join(sprites, clip, "%s_%03d.png" % (clip, i + 1)), clip, i)
            count += 1
    for clip in CUSTOM_CLIPS:
        body(os.path.join(sprites, "custom", clip, "%s_001.png" % clip), clip)
        count += 1

    flat(os.path.join(sprites, "hand_001.png"), 4, 4, SKIN)
    flat(os.path.join(root, "icon.png"), 14, 14, SKIN)            # minimap icon
    flat(os.path.join(root, "facecard.png"), 34, 34, SKIN)        # HUD portrait
    flat(os.path.join(root, "bosscard_001.png"), 96, 120, SKIN)   # boss intro card (required)
    print("Wrote %d clip frames and 4 loose sprites under %s" % (count, root))


if __name__ == "__main__":
    main()
