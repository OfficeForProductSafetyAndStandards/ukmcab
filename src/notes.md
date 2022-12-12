# Notes

## Generating a unit test code coverage report

- Run the tests and collect the coverage raw data: `dotnet test --collect:"XPlat Code Coverage"`
- Generate the coverage report as HTML:
`"%UserProfile%\.nuget\packages\reportgenerator\5.1.12\tools\net6.0\ReportGenerator.exe" -reports:TestResults\*\coverage.cobertura.xml -targetdir:coveragereport`


