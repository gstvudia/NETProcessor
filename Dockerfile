#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["NET.Processor.API/NET.Processor.API.csproj", "NET.Processor.API/"]
COPY ["NET.Processor.Services/NET.Processor.Core.csproj", "NET.Processor.Services/"]
RUN dotnet restore "NET.Processor.API/NET.Processor.API.csproj"
COPY . .
WORKDIR "/src/NET.Processor.API"
RUN dotnet build "NET.Processor.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NET.Processor.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NET.Processor.API.dll"]
