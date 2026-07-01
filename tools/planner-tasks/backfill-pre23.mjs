import fs from "fs";
import { execSync } from "child_process";
import path from "path";

const repo = path.resolve("..", "..");
const jsonPath = path.resolve("backfill-pre23.mjs", "..", "tasks.json");

const H = 8;
const JI = 8;
const JF = 18;
const MIN = 0.5;

function gitMeta(hash) {
  try {
    const out = execSync(`git log -1 --format=%ci ${hash}`, { cwd: repo, encoding: "utf8" }).trim();
    const m = out.match(/^(\d{4}-\d{2}-\d{2}) (\d{2}:\d{2})/);
    return m ? { date: m[1], time: m[2] } : null;
  } catch {
    return null;
  }
}

function fromCommits(commits) {
  const metas = commits.map(gitMeta).filter(Boolean);
  if (!metas.length) return null;
  return {
    inicio: metas[0].date,
    horarioInicio: metas[0].time,
    conclusao: metas[metas.length - 1].date,
    horarioConclusao: metas[metas.length - 1].time
  };
}

function parseDate(v) {
  const [y, m, d] = v.split("-").map(Number);
  return new Date(y, m - 1, d);
}

function parseTime(v) {
  if (!v) return null;
  const [h, m] = v.split(":").map(Number);
  return h + m / 60;
}

function eachDay(s, e, fn) {
  const c = new Date(s);
  while (c <= e) {
    fn(new Date(c));
    c.setDate(c.getDate() + 1);
  }
}

function countDays(s, e) {
  let n = 0;
  eachDay(s, e, () => { n += 1; });
  return n;
}

function between(ts, te) {
  if (ts == null || te == null) return null;
  return Math.round(Math.min(H, Math.max(0, te - ts)) * 100) / 100;
}

function normalizeSameDay(hours, task, dayCount) {
  const ts = parseTime(task.horarioInicio);
  const te = parseTime(task.horarioConclusao);
  if (dayCount === 1 && ts != null && te != null && hours < MIN) return MIN;
  return hours;
}

function hoursOnDay(task, i, n) {
  const ts = parseTime(task.horarioInicio);
  const te = parseTime(task.horarioConclusao);

  if (n === 1) {
    const b = between(ts, te);
    const hours = b != null ? b : H;
    return normalizeSameDay(hours, task, n);
  }
  if (i === 0) {
    if (ts == null) return H;
    const from = Math.max(ts, JI);
    if (from >= JF) return 0;
    const p = Math.round(Math.min(H, JF - from) * 100) / 100;
    return p > 0 ? p : 0;
  }
  if (i === n - 1) {
    if (te == null) return H;
    const to = Math.min(te, JF);
    if (to <= JI) return 0;
    const p = Math.round(Math.min(H, to - JI) * 100) / 100;
    return p > 0 ? p : 0;
  }
  return H;
}

function calcTotalHoras(task) {
  const a = parseDate(task.inicio);
  const b = parseDate(task.conclusao || task.inicio);
  if (!a || !b) return null;
  const n = countDays(a, b);
  let s = 0;
  for (let i = 0; i < n; i += 1) s += hoursOnDay(task, i, n);
  return Math.round(s * 100) / 100;
}

function formatDuration(hours) {
  const totalMinutes = Math.round(Number(hours) * 60);
  if (totalMinutes < 60) return `${totalMinutes}min`;
  const hh = Math.floor(totalMinutes / 60);
  const mm = totalMinutes % 60;
  return mm ? `${hh}h ${mm}min` : `${hh}h`;
}

function baseTask(partial) {
  return {
    bucket: "Concluído",
    percentualConcluido: 100,
    prioridade: "Média",
    atribuidaA: "Leonardo Sabino Mendes",
    ambiente: "HML",
    observacoes: "",
    ...partial
  };
}

const pre23Defs = [
  {
    nome: "[Portal RH] Bootstrap da solution e estrutura inicial",
    descricaoAmigavel: "Criação da solution .NET do Portal RH com projetos Web, API e base para evolução do produto.",
    pedidoOriginal: "Iniciar o projeto Portal RH com estrutura técnica mínima executável.",
    entregavel: "Solution Portal RH criada no repositório com build inicial.",
    commits: ["1d7035b"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)",
    prioridade: "Alta"
  },
  {
    nome: "[Portal RH] Protótipo visual da timeline Minha Jornada",
    descricaoAmigavel: "Iterações no layout da linha do tempo de carreira, com conectores curvos e ajustes visuais no painel de demonstração.",
    pedidoOriginal: "Evoluir o protótipo da timeline para apresentação da jornada do colaborador.",
    entregavel: "Timeline com conectores e layout refinado no showcase.",
    commits: [
      "90afc4b", "86d0a63", "d7118fb", "a07bbeb", "f684726", "60cf889",
      "32fee7e", "1256b5e", "0d7ab53", "51c8b28"
    ],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)"
  },
  {
    nome: "[Portal RH] Seção de testes e grid da timeline",
    descricaoAmigavel: "Montagem da área de testes do showcase com cards, badges e expansão do grid para validação visual.",
    pedidoOriginal: "Ampliar cenários de teste visual da timeline no ambiente de demonstração.",
    entregavel: "Grid de testes com múltiplas linhas e badges no showcase.",
    commits: [
      "9528277", "068cb3e", "8b04c94", "172f797", "dd5a487",
      "3398f0a", "eb4529b", "2d5fcb7"
    ],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)"
  },
  {
    nome: "[LIOCONNECTA] Protótipo do portal e backend de comunicados",
    descricaoAmigavel: "Primeira versão navegável do LioConnecta com API e listagem de comunicados corporativos.",
    pedidoOriginal: "Subir protótipo do portal com módulo de comunicados.",
    entregavel: "Frontend LioConnecta e backend de comunicados integrados.",
    commits: ["7b49d5e", "328239d"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)",
    prioridade: "Alta"
  },
  {
    nome: "[LIOCONNECTA] Área administrativa e configuração LDAP",
    descricaoAmigavel: "Painel admin para parametrização do LDAP e validação local do fluxo de configuração.",
    pedidoOriginal: "Criar área admin com cadastro de LDAP.",
    entregavel: "Tela admin e fluxo de configuração LDAP operacional em DEV.",
    commits: ["d27d365", "8826b1b"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)"
  },
  {
    nome: "[LIOCONNECTA] Login do colaborador via LDAP",
    descricaoAmigavel: "Autenticação do colaborador no portal usando credenciais do diretório corporativo.",
    pedidoOriginal: "Habilitar login LDAP no portal.",
    entregavel: "Endpoint e experiência de login LDAP publicados em DEV.",
    commits: ["6d4ff7a", "9f671d5"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)",
    prioridade: "Alta"
  },
  {
    nome: "[LIOCONNECTA] Evolução do portal DEV com enquetes",
    descricaoAmigavel: "Consolidação das funcionalidades de enquetes e ajustes gerais do portal em ambiente de desenvolvimento.",
    pedidoOriginal: "Avançar portal DEV com LDAP, admin e enquetes.",
    entregavel: "Portal DEV com enquetes e integrações LDAP estáveis.",
    commits: ["16d56ca"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)"
  },
  {
    nome: "[Infra] Gestor de deploy e padronização de portas DEV",
    descricaoAmigavel: "Ferramenta de gestão de deploy do LioConnecta e definição das portas padrão do ambiente de desenvolvimento.",
    pedidoOriginal: "Padronizar deploy e portas DEV/HML do portal.",
    entregavel: "Deploy manager e portas 3020/3030 documentadas e automatizadas.",
    commits: ["ddda902"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)",
    prioridade: "Alta"
  },
  {
    nome: "[Infra] Correções do deployer, health check e host da API",
    descricaoAmigavel: "Ajustes no script de publicação com health check resiliente, restart via sudo e resolução do host da API no frontend publicado.",
    pedidoOriginal: "Corrigir deployer e conectividade API após publicação.",
    entregavel: "Deployer estável com health check e frontend apontando para API correta.",
    commits: ["37f8941", "dc91d54", "bab7901", "48feea4", "d27278b"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)",
    prioridade: "Alta"
  },
  {
    nome: "[LIOCONNECTA] CORS da API para origens do frontend publicado",
    descricaoAmigavel: "Liberação das origens do frontend implantado no CORS da API para permitir acesso em DEV/HML.",
    pedidoOriginal: "Corrigir bloqueio CORS após deploy do frontend.",
    entregavel: "API aceitando origens do portal publicado.",
    commits: ["4809122"],
    branch: "Lioconnecta_DEV",
    prHml: "Incluído no Merge PR #1 (23/06)"
  }
];

function buildPre23Tasks() {
  return pre23Defs.map((def, index) => {
    const dates = fromCommits(def.commits);
    if (!dates) throw new Error(`Sem meta git: ${def.nome}`);
    const task = baseTask({
      id: `TASK-2026-${String(index + 1).padStart(3, "0")}`,
      nome: def.nome,
      descricaoAmigavel: def.descricaoAmigavel,
      pedidoOriginal: def.pedidoOriginal,
      entregavel: def.entregavel,
      inicio: dates.inicio,
      horarioInicio: dates.horarioInicio,
      conclusao: dates.conclusao,
      horarioConclusao: dates.horarioConclusao,
      prioridade: def.prioridade || "Média",
      referencias: {
        commits: def.commits,
        branch: def.branch,
        prHml: def.prHml
      },
      observacoes: "Backfill a partir de commits git (anterior a 23/06)."
    });
    task.totalHoras = calcTotalHoras(task);
    return task;
  });
}

function renumberId(oldId, offset) {
  const m = oldId.match(/^TASK-(\d{4})-(\d+)$/);
  if (!m) return oldId;
  const n = Number(m[2]) + offset;
  return `TASK-${m[1]}-${String(n).padStart(3, "0")}`;
}

const data = JSON.parse(fs.readFileSync(jsonPath, "utf8"));
const offset = pre23Defs.length;
const existing = data.tarefas.map((t) => {
  const copy = { ...t, id: renumberId(t.id, offset) };
  copy.referencias = { ...t.referencias };
  copy.totalHoras = calcTotalHoras(copy);
  return copy;
});

const pre23 = buildPre23Tasks();
data.tarefas = [...pre23, ...existing];
data.meta.atualizadoEm = "2026-06-29";
data.meta.backfillPre23Em = "2026-06-29";
data.meta.totalTarefas = data.tarefas.length;

const issues = [];
for (const t of data.tarefas) {
  const calc = calcTotalHoras(t);
  if (calc !== t.totalHoras) {
    issues.push(`${t.id}: totalHoras JSON=${t.totalHoras} calc=${calc}`);
    t.totalHoras = calc;
  }
  if (!t.horarioInicio || !t.horarioConclusao) {
    issues.push(`${t.id}: horario ausente`);
  }
}

fs.writeFileSync(jsonPath, `${JSON.stringify(data, null, 2)}\n`);

console.log(`Tarefas pre-23/06: ${pre23.length}`);
console.log(`Total tarefas: ${data.tarefas.length}`);
console.log(`Horas pre-23: ${pre23.reduce((s, t) => s + t.totalHoras, 0).toFixed(2)}`);
console.log(`Horas todas: ${data.tarefas.reduce((s, t) => s + t.totalHoras, 0).toFixed(2)}`);
console.log("\nPre-23 resumo:");
for (const t of pre23) {
  console.log(`  ${t.id} ${t.inicio} ${t.horarioInicio}-${t.conclusao} ${t.horarioConclusao} => ${formatDuration(t.totalHoras)} (${t.totalHoras}h)`);
}
if (issues.length) {
  console.log("\nAjustes aplicados:", issues.length);
  issues.forEach((i) => console.log(" ", i));
} else {
  console.log("\nValidacao: todos os totalHoras conferem.");
}
