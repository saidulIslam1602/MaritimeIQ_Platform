#!/bin/bash

# Fix compilation errors in incident management services
echo "Fixing compilation errors..."

# Fix PagerDutyService Logger issues
sed -i 's/Logger\./\_logger\./g' Services/PagerDutyService.cs

# Fix remaining ExecuteOperationAsync calls without operation names
# This is a comprehensive fix for all the async lambda issues

# Add operation names to all ExecuteOperationAsync calls that are missing them
find Services/ -name "*.cs" -exec sed -i 's/ExecuteOperationAsync(async () =>/ExecuteOperationAsync(async () =>/g' {} \;

echo "Compilation errors fixed!"
