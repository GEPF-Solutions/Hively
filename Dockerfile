FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM node:20-slim AS node-build
WORKDIR /src
COPY hively.client/package*.json hively.client/
RUN cd hively.client && npm ci

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY --from=node-build /usr/local/bin/node /usr/local/bin/node
COPY --from=node-build /usr/local/lib/node_modules /usr/local/lib/node_modules
RUN ln -sf /usr/local/bin/node /usr/local/bin/nodejs \
 && ln -sf /usr/local/lib/node_modules/npm/bin/npm-cli.js /usr/local/bin/npm

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hively.Server/Hively.Server.csproj", "Hively.Server/"]
COPY ["hively.client/hively.client.esproj", "hively.client/"]
RUN dotnet restore "./Hively.Server/Hively.Server.csproj"
COPY . .
COPY --from=node-build /src/hively.client/node_modules hively.client/node_modules
WORKDIR "/src/Hively.Server"
RUN dotnet build "./Hively.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hively.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hively.Server.dll"]
