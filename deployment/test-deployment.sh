#!/bin/bash

# Test script to validate deployment components
set -e

echo "🔍 Testing Maritime Platform Deployment Components"
echo "=================================================="

# Test 1: Check Azure CLI
echo "✅ Testing Azure CLI..."
if command -v az &> /dev/null; then
    echo "   Azure CLI: Installed ✓"
    if az account show &> /dev/null; then
        echo "   Azure Login: Authenticated ✓"
    else
        echo "   Azure Login: Not authenticated ⚠️"
    fi
else
    echo "   Azure CLI: Not installed ❌"
fi

# Test 2: Check Node.js/npm
echo "✅ Testing Node.js/npm..."
if command -v npm &> /dev/null; then
    echo "   Node.js/npm: Installed ✓"
else
    echo "   Node.js/npm: Not installed ❌"
fi

# Test 3: Check template file
echo "✅ Testing ARM template..."
TEMPLATE_FILE="deployment/azure/azure-infrastructure.json"
if [ -f "$TEMPLATE_FILE" ]; then
    echo "   Template file: Found ✓"
    # Basic JSON validation
    if python3 -m json.tool "$TEMPLATE_FILE" > /dev/null 2>&1; then
        echo "   Template JSON: Valid ✓"
    else
        echo "   Template JSON: Invalid ❌"
    fi
else
    echo "   Template file: Not found ❌"
fi

# Test 4: Check dashboard files
echo "✅ Testing dashboard files..."
if [ -f "maritime-dashboard/package.json" ]; then
    echo "   Dashboard package.json: Found ✓"
else
    echo "   Dashboard package.json: Not found ❌"
fi

if [ -f "maritime-dashboard/deploy.sh" ]; then
    echo "   Dashboard deploy script: Found ✓"
    if [ -x "maritime-dashboard/deploy.sh" ]; then
        echo "   Dashboard deploy script: Executable ✓"
    else
        echo "   Dashboard deploy script: Not executable ❌"
    fi
else
    echo "   Dashboard deploy script: Not found ❌"
fi

# Test 5: Check deployment scripts
echo "✅ Testing deployment scripts..."
for script in "deployment/deploy.sh" "deployment/deploy-all.sh"; do
    if [ -f "$script" ]; then
        echo "   $script: Found ✓"
        if [ -x "$script" ]; then
            echo "   $script: Executable ✓"
        else
            echo "   $script: Not executable ❌"
        fi
        # Syntax check
        if bash -n "$script"; then
            echo "   $script: Syntax valid ✓"
        else
            echo "   $script: Syntax error ❌"
        fi
    else
        echo "   $script: Not found ❌"
    fi
done

echo ""
echo "🎯 Test completed! Check results above for any issues."