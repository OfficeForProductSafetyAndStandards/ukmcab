# Enabling OPSS NuGet package source
1) Create a GitHub personal access token with `read:packages` permission: \
https://github.com/settings/tokens
1) Create environment variable `OPSS_UKMCAB_PACKAGES_TOKEN` with the value of the GitHub personal access token 
1) Build the solution

If nuget pretends a package version isn't available and you know it is, go to \
`%localappdata%\NuGet\v3-cache` and delete \
`8a8dd53763889d90f6e168259b4f605d628e235d$uctSafetyAndStandards_index.json`
