# Functions simuladas

Este entregable incluye proyectos `CCS.Functions.*` para representar las responsabilidades que en Azure podrian ejecutarse como Azure Functions. En esta prueba no se despliega ningun recurso en Azure, por lo que estos proyectos se mantienen como componentes .NET testeables y ejecutables dentro de la solucion.

## Componentes

- `CCS.Functions.Ingestion`: valida lotes de telemetria con el caso de uso `ProcessTelemetryBatch`.
- `CCS.Functions.Rules`: evalua eventos criticos derivados de telemetria y produce una senal de emergencia cuando aplica.
- `CCS.Functions.Dispatcher`: simula el despacho ordenado de acciones criticas hacia propietarios, autoridad u otros destinos.

## Alcance

Estos proyectos no requieren Azure Functions Core Tools, cuentas de almacenamiento, Service Bus, Event Hubs ni despliegues. Su objetivo es dejar clara la separacion de responsabilidades y permitir pruebas unitarias locales sobre la logica que luego podria conectarse a triggers reales.

## Validacion local

```bash
dotnet test CCS.slnx --disable-build-servers -m:1
```
