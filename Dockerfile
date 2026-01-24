# Brug det officielle .NET 8 SDK som build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# 1. Kopier projektfilen specifikt
COPY TodoApi.csproj ./

# 2. Kør restore specifikt på projektfilen (løser MSB1011 fejlen)
RUN dotnet restore TodoApi.csproj

# 3. Kopier alt andet indhold
COPY . .

# 4. Publish appen (her bruger vi også det specifikke filnavn)
RUN dotnet publish TodoApi.csproj -c Release -o /app/publish

# Runtime image - det billede der rent faktisk kører på Render
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kopier de færdige filer fra build-stadiet
COPY --from=build /app/publish .

# Render bruger typisk port 8080 eller 10000
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Start appen
ENTRYPOINT ["dotnet", "TodoApi.dll"]