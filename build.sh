#!/bin/bash
set -ev

dotnet build ./src/Marten.Testing/Marten.Testing.csproj --configuration Release
npm run test
dotnet test ./src/Marten.Testing/Marten.Testing.csproj --configuration Release 
