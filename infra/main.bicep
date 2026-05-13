targetScope = 'resourceGroup'

@allowed([
  'dev'
  'stg'
  'prod'
])
param env string

param location string = resourceGroup().location
param locationSecondary string = 'brazilsouth'
param namePrefix string = 'ccs'

@description('Object id del administrador inicial de Key Vault y SQL.')
param adminObjectId string = ''

var tags = {
  env: env
  owner: 'ccs'
  workload: 'central-seguimiento-vehiculos'
}

module network 'modules/network.bicep' = {
  name: 'network-${env}'
  params: {
    env: env
    location: location
    namePrefix: namePrefix
    tags: tags
  }
}

module observability 'modules/observability.bicep' = {
  name: 'observability-${env}'
  params: {
    env: env
    location: location
    namePrefix: namePrefix
    tags: tags
  }
}

module security 'modules/security.bicep' = {
  name: 'security-${env}'
  params: {
    env: env
    location: location
    namePrefix: namePrefix
    adminObjectId: adminObjectId
    tags: tags
  }
}

module messaging 'modules/messaging.bicep' = {
  name: 'messaging-${env}'
  params: {
    env: env
    location: location
    namePrefix: namePrefix
    tags: tags
  }
}

module data 'modules/data.bicep' = {
  name: 'data-${env}'
  params: {
    env: env
    location: location
    locationSecondary: locationSecondary
    namePrefix: namePrefix
    adminObjectId: adminObjectId
    tags: tags
  }
}

module iot 'modules/iot.bicep' = {
  name: 'iot-${env}'
  params: {
    env: env
    location: location
    namePrefix: namePrefix
    tags: tags
  }
}

module compute 'modules/compute.bicep' = {
  name: 'compute-${env}'
  params: {
    env: env
    location: location
    namePrefix: namePrefix
    appInsightsConnectionString: observability.outputs.appInsightsConnectionString
    keyVaultUri: security.outputs.keyVaultUri
    serviceBusNamespaceName: messaging.outputs.serviceBusNamespaceName
    eventHubNamespaceName: messaging.outputs.eventHubNamespaceName
    cosmosAccountName: data.outputs.cosmosAccountName
    sqlServerFqdn: data.outputs.sqlServerFqdn
    redisHostName: data.outputs.redisHostName
    tags: tags
  }
}

output apiManagementName string = compute.outputs.apiManagementName
output emergencyFunctionName string = compute.outputs.emergencyFunctionName
output sqlServerFqdn string = data.outputs.sqlServerFqdn
output cosmosAccountName string = data.outputs.cosmosAccountName
output serviceBusNamespaceName string = messaging.outputs.serviceBusNamespaceName
output eventHubNamespaceName string = messaging.outputs.eventHubNamespaceName
