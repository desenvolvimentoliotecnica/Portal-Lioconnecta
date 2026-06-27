import { getJson } from "../services/apiClient.js";
import { DATA_MODES, getRuntimeConfig, resolveApiEndpoint } from "../core/runtimeConfig.js?v=0.23.1";
import { getPortalAuthHeaders } from "../services/portalAuthService.js";
import { getHrProfileModule } from "./moduleCatalog.js";

export async function getHrProfileModuleData(slug, options = {}) {
  const module = getHrProfileModule(slug);
  if (!module) {
    throw new Error("Modulo de perfil RH invalido.");
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
  const module = getHrProfileModule(slug);
  return {
    title: module?.label || "Perfil RH",
    provider: "TOTVS RM",
    isSimulated: true
  };
}
