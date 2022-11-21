
param project string = 'ukmcab'
param env string = 'dev'
param location string = 'uksouth'

@secure()
param basicAuthPassword string = ''

@secure()
param sslCertPfxBase64 string = ''

@secure()
param dataProtectionX509CertBase64 string = ''

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
  }
}
