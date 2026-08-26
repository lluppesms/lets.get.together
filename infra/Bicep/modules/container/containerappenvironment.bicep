param environmentName string
param location string
param commonTags object = {}
param workspaceId string = ''

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: environmentName
  location: location
  tags: commonTags
  properties: {
    appLogsConfiguration: !empty(workspaceId) ? {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: reference(workspaceId, '2022-10-01').customerId
        sharedKey: listKeys(workspaceId, '2022-10-01').primarySharedKey
      }
    } : null
  }
}

output id string = containerAppsEnvironment.id
