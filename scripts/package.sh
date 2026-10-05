#!/usr/bin/env bash
# Build the mod in Release and assemble a Thunderstore package zip under artifacts/.
# Usage: scripts/package.sh            (version taken from Directory.Build.props)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

VERSION="$(sed -n 's:.*<ModVersion>\(.*\)</ModVersion>.*:\1:p' Directory.Build.props)"
if [ -z "$VERSION" ]; then echo "ModVersion not found in Directory.Build.props" >&2; exit 1; fi

MANIFEST_VERSION="$(python3 -c 'import json,sys; print(json.load(open("thunderstore/manifest.json"))["version_number"])')"
if [ "$MANIFEST_VERSION" != "$VERSION" ]; then
  echo "Version mismatch: Directory.Build.props=$VERSION thunderstore/manifest.json=$MANIFEST_VERSION" >&2
  exit 1
fi

dotnet build src/IsaacMode/IsaacMode.csproj -c Release -nologo

STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT
PKG="$STAGE/IsaacMode"
mkdir -p "$PKG/plugins/IsaacMode"

cp src/IsaacMode/bin/Release/net35/IsaacMode.dll "$PKG/plugins/IsaacMode/"
cp thunderstore/manifest.json thunderstore/icon.png "$PKG/"
cp README.md CHANGELOG.md "$PKG/"

python3 - "$PKG/icon.png" <<'PY'
import sys
from PIL import Image
w, h = Image.open(sys.argv[1]).size
if (w, h) != (256, 256):
    sys.exit(f"icon.png must be 256x256, got {w}x{h}")
PY

mkdir -p artifacts
OUT="$ROOT/artifacts/IsaacMode-$VERSION.zip"
rm -f "$OUT"
(cd "$PKG" && python3 -c 'import shutil,sys; shutil.make_archive(sys.argv[1][:-4], "zip", ".")' "$OUT")
echo "Wrote $OUT"
unzip -l "$OUT"
