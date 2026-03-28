FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TaskFlow.Api.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish .

ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/taskflow.db"
ENV ASPNETCORE_URLS="http://+:5000"
ENV ASPNETCORE_ENVIRONMENT="Production"

EXPOSE 5000

ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
