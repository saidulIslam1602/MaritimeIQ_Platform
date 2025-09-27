# CI/CD Pipeline Tutorial: From Zero to Hero

## What is CI/CD?

**CI/CD** stands for **Continuous Integration** and **Continuous Deployment**:

- **Continuous Integration (CI)**: Automatically test code when developers push changes
- **Continuous Deployment (CD)**: Automatically deploy tested code to production

Think of it like an **assembly line for software** - each stage does something specific, and if any step fails, the whole process stops.

## The Old Way vs The New Way

### 😰 The Old Way (Manual):
1. Developer writes code on laptop
2. Developer manually runs tests (maybe)
3. Developer manually builds Docker image
4. Developer manually uploads to server
5. Developer manually restarts services
6. **Problems**: Human errors, forgotten steps, different environments

### 🚀 The New Way (Automated):
1. Developer pushes code to GitHub
2. **Pipeline automatically**: tests → builds → scans → deploys
3. **Result**: Consistent, reliable, fast deployments

---

## Our Pipeline Architecture

Let me break down the 5-stage pipeline we built for your maritime platform:

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   STAGE 1   │ → │   STAGE 2   │ → │   STAGE 3   │ → │   STAGE 4   │ → │   STAGE 5   │
│ Build & Test│    │  Security   │    │  Deploy     │    │Integration  │    │  Production │
│             │    │  Scanning   │    │  Staging    │    │  Testing    │    │  Deployment │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

---

## Stage 1: Build & Test (The Foundation)

### What it does:
```yaml
- name: Setup .NET
- name: Restore dependencies  
- name: Build application
- name: Run tests
- name: Build Docker image
```

### Why this matters:
- **Catches bugs early** before they reach production
- **Ensures code compiles** on a clean machine (not just your laptop)
- **Creates consistent Docker images** that work everywhere

### Real Example from your pipeline:
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'  # Ensures exact .NET version

- name: Build application
  run: dotnet build --no-restore --configuration Release
```

**Translation**: "Set up .NET 8, download dependencies, build the app in Release mode"

---

## Stage 2: Security Scanning (The Guardian)

### What it does:
```yaml
- name: Run Trivy vulnerability scanner
```

### Why this matters:
- **Scans your Docker image** for known security vulnerabilities
- **Blocks deployment** if critical security issues found
- **Generates reports** for compliance and auditing

### Real Example:
```yaml
- name: Run Trivy vulnerability scanner
  uses: aquasecurity/trivy-action@master
  with:
    image-ref: ${{ needs.build-and-test.outputs.image-tag }}
    format: 'sarif'
```

**Translation**: "Scan the Docker image we just built and tell us if it has security problems"

---

## Stage 3: Deploy Staging (The Test Environment)

### What it does:
```yaml
- name: Deploy to Azure Container Instances (Staging)
```

### Why this matters:
- **Creates a production-like environment** for testing
- **Isolated from production** - safe to break
- **Automatic deployment** - no manual steps

### Real Example:
```yaml
- name: Deploy to Azure Container Instances (Staging)
  uses: azure/aci-deploy@v1
  with:
    resource-group: ${{ env.AZURE_RESOURCE_GROUP }}
    dns-name-label: havila-maritime-staging-${{ github.run_number }}
    image: ${{ needs.build-and-test.outputs.image-tag }}
    cpu: 1
    memory: 1.5
```

**Translation**: "Create a new Azure container with our app, give it 1 CPU and 1.5GB RAM, and make it accessible via web"

---

## Stage 4: Integration Testing (The Validator)

### What it does:
```yaml
- name: Run API Integration Tests
```

### Why this matters:
- **Tests the real deployed application** (not just unit tests)
- **Validates all APIs work** in the actual cloud environment
- **Catches deployment issues** before production

### Real Example:
```yaml
- name: Run API Integration Tests
  run: |
    BASE_URL="http://havila-maritime-staging-${{ github.run_number }}.norwayeast.azurecontainer.io:8080"
    
    # Test Fleet Management API
    curl -f "$BASE_URL/api/vessels" | jq '.[] | select(.name == "MS Havila Castor")'
```

**Translation**: "Call our real APIs in staging and make sure they return the expected data"

---

## Stage 5: Production Deployment (The Final Step)

### What it does:
```yaml
environment: production  # Requires manual approval
- name: Deploy to Azure Container Instances (Production)
```

### Why this matters:
- **Manual approval required** - human gate for production
- **Production resources** (more CPU/memory)
- **Real user traffic** - this is the live system

### Real Example:
```yaml
deploy-production:
  environment: production  # This creates an approval gate
  steps:
  - name: Deploy to Azure Container Instances (Production)
    with:
      name: havila-maritime-production
      cpu: 2      # More resources than staging
      memory: 3
```

**Translation**: "Wait for human approval, then deploy to production with more powerful resources"

---

## Key Concepts Explained

### 1. **Triggers** (When does the pipeline run?)
```yaml
on:
  push:
    branches: [ main, develop ]  # Run when code is pushed
  pull_request:
    branches: [ main ]           # Run when pull request created
  workflow_dispatch:             # Allow manual triggers
```

### 2. **Jobs** (Parallel work)
```yaml
jobs:
  build-and-test:    # Job 1: Build the app
  security-scan:     # Job 2: Scan for vulnerabilities  
  deploy-staging:    # Job 3: Deploy to staging
```

### 3. **Dependencies** (Order of execution)
```yaml
security-scan:
  needs: build-and-test  # Wait for build to finish first
```

### 4. **Secrets** (Secure credentials)
```yaml
username: ${{ secrets.ACR_USERNAME }}    # GitHub stores this securely
password: ${{ secrets.ACR_PASSWORD }}    # Never visible in logs
```

### 5. **Environments** (Approval gates)
```yaml
environment: production  # Requires manual approval in GitHub
```

---

## The Files We Created

### 1. **`.github/workflows/maritime-cicd.yml`**
- **Purpose**: The pipeline definition
- **Contains**: All 5 stages, triggers, and configurations
- **GitHub automatically**: Finds and runs this file

### 2. **`GitHub-Actions-Setup.md`**
- **Purpose**: Setup instructions for secrets
- **Contains**: Step-by-step guide for GitHub configuration
- **Security**: No actual secrets (removed by GitHub's protection)

### 3. **`SECRETS-LOCAL-ONLY.txt`** (local only)
- **Purpose**: Actual credentials for setup
- **Security**: Added to .gitignore, never committed
- **Usage**: Copy values to GitHub secrets, then delete

---

## What Happens When You Push Code?

Let me trace through a real deployment:

### Step 1: You Push Code
```bash
git add .
git commit -m "Fix bug in vessel API"
git push origin main
```

### Step 2: GitHub Detects Change
- GitHub sees new commit on `main` branch
- Automatically starts the pipeline
- You can watch in "Actions" tab

### Step 3: Pipeline Executes
```
✅ Stage 1: Build & Test (2 minutes)
   - Downloads .NET 8
   - Builds your C# application  
   - Runs unit tests
   - Creates Docker image
   - Pushes to Azure Container Registry

✅ Stage 2: Security Scan (1 minute)
   - Scans Docker image for vulnerabilities
   - Generates security report
   - Fails pipeline if critical issues found

✅ Stage 3: Deploy Staging (3 minutes)
   - Creates Azure Container Instance
   - Deploys your app with staging resources
   - Makes it accessible via public URL

✅ Stage 4: Integration Tests (1 minute)
   - Calls your APIs in staging environment
   - Validates all endpoints work correctly
   - Checks data quality and responses

⏸️  Stage 5: Production Deployment (Manual)
   - Waits for your approval in GitHub
   - You click "Approve" when ready
   - Deploys to production with more resources
```

### Step 4: Your App is Live!
- **Staging URL**: `http://havila-maritime-staging-123.norwayeast.azurecontainer.io:8080`
- **Production URL**: `http://havila-maritime-prod.norwayeast.azurecontainer.io:8080`
- **APIs**: Add `/api/vessels` or `/swagger` to test

---

## Why This is Impressive for Havila Kystruten

### 1. **Professional DevOps Practices**
- Multi-stage pipeline with quality gates
- Automated testing and security scanning
- Environment separation (staging/production)

### 2. **Cloud-Native Architecture**  
- Azure Container Registry for image storage
- Azure Container Instances for deployment
- Scalable and resilient infrastructure

### 3. **Security-First Approach**
- Vulnerability scanning on every build
- Secret management with GitHub secrets
- Approval workflows for production changes

### 4. **Maritime Domain Expertise**
- Industry-specific APIs (vessels, routes, safety)
- Norwegian coastal operations focus
- Havila Kystruten fleet management

### 5. **Enterprise-Ready Features**
- Health checks and monitoring
- Comprehensive logging
- Rollback capabilities
- Documentation and setup guides

---

## Common Pipeline Patterns You Should Know

### 1. **Feature Branch Workflow**
```
Developer creates branch → Makes changes → Creates pull request → 
Pipeline runs tests → Code review → Merge to main → Deploy
```

### 2. **Environment Promotion**
```
Code → Dev Environment → Staging Environment → Production Environment
```

### 3. **Blue-Green Deployment**
```
Blue (current production) ← Users
Green (new version) ← Deploy here, test, then switch traffic
```

### 4. **Rollback Strategy**
```
If production deployment fails → Automatically rollback to previous version
```

---

## What Makes Our Pipeline Special?

### ✅ **Beginner-Friendly**
- Clear stage separation
- Comprehensive documentation
- Error handling and rollback

### ✅ **Production-Ready**
- Security scanning
- Manual approval gates
- Health checks and monitoring

### ✅ **Cost-Effective**
- Uses free GitHub Actions minutes
- Azure Container Instances (pay-per-use)
- Efficient Docker layer caching

### ✅ **Scalable**
- Easy to add new environments
- Simple to extend with new stages
- Can handle multiple applications

---

## Next Steps to Master CI/CD

### 1. **Practice with Changes**
- Make small code changes
- Watch the pipeline run
- Understand each stage's logs

### 2. **Experiment with Configurations**
- Add new API endpoints
- Modify resource allocations
- Try different deployment strategies

### 3. **Learn Advanced Patterns**
- Canary deployments
- A/B testing
- Database migrations
- Infrastructure as Code

### 4. **Monitor and Optimize**
- Track deployment success rates
- Optimize build times
- Improve test coverage

---

## Interview Talking Points

When discussing this pipeline with Havila Kystruten:

🎯 **"I built a complete CI/CD pipeline that automatically tests, secures, and deploys maritime applications to Azure"**

🎯 **"The pipeline includes security scanning, staging environments, and production approval workflows"**

🎯 **"Every code change is automatically validated through 5 quality gates before reaching production"**

🎯 **"The system can deploy updates in minutes while maintaining high reliability and security standards"**

This pipeline demonstrates enterprise-level DevOps practices that any shipping company would value!