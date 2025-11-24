# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiér csproj og gendan afhængigheder først (for caching)
COPY *.sln .
COPY TodoApi/*.csproj ./TodoApi/
RUN dotnet restore

# Kopiér resten af koden
COPY TodoApi/. ./TodoApi/
WORKDIR /src/TodoApi

# Build app
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Exponér port
EXPOSE 8080

# Kør app
ENTRYPOINT ["dotnet", "TodoApi.dll"]
