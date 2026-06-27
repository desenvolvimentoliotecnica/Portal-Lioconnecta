import { escapeHtml } from "./html.js";
import { renderMoodCardHtml } from "../services/moodSurveyService.js";

function renderSkeletonLine(width = "100%") {
  return `<span class="skeleton-line" style="width:${escapeHtml(width)}"></span>`;
}

export function renderEmptyState(title, message, actionLabel = "") {
  return `
    <div class="empty-state" role="status" aria-live="polite">
      <span class="empty-state-icon" aria-hidden="true"><i class="fa-regular fa-folder-open"></i></span>
      <strong>${escapeHtml(title)}</strong>
      <p>${escapeHtml(message)}</p>
      ${actionLabel ? `<button type="button" class="empty-state-action">${escapeHtml(actionLabel)}</button>` : ""}
    </div>
  `;
}

export function renderHero(hero) {
  return `
    <section class="hero" aria-label="Banner principal">
      <div class="hero-copy">
        <h1>${escapeHtml(hero.title)}</h1>
        <p>${escapeHtml(hero.subtitle)}</p>
      </div>
    </section>
  `;
}

export function renderMoodCard(mood) {
  if (mood?.hasVoted !== undefined || mood?.items?.some((item) => item.key)) {
    return renderMoodCardHtml(mood);
  }

  return renderLegacyMoodCard(mood);
}

function renderLegacyMoodCard(mood) {
  return `
    <section class="card mood-card">
      <h2>${escapeHtml(mood.title)}</h2>
      <div class="mood-options" role="list" aria-label="Ranking de humor do dia">
        ${mood.items.map((item) => `
          <button
            class="mood-option"
            type="button"
            role="listitem"
            data-analytics="mood.vote"
            data-analytics-label="${escapeHtml(item.label)}"
            aria-label="${escapeHtml(item.label)} - ${escapeHtml(item.rank)}"
          >
            <span class="mood-option-emoji" aria-hidden="true">${escapeHtml(item.emoji)}</span>
            <strong>${escapeHtml(item.label)}</strong>
            <span class="mood-rank">${escapeHtml(item.rank)}</span>
          </button>
        `).join("")}
      </div>
    </section>
  `;
}

export function renderLoadingHeader() {
  return `
    <div class="topbar topbar--loading">
      <div class="brand">
        <span class="skeleton-circle"></span>
        <div class="loading-stack">
          ${renderSkeletonLine("180px")}
          ${renderSkeletonLine("130px")}
        </div>
      </div>
      <div class="loading-inline">
        ${renderSkeletonLine("240px")}
      </div>
    </div>
    <div class="nav nav--loading">
      <div class="nav-tabs">
        ${["80px", "120px", "100px", "90px", "85px", "95px"].map((width) => `
          <span class="nav-loading-pill">${renderSkeletonLine(width)}</span>
        `).join("")}
      </div>
    </div>
  `;
}

export function renderLoadingPanel(title = "Carregando") {
  return `
    <section class="card loading-card">
      <div class="card-header">${escapeHtml(title)}</div>
      <div class="loading-card-body">
        ${renderSkeletonLine("78%")}
        ${renderSkeletonLine("92%")}
        ${renderSkeletonLine("66%")}
        ${renderSkeletonLine("84%")}
      </div>
    </section>
  `;
}

export function renderLoadingHero() {
  return `
    <section class="hero hero--loading" aria-label="Carregando banner principal">
      <div class="hero-copy">
        ${renderSkeletonLine("240px")}
        ${renderSkeletonLine("190px")}
      </div>
    </section>
  `;
}

export function renderLoadingMoodCard() {
  return `
    <section class="card mood-card loading-card">
      <h2>Como você está se sentindo hoje?</h2>
      <div class="mood-options">
        ${Array.from({ length: 3 }, () => `
          <div class="mood-option mood-option--loading" aria-hidden="true">
            <span class="skeleton-circle skeleton-circle--lg"></span>
            ${renderSkeletonLine("88px")}
            ${renderSkeletonLine("64px")}
          </div>
        `).join("")}
      </div>
    </section>
  `;
}

export function renderLoadingCarousel() {
  return `
    <section class="card news-card loading-card">
      <div class="card-header">Comunicação centralizada</div>
      <div class="news-grid">
        <div class="carousel carousel--loading">
          <div class="carousel-slide carousel-slide--loading"></div>
        </div>
        <div class="carousel-dots carousel-dots--loading">
          ${Array.from({ length: 4 }, () => `<span class="skeleton-dot"></span>`).join("")}
        </div>
      </div>
    </section>
  `;
}

export function renderLoadingFeed() {
  return `
    <section class="card feed-card loading-card">
      <div class="card-header">Feed LIOCONNECTA</div>
      <div class="feed-composer-card loading-card-body">
        ${renderSkeletonLine("180px")}
        ${renderSkeletonLine("100%")}
        <div class="loading-inline">
          ${renderSkeletonLine("96px")}
          ${renderSkeletonLine("96px")}
          ${renderSkeletonLine("96px")}
        </div>
      </div>
      <div class="feed-list">
        ${Array.from({ length: 2 }, () => `
          <article class="post post--loading" aria-hidden="true">
            <div class="post-head">
              <div class="post-author">
                <span class="skeleton-circle"></span>
                <div class="loading-stack">
                  ${renderSkeletonLine("120px")}
                  ${renderSkeletonLine("86px")}
                </div>
              </div>
            </div>
            <div class="post-body">
              ${renderSkeletonLine("100%")}
              ${renderSkeletonLine("94%")}
              ${renderSkeletonLine("65%")}
              <div class="skeleton-block skeleton-block--media"></div>
            </div>
          </article>
        `).join("")}
      </div>
    </section>
  `;
}

export function renderErrorCard(message, detail) {
  return `
    <section class="card app-error-card">
      <div class="card-header">${escapeHtml(message)}</div>
      <div class="app-error-body">
        <span class="app-error-icon" aria-hidden="true"><i class="fa-solid fa-triangle-exclamation"></i></span>
        <strong>Não foi possível carregar a experiência completa.</strong>
        <p>${escapeHtml(detail)}</p>
        <button type="button" class="feed-composer-submit" data-action="retry-bootstrap">
          Tentar novamente
        </button>
      </div>
    </section>
  `;
}
