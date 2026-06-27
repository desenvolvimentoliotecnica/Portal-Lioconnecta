import { getJson, patchJson, postFormData, postJson, putJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getStoredPortalSession } from "./portalAuthService.js";

const STATUS_OPTIONS = Object.freeze([
  { key: "Draft", label: "Rascunho" },
  { key: "Published", label: "Publicada" },
  { key: "Closed", label: "Encerrada" },
  { key: "Archived", label: "Arquivada" }
]);

const RESULT_VISIBILITY_OPTIONS = Object.freeze([
  { key: "AfterVote", label: "Exibir apos voto" },
  { key: "Always", label: "Sempre exibir" },
  { key: "AfterClose", label: "Exibir apos encerramento" }
]);

function formatDate(value, { withTime = false } = {}) {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return date.toLocaleString("pt-BR", {
    timeZone: "UTC",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    ...(withTime
      ? {
        hour: "2-digit",
        minute: "2-digit"
      }
      : {})
  });
}

function formatDateTimeLocal(value) {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  const year = date.getUTCFullYear();
  const month = `${date.getUTCMonth() + 1}`.padStart(2, "0");
  const day = `${date.getUTCDate()}`.padStart(2, "0");
  const hours = `${date.getUTCHours()}`.padStart(2, "0");
  const minutes = `${date.getUTCMinutes()}`.padStart(2, "0");
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function normalizeText(value, fallback = "") {
  const text = String(value ?? "").trim();
  return text || fallback;
}

function mapOption(option = {}) {
  return {
    id: normalizeText(option.id),
    label: normalizeText(option.label, "Opcao"),
    displayOrder: Number(option.displayOrder ?? 0),
    votes: Number(option.votes ?? 0),
    percentage: Number(option.percentage ?? 0),
    isSelected: Boolean(option.isSelected)
  };
}

export function mapPollItem(item = {}) {
  return {
    id: normalizeText(item.id),
    slug: normalizeText(item.slug),
    title: normalizeText(item.title, "Enquete interna"),
    summary: normalizeText(item.summary, "Sem resumo disponivel."),
    body: normalizeText(item.body),
    imageUrl: normalizeText(item.imageUrl),
    attachmentLabel: normalizeText(item.attachmentLabel),
    attachmentUrl: normalizeText(item.attachmentUrl),
    audience: normalizeText(item.audience, "Toda a companhia"),
    status: normalizeText(item.status, "Draft"),
    statusLabel: normalizeText(item.statusLabel, "Rascunho"),
    allowMultipleChoices: Boolean(item.allowMultipleChoices),
    resultsVisibility: normalizeText(item.resultsVisibility, "AfterVote"),
    resultsVisibilityLabel: normalizeText(item.resultsVisibilityLabel, "Exibir apos voto"),
    isFeatured: Boolean(item.isFeatured),
    publishedAtUtc: item.publishedAtUtc || null,
    closesAtUtc: item.closesAtUtc || null,
    publishedAtLabel: formatDate(item.publishedAtUtc),
    closesAtLabel: formatDate(item.closesAtUtc, { withTime: true }),
    publishedAtEditorValue: formatDateTimeLocal(item.publishedAtUtc),
    closesAtEditorValue: formatDateTimeLocal(item.closesAtUtc),
    totalVotes: Number(item.totalVotes ?? 0),
    hasVoted: Boolean(item.hasVoted),
    resultsVisible: Boolean(item.resultsVisible),
    options: Array.isArray(item.options) ? item.options.map(mapOption) : []
  };
}

function mapAdminPollItem(item = {}) {
  const mapped = mapPollItem(item);

  return {
    ...mapped,
    uniqueVoters: Number(item.uniqueVoters ?? 0),
    createdAtUtc: item.createdAtUtc || null,
    updatedAtUtc: item.updatedAtUtc || null,
    createdAtLabel: formatDate(item.createdAtUtc, { withTime: true }),
    updatedAtLabel: formatDate(item.updatedAtUtc, { withTime: true })
  };
}

function createEmptyPublicData(loadError = "") {
  return {
    intro: {
      eyebrow: "PESQUISAS INTERNAS",
      title: "Enquetes da LIOCONNECTA",
      subtitle: "Acompanhe pautas abertas, registre sua preferencia e enxergue resultados com transparencia.",
      loadError
    },
    featured: null,
    homePolls: [],
    openPolls: [],
    closedPolls: [],
    allPolls: [],
    stats: {
      total: 0,
      open: 0,
      closed: 0,
      votes: 0
    }
  };
}

function buildHomePolls(items = []) {
  const now = Date.now();

  return items
    .filter((poll) => {
      if (poll.status !== "Published" || poll.hasVoted) {
        return false;
      }

      if (poll.publishedAtUtc) {
        const publishedAt = new Date(poll.publishedAtUtc).getTime();
        if (!Number.isNaN(publishedAt) && publishedAt > now) {
          return false;
        }
      }

      if (poll.closesAtUtc) {
        const closesAt = new Date(poll.closesAtUtc).getTime();
        if (!Number.isNaN(closesAt) && closesAt <= now) {
          return false;
        }
      }

      return true;
    })
    .sort((left, right) => {
      if (left.isFeatured !== right.isFeatured) {
        return left.isFeatured ? -1 : 1;
      }

      const leftTime = new Date(left.publishedAtUtc || 0).getTime();
      const rightTime = new Date(right.publishedAtUtc || 0).getTime();
      return rightTime - leftTime;
    });
}

function buildPublicData(items = [], loadError = "") {
  const mappedItems = items.map(mapPollItem);
  const homePolls = buildHomePolls(mappedItems);
  const featured = homePolls[0]
    || mappedItems.find((item) => item.status === "Published" && item.isFeatured)
    || mappedItems.find((item) => item.status === "Published")
    || mappedItems[0]
    || null;
  const openPolls = mappedItems.filter((item) => item.status === "Published");
  const closedPolls = mappedItems.filter((item) => item.status === "Closed");

  return {
    intro: {
      eyebrow: "PESQUISAS INTERNAS",
      title: "Enquetes da LIOCONNECTA",
      subtitle: "Acompanhe pautas abertas, registre sua preferencia e enxergue resultados com transparencia.",
      loadError
    },
    featured,
    homePolls,
    openPolls,
    closedPolls,
    allPolls: mappedItems,
    stats: {
      total: mappedItems.length,
      open: openPolls.length,
      closed: closedPolls.length,
      votes: mappedItems.reduce((accumulator, item) => accumulator + item.totalVotes, 0)
    }
  };
}

function createEmptyAdminData(loadError = "") {
  return {
    intro: {
      eyebrow: "ADMINISTRATIVO",
      title: "Enquetes",
      subtitle: "Publique novas pesquisas, acompanhe votos e ajuste o ciclo de vida das enquetes em um unico fluxo.",
      loadError
    },
    items: [],
    summary: {
      totalPolls: 0,
      publishedPolls: 0,
      draftPolls: 0,
      closedPolls: 0,
      archivedPolls: 0,
      totalVotes: 0
    },
    statusOptions: [...STATUS_OPTIONS],
    resultsVisibilityOptions: [...RESULT_VISIBILITY_OPTIONS]
  };
}

function buildAdminData(payload = {}, loadError = "") {
  return {
    intro: {
      eyebrow: "ADMINISTRATIVO",
      title: "Enquetes",
      subtitle: "Publique novas pesquisas, acompanhe votos e ajuste o ciclo de vida das enquetes em um unico fluxo.",
      loadError
    },
    items: Array.isArray(payload.items) ? payload.items.map(mapAdminPollItem) : [],
    summary: {
      totalPolls: Number(payload.summary?.totalPolls ?? 0),
      publishedPolls: Number(payload.summary?.publishedPolls ?? 0),
      draftPolls: Number(payload.summary?.draftPolls ?? 0),
      closedPolls: Number(payload.summary?.closedPolls ?? 0),
      archivedPolls: Number(payload.summary?.archivedPolls ?? 0),
      totalVotes: Number(payload.summary?.totalVotes ?? 0)
    },
    statusOptions: [...STATUS_OPTIONS],
    resultsVisibilityOptions: [...RESULT_VISIBILITY_OPTIONS]
  };
}

function mapUpsertPayload(payload = {}) {
  const options = Array.isArray(payload.options) ? payload.options : [];

  return {
    title: normalizeText(payload.title),
    summary: normalizeText(payload.summary),
    body: normalizeText(payload.body),
    imageUrl: normalizeText(payload.imageUrl) || null,
    attachmentLabel: normalizeText(payload.attachmentLabel) || null,
    attachmentUrl: normalizeText(payload.attachmentUrl) || null,
    audience: normalizeText(payload.audience, "Toda a companhia"),
    status: normalizeText(payload.status, "Draft"),
    allowMultipleChoices: Boolean(payload.allowMultipleChoices),
    resultsVisibility: normalizeText(payload.resultsVisibility, "AfterVote"),
    isFeatured: Boolean(payload.isFeatured),
    publishedAtUtc: payload.publishedAtUtc || null,
    closesAtUtc: payload.closesAtUtc || null,
    options: options
      .map((option) => ({
        id: option.id || null,
        label: normalizeText(option.label)
      }))
      .filter((option) => option.label)
  };
}

export function getPollStatusOptions() {
  return [...STATUS_OPTIONS];
}

export function getPollResultsVisibilityOptions() {
  return [...RESULT_VISIBILITY_OPTIONS];
}

export function canManagePolls(session = getStoredPortalSession()) {
  const user = session?.user;
  if (!user) {
    return false;
  }

  if (user.role === "PortalAdmin" || user.role === "HrManager") {
    return true;
  }

  const permissions = user.modulePermissions || [];
  const pollPermission = permissions.find((item) => item.moduleKey === "poll-admin");
  if (pollPermission?.accessLevel === "Manage") {
    return true;
  }

  const hrPermission = permissions.find((item) => item.moduleKey === "hr-profile");
  return hrPermission?.accessLevel === "Manage";
}

export async function listPolls(options = {}) {
  const payload = await getJson(resolveApiEndpoint("polls"), options);
  return Array.isArray(payload) ? payload : [];
}

export async function getPollBySlug(slug, options = {}) {
  return getJson(`${resolveApiEndpoint("polls")}/slug/${encodeURIComponent(slug)}`, options);
}

export async function votePoll(pollId, optionIds, options = {}) {
  return postJson(`${resolveApiEndpoint("polls")}/${encodeURIComponent(pollId)}/vote`, { optionIds }, options);
}

export async function listAdminPolls(options = {}) {
  return getJson(resolveApiEndpoint("adminPolls"), options);
}

export async function getAdminPollById(id, options = {}) {
  return getJson(`${resolveApiEndpoint("adminPolls")}/${encodeURIComponent(id)}`, options);
}

export async function createPoll(payload, options = {}) {
  return postJson(resolveApiEndpoint("adminPolls"), mapUpsertPayload(payload), options);
}

export async function updatePoll(id, payload, options = {}) {
  return putJson(`${resolveApiEndpoint("adminPolls")}/${encodeURIComponent(id)}`, mapUpsertPayload(payload), options);
}

export async function updatePollStatus(id, status, options = {}) {
  return patchJson(`${resolveApiEndpoint("adminPolls")}/${encodeURIComponent(id)}/status`, { status }, options);
}

export async function uploadPollAsset(assetType, file, options = {}) {
  const formData = new FormData();
  formData.append("file", file);
  return postFormData(`${resolveApiEndpoint("adminPollAssets")}/${encodeURIComponent(assetType)}`, formData, options);
}

export async function getPollCenterData(options = {}) {
  try {
    const items = await listPolls(options);
    return buildPublicData(items);
  } catch (error) {
    console.error("Falha ao carregar enquetes publicadas.", error);
    return buildPublicData([], "Nao foi possivel consultar as enquetes publicadas no backend.");
  }
}

export async function getPollDetailData(slug, options = {}) {
  try {
    const item = await getPollBySlug(slug, options);
    return item ? mapPollItem(item) : null;
  } catch (error) {
    console.error("Falha ao carregar detalhe da enquete.", error);
    return null;
  }
}

export async function getAdminPollData(options = {}) {
  try {
    const payload = await listAdminPolls(options);
    return buildAdminData(payload);
  } catch (error) {
    console.error("Falha ao carregar painel administrativo de enquetes.", error);
    return createEmptyAdminData("Nao foi possivel consultar as enquetes administrativas no backend.");
  }
}
