# Integracao TOTVS RM — Ponto (MEU PERFIL RH)

Este guia descreve como configurar e validar a integracao de ponto entre o portal LIOCONNECTA e o banco SQL Server do TOTVS RM (Automação de Ponto).

## Pre-requisitos

1. **Atributo AD `employeeId`** preenchido para cada colaborador (mapeado para CHAPA no RM, com zero-padding automatico para 8 digitos).
2. **Usuario SQL read-only** com `SELECT` em:
   - `dbo.ABATFUN` (batidas brutas)
   - `dbo.ANATUBAT` (descricao das naturezas)
   - `dbo.AAFHTFUN` (espelho processado)
3. **Espelho recalculado no RM** para o periodo consultado (menu Movimento > Calcular no Automação de Ponto).
4. **CodColigada fixa:** `1` (nao configuravel no portal).
5. Acesso de **super-admin** ao portal para a area Configuracoes.
6. **Microsoft Graph** habilitado em Configuracoes (opcional, recomendado): o portal usa `employeeId` do Graph como fallback quando o LDAP nao retorna matricula. Permissao de aplicativo: `User.Read.All`.

## Configurar no portal (super-admin)

1. Faca login administrativo como **super-admin**.
2. Acesse **CONFIGURACOES** no menu ou `#configuracoes`.
3. Abra o card **TOTVS RM** ou navegue para `#configuracoes/totvs-rm`.
4. Preencha:
   - Servidor SQL
   - Porta (padrao `1433`)
   - Database (ex.: `Corpore`)
   - Usuario SQL read-only
   - Senha SQL
   - Marque **Habilitar integracao TOTVS RM**
   - Mantenha **TrustServerCertificate** habilitado em ambientes HML se necessario
5. Clique em **Salvar configuracao**.

## Testar conexao (modal Swal)

1. Na mesma tela, clique em **Testar conexao** (nao e necessario salvar antes se todos os campos estiverem preenchidos).
2. Um modal exibira:
   - **Sucesso:** mensagem de conexao + contagem de registros em `ABATFUN` e `AAFHTFUN`.
   - **Erro:** mensagem amigavel + detalhe tecnico (timeout, login failed, tabela inacessivel, etc.).
3. Corrija credenciais/firewall/permissões SQL conforme o retorno.

## Testar consulta de ponto (colaborador)

1. Faca login LDAP com usuario que possua `employeeId` no AD.
2. Acesse **MEU PERFIL RH > Ponto** (`#perfil-rh/ponto`).
3. Selecione o mes de referencia no campo **Mes de referencia**.
4. Valide:
   - Cards de resumo (horas trabalhadas, previstas, banco, faltas, atrasos)
   - Tabela diaria com entrada, saida, intervalo, trabalhado, saldo e status

## Mensagens exibidas ao colaborador

| Situacao | Mensagem |
|----------|----------|
| Integracao desabilitada | Consulta de ponto temporariamente indisponivel. Entre em contato com o RH. |
| RM offline / erro SQL | Nao foi possivel consultar o ponto agora. Tente novamente em alguns minutos. |
| Matricula ausente (`employeeId`) | Sua matricula nao esta vinculada ao perfil. Solicite ao RH a regularizacao do cadastro. |

## Troubleshooting

### Matricula ausente
- Verifique se o atributo AD `employeeId` existe e esta populado.
- Faca logout/login para atualizar `PortalUser.EmployeeId`.

### RM offline
- Teste a conexao em Configuracoes > TOTVS RM.
- Verifique firewall entre API e SQL Server.
- Confirme que a senha nao expirou.

### Tabela AAFHTFUN vazia ou colunas invalidas
- Execute recalculo do espelho no RM para o periodo.
- Valide nomes de colunas (`HTRAB`, `BASE`, `TEMPOREF`, `ATRASO`, `FALTA`, `ATRASOCALC`, `FALTACALC`) com `SELECT TOP 1 * FROM AAFHTFUN`.
- ABATFUN ainda exibe batidas mesmo sem espelho processado; resumo pode ficar parcial.

### Batidas com entrada/saida incorretas
- Confirme codigos de `NATUREZA` em `ANATUBAT` (entrada/saida).
- Ajuste mapa de naturezas em `TimesheetAggregationService` se necessario.

## Checklist HML/PRD

- [ ] Usuario SQL read-only criado
- [ ] Firewall liberado (API -> SQL Server:1433)
- [ ] Config TOTVS RM salva e teste Swal com sucesso
- [ ] `employeeId` populado no AD para usuarios piloto
- [ ] Recalculo de espelho RM no periodo de teste
- [ ] Validacao E2E em `#perfil-rh/ponto`
