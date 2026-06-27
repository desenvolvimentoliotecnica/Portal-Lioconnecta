import { getJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders } from "./portalAuthService.js";

function normalizeParticipants(participants = []) {
  if (!Array.isArray(participants)) {
    return [];
  }

  return participants
    .map((participant) => ({
      name: String(participant?.name || "").trim(),
      email: String(participant?.email || "").trim(),
      role: String(participant?.role || "").trim(),
      responseStatus: String(participant?.responseStatus || "").trim(),
      photoUrl: String(participant?.photoUrl || "").trim()
    }))
    .filter((participant) => participant.name || participant.email);
}

function normalizeAgendaPayload(payload = {}) {
  const items = Array.isArray(payload.items) ? payload.items : [];

  return {
    date: payload.date || "",
    totalCount: Number(payload.totalCount ?? items.length) || 0,
    items: items.map((item) => ({
      id: String(item.id || ""),
      title: String(item.title || ""),
      description: String(item.description || ""),
      location: String(item.location || ""),
      joinUrl: String(item.joinUrl || ""),
      participants: normalizeParticipants(item.participants),
      timeLabel: String(item.timeLabel || ""),
      source: String(item.source || ""),
      audience: String(item.audience || ""),
      startAtUtc: String(item.startAtUtc || ""),
      endAtUtc: String(item.endAtUtc || "")
    })).filter((item) => item.title)
  };
}

export async function getAgendaDayData() {
  const payload = await getJson(resolveApiEndpoint("agenda"), {
    headers: getPortalAuthHeaders()
  });

  return normalizeAgendaPayload(payload);
}

function isAgendaPanelTitle(title) {
  return title === "AGENDA" || title === "AGENDA DO DIA";
}

function mapAgendaPanelItem(item) {
  return {
    type: "agenda-event",
    id: item.id,
    title: item.title,
    timeLabel: item.timeLabel,
    label: `${item.timeLabel} • ${item.title}`,
    description: item.location || item.description || "",
    detailDescription: item.description || "",
    location: item.location || "",
    joinUrl: item.joinUrl || "",
    participants: normalizeParticipants(item.participants),
    source: item.source || "",
    audience: item.audience || "",
    startAtUtc: item.startAtUtc,
    endAtUtc: item.endAtUtc
  };
}

export function normalizeAgendaPanelItem(item, fallbackId = "") {
  if (typeof item === "string") {
    const [time, ...titleParts] = String(item).split("•");
    const title = titleParts.join("•").trim() || String(item).trim();

    return {
      id: fallbackId,
      title,
      description: "",
      detailDescription: "",
      location: "",
      joinUrl: "",
      participants: [],
      timeLabel: time.trim(),
      source: "",
      audience: "",
      startAtUtc: "",
      endAtUtc: ""
    };
  }

  const label = String(item.label || "");
  const parsedTitle = String(item.title || "").trim();
  const parsedTime = String(item.timeLabel || "").trim();
  const [timeFromLabel, ...titlePartsFromLabel] = label.split("•");
  const title = parsedTitle || titlePartsFromLabel.join("•").trim() || label.trim();
  const timeLabel = parsedTime || timeFromLabel.trim();

  return {
    id: String(item.id || fallbackId),
    title,
    description: String(item.description || ""),
    detailDescription: String(item.detailDescription || item.description || ""),
    location: String(item.location || ""),
    joinUrl: String(item.joinUrl || ""),
    participants: normalizeParticipants(item.participants),
    timeLabel,
    source: String(item.source || ""),
    audience: String(item.audience || ""),
    startAtUtc: String(item.startAtUtc || ""),
    endAtUtc: String(item.endAtUtc || "")
  };
}

export function collectAgendaEventsFromPanels(panels = []) {
  const agendaPanel = panels.find((panel) => isAgendaPanelTitle(panel.title));
  if (!agendaPanel) {
    return [];
  }

  return (Array.isArray(agendaPanel.items) ? agendaPanel.items : [])
    .map((item, index) => normalizeAgendaPanelItem(item, `agenda-item-${index}`))
    .filter((item) => item.title);
}

export function applyAgendaToShellData(data, agenda) {
  const normalized = normalizeAgendaPayload(agenda);

  return {
    ...data,
    rightPanels: (data.rightPanels || []).map((panel) => {
      if (!isAgendaPanelTitle(panel.title)) {
        return panel;
      }

      return {
        ...panel,
        title: "AGENDA",
        items: normalized.items.map(mapAgendaPanelItem)
      };
    }),
    agenda: normalized
  };
}
