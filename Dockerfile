FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Backend/VetGest.API/VetGest.API.csproj", "src/Backend/VetGest.API/"]
COPY ["src/Backend/VetGest.Application/VetGest.Application.csproj", "src/Backend/VetGest.Application/"]
COPY ["src/Backend/VetGest.Infrastructure/VetGest.Infrastructure.csproj", "src/Backend/VetGest.Infrastructure/"]
COPY ["src/Backend/VetGest.Domain/VetGest.Domain.csproj", "src/Backend/VetGest.Domain/"]
COPY ["src/Shared/VetGest.Contracts/VetGest.Contracts.csproj", "src/Shared/VetGest.Contracts/"]

RUN dotnet restore "src/Backend/VetGest.API/VetGest.API.csproj"

COPY . .
WORKDIR "/src/src/Backend/VetGest.API"
RUN dotnet publish "VetGest.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VetGest.API.dll", "--urls", "http://0.0.0.0:${PORT:-8080}"]
