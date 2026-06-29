# Diretriz — Registro de tarefas para o Microsoft Planner

## Objetivo

Registrar no projeto tudo o que for solicitado e entregue, em um JSON único, para visualização e cópia rápida no Planner do gestor ("Acompanhamento de Atividades").

## Arquivos

| Arquivo | Função |
|---------|--------|
| `tools/planner-tasks/tasks.json` | Fonte de verdade das tarefas |
| `tools/planner-tasks/index.html` | Tabela para copiar dados ao Planner |
| `tools/planner-tasks/DIRETRIZ.md` | Esta diretriz |

## Ciclo de vida da tarefa

| Momento | Regra |
|---------|--------|
| **Início** | Quando o Leonardo faz um pedido no chat (nova solicitação de trabalho) — registrar **data e horário** |
| **Fim** | Quando o Pull Request / promoção para **HML** (`Lioconnecta_HML`) for concluída — registrar **data e horário** do merge/deploy |
| **Em andamento** | Pedido feito, trabalho em DEV ou aguardando deploy/validação antes do HML |

Uma conversa pode gerar **uma ou mais tarefas**, se o pedido for claramente composto (ex.: "implementar X e depois subir para HML" = 1 tarefa; "ajuste A" e "ajuste B" independentes = 2 tarefas).

## Quem registra

- O **assistente (Cursor)** deve atualizar `tasks.json` ao:
  1. **Abrir** uma tarefa nova quando o usuário pedir algo executável (com `horarioInicio` do pedido)
  2. **Atualizar** percentual, bucket e datas enquanto o trabalho avança
  3. **Fechar** a tarefa (100%, bucket `Concluído`, data, `horarioConclusao` e `totalHoras`) após promoção/PR em HML

- O Leonardo valida no HTML e cola no Planner.

## Campos do JSON

```json
{
  "id": "TASK-2026-001",
  "nome": "[LIOCONNECTA] Texto curto para o Planner",
  "descricaoAmigavel": "Texto humano, 1–3 frases, sem jargão excessivo.",
  "inicio": "2026-06-23",
  "horarioInicio": "14:35",
  "conclusao": "2026-06-27",
  "horarioConclusao": "11:20",
  "totalHoras": 32,
  "bucket": "Concluído",
  "percentualConcluido": 100,
  "prioridade": "Média",
  "atribuidaA": "Leonardo Sabino Mendes",
  "pedidoOriginal": "Resumo do que o usuário pediu",
  "entregavel": "O que foi feito em linguagem de negócio",
  "ambiente": "DEV | HML | PRD",
  "referencias": {
    "commits": ["abc1234"],
    "branch": "Lioconnecta_DEV",
    "prHml": "https://github.com/.../pull/..."
  },
  "observacoes": ""
}
```

### Regras de preenchimento

**nome** (coluna *Nome da Tarefa* no Planner)
- Prefixo do produto: `[LIOCONNECTA]`, `[Portal RH]` ou `[Infra]` conforme o foco
- Frase curta, orientada ao resultado (não ao arquivo alterado)
- Exemplo bom: `[LIOCONNECTA] Visualização de holerite com impressão e PDF`
- Exemplo ruim: `Refatorar payslipExport.js e bump service-worker v49`

**descricaoAmigavel**
- Linguagem para gestor/RH: o que mudou para o usuário final ou para a operação
- 1–3 frases; pode ir em notas do Planner se houver campo livre

**inicio** / **conclusao**
- Formato `AAAA-MM-DD`
- `inicio` = data do pedido (ou primeiro dia útil se pedido virou tarefa no dia seguinte)
- `conclusao` = data em que entrou em HML (merge/push em `Lioconnecta_HML`)

**horarioInicio** / **horarioConclusao**
- Formato `HH:mm` (24 horas), fuso **America/Sao_Paulo (Brasília)**
- `horarioInicio` = horário em que o Leonardo fez o pedido no chat (momento da mensagem)
- `horarioConclusao` = horário em que a promoção para HML foi concluída (merge/push ou fim do deploy no CI, o que ocorrer por último)
- Enquanto a tarefa estiver aberta: `horarioConclusao` = `null`
- Em backfill histórico sem horário exato: usar `null` ou estimativa documentada em `observacoes` (ex.: `"Horário estimado a partir do merge"`)

**Duração (dias)**
- Exibida na tabela HTML como dias corridos entre `inicio` e `conclusao` (inclusive)
- **Sábado e domingo** entram na contagem, assim como os demais dias

**totalHoras**
- Número decimal com até 2 casas (ex.: `0,5`, `8`, `16`)
- Jornada comercial para cálculo parcial: **08:00–18:00** (Brasília)
- **Mesmo dia** com `horarioInicio` e `horarioConclusao`: diferença real em horas (máx. 8h), com **mínimo de 30 minutos (0,5 h)** quando o intervalo for menor
- **Mesmo dia** sem horários: **8**
- **Vários dias**
  - **Primeiro dia**: de `horarioInicio` até **18:00** (ex.: início `17:04` → `56min`)
  - **Último dia**: de **08:00** até `horarioConclusao`
  - **Dias intermediários**: **8h** cada
  - Horário fora da jornada comercial (antes das 08:00 ou após as 18:00) no 1º/último dia → **0h** naquele dia (não usar fallback de 8h)
  - Sem horário no extremo → **8h** naquele dia
- **Sábado e domingo** contam normalmente nos dias corridos
- O `index.html` **recalcula** a exibição e o subtotal diário; o assistente deve manter `totalHoras` no JSON alinhado

**bucket**
- `A fazer` | `Em andamento` | `Concluído` | `Bloqueado`

**percentualConcluido**
- `0` ao abrir
- `50–90` se em DEV validado mas ainda não em HML
- `100` somente após HML

**prioridade**
- `Urgente` | `Alta` | `Média` | `Baixa` — padrão **Média** salvo pedido explícito

**atribuidaA**
- Padrão: `Leonardo Sabino Mendes`

## Convenção de IDs

`TASK-AAAA-NNN` (ex.: `TASK-2026-001`), sequencial por ano no array `tarefas`.

## Fluxo recomendado

1. Leonardo pede algo no chat
2. Assistente cria entrada em `tasks.json` (`Em andamento`, `inicio` e `horarioInicio` = agora)
3. Trabalho em DEV → atualizar `percentualConcluido`, `entregavel`, `commits`
4. Promoção HML → `Concluído`, `conclusao`, `horarioConclusao` e `totalHoras` = momento do encerramento, `prHml` se existir
5. Leonardo abre `index.html`, copia linhas para o Planner

## Como abrir o HTML

Na pasta do repositório:

```powershell
cd tools\planner-tasks
python -m http.server 8765
```

Abrir: `http://localhost:8765/index.html`

A tabela agrupa as tarefas **por dia** (cada dia entre `inicio` e `conclusao` da tarefa), com cabeçalho do dia, linha em branco entre blocos e **subtotal de horas** ao final de cada dia. Use os filtros **Esta Semana**, **Mês Atual** ou **Todos** (padrão) na barra superior.

## Backfill histórico

Tarefas já concluídas devem ser reconstruídas a partir de:
- histórico de commits em `Lioconnecta_DEV` e merges em `Lioconnecta_HML`
- conversas e entregas conhecidas

### Período anterior a 23/06/2026 (backfill git)

| Marco | Conteúdo típico | Referência |
|-------|-----------------|------------|
| **10/06** | Bootstrap Portal RH, timeline Minha Jornada, showcase de testes | commits `1d7035b` … `2d5fcb7` |
| **19/06** | Protótipo LioConnecta, comunicados, admin, LDAP, login | commits `7b49d5e` … `9f671d5` |
| **22/06** | Portal DEV, deploy manager, health check, host da API | commits `16d56ca` … `d27278b` |
| **23/06 (manhã)** | CORS da API (`4809122`) — antes da doc HML e do Merge PR #1 | |

Regras do backfill pré-23/06:
- **Agrupar por entrega de negócio**, não um commit = uma tarefa
- `inicio` / `horarioInicio` = primeiro commit do grupo (`git log -1 --format=%ci`)
- `conclusao` / `horarioConclusao` = último commit do grupo (trabalho em DEV na data real)
- `prHml` = `Incluído no Merge PR #1 (23/06)` quando promovido depois
- `totalHoras` = mesma regra do HTML (jornada 08:00–18:00, mínimo 30 min no mesmo dia)
- Script reprodutível: `tools/planner-tasks/backfill-pre23.mjs`

### Período a partir de 23/06/2026

Mesmas regras, com `conclusao` preferencialmente na data do merge/promoção HML quando houver PR documentado em `referencias.prHml`.

## Checklist antes de colar no Planner

- [ ] Nome legível para gestor
- [ ] Início e conclusão coerentes com pedido e HML
- [ ] Horário de início e término preenchidos (ou `null` justificado em `observacoes`)
- [ ] `totalHoras` coerente com datas, horários e regra de 8h/dia (incluindo fim de semana)
- [ ] Duração conferida na tabela HTML
- [ ] % = 100 só se já está em HML
- [ ] Prioridade e bucket corretos
