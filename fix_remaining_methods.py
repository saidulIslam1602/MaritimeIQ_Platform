#!/usr/bin/env python3
"""
Script to fix remaining ExecuteOperationAsync calls by adding proper operation name parameters
"""

import re
import os

def fix_execute_operation_calls(file_path):
    """Fix ExecuteOperationAsync calls in a specific file"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Find method signatures to get method names
    method_pattern = r'public async Task(?:<[^>]+>)?\s+(\w+)\s*\([^)]*\)\s*\{[^}]*?return await ExecuteOperationAsync\(async \(\) =>[^}]*?\}\);'
    
    methods = re.finditer(method_pattern, content, re.DOTALL)
    
    for match in methods:
        method_name = match.group(1)
        old_call = match.group(0)
        # Replace the ending }); with }, nameof(MethodName));
        new_call = old_call.replace('});', f'}}, nameof({method_name}));')
        content = content.replace(old_call, new_call)
    
    # Also handle void methods
    void_method_pattern = r'public async Task\s+(\w+)\s*\([^)]*\)\s*\{[^}]*?await ExecuteOperationAsync\(async \(\) =>[^}]*?\}\);'
    
    void_methods = re.finditer(void_method_pattern, content, re.DOTALL)
    
    for match in void_methods:
        method_name = match.group(1)
        old_call = match.group(0)
        new_call = old_call.replace('});', f'}}, nameof({method_name}));')
        content = content.replace(old_call, new_call)
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"Fixed ExecuteOperationAsync calls in {file_path}")

# Fix the remaining service files
services_to_fix = [
    'Services/IncidentManagementService.cs',
    'Services/OnCallService.cs', 
    'Services/PagerDutyService.cs'
]

for service in services_to_fix:
    if os.path.exists(service):
        fix_execute_operation_calls(service)
    else:
        print(f"File not found: {service}")

print("All ExecuteOperationAsync calls fixed!")
