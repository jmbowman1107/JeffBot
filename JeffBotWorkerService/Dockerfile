#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JeffBotWorkerService/JeffBotWorkerService.csproj", "JeffBotWorkerService/"]
RUN dotnet restore "JeffBotWorkerService/JeffBotWorkerService.csproj"
COPY . .
WORKDIR "/src/JeffBotWorkerService"
RUN dotnet build "JeffBotWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JeffBotWorkerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN apt update \
	&& apt install -y wget \
	&& apt install -y unzip \
	&& apt install -y chromium \
	&& wget https://chromedriver.storage.googleapis.com/109.0.5414.74/chromedriver_linux64.zip \
	&& unzip chromedriver_linux64.zip \
	&& mv chromedriver /usr/bin/chromedriver
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JeffBotWorkerService.dll"]