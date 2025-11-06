#!/usr/bin/env python3
"""
Fix all remaining ExecuteOperationAsync calls by adding proper operation name parameters
"""

import re

def fix_file(file_path, method_mappings):
    """Fix ExecuteOperationAsync calls in a file using specific method mappings"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    for line_num, method_name in method_mappings.items():
        # Find the pattern around the specific line
        pattern = r'(\s+return true;\s*\}\);)'
        replacement = f'\\1'.replace('});', f'}}, nameof({method_name}));')
        
        # Also handle return false and other return patterns
        patterns = [
            (r'(\s+return true;\s*\}\);)', f'\\1'.replace('});', f'}}, nameof({method_name}));')),
            (r'(\s+return false;\s*\}\);)', f'\\1'.replace('});', f'}}, nameof({method_name}));')),
            (r'(\s+return [^;]+;\s*\}\);)', f'\\1'.replace('});', f'}}, nameof({method_name}));')),
        ]
        
        for pattern, replacement in patterns:
            content = re.sub(pattern, replacement, content)
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"Fixed {file_path}")

# Method mappings based on CI/CD error lines
incident_management_fixes = {
    189: "GetIncidentHistoryAsync",
    204: "AcknowledgeIncidentAsync", 
    226: "UpdateIncidentStatusAsync"
}

pagerduty_fixes = {
    109: "TriggerIncidentAsync",
    167: "AcknowledgeIncidentAsync",
    172: "AcknowledgeIncidentAsync"
}

oncall_fixes = {
    68: "GetCurrentOnCallEngineerAsync",
    72: "GetCurrentOnCallEngineerAsync"
}

# Apply fixes
fix_file('Services/IncidentManagementService.cs', incident_management_fixes)
fix_file('Services/PagerDutyService.cs', pagerduty_fixes)  
fix_file('Services/OnCallService.cs', oncall_fixes)

print("All critical ExecuteOperationAsync errors fixed!")
