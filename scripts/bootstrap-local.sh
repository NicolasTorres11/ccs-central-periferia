#!/usr/bin/env bash
set -euo pipefail

echo "Iniciando dependencias locales con Docker Compose."
echo "Este script no crea recursos en Azure."

docker compose -f docker-compose.dev.yml up -d
