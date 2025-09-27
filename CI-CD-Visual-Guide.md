# Visual Guide: Your CI/CD Pipeline in Action

## The Big Picture: What Happens When You Push Code

```
    📱 Your Laptop           🌐 GitHub              ☁️  Azure Cloud
    ┌─────────────┐         ┌──────────────┐       ┌─────────────────┐
    │             │  push   │              │       │                 │
    │ git push    │────────▶│  Repository  │       │ Azure Container │        
    │ origin main │         │              │       │   Registry      │
    │             │         └──────┬───────┘       │                 │
    └─────────────┘                │               └─────────────────┘
                                   │                        ▲
                                   ▼                        │
                            ┌─────────────┐                │
                            │   GitHub    │                │
                            │   Actions   │────────────────┘
                            │  (Pipeline) │
                            └─────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────────┐
                    │      Staging Environment        │
                    │  havila-maritime-staging.com    │
                    └─────────────────────────────────┘
                                   │ (after tests pass)
                                   ▼
                    ┌─────────────────────────────────┐
                    │     Production Environment      │
                    │  havila-maritime-prod.com      │
                    └─────────────────────────────────┘
```

## Step-by-Step Breakdown

### STEP 1: The Trigger
```yaml
on:
  push:
    branches: [ main, develop ]  # When you push to main or develop
```
**What this means**: "Start the pipeline whenever someone pushes code to the main or develop branch"

### STEP 2: Environment Setup
```yaml
env:
  REGISTRY: maritimeplatformacr.azurecr.io
  IMAGE_NAME: havila-maritime
```
**What this means**: "These are variables we'll use throughout the pipeline"

### STEP 3: The Jobs (Stages)

#### Job 1: Build and Test
```yaml
build-and-test:
  runs-on: ubuntu-latest  # Use a fresh Ubuntu computer
  steps:
  - name: Checkout repository    # Download your code
  - name: Setup .NET            # Install .NET 8
  - name: Build application     # Compile your C# code
  - name: Run tests            # Run your unit tests
  - name: Build Docker image   # Create container image
```

#### Job 2: Security Scan
```yaml
security-scan:
  needs: build-and-test  # Wait for build to finish first
  steps:
  - name: Run Trivy vulnerability scanner  # Check for security issues
```

#### Job 3: Deploy to Staging
```yaml
deploy-staging:
  needs: [build-and-test, security-scan]  # Wait for both previous jobs
  steps:
  - name: Deploy to Azure Container Instances  # Create staging environment
```

#### Job 4: Integration Tests
```yaml
integration-tests:
  needs: deploy-staging  # Wait for staging deployment
  steps:
  - name: Run API Integration Tests  # Test the real deployed app
```

#### Job 5: Deploy to Production
```yaml
deploy-production:
  environment: production  # Requires manual approval
  steps:
  - name: Deploy to Azure Container Instances (Production)
```

## Real Example: What Gets Built

When your pipeline runs, it creates these actual resources:

### 1. Docker Image
```
maritimeplatformacr.azurecr.io/havila-maritime:main-a1b2c3d
```
- Contains your .NET application
- Includes all dependencies
- Optimized and secure

### 2. Staging Environment
```
URL: http://havila-maritime-staging-123.norwayeast.azurecontainer.io:8080
Resources: 1 CPU, 1.5GB RAM
APIs: /api/vessels, /api/routes, /api/safety, /api/iot
Swagger: /swagger
```

### 3. Production Environment
```
URL: http://havila-maritime-prod.norwayeast.azurecontainer.io:8080
Resources: 2 CPU, 3GB RAM
APIs: Same as staging but with production data
Health Check: /health
```

## The Secret Sauce: GitHub Actions Syntax

Let me explain the YAML syntax we use:

### Basic Structure
```yaml
name: Maritime Platform CI/CD        # Pipeline name
on: [push, pull_request]            # When to run
jobs:                               # What to do
  job-name:                         # Name of job
    runs-on: ubuntu-latest          # What computer to use
    steps:                          # List of actions
    - name: Do something            # Step name
      run: echo "Hello"             # Command to run
```

### Using Actions (Pre-built Components)
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4     # Someone else built this
  with:                             # Configuration
    dotnet-version: '8.0.x'
```

### Using Secrets (Secure Values)
```yaml
- name: Login to Azure
  with:
    username: ${{ secrets.ACR_USERNAME }}    # Stored securely in GitHub
    password: ${{ secrets.ACR_PASSWORD }}    # Never visible in logs
```

### Passing Data Between Jobs
```yaml
jobs:
  build:
    outputs:                        # Export data
      image-tag: ${{ steps.meta.outputs.tags }}
  
  deploy:
    needs: build                    # Import data
    steps:
    - name: Deploy
      with:
        image: ${{ needs.build.outputs.image-tag }}
```

## The Files in Our Project

```
azure_container_service/
├── .github/workflows/
│   └── maritime-cicd.yml           # 🚀 THE PIPELINE (main file)
├── Controllers/                    # Your API endpoints
├── Models/                         # Data structures
├── Dockerfile                      # How to build container
├── Program.cs                      # Application startup
├── GitHub-Actions-Setup.md         # Setup instructions
├── CI-CD-Tutorial-Complete.md      # This tutorial
└── SECRETS-LOCAL-ONLY.txt         # Credentials (local only)
```

## Monitoring Your Pipeline

### 1. In GitHub Repository
- Go to "Actions" tab
- See all pipeline runs
- Click on any run to see details
- View logs for each step

### 2. Pipeline Status
```
✅ Build and Test (2m 15s)
✅ Security Scan (45s)  
✅ Deploy Staging (3m 30s)
✅ Integration Tests (1m 10s)
⏸️  Deploy Production (waiting for approval)
```

### 3. Deployment URLs
After successful deployment, you'll see:
```
Staging: http://havila-maritime-staging-123.norwayeast.azurecontainer.io:8080
Production: http://havila-maritime-prod.norwayeast.azurecontainer.io:8080
```

## What Makes This Professional?

### 1. **Quality Gates**
- Code must build successfully
- Tests must pass
- Security scan must pass
- Integration tests must pass
- Manual approval required for production

### 2. **Environment Separation**
- Staging environment for testing
- Production environment for real users
- Different resource allocations
- Isolated from each other

### 3. **Security Best Practices**
- Vulnerability scanning on every build
- Secrets stored securely in GitHub
- No credentials in code
- Approval workflows for production

### 4. **Monitoring and Observability**
- Health checks on deployments
- Comprehensive logging
- Integration test validation
- Deployment status tracking

## Common Issues and Solutions

### Issue 1: Build Fails
```
Error: dotnet build failed
Solution: Check your C# code for compilation errors
```

### Issue 2: Tests Fail
```
Error: Unit tests failed
Solution: Fix failing tests before pipeline can continue
```

### Issue 3: Security Scan Fails
```
Error: Critical vulnerabilities found
Solution: Update dependencies or base Docker image
```

### Issue 4: Deployment Fails
```
Error: Container won't start
Solution: Check application configuration and health endpoints
```

## Next Level: What You Could Add

### 1. **Database Migrations**
```yaml
- name: Run Database Migrations
  run: dotnet ef database update
```

### 2. **Performance Testing**
```yaml
- name: Load Test APIs
  run: artillery run load-test.yml
```

### 3. **Multi-Region Deployment**
```yaml
- name: Deploy to Multiple Regions
  strategy:
    matrix:
      region: [norwayeast, westeurope, eastus]
```

### 4. **Slack Notifications**
```yaml
- name: Notify Team
  uses: 8398a7/action-slack@v3
  with:
    status: success
    text: "🚢 Maritime platform deployed!"
```

This pipeline represents enterprise-level DevOps practices that any modern shipping company would be impressed with!