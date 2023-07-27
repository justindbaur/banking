FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy everything
COPY ./src /app/src
COPY ./plugins /app/plugins
COPY ./Directory.Build.props /app/Directory.Build.props
COPY ./Directory.Build.targets /app/Directory.Build.targets
# Build and publish a release
RUN dotnet publish /app/src/Api -c Release -o ./out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 80
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Api.dll"]
