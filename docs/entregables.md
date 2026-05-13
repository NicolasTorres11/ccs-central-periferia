# Trazabilidad de entregables

Este documento resume donde se responde cada punto solicitado en la prueba tecnica.

| # | Entregable solicitado | Estado | Evidencia en el repositorio |
|---|---|---|---|
| 1 | Diagrama de secuencia de cada flujo, despliegue y componentes de la solucion propuesta. | Cubierto | `docs/sequence-diagrams.md`, `docs/deployment-diagram.md`, `docs/architecture.md` |
| 2 | Justificar componentes definidos y explicar disponibilidad/escalabilidad a nivel de aplicacion. | Cubierto | `docs/architecture.md`, `docs/availability-scalability.md`, `docs/decisions/ADR-001-emergency-hot-path.md`, `docs/decisions/ADR-002-data-storage-split.md` |
| 3 | Modelo entidad/relacion y explicacion de escalabilidad a nivel de base de datos. | Cubierto | `docs/er-model.md`, `database/model/ccs-er-model.dbml`, `docs/diagrams/er-model.mmd` |
| 4 | Scripts de base de datos. | Cubierto | `database/sql/00_create_database.sql`, `database/sql/01_schema.sql`, `database/sql/02_indexes.sql`, `database/sql/03_partitioning.sql`, `database/sql/04_seed_catalogs.sql`, `database/sql/05_security.sql`, `database/cosmos/*`, `database/redis/*` |
| 5 | Codigo fuente, tests automaticos y code coverage mayor a 50%. | Cubierto | `src/`, `tests/`, `coverlet.runsettings`, `docs/test-coverage.md` |
| 6 | Instrucciones de como ejecutar la solucion. | Cubierto | `README.md`, `scripts/smoke-test-emergency.sh`, `scripts/bootstrap-local.sh`, `scripts/apply-sql.sh`, `scripts/deploy-infra.sh` |
| 7 | Documentacion de servicios en OpenAPI 3.0 o superior. | Cubierto | `docs/openapi.yaml` con `openapi: 3.0.3` |

## Validacion local final

Comando de pruebas:

```bash
dotnet test CCS.slnx --disable-build-servers -m:1
```

Comando de cobertura:

```bash
dotnet test CCS.slnx --disable-build-servers -m:1 --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

La ultima validacion local registro 45 pruebas exitosas y cobertura global por union de lineas de 92.03%.

## Alcance

- No se despliegan recursos reales en Azure desde este repositorio.
- Los Bicep quedan como artefacto de infraestructura revisable.
- `docs/openapi.yaml` es un contrato documental local, no una plataforma de ejecucion.
- `CCS.Functions.*` representa componentes tipo Functions como clases .NET testeables localmente.
