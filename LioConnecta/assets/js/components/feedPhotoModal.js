import { escapeHtml } from "./html.js";

const MAX_PHOTOS = 10;
const ASPECT_OPTIONS = [
  { id: "1:1", label: "1:1", ratio: 1 },
  { id: "16:9", label: "16:9", ratio: 16 / 9 },
  { id: "9:16", label: "9:16", ratio: 9 / 16 },
  { id: "free", label: "Livre", ratio: Number.NaN }
];

let modalRoot = null;
let cropper = null;
let currentObjectUrl = "";
let currentAspectRatio = "1:1";
let pendingPhotos = [];
let onPhotosChange = null;
let cropperLoadPromise = null;

function createId() {
  return `feed-photo-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

function ensureCropperLoaded() {
  if (window.Cropper) {
    return Promise.resolve();
  }

  if (!cropperLoadPromise) {
    cropperLoadPromise = new Promise((resolve, reject) => {
      if (!document.querySelector("link[data-feed-cropper-css]")) {
        const link = document.createElement("link");
        link.rel = "stylesheet";
        link.href = "https://cdnjs.cloudflare.com/ajax/libs/cropperjs/1.6.2/cropper.min.css";
        link.dataset.feedCropperCss = "true";
        document.head.appendChild(link);
      }

      const script = document.createElement("script");
      script.src = "https://cdnjs.cloudflare.com/ajax/libs/cropperjs/1.6.2/cropper.min.js";
      script.async = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error("Nao foi possivel carregar o editor de recorte."));
      document.body.appendChild(script);
    });
  }

  return cropperLoadPromise;
}

function destroyCropper() {
  if (cropper) {
    cropper.destroy();
    cropper = null;
  }

  if (currentObjectUrl) {
    URL.revokeObjectURL(currentObjectUrl);
    currentObjectUrl = "";
  }
}

function notifyChange() {
  if (typeof onPhotosChange === "function") {
    onPhotosChange(getPendingFeedPhotos());
  }

  document.querySelectorAll("[data-feed-attachments]").forEach((container) => {
    container.innerHTML = renderFeedComposerAttachments(getPendingFeedPhotos());
    bindComposerAttachmentActions(container);
  });
}

export function getPendingFeedPhotos() {
  return pendingPhotos.map((item) => ({ ...item }));
}

export function clearPendingFeedPhotos() {
  pendingPhotos.forEach((item) => {
    if (item.previewUrl) {
      URL.revokeObjectURL(item.previewUrl);
    }
  });
  pendingPhotos = [];
  notifyChange();
}

export function setFeedPhotosChangeListener(listener) {
  onPhotosChange = listener;
}

export function renderFeedComposerAttachments(photos = []) {
  if (!photos.length) {
    return "";
  }

  return `
    <div class="feed-composer-attachments" aria-label="Fotos anexadas">
      ${photos.map((photo) => `
        <figure class="feed-composer-attachment" data-photo-id="${escapeHtml(photo.id)}">
          <img src="${escapeHtml(photo.previewUrl)}" alt="${escapeHtml(photo.description || "Foto anexada")}">
          ${photo.description ? `<figcaption>${escapeHtml(photo.description)}</figcaption>` : ""}
          <button
            type="button"
            class="feed-composer-attachment-remove"
            data-action="remove-feed-photo"
            data-photo-id="${escapeHtml(photo.id)}"
            aria-label="Remover foto"
          >
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
        </figure>
      `).join("")}
    </div>
  `;
}

function bindComposerAttachmentActions(container) {
  container.querySelectorAll("[data-action='remove-feed-photo']").forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", () => {
      const photoId = button.getAttribute("data-photo-id") || "";
      removePendingPhoto(photoId);
    });
  });
}

function removePendingPhoto(photoId) {
  const index = pendingPhotos.findIndex((item) => item.id === photoId);
  if (index < 0) {
    return;
  }

  const [removed] = pendingPhotos.splice(index, 1);
  if (removed?.previewUrl) {
    URL.revokeObjectURL(removed.previewUrl);
  }

  notifyChange();
  renderModalQueue();
}

function getAspectRatioValue(aspectId) {
  const option = ASPECT_OPTIONS.find((item) => item.id === aspectId);
  return option?.ratio ?? 1;
}

function renderAspectButtons(activeId) {
  return ASPECT_OPTIONS.map((option) => `
    <button
      type="button"
      class="feed-photo-aspect-btn ${option.id === activeId ? "is-active" : ""}"
      data-action="set-feed-photo-aspect"
      data-aspect-id="${escapeHtml(option.id)}"
    >${escapeHtml(option.label)}</button>
  `).join("");
}

function renderModalQueue() {
  const queue = modalRoot?.querySelector("[data-feed-photo-queue]");
  if (!queue) {
    return;
  }

  queue.innerHTML = pendingPhotos.length
    ? pendingPhotos.map((photo) => `
        <button
          type="button"
          class="feed-photo-queue-item"
          data-photo-id="${escapeHtml(photo.id)}"
          data-action="remove-feed-photo"
          title="${escapeHtml(photo.description || "Foto anexada")}"
        >
          <img src="${escapeHtml(photo.previewUrl)}" alt="">
          <span>${escapeHtml(photo.description || "Sem descricao")}</span>
        </button>
      `).join("")
    : `<p class="feed-photo-queue-empty">Nenhuma foto adicionada ainda.</p>`;
}

function showCropStage(file) {
  if (!modalRoot) {
    return;
  }

  destroyCropper();

  const stage = modalRoot.querySelector("[data-feed-photo-stage]");
  if (!stage) {
    return;
  }

  currentObjectUrl = URL.createObjectURL(file);
  currentAspectRatio = "1:1";

  stage.innerHTML = `
    <div class="feed-photo-cropper-wrap">
      <img class="feed-photo-cropper-image" src="${escapeHtml(currentObjectUrl)}" alt="Recortar imagem">
    </div>
    <div class="feed-photo-editor-controls">
      <div class="feed-photo-aspect-group" role="group" aria-label="Proporcao do recorte">
        ${renderAspectButtons(currentAspectRatio)}
      </div>
      <label class="feed-photo-description">
        <span>Descricao da foto</span>
        <textarea
          class="feed-photo-description-input"
          maxlength="500"
          rows="2"
          placeholder="Opcional: descreva o contexto desta imagem"
        ></textarea>
      </label>
      <div class="feed-photo-editor-actions">
        <button type="button" class="feed-photo-secondary-btn" data-action="cancel-feed-photo-edit">Cancelar</button>
        <button type="button" class="feed-composer-submit" data-action="confirm-feed-photo">Adicionar foto</button>
      </div>
    </div>
  `;

  const image = stage.querySelector(".feed-photo-cropper-image");
  cropper = new window.Cropper(image, {
    aspectRatio: getAspectRatioValue(currentAspectRatio),
    viewMode: 1,
    autoCropArea: 1,
    responsive: true,
    background: false
  });
}

function showDropStage() {
  if (!modalRoot) {
    return;
  }

  destroyCropper();

  const stage = modalRoot.querySelector("[data-feed-photo-stage]");
  if (!stage) {
    return;
  }

  const remaining = MAX_PHOTOS - pendingPhotos.length;

  stage.innerHTML = `
    <div class="feed-photo-dropzone ${remaining <= 0 ? "is-disabled" : ""}" data-feed-photo-dropzone>
      <input
        type="file"
        class="feed-photo-file-input"
        accept="image/jpeg,image/png,image/webp,image/gif"
        multiple
        ${remaining <= 0 ? "disabled" : ""}
      >
      <div class="feed-photo-dropzone-copy">
        <i class="fa-regular fa-images" aria-hidden="true"></i>
        <strong>Arraste fotos aqui ou clique para selecionar</strong>
        <span>JPG, PNG, WEBP ou GIF. Ate ${MAX_PHOTOS} fotos por publicacao.</span>
        <span class="feed-photo-dropzone-count">${remaining} restante${remaining === 1 ? "" : "s"}</span>
      </div>
    </div>
  `;

  bindDropzone(stage);
}

function bindDropzone(stage) {
  const dropzone = stage.querySelector("[data-feed-photo-dropzone]");
  const input = stage.querySelector(".feed-photo-file-input");
  if (!dropzone || !input) {
    return;
  }

  const openPicker = () => {
    if (pendingPhotos.length >= MAX_PHOTOS) {
      return;
    }
    input.click();
  };

  dropzone.addEventListener("click", (event) => {
    if (event.target.closest("[data-action]")) {
      return;
    }
    openPicker();
  });

  input.addEventListener("change", () => {
    const files = Array.from(input.files || []);
    if (!files.length) {
      return;
    }

    showCropStage(files[0]);
    input.value = "";
  });

  ["dragenter", "dragover"].forEach((eventName) => {
    dropzone.addEventListener(eventName, (event) => {
      event.preventDefault();
      dropzone.classList.add("is-dragover");
    });
  });

  ["dragleave", "drop"].forEach((eventName) => {
    dropzone.addEventListener(eventName, (event) => {
      event.preventDefault();
      dropzone.classList.remove("is-dragover");
    });
  });

  dropzone.addEventListener("drop", (event) => {
    const files = Array.from(event.dataTransfer?.files || []).filter((file) => file.type.startsWith("image/"));
    if (!files.length || pendingPhotos.length >= MAX_PHOTOS) {
      return;
    }

    showCropStage(files[0]);
  });
}

function closeModal() {
  destroyCropper();
  if (modalRoot) {
    modalRoot.remove();
    modalRoot = null;
  }
  document.body.classList.remove("feed-photo-modal-open");
}

async function confirmCurrentPhoto() {
  if (!cropper || !modalRoot) {
    return;
  }

  if (pendingPhotos.length >= MAX_PHOTOS) {
    return;
  }

  const description = modalRoot.querySelector(".feed-photo-description-input")?.value?.trim() || "";
  const canvas = cropper.getCroppedCanvas({
    maxWidth: 2048,
    maxHeight: 2048,
    imageSmoothingQuality: "high"
  });

  if (!canvas) {
    return;
  }

  const blob = await new Promise((resolve) => {
    canvas.toBlob((value) => resolve(value), "image/jpeg", 0.9);
  });

  if (!blob) {
    return;
  }

  const previewUrl = URL.createObjectURL(blob);
  pendingPhotos.push({
    id: createId(),
    blob,
    previewUrl,
    description,
    aspectRatio: currentAspectRatio,
    fileName: `feed-${Date.now()}.jpg`
  });

  notifyChange();
  renderModalQueue();
  showDropStage();
}

function renderModalShell() {
  return `
    <div class="feed-photo-modal" role="dialog" aria-modal="true" aria-labelledby="feed-photo-modal-title">
      <div class="feed-photo-modal__backdrop" data-action="close-feed-photo-modal"></div>
      <div class="feed-photo-modal__panel">
        <header class="feed-photo-modal__header">
          <div>
            <h2 id="feed-photo-modal-title">Adicionar fotos</h2>
            <p>Recorte, descreva e anexe ate ${MAX_PHOTOS} imagens ao seu post.</p>
          </div>
          <button type="button" class="feed-photo-modal__close" data-action="close-feed-photo-modal" aria-label="Fechar">
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
        </header>
        <div class="feed-photo-modal__body">
          <aside class="feed-photo-modal__queue">
            <h3>Fotos da publicacao</h3>
            <div data-feed-photo-queue></div>
          </aside>
          <section class="feed-photo-modal__stage" data-feed-photo-stage></section>
        </div>
        <footer class="feed-photo-modal__footer">
          <button type="button" class="feed-photo-secondary-btn" data-action="close-feed-photo-modal">Cancelar</button>
          <button type="button" class="feed-composer-submit" data-action="finish-feed-photo-modal">Concluir</button>
        </footer>
      </div>
    </div>
  `;
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

    if (action === "close-feed-photo-modal") {
      closeModal();
      return;
    }

    if (action === "finish-feed-photo-modal") {
      closeModal();
      return;
    }

    if (action === "cancel-feed-photo-edit") {
      showDropStage();
      return;
    }

    if (action === "set-feed-photo-aspect") {
      currentAspectRatio = target.getAttribute("data-aspect-id") || "1:1";
      modalRoot.querySelectorAll("[data-action='set-feed-photo-aspect']").forEach((button) => {
        button.classList.toggle("is-active", button === target);
      });
      if (cropper) {
        const ratio = getAspectRatioValue(currentAspectRatio);
        cropper.setAspectRatio(Number.isNaN(ratio) ? Number.NaN : ratio);
      }
      return;
    }

    if (action === "confirm-feed-photo") {
      target.disabled = true;
      try {
        await confirmCurrentPhoto();
      } finally {
        target.disabled = false;
      }
      return;
    }

    if (action === "remove-feed-photo") {
      const photoId = target.getAttribute("data-photo-id") || "";
      removePendingPhoto(photoId);
    }
  });
}

export async function openFeedPhotoModal() {
  if (pendingPhotos.length >= MAX_PHOTOS) {
    return;
  }

  await ensureCropperLoaded();

  if (modalRoot) {
    closeModal();
  }

  document.body.insertAdjacentHTML("beforeend", renderModalShell());
  modalRoot = document.querySelector(".feed-photo-modal");
  document.body.classList.add("feed-photo-modal-open");

  bindModalActions();
  renderModalQueue();
  showDropStage();
}

let photoActionsBound = false;

export function bindFeedPhotoComposerActions() {
  document.querySelectorAll("[data-feed-attachments]").forEach((container) => {
    bindComposerAttachmentActions(container);
  });

  if (photoActionsBound) {
    return;
  }

  photoActionsBound = true;

  document.addEventListener("click", (event) => {
    const trigger = event.target.closest("[data-action='open-feed-photo-modal']");
    if (!trigger || trigger.disabled) {
      return;
    }

    event.preventDefault();
    openFeedPhotoModal().catch((error) => {
      console.error("Falha ao abrir modal de fotos.", error);
    });
  });
}
