FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["agent/src/Lap.Agent/Lap.Agent.csproj", "agent/src/Lap.Agent/"]
RUN dotnet restore "agent/src/Lap.Agent/Lap.Agent.csproj"
COPY . .
WORKDIR "/src/agent/src/Lap.Agent"
RUN dotnet build "./Lap.Agent.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lap.Agent.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lap.Agent.dll"]
