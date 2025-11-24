# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiér projektfil og solution
COPY *.sln ./
COPY *.csproj ./

# Restore dependencies
RUN dotnet restore

# Kopiér resten af koden
COPY . ./

# Byg release
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "TodoApi.dll"]
