import { escapeHtml } from "./html.js";
import { DATA_MODES, getRuntimeConfig } from "../core/runtimeConfig.js?v=0.21.4";
import { createFeedMediaComment, getFeedMediaComments } from "../services/feedService.js?v=0.21.4";
import { getPortalAuthHeaders } from "../services/portalAuthService.js?v=0.13.0";
import { showToast } from "../core/feedback.js?v=0.16.0";
import { canInteractWithFeed } from "../services/portalPermissionService.js?v=0.17.0";
import { readGalleryImages, resolveFeedMediaUrl } from "../services/feedMedia.js?v=0.21.4";

let modalRoot = null;
let viewerState = null;
let viewerActionsBound = false;

function formatCommentTime(value) {
  if (!value) {
    return "agora";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "agora";
  }

  return date.toLocaleString("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    hour: "2-digit",
    minute: "2-digit"
  });
}

function canCommentOnPhotos() {
  return getRuntimeConfig().dataMode === DATA_MODES.API && canInteractWithFeed();
}

function getCurrentPhoto() {
  if (!viewerState?.images?.length) {
    return null;
  }

  return viewerState.images[viewerState.index] || null;
}

function closeModal() {
  if (modalRoot) {
    modalRoot.remove();
    modalRoot = null;
  }

  viewerState = null;
  document.body.classList.remove("feed-photo-viewer-open");
}

function renderComments(comments = [], loading = false) {
  if (loading) {
    return `<p class="feed-photo-viewer__loading">Carregando comentarios...</p>`;
  }

  if (!comments.length) {
    return `<p class="feed-photo-viewer__empty">Nenhum comentario nesta foto ainda.</p>`;
  }

  return comments.map((comment) => `
    <article class="feed-photo-viewer__comment">
      <div class="avatar avatar--small" aria-hidden="true"><i class="fa-solid fa-user"></i></div>
      <div class="feed-photo-viewer__comment-copy">
        <strong>${escapeHtml(comment.author || "Colaborador")}</strong>
        <span class="feed-photo-viewer__comment-time">${escapeHtml(formatCommentTime(comment.createdAtUtc))}</span>
        <p>${escapeHtml(comment.text || "")}</p>
      </div>
    </article>
  `).join("");
}

function renderThumbnails() {
  if (!viewerState || viewerState.images.length <= 1) {
    return "";
  }

  return `
    <div class="feed-photo-viewer__thumbs" role="tablist" aria-label="Fotos da publicacao">
      ${viewerState.images.map((image, index) => `
        <button
          type="button"
          class="feed-photo-viewer__thumb ${index === viewerState.index ? "is-active" : ""}"
          data-action="set-feed-photo-viewer-index"
          data-photo-index="${index}"
          aria-label="Foto ${index + 1}"
        >
          <img src="${escapeHtml(image.resolvedUrl || resolveFeedMediaUrl(image.url))}" alt="">
        </button>
      `).join("")}
    </div>
  `;
}

function renderCommentForm(photo) {
  if (!photo?.id) {
    return `
      <p class="feed-photo-viewer__notice">
        Comentarios por foto estao disponiveis apenas em publicacoes com fotos enviadas pelo feed.
      </p>
    `;
  }

  if (!canCommentOnPhotos()) {
    return `
      <p class="feed-photo-viewer__notice">
        Faca login com permissao no feed para comentar fotos.
      </p>
    `;
  }

  return `
    <form class="feed-photo-viewer__comment-form" data-action="submit-feed-photo-comment">
      <label for="feed-photo-comment-input">Comentar foto</label>
      <textarea
        id="feed-photo-comment-input"
        class="feed-photo-viewer__comment-input"
        maxlength="1000"
        rows="2"
        placeholder="Escreva um comentario sobre esta foto..."
        required
      ></textarea>
      <button type="submit" class="feed-composer-submit">Comentar foto</button>
    </form>
  `;
}

function updateViewerUi() {
  if (!modalRoot || !viewerState) {
    return;
  }

  const photo = getCurrentPhoto();
  if (!photo) {
    return;
  }

  const imageEl = modalRoot.querySelector("[data-feed-photo-viewer-image]");
  const captionEl = modalRoot.querySelector("[data-feed-photo-viewer-caption]");
  const counterEl = modalRoot.querySelector("[data-feed-photo-viewer-counter]");
  const commentsEl = modalRoot.querySelector("[data-feed-photo-viewer-comments]");
  const formSlot = modalRoot.querySelector("[data-feed-photo-viewer-form]");
  const thumbsSlot = modalRoot.querySelector("[data-feed-photo-viewer-thumbs]");
  const prevButton = modalRoot.querySelector("[data-action='prev-feed-photo-viewer']");
  const nextButton = modalRoot.querySelector("[data-action='next-feed-photo-viewer']");

  if (imageEl) {
    imageEl.src = photo.resolvedUrl || resolveFeedMediaUrl(photo.url);
    imageEl.alt = photo.description || "Foto da publicacao";
  }

  if (captionEl) {
    captionEl.textContent = photo.description || "";
    captionEl.hidden = !photo.description;
  }

  if (counterEl) {
    counterEl.textContent = viewerState.images.length > 1
      ? `Foto ${viewerState.index + 1} de ${viewerState.images.length}`
      : "Foto da publicacao";
  }

  if (commentsEl) {
    commentsEl.innerHTML = renderComments(viewerState.comments, viewerState.loadingComments);
  }

  if (formSlot) {
    formSlot.innerHTML = renderCommentForm(photo);
  }

  if (thumbsSlot) {
    thumbsSlot.innerHTML = renderThumbnails();
  }

  if (prevButton) {
    prevButton.disabled = viewerState.index <= 0;
  }

  if (nextButton) {
    nextButton.disabled = viewerState.index >= viewerState.images.length - 1;
  }
}

async function loadCommentsForCurrentPhoto() {
  const photo = getCurrentPhoto();
  if (!modalRoot || !photo) {
    return;
  }

  viewerState.loadingComments = true;
  viewerState.comments = [];
  updateViewerUi();

  if (!photo.id || getRuntimeConfig().dataMode !== DATA_MODES.API) {
    viewerState.loadingComments = false;
    viewerState.comments = [];
    updateViewerUi();
    return;
  }

  try {
    const payload = await getFeedMediaComments(photo.id, {
      headers: getPortalAuthHeaders()
    });
    viewerState.comments = Array.isArray(payload?.items) ? payload.items : [];
  } catch (error) {
    console.error("Falha ao carregar comentarios da foto.", error);
    viewerState.comments = [];
    showToast("Nao foi possivel carregar os comentarios desta foto.", "danger");
  } finally {
    viewerState.loadingComments = false;
    updateViewerUi();
  }
}

async function setViewerIndex(index) {
  if (!viewerState) {
    return;
  }

  const nextIndex = Math.max(0, Math.min(index, viewerState.images.length - 1));
  if (nextIndex === viewerState.index) {
    return;
  }

  viewerState.index = nextIndex;
  updateViewerUi();
  await loadCommentsForCurrentPhoto();
}

function renderModalShell() {
  return `
    <div class="feed-photo-viewer" role="dialog" aria-modal="true" aria-labelledby="feed-photo-viewer-title">
      <div class="feed-photo-viewer__backdrop" data-action="close-feed-photo-viewer"></div>
      <div class="feed-photo-viewer__panel">
        <header class="feed-photo-viewer__header">
          <div>
            <h2 id="feed-photo-viewer-title">Visualizar foto</h2>
            <p data-feed-photo-viewer-counter>Foto da publicacao</p>
          </div>
          <button type="button" class="feed-photo-viewer__close" data-action="close-feed-photo-viewer" aria-label="Fechar">
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
        </header>
        <div class="feed-photo-viewer__body">
          <section class="feed-photo-viewer__stage" aria-label="Imagem ampliada">
            <button type="button" class="feed-photo-viewer__nav feed-photo-viewer__nav--prev" data-action="prev-feed-photo-viewer" aria-label="Foto anterior">
              <i class="fa-solid fa-chevron-left" aria-hidden="true"></i>
            </button>
            <figure class="feed-photo-viewer__figure">
              <img data-feed-photo-viewer-image src="" alt="">
              <figcaption data-feed-photo-viewer-caption hidden></figcaption>
            </figure>
            <button type="button" class="feed-photo-viewer__nav feed-photo-viewer__nav--next" data-action="next-feed-photo-viewer" aria-label="Proxima foto">
              <i class="fa-solid fa-chevron-right" aria-hidden="true"></i>
            </button>
            <div data-feed-photo-viewer-thumbs></div>
          </section>
          <aside class="feed-photo-viewer__comments-panel" aria-label="Comentarios da foto">
            <div class="feed-photo-viewer__comments-head">
              <h3>Comentarios da foto</h3>
              <p>Separados dos comentarios do post.</p>
            </div>
            <div class="feed-photo-viewer__comments-list" data-feed-photo-viewer-comments></div>
            <div data-feed-photo-viewer-form></div>
          </aside>
        </div>
      </div>
    </div>
  `;
}

async function submitComment(form) {
  const photo = getCurrentPhoto();
  if (!photo?.id || !form) {
    return;
  }

  const textarea = form.querySelector("textarea");
  const text = String(textarea?.value || "").trim();
  const submitButton = form.querySelector("button[type='submit']");

  if (!text) {
    showToast("Escreva um comentario antes de enviar.", "danger");
    return;
  }

  if (submitButton) {
    submitButton.disabled = true;
  }

  try {
    const created = await createFeedMediaComment(photo.id, text, {
      headers: getPortalAuthHeaders()
    });

    viewerState.comments = [...viewerState.comments, created];
    photo.commentCount = Number(photo.commentCount || 0) + 1;
    if (textarea) {
      textarea.value = "";
    }
    updateViewerUi();
    showToast("Comentario adicionado a foto.", "success");
  } catch (error) {
    console.error("Falha ao comentar foto.", error);
    showToast("Nao foi possivel comentar esta foto agora.", "danger");
  } finally {
    if (submitButton) {
      submitButton.disabled = false;
    }
  }
}

function bindModalActions() {
  if (!modalRoot) {
    return;
  }

  modalRoot.addEventListener("click", async (event) => {
    const target = event.target.closest("[data-action]");
    if (!target) {
      return;
    }

    const action = target.getAttribute("data-action");

    if (action === "close-feed-photo-viewer") {
      closeModal();
      return;
    }

    if (action === "prev-feed-photo-viewer") {
      await setViewerIndex(viewerState.index - 1);
      return;
    }

    if (action === "next-feed-photo-viewer") {
      await setViewerIndex(viewerState.index + 1);
      return;
    }

    if (action === "set-feed-photo-viewer-index") {
      const index = Number(target.getAttribute("data-photo-index") || 0);
      await setViewerIndex(index);
    }
  });

  modalRoot.addEventListener("submit", async (event) => {
    const form = event.target.closest("[data-action='submit-feed-photo-comment']");
    if (!form) {
      return;
    }

    event.preventDefault();
    await submitComment(form);
  });
}

export async function openFeedPhotoViewer(images = [], startIndex = 0) {
  const normalizedImages = getResolvedImages(
    Array.isArray(images) ? images.filter((item) => item?.url) : []
  );

  if (!normalizedImages.length) {
    showToast("Nao foi possivel abrir esta foto.", "danger");
    return;
  }

  closeModal();

  viewerState = {
    images: normalizedImages,
    index: Math.max(0, Math.min(startIndex, normalizedImages.length - 1)),
    comments: [],
    loadingComments: false
  };

  document.body.insertAdjacentHTML("beforeend", renderModalShell());
  modalRoot = document.querySelector(".feed-photo-viewer");
  document.body.classList.add("feed-photo-viewer-open");

  bindModalActions();
  updateViewerUi();
  await loadCommentsForCurrentPhoto();
}

function getResolvedImages(images = []) {
  return images.map((image) => ({
    ...image,
    resolvedUrl: resolveFeedMediaUrl(image.url)
  }));
}

export function bindFeedPhotoViewerActions() {
  if (viewerActionsBound) {
    return;
  }

  viewerActionsBound = true;

  document.addEventListener("click", (event) => {
    const trigger = event.target.closest("[data-action='open-feed-photo-viewer'], .post-gallery__item");
    if (!trigger) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    const gallery = trigger.closest(".post-gallery[data-feed-gallery]");
    if (!gallery) {
      return;
    }

    const images = readGalleryImages(gallery);
    const index = Number(trigger.getAttribute("data-photo-index") || 0);

    if (!images.length) {
      showToast("Nao foi possivel abrir esta foto.", "danger");
      return;
    }

    openFeedPhotoViewer(images, index).catch((error) => {
      console.error("Falha ao abrir visualizador de fotos.", error);
      showToast("Nao foi possivel abrir a foto.", "danger");
    });
  }, true);

  document.addEventListener("keydown", (event) => {
    if (!modalRoot || event.key !== "Escape") {
      return;
    }

    closeModal();
  });
}
