import { escapeHtml } from "../components/html.js";
import { showToast } from "../core/feedback.js";
import { getJourneyDocumentContent } from "./service.js";

let currentObjectUrl = null;
let currentDocumentMeta = null;

function getModalElements(root = document) {
  return {
    modal: root.getElementById("document-viewer-modal"),
    body: root.getElementById("document-viewer-modal-body"),
    title: root.getElementById("document-viewer-modal-title"),
    subtitle: root.getElementById("document-viewer-modal-subtitle")
  };
}

function revokeCurrentObjectUrl() {
  if (currentObjectUrl) {
    URL.revokeObjectURL(currentObjectUrl);
    currentObjectUrl = null;
  }
}

function setDownloadActionEnabled(modal, enabled) {
  if (!modal) {
    return;
  }

  modal.querySelectorAll("[data-action='download-document']").forEach((button) => {
    button.disabled = !enabled;
  });
}

function triggerBlobDownload(blob, fileName) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName || "documento";
  link.click();
  URL.revokeObjectURL(url);
}

function renderPreviewContent(objectUrl, mimeType) {
  if (mimeType === "application/pdf" || mimeType.endsWith("/pdf")) {
    return `<iframe class="document-viewer-modal__frame" src="${escapeHtml(objectUrl)}" title="Visualizacao do documento"></iframe>`;
  }

  if (mimeType.startsWith("image/")) {
    return `<img class="document-viewer-modal__image" src="${escapeHtml(objectUrl)}" alt="Visualizacao do documento">`;
  }

  return `
    <div class="document-viewer-modal__unsupported">
      <strong>Visualizacao nao disponivel para este tipo de arquivo.</strong>
      <p>Use o botao Baixar para salvar o documento no seu dispositivo.</p>
    </div>
  `;
}

export function renderDocumentViewerModalShell() {
  return `
    <div class="document-viewer-modal" id="document-viewer-modal" hidden aria-hidden="true">
      <div class="document-viewer-modal__dialog card" role="dialog" aria-modal="true" aria-labelledby="document-viewer-modal-title">
        <div class="card-header document-viewer-modal__header">
          <div class="document-viewer-modal__title-block">
            <strong id="document-viewer-modal-title">Documento</strong>
            <span id="document-viewer-modal-subtitle">Visualizacao do arquivo</span>
          </div>
          <div class="document-viewer-modal__actions">
            <button type="button" class="comm-secondary-button" data-action="download-document" disabled>
              <i class="fa-solid fa-download" aria-hidden="true"></i>
              Baixar
            </button>
            <button
              type="button"
              class="comm-tertiary-button document-viewer-modal__close"
              data-action="close-document-viewer"
              aria-label="Fechar visualizacao"
            >
              <i class="fa-solid fa-xmark" aria-hidden="true"></i>
            </button>
          </div>
        </div>
        <div class="document-viewer-modal__body" id="document-viewer-modal-body">
          <div class="document-viewer-modal__loading">Carregando documento...</div>
        </div>
      </div>
    </div>
  `;
}

export function closeDocumentViewer(root = document) {
  const { modal, body } = getModalElements(root);
  if (!modal) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  root.body.classList.remove("modal-open");
  revokeCurrentObjectUrl();
  currentDocumentMeta = null;
  setDownloadActionEnabled(modal, false);

  if (body) {
    body.innerHTML = `<div class="document-viewer-modal__loading">Carregando documento...</div>`;
  }
}

export async function openDocumentViewer(documentId, meta = {}, root = document) {
  const { modal, body, title, subtitle } = getModalElements(root);
  if (!modal || !body || !documentId) {
    return;
  }

  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  root.body.classList.add("modal-open");
  revokeCurrentObjectUrl();
  currentDocumentMeta = null;
  setDownloadActionEnabled(modal, false);
  body.innerHTML = `<div class="document-viewer-modal__loading">Carregando documento...</div>`;

  if (title) {
    title.textContent = meta.title || "Documento";
  }

  if (subtitle) {
    const parts = [meta.category, meta.sizeLabel, meta.status].filter(Boolean);
    subtitle.textContent = parts.length ? parts.join(" • ") : "Visualizacao do arquivo";
  }

  try {
    const content = await getJourneyDocumentContent(documentId);
    currentObjectUrl = URL.createObjectURL(content.blob);
    currentDocumentMeta = {
      id: documentId,
      fileName: content.fileName || meta.fileName || "documento",
      mimeType: content.mimeType || meta.mimeType || "application/octet-stream",
      blob: content.blob
    };
    body.innerHTML = renderPreviewContent(currentObjectUrl, currentDocumentMeta.mimeType);
    setDownloadActionEnabled(modal, true);
  } catch {
    currentDocumentMeta = null;
    setDownloadActionEnabled(modal, false);
    body.innerHTML = `
      <div class="document-viewer-modal__error">
        <strong>Nao foi possivel carregar o documento.</strong>
        <p>Tente novamente em instantes ou entre em contato com o RH.</p>
        <button type="button" class="comm-secondary-button" data-action="close-document-viewer">Fechar</button>
      </div>
    `;
  }
}

function handleDownloadDocument(root = document) {
  if (!currentDocumentMeta?.blob) {
    showToast("Nao foi possivel baixar o documento.", "danger");
    return;
  }

  triggerBlobDownload(currentDocumentMeta.blob, currentDocumentMeta.fileName);
  showToast("Download iniciado.", "success");
}

function handleDocumentKeydown(event) {
  if (event.key === "Escape") {
    closeDocumentViewer(document);
  }
}

export function bindDocumentViewerModal(root = document) {
  const centerContent = root.getElementById("center-content");
  const { modal } = getModalElements(root);

  if (!centerContent || !modal) {
    return;
  }

  centerContent.querySelectorAll("[data-action='open-document-viewer']").forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", () => {
      const documentId = button.getAttribute("data-document-id") || "";
      if (!documentId) {
        return;
      }

      openDocumentViewer(documentId, {
        title: button.getAttribute("data-document-title") || "",
        category: button.getAttribute("data-document-category") || "",
        sizeLabel: button.getAttribute("data-document-size") || "",
        status: button.getAttribute("data-document-status") || "",
        mimeType: button.getAttribute("data-document-mime") || "",
        fileName: button.getAttribute("data-document-filename") || ""
      }, root);
    });
  });

  if (modal.dataset.bound === "true") {
    return;
  }

  modal.dataset.bound = "true";

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closeDocumentViewer(root);
      return;
    }

    if (event.target.closest("[data-action='close-document-viewer']")) {
      closeDocumentViewer(root);
      return;
    }

    if (event.target.closest("[data-action='download-document']")) {
      handleDownloadDocument(root);
    }
  });

  if (!root.body.dataset.documentViewerKeydownBound) {
    root.body.dataset.documentViewerKeydownBound = "true";
    root.addEventListener("keydown", handleDocumentKeydown);
  }
}
