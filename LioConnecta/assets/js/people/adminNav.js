import { escapeHtml } from "../components/html.js";

export const RH_ADMIN_ITEMS = Object.freeze([
  {
    id: "humor",
    label: "Humor da Companhia",
    route: "pessoas-rh",
    icon: "fa-solid fa-face-smile"
  },
  {
    id: "enquetes",
    label: "Enquetes",
    route: "admin/enquetes",
    icon: "fa-solid fa-square-poll-vertical"
  },
  {
    id: "comunicados",
    label: "Comunicados",
    route: "comunicacao/restrita",
    icon: "fa-solid fa-bullhorn"
  }
]);

export function renderRhAdminNav(activeItemId = "humor") {
  return `
    <aside class="rh-admin-nav card" aria-label="Menu administrativo de RH">
      <div class="card-header">Administrativo</div>
      <nav class="rh-admin-nav__list menu-list">
        ${RH_ADMIN_ITEMS.map((item) => `
          <a
            href="#${escapeHtml(item.route)}"
            class="rh-admin-nav__item ${item.id === activeItemId ? "is-active" : ""}"
            ${item.id === activeItemId ? 'aria-current="page"' : ""}
          >
            <i class="${escapeHtml(item.icon)}" aria-hidden="true"></i>
            <span>${escapeHtml(item.label)}</span>
          </a>
        `).join("")}
      </nav>
    </aside>
  `;
}

export function renderRhAdminHero({ eyebrow = "ADMINISTRATIVO", title, description = "" }) {
  return `
    <section class="card communication-admin-hero-card">
      <div class="communication-admin-hero">
        <div class="communication-admin-copy">
          <span class="communications-eyebrow">${escapeHtml(eyebrow)}</span>
          <h1>${escapeHtml(title)}</h1>
          ${description ? `<p>${escapeHtml(description)}</p>` : ""}
        </div>
      </div>
    </section>
  `;
}

export function wrapRhAdminShell(mainContent, activeItemId = "humor") {
  return `
    <div class="rh-admin-shell">
      ${renderRhAdminNav(activeItemId)}
      <div class="rh-admin-main">
        ${mainContent}
      </div>
    </div>
  `;
}
