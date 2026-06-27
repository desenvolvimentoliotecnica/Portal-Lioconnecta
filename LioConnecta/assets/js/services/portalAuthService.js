import { getJson, postJson, postWithoutBody } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";

const STORAGE_KEY = "lioconnecta.portalSession";
const DEFAULT_PORTAL_HASH = "#inicio";

function normalizePersonName(value) {
  const text = String(value ?? "").trim();
  if (!text) {
    return "";
  }

  if (/[a-zà-ÿ]/.test(text)) {
    return text;
  }

  return text
    .split(/\s+/)
    .map((part) => part ? `${part.charAt(0).toUpperCase()}${part.slice(1).toLowerCase()}` : "")
    .join(" ");
}

function getStorage() {
  return typeof window !== "undefined" ? window.localStorage : null;
}

function normalizeSession(payload) {
  if (!payload?.token || !payload?.expiresAtUtc || !payload?.user?.displayName) {
    return null;
  }

  return {
    token: String(payload.token),
    expiresAtUtc: String(payload.expiresAtUtc),
    user: {
      id: String(payload.user.id ?? ""),
      login: String(payload.user.login ?? ""),
      displayName: String(payload.user.displayName ?? ""),
      email: String(payload.user.email ?? ""),
      department: String(payload.user.department ?? ""),
      title: String(payload.user.title ?? ""),
      managerDisplayName: normalizePersonName(payload.user.managerDisplayName),
      role: String(payload.user.role ?? ""),
      roleLabel: String(payload.user.roleLabel ?? ""),
      permissions: Array.isArray(payload.user.permissions) ? payload.user.permissions.map((item) => String(item ?? "")) : [],
      modulePermissions: Array.isArray(payload.user.modulePermissions)
        ? payload.user.modulePermissions.map((item) => ({
          moduleKey: String(item?.moduleKey ?? ""),
          moduleLabel: String(item?.moduleLabel ?? ""),
          accessLevel: String(item?.accessLevel ?? ""),
          accessLevelLabel: String(item?.accessLevelLabel ?? "")
        }))
        : []
    }
  };
}

function isExpired(expiresAtUtc) {
  const expiresAt = new Date(expiresAtUtc).getTime();
  return !expiresAt || Number.isNaN(expiresAt) || expiresAt <= Date.now();
}

export function getStoredPortalSession() {
  const storage = getStorage();
  if (!storage) {
    return null;
  }

  try {
    const raw = JSON.parse(storage.getItem(STORAGE_KEY) ?? "null");
    const session = normalizeSession(raw);
    if (!session || isExpired(session.expiresAtUtc)) {
      clearPortalSession();
      return null;
    }

    return session;
  } catch {
    clearPortalSession();
    return null;
  }
}

export function storePortalSession(payload) {
  const storage = getStorage();
  const session = normalizeSession(payload);

  if (!storage || !session) {
    return null;
  }

  storage.setItem(STORAGE_KEY, JSON.stringify(session));
  return session;
}

export function clearPortalSession() {
  const storage = getStorage();
  storage?.removeItem(STORAGE_KEY);
}

export function getPortalAuthHeaders() {
  const session = getStoredPortalSession();
  return session?.token
    ? {
      "X-Portal-Token": session.token,
      Authorization: `Bearer ${session.token}`
    }
    : {};
}

export async function loginWithLdap(login, password) {
  const response = await postJson(resolveApiEndpoint("portalLdapLogin"), {
    login,
    password
  });

  return storePortalSession(response);
}

function parseHttpStatus(error) {
  const match = String(error?.message ?? "").match(/HTTP (\d{3})/);
  return match ? Number(match[1]) : 0;
}

function sleep(ms) {
  return new Promise((resolve) => {
    window.setTimeout(resolve, ms);
  });
}

async function requestPortalSession() {
  const session = getStoredPortalSession();
  if (!session) {
    return null;
  }

  try {
    const payload = await getJson(resolveApiEndpoint("portalSession"), {
      headers: getPortalAuthHeaders()
    });
    return storePortalSession(payload);
  } catch (error) {
    const status = parseHttpStatus(error);
    if (status === 401 || status === 403) {
      clearPortalSession();
      return null;
    }

    throw error;
  }
}

export async function fetchPortalSession() {
  return requestPortalSession();
}

export async function ensureValidPortalSession(options = {}) {
  const { retries = 3, retryDelayMs = 350 } = options;

  if (!getStoredPortalSession()) {
    return null;
  }

  let lastError = null;

  for (let attempt = 0; attempt <= retries; attempt += 1) {
    try {
      const validated = await requestPortalSession();
      if (validated) {
        return validated;
      }

      return null;
    } catch (error) {
      lastError = error;
      const status = parseHttpStatus(error);
      if (status === 401 || status === 403) {
        return null;
      }

      if (attempt < retries) {
        await sleep(retryDelayMs * (attempt + 1));
      }
    }
  }

  const cached = getStoredPortalSession();
  if (cached) {
    return cached;
  }

  throw lastError ?? new Error("Falha ao validar sessao do portal.");
}

export async function logoutPortal() {
  const session = getStoredPortalSession();

  if (session?.token) {
    try {
      await postWithoutBody(resolveApiEndpoint("portalLogout"), {
        headers: getPortalAuthHeaders()
      });
    } catch {
      // Ignora falha remota e encerra sessao localmente.
    }
  }

  clearPortalSession();
}

export function buildPortalLoginUrl(nextHash = DEFAULT_PORTAL_HASH) {
  return `./login/?next=${encodeURIComponent(nextHash || DEFAULT_PORTAL_HASH)}`;
}

export function redirectToPortalLogin(nextHash = DEFAULT_PORTAL_HASH) {
  if (typeof window === "undefined") {
    return;
  }

  window.location.href = buildPortalLoginUrl(nextHash);
}

export function resolvePortalPostLoginTarget() {
  if (typeof window === "undefined") {
    return DEFAULT_PORTAL_HASH;
  }

  const params = new URLSearchParams(window.location.search);
  return params.get("next") || DEFAULT_PORTAL_HASH;
}
