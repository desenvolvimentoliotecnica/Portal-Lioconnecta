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
