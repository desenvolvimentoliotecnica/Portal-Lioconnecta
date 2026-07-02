import { getJson, postJson, putJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";

const DEFAULT_TOTVS_RM_SETTINGS = Object.freeze({
  isEnabled: false,
  server: "",
  port: 1433,
  database: "",
  userName: "",
  hasPassword: false,
  trustServerCertificate: true,
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
    server: normalizeText(payload.server),
    port: Number(payload.port || 1433),
    database: normalizeText(payload.database),
    userName: normalizeText(payload.userName),
    hasPassword: Boolean(payload.hasPassword),
    trustServerCertificate: payload.trustServerCertificate !== false,
    updatedAtUtc: normalizeText(payload.updatedAtUtc),
    loadError
  };
}

function mapSavePayload(payload = {}) {
  return {
    isEnabled: Boolean(payload.isEnabled),
    server: normalizeText(payload.server),
    port: Number(payload.port || 1433),
    database: normalizeText(payload.database),
    userName: normalizeText(payload.userName),
    password: String(payload.password || ""),
    trustServerCertificate: payload.trustServerCertificate !== false
  };
}

export async function getTotvsRmSettingsData(options = {}) {
  try {
    const payload = await getJson(resolveApiEndpoint("adminTotvsRm"), options);
    return normalizeSettings(payload);
  } catch (error) {
    console.error("Falha ao carregar configuracao TOTVS RM.", error);
    return normalizeSettings(DEFAULT_TOTVS_RM_SETTINGS, "Nao foi possivel carregar a configuracao TOTVS RM persistida.");
  }
}

export async function saveTotvsRmSettings(payload = {}, options = {}) {
  const response = await putJson(resolveApiEndpoint("adminTotvsRm"), mapSavePayload(payload), options);
  return normalizeSettings(response);
}

export async function testTotvsRmSettings(payload = {}, options = {}) {
  const response = await postJson(
    `${resolveApiEndpoint("adminTotvsRm")}/test`,
    mapSavePayload(payload),
    options
  );

  return {
    success: Boolean(response?.success),
    message: normalizeText(response?.message, "Teste concluido."),
    detail: normalizeText(response?.detail)
  };
}
