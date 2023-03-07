
param project string = 'ukmcab'
param env string = 'dev'
param location string = 'uksouth'
param appServicePlanSkuName string = 'B1' // defaults to B1 for dev/stage/test

@secure()
param basicAuthPassword string = ''

@secure()
param sslCertPfxBase64 string = ''

@secure()
param dataProtectionX509CertBase64 string = ''

@secure()
param govukNotifyApiKey string = ''

param aspNetCoreEnvironment string = 'Development'

targetScope = 'subscription'

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
  }
}
