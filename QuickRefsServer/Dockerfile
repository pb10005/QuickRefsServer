#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./QuickRefsServer.csproj", "QuickRefsServer/"]
RUN dotnet restore "QuickRefsServer/QuickRefsServer.csproj"
COPY . ./QuickRefsServer/
WORKDIR "/src/QuickRefsServer"
RUN dotnet build "QuickRefsServer.csproj" -c Release -o /app/build

FROM build AS publish
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet publish "QuickRefsServer.csproj" -c Release -o /app/publish \
    && dotnet tool install --global dotnet-ef --version 6.0.0 \
    && dotnet ef migrations script -i -o /migration/migrationscript.sql

FROM debian:bullseye-slim AS migration
ENV PATH="$PATH:/usr/local/pgsql/bin/"
WORKDIR /migration
COPY --from=publish /migration .
RUN apt update \
    && apt install -y postgresql-client
ENTRYPOINT psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DATABASE -f ./migrationscript.sql

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickRefsServer.dll"]