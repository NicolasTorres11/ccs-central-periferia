# Diagramas de Secuencia

## Flujo 1 — Ingesta de Telemetria Rutinaria

Trigger: sensor del vehiculo emite senal cada 10 segundos. SLA: procesamiento asincrono (< 5 s aceptable).

![Flujo 1 — Ingesta de Telemetria Rutinaria](../architectures/Diagrama%20de%20flujo%201.png)

## Flujo 2 — Emergencia / Boton de Panico

Trigger: boton fisico o app movil. SLA: end-to-end < 2 s.

![Flujo 2 — Emergencia / Boton de Panico (camino critico < 2s)](../architectures/Diagrama%20de%20flujo%202.png)

## Flujo 3 — Evaluacion de Reglas sobre Telemetria

Trigger: evento de telemetria que requiere evaluacion. SLA: < 5 s no critico, < 2 s critico.

![Flujo 3 — Evaluacion de Reglas sobre Telemetria](../architectures/Diagrama%20de%20flujo%203.png)

## Flujo 4 — Despacho de Acciones / Notificacion a Interesados

Trigger: mensaje en Service Bus topic `actions` o `actions-critical`. SLA: critico < 1 s, normal < 10 s.

![Flujo 4 — Despacho de Acciones y Notificacion a Interesados](../architectures/Diagrama%20de%20flujo%204.png)

## Flujo 5 — Configuracion de Reglas desde App Movil

Trigger: propietario crea / edita / elimina regla. SLA: respuesta API < 500 ms, propagacion a cache < 1 s.

![Flujo 5 — Configuracion de Reglas desde App Movil del Propietario](../architectures/Diagrama%20de%20flujo%205.png)
