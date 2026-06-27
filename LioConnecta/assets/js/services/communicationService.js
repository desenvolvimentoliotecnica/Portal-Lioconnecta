import { deleteJson, getJson, postJson, putJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders, getStoredPortalSession } from "./portalAuthService.js";

const AVAILABLE_CATEGORIES = Object.freeze([
  "RH",
  "Corporativo",
  "Tecnologia",
  "Politicas",
  "Eventos"
]);

const KPI_TEMPLATE = Object.freeze([
  { label: "Comunicados publicados", detail: "Registros persistidos", tone: "brand" },
  { label: "Alta prioridade", detail: "Acompanhamento diário", tone: "danger" },
  { label: "Politicas ativas", detail: "Versões vigentes", tone: "info" },
  { label: "Campanhas em andamento", detail: "Com engajamento aberto", tone: "success" }
]);

function formatDate(value) {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return date.toLocaleDateString("pt-BR", { timeZone: "UTC" });
}

function formatDateTime(value) {
  if (!value) {
    return "Aguardando publicações persistidas";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "Aguardando publicações persistidas";
  }

  return date.toLocaleString("pt-BR", {
    timeZone: "UTC",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

function normalizeText(value, fallback = "") {
  const text = String(value ?? "").trim();
  return text || fallback;
}

function normalizeParagraphs(body) {
  return String(body || "")
    .split(/\n\s*\n|\n/)
    .map((paragraph) => paragraph.trim())
    .filter(Boolean);
}

function normalizeKey(value = "") {
  return String(value)
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}

function sortByPublishedAtDesc(items = []) {
  return [...items].sort((left, right) => {
    const leftTime = new Date(left?.publishedAt || 0).getTime();
    const rightTime = new Date(right?.publishedAt || 0).getTime();
    return rightTime - leftTime;
  });
}

function mapCommunicationItem(item = {}) {
  const bodyText = typeof item.body === "string" ? item.body : "";
  const publishedAtRaw = normalizeText(item.publishedAt);

  return {
    id: normalizeText(item.id),
    slug: normalizeText(item.slug),
    category: normalizeText(item.category, "Corporativo"),
    priority: normalizeText(item.priority, "Comunicado"),
    title: normalizeText(item.title, "Comunicado oficial"),
    summary: normalizeText(item.summary, "Resumo não informado."),
    publishedAt: formatDate(item.publishedAt),
    audience: normalizeText(item.audience, "Toda a companhia"),
    channel: normalizeText(item.channel, "Portal"),
    status: normalizeText(item.status, "Publicado"),
    attachmentLabel: normalizeText(item.attachmentLabel, "Abrir anexo"),
    owner: normalizeText(item.owner, "Comunicação Corporativa"),
    image: normalizeText(item.imageUrl),
    imageUrl: normalizeText(item.imageUrl),
    imageAlt: normalizeText(item.title, "Comunicado oficial"),
    body: normalizeParagraphs(bodyText),
    bodyText,
    isFeatured: Boolean(item.isFeatured),
    likeCount: Number(item.likeCount ?? 0),
    hasLiked: Boolean(item.hasLiked),
    hasSaved: Boolean(item.hasSaved),
    updatedAtUtc: normalizeText(item.updatedAtUtc),
    publishedAtRaw,
    publishedAtEditorValue: publishedAtRaw ? String(publishedAtRaw).slice(0, 10) : ""
  };
}

function buildFilters(items = []) {
  const counts = items.reduce((accumulator, item) => {
    const label = normalizeText(item.category, "Corporativo");
    accumulator.set(label, (accumulator.get(label) || 0) + 1);
    return accumulator;
  }, new Map());

  return [
    {
      label: "Todos",
      count: items.length,
      active: true
    },
    ...AVAILABLE_CATEGORIES.map((label) => ({
      label,
      count: counts.get(label) || 0,
      active: false
    }))
  ];
}

function buildKpis(items = []) {
  const values = [
    items.length,
    items.filter((item) => normalizeKey(item.priority).includes("alta")).length,
    items.filter((item) => normalizeKey(item.category) === "politicas").length,
    items.filter((item) => {
      const priority = normalizeKey(item.priority);
      const status = normalizeKey(item.status);
      return priority.includes("campanha") || priority.includes("inscricoes") || status.includes("vigor");
    }).length
  ];

  return KPI_TEMPLATE.map((item, index) => ({
    ...item,
    value: String(values[index] ?? 0)
  }));
}

function buildCommunicationCenter(items = [], loadError = "") {
  const latestUpdated = items[0]?.updatedAtUtc || items[0]?.publishedAtRaw || "";

  return {
    title: "CENTRAL DE COMUNICACAO OFICIAL",
    intro: {
      eyebrow: "COMUNICACAO INSTITUCIONAL",
      title: "Todos os comunicados oficiais em um unico lugar",
      subtitle: "Acompanhe avisos corporativos, políticas, comunicados de RH, atualizações de tecnologia e campanhas internas com governança e histórico centralizado.",
      lastUpdated: loadError
        ? "Falha ao consultar o backend de comunicados"
        : `Atualizado em ${formatDateTime(latestUpdated)}`
    },
    kpis: buildKpis(items),
    filters: buildFilters(items),
    featured: null,
    items,
    availableCategories: [...AVAILABLE_CATEGORIES],
    loadError
  };
}

function mapCreatePayload(payload = {}) {
  return {
    category: normalizeText(payload.category, "Corporativo"),
    priority: normalizeText(payload.priority, "Comunicado"),
    title: normalizeText(payload.title),
    summary: normalizeText(payload.summary),
    body: normalizeText(payload.body),
    audience: normalizeText(payload.audience, "Toda a companhia"),
    channel: normalizeText(payload.channel, "Portal"),
    status: normalizeText(payload.status, "Publicado"),
    attachmentLabel: normalizeText(payload.attachmentLabel, "Abrir anexo"),
    owner: normalizeText(payload.owner, "Comunicação Corporativa"),
    imageUrl: payload.imageUrl ? String(payload.imageUrl) : null,
    isFeatured: Boolean(payload.isFeatured),
    publishedAt: payload.publishedAt || new Date().toISOString()
  };
}

export async function listCommunications(options = {}) {
  const payload = await getJson(resolveApiEndpoint("communications"), options);
  return Array.isArray(payload) ? payload : [];
}

export async function toggleCommunicationLike(id, options = {}) {
  return postJson(`${resolveApiEndpoint("communications")}/${encodeURIComponent(id)}/like`, {}, options);
}

export async function createCommunication(payload = {}, options = {}) {
  return postJson(resolveApiEndpoint("communications"), mapCreatePayload(payload), options);
}

export async function getCommunicationById(id, options = {}) {
  const payload = await getJson(`${resolveApiEndpoint("communications")}/${id}`, options);
  return mapCommunicationItem(payload);
}

export async function updateCommunication(id, payload = {}, options = {}) {
  const response = await putJson(`${resolveApiEndpoint("communications")}/${id}`, mapCreatePayload(payload), options);
  return mapCommunicationItem(response);
}

export async function deleteCommunication(id, options = {}) {
  await deleteJson(`${resolveApiEndpoint("communications")}/${id}`, options);
}

export function getCommunicationEditorHeaders() {
  return getPortalAuthHeaders();
}

export function canManageCommunications(session = getStoredPortalSession()) {
  const user = session?.user;
  if (!user) {
    return false;
  }

  if (["PortalAdmin", "HrManager", "CommunicationEditor"].includes(user.role)) {
    return true;
  }

  const permissions = user.modulePermissions || [];
  const communicationPermission = permissions.find((item) => item.moduleKey === "communication-admin");
  if (communicationPermission?.accessLevel === "Manage") {
    return true;
  }

  const hrPermission = permissions.find((item) => item.moduleKey === "hr-profile");
  return hrPermission?.accessLevel === "Manage";
}

export async function getCommunicationCenterData() {
  try {
    const apiItems = await listCommunications({ headers: getPortalAuthHeaders() });
    const normalizedItems = sortByPublishedAtDesc(apiItems).map(mapCommunicationItem);
    return buildCommunicationCenter(normalizedItems);
  } catch (error) {
    console.error("Falha ao carregar comunicados persistidos.", error);
    return buildCommunicationCenter([], "Não foi possível consultar os comunicados persistidos no backend.");
  }
}
