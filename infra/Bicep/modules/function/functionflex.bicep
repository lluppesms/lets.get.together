param functionAppName string
param functionAppServicePlanName string
param deploymentStorageContainerName string
param functionInsightsName string
param functionStorageAccountName string
param addRoleAssignments bool = true
param keyVaultName string = ''
param location string
param commonTags object = {}
param deploymentSuffix string = ''
param customAppSettings object = {}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  tags: commonTags
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: functionAppServicePlanName
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
      ]
    }
  }
}

output name string = functionApp.name
