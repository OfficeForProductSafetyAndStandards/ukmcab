
param env string
param location string
param appServicePlanSkuName string
param storageAccountSkuName string
param provisionAppSvcVNextSlot bool
param appServiceUseBasicAuth bool
param appServiceUseBasicAuthVNext bool
param appServiceHostName string
param appServiceHostNameVNext string


@secure()
param basicAuthPassword string

@secure()
param sslCertPfxBase64 string

@secure()
param dataProtectionX509CertBase64 string

@secure()
param govukNotifyApiKey string

param aspNetCoreEnvironment string

@secure()
param sslCertPfxBase64VNextSlot string

targetScope = 'subscription'

var project = 'ukmcab'

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${project}-${env}'
  location: location
}

module resourceSet './resources.bicep' = {
  name: 'resource-set'
  scope: rg
  params: {
    project: project
    env: env
    location: location
    basicAuthPassword: basicAuthPassword
    sslCertPfxBase64: sslCertPfxBase64
    dataProtectionX509CertBase64: dataProtectionX509CertBase64
    govukNotifyApiKey: govukNotifyApiKey
    aspNetCoreEnvironment: aspNetCoreEnvironment
    appServicePlanSkuName: appServicePlanSkuName
    storageAccountSkuName: storageAccountSkuName
    provisionAppSvcVNextSlot: provisionAppSvcVNextSlot
    sslCertPfxBase64VNextSlot: sslCertPfxBase64VNextSlot
    appServiceUseBasicAuth: appServiceUseBasicAuth
    appServiceUseBasicAuthVNext: appServiceUseBasicAuthVNext
    appServiceHostName: appServiceHostName
    appServiceHostNameVNext: appServiceHostNameVNext
  }
}
