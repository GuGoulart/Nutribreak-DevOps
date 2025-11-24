# NutriBreak API

Plataforma para promover saúde física e mental no trabalho recomendando pausas inteligentes e cardápios equilibrados.

## Objetivo
Apoiar profissionais (ODS 3 e 8) com registros de usuários, refeições e pausas, além de recomendações (v2) baseadas em humor, nível de energia e tempo de tela.

## Tecnologias
- .NET 8 / ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- API Versioning (v1, v2)
- OpenTelemetry (tracing + metrics console exporter)
- Health Checks (/health)
- Swagger com versionamento
- HATEOAS + Paginação
- Testes (xUnit) – exemplo inicial

## Estrutura de Versões
- v1: CRUD completo de `users`, `meals`, `break-records`.
- v2: Inclui endpoint de recomendações por usuário: `/api/v2/users/{id}/recommendations`.

## Endpoints Principais (v1)
GET /api/v1/users?pageNumber=1&pageSize=10
POST /api/v1/users
GET /api/v1/users/{id}
PUT /api/v1/users/{id}
DELETE /api/v1/users/{id}

GET /api/v1/meals
POST /api/v1/meals
GET /api/v1/meals/{id}
PUT /api/v1/meals/{id}
DELETE /api/v1/meals/{id}

GET /api/v1/break-records
POST /api/v1/break-records
GET /api/v1/break-records/{id}
PUT /api/v1/break-records/{id}
DELETE /api/v1/break-records/{id}

## Endpoint Novo (v2)
GET /api/v2/users/{id}/recommendations
Retorna sugestões de: próxima pausa, intervalo recomendado e tipo de refeição sugerida.

## Paginação & HATEOAS
Listagens retornam: total, pageNumber, pageSize, items e links (self, next, prev, create). Recursos possuem links (self, update, delete).

## Status Codes
- 200 OK (consultas)
- 201 Created (criação)
- 204 NoContent (atualizações/remoções)
- 400 BadRequest (dados inválidos)
- 404 NotFound (não encontrado)
- 409 Conflict (duplicidade de e-mail)

## Health Check
GET /health
Retorna estado da conexão SQL Server.

## Observabilidade
OpenTelemetry configurado com instrumentação ASP.NET Core e HTTP + exportador console. Ajuste para Jaeger/OTLP em produção.

## Migrations & Banco
Configurar `ConnectionStrings:DefaultConnection` em `appsettings.json`.

Criar base:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```
(Gerar novamente se modelos mudarem.)

## Testes
Projeto de testes usa EF InMemory + xUnit. Expandir para incluir Meals e BreakRecords.
Executar:
```
dotnet test
```

## Licença
Uso acadêmico / demonstração.
