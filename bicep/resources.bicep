param project string
param env string
param location string = resourceGroup().location

resource storage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'stor${project}${env}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}


resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-${project}-${env}'
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
  kind: 'linux'
}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-${project}-${env}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}
