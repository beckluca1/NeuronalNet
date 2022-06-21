FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy as build
WORKDIR /src
COPY NeuronalNetServer/NeuronalNetServer.csproj /src/
RUN dotnet restore

FROM build as publish
WORKDIR /src
COPY . .
RUN dotnet publish NeuronalNetServer/NeuronalNetServer.csproj -c Release -o bin/Release/net

FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy AS runtime
EXPOSE 7613
WORKDIR /src
COPY --from=publish /src/bin/Release/net /src/bin/Release/net
ENTRYPOINT ["./bin/Release/net/NeuronalNetServer"]