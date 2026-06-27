import { DATA_MODES, getRuntimeConfig } from "../core/runtimeConfig.js?v=0.21.4";

function resolveUploadsOrigin() {
  const config = getRuntimeConfig();
  const apiBase = String(config.apiBaseUrl || "").trim();
  if (!apiBase) {
    return "";
  }

  return apiBase.replace(/\/api\/?$/i, "");
}

function normalizeFeedMediaPath(url = "") {
  const value = String(url || "").trim();
  if (!value) {
    return "";
  }

  if (/^https?:\/\//i.test(value)) {
    return value;
  }

  if (value.startsWith("/uploads/")) {
    return value;
  }

  if (!value.includes("/") && /\.(jpe?g|png|gif|webp|bmp)$/i.test(value)) {
    return `/uploads/feed/${value}`;
  }

  return value;
}

export function resolveFeedMediaUrl(url = "") {
  const normalized = normalizeFeedMediaPath(url);
  if (!normalized) {
    return "";
  }

  if (/^https?:\/\//i.test(normalized)) {
    try {
      const parsed = new URL(normalized);
      if (parsed.pathname.includes("/uploads/feed/")) {
        const origin = resolveUploadsOrigin();
        return origin ? `${origin}${parsed.pathname}` : normalized;
      }
    } catch {
      return normalized;
    }

    return normalized;
  }

  if (normalized.startsWith("/uploads/")) {
    const origin = resolveUploadsOrigin();
    if (origin) {
      return `${origin}${normalized}`;
    }

    if (getRuntimeConfig().dataMode === DATA_MODES.API) {
      const currentWindow = typeof window !== "undefined" ? window : undefined;
      const protocol = currentWindow?.location?.protocol?.startsWith("http")
        ? currentWindow.location.protocol
        : "http:";
      const hostname = currentWindow?.location?.hostname || "localhost";
      return `${protocol}//${hostname}:3030${normalized}`;
    }
  }

  return normalized;
}

export function serializeGalleryImages(images = []) {
  return encodeURIComponent(JSON.stringify(images.map((image) => ({
    id: String(image.id || ""),
    url: String(image.url || ""),
    description: String(image.description || ""),
    aspectRatio: String(image.aspectRatio || "free"),
    commentCount: Number(image.commentCount ?? 0)
  }))));
}

export function parseGalleryImages(rawValue) {
  if (!rawValue) {
    return [];
  }

  const attempts = [
    () => JSON.parse(decodeURIComponent(rawValue)),
    () => JSON.parse(rawValue)
  ];

  for (const attempt of attempts) {
    try {
      const parsed = attempt();
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      // tenta o proximo formato
    }
  }

  return [];
}

export function readGalleryImages(gallery) {
  const parsed = parseGalleryImages(gallery?.getAttribute("data-feed-gallery"));
  if (parsed.length) {
    return parsed;
  }

  return Array.from(gallery?.querySelectorAll(".post-gallery__item[data-photo-url]") || [])
    .map((button) => ({
      id: button.getAttribute("data-photo-id") || "",
      url: button.getAttribute("data-photo-url") || "",
      description: button.getAttribute("data-photo-description") || "",
      aspectRatio: button.getAttribute("data-photo-aspect") || "free",
      commentCount: Number(button.getAttribute("data-photo-comment-count") || 0)
    }))
    .filter((item) => item.url);
}
