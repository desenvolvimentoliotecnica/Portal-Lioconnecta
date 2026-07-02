import { escapeHtml } from "../components/html.js";
import { getLearningCatalog } from "./service.js";

const MODAL_ID = "journey-learning-catalog-modal";
const BODY_ID = "journey-learning-catalog-modal-body";
const SUBTITLE_ID = "journey-learning-catalog-modal-subtitle";

function renderStatusPill(status = "") {
  const normalized = String(status).toLowerCase();
  const tone = normalized.includes("aprov") || normalized.includes("dispon") || normalized.includes("conclu")
    ? "success"
    : normalized.includes("analise") || normalized.includes("andamento") || normalized.includes("aguard")
      ? "warning"
      : normalized.includes("atras") || normalized.includes("pend")
        ? "danger"
        : "info";

  return `<span class="panel-pill panel-pill--${tone}">${escapeHtml(status)}</span>`;
}

function renderMetricCards(summary = {}) {
  return `
    <div class="hr-profile-metrics journey-learning-catalog-modal__metrics">
      <article class="hr-profile-metric">
        <span>Cursos</span>
        <strong>${escapeHtml(String(summary.coursesCount ?? 0))}</strong>
      </article>
      <article class="hr-profile-metric">
        <span>Materiais</span>
        <strong>${escapeHtml(String(summary.materialsCount ?? 0))}</strong>
      </article>
      <article class="hr-profile-metric">
        <span>Carga horaria</span>
        <strong>${escapeHtml(summary.hoursLabel || "—")}</strong>
      </article>
    </div>
  `;
}

function renderCourseItem(course = {}) {
  return `
    <article class="hr-profile-list-item journey-learning-catalog-modal__item">
      <div>
        <strong>${escapeHtml(course.title || "—")}</strong>
        <span>
          ${escapeHtml(course.format || "—")}
          • ${escapeHtml(course.durationLabel || "—")}
          ${course.trilhaTitle ? ` • Trilha: ${escapeHtml(course.trilhaTitle)}` : ""}
        </span>
        ${course.description ? `<p class="journey-learning-catalog-modal__description">${escapeHtml(course.description)}</p>` : ""}
      </div>
      <div class="hr-profile-list-item__meta">
        ${renderStatusPill(course.status || "—")}
      </div>
    </article>
  `;
}

function renderMaterialItem(material = {}) {
  return `
    <article class="hr-profile-list-item journey-learning-catalog-modal__item">
      <div>
        <strong>${escapeHtml(material.title || "—")}</strong>
        <span>
          ${escapeHtml(material.type || "—")}
          • ${escapeHtml(material.sizeLabel || "—")}
        </span>
      </div>
      <div class="hr-profile-list-item__meta">
        ${renderStatusPill(material.status || "—")}
      </div>
    </article>
  `;
}

export function renderCatalogContent(data = {}) {
  const summary = data.summary || {};
  const courses = Array.isArray(data.courses) ? data.courses : [];
  const materials = Array.isArray(data.materials) ? data.materials : [];

  return `
    ${renderMetricCards(summary)}
    <section class="journey-learning-catalog-modal__section">
      <h3 class="journey-learning-catalog-modal__section-title">Cursos</h3>
      <div class="hr-profile-list">
        ${courses.length
          ? courses.map((course) => renderCourseItem(course)).join("")
          : `<p class="journey-learning-catalog-modal__empty">Nenhum curso disponivel no momento.</p>`}
      </div>
    </section>
    <section class="journey-learning-catalog-modal__section">
      <h3 class="journey-learning-catalog-modal__section-title">Materiais de apoio</h3>
      <div class="hr-profile-list">
        ${materials.length
          ? materials.map((material) => renderMaterialItem(material)).join("")
          : `<p class="journey-learning-catalog-modal__empty">Nenhum material disponivel no momento.</p>`}
      </div>
    </section>
  `;
}

export function renderLearningCatalogModalShell() {
  return `
    <div
      class="journey-learning-catalog-modal"
      id="${MODAL_ID}"
      hidden
      aria-hidden="true"
    >
      <div
        class="journey-learning-catalog-modal__dialog card"
        role="dialog"
        aria-modal="true"
        aria-labelledby="journey-learning-catalog-modal-title"
      >
        <div class="card-header journey-learning-catalog-modal__header">
          <div class="journey-learning-catalog-modal__title-block">
            <strong id="journey-learning-catalog-modal-title">Cursos e Materiais disponiveis</strong>
            <span id="${SUBTITLE_ID}">Consulta integrada ao LMS</span>
          </div>
          <button
            type="button"
            class="comm-tertiary-button journey-learning-catalog-modal__close"
            data-action="close-learning-catalog-modal"
            aria-label="Fechar modal"
          >
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
        </div>
        <div class="journey-learning-catalog-modal__body" id="${BODY_ID}"></div>
      </div>
    </div>
  `;
}

function getModalElements(root = document) {
  const modal = root.getElementById(MODAL_ID);
  const body = root.getElementById(BODY_ID);
  const subtitle = root.getElementById(SUBTITLE_ID);
  return { modal, body, subtitle };
}

export function closeLearningCatalogModal(root = document) {
  const { modal, body } = getModalElements(root);
  if (!modal) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  root.body.classList.remove("modal-open");
  if (body) {
    body.innerHTML = "";
  }
}

async function loadCatalogContent(root = document) {
  const { modal, body, subtitle } = getModalElements(root);
  if (!modal || !body) {
    return;
  }

  body.innerHTML = `<div class="journey-learning-catalog-modal__loading">Carregando cursos e materiais...</div>`;

  try {
    const data = await getLearningCatalog();
    const provider = data.provider || "LMS";
    const simulatedNote = data.isSimulated
      ? " Os dados exibidos nesta fase sao simulados para validacao da experiencia."
      : "";

    if (subtitle) {
      subtitle.textContent = `Consulta integrada ao ${provider}.${simulatedNote}`;
    }

    body.innerHTML = renderCatalogContent(data);
  } catch {
    body.innerHTML = `
      <div class="journey-learning-catalog-modal__error">
        <strong>Nao foi possivel carregar o catalogo.</strong>
        <p>Tente novamente em instantes.</p>
        <button type="button" class="comm-secondary-button" data-action="retry-learning-catalog-modal">
          Tentar novamente
        </button>
      </div>
    `;
  }
}

export async function openLearningCatalogModal(root = document) {
  const { modal } = getModalElements(root);
  if (!modal) {
    return;
  }

  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  root.body.classList.add("modal-open");
  await loadCatalogContent(root);
  modal.querySelector("[data-action='close-learning-catalog-modal']")?.focus();
}

export function bindLearningCatalogModal(root = document) {
  const centerContent = root.getElementById("center-content");
  const { modal } = getModalElements(root);

  if (!centerContent || !modal) {
    return;
  }

  centerContent.querySelectorAll("[data-action='open-learning-catalog-modal']").forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", () => {
      openLearningCatalogModal(root);
    });
  });

  if (modal.dataset.bound === "true") {
    return;
  }

  modal.dataset.bound = "true";

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closeLearningCatalogModal(root);
      return;
    }

    if (event.target.closest("[data-action='close-learning-catalog-modal']")) {
      closeLearningCatalogModal(root);
      return;
    }

    if (event.target.closest("[data-action='retry-learning-catalog-modal']")) {
      loadCatalogContent(root);
    }
  });

  root.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && !modal.hidden) {
      closeLearningCatalogModal(root);
    }
  });
}
