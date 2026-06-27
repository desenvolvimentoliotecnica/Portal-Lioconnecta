import { getJson, postJson, putJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";

const DEFAULT_MICROSOFT_GRAPH_SETTINGS = Object.freeze({
  isEnabled: false,
  tenantId: "",
  clientId: "",
  hasClientSecret: false,
  userIdentifier: "userPrincipalName",
  updatedAtUtc: "",
  loadError: ""
});

function normalizeText(value, fallback = "") {
  const text = String(value ?? "").trim();
  return text || fallback;
}

function normalizeSettings(payload = {}, loadError = "") {
  return {
    isEnabled: Boolean(payload.isEnabled),
    tenantId: normalizeText(payload.tenantId),
    clientId: normalizeText(payload.clientId),
    hasClientSecret: Boolean(payload.hasClientSecret),
    userIdentifier: normalizeText(payload.userIdentifier, DEFAULT_MICROSOFT_GRAPH_SETTINGS.userIdentifier),
    updatedAtUtc: normalizeText(payload.updatedAtUtc),
    loadError
  };
}

function mapSavePayload(payload = {}) {
  return {
    isEnabled: Boolean(payload.isEnabled),
    tenantId: normalizeText(payload.tenantId),
    clientId: normalizeText(payload.clientId),
    clientSecret: String(payload.clientSecret || ""),
    userIdentifier: normalizeText(payload.userIdentifier, DEFAULT_MICROSOFT_GRAPH_SETTINGS.userIdentifier)
  };
}

export async function getMicrosoftGraphSettingsData(options = {}) {
  try {
    const payload = await getJson(resolveApiEndpoint("adminMicrosoftGraph"), options);
    return normalizeSettings(payload);
  } catch (error) {
    console.error("Falha ao carregar configuracao Microsoft Graph.", error);
    return normalizeSettings(DEFAULT_MICROSOFT_GRAPH_SETTINGS, "Nao foi possivel carregar a configuracao Microsoft Graph persistida.");
  }
}

export async function saveMicrosoftGraphSettings(payload = {}, options = {}) {
  const response = await putJson(resolveApiEndpoint("adminMicrosoftGraph"), mapSavePayload(payload), options);
  return normalizeSettings(response);
}

export async function testMicrosoftGraphSettings(payload = {}, options = {}) {
  const response = await postJson(
    `${resolveApiEndpoint("adminMicrosoftGraph")}/test`,
    mapSavePayload(payload),
    options
  );

  return {
    success: Boolean(response?.success),
    message: normalizeText(response?.message, "Teste concluido."),
    detail: normalizeText(response?.detail)
  };
}
