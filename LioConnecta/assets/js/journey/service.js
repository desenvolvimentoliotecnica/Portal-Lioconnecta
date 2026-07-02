import { getJson, postJson } from "../services/apiClient.js";
import { DATA_MODES, getRuntimeConfig, resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders } from "../services/portalAuthService.js";
import { getJourneyModule } from "./moduleCatalog.js";
import { getRequestType } from "./requestTypeCatalog.js";

const MOCK_REQUESTS_STORAGE_KEY = "lio.journey.requests.mock";

function readMockRequestsStore() {
  try {
    const raw = localStorage.getItem(MOCK_REQUESTS_STORAGE_KEY);
    if (!raw) {
      return [];
    }

    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

function writeMockRequestsStore(items = []) {
  localStorage.setItem(MOCK_REQUESTS_STORAGE_KEY, JSON.stringify(items.slice(0, 50)));
}

function buildMockRequestsSummary(items = []) {
  const pendingApprovalCount = items.filter((item) => {
    const status = String(item.status || "").toLowerCase();
    return status.includes("analise") || status.includes("aguard");
  }).length;

  const inProgressCount = items.filter((item) => {
    const status = String(item.status || "").toLowerCase();
    return status.includes("andamento") || status.includes("process");
  }).length;

  return {
    totalCount: items.length,
    pendingApprovalCount,
    inProgressCount
  };
}

function buildSeedMockRequests() {
  const now = Date.now();
  return [
    {
      id: "b2000001-0000-4000-8000-000000000001",
      type: "Ferias",
      description: "Solicitacao de 10 dias em julho/2026",
      openedAtUtc: new Date(now - 5 * 86400000).toISOString(),
      status: "Em analise",
      stage: "Aprovacao do gestor"
    },
    {
      id: "b2000001-0000-4000-8000-000000000002",
      type: "Reembolso",
      description: "Despesas de viagem corporativa - maio/2026",
      openedAtUtc: new Date(now - 2 * 86400000).toISOString(),
      status: "Em andamento",
      stage: "Validacao financeira"
    },
    {
      id: "b2000001-0000-4000-8000-000000000003",
      type: "Equipamento",
      description: "Troca de notebook por desgaste",
      openedAtUtc: new Date(now - 8 * 86400000).toISOString(),
      status: "Aguardando",
      stage: "Triagem de TI"
    }
  ];
}

function buildMockRequestsPayload() {
  const created = readMockRequestsStore();
  const seed = buildSeedMockRequests();
  const items = [...created, ...seed.filter((seedItem) => !created.some((item) => item.id === seedItem.id))];
  return {
    title: "Solicitacoes em Andamento",
    summary: buildMockRequestsSummary(items),
    items,
    provider: "ServiceNow",
    isSimulated: true
  };
}

function buildMockLearningCatalogPayload() {
  return {
    title: "Cursos e Materiais disponiveis",
    summary: {
      coursesCount: 5,
      materialsCount: 5,
      hoursLabel: "6h45"
    },
    courses: [
      {
        id: "b3100001-0000-4000-8000-000000000001",
        title: "Fundamentos de lideranca situacional",
        description: "Conceitos essenciais para conduzir equipes com feedback continuo e metas claras.",
        durationLabel: "1h30",
        format: "Videoaula",
        status: "Em andamento",
        trilhaTitle: "Lideranca e feedback continuo"
      },
      {
        id: "b3100001-0000-4000-8000-000000000002",
        title: "Feedback continuo na pratica",
        description: "Tecnicas para conversas de desenvolvimento e acompanhamento de desempenho.",
        durationLabel: "1h00",
        format: "Curso online",
        status: "Disponivel",
        trilhaTitle: "Lideranca e feedback continuo"
      },
      {
        id: "b3100001-0000-4000-8000-000000000003",
        title: "Boas praticas de seguranca da informacao",
        description: "Protecao de dados, senhas fortes e prevencao de phishing no dia a dia.",
        durationLabel: "1h15",
        format: "Videoaula",
        status: "Em andamento",
        trilhaTitle: "Seguranca da informacao para colaboradores"
      },
      {
        id: "b3100001-0000-4000-8000-000000000004",
        title: "LGPD para colaboradores",
        description: "Obrigacoes legais e cuidados no tratamento de dados pessoais.",
        durationLabel: "45min",
        format: "Curso online",
        status: "Disponivel",
        trilhaTitle: "Seguranca da informacao para colaboradores"
      },
      {
        id: "b3100001-0000-4000-8000-000000000005",
        title: "Comunicacao assertiva para lideres",
        description: "Como alinhar expectativas e conduzir reunioes produtivas.",
        durationLabel: "1h15",
        format: "Videoaula",
        status: "Disponivel",
        trilhaTitle: "Lideranca e feedback continuo"
      }
    ],
    materials: [
      {
        id: "b3200001-0000-4000-8000-000000000001",
        title: "Guia de feedback continuo",
        type: "PDF",
        sizeLabel: "2,4 MB",
        status: "Disponivel"
      },
      {
        id: "b3200001-0000-4000-8000-000000000002",
        title: "Checklist de reuniao 1:1",
        type: "Planilha",
        sizeLabel: "180 KB",
        status: "Disponivel"
      },
      {
        id: "b3200001-0000-4000-8000-000000000003",
        title: "Politica de seguranca da informacao",
        type: "PDF",
        sizeLabel: "1,1 MB",
        status: "Disponivel"
      },
      {
        id: "b3200001-0000-4000-8000-000000000004",
        title: "Slides - Prevencao de phishing",
        type: "Apresentacao",
        sizeLabel: "3,6 MB",
        status: "Disponivel"
      },
      {
        id: "b3200001-0000-4000-8000-000000000005",
        title: "Modelo de plano de desenvolvimento individual",
        type: "Planilha",
        sizeLabel: "240 KB",
        status: "Disponivel"
      }
    ],
    provider: "LMS",
    isSimulated: true
  };
}

export async function getLearningCatalog(options = {}) {
  const config = getRuntimeConfig();
  if (config.dataMode !== DATA_MODES.API) {
    return buildMockLearningCatalogPayload();
  }

  const endpoint = resolveApiEndpoint("journeyTrilhasCatalogo");
  return getJson(endpoint, {
    headers: getPortalAuthHeaders(),
    ...options
  });
}

export async function getJourneyModuleData(slug, options = {}) {
  const module = getJourneyModule(slug);
  if (!module) {
    throw new Error("Modulo de jornada invalido.");
  }

  const config = getRuntimeConfig();
  if (config.dataMode !== DATA_MODES.API) {
    return buildMockPayload(slug);
  }

  const endpoint = resolveApiEndpoint(module.endpointKey);
  return getJson(endpoint, {
    headers: getPortalAuthHeaders(),
    ...options
  });
}

export async function createJourneyRequest(payload, options = {}) {
  const config = getRuntimeConfig();
  if (config.dataMode !== DATA_MODES.API) {
    return createMockJourneyRequest(payload);
  }

  const endpoint = resolveApiEndpoint("journeySolicitacoes");
  return postJson(endpoint, payload, {
    headers: getPortalAuthHeaders(),
    ...options
  });
}

function createMockJourneyRequest(payload = {}) {
  const type = getRequestType(payload.typeKey);
  const item = {
    id: crypto.randomUUID(),
    type: type?.listTypeLabel || payload.typeLabel || payload.typeKey || "Solicitacao",
    description: payload.subject || payload.description || "Nova solicitacao registrada",
    openedAtUtc: new Date().toISOString(),
    status: type?.defaultStatus || "Aguardando",
    stage: type?.defaultStage || "Triagem inicial"
  };

  const created = readMockRequestsStore();
  writeMockRequestsStore([item, ...created]);
  const allItems = buildMockRequestsPayload().items;

  return {
    item,
    summary: buildMockRequestsSummary(allItems),
    provider: "ServiceNow",
    isSimulated: true
  };
}

function buildMockPayload(slug) {
  const module = getJourneyModule(slug);
  if (slug === "solicitacoes") {
    return buildMockRequestsPayload();
  }

  return {
    title: module?.label || "Minha Jornada",
    provider: "ServiceNow",
    isSimulated: true
  };
}
