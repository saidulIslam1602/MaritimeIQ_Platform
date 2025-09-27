# Havila Kystruten Maritime Service - ACR Tasks Setup

## Quick Task (Immediate Build)
Create a quick task to build your maritime service:

```bash
# Build from current directory
az acr build --registry maritimeplatformacr \
  --image havila/maritime-api:{{.Run.ID}} \
  --file Dockerfile .
```

## Multi-Step Task (Full CI/CD Pipeline)
Create automated pipeline using your havila-maritime-task.yaml:

```bash
# Create the task
az acr task create \
  --registry maritimeplatformacr \
  --name havila-maritime-pipeline \
  --context https://github.com/youraccount/maritime-service.git \
  --file havila-maritime-task.yaml \
  --commit-trigger-enabled true \
  --pull-request-trigger-enabled true
```

## Git-Triggered Task (Automated on Push)
Set up automatic builds when you push code:

```bash
# Create Git-triggered task
az acr task create \
  --registry maritimeplatformacr \
  --name havila-auto-build \
  --context https://github.com/youraccount/maritime-service.git \
  --file Dockerfile \
  --image havila/maritime-api:{{.Run.ID}} \
  --git-access-token <your-github-token> \
  --commit-trigger-enabled true
```

## Scheduled Task (Regular Security Updates)
Build regularly to get security updates:

```bash
# Create scheduled task (daily at 2 AM UTC)
az acr task create \
  --registry maritimeplatformacr \
  --name havila-security-updates \
  --context https://github.com/youraccount/maritime-service.git \
  --file Dockerfile \
  --image havila/maritime-api:security-{{.Date.YYYY-MM-DD}} \
  --schedule "0 2 * * *"
```

## Interview Talking Points

### DevOps Capabilities:
- "Implemented automated CI/CD using ACR Tasks"
- "Set up Git-triggered builds for continuous integration"
- "Configured scheduled builds for security updates"
- "Used multi-stage pipelines for build, test, and deploy"

### Maritime-Specific Value:
- "Critical for vessel software updates with zero downtime"
- "Automated security scanning for maritime compliance"
- "Multi-region deployments for global fleet operations"
- "Integration with maritime IoT sensor updates"

### Technical Excellence:
- "Infrastructure as Code using YAML pipelines"
- "Container vulnerability scanning in pipeline"
- "Blue-green deployments for production safety"
- "Rollback capabilities for emergency situations"