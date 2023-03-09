
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["JeffBotWeb/JeffBotWeb.csproj", "JeffBotWeb/"]
RUN dotnet restore "JeffBotWeb/JeffBotWeb.csproj"
COPY . .
WORKDIR "/src/JeffBotWeb"
RUN dotnet build "JeffBotWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JeffBotWeb.csproj" -c Release -o /app/publish

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
ENTRYPOINT ["dotnet", "JeffBotWeb.dll"]