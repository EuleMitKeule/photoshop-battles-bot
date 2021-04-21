#FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
#WORKDIR /src
#COPY ["src/PhotoBot/PhotoBot.csproj", "PhotoBot/"]
#RUN dotnet restore "PhotoBot/PhotoBot.csproj"
#COPY . .
#WORKDIR "/src/PhotoBot"
#RUN dotnet publish "PhotoBot.csproj" -c Release -o /app
#
#FROM build AS publish
#RUN dotnet publish "PhotoBot.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#RUN ls -la
#ENTRYPOINT ["dotnet", "PhotoBot.dll"]

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

COPY ./src/PhotoBot/PhotoBot.csproj .
RUN dotnet restore

COPY ./src/ .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build /app .
RUN ls -la
ENTRYPOINT ["dotnet", "PhotoBot.dll"]