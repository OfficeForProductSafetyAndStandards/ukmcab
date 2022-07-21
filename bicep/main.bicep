
param project string = 'ukmcab'
param env string = 'dev'
param location string = 'uksouth'
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
  }
}
