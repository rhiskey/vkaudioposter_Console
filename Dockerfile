#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["vkaudioposter_Console.csproj", "."]
COPY ["vkaudioposter-ef/vkaudioposter-ef/vkaudioposter-ef.csproj", "vkaudioposter-ef/vkaudioposter-ef/"]
RUN dotnet restore "./vkaudioposter_Console.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "vkaudioposter_Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "vkaudioposter_Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vkaudioposter_Console.dll"]