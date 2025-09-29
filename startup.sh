#!/bin/bash
echo "Starting MaritimeIQ Platform..."
cd /home/site/wwwroot
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:8080
echo "Files in directory:"
ls -la
echo "Starting dotnet application..."
dotnet MaritimeIQ.Platform.dll
