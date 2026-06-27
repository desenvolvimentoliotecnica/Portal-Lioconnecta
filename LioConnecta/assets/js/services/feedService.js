import { getJson, postFormData, postJson, deleteJson } from "./apiClient.js";
import { unwrapDataEnvelope } from "./apiClient.js";
import { mapFeedViewModel } from "../mappers/feedMapper.js";
import { validateFeedContract } from "../validators/feedValidator.js";
import { DATA_MODES, getRuntimeConfig, resolveApiEndpoint, resolveDataSource, usesEnvelope } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders } from "./portalAuthService.js";
import { DEFAULT_FEED_TITLE } from "../view-models/defaults.js";

function formatTimeAgo(value) {
  if (!value) {
    return "agora";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "agora";
  }

  const diffMs = Date.now() - date.getTime();
  const minutes = Math.floor(diffMs / 60000);
  if (minutes < 1) {
    return "agora";
  }
  if (minutes < 60) {
    return `há ${minutes} min`;
  }

  const hours = Math.floor(minutes / 60);
  if (hours < 24) {
    return `há ${hours} h`;
  }

  const days = Math.floor(hours / 24);
  return `há ${days} dia${days > 1 ? "s" : ""}`;
}

function mapMentions(items = []) {
  const list = Array.isArray(items) ? items : [];
  return list.map((mention) => ({
    userId: String(mention.userId || mention.UserId || mention.user_id || ""),
    displayName: String(mention.displayName || mention.DisplayName || mention.display_name || "")
  })).filter((mention) => mention.userId && mention.displayName);
}

function mapPostComment(item = {}) {
  return {
    id: String(item.id || ""),
    author: String(item.author || "Colaborador"),
    text: String(item.text || ""),
    createdAtUtc: item.createdAtUtc || item.created_at_utc || null,
    mentions: mapMentions(item.mentions || item.Mentions)
  };
}

function mapFeedItemToPost(item = {}) {
  const source = String(item.source || "");
  const isCommunication = source === "Communication";
  const media = Array.isArray(item.media) ? item.media : [];
  const images = media.map((entry) => ({
    id: String(entry.id || ""),
    url: String(entry.url || ""),
    description: String(entry.description || ""),
    aspectRatio: String(entry.aspectRatio || "free"),
    commentCount: Number(entry.commentCount ?? 0)
  })).filter((entry) => entry.url);

  return {
    postId: String(item.id || ""),
    source,
    communicationId: isCommunication ? String(item.communicationId || item.id || "") : "",
    slug: "",
    author: String(item.author || "Colaborador"),
    authorUserId: String(item.authorUserId || item.AuthorUserId || item.author_user_id || ""),
    area: String(item.area || "Companhia"),
    timeAgo: formatTimeAgo(item.publishedAtUtc),
    text: String(item.text || ""),
    mentions: mapMentions(item.mentions || item.Mentions),
    highlightTitle: String(item.highlightTitle || ""),
    highlightText: String(item.highlightText || ""),
    image: String(item.imageUrl || images[0]?.url || ""),
    imageAlt: String(item.highlightTitle || images[0]?.description || item.author || "Publicacao"),
    images,
    reactions: Number(item.likeCount ?? 0),
    hasLiked: Boolean(item.hasLiked),
    commentsCount: Number(item.commentCount ?? item.commentsCount ?? 0),
    sharesCount: Number(item.shareCount ?? item.sharesCount ?? 0),
    hasShared: Boolean(item.hasShared),
    hasSaved: Boolean(item.hasSaved),
    comments: Array.isArray(item.comments)
      ? item.comments.map(mapPostComment).filter((comment) => comment.text)
      : []
  };
}

function mapApiFeedPayload(payload = {}) {
  const items = Array.isArray(payload.items) ? payload.items : [];

  return {
    title: String(payload.title || DEFAULT_FEED_TITLE),
    posts: items.map(mapFeedItemToPost).filter((post) => post.text || post.image || post.images.length)
  };
}

export async function getFeedData() {
  const config = getRuntimeConfig();

  if (config.dataMode === DATA_MODES.API) {
    try {
      const payload = await getJson(resolveApiEndpoint("feed"), {
        headers: getPortalAuthHeaders()
      });
      return mapFeedViewModel(mapApiFeedPayload(payload), { allowDefaults: false });
    } catch (error) {
      console.error("Falha ao carregar feed do portal.", error);
      return mapFeedViewModel({ title: DEFAULT_FEED_TITLE, posts: [] }, { allowDefaults: false });
    }
  }

  const rawPayload = await getJson(resolveDataSource("feed"));
  const raw = usesEnvelope(config.dataMode) ? unwrapDataEnvelope(rawPayload) : rawPayload;
  validateFeedContract(raw);
  return mapFeedViewModel(raw);
}

export async function uploadFeedAsset(file, options = {}) {
  const formData = new FormData();
  formData.append("file", file, file.name || "feed-photo.jpg");
  return postFormData(resolveApiEndpoint("feedAssets"), formData, options);
}

export async function createFeedPost(payload, options = {}) {
  const body = typeof payload === "string"
    ? { text: payload, media: [], mentionedUserIds: [] }
    : {
      text: String(payload?.text || ""),
      media: Array.isArray(payload?.media) ? payload.media : [],
      mentionedUserIds: Array.isArray(payload?.mentionedUserIds) ? payload.mentionedUserIds : []
    };

  const response = await postJson(resolveApiEndpoint("feed"), body, options);
  return mapFeedItemToPost(response?.item || response);
}

export async function toggleFeedLike(itemId, source, options = {}) {
  return postJson(`${resolveApiEndpoint("feed")}/${encodeURIComponent(itemId)}/like`, { source }, options);
}

export async function toggleFeedShare(itemId, source, options = {}) {
  return postJson(`${resolveApiEndpoint("feed")}/${encodeURIComponent(itemId)}/share`, { source }, options);
}

export async function toggleFeedSave(itemId, source, options = {}) {
  return postJson(`${resolveApiEndpoint("feed")}/${encodeURIComponent(itemId)}/save`, { source }, options);
}

export async function getSavedFeedData() {
  const config = getRuntimeConfig();

  if (config.dataMode !== DATA_MODES.API) {
    return mapFeedViewModel({ title: "ITENS SALVOS", posts: [] }, { allowDefaults: false });
  }

  try {
    const payload = await getJson(`${resolveApiEndpoint("feed")}/saved`, {
      headers: getPortalAuthHeaders()
    });
    return mapFeedViewModel(mapApiFeedPayload(payload), { allowDefaults: false });
  } catch (error) {
    console.error("Falha ao carregar itens salvos.", error);
    return mapFeedViewModel({ title: "ITENS SALVOS", posts: [] }, { allowDefaults: false });
  }
}

export async function getFeedMediaComments(mediaId, options = {}) {
  return getJson(`${resolveApiEndpoint("feed")}/media/${encodeURIComponent(mediaId)}/comments`, options);
}

export async function createFeedMediaComment(mediaId, text, options = {}) {
  const payload = await postJson(
    `${resolveApiEndpoint("feed")}/media/${encodeURIComponent(mediaId)}/comments`,
    { text },
    options
  );
  return mapMediaComment(payload?.item || payload);
}

export async function getFeedPostComments(postId, options = {}) {
  return getJson(`${resolveApiEndpoint("feed")}/${encodeURIComponent(postId)}/comments`, options);
}

export async function createFeedPostComment(postId, payload, options = {}) {
  const body = typeof payload === "string"
    ? { text: payload, mentionedUserIds: [] }
    : {
      text: String(payload?.text || ""),
      mentionedUserIds: Array.isArray(payload?.mentionedUserIds) ? payload.mentionedUserIds : []
    };

  const response = await postJson(
    `${resolveApiEndpoint("feed")}/${encodeURIComponent(postId)}/comments`,
    body,
    options
  );
  return mapPostComment(response?.item || response);
}

export async function suggestFeedMentions(query, options = {}) {
  const params = new URLSearchParams({ q: String(query || "") });
  return getJson(`${resolveApiEndpoint("feed")}/mentions/suggest?${params.toString()}`, options);
}

export async function deleteFeedPost(postId, options = {}) {
  return deleteJson(`${resolveApiEndpoint("feed")}/${encodeURIComponent(postId)}`, options);
}

function mapMediaComment(item = {}) {
  return {
    id: String(item.id || ""),
    author: String(item.author || "Colaborador"),
    text: String(item.text || ""),
    createdAtUtc: item.createdAtUtc || null
  };
}

function formatLikeLabel(count) {
  const total = Number(count ?? 0);
  if (total <= 0) {
    return "Nenhuma curtida ainda";
  }
  return total === 1 ? "1 curtida" : `${total} curtidas`;
}

function formatSharesLabel(count) {
  const total = Number(count ?? 0);
  return total === 1 ? "1 compartilhamento" : `${total} compartilhamentos`;
}

export function updateFeedShareUi(scope, result) {
  if (!scope || !result) {
    return;
  }

  const shareButton = scope.querySelector("[data-action='toggle-feed-share']");
  const statsRow = scope.querySelector(".post-stats > div:last-child");
  const shareCount = Number(result.shareCount ?? 0);

  if (shareButton) {
    shareButton.classList.toggle("is-active", Boolean(result.hasShared));
    shareButton.setAttribute("aria-pressed", result.hasShared ? "true" : "false");
  }

  if (statsRow) {
    const commentsCount = Number(statsRow.getAttribute("data-comments-count") ?? 0);
    const commentsLabel = commentsCount === 1 ? "1 comentário" : `${commentsCount} comentários`;
    statsRow.setAttribute("data-shares-count", String(shareCount));
    statsRow.textContent = `${commentsLabel} • ${formatSharesLabel(shareCount)}`;
  }
}

export function updateFeedSaveUi(scope, result) {
  if (!scope || !result) {
    return;
  }

  const saveButton = scope.querySelector("[data-action='toggle-feed-save']");
  if (saveButton) {
    saveButton.classList.toggle("is-active", Boolean(result.hasSaved));
    saveButton.setAttribute("aria-pressed", result.hasSaved ? "true" : "false");
  }
}

export function updatePostLikeState(post, result) {
  updateFeedLikeUi(post, result);
}

export function updateCommunicationLikeUi(scope, result) {
  updateFeedLikeUi(scope, result);
}

export function updateFeedLikeUi(scope, result) {
  if (!scope || !result) {
    return;
  }

  const likeButton = scope.querySelector("[data-action='toggle-feed-like'], [data-action='toggle-communication-like']");
  const reactionsLabel = scope.querySelector("[data-post-reactions-count], [data-communication-like-count]");
  const reactionsRow = scope.querySelector(".post-reactions");
  const likeCount = Number(result.likeCount ?? 0);

  if (likeButton) {
    likeButton.classList.toggle("is-active", Boolean(result.hasLiked));
    likeButton.setAttribute("aria-pressed", result.hasLiked ? "true" : "false");
  }

  if (reactionsLabel) {
    reactionsLabel.textContent = scope.classList.contains("communication-detail-card")
      ? String(likeCount)
      : formatLikeLabel(likeCount);
  }

  if (reactionsRow) {
    const existingBubble = reactionsRow.querySelector(".reaction-bubble.like");
    if (likeCount > 0 && !existingBubble) {
      reactionsRow.insertAdjacentHTML("afterbegin", `
        <span class="reaction-bubble like" aria-label="Curtir">
          <i class="fa-solid fa-thumbs-up" aria-hidden="true"></i>
        </span>
      `);
    }
    if (likeCount <= 0 && existingBubble) {
      existingBubble.remove();
    }
  }
}
