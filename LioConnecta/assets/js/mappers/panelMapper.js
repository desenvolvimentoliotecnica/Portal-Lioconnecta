import { DEFAULT_PANELS } from "../view-models/defaults.js";
import { asArray, asString } from "./shared.js";

function mapPanelItem(item) {
  if (typeof item === "string") {
    return item;
  }

  const mapped = {
    label: asString(item?.label, ""),
    badge: asString(item?.badge, ""),
    value: asString(item?.value, ""),
    className: asString(item?.className, ""),
    shortLabel: asString(item?.shortLabel, ""),
    url: asString(item?.url, ""),
    provider: asString(item?.provider, "")
  };

  if (asString(item?.type, "") === "agenda-event" || item?.startAtUtc || item?.title) {
    return {
      ...mapped,
      type: asString(item?.type, "agenda-event"),
      id: asString(item?.id, ""),
      title: asString(item?.title, ""),
      timeLabel: asString(item?.timeLabel, ""),
      detailDescription: asString(item?.detailDescription, ""),
      location: asString(item?.location, ""),
      joinUrl: asString(item?.joinUrl, ""),
      participants: Array.isArray(item?.participants)
        ? item.participants.map((participant) => ({
          name: asString(participant?.name, ""),
          email: asString(participant?.email, ""),
          role: asString(participant?.role, ""),
          responseStatus: asString(participant?.responseStatus, ""),
          photoUrl: asString(participant?.photoUrl, "")
        })).filter((participant) => participant.name || participant.email)
        : [],
      source: asString(item?.source, ""),
      audience: asString(item?.audience, ""),
      startAtUtc: asString(item?.startAtUtc, ""),
      endAtUtc: asString(item?.endAtUtc, "")
    };
  }

  return mapped;
}

function mapGenericPanel(panel) {
  return {
    type: asString(panel?.type, ""),
    title: asString(panel?.title, "Painel"),
    name: asString(panel?.name, ""),
    subtitle: asString(panel?.subtitle, ""),
    description: asString(panel?.description, ""),
    manager: asString(panel?.manager, ""),
    moduleKey: asString(panel?.moduleKey, ""),
    items: asArray(panel?.items).map(mapPanelItem).filter((item) => {
      if (typeof item === "string") {
        return Boolean(item);
      }

      return Boolean(item.label || item.value || item.shortLabel || item.url);
    })
  };
}

export function mapPanelViewModel(raw = {}) {
  return {
    leftPanels: asArray(raw.leftPanels).length
      ? asArray(raw.leftPanels).map(mapGenericPanel)
      : [...DEFAULT_PANELS],
    rightPanels: asArray(raw.rightPanels).length
      ? asArray(raw.rightPanels).map(mapGenericPanel)
      : [...DEFAULT_PANELS]
  };
}
