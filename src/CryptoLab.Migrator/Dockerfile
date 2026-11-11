FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/CryptoLab.Data/CryptoLab.Data.csproj src/CryptoLab.Data/
COPY src/CryptoLab.Migrator/CryptoLab.Migrator.csproj src/CryptoLab.Migrator/
RUN dotnet restore src/CryptoLab.Migrator/CryptoLab.Migrator.csproj
COPY src/CryptoLab.Data src/CryptoLab.Data
COPY src/CryptoLab.Migrator src/CryptoLab.Migrator
RUN dotnet publish src/CryptoLab.Migrator/CryptoLab.Migrator.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "CryptoLab.Migrator.dll"]