FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["app/CacheWorker/CacheWorker.csproj", "app/CacheWorker/"]
RUN dotnet restore "app/CacheWorker/CacheWorker.csproj"
COPY . .
WORKDIR "/src/app/CacheWorker"
RUN dotnet build "CacheWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CacheWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CacheWorker.dll"]
