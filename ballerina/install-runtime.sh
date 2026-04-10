#!/usr/bin/env bash
set -euo pipefail

# Install the Ballerina runtime and optionally the VS Code extension.
#
# Defaults:
#   INSTALL_PREFIX=/usr/local
#   RUNTIME_DIR=$INSTALL_PREFIX/lib/ballerina/runtime
#   BIN_DIR=$INSTALL_PREFIX/bin
#   EXE_NAME=ballerina
#
# Optional env overrides:
#   INSTALL_PREFIX, RUNTIME_DIR, BIN_DIR, EXE_NAME, CONFIGURATION, RUNTIME

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${PROJECT_PATH:-${SCRIPT_DIR}/ballerina.fsproj}"
CONFIGURATION="${CONFIGURATION:-Release}"
INSTALL_PREFIX="${INSTALL_PREFIX:-/usr/local}"
RUNTIME_DIR="${RUNTIME_DIR:-${INSTALL_PREFIX}/lib/ballerina/runtime}"
BIN_DIR="${BIN_DIR:-${INSTALL_PREFIX}/bin}"
EXE_NAME="${EXE_NAME:-ballerina}"
SOURCE_EXE_NAME="$(basename "${PROJECT_PATH}" .fsproj)"

if [[ -z "${RUNTIME:-}" ]]; then
  case "$(uname -s)-$(uname -m)" in
    Darwin-arm64)  RUNTIME="osx-arm64"   ;;
    Darwin-x86_64) RUNTIME="osx-x64"     ;;
    Linux-x86_64)  RUNTIME="linux-x64"   ;;
    Linux-aarch64) RUNTIME="linux-arm64" ;;
    *)
      echo "ERROR: Unsupported platform. Set RUNTIME manually (e.g. osx-arm64, linux-x64)."
      exit 1
      ;;
  esac
fi

PUBLISH_DIR="$(mktemp -d "${TMPDIR:-/tmp}/ballerina-publish-${RUNTIME}-XXXXXX")"
trap 'rm -rf "${PUBLISH_DIR}"' EXIT

has_required_assets() {
  local asset_root="$1"
  [[ -f "${asset_root}/default/model.onnx" && -f "${asset_root}/default/vocab.txt" ]]
}

# ── Runtime ──────────────────────────────────────────────────────────────────

echo "==> Publishing runtime"
echo "    Project: ${PROJECT_PATH}"
echo "    Runtime: ${RUNTIME}"
echo "    Output : ${PUBLISH_DIR}"

dotnet publish "${PROJECT_PATH}" \
  -c "${CONFIGURATION}" \
  -r "${RUNTIME}" \
  --self-contained true \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false \
  -o "${PUBLISH_DIR}"

SOURCE_EXE="${PUBLISH_DIR}/${SOURCE_EXE_NAME}"
if [[ ! -f "${SOURCE_EXE}" ]]; then
  echo "ERROR: Expected published executable at ${SOURCE_EXE}"
  exit 1
fi

echo "==> Installing runtime"
echo "    Runtime dir: ${RUNTIME_DIR}"
echo "    Bin dir    : ${BIN_DIR}"

mkdir -p "${RUNTIME_DIR}"
mkdir -p "${BIN_DIR}"

echo "==> Copying runtime payload"
rm -rf "${RUNTIME_DIR:?}"/*
cp -R "${PUBLISH_DIR}/." "${RUNTIME_DIR}/"

if [[ "${SOURCE_EXE_NAME}" != "${EXE_NAME}" ]]; then
  install -m 0755 "${RUNTIME_DIR}/${SOURCE_EXE_NAME}" "${RUNTIME_DIR}/${EXE_NAME}"
fi

# Copy LocalEmbeddingsModel assets if present
ASSET_SOURCE_DIR=""
if has_required_assets "${PUBLISH_DIR}/LocalEmbeddingsModel"; then
  ASSET_SOURCE_DIR="${PUBLISH_DIR}/LocalEmbeddingsModel"
elif has_required_assets "${SCRIPT_DIR}/bin/${CONFIGURATION}/net10.0/${RUNTIME}/LocalEmbeddingsModel"; then
  ASSET_SOURCE_DIR="${SCRIPT_DIR}/bin/${CONFIGURATION}/net10.0/${RUNTIME}/LocalEmbeddingsModel"
elif has_required_assets "${SCRIPT_DIR}/bin/Debug/net10.0/LocalEmbeddingsModel"; then
  ASSET_SOURCE_DIR="${SCRIPT_DIR}/bin/Debug/net10.0/LocalEmbeddingsModel"
fi

if [[ -n "${ASSET_SOURCE_DIR}" ]]; then
  ASSET_TARGET_DIR="${RUNTIME_DIR}/LocalEmbeddingsModel"
  echo "==> Copying runtime assets"
  echo "    Source : ${ASSET_SOURCE_DIR}"
  echo "    Target : ${ASSET_TARGET_DIR}"
  rm -rf "${RUNTIME_DIR}/LocalEmbeddingsModel"
  cp -R "${ASSET_SOURCE_DIR}" "${RUNTIME_DIR}/LocalEmbeddingsModel"
fi

cat > "${BIN_DIR}/${EXE_NAME}" <<EOF
#!/usr/bin/env bash
set -euo pipefail
exec "${RUNTIME_DIR}/${EXE_NAME}" "\$@"
EOF
chmod +x "${BIN_DIR}/${EXE_NAME}"

echo ""
echo "✓ Runtime installed"
echo "  Launcher : ${BIN_DIR}/${EXE_NAME}"
echo "  Runtime  : ${RUNTIME_DIR}"

# ── VS Code extension ─────────────────────────────────────────────────────────

EXTENSION_DIR="${SCRIPT_DIR}/vscode-bl-extension"

if [[ ! -d "${EXTENSION_DIR}" ]]; then
  echo ""
  echo "  (VS Code extension source not found at ${EXTENSION_DIR}, skipping)"
else
  echo ""
  echo "==> Installing VS Code extension"

  if ! command -v code &>/dev/null; then
    echo "  WARNING: 'code' CLI not found — install the extension manually:"
    echo "    1. Open VS Code"
    echo "    2. Press Cmd+Shift+P (macOS) or Ctrl+Shift+P (Linux/Windows)"
    echo "    3. Run: Extensions: Install from VSIX..."
    echo "    4. Select the .vsix file from: ${EXTENSION_DIR}"
  else
    # Build extension if out/ is missing or stale
    if [[ ! -f "${EXTENSION_DIR}/out/extension.js" ]]; then
      echo "  Building extension..."
      (cd "${EXTENSION_DIR}" && npm install && npm run compile)
    fi

    # Package as .vsix if vsce is available
    VSIX_PATH="${EXTENSION_DIR}/bl-language-tools.vsix"
    if command -v vsce &>/dev/null; then
      echo "  Packaging extension..."
      (cd "${EXTENSION_DIR}" && vsce package --out "${VSIX_PATH}" 2>/dev/null) || true
    fi

    if [[ -f "${VSIX_PATH}" ]]; then
      code --install-extension "${VSIX_PATH}"
      echo "  ✓ VS Code extension installed"
    else
      # Fall back to installing the extension directory directly
      EXT_INSTALL_DIR="${HOME}/.vscode/extensions/ballerina-bl-language-tools"
      echo "  Installing extension directory to ${EXT_INSTALL_DIR}"
      mkdir -p "${EXT_INSTALL_DIR}"
      cp -R "${EXTENSION_DIR}/." "${EXT_INSTALL_DIR}/"
      echo "  ✓ VS Code extension installed (restart VS Code to activate)"
    fi
  fi
fi

echo ""
echo "Run with:"
echo "  ${EXE_NAME} --help"
echo "  ${EXE_NAME} -f path/to/program.bl"
echo "  ${EXE_NAME} -f path/to/program.bl --run"
echo "  ${EXE_NAME} -f path/to/program.blproj --run"
