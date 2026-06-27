# LIOCONNECTA - Checklist de Portal Dinamico

Objetivo: remover mocks remanescentes e deixar o portal alimentado por dados persistidos/API, mantendo modo mock apenas onde ainda nao existe backend.

## Modulos ja dinamicos

- [x] Comunicados oficiais persistidos em PostgreSQL
- [x] Area editorial de comunicados
- [x] Leitura dedicada de comunicados
- [x] Enquetes persistidas em PostgreSQL
- [x] Upload de imagem/anexo de enquetes
- [x] Administracao de usuarios autenticados
- [x] LDAP/AD para login corporativo

## Proximos modulos

- [x] Notificacoes
  - [x] Criar modelo/tabelas persistidas
  - [x] Gerar notificacoes a partir de comunicados e enquetes reais
  - [x] Expor API para listagem e resumo
  - [x] Registrar leitura por usuario
  - [x] Atualizar topbar e painel lateral sem JSON mockado
  - [x] Cobrir com testes automatizados
- [x] Agenda / compromissos
  - [x] Definir contrato de agenda
  - [x] Persistir compromissos
  - [ ] Preparar futura integracao Microsoft Graph/Teams
- [ ] Acessos rapidos
  - [ ] Persistir catalogo de links
  - [ ] Permitir ordenacao por ambiente/perfil
  - [ ] Administrar via area restrita
- [ ] Paineis laterais
  - [ ] Substituir `panels.json` por endpoint home/widgets
  - [ ] Permitir widgets por perfil
- [ ] Feed social interno
  - [ ] Persistir posts
  - [ ] Persistir comentarios e reacoes
  - [ ] Moderacao administrativa
- [ ] Home agregada
  - [ ] Endpoint unico `GET /api/home`
  - [ ] Compor usuario, notificacoes, agenda, comunicados, enquetes e atalhos

## Regra de execucao

- [ ] Todo modulo novo deve ter contrato de API documentado
- [ ] Todo modulo novo deve ter estado vazio amigavel
- [ ] Todo modulo novo deve evitar fallback mockado quando o backend ja existir
- [ ] Todo modulo novo deve atualizar a badge de versao visual
- [ ] Todo modulo novo deve ter testes automatizados antes de promover ambiente
