FROM microsoft/aspnetcore-build AS develop
WORKDIR /src
COPY *.sln ./
# Feature request: COPY glob pattern, maintain directory structure
# COPY --glob --recursive */*.*proj ./
COPY MyMvcApp/*.csproj ./MyMvcApp/
COPY MyMvcApp.Tests/*.csproj ./MyMvcApp.Tests/
RUN dotnet restore
COPY . .

FROM develop AS run
EXPOSE 80
WORKDIR /src/MyMvcApp
ENV ASPNETCORE_ENVIRONMENT Development
RUN dotnet build
ENTRYPOINT ["dotnet", "run"]

FROM develop AS test
WORKDIR /src/MyMvcApp.Tests
ENTRYPOINT ["dotnet", "test"]

FROM develop AS publish
WORKDIR /src/MyMvcApp
RUN dotnet publish -c Release -o /app

FROM microsoft/aspnetcore AS production
EXPOSE 80
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MyMvcApp.dll"]
