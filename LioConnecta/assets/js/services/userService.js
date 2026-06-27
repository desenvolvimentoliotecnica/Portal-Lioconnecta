import { getJson } from "./apiClient.js";
import { unwrapDataEnvelope } from "./apiClient.js";
import { mapUserHomeContextViewModel } from "../mappers/userMapper.js";
import { validateUserContract } from "../validators/userValidator.js";
import { DATA_MODES, getRuntimeConfig, resolveDataSource, usesEnvelope } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders, getStoredPortalSession } from "./portalAuthService.js";

function buildUserHeadline(user = {}, fallbackName = "") {
  return [user.displayName || fallbackName, user.title, user.department]
    .map((item) => String(item ?? "").trim())
    .filter(Boolean)
    .join(" • ");
}

export async function getUserHomeContext() {
  const config = getRuntimeConfig();
  const requestOptions = config.dataMode === DATA_MODES.API
    ? { headers: getPortalAuthHeaders() }
    : {};
  const rawPayload = await getJson(resolveDataSource("user"), requestOptions);
  const raw = usesEnvelope(config.dataMode) ? unwrapDataEnvelope(rawPayload) : rawPayload;
  validateUserContract(raw);
  const viewModel = mapUserHomeContextViewModel(raw);
  const portalSession = getStoredPortalSession();

  if (!portalSession?.user) {
    return viewModel;
  }

  return {
    ...viewModel,
    user: {
      ...viewModel.user,
      name: buildUserHeadline(portalSession.user, viewModel.user.name) || viewModel.user.name,
      area: ""
    }
  };
}
