FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /project
ADD src/Generator .

RUN dotnet publish -c Release -o /dist

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime

WORKDIR /app
COPY --from=build /dist .

CMD ["dotnet", "Generator.dll"]