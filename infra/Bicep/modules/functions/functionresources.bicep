param functionInsightsName string
param functionStorageAccountName string
param location string
param commonTags object = {}
param workspaceId string = ''

output appInsightsName string = functionInsightsName
output storageAccountName string = functionStorageAccountName
