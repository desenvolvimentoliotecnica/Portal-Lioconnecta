import { renderEmptyState } from "./cards.js";
import { escapeHtml } from "./html.js";
import { resolveFeedMediaUrl, serializeGalleryImages } from "../services/feedMedia.js?v=0.21.4";
import { renderPostCommentComposer } from "./feedPostCommentComposer.js?v=0.21.4";
import { renderMentionBody, renderMentionDropdownMarkup } from "./feedMentions.js?v=0.21.7";
import { DATA_MODES, getRuntimeConfig } from "../core/runtimeConfig.js?v=0.21.4";
import { canInteractWithFeed } from "../services/portalPermissionService.js?v=0.17.0";
import { getStoredPortalSession } from "../services/portalAuthService.js?v=0.13.0";

const PHOTO_ACTION_LABEL = "Adicionar fotos";

function renderComposer(composer) {
  const photoEnabled = Boolean(composer.photoEnabled);

  return `
    <form class="feed-composer-card feed-composer-form" data-action="submit-feed-post">
      <div class="feed-composer-head">
        <div>
          <h2>${escapeHtml(composer.title)}</h2>
          <p>Compartilhe uma atualização com colegas, times e lideranças.</p>
        </div>
        <button class="feed-composer-submit" type="submit" data-action="submit-feed-post">Publicar</button>
      </div>
      <div class="feed-composer-box" data-analytics="composer.focus">
        <div class="avatar" aria-hidden="true"><i class="fa-solid fa-user"></i></div>
        <div>
          <div class="feed-composer-mention-field feed-mention-field">
            <div
              class="feed-mention-editor feed-composer-input feed-composer-textarea"
              contenteditable="true"
              role="textbox"
              aria-multiline="true"
              spellcheck="true"
              data-placeholder="${escapeHtml(composer.placeholder)}"
              aria-label="${escapeHtml(composer.placeholder)}"
            ></div>
            ${renderMentionDropdownMarkup()}
          </div>
          <div data-feed-attachments></div>
        </div>
      </div>
      <div class="feed-composer-actions">
        <button
          class="feed-action-chip ${photoEnabled ? "is-enabled" : ""}"
          type="button"
          data-analytics="composer.action"
          data-analytics-label="${escapeHtml(PHOTO_ACTION_LABEL)}"
          ${photoEnabled ? `data-action="open-feed-photo-modal"` : ""}
          ${photoEnabled ? "" : "disabled"}
          ${photoEnabled ? "" : `title="Disponivel em uma proxima versao"`}
        >
          <i class="fa-regular fa-image" aria-hidden="true"></i>
          <span>${escapeHtml(PHOTO_ACTION_LABEL)}</span>
        </button>
      </div>
    </form>
  `;
}

function renderPostGallery(post) {
  const images = Array.isArray(post.images) && post.images.length
    ? post.images
    : (post.image ? [{ url: post.image, description: post.imageAlt || "", aspectRatio: "free" }] : []);

  if (!images.length) {
    return "";
  }

  const total = images.length;
  const visible = images.map((image) => ({
    ...image,
    resolvedUrl: resolveFeedMediaUrl(image.url)
  })).slice(0, 4);

  return `
    <div class="post-gallery post-gallery--${Math.min(total, 4)}" data-gallery-count="${total}" data-feed-gallery="${serializeGalleryImages(images)}">
      ${visible.map((image, index) => `
        <button
          type="button"
          class="post-gallery__item ${index === 3 && total > 4 ? "post-gallery__item--more" : ""}"
          data-aspect="${escapeHtml(image.aspectRatio || "free")}"
          data-action="open-feed-photo-viewer"
          data-photo-index="${index}"
          data-photo-id="${escapeHtml(image.id || "")}"
          data-photo-url="${escapeHtml(image.url || "")}"
          data-photo-description="${escapeHtml(image.description || "")}"
          data-photo-aspect="${escapeHtml(image.aspectRatio || "free")}"
          data-photo-comment-count="${Number(image.commentCount || 0)}"
          aria-label="${escapeHtml(image.description || `Abrir foto ${index + 1}`)}"
        >
          <img src="${escapeHtml(image.resolvedUrl)}" alt="${escapeHtml(image.description || post.author)}" loading="lazy">
          ${image.description ? `<span class="post-gallery__caption">${escapeHtml(image.description)}</span>` : ""}
          ${Number(image.commentCount) > 0 ? `<span class="post-gallery__comment-badge">${escapeHtml(String(image.commentCount))}</span>` : ""}
          ${index === 3 && total > 4 ? `<span class="post-gallery__more">+${total - 4}</span>` : ""}
        </button>
      `).join("")}
    </div>
  `;
}

function renderReactionBubble(type, iconClass, label) {
  return `
    <span class="reaction-bubble ${type}" aria-label="${escapeHtml(label)}">
      <i class="${escapeHtml(iconClass)}" aria-hidden="true"></i>
    </span>
  `;
}

function formatLikeLabel(count) {
  const total = Number(count ?? 0);
  if (total <= 0) {
    return "Nenhuma curtida ainda";
  }
  return total === 1 ? "1 curtida" : `${total} curtidas`;
}

function renderReactionSummary(post) {
  const count = Number(post.reactions ?? 0);

  return `
    <div class="post-reactions">
      ${count > 0 ? renderReactionBubble("like", "fa-solid fa-thumbs-up", "Curtir") : ""}
      <span data-post-reactions-count>${escapeHtml(formatLikeLabel(count))}</span>
    </div>
  `;
}

function canShowPostMenu(post, currentUserId) {
  return getRuntimeConfig().dataMode === DATA_MODES.API
    && canInteractWithFeed()
    && post.source === "UserPost"
    && post.authorUserId
    && currentUserId
    && post.authorUserId === currentUserId;
}

function renderPostMoreMenu(post, currentUserId) {
  if (!canShowPostMenu(post, currentUserId)) {
    return "";
  }

  return `
    <div class="post-more-menu">
      <button
        type="button"
        class="post-more-trigger"
        data-action="toggle-post-menu"
        aria-label="Mais opcoes da publicacao"
        aria-haspopup="menu"
        aria-expanded="false"
      >•••</button>
      <div class="post-more-dropdown" hidden role="menu" aria-label="Acoes da publicacao">
        <button
          type="button"
          class="post-more-option post-more-option--danger"
          role="menuitem"
          data-action="delete-feed-post"
          data-post-id="${escapeHtml(post.postId)}"
        >Excluir</button>
      </div>
    </div>
  `;
}

function formatSharesLabel(count) {
  const total = Number(count ?? 0);
  return total === 1 ? "1 compartilhamento" : `${total} compartilhamentos`;
}

function canSharePost(post, currentUserId) {
  return getRuntimeConfig().dataMode === DATA_MODES.API
    && canInteractWithFeed()
    && post.source === "UserPost"
    && post.authorUserId
    && currentUserId
    && post.authorUserId !== currentUserId;
}

function canSavePost(post) {
  return getRuntimeConfig().dataMode === DATA_MODES.API
    && Boolean(getStoredPortalSession()?.token)
    && Boolean(post.postId && post.source);
}

function renderSavedPostMenu(post) {
  if (!canSavePost(post)) {
    return "";
  }

  return `
    <div class="post-more-menu">
      <button
        type="button"
        class="post-more-trigger"
        data-action="toggle-post-menu"
        aria-label="Mais opcoes da publicacao"
        aria-haspopup="menu"
        aria-expanded="false"
      >•••</button>
      <div class="post-more-dropdown" hidden role="menu" aria-label="Acoes da publicacao salva">
        <button
          type="button"
          class="post-more-option"
          role="menuitem"
          data-action="remove-saved-feed-item"
          data-feed-item-id="${escapeHtml(post.postId)}"
          data-feed-source="${escapeHtml(post.source)}"
        >Remover dos salvos</button>
      </div>
    </div>
  `;
}

function renderPost(post, { currentUserId = "", savedList = false } = {}) {
  const canLike = Boolean(post.postId && post.source);
  const canShare = canSharePost(post, currentUserId);
  const canSave = canSavePost(post);
  const postActions = [
    { label: "Curtir", action: canLike ? "toggle-feed-like" : "", active: post.hasLiked },
    { label: "Compartilhar", action: canShare ? "toggle-feed-share" : "", active: post.hasShared },
    { label: "Salvar", action: canSave ? "toggle-feed-save" : "", active: post.hasSaved }
  ];

  const commentsCount = Number(post.commentsCount ?? 0);
  const commentsLabel = commentsCount === 1 ? "1 comentário" : `${commentsCount} comentários`;

  return `
    <article class="post" data-post-id="${escapeHtml(post.postId)}" data-communication-id="${escapeHtml(post.communicationId)}">
      <div class="post-head">
        <div class="post-author">
          <div class="avatar" aria-hidden="true"><i class="fa-solid fa-user"></i></div>
          <div class="post-author-copy">
            <strong>${escapeHtml(post.author)}</strong>
            <span>${escapeHtml(post.area)} • ${escapeHtml(post.timeAgo)}</span>
          </div>
        </div>
        ${savedList ? renderSavedPostMenu(post) : renderPostMoreMenu(post, currentUserId)}
      </div>

      <div class="post-body">
        ${post.text ? `<p class="post-text">${renderMentionBody({ text: post.text, mentions: post.mentions })}</p>` : ""}
        ${post.highlightTitle ? `
          <div class="post-highlight">
            <strong>${escapeHtml(post.highlightTitle)}</strong>
            <span>${escapeHtml(post.highlightText)}</span>
          </div>
        ` : ""}
        ${renderPostGallery(post)}
      </div>

      <div class="post-stats">
        ${renderReactionSummary(post)}
        <div data-post-stats-meta data-comments-count="${commentsCount}" data-shares-count="${Number(post.sharesCount ?? 0)}">${escapeHtml(commentsLabel)} • ${escapeHtml(formatSharesLabel(post.sharesCount))}</div>
      </div>

      <div class="post-actions">
        ${postActions.map((item) => `
          <button
            type="button"
            class="${item.active ? "is-active" : ""}"
            data-post-author="${escapeHtml(post.author)}"
            ${item.action ? `data-action="${escapeHtml(item.action)}"` : ""}
            ${canLike && item.label === "Curtir" ? `data-feed-item-id="${escapeHtml(post.postId)}" data-feed-source="${escapeHtml(post.source)}"` : ""}
            ${canShare && item.label === "Compartilhar" ? `data-feed-item-id="${escapeHtml(post.postId)}" data-feed-source="${escapeHtml(post.source)}"` : ""}
            ${canSave && item.label === "Salvar" ? `data-feed-item-id="${escapeHtml(post.postId)}" data-feed-source="${escapeHtml(post.source)}"` : ""}
            ${item.label === "Curtir" || item.label === "Compartilhar" || item.label === "Salvar" ? `aria-pressed="${item.active ? "true" : "false"}"` : ""}
            data-analytics="post.action"
            data-analytics-label="${escapeHtml(post.author)}:${escapeHtml(item.label)}"
          >${escapeHtml(item.label)}</button>
        `).join("")}
      </div>

      ${post.comments.length ? `
        <div class="post-comments">
          ${post.comments.map((comment) => `
            <div class="post-comment-item" data-comment-id="${escapeHtml(comment.id || "")}">
              <div class="avatar avatar--small" aria-hidden="true"><i class="fa-solid fa-user"></i></div>
              <div class="post-comment-bubble">
                <strong>${escapeHtml(comment.author)}</strong>
                <span class="post-comment-text">${renderMentionBody(comment)}</span>
              </div>
            </div>
          `).join("")}
        </div>
      ` : ""}

      ${renderPostCommentComposer(post)}
    </article>
  `;
}

export function renderSavedFeed(feed, { currentUserId = "" } = {}) {
  const posts = Array.isArray(feed.posts) ? feed.posts : [];

  return `
    <section class="card feed-card feed-card--saved">
      <div class="card-header">${escapeHtml(feed.title || "ITENS SALVOS")}</div>
      <div class="feed-list">
        ${posts.length
          ? posts.map((post) => renderPost(post, { currentUserId, savedList: true })).join("")
          : renderEmptyState(
            "Nenhum item salvo ainda.",
            "Use o botao Salvar em posts e comunicados para acompanhar o que importa para voce."
          )}
      </div>
    </section>
  `;
}

export function renderFeed(feed, composer, { currentUserId = "" } = {}) {
  const posts = Array.isArray(feed.posts) ? feed.posts : [];
  const showComposer = composer?.enabled !== false;

  return `
    <section class="card feed-card">
      <div class="card-header">${escapeHtml(feed.title)}</div>
      ${showComposer ? renderComposer(composer) : ""}
      <div class="feed-list">
        ${posts.length
          ? posts.map((post) => renderPost(post, { currentUserId })).join("")
          : renderEmptyState(
            "Ainda não há posts publicados.",
            "Assim que a comunicação interna ou os times compartilharem novidades, o mural aparecerá aqui."
          )}
      </div>
    </section>
  `;
}
