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

var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storage.listKeys().keys[0].value}'

resource redis 'Microsoft.Cache/redis@2021-06-01' = {
  name: 'redis-${project}-${env}'
  location: location
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
  }
}

var redisConnectionString = '${redis.name}.redis.cache.windows.net:6380,password=${redis.listKeys().primaryKey},ssl=True,abortConnect=False'




resource kv 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: 'kv1-${project}-${env}'
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    enableRbacAuthorization: false // I don't have permission to assign roles in Azure RBAC, so will use 'Vault access policy'
    accessPolicies: []
  }
}

resource storageConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'storageConnectionString'
  properties: {
    value: storageConnectionString
  }
}

resource redisConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'redisConnectionString'
  properties: {
    value: redisConnectionString
  }
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
  name: 'appl-${project}-${env}'
  location: location
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      appSettings: [
        // {
        //   name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
        //   value: applicationInsights.properties.InstrumentationKey
        // }
        {
          name: 'DataConnectionString-10'
          value: '@Microsoft.KeyVault(SecretUri=${storageConnectionStringSecret.properties.secretUri})'
        }
        {
          name: 'RedisConnectionString-10'
          value: '@Microsoft.KeyVault(SecretUri=${redisConnectionStringSecret.properties.secretUri})'
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}







resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-06-01-preview' = {
  name: '${kv.name}/add'
  properties: {
      accessPolicies: [
          {
              tenantId: subscription().tenantId
              objectId: appService.identity.principalId
              permissions: {
                certificates: [
                  'all'
                ]
                keys:[
                  'all'
                ]
                secrets:[
                  'all'
                ]
                storage: [
                  'all'
                ]
               }
          }
      ]
  }
}

