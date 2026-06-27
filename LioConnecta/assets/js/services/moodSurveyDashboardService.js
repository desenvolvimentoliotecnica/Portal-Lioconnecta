import { getJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders, getStoredPortalSession } from "./portalAuthService.js";

const PERIOD_PRESETS = Object.freeze({
  "7d": 6,
  "30d": 29
});

function normalizeText(value, fallback = "") {
  return String(value ?? "").trim() || fallback;
}

function formatDateLabel(value) {
  if (!value) {
    return "";
  }

  const parts = String(value).split("-");
  if (parts.length !== 3) {
    return value;
  }

  return `${parts[2]}/${parts[1]}/${parts[0]}`;
}

function formatDateTime(value) {
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
    hour: "2-digit",
    minute: "2-digit"
  });
}

function toDateOnly(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

export function resolveMoodDashboardPeriod(preset = "7d") {
  const end = new Date();
  const start = new Date(end);
  start.setDate(end.getDate() - (PERIOD_PRESETS[preset] ?? PERIOD_PRESETS["7d"]));

  return {
    preset,
    startDate: toDateOnly(start),
    endDate: toDateOnly(end)
  };
}

export function canAccessHrMoodDashboard(session = getStoredPortalSession()) {
  const user = session?.user;
  if (!user) {
    return false;
  }

  if (user.role === "PortalAdmin" || user.role === "HrManager") {
    return true;
  }

  const hrPermission = (user.modulePermissions || []).find((item) => item.moduleKey === "hr-profile");
  return ["Interact", "Manage"].includes(hrPermission?.accessLevel);
}

function mapOption(item = {}) {
  return {
    key: normalizeText(item.key),
    label: normalizeText(item.label),
    emoji: normalizeText(item.emoji, "🙂"),
    count: Number(item.count ?? 0) || 0,
    percentage: Number(item.percentage ?? 0) || 0
  };
}

function mapDepartment(item = {}) {
  return {
    department: normalizeText(item.department, "Sem departamento"),
    totalVotes: Number(item.totalVotes ?? 0) || 0,
    motivatedCount: Number(item.motivatedCount ?? 0) || 0,
    goodCount: Number(item.goodCount ?? 0) || 0,
    tiredCount: Number(item.tiredCount ?? 0) || 0,
    options: Array.isArray(item.options) ? item.options.map(mapOption) : []
  };
}

function mapDailyTrend(item = {}) {
  return {
    date: normalizeText(item.date),
    dateLabel: formatDateLabel(item.date),
    totalVotes: Number(item.totalVotes ?? 0) || 0,
    motivatedCount: Number(item.motivatedCount ?? 0) || 0,
    goodCount: Number(item.goodCount ?? 0) || 0,
    tiredCount: Number(item.tiredCount ?? 0) || 0
  };
}

function normalizeDashboardPayload(payload = {}) {
  return {
    startDate: normalizeText(payload.startDate),
    endDate: normalizeText(payload.endDate),
    startDateLabel: formatDateLabel(payload.startDate),
    endDateLabel: formatDateLabel(payload.endDate),
    department: normalizeText(payload.department),
    summary: {
      totalVotes: Number(payload.summary?.totalVotes ?? 0) || 0,
      uniqueUsers: Number(payload.summary?.uniqueUsers ?? 0) || 0,
      activeUsers: Number(payload.summary?.activeUsers ?? 0) || 0,
      motivatedCount: Number(payload.summary?.motivatedCount ?? 0) || 0,
      goodCount: Number(payload.summary?.goodCount ?? 0) || 0,
      tiredCount: Number(payload.summary?.tiredCount ?? 0) || 0,
      participationRate: Number(payload.summary?.participationRate ?? 0) || 0
    },
    options: Array.isArray(payload.options) ? payload.options.map(mapOption) : [],
    departments: Array.isArray(payload.departments) ? payload.departments.map(mapDepartment) : [],
    dailyTrend: Array.isArray(payload.dailyTrend) ? payload.dailyTrend.map(mapDailyTrend) : [],
    departmentOptions: Array.isArray(payload.departmentOptions)
      ? payload.departmentOptions.map((item) => ({
        key: normalizeText(item.key || item.label),
        label: normalizeText(item.label || item.key),
        count: Number(item.count ?? 0) || 0
      }))
      : []
  };
}

function buildUrlWithQuery(baseUrl, query = {}) {
  const url = new URL(baseUrl);
  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") {
      return;
    }

    url.searchParams.set(key, String(value));
  });

  return url.toString();
}

export async function getMoodSurveyDashboard(query = {}, options = {}) {
  const payload = await getJson(
    buildUrlWithQuery(resolveApiEndpoint("moodSurveyDashboard"), query),
    {
      headers: getPortalAuthHeaders(),
      ...options
    }
  );

  return normalizeDashboardPayload(payload);
}

export function mapMoodAuditEntry(item = {}) {
  return {
    id: normalizeText(item.id),
    portalUserId: normalizeText(item.portalUserId),
    portalUserDisplayName: normalizeText(item.portalUserDisplayName, "Usuario"),
    department: normalizeText(item.department),
    optionKey: normalizeText(item.optionKey),
    optionLabel: normalizeText(item.optionLabel),
    optionEmoji: normalizeText(item.optionEmoji, "🙂"),
    actionType: normalizeText(item.actionType),
    actionTypeLabel: normalizeText(item.actionTypeLabel, "Humor registrado"),
    ipAddress: normalizeText(item.ipAddress),
    origin: normalizeText(item.origin),
    surveyDate: normalizeText(item.surveyDate),
    surveyDateLabel: formatDateLabel(item.surveyDate),
    createdAtUtc: normalizeText(item.createdAtUtc),
    createdAtLabel: formatDateTime(item.createdAtUtc)
  };
}
