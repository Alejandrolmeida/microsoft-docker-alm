FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY solution.sln ./
COPY Core2.MVC/Core2.MVC.csproj Core2.MVC/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/Core2.MVC
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Core2.MVC.dll"]
