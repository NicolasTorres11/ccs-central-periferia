# Arquitectura de la Central CCS

## Objetivo

Disenar una central de procesamiento de senales vehiculares que permita:

- Ejecutar acciones de emergencia en menos de 2 segundos.
- Procesar 500 senales por segundo durante 2 minutos.
- Escalar de 9.500 vehiculos actuales a aproximadamente 16.400 en 3 anos.
- Mantener disponibilidad y escalabilidad a nivel de aplicacion.

## Vista de Componentes

![Diagrama de Componentes — Arquitectura Azure cloud-native Central CCS](../architectures/Diagrama%20de%20Componentes.png)

## Camino Critico de Emergencia

Para cumplir el SLA menor a 2 segundos, el panico usa un camino dedicado:

```text
App/Boton -> API Management -> CCS.Api.Emergency -> Redis -> Service Bus actions-critical -> CCS.Functions.Dispatcher
```

Presupuesto objetivo de latencia P95:

| Paso                                       | Latencia objetivo |
| ------------------------------------------ | ----------------: |
| API Management + validacion                |             50 ms |
| Emergency API + validacion payload         |             80 ms |
| Consulta reglas en Redis                   |             10 ms |
| Publicacion Service Bus `actions-critical` |             80 ms |
| Trigger dispatcher                         |            150 ms |
| Fan-out paralelo a canales externos        |            800 ms |
| Margen operativo                           |            830 ms |
| Total objetivo                             |        < 2.000 ms |

## Flujo de Telemetria

La telemetria rutinaria usa una ruta asincrona y desacoplada:

```text
Dispositivo -> IoT Hub -> Event Hubs -> Ingestion -> Cosmos DB -> RulesEngine -> Service Bus -> Dispatcher
```

Esta separacion evita que picos de telemetria afecten el canal critico de panico.

## Justificacion de Componentes

| Componente              | Uso                                     | Justificacion                                                                    |
| ----------------------- | --------------------------------------- | -------------------------------------------------------------------------------- |
| IoT Hub                 | Entrada MQTT/AMQP de sensores           | Autenticacion por dispositivo, escalabilidad administrada y enrute a Event Hubs. |
| Event Hubs              | Bus de telemetria                       | Alto throughput y particionamiento por `deviceId`.                               |
| Service Bus             | Acciones a despachar                    | Topics, DLQ, reintentos y priorizacion mediante `actions-critical`.              |
| Azure Functions Premium | Workers de ingesta, reglas y despacho   | Escalado independiente y sin cold starts relevantes para el SLA.                 |
| Azure SQL               | Maestros, reglas, contactos y auditoria | Consistencia ACID y consultas relacionales.                                      |
| Cosmos DB               | Telemetria, eventos y estado vehicular  | Escritura de alto volumen, TTL y particionamiento por `deviceId`.                |
| Redis                   | Reglas activas del camino critico       | Lecturas sub-milisegundo para panico y motor de reglas.                          |
| API Management          | Publicacion de APIs                     | Seguridad, cuotas, versionado y OpenAPI.                                         |
| Application Insights    | Observabilidad                          | Trazas con correlationId, metricas y alertas.                                    |

## Escalabilidad

- Functions separadas por responsabilidad para escalar de forma independiente.
- Particionamiento de telemetria por `deviceId`.
- Cosmos con autoscale y TTL para datos efimeros.
- SQL con indices por propietario, vehiculo, regla y fechas de auditoria.
- Redis precargado con reglas activas para evitar consultas SQL en el camino caliente.

## Disponibilidad

- Servicios Azure con soporte de zonas de disponibilidad donde aplique.
- Cosmos DB con region secundaria.
- Retries, circuit breaker y DLQ para canales externos.
- Health checks y alertas por latencia, errores y acumulacion de mensajes.

La matriz detallada de disponibilidad y escalabilidad esta en `docs/availability-scalability.md`.
