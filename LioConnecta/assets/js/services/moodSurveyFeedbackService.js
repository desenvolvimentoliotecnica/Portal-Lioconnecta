import { deleteJson, getJson, postJson, putJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders, getStoredPortalSession } from "./portalAuthService.js";

export const MOOD_FEEDBACK_OPTION_GROUPS = Object.freeze([
  { key: "motivated", label: "Motivado", emoji: "😄" },
  { key: "good", label: "Bem", emoji: "🙂" },
  { key: "tired", label: "Cansado", emoji: "😴" }
]);

function normalizeMessage(item = {}) {
  return {
    id: item.id || "",
    optionKey: item.optionKey || "",
    optionLabel: item.optionLabel || "",
    optionEmoji: item.optionEmoji || "🙂",
    message: item.message || "",
    sortOrder: Number(item.sortOrder ?? 0) || 0,
    isActive: Boolean(item.isActive),
    createdAtUtc: item.createdAtUtc || null,
    updatedAtUtc: item.updatedAtUtc || null
  };
}

function normalizeSummary(item = {}) {
  return {
    optionKey: item.optionKey || "",
    optionLabel: item.optionLabel || "",
    optionEmoji: item.optionEmoji || "🙂",
    totalMessages: Number(item.totalMessages ?? 0) || 0,
    activeMessages: Number(item.activeMessages ?? 0) || 0
  };
}

export function canManageMoodSurveyFeedback(session = getStoredPortalSession()) {
  const user = session?.user;
  if (!user) {
    return false;
  }

  if (user.role === "PortalAdmin" || user.role === "HrManager") {
    return true;
  }

  const hrPermission = (user.modulePermissions || []).find((item) => item.moduleKey === "hr-profile");
  return hrPermission?.accessLevel === "Manage";
}

export async function listMoodFeedbackMessages(optionKey = "") {
  const query = optionKey ? `?optionKey=${encodeURIComponent(optionKey)}` : "";
  const payload = await getJson(`${resolveApiEndpoint("moodSurveyFeedbackMessages")}${query}`, {
    headers: getPortalAuthHeaders()
  });

  return {
    items: Array.isArray(payload?.items) ? payload.items.map(normalizeMessage) : [],
    optionSummaries: Array.isArray(payload?.optionSummaries)
      ? payload.optionSummaries.map(normalizeSummary)
      : []
  };
}

export async function createMoodFeedbackMessage(body) {
  const payload = await postJson(resolveApiEndpoint("moodSurveyFeedbackMessages"), body, {
    headers: getPortalAuthHeaders()
  });

  return normalizeMessage(payload);
}

export async function updateMoodFeedbackMessage(id, body) {
  const payload = await putJson(`${resolveApiEndpoint("moodSurveyFeedbackMessages")}/${id}`, body, {
    headers: getPortalAuthHeaders()
  });

  return normalizeMessage(payload);
}

export async function deleteMoodFeedbackMessage(id) {
  await deleteJson(`${resolveApiEndpoint("moodSurveyFeedbackMessages")}/${id}`, {
    headers: getPortalAuthHeaders()
  });
}
