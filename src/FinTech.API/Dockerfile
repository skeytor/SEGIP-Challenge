FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FinTech.API/FinTech.API.csproj", "src/FinTech.API/"]
COPY ["src/Core/FinTech.Application/FinTech.Application.csproj", "src/Core/FinTech.Application/"]
COPY ["src/Core/FinTech.Domain/FinTech.Domain.csproj", "src/Core/FinTech.Domain/"]
COPY ["src/FinTech.Persistence/FinTech.Persistence.csproj", "src/FinTech.Persistence/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
RUN dotnet restore "src/FinTech.API/FinTech.API.csproj"
COPY . .
WORKDIR "/src/src/FinTech.API"
RUN dotnet build "FinTech.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FinTech.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinTech.API.dll"]
