# ADR-001: Camino Critico de Emergencia

## Estado
Aceptado

## Decision
Los eventos de panico/emergencia originados desde la app movil o desde el boton fisico de panico usan el camino mas corto:

```text
App/Boton -> API Management -> CCS.Api.Emergency -> consulta de reglas en Redis -> Service Bus actions-critical -> CCS.Functions.Dispatcher
```

La telemetria IoT y las evaluaciones de reglas derivadas continuan por IoT Hub, Event Hubs, el worker de ingesta y el worker de reglas.

## Consecuencias
- El flujo de emergencia evita un salto adicional por Event Hubs y RulesEngine.
- Redis debe estar precargado con las reglas activas de panico.
- El dispatcher debe priorizar `actions-critical`.
