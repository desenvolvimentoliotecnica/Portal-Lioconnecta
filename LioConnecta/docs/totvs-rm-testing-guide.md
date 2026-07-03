# Guia de testes — Integracao TOTVS RM

## Pre-requisitos

- [ ] Migration `AddTotvsRmExtendedConfiguration` aplicada
- [ ] RM habilitado em `#configuracoes/totvs-rm`
- [ ] Usuario SQL com GRANTs de `tools/rm-sql-grants.sql`
- [ ] Colaborador de teste com CHAPA no AD ou PFUNC

## Testes automatizados

```powershell
cd .promote-work
dotnet test tests/PortalLioConnecta.Api.Tests/PortalLioConnecta.Api.Tests.csproj
cd LioConnecta
npm test
```

Suites relevantes:

- `HrRmMapperTests` — formatacao periodo, CPF, status ferias
- `TotvsRmModuleFlagsTests` — flags e resolucao HR
- `TimesheetAggregationServiceTests` — agregacao ponto
- `ApiSmokeTests.HrWorkspaceEndpoints_ReturnRmUnavailableResponsesWhenIntegrationDisabled`

## Testes manuais por modulo

### Cadastro (`#perfil-rh/cadastro`)

- [ ] Nome, matricula, cargo, area, gestor, telefone, cidade
- [ ] `isSimulated: false` quando RM ativo
- [ ] CPF mascarado (`***.XXX.XXX-**`)

### Holerite (`#perfil-rh/holerite`)

- [ ] Lista ultimos meses com bruto/liquido
- [ ] Modal detalhe com proventos e descontos
- [ ] Impressao/PDF funcionando

### Ferias (`#perfil-rh/ferias`)

- [ ] Saldo disponivel/gozado/programado
- [ ] Historico de gozos com status legivel
- [ ] Botao solicitar permanece desabilitado (read-only)

### Beneficios (`#perfil-rh/beneficios`)

- [ ] Cards VR/VT/saude quando existirem eventos na folha
- [ ] Dependentes listados quando PFDEPEND disponivel

### Ponto (`#perfil-rh/ponto`)

- [ ] Espelho mensal com 4 batidas quando aplicavel
- [ ] Resumo horas trabalhadas/previstas/saldo
- [ ] Dias futuros ocultos

### Home / gestores

- [ ] `GET /api/hr/rh-summary` retorna saldo ferias e ultimo holerite
- [ ] `GET /api/hr/equipe` lista equipe para gestor com permissao

### Health

- [ ] `GET /health/rm` retorna `healthy` com Corpore acessivel
- [ ] Retorna `disabled` quando integracao off

## Regressao portal

- [ ] Feed, comunicados, enquetes, notificacoes inalterados
- [ ] Login LDAP funcional
- [ ] Admin TOTVS RM salva CodColigada e flags de modulo
