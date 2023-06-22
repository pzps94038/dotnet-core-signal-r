FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy File
COPY ["dotnet-core-signal-r.csproj", ""]
RUN dotnet restore "dotnet-core-signal-r.csproj"
# copy everything else and build app
COPY . .

RUN dotnet build "dotnet-core-signal-r.csproj" -c Release -o /app/build
# Publish
FROM build AS publish
RUN dotnet publish "dotnet-core-signal-r.csproj" -c Release -o /app/publish
# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet dotnet-core-signal-r.dll