export const DEFAULT_BRAND = Object.freeze({
  name: "LIOCONNECTA",
  tagline: "Capacidade e Transformação Digital"
});

export const DEFAULT_USER = Object.freeze({
  greeting: "Olá,",
  name: "Colaborador"
});

export const DEFAULT_HERO = Object.freeze({
  title: "Bem-vindo à LIOCONNECTA!",
  subtitle: "O seu ponto central de acesso e colaboração."
});

export const DEFAULT_COMPOSER = Object.freeze({
  title: "No que você está pensando?",
  placeholder: "Compartilhe uma atualização com a equipe...",
  actions: ["Adicionar fotos"]
});

export const DEFAULT_MOOD_TITLE = "Como você está se sentindo hoje?";
export const DEFAULT_FEED_TITLE = "FEED LIOCONNECTA";
export const DEFAULT_CAROUSEL_TITLE = "COMUNICAÇÃO CENTRALIZADA";

export const DEFAULT_MOOD_ITEMS = Object.freeze([
  { emoji: "😄", label: "Motivado", rank: "1º mais votado" },
  { emoji: "🙂", label: "Bem", rank: "2º mais votado" },
  { emoji: "😴", label: "Cansado", rank: "3º mais votado" }
]);

export const DEFAULT_NAV_ITEMS = Object.freeze([
  { label: "INÍCIO", active: true },
  { label: "COMUNICAÇÃO", active: false },
  { label: "PESSOAS (RH)", active: false },
  { label: "SISTEMAS", active: false },
  { label: "PROJETOS", active: false },
  { label: "RECURSOS", active: false }
]);

export const DEFAULT_SLIDES = Object.freeze([]);
export const DEFAULT_POSTS = Object.freeze([]);
export const DEFAULT_PANELS = Object.freeze([]);
export const DEFAULT_COMMUNICATION_FILTERS = Object.freeze([]);
export const DEFAULT_COMMUNICATION_ITEMS = Object.freeze([]);

export const DEFAULT_COMMUNICATIONS_CENTER = Object.freeze({
  title: "CENTRAL DE COMUNICACAO OFICIAL",
  intro: {
    eyebrow: "COMUNICACAO INSTITUCIONAL",
    title: "Todos os comunicados oficiais em um unico lugar",
    subtitle: "Acompanhe avisos corporativos, politicas e campanhas internas em um unico canal.",
    lastUpdated: "Atualizacao indisponivel"
  },
  featured: {
    slug: "",
    category: "Corporativo",
    priority: "Comunicado",
    title: "Nenhum destaque definido",
    summary: "Assim que um comunicado oficial for priorizado, ele aparecera neste bloco.",
    publishedAt: "",
    audience: "",
    owner: "",
    channel: "",
    status: "",
    attachmentLabel: "Abrir",
    image: "",
    imageAlt: "Comunicado em destaque",
    body: []
  }
});
