# Mapeamento schema TOTVS RM

Validar no ambiente real com `tools/rm-schema-discovery.sql`.

| Tabela | Uso portal | Colunas principais |
|--------|-----------|-------------------|
| PFUNC | Cadastro, CHAPA, equipe | CHAPA, CODPESSOA, NOME, CODSECAO, CODFUNCAO, DATAADMISSAO, CODSITUACAO |
| PPESSOA | Dados pessoais | CPF, TELEFONE1, EMAIL, CIDADE, ESTADO, RUA |
| PSECAO | Area/departamento | CODIGO, DESCRICAO, CHAPACHEFE |
| PFUNCAO | Cargo | CODIGO, NOME |
| PFFINANC | Holerite, beneficios | CHAPA, ANOCOMP, MESCOMP, CODEVENTO, VALOR, DTPAGTO, REF |
| PEVENTO | Descricao eventos | CODIGO, DESCRICAO, PROVDESCBASE |
| PFUFERIAS | Saldo ferias | CHAPA, SALDO, DIASGOZADOS, FIMPERAQUIS |
| PFUFERIASPER | Gozos | DATAINICIO, DATAFIM, NRODIASFERIAS, SITUACAOFERIAS |
| PFDEPEND | Dependentes | NOME, GRAUPARENTESCO, INATIVO |
| ABATFUN | Batidas ponto | DATA, BATIDA, NATUREZA, STATUS |
| AAFHTFUN | Espelho ponto | DATA, HTRAB, BASE, TEMPOREF, ATRASOCALC, FALTACALC |
| ANATUBAT | Natureza batida | CODINTERNO, DESCRICAO |

## Notas

- Nomes de colunas podem variar por versao RM — ajustar queries se discovery divergir
- CodColigada configuravel no admin (padrao 1)
- Espelho: validar `AAFHTFUN` vs `AAHTFUN` no ambiente
