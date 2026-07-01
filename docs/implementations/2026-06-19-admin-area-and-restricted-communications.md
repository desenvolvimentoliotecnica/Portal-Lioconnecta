# 2026-06-19 - Area admin e acesso restrito para comunicados

## Objetivo

Adicionar uma area administrativa autenticada para acesso a `#comunicacao/restrita`, com usuario super-admin persistido em banco PostgreSQL e publicacao protegida de comunicados oficiais.

## Entregas implementadas

- Backend de autenticacao admin na API .NET 8.
- Persistencia de usuarios admin e sessoes admin no PostgreSQL.
- Seed automatico do usuario `super-admin`.
- Protecao dos endpoints de criacao, edicao e exclusao de comunicados.
- Tela de login administrativo em `LioConnecta/admin/index.html`.
- Gate visual da rota `#comunicacao/restrita`, com redirecionamento para `/admin`.
- Envio autenticado de publicacoes com token admin.
- Script `stop-lioconnecta-full.bat` para encerramento da stack local.

## Credenciais iniciais

- Usuario: `super-admin`
- Senha: `Liotec@2026`

## Backend

### Novas entidades

- `AdminUser`
- `AdminSession`

### Novos endpoints

- `POST /api/admin/auth/login`
- `GET /api/admin/auth/session`
- `POST /api/admin/auth/logout`

### Regra de protecao

Os endpoints abaixo agora exigem sessao administrativa valida:

- `POST /api/communications`
- `PUT /api/communications/{id}`
- `DELETE /api/communications/{id}`

### Persistencia

A migration `20260619180759_AddAdminAccess` cria:

- tabela `admin_users`
- tabela `admin_sessions`

## Frontend

### Nova rota

- `/admin`

### Fluxo

1. Usuario acessa `/admin`
2. Faz login com usuario e senha
3. Sessao admin e salva em `localStorage`
4. Usuario e redirecionado para `#comunicacao/restrita`
5. Publicacao de comunicado envia token admin para a API

## Validacoes executadas

- `dotnet build src/PortalLioConnecta.Api/PortalLioConnecta.Api.csproj`
- `dotnet build tests/PortalLioConnecta.Api.Tests/PortalLioConnecta.Api.Tests.csproj`
- `dotnet vstest tests/PortalLioConnecta.Api.Tests/bin/Debug/net8.0/PortalLioConnecta.Api.Tests.dll`
- `dotnet ef database update --project src/PortalLioConnecta.Api/PortalLioConnecta.Api.csproj --startup-project src/PortalLioConnecta.Api/PortalLioConnecta.Api.csproj`

## Observacoes

- O seed do super-admin tambem ocorre na inicializacao da API, entao ambientes novos recebem o usuario automaticamente.
- A autenticacao atual usa sessao persistida em banco, adequada para o MVP local.
- Em evolucao futura, essa camada pode migrar para SSO e perfis/claims reais sem descartar a separacao atual entre area publica e area restrita.
