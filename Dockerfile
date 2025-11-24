# -------------------------------
# 1) Build stage
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiér solution og projektfil
COPY *.sln .
COPY TodoApi/*.csproj ./TodoApi/

# Restore dependencies for solution
RUN dotnet restore TodoApi.sln

# Kopiér resten af koden
COPY . ./
WORKDIR /src/TodoApi

# Build og publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# -------------------------------
# 2) Runtime stage
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Kopiér det publicerede output fra build stage
COPY --from=build /app/publish .

# Start appen
ENTRYPOINT ["dotnet", "TodoApi.dll"]
