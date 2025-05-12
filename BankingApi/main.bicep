targetScope = 'resourceGroup'

@description('Location for all resources')
param location              string = 'australiaeast'

@description('SQL administrator login name')
param sqlAdmin              string = 'sqladmin'

@secure()
@description('SQL administrator password')
param sqlPassword    string

@secure()
@description('The API Key clients must send (X-API-Key header)')
param apiKeyValue string

@description('Name of the App Service (Web App)')
param appServiceName        string = 'bigpurplebankifysy3weuhd6k'

@description('Name of the existing App Service Plan')
param appServicePlanName    string = 'plan-bigpurplebank-ifysy3weuhd6k'

@description('SKU for the App Service Plan')
param appServiceSku         string = 'B1'

@description('Name of the Azure SQL database')
param dbName                string = 'BigPurpleBankDB'

@description('Name of the Key Vault')
param keyVaultName          string = 'kv-bpb-ifysy3weuhd6k'

/* 1) Reuse your existing Windows App Service Plan */
resource plan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: appServicePlanName
}

/* 2) Web App with System-Assigned Identity & Key Vault reference */
resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name:     appServiceName
  location: location
  kind:     'app'                // Windows
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      // Windows .NET apps use the default .NET runtime, no linuxFxVersion
      appSettings: [
        {
          name:  'ConnectionStrings:DefaultConnection'
          value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/DbConnectionString/)'
        }
        {
          name:  'ApiKey'
          value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/ApiKey/)'
        }
      ]
    }
  }
  dependsOn: [
    plan
  ]
}

/* 3) Azure SQL Server */
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name:     '${appServiceName}-sql'
  location: location
  properties: {
    administratorLogin:         sqlAdmin
    administratorLoginPassword: sqlPassword
  }
  sku: {
    name: 'GP_Gen5_2'
  }
}

/* 4) Azure SQL Database */
resource sqlDb 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent:   sqlServer
  name:     dbName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

/* 5) Key Vault with access policy for the Web App’s identity */
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name:     keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      name:   'standard'
      family: 'A'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: webApp.identity.principalId
        permissions: {
          secrets: [ 'get' ]
        }
      }
    ]
    enableSoftDelete:             true
    enabledForDeployment:         true
    enabledForTemplateDeployment: true
    enabledForDiskEncryption:     true
  }
}

/* 6) Secret in Key Vault containing the SQL connection string */
var connectionString = 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Initial Catalog=${dbName};Persist Security Info=False;User ID=${sqlAdmin};Password=${sqlPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

resource dbSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name:       '${keyVault.name}/DbConnectionString'
  properties: {
    value: connectionString
  }
  dependsOn: [
    sqlDb
    keyVault
  ]
}

/* 7) Add APIKey in vault*/
resource apiKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVault.name}/ApiKey'
  properties: {
    value: apiKeyValue
  }
  dependsOn: [
    keyVault
  ]
}


/* 8) Outputs */
output webAppUrl     string = 'https://${appServiceName}.azurewebsites.net'
output sqlServerName string = sqlServer.name
output keyVaultUri   string = keyVault.properties.vaultUri
