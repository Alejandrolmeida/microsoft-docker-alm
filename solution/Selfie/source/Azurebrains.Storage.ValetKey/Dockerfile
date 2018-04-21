FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY *.sln ./
COPY Azurebrains.Storage.ValetKey/Azurebrains.Storage.ValetKey.csproj Azurebrains.Storage.ValetKey/
RUN dotnet restore
COPY . .
WORKDIR /src/Azurebrains.Storage.ValetKey
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN apt-get update && apt-get install -y imagemagick && apt-get upgrade -y imagemagick 
ENTRYPOINT ["dotnet", "Azurebrains.Storage.ValetKey.dll"]
