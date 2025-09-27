# GitHub Actions Setup for Maritime Platform

## Required GitHub Repository Secrets

Go to your GitHub repository: https://github.com/saidulIslam1602/Maritime_DataEngineering_Plaatform

Navigate to: **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

### 1. Azure Container Registry Credentials

**Secret Name:** `ACR_USERNAME`
**Value:** 
```
maritimeplatformacr
```

**Secret Name:** `ACR_PASSWORD`  
**Value:**
```
[GET FROM COMMAND: az acr credential show --name maritimeplatformacr --query "passwords[0].value" -o tsv]
```

### 2. Azure Service Principal (for deployments)

**Secret Name:** `AZURE_CREDENTIALS`
**Value:** (Get from Azure CLI command below)
```bash
az ad sp create-for-rbac --name "GitHub-Actions-Maritime-Platform" \
  --role contributor \
  --scopes "/subscriptions/$(az account show --query id -o tsv)" \
  --sdk-auth
```

## GitHub Actions Workflow Features

✅ **Automated CI/CD Pipeline**
- Triggers on push to main/develop branches
- Manual workflow dispatch option
- Pull request validation

✅ **Multi-Stage Build Process**
1. **Build & Test**: .NET application build and unit tests
2. **Docker Build**: Multi-platform container build with caching
3. **Security Scan**: Trivy vulnerability scanning
4. **Staging Deploy**: Automatic deployment to Azure Container Instances
5. **Integration Tests**: API endpoint validation
6. **Production Deploy**: Manual approval required

✅ **Advanced Features**
- Docker layer caching for faster builds
- Semantic versioning with Git metadata
- Environment-specific configurations
- Health check validations
- Security scanning with SARIF reporting

✅ **Production Ready**
- Staging environment for testing
- Production environment with manual approval
- Resource scaling (staging: 1 CPU, prod: 2 CPU)
- Comprehensive logging and monitoring

## Next Steps

1. **Add the secrets** to your GitHub repository
2. **Push the workflow** to trigger the first build
3. **Monitor the Actions tab** for pipeline execution
4. **Access your APIs** at the deployed endpoints

## Deployment URLs (after workflow runs)

- **Staging**: `http://havila-maritime-staging-{run-number}.norwayeast.azurecontainer.io:8080`
- **Production**: `http://havila-maritime-prod.norwayeast.azurecontainer.io:8080`
- **Swagger API**: Add `/swagger` to any URL above

## Interview Talking Points

🎯 **DevOps Excellence**: GitHub Actions with multi-stage pipeline
🎯 **Cloud Native**: Azure Container Registry + Container Instances  
🎯 **Security First**: Vulnerability scanning, secret management
🎯 **Quality Gates**: Automated testing, health checks, approval workflows
🎯 **Maritime Domain**: Industry-specific APIs and business logic