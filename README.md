# CCS Central Periferia

Solucion tecnica para la central de seguimiento vehicular CCS. El repositorio incluye documentacion de arquitectura, modelo ER, scripts de base de datos, Bicep documentado, contratos OpenAPI y una base .NET con pruebas automatizadas.

## Estructura

```text
docs/       Documentacion tecnica, OpenAPI y diagramas.
database/   SQL, Cosmos, Redis y modelo ER DBML.
infra/      Bicep documentado para Azure, sin despliegue real por defecto.
src/        Codigo fuente .NET.
tests/      Pruebas automatizadas.
scripts/    Scripts auxiliares locales.
```

## Pruebas .NET

El entorno local actual usa SDK .NET 10 para ejecutar la solucion. Los proyectos estan organizados con arquitectura por capas y pruebas sobre dominio, aplicacion e infraestructura.

```bash
dotnet restore CCS.slnx
dotnet test CCS.slnx --disable-build-servers -m:1
```

En este entorno, `dotnet test` puede requerir ejecucion fuera del sandbox por restricciones de MSBuild con named pipes.

## APIs locales

Las APIs se ejecutan localmente y usan almacenamiento en memoria. No publican ni consultan recursos de Azure.

### Emergencias

```bash
dotnet run --project src/CCS.Api.Emergency --urls http://localhost:5101
```

```bash
curl -X POST http://localhost:5101/emergency \
  -H "Content-Type: application/json" \
  -H "x-correlation-id: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "deviceId": "DEV-SMOKE",
    "type": "Panic",
    "source": "PhysicalButton",
    "gps": { "lat": 4.711, "lng": -74.072 },
    "timestamp": "2026-05-12T14:23:10Z"
  }'
```

### Telemetria

```bash
dotnet run --project src/CCS.Api.Telemetry --urls http://localhost:5102
```

```bash
curl -X POST http://localhost:5102/telemetry/ingest \
  -H "Content-Type: application/json" \
  -d '[{
    "deviceId": "DEV-SMOKE",
    "ts": "2026-05-12T14:23:10Z",
    "gps": { "lat": 4.711, "lng": -74.072 },
    "speedKmh": 48,
    "temperatureC": 6.5,
    "eventType": "telemetry"
  }]'
```

### Administracion

```bash
dotnet run --project src/CCS.Api.Admin --urls http://localhost:5103
```

```bash
curl -X POST http://localhost:5103/owners \
  -H "Content-Type: application/json" \
  -d '{
    "documentType": "CC",
    "documentNumber": "123456",
    "fullName": "Propietario Demo",
    "email": "demo@example.com",
    "phoneE164": "+573001111111"
  }'
```

## Modelo ER

El modelo ER principal esta en formato DBML para dbdiagram.io:

```text
database/model/ccs-er-model.dbml
```

## Infraestructura

Los archivos Bicep estan en `infra/` como artefacto tecnico revisable. No se despliegan recursos reales desde este repositorio por defecto.

## Documentacion principal

- `docs/architecture.md`
- `docs/sequence-diagrams.md`
- `docs/er-model.md`
- `docs/openapi.yaml`
