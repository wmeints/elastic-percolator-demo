FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /project
ADD src/Enricher ./src/Enricher
ADD src/Messaging ./src/Messaging

RUN dotnet publish src/Enricher/Enricher.csproj -c Release -o /dist

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime

WORKDIR /app
COPY --from=build /dist .

CMD ["dotnet", "Enricher.dll"]