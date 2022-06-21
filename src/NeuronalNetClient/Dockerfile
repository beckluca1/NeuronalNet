FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build
WORKDIR /src
COPY NeuronalNetClient/NeuronalNetClient.csproj /src/
RUN dotnet restore

FROM build AS publish
WORKDIR /src
COPY . .
RUN dotnet publish NeuronalNetClient/NeuronalNetClient.csproj -c Release -o bin/Release/net

FROM nginx AS runner
WORKDIR /var/www/web
COPY --from=publish /src/bin/Release/net/wwwroot .
COPY NeuronalNetServer/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
EXPOSE 443