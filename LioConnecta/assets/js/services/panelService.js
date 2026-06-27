import { getJson } from "./apiClient.js";
import { unwrapDataEnvelope } from "./apiClient.js";
import { mapPanelViewModel } from "../mappers/panelMapper.js";
import { validatePanelContract } from "../validators/panelValidator.js";
import { DATA_MODES, getRuntimeConfig, resolveDataSource, usesEnvelope } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders, getStoredPortalSession } from "./portalAuthService.js";

export async function getPanelData() {
  const config = getRuntimeConfig();
  const requestOptions = config.dataMode === DATA_MODES.API
    ? { headers: getPortalAuthHeaders() }
    : {};
  const rawPayload = await getJson(resolveDataSource("panels"), requestOptions);
  const raw = usesEnvelope(config.dataMode) ? unwrapDataEnvelope(rawPayload) : rawPayload;
  validatePanelContract(raw);
  const viewModel = mapPanelViewModel(raw);

  if (config.dataMode === DATA_MODES.API) {
    return viewModel;
  }

  const portalSession = getStoredPortalSession();

  if (!portalSession?.user) {
    return viewModel;
  }

  return {
    ...viewModel,
    rightPanels: viewModel.rightPanels.map((panel) => {
      if (panel.type !== "profile") {
        return panel;
      }

      return {
        ...panel,
        name: portalSession.user.displayName || panel.name,
        subtitle: portalSession.user.department || "",
        description: portalSession.user.title || "",
        manager: portalSession.user.managerDisplayName || ""
      };
    })
  };
}
