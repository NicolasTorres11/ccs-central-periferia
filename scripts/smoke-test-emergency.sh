#!/usr/bin/env bash
set -euo pipefail

api_url="${EMERGENCY_API_URL:-http://localhost:5101}"
correlation_id="${CORRELATION_ID:-11111111-1111-1111-1111-111111111111}"

echo "Ejecutando smoke test local contra ${api_url}."

payload='{
  "deviceId": "DEV-SMOKE",
  "type": "Panic",
  "source": "Button",
  "gps": { "lat": 4.65, "lng": -74.1 },
  "timestamp": "2026-05-13T12:00:00Z",
  "reason": "panic-button"
}'

status_code="$(
  curl -sS -o /tmp/ccs-emergency-response.json -w "%{http_code}" \
    -X POST "${api_url}/emergency" \
    -H "Content-Type: application/json" \
    -H "x-correlation-id: ${correlation_id}" \
    -d "${payload}"
)"

if [[ "${status_code}" != "202" ]]; then
  echo "Smoke test fallido: se esperaba HTTP 202 y se recibio HTTP ${status_code}."
  cat /tmp/ccs-emergency-response.json
  exit 1
fi

echo "Smoke test OK: senal de panico aceptada con correlationId ${correlation_id}."
