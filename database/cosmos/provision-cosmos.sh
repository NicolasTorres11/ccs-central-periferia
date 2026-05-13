#!/usr/bin/env bash
set -euo pipefail

RG="${RG:-rg-ccs-prod}"
LOC_PRIMARY="${LOC_PRIMARY:-eastus2}"
LOC_SECONDARY="${LOC_SECONDARY:-brazilsouth}"
ACCT="${ACCT:-cosmos-ccs-prod}"
DB="${DB:-ccs}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

az cosmosdb create \
  --name "$ACCT" \
  --resource-group "$RG" \
  --kind GlobalDocumentDB \
  --locations regionName="$LOC_PRIMARY" failoverPriority=0 isZoneRedundant=True \
  --locations regionName="$LOC_SECONDARY" failoverPriority=1 isZoneRedundant=True \
  --default-consistency-level "Session" \
  --enable-automatic-failover true \
  --enable-multiple-write-locations false \
  --backup-policy-type Continuous

az cosmosdb sql database create \
  --account-name "$ACCT" \
  --resource-group "$RG" \
  --name "$DB"

az cosmosdb sql container create \
  --account-name "$ACCT" \
  --resource-group "$RG" \
  --database-name "$DB" \
  --name "telemetry" \
  --partition-key-path "/deviceId" \
  --max-throughput 40000 \
  --ttl 7776000 \
  --idx @"$SCRIPT_DIR/indexing-policies/telemetry.json"

az cosmosdb sql container create \
  --account-name "$ACCT" \
  --resource-group "$RG" \
  --database-name "$DB" \
  --name "events" \
  --partition-key-path "/deviceId" \
  --max-throughput 20000 \
  --ttl 31536000 \
  --idx @"$SCRIPT_DIR/indexing-policies/events.json"

az cosmosdb sql container create \
  --account-name "$ACCT" \
  --resource-group "$RG" \
  --database-name "$DB" \
  --name "vehicleState" \
  --partition-key-path "/deviceId" \
  --max-throughput 10000 \
  --idx @"$SCRIPT_DIR/indexing-policies/vehicleState.json"

echo "Cosmos DB provisionado correctamente."

