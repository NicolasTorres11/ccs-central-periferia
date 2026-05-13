# Pruebas de carga locales

El entregable incluye una prueba local de throughput en `tests/CCS.LoadTests`.

## Objetivo

Validar de forma deterministica que la capa de aplicacion puede procesar 500 mensajes de telemetria en menos de 1 segundo en memoria local, equivalente al objetivo de 500 senales por segundo definido para la solucion.

## Ejecucion

```bash
dotnet test tests/CCS.LoadTests/CCS.LoadTests.csproj --disable-build-servers -m:1
```

## Alcance

Esta prueba no reemplaza una prueba distribuida contra Azure Event Hubs, Functions o Cosmos DB. Sirve como evidencia local de capacidad del codigo de aplicacion y como base para escenarios de carga mas amplios en un ambiente desplegado.
