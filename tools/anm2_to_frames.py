#!/usr/bin/env python3
"""Convert The Binding of Isaac player animations into the frame folders Alexandria expects (CHR-14).

Reads the player .anm2 and its sprite sheets from an extracted copy of the game's resources,
composes the body and head layers per frame, and writes one PNG per frame into
src/IsaacMode/Resources/Characters/Isaac/newspritesetup/<clip>/. Standard library only.

    python tools/anm2_to_frames.py <extracted>/resources

See tools/README.md for how to extract the resources. Pixels are kept 1:1 (16 px = 1 Gungeon unit).
Every frame of a clip shares one canvas that is symmetric around Isaac's position, with his feet
on the bottom row; the mod centres each frame on the player from its width.
"""
import math
import os
import shutil
import struct
import sys
import xml.etree.ElementTree as ET
import zlib

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, "..", "src", "IsaacMode", "Resources", "Characters", "Isaac"))

PLAYER_ANM2 = "gfx/001.000_player.anm2"
TBOI_FPS = 30.0

# The ghost that rises out of the death animation would stretch the canvas; ETG has its own ghost clips.
SKIPPED_LAYERS = {"ghost"}


# --------------------------------------------------------------------------- PNG

class Image(object):
    def __init__(self, width, height, pixels=None):
        self.width = width
        self.height = height
        self.pixels = pixels if pixels is not None else bytearray(width * height * 4)

    def get(self, x, y):
        i = (y * self.width + x) * 4
        return self.pixels[i:i + 4]

    def blend(self, x, y, r, g, b, a):
        """Source-over blend of one straight-alpha pixel."""
        if a <= 0:
            return
        i = (y * self.width + x) * 4
        da = self.pixels[i + 3]
        if a >= 255 or da == 0:
            self.pixels[i:i + 4] = bytes((r, g, b, a))
            return
        sa = a / 255.0
        ba = da / 255.0
        oa = sa + ba * (1 - sa)
        for k, s in enumerate((r, g, b)):
            self.pixels[i + k] = int(round((s * sa + self.pixels[i + k] * ba * (1 - sa)) / oa))
        self.pixels[i + 3] = int(round(oa * 255))


def read_png(path):
    with open(path, "rb") as f:
        data = f.read()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("not a PNG: " + path)
    pos, idat, header = 8, bytearray(), None
    while pos < len(data):
        length, tag = struct.unpack(">I4s", data[pos:pos + 8])
        body = data[pos + 8:pos + 8 + length]
        if tag == b"IHDR":
            header = struct.unpack(">IIBBBBB", body)
        elif tag == b"IDAT":
            idat.extend(body)
        pos += 12 + length
    width, height, depth, colour, _, _, interlace = header
    if depth != 8 or colour != 6 or interlace != 0:
        raise ValueError("%s: only 8-bit non-interlaced RGBA sheets are supported" % path)
    raw = zlib.decompress(bytes(idat))
    stride = width * 4
    pixels = bytearray(height * stride)
    prev = bytearray(stride)
    p = 0
    for y in range(height):
        kind = raw[p]
        line = bytearray(raw[p + 1:p + 1 + stride])
        p += 1 + stride
        if kind == 1:
            for i in range(4, stride):
                line[i] = (line[i] + line[i - 4]) & 255
        elif kind == 2:
            for i in range(stride):
                line[i] = (line[i] + prev[i]) & 255
        elif kind == 3:
            for i in range(stride):
                left = line[i - 4] if i >= 4 else 0
                line[i] = (line[i] + ((left + prev[i]) >> 1)) & 255
        elif kind == 4:
            for i in range(stride):
                a = line[i - 4] if i >= 4 else 0
                b = prev[i]
                c = prev[i - 4] if i >= 4 else 0
                pa, pb, pc = abs(b - c), abs(a - c), abs(a + b - 2 * c)
                pred = a if (pa <= pb and pa <= pc) else (b if pb <= pc else c)
                line[i] = (line[i] + pred) & 255
        pixels[y * stride:(y + 1) * stride] = line
        prev = line
    return Image(width, height, pixels)


def write_png(path, image):
    stride = image.width * 4
    raw = bytearray()
    for y in range(image.height):
        raw.append(0)
        raw.extend(image.pixels[y * stride:(y + 1) * stride])

    def chunk(tag, data):
        body = tag + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(chunk(b"IHDR", struct.pack(">IIBBBBB", image.width, image.height, 8, 6, 0, 0, 0)))
        f.write(chunk(b"IDAT", zlib.compress(bytes(raw), 9)))
        f.write(chunk(b"IEND", b""))


# --------------------------------------------------------------------------- anm2

NUMERIC = ("XPosition", "YPosition", "XPivot", "YPivot", "XCrop", "YCrop", "Width", "Height",
           "XScale", "YScale", "Delay", "RedTint", "GreenTint", "BlueTint", "AlphaTint",
           "RedOffset", "GreenOffset", "BlueOffset", "Rotation")
LERPED = ("XPosition", "YPosition", "XScale", "YScale", "RedTint", "GreenTint", "BlueTint", "AlphaTint",
          "RedOffset", "GreenOffset", "BlueOffset", "Rotation")


def parse_frames(parent):
    frames = []
    for node in parent.findall("Frame"):
        frame = {k: float(node.get(k, 0)) for k in NUMERIC}
        frame["Visible"] = node.get("Visible", "true") == "true"
        frame["Interpolated"] = node.get("Interpolated", "false") == "true"
        frames.append(frame)
    return frames


def frame_at(frames, tick):
    """The keyframe state at a tick, interpolated towards the next keyframe where the file says so."""
    if not frames:
        return None
    start = 0
    for i, frame in enumerate(frames):
        delay = max(1, int(frame["Delay"]))
        last = i == len(frames) - 1
        if tick < start + delay or last:
            if frame["Interpolated"] and not last:
                t = min(1.0, max(0.0, (tick - start) / float(delay)))
                nxt = frames[i + 1]
                out = dict(frame)
                for k in LERPED:
                    out[k] = frame[k] + (nxt[k] - frame[k]) * t
                return out
            return frame
        start += delay
    return frames[-1]


class Anm2(object):
    def __init__(self, resources, relative_path):
        self.resources = resources
        root = ET.parse(os.path.join(resources, relative_path)).getroot()
        content = root.find("Content")
        self.sheets = {s.get("Id"): s.get("Path") for s in content.find("Spritesheets")}
        self.layers = {l.get("Id"): (l.get("Name"), l.get("SpritesheetId")) for l in content.find("Layers")}
        self.animations = {}
        for anim in root.find("Animations"):
            layers = []
            for la in anim.find("LayerAnimations"):
                if la.get("Visible", "true") != "true":
                    continue
                frames = parse_frames(la)
                if frames:
                    layers.append((la.get("LayerId"), frames))
            self.animations[anim.get("Name")] = {
                "length": int(anim.get("FrameNum")),
                "root": parse_frames(anim.find("RootAnimation")),
                "layers": layers,
            }
        self._images = {}

    def sheet(self, sheet_id):
        if sheet_id not in self._images:
            self._images[sheet_id] = read_png(find_case_insensitive(self.resources, "gfx/" + self.sheets[sheet_id]))
        return self._images[sheet_id]

    def draw(self, canvas, origin_x, origin_y, animation, tick):
        """Draws every layer of one animation tick with Isaac's position at (origin_x, origin_y)."""
        anim = self.animations[animation]
        root = frame_at(anim["root"], tick) or {"XPosition": 0, "YPosition": 0, "XScale": 100, "YScale": 100}
        for layer_id, frames in anim["layers"]:
            name, sheet_id = self.layers[layer_id]
            if name in SKIPPED_LAYERS:
                continue
            frame = frame_at(frames, tick)
            if frame is None or not frame["Visible"]:
                continue
            draw_layer(canvas, self.sheet(sheet_id), frame, root, origin_x, origin_y)


def find_case_insensitive(base, relative):
    """The .anm2 files spell paths with a different case than the files on disk."""
    current = base
    for part in relative.replace("\\", "/").split("/"):
        match = None
        for entry in os.listdir(current):
            if entry.lower() == part.lower():
                match = entry
                break
        if match is None:
            raise IOError("missing %s under %s" % (part, current))
        current = os.path.join(current, match)
    return current


def draw_layer(canvas, sheet, frame, root, origin_x, origin_y):
    sx = frame["XScale"] / 100.0 * root["XScale"] / 100.0
    sy = frame["YScale"] / 100.0 * root["YScale"] / 100.0
    if sx == 0 or sy == 0:
        return
    angle = math.radians(frame["Rotation"])
    cos_a, sin_a = math.cos(angle), math.sin(angle)
    px, py = frame["XPivot"], frame["YPivot"]
    tx = origin_x + root["XPosition"] + frame["XPosition"]
    ty = origin_y + root["YPosition"] + frame["YPosition"]
    w, h = int(frame["Width"]), int(frame["Height"])
    crop_x, crop_y = int(frame["XCrop"]), int(frame["YCrop"])

    # Destination bounding box of the transformed crop rectangle.
    corners = []
    for cx, cy in ((0, 0), (w, 0), (0, h), (w, h)):
        lx, ly = (cx - px) * sx, (cy - py) * sy
        corners.append((tx + lx * cos_a - ly * sin_a, ty + lx * sin_a + ly * cos_a))
    x0 = max(0, int(math.floor(min(c[0] for c in corners))))
    x1 = min(canvas.width, int(math.ceil(max(c[0] for c in corners))))
    y0 = max(0, int(math.floor(min(c[1] for c in corners))))
    y1 = min(canvas.height, int(math.ceil(max(c[1] for c in corners))))

    tint = (frame["RedTint"] / 255.0, frame["GreenTint"] / 255.0, frame["BlueTint"] / 255.0)
    offset = (frame["RedOffset"], frame["GreenOffset"], frame["BlueOffset"])
    alpha = frame["AlphaTint"] / 255.0

    for y in range(y0, y1):
        for x in range(x0, x1):
            # Inverse transform of the pixel centre back into the crop rectangle (nearest neighbour).
            dx, dy = x + 0.5 - tx, y + 0.5 - ty
            lx = (dx * cos_a + dy * sin_a) / sx + px
            ly = (-dx * sin_a + dy * cos_a) / sy + py
            ix, iy = int(math.floor(lx)), int(math.floor(ly))
            if ix < 0 or iy < 0 or ix >= w or iy >= h:
                continue
            ux, uy = crop_x + ix, crop_y + iy
            if ux >= sheet.width or uy >= sheet.height:
                continue
            r, g, b, a = sheet.get(ux, uy)
            if a == 0:
                continue
            colour = [min(255, max(0, int(round(c * t + o)))) for c, t, o in zip((r, g, b), tint, offset)]
            canvas.blend(x, y, colour[0], colour[1], colour[2], int(round(a * alpha)))


# --------------------------------------------------------------------------- clip mapping

def still(body, head):
    """One frame: the standing body with a head direction."""
    return [((body, 0), (head, 0))]


def walk(body, head, frames=6):
    """A walk cycle sampled down to `frames` frames; the head stays on its first frame."""
    return [((body, int(round(i * 20.0 / frames)) % 20), (head, 0)) for i in range(frames)]


def sampled(animation, frames):
    """A whole-body animation (its own layers only) sampled evenly to `frames` frames."""
    return [((animation, None, i, frames), None) for i in range(frames)]


SIDE = still("WalkDown", "HeadRight")
FRONT = still("WalkDown", "HeadDown")
BACK = still("WalkDown", "HeadUp")

# ETG clip -> list of frames. Each frame is (body, head): either (animation, tick) pairs to overlay,
# or a sampled whole-body animation. Alexandria needs run clips to have 1, 3, 4 or at least 6 frames
# and dodge clips to not have exactly 5.
CLIPS = {
    "idle": SIDE, "idle_hand": SIDE, "idle_twohands": SIDE,
    "idle_forward": FRONT, "idle_forward_hand": FRONT, "idle_forward_twohands": FRONT,
    "idle_backward": BACK, "idle_backward_hand": BACK, "idle_backward_twohands": BACK,
    "idle_bw": BACK, "idle_bw_twohands": BACK,

    "run_right": walk("WalkRight", "HeadRight"),
    "run_right_hand": walk("WalkRight", "HeadRight"),
    "run_right_twohands": walk("WalkRight", "HeadRight"),
    "run_down": walk("WalkDown", "HeadDown"),
    "run_down_hand": walk("WalkDown", "HeadDown"),
    "run_down_twohands": walk("WalkDown", "HeadDown"),
    "run_up": walk("WalkUp", "HeadUp"),
    "run_up_hand": walk("WalkUp", "HeadUp"),
    "run_up_twohands": walk("WalkUp", "HeadUp"),
    "run_right_bw": walk("WalkRight", "HeadUp"),
    "run_right_bw_twohands": walk("WalkRight", "HeadUp"),

    # Isaac has no dodge roll (MOD-13); these only exist so the clip names resolve.
    "dodge": FRONT, "dodge_bw": BACK, "dodge_left": SIDE, "dodge_left_bw": BACK,

    "death": sampled("Death", 12), "death_shot": sampled("Death", 12), "death_coop": sampled("Death", 12),
    "item_get": sampled("Pickup", 8),
    "chest_recover": sampled("Happy", 8),
    "pitfall": sampled("FallIn", 6), "pitfall_down": sampled("FallIn", 6),
    "pitfall_return": sampled("JumpOut", 6),
    "spit_out": sampled("JumpOut", 6),
    "spinfall": sampled("Jump", 8), "timefall": sampled("Jump", 8),
    "doorway": FRONT,
    "pet": SIDE,

    "slide_right": SIDE, "slide_down": FRONT, "slide_up": BACK,
    "jetpack_right": SIDE, "jetpack_right_hand": SIDE, "jetpack_right_bw": BACK,
    "jetpack_down": FRONT, "jetpack_down_hand": FRONT, "jetpack_up": BACK,
    "tablekick_right": SIDE * 3, "tablekick_right_hand": SIDE * 3,
    "tablekick_down": FRONT * 3, "tablekick_down_hand": FRONT * 3,
    "tablekick_up": BACK * 3,

    "custom/select_idle": FRONT,
    "custom/select_choose": sampled("Happy", 8),
}

# Clips this tool does not produce (the ghost ones); the placeholder squares stay for them.
CANVAS_HALF_WIDTH = 48
CANVAS_UP = 80
CANVAS_DOWN = 32


def render(anm2, frame):
    """Renders one mapped frame onto a generous canvas with Isaac's position at a fixed origin."""
    canvas = Image(CANVAS_HALF_WIDTH * 2, CANVAS_UP + CANVAS_DOWN)
    body, head = frame
    if len(body) == 4:
        animation, _, index, count = body
        length = anm2.animations[animation]["length"]
        tick = int(round(index * (length - 1) / float(max(1, count - 1))))
        anm2.draw(canvas, CANVAS_HALF_WIDTH, CANVAS_UP, animation, tick)
    else:
        anm2.draw(canvas, CANVAS_HALF_WIDTH, CANVAS_UP, body[0], body[1])
        anm2.draw(canvas, CANVAS_HALF_WIDTH, CANVAS_UP, head[0], head[1])
    return canvas


def opaque_bounds(image):
    xs, ys = [], []
    for y in range(image.height):
        row = image.pixels[y * image.width * 4:(y + 1) * image.width * 4]
        for x in range(image.width):
            if row[x * 4 + 3]:
                xs.append(x)
                ys.append(y)
    if not xs:
        return None
    return min(xs), min(ys), max(xs), max(ys)


def crop(image, x0, y0, x1, y1):
    out = Image(x1 - x0, y1 - y0)
    for y in range(y0, y1):
        src = (y * image.width + x0) * 4
        dst = (y - y0) * out.width * 4
        out.pixels[dst:dst + out.width * 4] = image.pixels[src:src + out.width * 4]
    return out


def main():
    if len(sys.argv) != 2 or not os.path.isfile(os.path.join(sys.argv[1], PLAYER_ANM2)):
        sys.exit("usage: python tools/anm2_to_frames.py <extracted>/resources\n"
                 "       (the folder that contains %s)" % PLAYER_ANM2)
    anm2 = Anm2(sys.argv[1], PLAYER_ANM2)

    # Isaac's feet: the lowest opaque row of the standing pose. Every clip is cut off there.
    feet = opaque_bounds(render(anm2, FRONT[0]))[3] + 1

    sprites = os.path.join(OUT, "newspritesetup")
    written = 0
    for clip in sorted(CLIPS):
        rendered = [render(anm2, frame) for frame in CLIPS[clip]]
        boxes = [b for b in (opaque_bounds(r) for r in rendered) if b]
        if not boxes:
            print("skipped %s: nothing visible" % clip)
            continue
        # One canvas per clip: symmetric around Isaac's x, top of the tallest frame, bottom at his feet.
        half = max(max(CANVAS_HALF_WIDTH - b[0], b[2] + 1 - CANVAS_HALF_WIDTH) for b in boxes)
        top = min(b[1] for b in boxes)
        folder = os.path.join(sprites, *clip.split("/"))
        if os.path.isdir(folder):
            shutil.rmtree(folder)
        name = clip.split("/")[-1]
        for i, image in enumerate(rendered):
            cut = crop(image, CANVAS_HALF_WIDTH - half, top, CANVAS_HALF_WIDTH + half, feet)
            write_png(os.path.join(folder, "%s_%03d.png" % (name, i + 1)), cut)
            written += 1

    # HUD portrait: the front-facing head on the 34x34 card Alexandria expects.
    head = Image(CANVAS_HALF_WIDTH * 2, CANVAS_UP + CANVAS_DOWN)
    anm2.draw(head, CANVAS_HALF_WIDTH, CANVAS_UP, "HeadDown", 0)
    x0, y0, x1, y1 = opaque_bounds(head)
    cx, cy = (x0 + x1 + 1) // 2, (y0 + y1 + 1) // 2
    write_png(os.path.join(OUT, "facecard.png"), crop(head, cx - 17, cy - 17, cx + 17, cy + 17))

    print("Wrote %d frames for %d clips under %s" % (written, len(CLIPS), sprites))


if __name__ == "__main__":
    main()
