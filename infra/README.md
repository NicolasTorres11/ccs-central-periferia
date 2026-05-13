# Infraestructura Azure

Esta carpeta contiene la definicion de infraestructura como codigo en Bicep para la central CCS.

## Alcance
Los archivos Bicep son un artefacto tecnico documentado para la prueba. No se ejecutan despliegues reales desde este repositorio.

## Estructura

```text
infra/
  main.bicep
  modules/
    network.bicep
    security.bicep
    messaging.bicep
    data.bicep
    iot.bicep
    compute.bicep
    observability.bicep
  params/
    dev.bicepparam
    stg.bicepparam
    prod.bicepparam
```

## Capas
- `network`: VNet y subredes base.
- `security`: Key Vault y permisos iniciales.
- `messaging`: Event Hubs y Service Bus.
- `data`: Azure SQL, Cosmos DB, Redis y Storage.
- `iot`: IoT Hub.
- `compute`: Function Apps, APIs y API Management.
- `observability`: Log Analytics y Application Insights.

## Comando de referencia
No ejecutar contra una suscripcion real durante la prueba salvo autorizacion explicita.

```bash
az deployment group create \
  --resource-group rg-ccs-prod \
  --template-file infra/main.bicep \
  --parameters infra/params/prod.bicepparam
```

## Nota
Los secretos y connection strings productivos no se declaran en Bicep. La solucion usa Managed Identity y Key Vault como patron esperado.

