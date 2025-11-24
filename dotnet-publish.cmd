:: Restore, build, and publish the project
dotnet restore
dotnet publish -c Release -o %DEPLOYMENT_TARGET%

:: Remove the deployment script itself from the output
del "%DEPLOYMENT_TARGET%\dotnet-publish.cmd"

:: Final output message
echo .NET publish finished successfully.