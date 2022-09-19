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









/*
  COSMOS
*/
resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: 'cosmos-${project}-${env}'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    publicNetworkAccess: 'Enabled'
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: { defaultConsistencyLevel: 'Session'  }
    enableAutomaticFailover: false
    capacity: {
      totalThroughputLimit: 4000
    }
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}
var cosmosConnectionString = cosmosDb.listConnectionStrings().connectionStrings[0].connectionString


resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  parent: cosmosDb
  name: 'main'
  properties: {
    resource: {
      id: 'main'
    }
  }
}


resource cosmosDbContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'cabs'
  properties: {
    resource: {
      id: 'cabs'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        automatic: true
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/_etag/?'
          }
        ]
      }
    }
  }
}











/*
  APP INSIGHTS
*/
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-08-01' = {
  name: 'la-${project}-${env}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 120
    features: {
      searchVersion: 1
      legacy: 0
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai-${project}-${env}'
  location: location
  kind: 'string'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.OperationalInsights/workspaces/${logAnalyticsWorkspace.name}'
  }
}








/*
  KEY VAULT
*/
resource kv 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: 'kv-${project}-${env}'
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

resource appInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'appInsightsConnectionString'
  properties: {
    value: appInsights.properties.ConnectionString
  }
}

resource cosmosConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'cosmosConnectionString'
  properties: {
    value: cosmosConnectionString
  }
}




/*
  APP SERVICE and APP SERVICE PLAN
*/

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
  name: 'app-opss-${project}-${env}'
  location: location
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      appSettings: [
        {
          name: 'AppInsightsConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${appInsightsConnectionStringSecret.properties.secretUri})'
        }
        {
          name: 'DataConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${storageConnectionStringSecret.properties.secretUri})'
        }
        {
          name: 'RedisConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${redisConnectionStringSecret.properties.secretUri})'
        }
        {
          name: 'CosmosConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${cosmosConnectionStringSecret.properties.secretUri})'
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}





/*
  KEY VAULT ACCESS POLICY
*/ 
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

// cheeky access policy for kd
resource keyVaultAccessPolicy2 'Microsoft.KeyVault/vaults/accessPolicies@2021-06-01-preview' = {
  name: '${kv.name}/add'
  properties: {
      accessPolicies: [
          {
              tenantId: subscription().tenantId
              objectId: 'bdb1afdf-b36e-4947-880b-fc945baa7aa7'
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

