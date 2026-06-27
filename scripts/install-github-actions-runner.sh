#!/usr/bin/env bash
# Instala GitHub Actions self-hosted runner (Linux x64) para deploy LIOCONNECTA.
# Uso no servidor (ex.: 10.0.0.79):
#   curl -fsSL .../install-github-actions-runner.sh | bash -s -- <REGISTRATION_TOKEN>
# Ou:
#   ./scripts/install-github-actions-runner.sh <REGISTRATION_TOKEN>
set -euo pipefail

REPO_URL="https://github.com/desenvolvimentoliotecnica/PortalRH"
RUNNER_NAME="${RUNNER_NAME:-lioconnecta-dev-runner}"
RUNNER_LABELS="${RUNNER_LABELS:-lioconnecta,linux}"
RUNNER_DIR="${RUNNER_DIR:-/home/administrator/actions-runner}"
RUNNER_VERSION="${RUNNER_VERSION:-2.325.0}"

TOKEN="${1:-}"
if [ -z "$TOKEN" ]; then
  echo "Uso: $0 <registration-token>" >&2
  echo "Gere o token com:" >&2
  echo "  gh api --method POST repos/desenvolvimentoliotecnica/PortalRH/actions/runners/registration-token --jq .token" >&2
  exit 1
fi

ARCH="$(uname -m)"
case "$ARCH" in
  x86_64) RUNNER_ARCH="x64" ;;
  aarch64|arm64) RUNNER_ARCH="arm64" ;;
  *)
    echo "Arquitetura nao suportada: $ARCH" >&2
    exit 1
    ;;
esac

PACK="actions-runner-linux-${RUNNER_ARCH}-${RUNNER_VERSION}.tar.gz"
URL="https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/${PACK}"

echo "==> Preparando diretorio ${RUNNER_DIR}"
mkdir -p "$RUNNER_DIR"
cd "$RUNNER_DIR"

if [ -f ./config.sh ] && [ -f ./run.sh ]; then
  echo "==> Runner existente detectado; removendo registro anterior..."
  if [ -f ./svc.sh ] && [ -f /etc/systemd/system/actions.runner.*.service ]; then
    sudo ./svc.sh stop || true
    sudo ./svc.sh uninstall || true
  fi
  ./config.sh remove --token "$TOKEN" || true
fi

echo "==> Baixando runner v${RUNNER_VERSION} (${RUNNER_ARCH})"
curl -fsSL "$URL" -o "$PACK"
tar xzf "$PACK"
rm -f "$PACK"

echo "==> Configurando runner ${RUNNER_NAME} (${RUNNER_LABELS})"
./config.sh \
  --url "$REPO_URL" \
  --token "$TOKEN" \
  --name "$RUNNER_NAME" \
  --labels "$RUNNER_LABELS" \
  --unattended \
  --replace

echo "==> Instalando servico systemd"
sudo ./svc.sh install
sudo ./svc.sh start
sudo ./svc.sh status || true

echo "==> Runner instalado em ${RUNNER_DIR}"
