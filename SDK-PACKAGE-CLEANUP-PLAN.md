# 🔧 SDK and Package Cleanup Plan

## 🚨 **Critical Package Issues Found**

### **Security Vulnerabilities**
1. **Test packages in production project** - exposes test infrastructure in production
2. **Deprecated ActiveDirectory package** - has known security issues
3. **Circular dependency risk** - Functions references main project

### **Bloat Issues**
1. **Duplicate Azure packages** across projects
2. **Missing project references** in solution file
3. **Unnecessary test dependencies** in main project

## 🎯 **Cleanup Actions Required**

### **Phase 1: Remove Security Risks**
```bash
# Remove from MaritimeIQ.Platform.csproj:
- Microsoft.NET.Test.Sdk (test framework)
- xunit packages (unit testing)
- Microsoft.AspNetCore.Mvc.Testing (integration testing)
- Microsoft.IdentityModel.Clients.ActiveDirectory (deprecated)
```

### **Phase 2: Fix Solution Structure**
```bash
# Add Functions project to solution
# Remove circular dependency
# Organize project references properly
```

### **Phase 3: Optimize Dependencies**
```bash
# Move shared packages to common location
# Update to latest secure versions
# Remove duplicate package references
```

### **Phase 4: Create Separate Test Project**
```bash
# Create proper test project structure
# Move test dependencies there
# Implement proper testing architecture
```

## 📊 **Expected Benefits**
- **Security**: Remove test packages from production
- **Performance**: Smaller deployment packages  
- **Maintainability**: Clear project separation
- **Compliance**: Remove deprecated packages

## 🚀 **Implementation Priority**
1. **CRITICAL**: Remove test packages from production (security)
2. **HIGH**: Update deprecated ActiveDirectory package  
3. **HIGH**: Fix solution structure
4. **MEDIUM**: Optimize duplicate dependencies