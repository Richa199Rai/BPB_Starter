# Banking API Starter (BPB_Starter)

A secure, cloud-ready .NET 8 Web API for managing banking accounts — built with CI/CD, API key authentication, Azure SQL, and integration testing.
Features

-  **.NET 8 Web API** with clean architecture
-  **API Key authentication** using a custom middleware
-  **Secrets stored in Azure Key Vault**
-  **CI/CD pipeline** via Azure DevOps
-  **Swagger** for API testing and documentation
-  **Integration tests** using xUnit and EF Core InMemory
-  **Deployed to Azure App Service** + Azure SQL

---

🧾 Endpoints

- GET /banking/accounts – Get all accounts  
  Requires X-API-Key header

Example request:
```http
GET /banking/accounts
X-API-Key: your-api-key-here


Local Setup
# Restore packages
dotnet restore

# Run API
dotnet run --project BankingApi

# Run tests
dotnet test BankingApi.Tests


Set the key in your launchSettings.json:
"environmentVariables": {
  "ApiKey": "your-api-key-here"
}
Or use environment variables:
$Env:ApiKey = "your-api-key-here"

This repo includes azure-pipelines.yml for Azure DevOps:

Restores dependencies
Builds and tests
Publishes to Azure App Service
Pulls secrets from Azure Key Vault
