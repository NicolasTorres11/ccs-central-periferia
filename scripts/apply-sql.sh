#!/usr/bin/env bash
set -euo pipefail

: "${SQL_SERVER:?Debe definir SQL_SERVER. Ej: localhost,1433 o sql-ccs-prod.database.windows.net}"

SQL_DATABASE="${SQL_DATABASE:-ccs_db}"
SQL_AUTH_ARGS="${SQL_AUTH_ARGS:--G}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

sqlcmd -S "$SQL_SERVER" -d master $SQL_AUTH_ARGS -i "$SCRIPT_DIR/database/sql/00_create_database.sql"

for script in "$SCRIPT_DIR"/database/sql/0[1-5]_*.sql; do
  sqlcmd -S "$SQL_SERVER" -d "$SQL_DATABASE" $SQL_AUTH_ARGS -i "$script"
done

echo "Scripts SQL aplicados correctamente."
