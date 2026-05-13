#!/usr/bin/env bash
set -euo pipefail

ENVIRONMENT="${1:-dev}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-ccs-${ENVIRONMENT}}"
LOCATION="${LOCATION:-eastus2}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cat <<EOF
Este repositorio no despliega recursos reales de Azure por defecto.

Comandos de referencia para una ejecucion controlada:

az group create \\
  --name "$RESOURCE_GROUP" \\
  --location "$LOCATION"

az deployment group create \\
  --resource-group "$RESOURCE_GROUP" \\
  --template-file "$ROOT_DIR/infra/main.bicep" \\
  --parameters "$ROOT_DIR/infra/params/${ENVIRONMENT}.bicepparam"

Para esta prueba tecnica los archivos Bicep quedan como artefacto de infraestructura documentado.
EOF
