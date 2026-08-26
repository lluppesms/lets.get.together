param containerRegistryName string
param location string
param commonTags object = {}
param sku string = 'Basic'
param adminUserEnabled bool = true
param workspaceId string = ''
param managedIdentityPrincipalId string = ''
param pipelineServicePrincipalObjectId string = ''

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  tags: commonTags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: adminUserEnabled
  }
}

output name string = containerRegistry.name
output loginServer string = containerRegistry.properties.loginServer
