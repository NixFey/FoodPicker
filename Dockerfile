# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY FoodPicker.Web/*.csproj ./FoodPicker.Web/
COPY FoodPicker.Infrastructure/*.csproj ./FoodPicker.Infrastructure/
COPY FoodPicker.Migrations/*.csproj ./FoodPicker.Migrations/

RUN dotnet restore

# copy everything else and build app
COPY FoodPicker.Web/. ./FoodPicker.Web/
COPY FoodPicker.Infrastructure/. ./FoodPicker.Infrastructure/
COPY FoodPicker.Migrations/. ./FoodPicker.Migrations/
WORKDIR /source/FoodPicker.Web
RUN dotnet publish -c release -o /app 

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "FoodPicker.Web.dll"]