param env string
param location string
param namePrefix string
param appInsightsConnectionString string
param keyVaultUri string
param serviceBusNamespaceName string
param eventHubNamespaceName string
param cosmosAccountName string
param sqlServerFqdn string
param redisHostName string
param tags object

var suffix = uniqueString(resourceGroup().id)
var workerRuntime = 'dotnet-isolated'

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'stfunc${namePrefix}${env}${suffix}'
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-${namePrefix}-func-${env}'
  location: location
  tags: tags
  sku: {
    name: env == 'prod' ? 'EP2' : 'EP1'
    tier: 'ElasticPremium'
    size: env == 'prod' ? 'EP2' : 'EP1'
    capacity: env == 'prod' ? 2 : 1
  }
  properties: {
    reserved: false
  }
}

var commonSettings = [
  {
    name: 'FUNCTIONS_EXTENSION_VERSION'
    value: '~4'
  }
  {
    name: 'FUNCTIONS_WORKER_RUNTIME'
    value: workerRuntime
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightsConnectionString
  }
  {
    name: 'KeyVaultUri'
    value: keyVaultUri
  }
  {
    name: 'ServiceBus__fullyQualifiedNamespace'
    value: '${serviceBusNamespaceName}.servicebus.windows.net'
  }
  {
    name: 'EventHub__fullyQualifiedNamespace'
    value: '${eventHubNamespaceName}.servicebus.windows.net'
  }
  {
    name: 'Cosmos__AccountEndpoint'
    value: 'https://${cosmosAccountName}.documents.azure.com:443/'
  }
  {
    name: 'Sql__Server'
    value: sqlServerFqdn
  }
  {
    name: 'Redis__Host'
    value: redisHostName
  }
  {
    name: 'AzureWebJobsStorage'
    value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storage.listKeys().keys[0].value}'
  }
]

resource ingestionFunc 'Microsoft.Web/sites@2023-12-01' = {
  name: 'func-${namePrefix}-ingestion-${env}'
  location: location
  tags: tags
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      appSettings: union(commonSettings, [
        {
          name: 'WorkerRole'
          value: 'Ingestion'
        }
      ])
    }
  }
}

resource rulesFunc 'Microsoft.Web/sites@2023-12-01' = {
  name: 'func-${namePrefix}-rules-${env}'
  location: location
  tags: tags
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      appSettings: union(commonSettings, [
        {
          name: 'WorkerRole'
          value: 'Rules'
        }
      ])
    }
  }
}

resource dispatcherFunc 'Microsoft.Web/sites@2023-12-01' = {
  name: 'func-${namePrefix}-dispatcher-${env}'
  location: location
  tags: tags
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      appSettings: union(commonSettings, [
        {
          name: 'WorkerRole'
          value: 'Dispatcher'
        }
      ])
    }
  }
}

resource emergencyFunc 'Microsoft.Web/sites@2023-12-01' = {
  name: 'func-${namePrefix}-emergency-${env}'
  location: location
  tags: tags
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      appSettings: union(commonSettings, [
        {
          name: 'WorkerRole'
          value: 'Emergency'
        }
      ])
    }
  }
}

resource apiPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-${namePrefix}-api-${env}'
  location: location
  tags: tags
  sku: {
    name: env == 'prod' ? 'P1v3' : 'B1'
    tier: env == 'prod' ? 'PremiumV3' : 'Basic'
    capacity: 1
  }
}

resource adminApi 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-${namePrefix}-admin-${env}'
  location: location
  tags: tags
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: apiPlan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      netFrameworkVersion: 'v8.0'
      appSettings: commonSettings
    }
  }
}

resource telemetryApi 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-${namePrefix}-telemetry-${env}'
  location: location
  tags: tags
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: apiPlan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      netFrameworkVersion: 'v8.0'
      appSettings: commonSettings
    }
  }
}

resource apim 'Microsoft.ApiManagement/service@2023-09-01-preview' = {
  name: 'apim-${namePrefix}-${env}'
  location: location
  tags: tags
  sku: {
    name: env == 'prod' ? 'StandardV2' : 'Developer'
    capacity: 1
  }
  properties: {
    publisherEmail: 'engineering@ccs.example'
    publisherName: 'CCS Engineering'
  }
}

output apiManagementName string = apim.name
output emergencyFunctionName string = emergencyFunc.name
output ingestionFunctionName string = ingestionFunc.name
output rulesFunctionName string = rulesFunc.name
output dispatcherFunctionName string = dispatcherFunc.name
output adminApiName string = adminApi.name
output telemetryApiName string = telemetryApi.name

