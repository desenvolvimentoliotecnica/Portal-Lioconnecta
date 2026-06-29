#!/usr/bin/env bash
set -euo pipefail

SUDO_PASS="${1:-}"
GH_PAT="${2:-}"
REG_TOKEN="${3:-}"
RUNNER_DIR="${RUNNER_DIR:-/home/administrator/actions-runner}"
REPO_URL="https://github.com/desenvolvimentoliotecnica/Portal-Lioconnecta"
RUNNER_NAME="${RUNNER_NAME:-lioconnecta-dev-runner}"
RUNNER_LABELS="${RUNNER_LABELS:-lioconnecta,linux}"
OLD_SERVICE="actions.runner.munizlmachado-jpg-RH.svr-dev-tst-portalrh-dev.service"

if [ -z "$SUDO_PASS" ] || [ -z "$GH_PAT" ] || [ -z "$REG_TOKEN" ]; then
  echo "Uso interno: migrate-runner.sh <sudo_pass> <gh_pat> <registration_token>" >&2
  exit 1
fi

sudo_cmd() {
  echo "$SUDO_PASS" | sudo -S "$@"
}

cd "$RUNNER_DIR"

echo "==> Parando runner antigo"
if systemctl is-active --quiet "$OLD_SERVICE" 2>/dev/null; then
  sudo_cmd systemctl stop "$OLD_SERVICE"
fi

if [ -x ./svc.sh ]; then
  sudo_cmd ./svc.sh stop || true
  sudo_cmd ./svc.sh uninstall || true
fi

echo "==> Removendo registro local antigo (se existir)"
./config.sh remove --local 2>/dev/null || true
rm -f .runner .credentials .credentials_rsaparams .service

echo "==> Registrando runner no repo ${REPO_URL}"
./config.sh \
  --url "$REPO_URL" \
  --token "$REG_TOKEN" \
  --name "$RUNNER_NAME" \
  --labels "$RUNNER_LABELS" \
  --unattended \
  --replace

echo "==> Instalando servico systemd"
sudo_cmd ./svc.sh install
sudo_cmd ./svc.sh start
sudo_cmd ./svc.sh status || true

echo "==> Migracao concluida"
