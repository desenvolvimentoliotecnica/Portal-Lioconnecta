import { renderEmptyState } from "../components/cards.js";
import { escapeHtml } from "../components/html.js";
import { renderRhAdminHero } from "../people/adminNav.js";
import { renderPollAdminWizardModal } from "./adminPollWizard.js";
import { renderHomePollCarousel } from "./pollHomeCarousel.js";
import { renderPollInteractionBlock } from "./pollVoteUi.js";

function renderPollAttachment(poll, className = "") {
  if (!poll?.attachmentUrl || !poll?.attachmentLabel) {
    return "";
  }

  const classes = ["comm-inline-action", className].filter(Boolean).join(" ");

  return `
    <a href="${escapeHtml(poll.attachmentUrl)}" class="${classes}" target="_blank" rel="noreferrer noopener">
      <i class="fa-solid fa-paperclip"></i>
      ${escapeHtml(poll.attachmentLabel)}
    </a>
  `;
}

function renderPollMedia(poll, { compact = false, cover = false } = {}) {
  if (!poll?.imageUrl) {
    return "";
  }

  const modifier = [
    "poll-media",
    compact ? "is-compact" : "",
    cover ? "is-cover" : ""
  ].filter(Boolean).join(" ");

  return `
    <div class="${modifier}">
      <img src="${escapeHtml(poll.imageUrl)}" alt="${escapeHtml(poll.title || "Imagem da enquete")}" loading="lazy" />
    </div>
  `;
}

function renderPollAssetUploader({
  assetType,
  label,
  inputName,
  valueName,
  value,
  accept,
  buttonLabel,
  helper,
  previewType
}) {
  const hasValue = Boolean(value);
  const preview = previewType === "image" && hasValue
    ? `
      <div class="poll-asset-preview poll-asset-preview--image">
        <img src="${escapeHtml(value)}" alt="${escapeHtml(label)}" loading="lazy" />
      </div>
    `
    : hasValue
      ? `<div class="poll-asset-preview"><i class="fa-solid fa-paperclip"></i><span>${escapeHtml(value)}</span></div>`
      : `<div class="poll-asset-preview is-empty">Nenhum arquivo enviado ainda.</div>`;

  return `
    <div class="communication-form-field communication-form-field--full poll-asset-field" data-poll-asset="${escapeHtml(assetType)}">
      <span>${escapeHtml(label)}</span>
      <input type="hidden" name="${escapeHtml(valueName)}" value="${escapeHtml(value || "")}" />
      <div class="poll-asset-field__controls">
        <input type="file" name="${escapeHtml(inputName)}" accept="${escapeHtml(accept)}" />
        <button type="button" class="comm-secondary-button" data-action="upload-poll-asset" data-asset-type="${escapeHtml(assetType)}">
          ${escapeHtml(buttonLabel)}
        </button>
      </div>
      <small>${escapeHtml(helper)}</small>
      ${preview}
    </div>
  `;
}

function renderPollStatsCard(label, value, detail, tone = "brand") {
  return `
    <article class="comm-kpi comm-kpi--${escapeHtml(tone)}">
      <span class="comm-kpi-label">${escapeHtml(label)}</span>
      <strong>${escapeHtml(String(value))}</strong>
      <span class="comm-kpi-detail">${escapeHtml(detail)}</span>
    </article>
  `;
}

function renderPollMeta(poll) {
  return `
    <div class="poll-meta-row">
      <span><i class="fa-regular fa-calendar"></i>${escapeHtml(poll.publishedAtLabel || "A publicar")}</span>
      <span><i class="fa-solid fa-users"></i>${escapeHtml(poll.audience)}</span>
      <span><i class="fa-solid fa-chart-simple"></i>${escapeHtml(String(poll.totalVotes))} voto(s)</span>
      <span><i class="fa-solid fa-clock"></i>${escapeHtml(poll.statusLabel)}</span>
    </div>
  `;
}

function renderPollCard(poll) {
  const footer = renderPollInteractionBlock(poll, { compact: true });

  return `
    <article class="card poll-card">
      <div class="poll-card__top">
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(poll.statusLabel)}</span>
          ${poll.allowMultipleChoices ? '<span class="comm-tag">Multipla escolha</span>' : '<span class="comm-tag">Escolha unica</span>'}
        </div>
        ${poll.isFeatured ? '<span class="comm-status">Destaque</span>' : ""}
      </div>
      ${renderPollMedia(poll, { compact: true })}
      <h3>${escapeHtml(poll.title)}</h3>
      <p>${escapeHtml(poll.summary)}</p>
      ${renderPollMeta(poll)}
      <div class="poll-card__actions">
        <a href="#enquetes/leitura/${escapeHtml(poll.slug)}" class="comm-inline-action">Abrir enquete</a>
        ${renderPollAttachment(poll)}
      </div>
      ${footer}
    </article>
  `;
}

function renderPollHero(data, canManage = false) {
  return `
    <section class="card comm-hero-card poll-hero-card">
      <div class="comm-hero-copy comm-hero-copy--solid">
        <span class="comm-hero-eyebrow">${escapeHtml(data.intro.eyebrow)}</span>
        <h1>${escapeHtml(data.intro.title)}</h1>
        <p>${escapeHtml(data.intro.subtitle)}</p>
        ${canManage ? `
          <div class="poll-hero-actions">
            <a href="#admin/enquetes" class="feed-composer-submit">Gerenciar enquetes</a>
          </div>
        ` : ""}
      </div>
    </section>
  `;
}

export function renderHomePollHighlight(poll) {
  if (!poll) {
    return "";
  }

  return renderHomePollCarousel([poll]);
}

export function renderPollsHub(data, { canManage = false } = {}) {
  const featuredBlock = data.featured
    ? `
      <section class="card poll-featured-card">
        <div class="card-header">Enquete em destaque</div>
        <div class="poll-featured-card__body">
          <div class="poll-featured-card__copy">
            <div class="comm-meta-row">
              <span class="comm-tag comm-tag--solid">${escapeHtml(data.featured.statusLabel)}</span>
              <span class="comm-tag">${data.featured.allowMultipleChoices ? "Multipla escolha" : "Escolha unica"}</span>
            </div>
            <h2>${escapeHtml(data.featured.title)}</h2>
            <p>${escapeHtml(data.featured.body || data.featured.summary)}</p>
            ${renderPollMeta(data.featured)}
            ${renderPollAttachment(data.featured)}
          </div>
          <div class="poll-featured-card__vote">
            ${renderPollMedia(data.featured, { cover: true })}
            ${renderPollInteractionBlock(data.featured)}
          </div>
        </div>
      </section>
    `
    : `
      <section class="card">
        <div class="card-header">Enquetes em destaque</div>
        ${renderEmptyState("Nenhuma enquete publicada", "Assim que a primeira enquete for publicada, ela aparecera aqui para toda a companhia.")}
      </section>
    `;

  const listBlock = data.allPolls.length
    ? `<div class="poll-grid">${data.allPolls.map(renderPollCard).join("")}</div>`
    : renderEmptyState("Nenhuma enquete disponivel", data.intro.loadError || "Nao existem enquetes publicadas neste momento.");

  return [
    renderPollHero(data, canManage),
    `<section class="comm-kpi-grid">
      ${renderPollStatsCard("Enquetes publicadas", data.stats.total, "Disponiveis para a companhia", "brand")}
      ${renderPollStatsCard("Abertas agora", data.stats.open, "Recebendo votos", "success")}
      ${renderPollStatsCard("Encerradas", data.stats.closed, "Historico consolidado", "info")}
      ${renderPollStatsCard("Votos contabilizados", data.stats.votes, "Participacao registrada", "danger")}
    </section>`,
    featuredBlock,
    `<section class="card">
      <div class="card-header">Todas as enquetes</div>
      <div class="poll-list-section">${listBlock}</div>
    </section>`
  ].join("");
}

export function renderPollDetailPage(poll) {
  if (!poll) {
    return `
      <section class="card">
        <div class="card-header">Detalhe da enquete</div>
        ${renderEmptyState("Enquete nao encontrada", "A enquete solicitada nao esta disponivel ou foi removida do ciclo publico.")}
      </section>
    `;
  }

  return `
    <section class="card poll-detail-card">
      <div class="card-header">Leitura da enquete</div>
      <div class="poll-detail-card__body">
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(poll.statusLabel)}</span>
          <span class="comm-tag">${poll.allowMultipleChoices ? "Multipla escolha" : "Escolha unica"}</span>
          <span class="comm-tag">${escapeHtml(poll.resultsVisibilityLabel)}</span>
        </div>
        <h1>${escapeHtml(poll.title)}</h1>
        <p class="poll-detail-card__summary">${escapeHtml(poll.summary)}</p>
        ${renderPollMedia(poll, { cover: true })}
        <div class="poll-detail-card__content">
          ${escapeHtml(poll.body).replace(/\n/g, "<br>")}
        </div>
        ${renderPollMeta(poll)}
        ${renderPollAttachment(poll)}
        ${renderPollInteractionBlock(poll)}
      </div>
    </section>
  `;
}

function renderAdminPollCard(item) {
  const nextAction = item.status === "Draft"
    ? { label: "Publicar", status: "Published" }
    : item.status === "Published"
      ? { label: "Encerrar", status: "Closed" }
      : item.status === "Closed"
        ? { label: "Arquivar", status: "Archived" }
        : null;

  return `
    <article class="poll-admin-card ${item.isFeatured ? "is-featured" : ""}">
      <div class="poll-admin-card__top">
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(item.statusLabel)}</span>
          <span class="comm-tag">${escapeHtml(String(item.totalVotes))} voto(s)</span>
          <span class="comm-tag">${escapeHtml(String(item.uniqueVoters))} participante(s)</span>
        </div>
        ${item.isFeatured ? '<span class="comm-status">Home</span>' : ""}
      </div>
      <h3>${escapeHtml(item.title)}</h3>
      <p>${escapeHtml(item.summary)}</p>
      ${renderPollMedia(item, { compact: true })}
      <div class="poll-meta-row">
        <span><i class="fa-regular fa-calendar"></i>${escapeHtml(item.publishedAtLabel || "Nao publicada")}</span>
        <span><i class="fa-regular fa-clock"></i>${escapeHtml(item.closesAtLabel || "Sem encerramento")}</span>
      </div>
      ${renderPollAttachment(item)}
      <div class="poll-admin-card__options">
        ${item.options.map((option) => `<span class="comm-tag">${escapeHtml(option.label)} • ${escapeHtml(String(option.votes))}</span>`).join("")}
      </div>
      <div class="poll-admin-card__actions">
        <button type="button" class="comm-inline-action" data-action="admin-poll-edit" data-poll-id="${escapeHtml(item.id)}">Editar</button>
        ${nextAction ? `
          <button
            type="button"
            class="comm-secondary-button"
            data-action="admin-poll-status"
            data-poll-id="${escapeHtml(item.id)}"
            data-next-status="${escapeHtml(nextAction.status)}"
          >
            ${escapeHtml(nextAction.label)}
          </button>
        ` : ""}
        <a href="#enquetes/leitura/${escapeHtml(item.slug)}" class="comm-tertiary-button">Ver publico</a>
      </div>
    </article>
  `;
}

export function renderAdminPollsPage(data, selectedPoll = null) {
  const editing = Boolean(selectedPoll?.id);
  const formPoll = selectedPoll || {
    id: "",
    title: "",
    summary: "",
    body: "",
    imageUrl: "",
    attachmentLabel: "",
    attachmentUrl: "",
    audience: "Toda a companhia",
    status: "Draft",
    resultsVisibility: "AfterVote",
    allowMultipleChoices: false,
    isFeatured: false,
    publishedAtEditorValue: "",
    closesAtEditorValue: "",
    options: [{ id: "", label: "" }, { id: "", label: "" }]
  };

  return [
    renderRhAdminHero({
      title: "Enquetes",
      description: data.intro.subtitle
    }),
    `<section class="comm-kpi-grid">
      ${renderPollStatsCard("Enquetes cadastradas", data.summary.totalPolls, "Base editorial", "brand")}
      ${renderPollStatsCard("Publicadas", data.summary.publishedPolls, "Disponiveis no portal", "success")}
      ${renderPollStatsCard("Encerradas", data.summary.closedPolls, "Historico consolidado", "info")}
      ${renderPollStatsCard("Votos acumulados", data.summary.totalVotes, "Participacao registrada", "danger")}
    </section>`,
    `<section class="poll-admin-layout poll-admin-layout--list-only">
      <div class="card">
        <div class="card-header poll-admin-list__header">
          <span>Enquetes publicadas e rascunhos</span>
          <button type="button" class="feed-composer-submit" data-action="admin-poll-create">
            <i class="fa-solid fa-plus" aria-hidden="true"></i>
            Nova enquete
          </button>
        </div>
        <div class="poll-admin-list">
          ${data.items.length ? data.items.map(renderAdminPollCard).join("") : renderEmptyState("Nenhuma enquete cadastrada", data.intro.loadError || "Crie a primeira enquete para iniciar o modulo editorial.")}
        </div>
      </div>
    </section>`,
    renderPollAdminWizardModal(data, formPoll, editing)
  ].join("");
}
