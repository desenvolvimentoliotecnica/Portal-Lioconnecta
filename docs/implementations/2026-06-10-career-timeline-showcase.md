# Career timeline showcase - 2026-06-10

## O que foi criado
- `CareerTimelineViewComponent` como componente mocado e reaproveitavel
- View de showcase em `Home/CareerTimelineShowcase`
- Layout visual inspirado na timeline enviada pelo time

## Estrutura adicionada
- Modelos de apoio em `src/PortalLioConnecta.Web/Models/CareerTimeline`
- Partial de icones SVG para manter o componente independente de bibliotecas extras
- Estilos especificos em `wwwroot/css/site.css`

## Cobertura de teste
- Smoke test da pagina de showcase em `tests/PortalLioConnecta.Web.Tests/CareerTimelineShowcaseTests.cs`

## Observacao
- Os dados estao mockados de proposito para servir como base visual e de integracao futura com o TOTVS RM.

## Ajuste visual em 2026-06-10
- KPIs posicionados na mesma linha do titulo, alinhados a direita em telas largas
- Titulo reduzido para ficar mais proximo da referencia
- Altura dos cards do historico diminuida para melhorar a densidade visual

## Ajuste funcional em 2026-06-10
- Adicionados chips com a quantidade de dias entre uma movimentacao e outra
- Datas passaram a ser tratadas como valores reais para calculo automatico dos intervalos
- O chip e renderizado no pipe entre os cards da mesma linha

## Ajuste visual em 2026-06-10
- Conector curvo adicionado entre as linhas da timeline para dar continuidade visual ao pipe
- Conector recalibrado para sair do fim direito da primeira linha e entrar no inicio esquerdo da segunda
- Conector ajustado para usar trechos retos com curvas de 90 graus entre as linhas
- Badges numericas mantidas acima do tracado para preservar a leitura da sequencia

## Ajuste visual em 2026-06-10
- Secao `Testes` adicionada ao showcase com 30 divisoes iguais em linha unica
- Divisoes renderizadas sem texto, sem gaps, com 15px de altura e borda tracejada cinza
- Segunda linha adicionada a secao `Testes` com 6 divisoes iguais, colada a primeira linha
- Cards vazios adicionados dentro das divisoes da segunda linha, com 200px de altura, 90% de largura e cantos arredondados
- Badge numerica adicionada na terceira divisao da primeira linha da secao `Testes`
- Badges numericas adicionadas na oitava e decima terceira divisoes da primeira linha da secao `Testes`
- Badges numericas adicionadas na decima oitava, vigesima terceira e vigesima oitava divisoes da primeira linha da secao `Testes`
- Duas novas linhas com 30 divisoes cada adicionadas abaixo da linha com 6 divisoes
- Cada um dos 6 cards da segunda linha recebeu um icone `$` dentro de um circulo, centralizado no topo
- Celulas mescladas da primeira linha de testes passaram a exibir um chip `xxx dias` centralizado
- Toggle adicionado na showcase para habilitar ou desabilitar o tracejado de todas as divisoes da secao `Testes`
- Os 12 cards da secao `Testes` passaram a exibir datas distintas e ordenadas abaixo do icone
- Os 12 cards da secao `Testes` passaram a exibir tipo de movimentacao e um chip com valor monetario
- Os icones dos 12 cards da secao `Testes` passaram a refletir o tipo de movimentacao, reutilizando o mesmo mapeamento visual da timeline principal
- Secao `Testes 2` adicionada como experimento visual, com 8 cards distribuidos em 2 linhas (6 na primeira e 2 na segunda)
- Showcase adicional com `Ant Design Blazor` criado em modo hibrido dentro da aplicacao MVC, usando service mockada, controller MVC e componente Blazor hospedado na view
- Showcase `Ant Design Blazor` ampliado com hero section, cards de resumo, etapas, progressos, alertas, timeline, estados vazios, avatares, tags e blocos operacionais
- Estrutura de estilos do showcase Ant Design consolidada em `wwwroot/css/site.css` para suportar uma demonstracao mais completa e mais proxima de um painel real de RH
