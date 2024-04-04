# UK Market Conformity Assessment Bodies
---
[![dev-app-deploy](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/dev-app-deploy.yml/badge.svg)](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/dev-app-deploy.yml)
[![stage-app-deploy](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/stage-app-deploy.yml/badge.svg)](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/stage-app-deploy.yml)
[![preprod-app-deploy](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/preprod-app-deploy.yml/badge.svg)](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/preprod-app-deploy.yml)
[![prod-app-deploy](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/prod-app-deploy.yml/badge.svg)](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/actions/workflows/prod-app-deploy.yml)

Built by the Office for Product Safety and Standards
For enquiries, contact opss.enquiries@beis.gov.uk

## Overview
UKMCAB is written in C# / .NET 6, HTML/CSS/JavaScript.  The front-end depends on the GDS Design System.
It's backed Redis cache and Azure Cosmos DB.

Unit tests are found in the Test assembly and code unit test code coverage measurement is a manual execise at present. 

Email and SMS Notifications are facilitated via GOV.UK Notify.


## Running for the first time

### Preparation
Provision or prepare the following dependent services:
- Azure Storage
- Azure Cognitive Search
- Cosmos DB
- Redis
- GOV.UK Notify

_This can be done one of the following ways:_
- Manually
- Run the main.bicep file
- Use the GitHub Action (e.g., .github\workflows\deploy-bicep-dev.yml creates the resources for the dev environment)
  - Ensure the following repository secrets are configured:
    - basicAuthPassword
    - sslCertPfxBase64
    - dataProtectionX509CertBase64
    - govukNotifyApiKey


### Running locally 
(No need to run in Docker or any container as there is currently no benefit to this)
- Create your secrets.json file and populate with the following secrets:
  - DataConnectionString (Azure Storage)
  - AzureSearchEndPoint (Azure Cognitive Search URL)
  - AzureSearchKey (Azure Cognitive Search API key)
  - CosmosConnectionString (Cosmos DB connection string)
  - RedisConnectionString ()
  - GovUkNotifyApiKey
  - DataProtectionX509CertBase64
  
**Note: for local development, you'll want the `DataProtectionX509CertBase64` to be the same as the remote dev environment so that you can share the Cosmos DB between local and remote dev, otherwise data encryption wil fail.
The output can be copied into the github secret or `secrets.json` file.

## Setup steps
1. Setup environment variable for Nuget Package source see [src/NuGetPackageSourceSetup.md](https://github.com/OfficeForProductSafetyAndStandards/ukmcab/blob/b772867f448daa8f8eb44a14af47ae5f885debf6/src/NuGetPackageSourceSetup.md)
2. Install Node and NPM locally
3. Run `npm install` from src/UKMCAB.Web.UI
4. Run `npx webpack` from src/UKMCAB.Web.UI to run the webpack script
5. Update secrets file accordingly
6. Open the solution file in preferred ide and run or use the command `dotnet run`

## Generating an X509 self-signed certificate for data protection/encipherment
To create a new X509 cert, feel free to use the code below:
```
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

var cert = GenerateSelfSignedCertificate();
var exported = cert.Export(X509ContentType.Pfx);
var exportedBase64 = Convert.ToBase64String(exported);

Console.WriteLine(" -- X509 CERT BASE 64 -- ");
Console.WriteLine(exportedBase64);


static X509Certificate2 GenerateSelfSignedCertificate()
{
    var subjectName = "UKMCAB-Data-Protection-Keys";
    var rsa = RSA.Create();
    var certRequest = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment, true));
    var generatedCert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(10)); // generate the cert and sign!
    X509Certificate2 pfxGeneratedCert = new(generatedCert.Export(X509ContentType.Pfx), (string?)null, X509KeyStorageFlags.Exportable); //has to be turned into pfx or Windows at least throws a security credentials not found during sslStream.connectAsClient or HttpClient request...
    return pfxGeneratedCert;
}
```


## Unit test code coverage
Run `dotnet restore`
Generating a unit test code coverage report
Run the tests and collect the coverage raw data: dotnet test --collect:"XPlat Code Coverage"
Generate the coverage report as HTML: "%UserProfile%\.nuget\packages\reportgenerator\5.1.12\tools\net6.0\ReportGenerator.exe" -reports:TestResults\*\coverage.cobertura.xml -targetdir:coveragereport



