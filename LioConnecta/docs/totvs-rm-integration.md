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
5. Clique **Testar conexao** — valida SELECT em todas as 12 tabelas (PFUNC, PFFINANC, ABATFUN, etc.)
6. Salve e aplique migration no PostgreSQL do portal

> **Usuario ja cadastrado (ex. `rm_readonly_voltage`):** se o teste de conexao retornar sucesso com todas as tabelas OK, **nao e necessario** executar `tools/rm-sql-grants.sql`. Esse script e apenas referencia para criar permissoes em um login novo.

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
2. Configurar conexao RM em `#configuracoes/totvs-rm` (usuario `rm_readonly_voltage` ou equivalente)
3. **Testar conexao** no admin — todas as tabelas devem aparecer como OK
4. Validar `/health/rm` = healthy
5. Testar um colaborador real em cada modulo RH

## Scripts auxiliares

- `tools/rm-schema-discovery.sql` — validar colunas no Corpore
- `tools/rm-validate-permissions.sql` — validar SELECT do usuario ja cadastrado
- `tools/rm-sql-grants.sql` — referencia opcional para login novo sem permissoes

## Documentos relacionados

- `docs/totvs-rm-testing-guide.md` — checklist de testes
- `docs/totvs-rm-schema-map.md` — mapeamento tabelas/colunas
