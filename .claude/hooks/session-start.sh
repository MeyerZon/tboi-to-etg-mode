#!/bin/bash
# SessionStart hook for Claude Code cloud sessions.
# Installs the .NET 8 SDK (needed to build the net35 mod and run the tests), Pillow for the
# sprite/icon tooling, and warms the NuGet cache. Idempotent and non-interactive.
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if ! "$DOTNET_ROOT/dotnet" --list-sdks 2>/dev/null | grep -q '^8\.'; then
  echo "[session-start] installing .NET 8 SDK into $DOTNET_ROOT"
  curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "$DOTNET_ROOT" >/dev/null
fi

if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_ROOT\""
    echo "export PATH=\"$DOTNET_ROOT:\$PATH\""
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1"
  } >> "$CLAUDE_ENV_FILE"
fi
export PATH="$DOTNET_ROOT:$PATH"

if ! python3 -c "import PIL" 2>/dev/null; then
  echo "[session-start] installing Pillow"
  python3 -m pip install --quiet --user pillow 2>/dev/null || python3 -m pip install --quiet --user --break-system-packages pillow
fi

cd "${CLAUDE_PROJECT_DIR:-$(pwd)}"
echo "[session-start] restoring NuGet packages"
dotnet restore IsaacMode.sln -nologo -v q

echo "[session-start] ready: $(dotnet --version)"
