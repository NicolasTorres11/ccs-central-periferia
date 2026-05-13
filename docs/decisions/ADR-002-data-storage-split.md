# ADR-002: Separacion de Almacenamiento de Datos

## Estado
Propuesto

## Decision
Usar Azure SQL para datos transaccionales/maestros y Azure Cosmos DB para telemetria y eventos de alto volumen.

## Consecuencias
- SQL mantiene consistentes los datos de propietarios, vehiculos, reglas, contactos y auditoria.
- Cosmos absorbe la telemetria de alta escritura con TTL y particionamiento por `deviceId`.
- Redis sirve las reglas activas en el camino critico.
