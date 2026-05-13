param env string
param location string
param namePrefix string
param tags object

resource iotHub 'Microsoft.Devices/IotHubs@2023-06-30' = {
  name: 'iot-${namePrefix}-${env}-${uniqueString(resourceGroup().id)}'
  location: location
  tags: tags
  sku: {
    name: env == 'prod' ? 'S2' : 'S1'
    capacity: env == 'prod' ? 2 : 1
  }
  properties: {
    publicNetworkAccess: 'Enabled'
    minTlsVersion: '1.2'
    eventHubEndpoints: {
      events: {
        retentionTimeInDays: 1
        partitionCount: 4
      }
    }
    routing: {
      endpoints: {
        eventHubs: []
        serviceBusQueues: []
        serviceBusTopics: []
        storageContainers: []
      }
      routes: [
        {
          name: 'all-telemetry'
          source: 'DeviceMessages'
          condition: 'true'
          endpointNames: [
            'events'
          ]
          isEnabled: true
        }
      ]
      fallbackRoute: {
        name: '$fallback'
        source: 'DeviceMessages'
        condition: 'true'
        endpointNames: [
          'events'
        ]
        isEnabled: true
      }
    }
    cloudToDevice: {
      maxDeliveryCount: 10
      defaultTtlAsIso8601: 'PT1H'
      feedback: {
        lockDurationAsIso8601: 'PT1M'
        ttlAsIso8601: 'PT1H'
        maxDeliveryCount: 10
      }
    }
  }
}

output iotHubName string = iotHub.name
output iotHubId string = iotHub.id
