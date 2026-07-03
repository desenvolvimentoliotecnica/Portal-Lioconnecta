import { getJson } from "../services/apiClient.js";
import { DATA_MODES, getRuntimeConfig, resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders } from "../services/portalAuthService.js";

export async function getHrTeamDashboard(options = {}) {
  const config = getRuntimeConfig();
  if (config.dataMode !== DATA_MODES.API) {
    return {
      title: "Minha Equipe",
      members: [],
      activeCount: 0,
      provider: "TOTVS RM",
      isSimulated: true,
      availabilityStatus: "module_disabled",
      userMessage: "Dashboard de equipe disponivel apenas com API ativa."
    };
  }

  return getJson(resolveApiEndpoint("hrEquipe"), {
    headers: options.headers || getPortalAuthHeaders(),
    ...options
  });
}

export function normalizeHrTeamDashboard(payload = {}) {
  return {
    title: payload.title || payload.Title || "Minha Equipe",
    members: (payload.members || payload.Members || []).map((member) => ({
      chapa: member.chapa || member.Chapa || "",
      name: member.name || member.Name || "",
      role: member.role || member.Role || "",
      department: member.department || member.Department || "",
      status: member.status || member.Status || ""
    })),
    activeCount: payload.activeCount ?? payload.ActiveCount ?? 0,
    provider: payload.provider || payload.Provider || "TOTVS RM",
    isSimulated: payload.isSimulated ?? payload.IsSimulated ?? false,
    availabilityStatus: payload.availabilityStatus || payload.AvailabilityStatus || "ok",
    userMessage: payload.userMessage || payload.UserMessage || ""
  };
}
