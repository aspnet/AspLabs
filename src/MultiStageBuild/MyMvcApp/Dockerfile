FROM microsoft/aspnetcore-build AS base
EXPOSE 80
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .

FROM base AS develop
ENV ASPNETCORE_ENVIRONMENT Development
RUN dotnet build
ENTRYPOINT ["dotnet", "run"]

FROM base AS publish
RUN dotnet publish -c Release -o /app

FROM microsoft/aspnetcore AS production
EXPOSE 80
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MyMvcApp.dll"]
