FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
WORKDIR /src
COPY . .
RUN dotnet restore && dotnet publish -c Release -o /build

FROM mcr.microsoft.com/dotnet/runtime:6.0 as runner
WORKDIR /data
VOLUME /data
COPY --from=builder /build /app
ENTRYPOINT ["dotnet", "/app/CampBot.dll"]
