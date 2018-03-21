FROM microsoft/aspnetcore:1.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:1.0 AS build
WORKDIR /src
COPY dotnet-tour-of-heroes.sln ./
COPY donetangular/dotnet-tour-of-heroes.csproj donetangular/
RUN npm install
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/donetangular
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "dotnet-tour-of-heroes.dll"]
