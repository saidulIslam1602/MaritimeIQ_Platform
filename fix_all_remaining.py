#!/usr/bin/env python3
"""
Comprehensive fix for all remaining ExecuteOperationAsync calls
"""

import re

def fix_all_execute_operation_calls(file_path):
    """Fix all ExecuteOperationAsync calls in a file"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Pattern to find method signatures and their corresponding ExecuteOperationAsync calls
    # This will match: public async Task<ReturnType> MethodName(...) { return await ExecuteOperationAsync(async () => { ... });
    method_pattern = r'public async Task(?:<[^>]+>)?\s+(\w+)\s*\([^)]*\)\s*\{[^}]*?return await ExecuteOperationAsync\(async \(\) =>[^}]*?\}\);'
    
    def replace_method(match):
        method_name = match.group(1)
        full_match = match.group(0)
        # Replace the ending }); with }, nameof(MethodName));
        return full_match.replace('});', f'}}, nameof({method_name}));')
    
    content = re.sub(method_pattern, replace_method, content, flags=re.DOTALL)
    
    # Pattern for void methods: public async Task MethodName(...) { await ExecuteOperationAsync(async () => { ... });
    void_method_pattern = r'public async Task\s+(\w+)\s*\([^)]*\)\s*\{[^}]*?await ExecuteOperationAsync\(async \(\) =>[^}]*?\}\);'
    
    def replace_void_method(match):
        method_name = match.group(1)
        full_match = match.group(0)
        return full_match.replace('});', f'}}, nameof({method_name}));')
    
    content = re.sub(void_method_pattern, replace_void_method, content, flags=re.DOTALL)
    
    # Handle any remaining }); patterns that don't have operation names
    # Look for ExecuteOperationAsync calls that end with just });
    remaining_pattern = r'(ExecuteOperationAsync\(async \(\) =>[^}]*?\}\);)'
    
    # This is a fallback - we'll replace with a generic operation name
    def replace_remaining(match):
        return match.group(0).replace('});', '}, "Operation");')
    
    content = re.sub(remaining_pattern, replace_remaining, content, flags=re.DOTALL)
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"Comprehensively fixed {file_path}")

# Fix all service files
services = [
    'Services/IncidentManagementService.cs',
    'Services/OnCallService.cs',
    'Services/PagerDutyService.cs'
]

for service in services:
    fix_all_execute_operation_calls(service)

print("All ExecuteOperationAsync calls comprehensively fixed!")
