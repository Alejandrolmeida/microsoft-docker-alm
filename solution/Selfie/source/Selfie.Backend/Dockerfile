FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY Selfie.sln ./
COPY Selfie.Backend/Selfie.Backend.csproj Selfie.Backend/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/Selfie.Backend
RUN dotnet build Selfie.Backend.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Selfie.Backend.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Selfie.Backend.dll"]
