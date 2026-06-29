# Deploy automático da LIOCONNECTA

## Objetivo

Estruturar o deploy automático da LIOCONNECTA com GitHub Actions, separando os ambientes por branch:

- `Lioconnecta_DEV`
- `Lioconnecta_HML`
- `Lioconnecta_PRD`

O pipeline foi desenhado para:

1. restaurar e compilar a API
2. executar testes da API
3. executar testes do frontend
4. empacotar `PortalLioConnecta.Api` + frontend estático `LioConnecta`
5. publicar o artefato
6. fazer deploy remoto por SSH quando os secrets do ambiente estiverem configurados

## Workflow criado

Arquivo:

- `.github/workflows/lioconnecta-cd.yml`

## Estratégia por evento

### Push

Quando houver `push` para:

- `Lioconnecta_DEV`
- `Lioconnecta_HML`
- `Lioconnecta_PRD`

o workflow roda `build + test + package` e tenta executar o deploy.

### Pull Request

Quando houver `pull_request` com destino para essas branches, o workflow roda:

- `build`
- `test`
- `package`

Sem deploy. Isso serve como validação técnica antes da promoção.

## Estrutura do pacote gerado

O script `scripts/package-lioconnecta.ps1` gera:

- `api/`
  - saída de `dotnet publish` da `PortalLioConnecta.Api`
- `frontend/`
  - arquivos estáticos do frontend LIOCONNECTA
- `deploy-manifest.json`
  - metadados do pacote

## Environments do GitHub recomendados

Criar no repositório os environments:

- `lioconnecta-dev`
- `lioconnecta-hml`
- `lioconnecta-prd`

Cada environment deve ter seus próprios secrets.

## Secrets esperados por environment

### Obrigatórios

- `LIOCONNECTA_DEPLOY_HOST`
- `LIOCONNECTA_DEPLOY_PORT`
- `LIOCONNECTA_DEPLOY_USER`
- `LIOCONNECTA_DEPLOY_SSH_KEY`
- `LIOCONNECTA_DEPLOY_PATH`
- `LIOCONNECTA_API_SERVICE`

### Opcionais

- `LIOCONNECTA_DEPLOY_KNOWN_HOSTS`
  - recomendado para endurecer a conexão SSH
- `LIOCONNECTA_DEPLOY_POST_COMMAND`
  - comando adicional no servidor após o restart da API
  - exemplo: reload do nginx, limpeza de cache, rotação de symlink auxiliar

## Assunções do servidor remoto

O workflow atual assume:

1. servidor Linux com SSH acessível
2. usuário de deploy com permissão de escrita em `LIOCONNECTA_DEPLOY_PATH`
3. serviço systemd já existente para a API
4. `sudo systemctl restart <servico>` disponível sem prompt interativo
5. frontend servido de forma estática a partir da release atual

## Layout remoto esperado

Exemplo:

```text
/opt/lioconnecta/dev
  /releases
    /<commit-sha>
      /api
      /frontend
      deploy-manifest.json
  /current -> /opt/lioconnecta/dev/releases/<commit-sha>
```

## Próximos passos sugeridos

### 1. Infra por ambiente

Definir para cada ambiente:

- host
- usuário
- chave SSH
- path base
- nome do serviço systemd da API
- estratégia de publicação do frontend

### 2. Frontend em produção

Decidir entre:

- servir por `nginx`
- servir por `apache`
- servir por processo Node simples

Hoje o pipeline já empacota o frontend estático, então a recomendação é `nginx`.

### 3. Configuração da API

Padronizar no servidor:

- `appsettings.Production.json`
- variáveis de ambiente
- connection string do PostgreSQL
- CORS por ambiente

### 4. Hardening

Depois da primeira subida:

- trocar `StrictHostKeyChecking=no` por `known_hosts`
- reduzir permissões do usuário de deploy
- separar secrets por environment com aprovação para `PRD`

### 5. Evolução natural

Próxima evolução recomendada:

1. deploy real no `DEV`
2. validar rollback por symlink `current`
3. adicionar smoke test pós-deploy
4. replicar para `HML`
5. colocar proteção manual/aprovação para `PRD`
