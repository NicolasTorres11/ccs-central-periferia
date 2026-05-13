# Diagrama de despliegue

Este despliegue es la propuesta objetivo en Azure. Para la prueba tecnica no se ejecuta ningun despliegue; los archivos Bicep en `infra/` quedan como artefacto revisable.

## Vista de despliegue

```mermaid
flowchart TB
  subgraph Internet
    Device[Dispositivos vehiculares]
    DriverApp[App conductor / propietario]
    AdminUser[Usuario administrativo]
  end

  subgraph Edge["Capa perimetral"]
    FD[Azure Front Door + WAF]
    APIM[API Management]
    IoTHub[Azure IoT Hub]
  end

  subgraph Apps["Capa de aplicacion"]
    Emergency[CCS.Api.Emergency]
    Admin[CCS.Api.Admin]
    Telemetry[CCS.Api.Telemetry]
    Ingestion[CCS.Functions.Ingestion]
    Rules[CCS.Functions.Rules]
    Dispatcher[CCS.Functions.Dispatcher]
  end

  subgraph Messaging["Capa de mensajeria"]
    EH[Event Hubs]
    SB[Service Bus<br/>actions / actions-critical]
  end

  subgraph Data["Capa de datos"]
    SQL[(Azure SQL)]
    Cosmos[(Cosmos DB)]
    Redis[(Azure Cache for Redis)]
    Blob[(Blob Storage)]
  end

  subgraph Observability["Observabilidad"]
    AppInsights[Application Insights]
    LogAnalytics[Log Analytics]
  end

  subgraph External["Servicios externos"]
    ACS[Communication Services]
    Push[Notification Hubs]
    SignalR[SignalR Service]
    Authority[Webhooks autoridad]
  end

  DriverApp --> FD --> APIM
  AdminUser --> FD
  Device --> IoTHub
  APIM --> Emergency
  APIM --> Admin
  APIM --> Telemetry
  IoTHub --> EH
  Telemetry --> EH
  EH --> Ingestion --> Cosmos
  EH --> Rules
  Rules --> Redis
  Rules --> Cosmos
  Rules --> SB
  Emergency --> Redis
  Emergency --> SB
  Admin --> SQL
  Admin --> Redis
  SB --> Dispatcher
  Dispatcher --> SQL
  Dispatcher --> ACS
  Dispatcher --> Push
  Dispatcher --> SignalR
  Dispatcher --> Authority
  Device -. evidencias .-> Blob

  Emergency --> AppInsights
  Admin --> AppInsights
  Telemetry --> AppInsights
  Ingestion --> AppInsights
  Rules --> AppInsights
  Dispatcher --> AppInsights
  AppInsights --> LogAnalytics
```

## Ambientes

Los parametros Bicep estan separados por ambiente:

- `infra/params/dev.bicepparam`
- `infra/params/stg.bicepparam`
- `infra/params/prod.bicepparam`

La separacion permite ajustar capacidad, SKU, retencion y politicas por ambiente sin cambiar los modulos base.

## Relacion con el repositorio local

- Las APIs locales usan almacenamiento en memoria para validar contratos y flujos.
- Las Functions estan representadas como proyectos .NET testeables.
- `scripts/deploy-infra.sh` solo imprime comandos de referencia; no ejecuta `az deployment`.
