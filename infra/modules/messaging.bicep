param env string
param location string
param namePrefix string
param tags object

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2024-01-01' = {
  name: 'evhns-${namePrefix}-${env}-${uniqueString(resourceGroup().id)}'
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 4
  }
  properties: {
    isAutoInflateEnabled: true
    maximumThroughputUnits: 20
    publicNetworkAccess: 'Enabled'
    minimumTlsVersion: '1.2'
  }
}

resource telemetryHub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = {
  parent: eventHubNamespace
  name: 'telemetry'
  properties: {
    partitionCount: 32
    messageRetentionInDays: 7
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'sb-${namePrefix}-${env}-${uniqueString(resourceGroup().id)}'
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource actionsTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'actions'
  properties: {
    defaultMessageTimeToLive: 'P14D'
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT5M'
  }
}

resource actionsCriticalTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'actions-critical'
  properties: {
    defaultMessageTimeToLive: 'P14D'
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT5M'
  }
}

resource dispatcherSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: actionsTopic
  name: 'dispatcher'
  properties: {
    maxDeliveryCount: 5
    lockDuration: 'PT1M'
  }
}

resource criticalDispatcherSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: actionsCriticalTopic
  name: 'dispatcher-critical'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}

output eventHubNamespaceId string = eventHubNamespace.id
output eventHubNamespaceName string = eventHubNamespace.name
output telemetryHubName string = telemetryHub.name
output serviceBusNamespaceId string = serviceBusNamespace.id
output serviceBusNamespaceName string = serviceBusNamespace.name

