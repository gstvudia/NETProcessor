FROM mcr.microsoft.com/dotnet/core/sdk:3 AS build-env
WORKDIR /app

# Do not expose any port as heroku does not allow it 
# for security reasons 
#EXPOSE 80

# Copy everything else and build
COPY . .
#Publish
RUN dotnet publish NETProcessor.sln -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3
WORKDIR /app
COPY --from=build-env /app/NETProcessor/out .
ENTRYPOINT ["dotnet", "NETProcessor.dll"]
