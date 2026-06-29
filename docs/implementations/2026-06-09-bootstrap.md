# Bootstrap da solução - 2026-06-09

## O que foi criado
- Solution `PortalLioConnecta.sln`
- Projeto API `src/PortalLioConnecta.Api` em ASP.NET Core 8
- Projeto MVC `src/PortalLioConnecta.Web` em ASP.NET Core 8
- Projetos de teste:
  - `tests/PortalLioConnecta.Api.Tests`
  - `tests/PortalLioConnecta.Web.Tests`

## Base técnica adicionada
- PostgreSQL como banco alvo
- EF Core preparado para migrations
- MediatR preparado na API
- Estrutura inicial de pastas para futura organização:
  - API: `Controllers`, `Interfaces`, `Models`, `Services`, `MediatR`, `Data`
  - Web: `Controllers`, `Views`, `Components`, `Services`, `Models`, `ViewComponents`

## Smoke tests
- API: `GET /api/health`
- Web: `GET /`

## Próximos passos sugeridos
- Criar a primeira entidade de domínio do Portal RH
- Implementar o `DbContext` com `DbSet`s reais
- Configurar a primeira migration
- Definir autenticação, autorização e integração com TOTVS RM
