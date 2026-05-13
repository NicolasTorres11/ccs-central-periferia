# Modelo Entidad Relacion

## Separacion de Datos

| Motor | Responsabilidad |
|---|---|
| Azure SQL | Propietarios, vehiculos, dispositivos, reglas, acciones, contactos, autoridades, auditoria y notificaciones. |
| Cosmos DB | Telemetria cruda, eventos operativos y estado actual por vehiculo/dispositivo. |
| Redis | Reglas activas por `deviceId` y datos calientes del camino critico. |
| Blob Storage | Snapshots, evidencias y referencias a video. |

## Entidades SQL

- `Owner`: propietario del vehiculo.
- `Vehicle`: vehiculo afiliado.
- `Device`: sensor fisico instalado.
- `Rule`: regla definida por propietario.
- `RuleAction`: accion asociada a una regla.
- `EmergencyContact`: contactos de emergencia.
- `Authority`: autoridades y organismos de socorro.
- `AuditLog`: auditoria de cambios.
- `NotificationLog`: registro de notificaciones enviadas.

## Diagrama ER

![Modelo Entidad Relacion — Central CCS (Azure SQL)](../architectures/er-model-ccs-periferia.png)

El modelo DBML fuente esta en `database/model/ccs-er-model.dbml` (abrir en dbdiagram.io para edicion).

## Escalabilidad de Base de Datos
- SQL usa indices sobre `OwnerId`, `VehicleId`, `EventType`, `CorrelationId` y fechas operativas.
- `AuditLog` y `NotificationLog` se particionan por mes.
- Cosmos usa partition key `/deviceId`, TTL e indexing policies optimizadas.
- Redis evita consultas SQL en el camino critico de emergencia.
