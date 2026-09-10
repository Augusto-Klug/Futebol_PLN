# Projeto: Coletor de Comentários e Metadados do YouTube para Análise de Sentimento de Torcedores

## Objetivo

Desenvolver uma aplicação backend capaz de coletar dados públicos do YouTube referentes aos canais oficiais de:

- Sociedade Esportiva Palmeiras
- Sport Club Internacional

A aplicação deve utilizar a **YouTube Data API v3 oficial**.

O sistema deverá coletar:

- vídeos publicados pelos canais oficiais;
- metadados dos vídeos;
- estatísticas dos vídeos;
- comentários principais;
- respostas aos comentários;
- dados suficientes para futura análise de sentimento;
- dados suficientes para futura análise por evento, partida ou período.

A aplicação deverá armazenar os dados em banco PostgreSQL e deixar a arquitetura preparada para uma etapa posterior de PLN/IA.

O foco inicial do projeto é:

1. identificar os canais oficiais dos clubes;
2. localizar vídeos publicados pelos canais;
3. obter os metadados completos relevantes de cada vídeo;
4. coletar comentários;
5. coletar respostas;
6. persistir todos os dados;
7. impedir duplicidade;
8. permitir consultas por clube, vídeo e período;
9. gerar estatísticas básicas;
10. deixar uma camada preparada para futura análise de sentimento.

---

# Stack recomendada

Utilizar:

- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- PostgreSQL
- Npgsql
- YouTube Data API v3
- Google.Apis.YouTube.v3
- Swagger / OpenAPI
- Serilog

Opcional:

- Docker
- Docker Compose
- FluentValidation
- Polly
- Hangfire ou Quartz.NET
- xUnit
- Testcontainers

Não utilizar Python nesta primeira etapa.

C#/.NET é adequado para todo o projeto.

---

# Arquitetura

Utilizar arquitetura em camadas inspirada em Clean Architecture.

Estrutura sugerida:

```text
src/
├── FootballSentiment.Api
│   ├── Controllers
│   ├── Middlewares
│   ├── Extensions
│   ├── Program.cs
│   └── appsettings.json
│
├── FootballSentiment.Application
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   ├── UseCases
│   └── Validators
│
├── FootballSentiment.Domain
│   ├── Entities
│   ├── Enums
│   └── ValueObjects
│
└── FootballSentiment.Infrastructure
    ├── Persistence
    ├── Repositories
    ├── YouTube
    └── Migrations
```

Testes:

```text
tests/
├── FootballSentiment.UnitTests
└── FootballSentiment.IntegrationTests
```

---

# Regra arquitetural principal

A aplicação NÃO deve depender diretamente do YouTube nas camadas:

```text
Domain
Application
```

A integração concreta com a API deve existir somente em:

```text
Infrastructure
```

Criar uma abstração genérica para futuras redes sociais.

```csharp
public interface ISocialMediaSource
{
    Task<IReadOnlyCollection<SocialCommentDto>> GetCommentsAsync(
        SocialCollectionRequest request,
        CancellationToken cancellationToken);
}
```

Implementação inicial:

```text
YouTubeSocialMediaSource
```

Isso deverá permitir futuramente:

```text
XSocialMediaSource
BlueskySocialMediaSource
RedditSocialMediaSource
```

sem alterar a lógica principal da aplicação.

---

# Configuração

Adicionar:

```json
{
  "YouTube": {
    "ApiKey": "",
    "ApplicationName": "FootballSentimentCollector"
  },
  "ConnectionStrings": {
    "PostgreSQL": ""
  }
}
```

Permitir sobrescrever com:

```text
YouTube__ApiKey
ConnectionStrings__PostgreSQL
```

Nunca versionar API Key.

---

# Dependências NuGet

Adicionar conforme necessário:

```text
Google.Apis.YouTube.v3
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Design
Npgsql.EntityFrameworkCore.PostgreSQL
Swashbuckle.AspNetCore
Serilog.AspNetCore
Serilog.Sinks.Console
FluentValidation.AspNetCore
Polly
```

---

# Fontes de dados da YouTube Data API

A coleta utilizará principalmente:

```text
channels.list
playlistItems.list
videos.list
commentThreads.list
comments.list
```

Evitar utilizar:

```text
search.list
```

sempre que for possível encontrar os vídeos através da playlist de uploads do canal.

---

# Fluxo geral

```text
Clube
   ↓
Canal oficial
   ↓
channels.list
   ↓
Uploads Playlist
   ↓
playlistItems.list
   ↓
Video IDs
   ↓
videos.list
   ↓
Metadados + estatísticas
   ↓
commentThreads.list
   ↓
Comentários
   ↓
comments.list
   ↓
Replies
   ↓
PostgreSQL
```

---

# Descoberta dos vídeos

Utilizar:

```text
channels.list
```

para obter:

```text
contentDetails.relatedPlaylists.uploads
```

Essa playlist representa os vídeos enviados pelo canal.

Depois utilizar:

```text
playlistItems.list
```

para percorrer os vídeos.

Fluxo:

```text
Channel
   ↓
Uploads Playlist ID
   ↓
PlaylistItems
   ↓
Video IDs
```

Evitar fazer buscas textuais repetidas pelo canal.

---

# Metadados dos vídeos

Para cada `videoId`, realizar uma chamada utilizando:

```text
videos.list
```

Solicitar:

```text
part=snippet,statistics,contentDetails
```

Quando necessário, incluir outras partes relevantes.

O sistema deverá tentar armazenar os seguintes dados.

---

# Dados de identificação

```text
YouTubeVideoId
ChannelId
ChannelTitle
```

---

# Informações do vídeo

Coletar quando disponível:

```text
Title
Description
PublishedAt
Tags
CategoryId
DefaultLanguage
DefaultAudioLanguage
LiveBroadcastContent
```

---

# Conteúdo técnico

Coletar:

```text
Duration
Definition
Dimension
Caption
LicensedContent
Projection
```

Nem todos precisam ser obrigatórios no banco.

Campos ausentes devem ser tratados como `null`.

---

# Estatísticas

Coletar quando disponíveis:

```text
ViewCount
LikeCount
CommentCount
```

Não assumir que todas as estatísticas estarão disponíveis.

---

# Thumbnails

Coletar preferencialmente a URL da melhor thumbnail disponível.

Possíveis opções:

```text
Default
Medium
High
Standard
Maxres
```

Criar lógica semelhante:

```text
Maxres
↓
Standard
↓
High
↓
Medium
↓
Default
```

Utilizar a primeira disponível.

---

# Entidade ClubChannel

```csharp
public class ClubChannel
{
    public Guid Id { get; set; }

    public Club Club { get; set; }

    public string YouTubeChannelId { get; set; } = null!;

    public string ChannelName { get; set; } = null!;

    public string? UploadsPlaylistId { get; set; }

    public bool Active { get; set; }
}
```

Não espalhar Channel IDs pelo código.

Cadastrar através de:

- seed;
- configuração;
- ou banco.

---

# Clube

```csharp
public enum Club
{
    Palmeiras = 1,
    Internacional = 2
}
```

---

# Entidade Video

Criar aproximadamente:

```csharp
public class Video
{
    public Guid Id { get; set; }

    public Club Club { get; set; }

    public string YouTubeVideoId { get; set; } = null!;

    public string ChannelId { get; set; } = null!;

    public string ChannelTitle { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset PublishedAt { get; set; }

    public string? CategoryId { get; set; }

    public string? DefaultLanguage { get; set; }

    public string? DefaultAudioLanguage { get; set; }

    public string? LiveBroadcastContent { get; set; }

    public TimeSpan? Duration { get; set; }

    public string? Definition { get; set; }

    public string? Dimension { get; set; }

    public bool? HasCaption { get; set; }

    public bool? LicensedContent { get; set; }

    public string? Projection { get; set; }

    public string? ThumbnailUrl { get; set; }

    public long? ViewCount { get; set; }

    public long? LikeCount { get; set; }

    public long? CommentCount { get; set; }

    public DateTimeOffset CollectedAt { get; set; }

    public DateTimeOffset LastUpdatedAt { get; set; }

    public ICollection<VideoTag> Tags { get; set; }
        = new List<VideoTag>();

    public ICollection<Comment> Comments { get; set; }
        = new List<Comment>();
}
```

---

# Tags

Como um vídeo pode possuir diversas tags, evitar armazenar todas concatenadas em uma string.

Criar:

```csharp
public class VideoTag
{
    public Guid Id { get; set; }

    public Guid VideoId { get; set; }

    public Video Video { get; set; } = null!;

    public string Value { get; set; } = null!;
}
```

Criar unique constraint:

```text
VideoId + Value
```

---

# Identificador do vídeo

Criar índice único:

```text
YouTubeVideoId
```

Esse identificador externo deve continuar sendo string.

O ID interno continua sendo:

```text
Guid
```

---

# Comentários

Utilizar:

```text
commentThreads.list
```

com:

```text
part=snippet,replies
videoId={videoId}
maxResults=100
textFormat=plainText
order=time
```

Paginar com:

```text
nextPageToken
```

Fluxo:

```text
request 1
↓
até 100 comentários
↓
nextPageToken
↓
request 2
↓
...
```

---

# Replies

`commentThreads.list` pode trazer algumas respostas embutidas, mas não necessariamente todas.

Quando:

```text
snippet.totalReplyCount
```

for maior do que:

```text
replies.comments.Count
```

buscar replies adicionais utilizando:

```text
comments.list
```

com:

```text
part=snippet
parentId={topLevelCommentId}
maxResults=100
textFormat=plainText
```

Paginar também com:

```text
nextPageToken
```

---

# Entidade Comment

```csharp
public class Comment
{
    public Guid Id { get; set; }

    public Guid VideoId { get; set; }

    public Video Video { get; set; } = null!;

    public string YouTubeCommentId { get; set; } = null!;

    public string? ParentYouTubeCommentId { get; set; }

    public string Text { get; set; } = null!;

    public string? AuthorChannelId { get; set; }

    public string? AuthorDisplayName { get; set; }

    public string? AuthorProfileImageUrl { get; set; }

    public DateTimeOffset PublishedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset CollectedAt { get; set; }

    public long LikeCount { get; set; }

    public bool IsReply { get; set; }

    public SentimentType? Sentiment { get; set; }

    public decimal? SentimentScore { get; set; }
}
```

Criar índice único:

```text
YouTubeCommentId
```

---

# Transcrição e legendas

Não implementar obtenção de transcrição neste MVP.

A YouTube Data API possui recursos relacionados a legendas:

```text
captions.list
captions.download
```

Entretanto, isso NÃO deve ser tratado como uma API pública de transcrição para qualquer vídeo público.

Não assumir que será possível obter:

```text
videoId
↓
transcrição completa
```

somente com uma API Key.

Portanto:

```text
Comentários públicos
✅ coletar

Metadados públicos
✅ coletar

Estatísticas públicas
✅ coletar

Transcrição completa de qualquer vídeo
❌ não implementar no MVP
```

Manter a arquitetura preparada para, futuramente, adicionar uma fonte de transcrição separada.

---

# Arquitetura futura de transcrição

Criar apenas a interface, sem implementação real:

```csharp
public interface IVideoTranscriptProvider
{
    Task<VideoTranscript?> GetTranscriptAsync(
        string videoId,
        CancellationToken cancellationToken);
}
```

DTO:

```csharp
public record VideoTranscript(
    string VideoId,
    string Language,
    string Text);
```

Criar implementação inicial:

```text
NullVideoTranscriptProvider
```

que retorna:

```text
null
```

Isso evitará remodelar a aplicação futuramente.

---

# Uso de contexto na análise de sentimento

A futura análise de sentimento não deve considerar somente o comentário.

Preparar a arquitetura para permitir algo como:

```text
Clube
+
Título do vídeo
+
Descrição do vídeo
+
Data
+
Comentário
↓
SentimentAnalyzer
```

Exemplo:

```text
Clube:
Palmeiras

Vídeo:
"PALMEIRAS 0 X 3 INTERNACIONAL | BASTIDORES"

Comentário:
"parabéns diretoria 👏"

Resultado esperado:
Negative
```

Isso permite detectar melhor contexto e possíveis casos de ironia.

---

# Identificação futura de eventos

Preparar a entidade Video para futuramente possuir:

```text
EventType
MatchId
Opponent
Competition
MatchDate
HomeTeam
AwayTeam
HomeScore
AwayScore
```

Não implementar isso neste MVP.

A ideia futura será tentar inferir automaticamente o evento com base em:

```text
Title
Description
PublishedAt
Tags
```

Exemplo:

```text
"PALMEIRAS 2 X 0 INTERNACIONAL | MELHORES MOMENTOS"
```

poderá futuramente gerar:

```text
HomeTeam = Palmeiras
AwayTeam = Internacional
HomeScore = 2
AwayScore = 0
EventType = Match
```

---

# DTO de vídeo

Criar:

```csharp
public record YouTubeVideoDto(
    string Id,
    string ChannelId,
    string ChannelTitle,
    string Title,
    string? Description,
    DateTimeOffset PublishedAt,
    IReadOnlyCollection<string> Tags,
    string? CategoryId,
    string? DefaultLanguage,
    string? DefaultAudioLanguage,
    string? LiveBroadcastContent,
    TimeSpan? Duration,
    string? Definition,
    string? Dimension,
    bool? HasCaption,
    bool? LicensedContent,
    string? Projection,
    string? ThumbnailUrl,
    long? ViewCount,
    long? LikeCount,
    long? CommentCount);
```

---

# DTO de comentário

```csharp
public record YouTubeCommentDto(
    string Id,
    string Text,
    string? AuthorChannelId,
    string? AuthorDisplayName,
    string? AuthorProfileImageUrl,
    DateTimeOffset PublishedAt,
    DateTimeOffset? UpdatedAt,
    long LikeCount,
    bool IsReply,
    string? ParentCommentId);
```

---

# Serviço do YouTube

Criar:

```csharp
public interface IYouTubeService
{
    Task<YouTubeChannelDto?> GetChannelAsync(
        string channelId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<YouTubeVideoDto>> GetChannelVideosAsync(
        string channelId,
        int maxVideos,
        CancellationToken cancellationToken);

    Task<YouTubeVideoDto?> GetVideoAsync(
        string videoId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<YouTubeCommentDto>> GetVideoCommentsAsync(
        string videoId,
        int? maxComments,
        bool includeReplies,
        CancellationToken cancellationToken);
}
```

---

# YouTubeService

Criar uma única instância através de DI.

Exemplo conceitual:

```csharp
new YouTubeService(
    new BaseClientService.Initializer
    {
        ApiKey = options.ApiKey,
        ApplicationName = options.ApplicationName
    });
```

Não instanciar `YouTubeService` a cada request.

---

# Serviço de coleta

Criar:

```csharp
public interface ICommentCollectorService
{
    Task<CommentCollectionResult> CollectFromVideoAsync(
        string videoId,
        Club club,
        int? maxComments,
        bool includeReplies,
        CancellationToken cancellationToken);

    Task<ChannelCollectionResult> CollectFromChannelAsync(
        Club club,
        int maxVideos,
        int? maxCommentsPerVideo,
        bool includeReplies,
        CancellationToken cancellationToken);
}
```

---

# Fluxo da coleta de um vídeo

```text
1. receber videoId;

2. consultar videos.list;

3. obter metadados;

4. verificar se vídeo existe;

5. inserir ou atualizar vídeo;

6. sincronizar tags;

7. buscar comentários;

8. buscar replies quando necessário;

9. verificar YouTubeCommentId;

10. inserir novos comentários;

11. atualizar dados mutáveis;

12. retornar estatísticas.
```

---

# Atualização de métricas

Mesmo quando um vídeo já estiver no banco, atualizar:

```text
ViewCount
LikeCount
CommentCount
LastUpdatedAt
```

Esses valores mudam ao longo do tempo.

Não considerar o objeto `Video` completamente imutável.

---

# Histórico de métricas

Opcionalmente criar:

```csharp
public class VideoMetricSnapshot
{
    public Guid Id { get; set; }

    public Guid VideoId { get; set; }

    public long? ViewCount { get; set; }

    public long? LikeCount { get; set; }

    public long? CommentCount { get; set; }

    public DateTimeOffset CapturedAt { get; set; }
}
```

Isso permitirá futuramente analisar evolução:

```text
12h após publicação
↓
10.000 views

24h
↓
30.000 views

48h
↓
75.000 views
```

Essa feature pode ser opcional no MVP.

---

# Idempotência

Obrigatório.

Executar a coleta repetidamente não pode duplicar:

```text
Videos
VideoTags
Comments
```

Utilizar unique constraints.

---

# Incrementalidade

Ao buscar comentários com:

```text
order=time
```

os mais recentes aparecem primeiro.

Permitir otimização:

```text
novo
novo
novo
existente
existente
existente
```

Após uma quantidade configurável de comentários consecutivos já existentes, a aplicação pode interromper a paginação.

Exemplo:

```text
StopAfterExistingComments = 100
```

Ainda assim, a proteção real contra duplicidade será o unique constraint.

---

# Endpoints

## Clubes

```http
GET /api/clubs
```

---

# Vídeos

## Listar

```http
GET /api/videos
```

Filtros:

```text
club
from
to
title
page
pageSize
```

---

## Detalhes

```http
GET /api/videos/{id}
```

Retornar:

```text
dados do vídeo
estatísticas
tags
quantidade de comentários armazenados
```

---

## Detalhes pelo ID externo

```http
GET /api/videos/youtube/{youtubeVideoId}
```

---

# Comentários

```http
GET /api/comments
```

Filtros:

```text
club
videoId
youtubeVideoId
from
to
isReply
sentiment
page
pageSize
```

Default:

```text
page = 1
pageSize = 50
```

Máximo:

```text
pageSize = 200
```

---

# Coleta manual por vídeo

```http
POST /api/collection/video
```

Body:

```json
{
  "videoId": "abc123",
  "club": "Palmeiras",
  "maxComments": 1000,
  "includeReplies": true
}
```

Resposta aproximada:

```json
{
  "videoId": "abc123",
  "videoCreated": true,
  "videoUpdated": true,
  "receivedComments": 1000,
  "newComments": 873,
  "existingComments": 127,
  "replies": 214,
  "estimatedQuotaUnitsUsed": 18,
  "startedAt": "2026-08-27T20:00:00Z",
  "finishedAt": "2026-08-27T20:01:12Z"
}
```

---

# Coleta por canal

```http
POST /api/collection/channel
```

Body:

```json
{
  "club": "Palmeiras",
  "maxVideos": 10,
  "maxCommentsPerVideo": 2000,
  "includeReplies": true
}
```

Fluxo:

```text
Palmeiras
   ↓
canal oficial
   ↓
playlist de uploads
   ↓
10 vídeos recentes
   ↓
videos.list
   ↓
metadados
   ↓
comentários
   ↓
replies
   ↓
PostgreSQL
```

---

# Estatísticas

Criar:

```http
GET /api/statistics/comments
```

Filtros:

```text
club
from
to
```

Retornar:

```json
{
  "club": "Palmeiras",
  "totalComments": 13572,
  "topLevelComments": 10984,
  "replies": 2588,
  "videos": 12,
  "period": {
    "from": "2026-08-01",
    "to": "2026-08-31"
  }
}
```

---

# Estatísticas de vídeos

Criar:

```http
GET /api/statistics/videos
```

Retornar agregações como:

```json
{
  "club": "Palmeiras",
  "videos": 20,
  "totalViews": 8200000,
  "totalLikes": 730000,
  "totalCommentsReportedByYouTube": 95000,
  "commentsStored": 68231,
  "averageViewsPerVideo": 410000,
  "averageLikesPerVideo": 36500
}
```

Não confundir:

```text
CommentCount retornado pelo YouTube
```

com:

```text
quantidade de Comment realmente armazenada
```

Esses valores podem ser diferentes.

---

# Sentimento

Não implementar análise real inicialmente.

Criar:

```csharp
public enum SentimentType
{
    Negative = -1,
    Neutral = 0,
    Positive = 1
}
```

Interface:

```csharp
public interface ISentimentAnalyzer
{
    Task<SentimentResult> AnalyzeAsync(
        SentimentAnalysisContext context,
        CancellationToken cancellationToken);
}
```

Contexto:

```csharp
public record SentimentAnalysisContext(
    Club Club,
    string VideoTitle,
    string? VideoDescription,
    string CommentText);
```

Resultado:

```csharp
public record SentimentResult(
    SentimentType Sentiment,
    decimal Score);
```

Criar:

```text
NullSentimentAnalyzer
```

inicialmente.

---

# Métricas futuras

Preparar para futuramente obter:

```json
{
  "club": "Palmeiras",
  "totalAnalyzed": 10000,
  "positive": {
    "count": 5200,
    "percentage": 52.0
  },
  "neutral": {
    "count": 1800,
    "percentage": 18.0
  },
  "negative": {
    "count": 3000,
    "percentage": 30.0
  },
  "averageSentimentScore": 0.22
}
```

---

# Controle de quota

Criar:

```csharp
public interface IYouTubeQuotaTracker
{
    void RegisterRequest(
        string endpoint,
        int units);

    YouTubeQuotaUsage GetCurrentUsage();
}
```

Registrar estimativa local.

Criar tabela:

```text
ApiQuotaUsage

Id uuid
Date
Endpoint
RequestCount
EstimatedUnits
```

Endpoint:

```http
GET /api/youtube/quota
```

Exemplo:

```json
{
  "date": "2026-08-27",
  "estimatedUnits": 147,
  "requests": {
    "commentThreads.list": 100,
    "comments.list": 37,
    "videos.list": 10
  }
}
```

---

# Paginação

Nunca assumir:

```text
1 chamada = 100 comentários
```

O sistema solicitará:

```text
maxResults=100
```

mas poderá receber menos.

Registrar quantidade efetivamente recebida.

---

# Limites de desenvolvimento

Permitir:

```text
maxComments = 100
maxComments = 500
maxComments = 1000
maxComments = 5000
maxComments = null
```

`null` significa:

```text
percorrer todas as páginas disponíveis
```

Durante desenvolvimento, preferir limites pequenos.

---

# Tratamento de erros

Tratar:

```text
quotaExceeded
commentsDisabled
videoNotFound
channelNotFound
invalidParameter
rate limit
timeout
5xx
CancellationToken
```

`commentsDisabled` não deve derrubar a coleta inteira do canal.

Exemplo:

```json
{
  "videoId": "...",
  "status": "CommentsDisabled"
}
```

Continuar para o próximo vídeo.

---

# Retry

Aplicar retry somente para erros transitórios.

Exemplo:

```text
500 ms
1 s
2 s
```

Não repetir automaticamente:

```text
400
401
404
commentsDisabled
erros de configuração
```

---

# Logging

Utilizar logs estruturados.

Exemplo:

```text
YouTube collection started

Club=Palmeiras
VideoId=abc
MaxComments=1000
IncludeReplies=true
```

Final:

```text
YouTube collection completed

Club=Palmeiras
VideoId=abc
Views=523100
YouTubeCommentCount=4200
Received=1000
Inserted=850
Existing=150
ElapsedMs=4215
```

Nunca logar:

```text
API Key
Connection String completa
segredos
tokens
```

---

# PostgreSQL

Criar:

```text
ClubChannels
Videos
VideoTags
Comments
ApiQuotaUsage
```

Opcional:

```text
VideoMetricSnapshots
```

---

# Tipos de IDs

Usar:

```text
Guid
```

para IDs internos.

No PostgreSQL:

```text
uuid
```

Não utilizar:

```text
int
bigint
```

como PK/FK internas.

Exemplo:

```text
Videos.Id              uuid
Comments.Id            uuid
Comments.VideoId       uuid
VideoTags.Id           uuid
VideoTags.VideoId      uuid
```

IDs externos do YouTube permanecem strings:

```text
YouTubeVideoId
YouTubeCommentId
ChannelId
```

---

# Relacionamentos

```text
ClubChannel

Video
 ├── VideoTags
 ├── Comments
 │     └── Replies representados por ParentYouTubeCommentId
 │
 └── VideoMetricSnapshots
```

---

# Docker Compose

Criar:

```yaml
services:

  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: football_sentiment
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"

  api:
    build:
      context: .
    depends_on:
      - postgres
    environment:
      ConnectionStrings__PostgreSQL: "Host=postgres;Port=5432;Database=football_sentiment;Username=postgres;Password=postgres"
      YouTube__ApiKey: "${YOUTUBE_API_KEY}"
    ports:
      - "8080:8080"
```

Criar:

```text
.env.example
```

com:

```text
YOUTUBE_API_KEY=
```

Não versionar `.env`.

---

# README

Documentar:

```text
.NET 8 SDK
PostgreSQL 16
Docker opcional
Google Cloud Project
YouTube Data API v3
API Key
```

Explicar configuração:

```text
Google Cloud Console
↓
Criar projeto
↓
APIs & Services
↓
Library
↓
YouTube Data API v3
↓
Enable
↓
Credentials
↓
Create Credentials
↓
API Key
```

Recomendar restringir a chave para:

```text
YouTube Data API v3
```

---

# Swagger

Disponibilizar Swagger.

Exemplo:

```text
https://localhost:{port}/swagger
```

Documentar endpoints e DTOs.

---

# Health Check

Criar:

```http
GET /health
```

Verificar:

```text
API online
PostgreSQL online
```

Não chamar YouTube API no health check.

---

# Testes

Criar testes para:

```text
paginação
idempotência
Video upsert
Comment upsert
tags
metadados ausentes
maxComments
commentsDisabled
replies
thumbnail fallback
atualização das métricas
duplicidade
quota tracker
```

Mockar YouTube.

Testes unitários não devem consumir quota real.

---

# Critérios de aceite do MVP

O projeto estará funcional quando for possível:

1. iniciar PostgreSQL;
2. iniciar a API .NET;
3. configurar a API Key;
4. informar um vídeo real;
5. obter os metadados do vídeo;
6. armazenar título;
7. armazenar descrição;
8. armazenar publicação;
9. armazenar duração;
10. armazenar views;
11. armazenar likes;
12. armazenar quantidade pública de comentários;
13. armazenar tags;
14. armazenar thumbnail;
15. coletar pelo menos 100 comentários;
16. persistir comentários;
17. coletar múltiplas páginas;
18. opcionalmente coletar replies;
19. executar novamente sem duplicar registros;
20. atualizar métricas mutáveis do vídeo;
21. consultar vídeos pela API;
22. consultar comentários pela API;
23. visualizar estatísticas;
24. visualizar tudo pelo Swagger.

---

# Ordem de implementação

## Fase 1

Criar:

```text
Domain
Application
Infrastructure
Api
Tests
```

---

## Fase 2

Configurar:

```text
PostgreSQL
Entity Framework Core
Migrations
Swagger
Logging
DI
```

---

## Fase 3

Implementar:

```text
YouTubeService
channels.list
playlistItems.list
videos.list
```

Validar primeiro a coleta dos metadados.

---

## Fase 4

Criar:

```http
POST /api/collection/video
```

Neste momento já deve:

```text
receber videoId
↓
buscar videos.list
↓
persistir metadados
```

---

## Fase 5

Adicionar:

```text
commentThreads.list
paginação
persistência
idempotência
```

---

## Fase 6

Implementar replies utilizando:

```text
comments.list
```

---

## Fase 7

Implementar coleta por canal:

```http
POST /api/collection/channel
```

---

## Fase 8

Implementar:

```text
estatísticas
quota tracker
métricas básicas
```

---

# Não implementar agora

Não desenvolver no MVP:

```text
frontend
chatbot
LLM
BERT
Azure OpenAI
OpenAI API
classificação real de sentimento
transcrição de vídeo
download de vídeo
download de áudio
speech-to-text
autenticação de usuários
multi-tenant
billing
Kafka
RabbitMQ
Kubernetes
```

---

# Próxima etapa

Depois de validar aquisição dos dados:

```text
YouTube
   ↓
Videos
   ↓
Metadados
   ↓
Comments
   ↓
Context Builder
   ↓
Sentiment Analyzer
   ↓
Positive / Neutral / Negative
   ↓
Agregações
   ↓
Dashboard
   ↓
Chatbot
```

A futura análise deverá levar em consideração português brasileiro e linguagem típica do futebol:

```text
gírias
abreviações
xingamentos
emojis
ironia
sarcasmo
nomes de jogadores
nomes de técnicos
hashtags
contexto da partida
resultado do jogo
```

Exemplos:

```text
"jogou demais hoje"
→ Positive

"time morto, não cria nada"
→ Negative

"0x0 no intervalo"
→ Neutral

"parabéns diretoria 👏 mais uma atuação brilhante"
→ provavelmente Negative

"Abel gênio demais"
→ Positive, dependendo do contexto

"que beleza, tomamos outro gol"
→ Negative apesar da construção aparentemente positiva
```

---

# Resultado esperado do MVP

Exemplo de entrada:

```http
POST /api/collection/video
```

```json
{
  "videoId": "VIDEO_REAL",
  "club": "Palmeiras",
  "maxComments": 1000,
  "includeReplies": true
}
```

Fluxo:

```text
YouTube
   │
   ├── videos.list
   │       ↓
   │   título
   │   descrição
   │   tags
   │   duração
   │   views
   │   likes
   │   commentCount
   │   thumbnail
   │
   ├── commentThreads.list
   │       ↓
   │   comentários
   │
   └── comments.list
           ↓
       replies

           ↓

       PostgreSQL

           ↓

        REST API
```

O resultado final deve ser uma base própria formada por dados coletados diretamente da plataforma oficial, sem scraping e sem datasets previamente preparados.

Priorizar:

```text
simplicidade
legibilidade
testabilidade
idempotência
baixo consumo de quota
arquitetura desacoplada
```

Evitar abstrações desnecessárias e overengineering.