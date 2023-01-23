param project string
param env string
param location string = resourceGroup().location

@secure()
param basicAuthPassword string

@secure()
param sslCertPfxBase64 string = ''

@secure()
param dataProtectionX509CertBase64 string = ''

@secure()
param govukNotifyApiKey string = ''

param aspNetCoreEnvironment string = 'Development'

resource storage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'stor${project}${env}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'

  resource storageBlobService 'blobServices@2022-05-01' = {
    name: 'default'
    properties: {
      deleteRetentionPolicy: {
        allowPermanentDelete: false
        days: 30
        enabled: true
      }
    }
  }

}
var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storage.listKeys().keys[0].value}'









resource redis 'Microsoft.Cache/redis@2021-06-01' = {
  name: 'redis-${project}-${env}'
  location: location
  properties: {
    redisVersion: '6.0'
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
  }
}
var redisConnectionString = '${redis.name}.redis.cache.windows.net:6380,password=${redis.listKeys().primaryKey},ssl=True,abortConnect=False'









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
    enableRbacAuthorization: false // I don't have permission to assign roles in Azure RBAC, so will use 'Vault access policy'.
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

resource basicAuthPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'basicAuthPassword'
  properties: {
    value: basicAuthPassword
  }
}


resource dataProtectionX509CertBase64Secret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'dataProtectionX509CertBase64'
  properties: {
    value: dataProtectionX509CertBase64
  }
}


resource govukNotifyApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'govukNotifyApiKey'
  properties: {
    value: govukNotifyApiKey
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
    httpsOnly: false
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
        {
          name: 'BasicAuthPassword'
          value: '@Microsoft.KeyVault(SecretUri=${basicAuthPasswordSecret.properties.secretUri})'
        }
        {
          name: 'DataProtectionX509CertBase64'
          value: '@Microsoft.KeyVault(SecretUri=${dataProtectionX509CertBase64Secret.properties.secretUri})'
        }
        {
          name: 'GovUkNotifyApiKey'
          value: '@Microsoft.KeyVault(SecretUri=${govukNotifyApiKeySecret.properties.secretUri})'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: aspNetCoreEnvironment
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
          {
            tenantId: subscription().tenantId
            objectId: 'bdb1afdf-b36e-4947-880b-fc945baa7aa7' // kd
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













resource publicIpAddress 'Microsoft.Network/publicIPAddresses@2022-05-01' = {
  name: 'ip-${project}-${env}'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
    publicIPAddressVersion: 'IPv4'
    dnsSettings: {
      domainNameLabel: '${project}-${env}'
    }

  }
}




var vnetSubnetNameApplication = 'agsubnetapp'
var vnetSubnetNameApplicationGateway = 'agsubnetappgw'

resource vnet 'Microsoft.Network/virtualNetworks@2022-05-01' = {
  name: 'vnet-${project}-${env}'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: vnetSubnetNameApplicationGateway // vnet subnet for application-gateway
        type: 'Microsoft.Network/virtualNetworks/subnets'
        properties: {
          addressPrefix: '10.0.0.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
      {
        name: vnetSubnetNameApplication // vnet subnet for application itself
        type: 'Microsoft.Network/virtualNetworks/subnets'
        properties: {
          addressPrefix: '10.0.1.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
    ]
  }

  resource vnetSubnetApplicationGateway 'subnets' existing = {
    name: vnetSubnetNameApplicationGateway
  }

  resource vnetSubnetApplication 'subnets' existing = {
    name: vnetSubnetNameApplication
  }
}



var applicationGatewayName = 'agw-${project}-${env}'
var applicationGatewaySslCertificateName = 'ssl-cert'
var applicationGatewayBackendPool = 'agw-backend-pool'
var applicationGatewayBackendHttpSettingsName = 'agw-be-http-settings'
var applicationGatewayHttpsListener = 'agw-https-listener'
var applicationGatewayCustomProbeName = 'agw-custom-backend-probe-http'

resource applicationGateway 'Microsoft.Network/applicationGateways@2022-05-01' = {
  location: location
  name: applicationGatewayName
  properties: {    

    enableHttp2: false
    sku: {
      name: 'Standard_v2'
      tier: 'Standard_v2'
    }

    autoscaleConfiguration: {
      maxCapacity: 10
      minCapacity: 0
    }

    backendHttpSettingsCollection: [
      {
        name: applicationGatewayBackendHttpSettingsName
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: true
          affinityCookieName: 'ApplicationGatewayAffinity'
          requestTimeout: 20
          probe: {
            id: resourceId('Microsoft.Network/applicationGateways/probes', applicationGatewayName, applicationGatewayCustomProbeName)
          }
        }
      }
    ]


    backendAddressPools: [
      {
        name: applicationGatewayBackendPool
        properties: {
          backendAddresses: [
            {
              fqdn: appService.properties.defaultHostName
            }
          ]
        }
      }
    ]

    
    frontendPorts: [
      {
        name: 'port_80'
        properties: {
          port: 80
        }
      }
      {
        name: 'port_443'
        properties: {
          port: 443
        }
      }
    ]

    frontendIPConfigurations: [
      {
        name: 'appGwPublicFrontendIp'
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIpAddress.id
          }
        }
      }
    ]
    

    sslCertificates: [ 
      {
        name: applicationGatewaySslCertificateName
        properties: {
          data: sslCertPfxBase64
          password: ''
        }
      }
    ]

    
    httpListeners: [
      {
        name: applicationGatewayHttpsListener
        properties: {
          frontendIPConfiguration: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', applicationGatewayName, 'appGwPublicFrontendIp')
          }
          frontendPort: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', applicationGatewayName, 'port_443')
          }
          protocol: 'Https'
          sslCertificate: {
            id: resourceId('Microsoft.Network/applicationGateways/sslCertificates', applicationGatewayName, applicationGatewaySslCertificateName)
          }
          hostNames: []
          requireServerNameIndication: false
        }
      }
    ]

    
    requestRoutingRules: [
      {
        name: 'httpsrule'
        properties: {
          ruleType: 'Basic'
          priority: 2
          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', applicationGatewayName, applicationGatewayHttpsListener)
          }
          backendAddressPool: {
            id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', applicationGatewayName, applicationGatewayBackendPool)
          }
          backendHttpSettings: {
            id: resourceId('Microsoft.Network/applicationGateways/backendHttpSettingsCollection', applicationGatewayName, applicationGatewayBackendHttpSettingsName)
          }
        }
      }
    ]


    probes: [
      {
        name: applicationGatewayCustomProbeName
        properties: {
          protocol: 'Http'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: true
          minServers: 0
          match: {
            statusCodes: [
              '200-399'
              '401'
              '403'
            ]
          }
        }
      }
    ]


    

    
    gatewayIPConfigurations: [
      {
        name: 'appGatewayIpConfig'
        properties: {
          subnet: {
            id: vnet::vnetSubnetApplicationGateway.id  
          }
        }
      }
    ]

  }
}







/*
    LOGIC APP (responds to Microsoft Defender for Cloud security alert; aka, virus alerts)
*/
resource connectionDefenderConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: 'apiconn-defenderalerts-${project}-${env}'
  location: location
  properties: {
    displayName: 'Microsoft Defender Connection (security alerts)'
    api: {
      name: 'ascalert'
      displayName: 'Microsoft Defender Alert'
      id: subscriptionResourceId('Microsoft.Web/locations/managedApis', location, 'ascalert') 
      type: 'Microsoft.Web/locations/managedApis'
    }
  }
}

resource connectionAzureBlob 'Microsoft.Web/connections@2016-06-01' = {
  name: 'apiconn-azureblob-${project}-${env}'
  location: location
  properties: {
    displayName: 'Azure Blob Storage Connection'
    parameterValues: {
      accountName: storage.name
      accessKey: storage.listKeys().keys[0].value
    }
    api: {
      name: 'azureblob'
      displayName: 'Azure Blob Storage'
      id: subscriptionResourceId('Microsoft.Web/locations/managedApis', location, 'azureblob')
      type: 'Microsoft.Web/locations/managedApis'
    }
  }
}



var workflowSchema = 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'

resource anitvirusLogicApp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: 'lapp-antivirus-responder-${project}-${env}'
  location: location
  properties: {
    state: 'Enabled'
    definition: {
      '$schema': workflowSchema
      contentVersion: '1.0.0.0'
      triggers: {
        DefenderSecurityAlert: {
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              callback_url: '@{listCallbackUrl()}'
            }
            host: {
              connection: {
                name: connectionDefenderConnection.id
              }
            }
            path: '/Microsoft.Security/Alert/subscribe'
          }
        }
      }
      
      actions: {
        DeleteBlob: {
          runAfter: {
          }
          type: 'ApiConnection'
          inputs: {
            headers: {
              SkipDeleteIfFileNotFoundOnServer: false
            }
            host: {
              connection: {
                name: connectionAzureBlob.id
              }
            }
            method: 'delete'
            path: '/v2/datasets/@{encodeURIComponent(encodeURIComponent(\'AccountNameFromSettings\'))}/files/@{encodeURIComponent(encodeURIComponent(concat(\'/\',triggerBody().extendedProperties.container,\'/\',triggerBody().extendedProperties.blob)))}'
          }
        }
      }

      outputs: {
      }
    }
  }

}
