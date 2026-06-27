import { getJson } from "../services/apiClient.js";
import { DATA_MODES, getRuntimeConfig, resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders } from "../services/portalAuthService.js";
import { getJourneyModule } from "./moduleCatalog.js";

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

function buildMockPayload(slug) {
  const module = getJourneyModule(slug);
  return {
    title: module?.label || "Minha Jornada",
    provider: "ServiceNow",
    isSimulated: true
  };
}
