#!/bin/bash

# Deploy Databricks Notebooks to Workspace
# Standard deployment script for Databricks notebooks

set -e

echo "🚀 Deploying MaritimeIQ Databricks Notebooks"

# Configuration
WORKSPACE_PATH="/Workspace/MaritimeIQ"
NOTEBOOKS_DIR="./Notebooks"

# Check if databricks CLI is configured
if ! command -v databricks &> /dev/null; then
    echo "❌ Databricks CLI not found. Install with: pip install databricks-cli"
    exit 1
fi

# Check authentication
echo "📋 Checking Databricks authentication..."
if ! databricks workspace ls / &> /dev/null; then
    echo "❌ Not authenticated. Run: databricks configure --token"
    exit 1
fi

echo "✅ Authentication confirmed"

# Create workspace directory
echo "📁 Creating workspace directory: $WORKSPACE_PATH"
databricks workspace mkdirs "$WORKSPACE_PATH" || true

# Upload notebooks
echo "📤 Uploading notebooks..."
databricks workspace import_dir \
    "$NOTEBOOKS_DIR" \
    "$WORKSPACE_PATH" \
    --overwrite

echo "✅ Notebooks deployed successfully!"
echo ""
echo "📍 Access your notebooks at:"
echo "   $WORKSPACE_PATH/01_Maritime_Data_Ingestion"
echo "   $WORKSPACE_PATH/02_Maritime_Data_Processing"
echo ""
echo "🎯 Next steps:"
echo "   1. Create a Databricks cluster"
echo "   2. Mount Azure Storage: /mnt/maritime"
echo "   3. Run the ingestion notebook"