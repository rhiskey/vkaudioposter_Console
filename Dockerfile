# Load env + debian 10 to download photos correctly
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime

COPY . /app
WORKDIR /app
#COPY --from=build /app ./
ENTRYPOINT ["dotnet", "vkaudioposter_Console.dll"]

