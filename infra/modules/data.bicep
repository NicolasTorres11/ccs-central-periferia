param env string
param location string
param locationSecondary string
param namePrefix string
param adminObjectId string
param tags object

var suffix = uniqueString(resourceGroup().id)

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'sql-${namePrefix}-${env}-${suffix}'
  location: location
  tags: tags
  properties: {
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlAadAdmin 'Microsoft.Sql/servers/administrators@2023-08-01-preview' = if (!empty(adminObjectId)) {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    principalType: 'User'
    login: 'ccs-admin'
    sid: adminObjectId
    tenantId: subscription().tenantId
    azureADOnlyAuthentication: true
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'ccs_db'
  location: location
  tags: tags
  sku: {
    name: 'BC_Gen5'
    tier: 'BusinessCritical'
    family: 'Gen5'
    capacity: 4
  }
  properties: {
    maxSizeBytes: 274877906944
    zoneRedundant: env == 'prod'
  }
}

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: 'cosmos-${namePrefix}-${env}-${suffix}'
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: true
    enableMultipleWriteLocations: false
    publicNetworkAccess: 'Enabled'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    backupPolicy: {
      type: 'Continuous'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: env == 'prod'
      }
      {
        locationName: locationSecondary
        failoverPriority: 1
        isZoneRedundant: false
      }
    ]
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmos
  name: 'ccs'
  properties: {
    resource: {
      id: 'ccs'
    }
  }
}

resource redis 'Microsoft.Cache/redis@2023-08-01' = {
  name: 'redis-${namePrefix}-${env}-${suffix}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: env == 'prod' ? 'Premium' : 'Standard'
      family: env == 'prod' ? 'P' : 'C'
      capacity: env == 'prod' ? 1 : 1
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'st${namePrefix}${env}${suffix}'
  location: location
  tags: tags
  sku: {
    name: env == 'prod' ? 'Standard_ZRS' : 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDb.name
output cosmosAccountName string = cosmos.name
output cosmosDatabaseName string = cosmosDb.name
output redisName string = redis.name
output redisHostName string = redis.properties.hostName
output storageAccountName string = storage.name
