# Disponibilidad y escalabilidad de aplicacion

## Estrategia general

La solucion separa el camino critico de panico de la telemetria rutinaria. El flujo de emergencia usa API dedicada, Redis para reglas activas y Service Bus con cola/topico critico. La telemetria usa Event Hubs y procesamiento asincrono para absorber picos sin presionar el SLA de panico.

## Componentes

| Componente | Disponibilidad | Escalabilidad | Resiliencia |
|---|---|---|---|
| Azure Front Door + WAF | Entrada global administrada y health probes. | Distribuye trafico hacia origen saludable. | Proteccion perimetral y failover de origen. |
| API Management | Publicacion central de APIs. | Escalado por unidades/capacidad del servicio. | Cuotas, throttling y versionado de contratos. |
| CCS.Api.Emergency | Servicio dedicado al flujo de panico. | Escala horizontalmente de forma independiente. | Validacion rapida, correlationId y dependencia caliente en Redis. |
| CCS.Api.Admin | Gestion de propietarios, vehiculos y reglas. | Escala separado del flujo critico. | Cambios persistidos en SQL e invalidacion de cache Redis. |
| CCS.Api.Telemetry | Ingesta HTTP alternativa para telemetria. | Escala separado y publica asincronicamente. | No bloquea el flujo critico de panico. |
| CCS.Functions.Ingestion | Procesa batches de telemetria. | Escalado por particiones/eventos. | Reintentos y DLQ segun trigger objetivo. |
| CCS.Functions.Rules | Evalua eventos derivados. | Escala por particion de Event Hubs. | Usa Redis para reglas activas y Cosmos para estado operativo. |
| CCS.Functions.Dispatcher | Despacha acciones de emergencia. | Escala por mensajes pendientes en Service Bus. | Reintentos, DLQ y fan-out paralelo a canales externos. |
| Event Hubs | Buffer de alta tasa para telemetria. | Particiones por `deviceId`. | Retencion temporal y consumidores desacoplados. |
| Service Bus | Desacopla acciones a ejecutar. | Topicos/colas separados para critico y no critico. | DLQ, lock de mensajes, reintentos e idempotencia. |
| Redis | Cache de reglas activas. | Lecturas de baja latencia. | Evita SQL en el camino caliente; se reconstruye desde SQL. |
| Application Insights | Observabilidad end-to-end. | Ingesta administrada de metricas y trazas. | Alertas por latencia, errores y acumulacion de mensajes. |

## Garantias del camino critico

- El endpoint `/emergency` solo hace validacion, consulta cache y publica una accion critica.
- Las acciones externas se ejecutan fuera del request HTTP mediante `CCS.Functions.Dispatcher`.
- Las reglas activas se resuelven desde Redis para evitar joins y consultas relacionales bajo presion.
- El canal `actions-critical` permite priorizar panico sobre acciones rutinarias.

## Escalamiento operativo

- Escalar APIs por CPU, latencia P95 y tasa de errores.
- Escalar Functions por backlog de Event Hubs o Service Bus.
- Aumentar particiones de Event Hubs cuando crezca el volumen de dispositivos.
- Mantener dashboards por `correlationId`, latencia de emergencia, mensajes en DLQ y errores por canal externo.
