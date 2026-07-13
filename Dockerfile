FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore

RUN dotnet build -c Release

EXPOSE 80
ENTRYPOINT ["dotnet", "run", "--project", "Meteohost/Meteohost.csproj", "--urls=http://+:80"]