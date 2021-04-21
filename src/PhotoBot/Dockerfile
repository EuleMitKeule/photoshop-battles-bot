FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PhotoBot/PhotoBot.csproj", "PhotoBot/"]
RUN dotnet restore "PhotoBot/PhotoBot.csproj"
COPY . .
WORKDIR "/src/PhotoBot"
RUN dotnet publish "PhotoBot.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PhotoBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN ls -la
ENTRYPOINT ["dotnet", "PhotoBot.dll"]
