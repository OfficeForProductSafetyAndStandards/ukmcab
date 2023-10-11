# CI/CD notes
- As you push changes to the `dev` branch changes will be deployed automatically
- If you change anything under the bicep folder - then the `dev-iac-deploy` GitHub Action will invoke
- All other changes will invoke the `dev-app-deploy` Action
- The GitHub Actions reference GitHub Secrets (https://github.com/OfficeForProductSafetyAndStandards/ukmcab/settings/secrets/actions) which are then deployed into Key Vault secrets via bicep code.

# Environments
- Dev: https://ukmcab-dev.beis.gov.uk/
- Stage: https://ukmcab-stage.beis.gov.uk/
- Preprod: https://ukmcab-pp.beis.gov.uk/
- Production: 
	- vnext: https://ukmcab-vnext.beis.gov.uk (slot next to prod)
	- prod: https://find-a-conformity-assessment-body.service.gov.uk (production site main slot)

All environments are separate and self-contained.  The production environment is configured with slots.

# UKMCAB release process
---
1. Develop on `dev` branch
1. When `dev` is ready for a release cut, create a new branch off `dev` called `release/vX.X`
1. To release to stage, manually invoke the `stage-iac-deploy` and `stage-app-deploy` Actions (selecting the new `release/vX.X` branch)
1. If you wish to release to preprod, manually invoke the `preprod-iac-deploy` and `preprod-app-deploy` Actions (selecting the new `release/vX.X` branch)
1. To release to production, you actually release into a `vnext` slot.  GitHub Actions never deploy directly into production.
	1. Manually invoke `prod-iac-deploy` and `prod-app-deploy` (selecting the new `release/vX.X` branch)
	1. A while later the application will be available in the vnext site.
	1. It is wise to turn the vnext site OFF when it's not being used - remember there are background processes running in vnext as soon as it's deployed.
	1. It is a good idea to ensure that all data changes are backward compatible should you need to rollback
1. GOING LIVE: To go-live with a deployment that's already in vnext - you need to swap the slots in the app service.  Therefore you will need to request production access beforehand.
1. ROLLBACK: If for some reason you need to rollback, you just swap the slots back.  
Bear in mind backward compatibility - if the vnext app changes the data substantially, is the current production version going to work with that.
1. Clear redis cache
1. Merge `release/vX.X` into `master` and delete the release branch


