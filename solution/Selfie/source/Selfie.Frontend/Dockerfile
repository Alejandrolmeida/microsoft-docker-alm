FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY Selfie.sln ./
COPY Selfie.Frontend/Selfie.Frontend.csproj Selfie.Frontend/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/Selfie.Frontend
RUN dotnet build Selfie.Frontend.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Selfie.Frontend.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Selfie.Frontend.dll"]
