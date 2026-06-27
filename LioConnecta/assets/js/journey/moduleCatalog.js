export const JOURNEY_ROUTE = "minha-jornada";

export const JOURNEY_MODULES = Object.freeze({
  tarefas: {
    key: "tarefas",
    label: "Tarefas Pendentes",
    endpointKey: "journeyTarefas"
  },
  solicitacoes: {
    key: "solicitacoes",
    label: "Solicitacoes em Andamento",
    endpointKey: "journeySolicitacoes"
  },
  trilhas: {
    key: "trilhas",
    label: "Trilhas de Aprendizagem",
    endpointKey: "journeyTrilhas"
  },
  documentos: {
    key: "documentos",
    label: "Documentos Recentes",
    endpointKey: "journeyDocumentos"
  }
});

export function isJourneyModuleSlug(slug = "") {
  return Boolean(JOURNEY_MODULES[slug]);
}

export function getJourneyModule(slug = "") {
  return JOURNEY_MODULES[slug] || null;
}
