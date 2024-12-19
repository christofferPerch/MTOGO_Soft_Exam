FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MTOGO.Services.EmailAPI/MTOGO.Services.EmailAPI.csproj", "MTOGO.Services.EmailAPI/"]
RUN dotnet restore "./MTOGO.Services.EmailAPI/MTOGO.Services.EmailAPI.csproj"
COPY . .
WORKDIR "/src/MTOGO.Services.EmailAPI"
RUN dotnet build "./MTOGO.Services.EmailAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MTOGO.Services.EmailAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MTOGO.Services.EmailAPI.dll"]