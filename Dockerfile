FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine@sha256:33be1326b4a2602d08e145cf7e4a8db4b243db3cac3bdec42e91aef930656080 AS build
WORKDIR /app
COPY src/ .
RUN dotnet restore PopcornBytes.Api/PopcornBytes.Api.csproj
RUN dotnet publish PopcornBytes.Api/PopcornBytes.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine@sha256:3fce6771d84422e2396c77267865df61174a3e503c049f1fe242224c012fde65
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5000
ENTRYPOINT ["dotnet", "PopcornBytes.Api.dll"]
