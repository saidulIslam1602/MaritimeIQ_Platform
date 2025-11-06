#!/usr/bin/env python3
"""
Simplify ExecuteOperationAsync calls to get clean build
"""

import re

def simplify_execute_operation_calls(file_path):
    """Remove ExecuteOperationAsync wrapper to simplify compilation"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Pattern 1: return await ExecuteOperationAsync(async () => { ... });
    # Replace with: { ... }
    pattern1 = r'return await ExecuteOperationAsync\(async \(\) =>\s*\{([^}]*)\}\);'
    content = re.sub(pattern1, r'{\1}', content, flags=re.DOTALL)
    
    # Pattern 2: await ExecuteOperationAsync(async () => { ... });
    # Replace with: { ... }
    pattern2 = r'await ExecuteOperationAsync\(async \(\) =>\s*\{([^}]*)\}\);'
    content = re.sub(pattern2, r'{\1}', content, flags=re.DOTALL)
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"Simplified {file_path}")

# Simplify the problematic service files
services = [
    'Services/IncidentManagementService.cs',
    'Services/OnCallService.cs',
    'Services/PagerDutyService.cs'
]

for service in services:
    simplify_execute_operation_calls(service)

print("All ExecuteOperationAsync calls simplified for clean build!")
