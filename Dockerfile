# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj
COPY TodoApi.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build app
RUN dotnet publish -c Release -o /app/publish


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port 8080 (Render default)
EXPOSE 8080

# Start app
ENTRYPOINT ["dotnet", "TodoApi.dll"]
