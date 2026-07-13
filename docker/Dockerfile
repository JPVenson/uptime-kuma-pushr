# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY UptimeKuma.Pushr/UptimeKuma.Pushr.sln ./
COPY UptimeKuma.Pushr/UptimeKuma.Pushr/UptimeKuma.Pushr.csproj UptimeKuma.Pushr/
COPY UptimeKuma.Pushr/UptimeKuma.Pushr.BlazorUi/UptimeKuma.Pushr.BlazorUi.csproj UptimeKuma.Pushr.BlazorUi/
RUN dotnet restore UptimeKuma.Pushr.sln

COPY UptimeKuma.Pushr/ ./
RUN dotnet publish UptimeKuma.Pushr/UptimeKuma.Pushr.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
RUN rm -rf BlazorDebugProxy

EXPOSE 8080
ENTRYPOINT ["dotnet", "UptimeKuma.Pushr.dll"]
