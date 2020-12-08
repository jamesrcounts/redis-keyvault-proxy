FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CacheWorker.csproj", "app/CacheWorker/"]
RUN dotnet restore "app/CacheWorker/CacheWorker.csproj"
COPY . app/CacheWorker/
WORKDIR "/src/app/CacheWorker"
RUN dotnet build "CacheWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CacheWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CacheWorker.dll"]
