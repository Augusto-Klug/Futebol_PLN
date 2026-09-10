# Football Sentiment Collector

Backend .NET 10 para coletar metadados, estatisticas e comentarios publicos do YouTube de canais oficiais de Palmeiras e Internacional.

## Requisitos

- .NET 10 SDK
- PostgreSQL 16
- YouTube Data API v3 habilitada em um projeto do Google Cloud
- API Key restrita para YouTube Data API v3

## Configuracao

Use variaveis de ambiente:

```text
YouTube__ApiKey=sua-chave
ConnectionStrings__PostgreSQL=Host=localhost;Port=5432;Database=football_sentiment;Username=postgres;Password=postgres
```

Para Docker, crie um `.env` a partir de `.env.example`.

## Executar

```bash
docker compose up --build
```

Ou localmente:

```bash
dotnet restore FootballSentiment.slnx
dotnet ef database update --project src/FootballSentiment.Infrastructure/FootballSentiment.Infrastructure.csproj --startup-project src/FootballSentiment.Api/FootballSentiment.Api.csproj
dotnet run --project src/FootballSentiment.Api/FootballSentiment.Api.csproj
```

Swagger:

```text
https://localhost:{porta}/swagger
```

Health check:

```text
GET /health
```

## Endpoints principais

- `GET /api/clubs`
- `GET /api/videos`
- `GET /api/videos/{id}`
- `GET /api/videos/youtube/{youtubeVideoId}`
- `GET /api/comments`
- `POST /api/collection/video`
- `POST /api/collection/channel`
- `GET /api/statistics/comments`
- `GET /api/statistics/videos`
- `GET /api/youtube/quota`

## Observacoes

O MVP nao implementa transcricao, frontend ou analise real de sentimento. As interfaces para transcricao e sentimento existem com implementacoes nulas para permitir evolucao futura.
