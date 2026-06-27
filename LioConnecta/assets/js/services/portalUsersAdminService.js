import { getJson, patchJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getStoredPortalSession } from "./portalAuthService.js";
import { mapMoodAuditEntry } from "./moodSurveyDashboardService.js";

function normalizeText(value, fallback = "") {
  const text = String(value ?? "").trim();
  return text || fallback;
}

function normalizePersonName(value) {
  const text = normalizeText(value);
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

function mapUser(item = {}) {
  return {
    id: normalizeText(item.id),
    login: normalizeText(item.login),
    displayName: normalizeText(item.displayName, "Usuario sem nome"),
    email: normalizeText(item.email),
    department: normalizeText(item.department),
    title: normalizeText(item.title),
    managerDisplayName: normalizePersonName(item.managerDisplayName),
    authenticationProvider: normalizeText(item.authenticationProvider, "LDAP"),
    role: normalizeText(item.role, "Collaborator"),
    roleLabel: normalizeText(item.roleLabel, "Colaborador"),
    permissions: Array.isArray(item.permissions) ? item.permissions.map((permission) => normalizeText(permission)).filter(Boolean) : [],
    modulePermissions: Array.isArray(item.modulePermissions)
      ? item.modulePermissions.map((permission) => ({
        moduleKey: normalizeText(permission.moduleKey),
        moduleLabel: normalizeText(permission.moduleLabel),
        accessLevel: normalizeText(permission.accessLevel, "None"),
        accessLevelLabel: normalizeText(permission.accessLevelLabel, "Sem acesso")
      }))
      : [],
    isActive: Boolean(item.isActive),
    loginCount: Number(item.loginCount ?? 0),
    failedLoginCount: Number(item.failedLoginCount ?? 0),
    createdAtUtc: normalizeText(item.createdAtUtc),
    updatedAtUtc: normalizeText(item.updatedAtUtc),
    lastLoginAtUtc: normalizeText(item.lastLoginAtUtc),
    lastFailedLoginAtUtc: normalizeText(item.lastFailedLoginAtUtc),
    lastKnownIpAddress: normalizeText(item.lastKnownIpAddress),
    lastOrigin: normalizeText(item.lastOrigin),
    createdAtLabel: formatDateTime(item.createdAtUtc),
    updatedAtLabel: formatDateTime(item.updatedAtUtc),
    lastLoginLabel: formatDateTime(item.lastLoginAtUtc),
    lastFailedLoginLabel: formatDateTime(item.lastFailedLoginAtUtc)
  };
}

function enrichWithCurrentPortalSession(user) {
  const portalSession = getStoredPortalSession();
  const portalUser = portalSession?.user;

  if (!portalUser || !user) {
    return user;
  }

  const sameUser =
    (portalUser.id && portalUser.id === user.id) ||
    (portalUser.login && portalUser.login === user.login) ||
    (portalUser.email && portalUser.email === user.email);

  if (!sameUser) {
    return user;
  }

  return {
    ...user,
    department: user.department || normalizeText(portalUser.department),
    title: user.title || normalizeText(portalUser.title),
    managerDisplayName: user.managerDisplayName || normalizePersonName(portalUser.managerDisplayName)
  };
}

function mapRoleOption(item = {}) {
  return {
    key: normalizeText(item.key),
    label: normalizeText(item.label),
    permissions: Array.isArray(item.permissions) ? item.permissions.map((permission) => normalizeText(permission)).filter(Boolean) : []
  };
}

function mapLoginEvent(item = {}) {
  return {
    id: normalizeText(item.id),
    portalUserId: normalizeText(item.portalUserId),
    login: normalizeText(item.login),
    displayName: normalizeText(item.displayName, "Usuario"),
    department: normalizeText(item.department),
    authenticationProvider: normalizeText(item.authenticationProvider, "LDAP"),
    eventType: normalizeText(item.eventType),
    eventTypeLabel: normalizeText(item.eventTypeLabel, "Evento"),
    isSuccess: Boolean(item.isSuccess),
    failureReason: normalizeText(item.failureReason),
    ipAddress: normalizeText(item.ipAddress),
    origin: normalizeText(item.origin),
    loggedAtUtc: normalizeText(item.loggedAtUtc),
    loggedAtLabel: formatDateTime(item.loggedAtUtc)
  };
}

function mapAuditEntry(item = {}) {
  return {
    id: normalizeText(item.id),
    portalUserId: normalizeText(item.portalUserId),
    portalUserDisplayName: normalizeText(item.portalUserDisplayName, "Usuario"),
    actionType: normalizeText(item.actionType),
    actorUsername: normalizeText(item.actorUsername),
    actorDisplayName: normalizeText(item.actorDisplayName),
    actorRole: normalizeText(item.actorRole),
    previousValue: normalizeText(item.previousValue),
    newValue: normalizeText(item.newValue),
    notes: normalizeText(item.notes),
    createdAtUtc: normalizeText(item.createdAtUtc),
    createdAtLabel: formatDateTime(item.createdAtUtc)
  };
}

function buildUrlWithQuery(baseUrl, query = {}) {
  const url = new URL(baseUrl);
  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "" || value === "all") {
      return;
    }

    url.searchParams.set(key, String(value));
  });

  return url.toString();
}

export async function listPortalUsers(query = {}, options = {}) {
  const payload = await getJson(
    buildUrlWithQuery(resolveApiEndpoint("adminPortalUsers"), query),
    options
  );

  return {
    items: Array.isArray(payload?.items) ? payload.items.map(mapUser).map(enrichWithCurrentPortalSession) : [],
    summary: {
      registeredUsers: Number(payload?.summary?.registeredUsers ?? 0),
      activeUsers: Number(payload?.summary?.activeUsers ?? 0),
      inactiveUsers: Number(payload?.summary?.inactiveUsers ?? 0),
      departmentsMapped: Number(payload?.summary?.departmentsMapped ?? 0),
      portalAdmins: Number(payload?.summary?.portalAdmins ?? 0),
      loginEvents: Number(payload?.summary?.loginEvents ?? 0),
      failedLoginEvents: Number(payload?.summary?.failedLoginEvents ?? 0),
      logoutEvents: Number(payload?.summary?.logoutEvents ?? 0),
      moodSurveyEvents: Number(payload?.summary?.moodSurveyEvents ?? 0)
    },
    roleOptions: Array.isArray(payload?.roleOptions) ? payload.roleOptions.map(mapRoleOption) : [],
    departmentOptions: Array.isArray(payload?.departmentOptions)
      ? payload.departmentOptions.map((item) => ({
        key: normalizeText(item.key || item.label),
        label: normalizeText(item.label || item.key),
        count: Number(item.count ?? 0)
      }))
      : [],
    moduleOptions: Array.isArray(payload?.moduleOptions)
      ? payload.moduleOptions.map((item) => ({
        key: normalizeText(item.key),
        label: normalizeText(item.label)
      }))
      : [],
    accessLevelOptions: Array.isArray(payload?.accessLevelOptions)
      ? payload.accessLevelOptions.map((item) => ({
        key: normalizeText(item.key),
        label: normalizeText(item.label)
      }))
      : [],
    recentLogins: Array.isArray(payload?.recentLogins) ? payload.recentLogins.map(mapLoginEvent) : [],
    recentAuditEntries: Array.isArray(payload?.recentAuditEntries) ? payload.recentAuditEntries.map(mapAuditEntry) : [],
    recentMoodSurveyEntries: Array.isArray(payload?.recentMoodSurveyEntries)
      ? payload.recentMoodSurveyEntries.map(mapMoodAuditEntry)
      : [],
    page: Number(payload?.page ?? 1),
    pageSize: Number(payload?.pageSize ?? 8),
    totalItems: Number(payload?.totalItems ?? 0),
    totalPages: Number(payload?.totalPages ?? 1),
    query: normalizeText(payload?.query),
    status: normalizeText(payload?.status, "all"),
    role: normalizeText(payload?.role),
    department: normalizeText(payload?.department, "all"),
    sortBy: normalizeText(payload?.sortBy, "displayName"),
    sortDirection: normalizeText(payload?.sortDirection, "asc")
  };
}

export async function updatePortalUserStatus(userId, isActive, options = {}) {
  const payload = await patchJson(
    resolveApiEndpoint("adminPortalUserStatus").replace("{id}", encodeURIComponent(userId)),
    { isActive },
    options
  );

  return mapUser(payload);
}

export async function updatePortalUserRole(userId, role, options = {}) {
  const payload = await patchJson(
    resolveApiEndpoint("adminPortalUserRole").replace("{id}", encodeURIComponent(userId)),
    { role },
    options
  );

  return mapUser(payload);
}

export async function updatePortalUserPermission(userId, moduleKey, accessLevel, options = {}) {
  const payload = await patchJson(
    resolveApiEndpoint("adminPortalUserPermission").replace("{id}", encodeURIComponent(userId)),
    { moduleKey, accessLevel },
    options
  );

  return mapUser(payload);
}
