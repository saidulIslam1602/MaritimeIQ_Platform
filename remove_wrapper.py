#!/usr/bin/env python3
"""
Remove ExecuteOperationAsync wrapper completely to get clean build
"""

import re

def remove_execute_operation_wrapper(file_path):
    """Remove ExecuteOperationAsync wrapper completely"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Pattern 1: return await ExecuteOperationAsync(async () => { CONTENT });
    # Replace with: CONTENT
    pattern1 = r'return await ExecuteOperationAsync\(async \(\) =>\s*\{(.*?)\}\);'
    
    def replace_return_wrapper(match):
        inner_content = match.group(1).strip()
        # Remove any leading/trailing whitespace and ensure proper indentation
        lines = inner_content.split('\n')
        cleaned_lines = []
        for line in lines:
            if line.strip():
                cleaned_lines.append(line)
        return '\n'.join(cleaned_lines)
    
    content = re.sub(pattern1, replace_return_wrapper, content, flags=re.DOTALL)
    
    # Pattern 2: await ExecuteOperationAsync(async () => { CONTENT });
    # Replace with: CONTENT
    pattern2 = r'await ExecuteOperationAsync\(async \(\) =>\s*\{(.*?)\}\);'
    
    def replace_await_wrapper(match):
        inner_content = match.group(1).strip()
        lines = inner_content.split('\n')
        cleaned_lines = []
        for line in lines:
            if line.strip():
                cleaned_lines.append(line)
        return '\n'.join(cleaned_lines)
    
    content = re.sub(pattern2, replace_await_wrapper, content, flags=re.DOTALL)
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"Removed ExecuteOperationAsync wrapper from {file_path}")

# Remove wrapper from problematic service files
services = [
    'Services/IncidentManagementService.cs',
    'Services/OnCallService.cs',
    'Services/PagerDutyService.cs'
]

for service in services:
    remove_execute_operation_wrapper(service)

print("All ExecuteOperationAsync wrappers removed - clean build ready!")
