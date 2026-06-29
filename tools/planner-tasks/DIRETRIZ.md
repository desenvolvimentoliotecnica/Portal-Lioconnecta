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
| **Início** | Quando o Leonardo faz um pedido no chat (nova solicitação de trabalho) |
| **Fim** | Quando o Pull Request / promoção para **HML** (`Lioconnecta_HML`) for concluída |
| **Em andamento** | Pedido feito, trabalho em DEV ou aguardando deploy/validação antes do HML |

Uma conversa pode gerar **uma ou mais tarefas**, se o pedido for claramente composto (ex.: "implementar X e depois subir para HML" = 1 tarefa; "ajuste A" e "ajuste B" independentes = 2 tarefas).

## Quem registra

- O **assistente (Cursor)** deve atualizar `tasks.json` ao:
  1. **Abrir** uma tarefa nova quando o usuário pedir algo executável
  2. **Atualizar** percentual, bucket e datas enquanto o trabalho avança
  3. **Fechar** a tarefa (100%, bucket `Concluído`, data de conclusão) após promoção/PR em HML

- O Leonardo valida no HTML e cola no Planner.

## Campos do JSON

```json
{
  "id": "TASK-2026-001",
  "nome": "[LIOCONNECTA] Texto curto para o Planner",
  "descricaoAmigavel": "Texto humano, 1–3 frases, sem jargão excessivo.",
  "inicio": "2026-06-23",
  "conclusao": "2026-06-27",
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
2. Assistente cria entrada em `tasks.json` (`Em andamento`, `inicio` = hoje)
3. Trabalho em DEV → atualizar `percentualConcluido`, `entregavel`, `commits`
4. Promoção HML → `Concluído`, `conclusao` = hoje, `prHml` se existir
5. Leonardo abre `index.html`, copia linhas para o Planner

## Como abrir o HTML

Na pasta do repositório:

```powershell
cd tools\planner-tasks
python -m http.server 8765
```

Abrir: `http://localhost:8765/index.html`

Ou usar o botão **Carregar JSON** na página se abrir o arquivo direto do disco.

## Backfill histórico

Tarefas já concluídas (desde 23/06/2026) devem ser reconstruídas a partir de:
- histórico de commits / merges em `Lioconnecta_HML`
- conversas e entregas conhecidas (holerite, cadastro, rename, etc.)

Cada entrada histórica segue as mesmas regras de `nome` amigável e datas de início/fim.

## Checklist antes de colar no Planner

- [ ] Nome legível para gestor
- [ ] Início e conclusão coerentes com pedido e HML
- [ ] Duração conferida na tabela HTML
- [ ] % = 100 só se já está em HML
- [ ] Prioridade e bucket corretos
