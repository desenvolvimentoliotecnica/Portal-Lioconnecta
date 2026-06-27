import { escapeHtml } from "./html.js";

function renderUserAvatar(photoUrl = "") {
  const normalizedPhotoUrl = String(photoUrl || "").trim();

  if (normalizedPhotoUrl) {
    return `
      <div class="avatar avatar--photo" aria-hidden="true">
        <img
          class="avatar__photo"
          src="${escapeHtml(normalizedPhotoUrl)}"
          alt=""
          loading="lazy"
          decoding="async"
        >
        <i class="fa-solid fa-user avatar__fallback"></i>
      </div>
    `;
  }

  return `
    <div class="avatar" aria-hidden="true">
      <i class="fa-solid fa-user"></i>
    </div>
  `;
}

export function renderHeaderShell(data) {
  return `
    <header class="topbar" aria-label="Barra superior">
      <div class="brand">
        <div class="brand-mark" aria-hidden="true"></div>
        <div class="brand-copy">
          <strong>${escapeHtml(data.brand.name)}</strong>
          <span>${escapeHtml(data.brand.tagline)}</span>
        </div>
      </div>

      <div class="topbar-actions">
        <div class="user-chip">
          ${renderUserAvatar(data.user.photoUrl)}
          <div class="user-info">
            <strong>${escapeHtml(data.user.name)}</strong>
            ${data.user.area ? `<span class="user-area">${escapeHtml(data.user.area)}</span>` : ""}
          </div>
        </div>
        <button
          type="button"
          class="topbar-logout-btn"
          data-analytics="topbar.logout"
          data-action="portal-logout"
        >
          <i class="fa-solid fa-right-from-bracket" aria-hidden="true"></i>
          Sair
        </button>
      </div>
    </header>

    <nav class="nav" aria-label="Menu principal">
      <div class="nav-tabs">
        ${data.navItems.map((item) => `
          <a
            href="${escapeHtml(item.href || "#")}"
            class="${item.active ? "active" : ""}"
            data-analytics="nav.tab"
            data-analytics-label="${escapeHtml(item.label)}"
            aria-current="${item.active ? "page" : "false"}"
          >${escapeHtml(item.label)}</a>
        `).join("")}
      </div>
    </nav>
  `;
}
