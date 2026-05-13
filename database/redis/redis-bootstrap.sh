#!/usr/bin/env bash
set -euo pipefail

: "${REDIS_HOST:?Debe definir REDIS_HOST}"
: "${REDIS_KEY:?Debe definir REDIS_KEY}"

REDIS_PORT="${REDIS_PORT:-6380}"

redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_KEY" --tls PING
redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_KEY" --tls CONFIG SET notify-keyspace-events "KEA"

echo "Redis configurado para invalidacion de reglas."

