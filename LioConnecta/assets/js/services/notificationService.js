import { getJson, postWithoutBody } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders } from "./portalAuthService.js";

const CATEGORY_LABELS = {
  RH: "Comunicados RH",
  Corporativo: "Comunicados Corporativos",
  Tecnologia: "Tecnologia",
  Politicas: "Politicas",
  "Políticas": "Politicas",
  Eventos: "Eventos",
  Enquetes: "Enquetes"
};

function normalizeNotificationsPayload(payload = {}) {
  const summary = payload.summary || {};

  return {
    items: Array.isArray(payload.items) ? payload.items : [],
    summary: {
      totalCount: Number(summary.totalCount ?? 0) || 0,
      unreadCount: Number(summary.unreadCount ?? 0) || 0,
      readCount: Number(summary.readCount ?? 0) || 0,
      categoryCounts: summary.categoryCounts || {}
    }
  };
}

export async function getNotificationCenterData() {
  const payload = await getJson(resolveApiEndpoint("notifications"), {
    headers: getPortalAuthHeaders()
  });

  return normalizeNotificationsPayload(payload);
}

export async function markNotificationAsRead(notificationId) {
  return postWithoutBody(`${resolveApiEndpoint("notifications")}/${notificationId}/read`, {
    headers: getPortalAuthHeaders()
  });
}

export async function markAllNotificationsAsRead() {
  return postWithoutBody(`${resolveApiEndpoint("notifications")}/read-all`, {
    headers: getPortalAuthHeaders()
  });
}

export function applyNotificationsToShellData(data, notifications) {
  const normalized = normalizeNotificationsPayload(notifications);

  return {
    ...data,
    user: {
      ...(data.user || {}),
      notificationCount: normalized.summary.unreadCount
    },
    leftPanels: (data.leftPanels || []).map((panel) => {
      if (panel.title !== "MEU PAINEL") {
        return panel;
      }

      return {
        ...panel,
        items: buildNotificationPanelItems(normalized)
      };
    }),
    notifications: normalized
  };
}

function buildNotificationPanelItems(notifications) {
  const items = [
    {
      label: "Notificações Totais",
      badge: String(notifications.summary.totalCount)
    }
  ];

  Object.entries(notifications.summary.categoryCounts || {}).forEach(([category, count]) => {
    items.push({
      label: CATEGORY_LABELS[category] || category,
      badge: String(count)
    });
  });

  if (items.length === 1 && notifications.summary.totalCount > 0) {
    items.push({
      label: "Lidas",
      badge: String(notifications.summary.readCount)
    });
  }

  return items;
}
