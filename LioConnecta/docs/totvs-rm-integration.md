# Integracao TOTVS RM — LIOCONNECTA

Guia unificado para consulta read-only ao Corpore via SQL Server.

## Modulos integrados

| Modulo | Endpoint | Tabelas RM |
|--------|----------|------------|
| Cadastro | `GET /api/hr/cadastro` | PFUNC, PPESSOA, PSECAO, PFUNCAO |
| Holerite | `GET /api/hr/holerite`, `/api/hr/holerite/{periodo}` | PFFINANC, PEVENTO |
| Ferias | `GET /api/hr/ferias` | PFUFERIAS, PFUFERIASPER |
| Beneficios | `GET /api/hr/beneficios` | PFFINANC, PFDEPEND |
| Ponto | `GET /api/hr/ponto?month=&year=` | ABATFUN, AAFHTFUN, ANATUBAT |
| Resumo RH | `GET /api/hr/rh-summary` | Agregacao dos modulos |
| Equipe | `GET /api/hr/equipe` | PFUNC, PSECAO (gestores) |
| Health | `GET /health/rm` | Ping + contagem tabelas |

## Configuracao admin

1. Acesse `#configuracoes/totvs-rm` como super-admin
2. Informe servidor, database (Corpore), usuario/senha read-only
3. Defina **CodColigada** (padrao: 1)
4. Habilite modulos desejados (cadastro, holerite, ferias, etc.)
5. Clique **Testar conexao** — deve listar contagens de ABATFUN, AAFHTFUN, PFUNC
6. Salve e aplique migration no PostgreSQL do portal

## Resolucao de matricula (CHAPA)

Ordem de resolucao:

1. `portal_users.EmployeeId` (persistido no login)
2. LDAP / Active Directory (`employeeId`)
3. Microsoft Graph (`employeeId`, `employeeNumber`, extension attributes)
4. PFUNC por nome completo (match unico, colaborador ativo)

## Como testar localmente

```powershell
cd .promote-work
dotnet ef database update --project src/PortalLioConnecta.Api
dotnet run --project src/PortalLioConnecta.Api
```

1. Login LDAP com usuario que tenha `employeeId` ou nome igual ao PFUNC
2. Abra `#perfil-rh/cadastro` — deve retornar `isSimulated: false` com RM habilitado
3. Teste holerite, ferias, beneficios e ponto
4. Verifique `GET http://localhost:3030/health/rm`

### Cenarios de erro esperados

| Situacao | availabilityStatus | Comportamento |
|----------|-------------------|---------------|
| RM desabilitado | `rm_disabled` | Mensagem amigavel, listas vazias |
| Matricula ausente | `missing_employee_id` | Orienta contato com RH |
| SQL indisponivel | `rm_unavailable` | Retry sugerido |
| Modulo desligado no admin | `module_disabled` | Apenas aquele modulo |

## Pos-deploy (HML/PRD)

1. Rodar migration PostgreSQL
2. Configurar conexao RM em `#configuracoes/totvs-rm`
3. Validar `/health/rm` = healthy
4. Testar um colaborador real em cada modulo RH
5. Conferir GRANTs SQL com `tools/rm-sql-grants.sql`

## Scripts auxiliares

- `tools/rm-schema-discovery.sql` — validar colunas no Corpore
- `tools/rm-sql-grants.sql` — permissoes read-only

## Documentos relacionados

- `docs/totvs-rm-testing-guide.md` — checklist de testes
- `docs/totvs-rm-schema-map.md` — mapeamento tabelas/colunas
