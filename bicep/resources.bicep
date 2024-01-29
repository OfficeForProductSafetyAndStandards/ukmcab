param project string
param env string
param location string = resourceGroup().location
param appServicePlanSkuName string
param storageAccountSkuName string
param provisionAppSvcVNextSlot bool
param appServiceUseBasicAuth bool      // for prod deployment, after go-live, set to false in the prod.params.json
param appServiceUseBasicAuthVNext bool
param appServiceHostName string
param appServiceHostNameVNext string
param appServiceUseAutoScale bool
param searchReplicaCount int

@secure()
param basicAuthPassword string

@secure()
param sslCertPfxBase64 string = ''

@secure()
param sslCertPfxBase64VNextSlot string = ''

@secure()
param dataProtectionX509CertBase64 string = ''

@secure()
param govukNotifyApiKey string = ''


param aspNetCoreEnvironment string = 'Development'

@secure()
param encryptionKey string

@secure()
param oneLoginClientId string

@secure()
param oneLoginKeyPairBase64 string


resource storage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'stor${project}${env}'
  location: location
  sku: {
    name: storageAccountSkuName
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
      name: 'Standard'
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
  AZURE COGNITIVE SEARCH
*/
resource cognitiveSearch 'Microsoft.Search/searchServices@2022-09-01' = {
  name: 'acs-${project}-${env}'
  location: location
  sku: {
    name: 'basic'
  }
  properties: {
    replicaCount: searchReplicaCount
    partitionCount: 1
    hostingMode: 'default'
  }
}
var acsConnectionString = 'endpoint=https://${cognitiveSearch.name}.search.windows.net;apikey=${cognitiveSearch.listAdminKeys().primaryKey}'


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
  name: 'cab-documents'
  properties: {
    resource: {
      id: 'cab-documents'
      partitionKey: {
        paths: [
          '/CABId'
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

resource cosmosDbContainerUsers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'user-accounts'
  properties: {
    resource: {
      id: 'user-accounts'
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

resource cosmosDbContainerUserAccountRequests 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'user-account-requests'
  properties: {
    resource: {
      id: 'user-account-requests'
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

resource cosmosDbContainerTasks 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'workflow-tasks'
  properties: {
    resource: {
      id: 'workflow-tasks'
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



resource cosmosDbContainerLegislativeArea 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'legislative-areas'
  properties: {
    resource: {
      id: 'legislative-areas'
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

resource cosmosDbContainerPurposeOfAppointment 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'purpose-of-appointment'
  properties: {
    resource: {
      id: 'purpose-of-appointment'
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


resource cosmosDbContainerProcedures  'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'procedures'
  properties: {
    resource: {
      id: 'procedures'
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

resource cosmosDbContainerCategories  'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'categories'
  properties: {
    resource: {
      id: 'categories'
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


resource cosmosDbContainerProducts  'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: cosmosDbDatabase
  name: 'products'
  properties: {
    resource: {
      id: 'products'
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

resource encryptionKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'encryptionKey'
  properties: {
    value: encryptionKey
  }
}

resource oneLoginClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'oneLoginClientId'
  properties: {
    value: oneLoginClientId
  }
}

resource oneLoginKeyPairBase64Secret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'oneLoginKeyPairBase64'
  properties: {
    value: oneLoginKeyPairBase64
  }
}

resource govukNotifyApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'govukNotifyApiKey'
  properties: {
    value: govukNotifyApiKey
  }
}


resource acsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: kv
  name: 'acsConnectionString'
  properties: {
    value: acsConnectionString
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
    name: appServicePlanSkuName
  }
  kind: 'linux'

}

var siteProperties = {
  httpsOnly: false
  serverFarmId: appServicePlan.id
  siteConfig: {
    linuxFxVersion: 'DOTNETCORE|6.0'
    alwaysOn: true
  }
}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-opss-${project}-${env}'
  location: location
  properties: siteProperties
  identity: {
    type: 'SystemAssigned'
  }

  resource hostname 'hostNameBindings@2022-03-01' = {
    name: appServiceHostName
    properties: {
      customHostNameDnsRecordType: 'A'
      hostNameType: 'Verified'
      siteName: appService.name
    }
  }

  resource vnext 'slots@2022-03-01' = if(provisionAppSvcVNextSlot) {
    name: 'vnext'
    location: location
    properties: siteProperties
    identity: {
      type: 'SystemAssigned'
    }

    resource hostnameVNext 'hostNameBindings@2022-03-01' = {
      name: appServiceHostNameVNext
      properties: {
        customHostNameDnsRecordType: 'A'
        hostNameType: 'Verified'
        siteName: appService::vnext.name
      }
    }
  }
}







/*
  KEY VAULT ACCESS POLICY
*/ 
var keyVaultPermissions = {
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

var keyVaultAccessPrincipalIds = [
  'bdb1afdf-b36e-4947-880b-fc945baa7aa7' // usr: kd 
  'fc5dc564-fb66-420d-8fa0-530c1f4da579' // sp: prod (secret: AZURE_CREDENTIALS_PROD),  production
  '20003b81-d9ac-48fb-940f-200c9644a18d' // sp: uat (secret: AZURE_CREDENTIALS_UAT)     preprod/uat
  '5ba190d7-7b3a-488f-b7df-48985aecc558' // sp: dev (secret: AZURE_CREDENTIALS)         dev/stage/test
]

var keyVaultAccessPrincipalDescriptors = [for id in keyVaultAccessPrincipalIds: {
  tenantId: subscription().tenantId
  objectId: id
  permissions: keyVaultPermissions
}]

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-06-01-preview' = {
  name: 'add'
  parent: kv
  properties: {
      accessPolicies: concat([
        {
          tenantId: subscription().tenantId
          objectId: appService.identity.principalId
          permissions: keyVaultPermissions
        }
      ], 
      provisionAppSvcVNextSlot ? [
        {
          tenantId: subscription().tenantId
          objectId: appService::vnext.identity.principalId
          permissions: keyVaultPermissions
        }
      ] : [],
      keyVaultAccessPrincipalDescriptors)
  }
}






resource firewallPolicy 'Microsoft.Network/ApplicationGatewayWebApplicationFirewallPolicies@2022-09-01' = {
  name: 'wafpol-main'
  location: location
  properties: {
    customRules: []
    policySettings: {
      requestBodyCheck: true
      maxRequestBodySizeInKb: 128
      fileUploadLimitInMb: 100
      state: 'Enabled'
      mode: 'Detection'
    }
    managedRules: {
      managedRuleSets: [
        {
          ruleSetType: 'OWASP'
          ruleSetVersion: '3.2'
          ruleGroupOverrides: []
        }
      ]
      exclusions: [
        {
          matchVariable: 'RequestCookieNames'
          selectorMatchOperator: 'StartsWith'
          selector: 'UKMCAB_'
          exclusionManagedRuleSets: [
            {
              ruleSetType: 'OWASP'
              ruleSetVersion: '3.2'
              ruleGroups: [
                {
                  ruleGroupName: 'REQUEST-942-APPLICATION-ATTACK-SQLI'
                  rules: [
                    {
                      ruleId: '942440'
                    }
                    {
                      ruleId: '942450'
                    }
                  ]
                }
              ]
            }
          ]
        }
        {
          matchVariable: 'RequestArgKeys'
          selectorMatchOperator: 'Equals'
          selector: '__RequestVerificationToken'
          exclusionManagedRuleSets: [
            {
              ruleSetType: 'OWASP'
              ruleSetVersion: '3.2'
              ruleGroups: [
                {
                  ruleGroupName: 'REQUEST-942-APPLICATION-ATTACK-SQLI'
                  rules: [
                    {
                      ruleId: '942440'
                    }
                    {
                      ruleId: '942450'
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
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
        name: vnetSubnetNameApplicationGateway // vnet subnet for application-gateway (range: 10.0.0.0-10.0.0.255)
        type: 'Microsoft.Network/virtualNetworks/subnets'
        properties: {
          addressPrefix: '10.0.0.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
      {
        name: vnetSubnetNameApplication // vnet subnet for application itself (range: 10.0.1.0 to 10.0.1.255)
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
var applicationGatewayPublicFrontendIpConfigurationName = 'appGwPublicFrontendIp'

var applicationGatewayCustomProbeName = 'agw-custom-backend-probe-http'
var applicationGatewayCustomProbeNameVNext = 'agw-custom-backend-probe-http-vnext'
var applicationGatewayBackendHttpSettingsName = 'agw-be-http-settings'
var applicationGatewayBackendHttpSettingsNameVNext = 'agw-be-http-settings-vnext'

var applicationGatewaySslCertificateName = 'ssl-cert'
var applicationGatewaySslCertificateNameVNext = 'ssl-cert-vnext'
var applicationGatewayBackendPool = 'agw-backend-pool'
var applicationGatewayBackendPoolVNext = 'agw-backend-pool-vnext'
var applicationGatewayHttpsListener = 'agw-https-listener'
var applicationGatewayHttpsListenerVNext = 'agw-https-listener-vnext'
var applicationGatewaySslProfileName = 'agw-ssl-profile-main'

resource applicationGateway 'Microsoft.Network/applicationGateways@2022-05-01' = {
  location: location
  name: applicationGatewayName
  properties: {    

    sslPolicy: {
      policyType: 'Predefined'
      policyName: 'AppGwSslPolicy20220101'
    }

    enableHttp2: false
    sku: {
      name: 'WAF_v2'
      tier: 'WAF_v2'
    }

    firewallPolicy: {
      id: firewallPolicy.id
    }
    
    forceFirewallPolicyAssociation: true

    autoscaleConfiguration: {
      maxCapacity: 10
      minCapacity: 0
    }

    backendHttpSettingsCollection: concat([
      {
        name: applicationGatewayBackendHttpSettingsName
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          affinityCookieName: 'ApplicationGatewayAffinity'
          requestTimeout: 240
          hostName: appServiceHostName
          probe: {
            id: resourceId('Microsoft.Network/applicationGateways/probes', applicationGatewayName, applicationGatewayCustomProbeName)
          }
        }
      }
    ], provisionAppSvcVNextSlot ? [
      {
        name: applicationGatewayBackendHttpSettingsNameVNext
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          affinityCookieName: 'ApplicationGatewayAffinity'
          requestTimeout: 30
          hostName: appServiceHostNameVNext
          probe: {
            id: resourceId('Microsoft.Network/applicationGateways/probes', applicationGatewayName, applicationGatewayCustomProbeNameVNext)
          }
        }
      }
    ] : [])


    backendAddressPools: concat([
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
    ], provisionAppSvcVNextSlot ? [
      {
        name: applicationGatewayBackendPoolVNext
        properties: {

          backendAddresses: [
            {
              fqdn: appService::vnext.properties.defaultHostName
            }
          ]
        }
      }
    ] : [])

    
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
        name: applicationGatewayPublicFrontendIpConfigurationName
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIpAddress.id
          }
        }
      }
    ]

    sslCertificates: concat([ 
      {
        name: applicationGatewaySslCertificateName
        properties: {
          data: sslCertPfxBase64
          password: ''
        }
      }
    ], 
    provisionAppSvcVNextSlot ? [
      {
        name: applicationGatewaySslCertificateNameVNext
        properties: {
          data: sslCertPfxBase64VNextSlot
          password: ''
        }
      }
    ] : [])


    sslProfiles: [
      {
        name: applicationGatewaySslProfileName
        properties: {
          sslPolicy: {
            policyType: 'Predefined'
            policyName: 'AppGwSslPolicy20220101'
          }
        }
      }
    ]

    
    httpListeners: concat(
      provisionAppSvcVNextSlot ? [
        {
          name: applicationGatewayHttpsListenerVNext
          properties: {
            frontendIPConfiguration: {
              id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', applicationGatewayName, applicationGatewayPublicFrontendIpConfigurationName)
            }
            frontendPort: {
              id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', applicationGatewayName, 'port_443')
            }
            protocol: 'Https'
            sslCertificate: {
              id: resourceId('Microsoft.Network/applicationGateways/sslCertificates', applicationGatewayName, applicationGatewaySslCertificateNameVNext)
            }
            sslProfile: {
              id: resourceId('Microsoft.Network/applicationGateways/sslProfiles', applicationGatewayName, applicationGatewaySslProfileName)
            }
            hostNames: [appServiceHostNameVNext]
            requireServerNameIndication: false
            customErrorConfigurations: [
              {
                statusCode: 'HttpStatus502'
                #disable-next-line no-hardcoded-env-urls // ultimately it needs a FQ URL
                customErrorPageUrl: 'https://storukmcabdev.blob.core.windows.net/public/badgateway.html'
              }
              {
                statusCode: 'HttpStatus403'
                #disable-next-line no-hardcoded-env-urls // ultimately it needs a FQ URL
                customErrorPageUrl: 'https://storukmcabdev.blob.core.windows.net/public/forbidden.html'
              }
            ]
          }
        }
      ] : [],
      
      [{
        name: applicationGatewayHttpsListener
        properties: {
          frontendIPConfiguration: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', applicationGatewayName, applicationGatewayPublicFrontendIpConfigurationName)
          }
          frontendPort: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', applicationGatewayName, 'port_443')
          }
          protocol: 'Https'
          sslCertificate: {
            id: resourceId('Microsoft.Network/applicationGateways/sslCertificates', applicationGatewayName, applicationGatewaySslCertificateName)
          }
          sslProfile: {
            id: resourceId('Microsoft.Network/applicationGateways/sslProfiles', applicationGatewayName, applicationGatewaySslProfileName)
          }
          hostNames: [appServiceHostName]
          requireServerNameIndication: false
          customErrorConfigurations: [
            {
              statusCode: 'HttpStatus502'
              #disable-next-line no-hardcoded-env-urls // ultimately it needs a FQ URL
              customErrorPageUrl: 'https://storukmcabdev.blob.core.windows.net/public/badgateway.html'
            }
            {
              statusCode: 'HttpStatus403'
              #disable-next-line no-hardcoded-env-urls // ultimately it needs a FQ URL
              customErrorPageUrl: 'https://storukmcabdev.blob.core.windows.net/public/forbidden.html'
            }
          ]
        }
      }]
    ) //concat

    
    requestRoutingRules: concat([
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
    ], 
      provisionAppSvcVNextSlot ? [
      {
        name: 'httpsrule-vnext'
        properties: {
          ruleType: 'Basic'
          priority: 3
          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', applicationGatewayName, applicationGatewayHttpsListenerVNext)
          }
          backendAddressPool: {
            id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', applicationGatewayName, applicationGatewayBackendPoolVNext)
          }
          backendHttpSettings: {
            id: resourceId('Microsoft.Network/applicationGateways/backendHttpSettingsCollection', applicationGatewayName, applicationGatewayBackendHttpSettingsNameVNext)
          }
        }
      }
    ] : [])


    probes: concat([
      {
        name: applicationGatewayCustomProbeName
        properties: {
          protocol: 'Http'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          host: appServiceHostName
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
    ],
    provisionAppSvcVNextSlot ? [
      {
        name: applicationGatewayCustomProbeNameVNext
        properties: {
          protocol: 'Http'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          host: appServiceHostNameVNext
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
    ] : [])


    

    
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
  PRIVATE LINK
*/
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  name: 'ep-${appService.name}'
  location: location
  properties: {
    subnet: {
      id: vnet::vnetSubnetApplication.id
    }
    privateLinkServiceConnections: [
      {
        name: 'plconnection'
        properties: {
          privateLinkServiceId: appService.id
          groupIds: [
            'sites'
          ]
        }
      }
    ]
  }
}

resource privateEndpointVNext 'Microsoft.Network/privateEndpoints@2021-05-01' = if(provisionAppSvcVNextSlot) {
  name: 'ep-${appService.name}-vnext'
  location: location
  properties: {
    subnet: {
      id: vnet::vnetSubnetApplication.id
    }
    privateLinkServiceConnections: [
      {
        name: 'plconnection-vnext'
        properties: {
          privateLinkServiceId: appService.id // this purposefully points to the parent web app
          groupIds: [
            'sites-vnext' // this is the bit which links to the vnext slot. 
          ]
        }
      }
    ]
  }
}

// DNS zone for the privatelink dns entry
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink.azurewebsites.net'
  location: 'global'

  resource privateDnsZoneLink 'virtualNetworkLinks@2020-06-01' = {
    name: 'privatelink.azurewebsites.net-dnslink'
    location: 'global'
    properties: {
      registrationEnabled: false
      virtualNetwork: {
        id: vnet.id
      }
    }
  }
}

resource pvtEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2021-05-01' = {
  name: 'privatelinkdns' 
  parent: privateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}

resource pvtEndpointDnsGroupVNext 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2021-05-01' = if(provisionAppSvcVNextSlot) {
  name: 'privatelinkdns-vnext' 
  parent: privateEndpointVNext
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1vnext'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}






/*
  NOTE: APPLICATION GATEWAY PRIVATE ENDPOINT DNS ISSUE
  If the application is not accessible (returns 403), it's likely the backend pool dns needs updating. i.e, Application Gateway needs to restart
  in order to re-resolve the DNS of the backend pool AFTER the private endpoint config has been activated above.

  To restart app gateway in the dev env:
  az network application-gateway stop -n agw-ukmcab-dev -g rg-ukmcab-dev
  az network application-gateway start -n agw-ukmcab-dev -g rg-ukmcab-dev

*/
















/*
  =================================  
  ADJUSTMENTS TO THE APP SERVICE(S)
  =================================
*/
var appSettings = [
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
    name: 'DataProtectionX509CertBase64'
    value: '@Microsoft.KeyVault(SecretUri=${dataProtectionX509CertBase64Secret.properties.secretUri})'
  }
  {
    name: 'EncryptionKey'
    value: '@Microsoft.KeyVault(SecretUri=${encryptionKeySecret.properties.secretUri})'
  }
  {
    name: 'GovUkNotifyApiKey'
    value: '@Microsoft.KeyVault(SecretUri=${govukNotifyApiKeySecret.properties.secretUri})'
  }
  {
    name: 'AcsConnectionString'
    value: '@Microsoft.KeyVault(SecretUri=${acsConnectionStringSecret.properties.secretUri})'
  }
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: aspNetCoreEnvironment
  }

  // One Login settings
  {
    name: 'OneLoginClientId'
    value: '@Microsoft.KeyVault(SecretUri=${oneLoginClientIdSecret.properties.secretUri})'
  }
  {
    name: 'OneLoginKeyPairBase64'
    value: '@Microsoft.KeyVault(SecretUri=${oneLoginKeyPairBase64Secret.properties.secretUri})'
  }
  
]

var appSettingBasicAuth = {
  name: 'BasicAuthPassword'
  value: '@Microsoft.KeyVault(SecretUri=${basicAuthPasswordSecret.properties.secretUri})'
}


resource webConfig 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'web'
  parent: appService
  properties: {
    publicNetworkAccess: provisionAppSvcVNextSlot ? 'Disabled' : 'Enabled'   // allow public access generally, then use ip restrictions to lock down the main site and ALLOW traffic to the SCM, otherwise this will stop deployments; unless we're provisioning a vnext slot, in which case deployments should target vnext
    
    #disable-next-line BCP037 // Bicep linter is wrong.
    ipSecurityRestrictionsDefaultAction: 'Deny'
    
    #disable-next-line BCP037 // Bicep linter is wrong.
    scmIpSecurityRestrictionsDefaultAction: 'Allow'
    
    appSettings: concat(appSettings, appServiceUseBasicAuth ? [appSettingBasicAuth] : [], [{name:'AppHostName',value:appServiceHostName}])
  }
}

resource webConfigVNext 'Microsoft.Web/sites/slots/config@2022-03-01' = if(provisionAppSvcVNextSlot) {
  name: 'web'
  parent: appService::vnext
  properties: {
    publicNetworkAccess: 'Enabled'
    
    #disable-next-line BCP037 // Bicep linter is wrong.
    ipSecurityRestrictionsDefaultAction: 'Deny'
    
    #disable-next-line BCP037 // Bicep linter is wrong.
    scmIpSecurityRestrictionsDefaultAction: 'Allow'
    
    appSettings: concat(appSettings, appServiceUseBasicAuthVNext ? [appSettingBasicAuth] : [], [{name:'DisableSubscriptionsEngine',value:'true'}, {name:'AppHostName',value:appServiceHostNameVNext}])
  }
}


resource slotConfigNames 'Microsoft.Web/sites/config@2022-03-01' = if(provisionAppSvcVNextSlot) {
  name: 'slotConfigNames'
  parent: appService
  kind: 'string'
  properties:{
    appSettingNames: [
      'BasicAuthPassword'
      'AppHostName'
      'DisableSubscriptionsEngine'
    ]
  }
}



/*
  =================================  
  App service auto-scale
  =================================
*/
resource appServiceAutoscale 'Microsoft.Insights/autoscalesettings@2022-10-01' = if(appServiceUseAutoScale) {
  location: location
  name: 'autoscale-setting-${project}-${env}'
  properties: {
    enabled: true
    name: 'autoscale-setting-${project}-${env}'
    targetResourceUri: appServicePlan.id
    predictiveAutoscalePolicy:{
      scaleMode: 'Disabled'
    }
    profiles: [
      {
        name: 'autoscale'
        capacity: {
          minimum: '2'
          maximum: '10'
          default: '2'
        }
        rules: [
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: appServicePlan.id
              timeGrain: 'PT1M'
              statistic: 'Average'
              timeWindow: 'PT5M'
              timeAggregation: 'Average'
              operator: 'GreaterThan'
              threshold: 70
              dimensions: []
              dividePerInstance: false
            }
            scaleAction: {
              direction: 'Increase'
              type: 'ChangeCount'
              value: '1'
              cooldown: 'PT5M'
            }
          }
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: appServicePlan.id
              timeGrain: 'PT1M'
              statistic: 'Average'
              timeWindow: 'PT5M'
              timeAggregation: 'Average'
              operator: 'LessThan'
              threshold: 20
              dimensions: []
              dividePerInstance: false
            }
            scaleAction: {
              direction: 'Decrease'
              type: 'ChangeCount'
              value: '1'
              cooldown: 'PT5M'
            }
          }
        ]
      }
    ]

  }
}






// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++









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
      parameters: {
        '$connections': {
          defaultValue: {
          }
          type: 'Object'
        }
      }
      triggers: {
        DefenderSecurityAlert: {
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              callback_url: '@{listCallbackUrl()}'
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'ascalert\'][\'connectionId\']'
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
                name: '@parameters(\'$connections\')[\'azureblob\'][\'connectionId\']'
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
    parameters: {
      '$connections': {
        value: {
          ascalert: {
            connectionId: connectionDefenderConnection.id //'/subscriptions/{subcription_id}/resourceGroups/rg-playground/providers/Microsoft.Web/connections/ascalert'
            connectionName: 'ascalert'
            id: subscriptionResourceId('Microsoft.Web/locations/managedApis', location, 'ascalert') //'/subscriptions/{subcription_id}/providers/Microsoft.Web/locations/uksouth/managedApis/ascalert'
          }
          azureblob: {
            connectionId: connectionAzureBlob.id //'/subscriptions/{subcription_id}/resourceGroups/rg-playground/providers/Microsoft.Web/connections/azureblob'
            connectionName: 'azureblob'
            id: subscriptionResourceId('Microsoft.Web/locations/managedApis', location, 'azureblob')           //'/subscriptions/{subcription_id}/providers/Microsoft.Web/locations/uksouth/managedApis/azureblob'
          }
        }
      }
    }
  }

}
