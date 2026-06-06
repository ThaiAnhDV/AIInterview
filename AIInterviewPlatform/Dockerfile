FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AIInterviewPlatform.sln ./
COPY AIInterviewPlatform.API/AIInterviewPlatform.API.csproj AIInterviewPlatform.API/
COPY AIInterviewPlatform.Application/AIInterviewPlatform.Application.csproj AIInterviewPlatform.Application/
COPY AIInterviewPlatform.Domain/AIInterviewPlatform.Domain.csproj AIInterviewPlatform.Domain/
COPY AIInterviewPlatform.Infrastructure/AIInterviewPlatform.Infrastructure.csproj AIInterviewPlatform.Infrastructure/

RUN dotnet restore AIInterviewPlatform.API/AIInterviewPlatform.API.csproj

COPY . .
RUN dotnet publish AIInterviewPlatform.API/AIInterviewPlatform.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .

EXPOSE 8080
CMD ["sh", "-c", "dotnet AIInterviewPlatform.API.dll --urls http://0.0.0.0:${PORT:-8080}"]
