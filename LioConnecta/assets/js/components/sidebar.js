import { renderEmptyState } from "./cards.js";
import { escapeHtml } from "./html.js";
import { normalizeAgendaPanelItem } from "../services/agendaService.js";

const PANEL_ITEM_ICONS = {
  "tarefas pendentes": "fa-solid fa-list-check",
  "solicitacoes em andamento": "fa-solid fa-hourglass-half",
  "trilhas de aprendizagem": "fa-solid fa-route",
  "documentos recentes": "fa-solid fa-file-lines",
  "notificacoes totais": "fa-solid fa-bell",
  "comunicados rh": "fa-solid fa-bullhorn",
  "comunicados corporativos": "fa-solid fa-building",
  tecnologia: "fa-solid fa-microchip",
  politicas: "fa-solid fa-scale-balanced",
  eventos: "fa-solid fa-calendar-days",
  enquetes: "fa-solid fa-square-poll-vertical",
  lidas: "fa-solid fa-check-double",
  "comunicados novos": "fa-solid fa-bullhorn",
  "interacoes no feed": "fa-solid fa-comments",
  "aprovacoes pendentes": "fa-solid fa-circle-check",
  "eventos/reunioes": "fa-solid fa-calendar-days",
  aniversarios: "fa-solid fa-cake-candles",
  "atualizacoes de sistema": "fa-solid fa-gear",
  "itens salvos": "fa-solid fa-bookmark",
  corporativos: "fa-solid fa-building",
  "google workspace": "fa-brands fa-google",
  sistemas: "fa-solid fa-server",
  projetos: "fa-solid fa-diagram-project",
  recursos: "fa-solid fa-folder-open",
  "presenca hoje": "fa-solid fa-user-check",
  "chamados abertos": "fa-solid fa-ticket",
  "projetos ativos": "fa-solid fa-briefcase",
  "eventos da semana": "fa-solid fa-calendar-week",
  "treinamentos do mes": "fa-solid fa-chalkboard-user",
  "indicadores rapidos": "fa-solid fa-chart-line",
  "ferias (consultar/solicitar)": "fa-solid fa-umbrella-beach",
  "holerite (maio 2024)": "fa-solid fa-file-invoice-dollar",
  holerite: "fa-solid fa-file-invoice-dollar",
  "beneficios (seguro/vt)": "fa-solid fa-heart-pulse",
  "beneficios (vr/vt)": "fa-solid fa-heart-pulse",
  beneficios: "fa-solid fa-heart-pulse",
  "minha avaliacao": "fa-solid fa-star",
  "dados cadastrais": "fa-solid fa-id-card",
  ponto: "fa-solid fa-clock",
  treinamentos: "fa-solid fa-graduation-cap",
  "chamados rh": "fa-solid fa-headset",
  "gestao integrada": "fa-solid fa-industry",
  servicenow: "fa-solid fa-screwdriver-wrench",
  "microsoft teams": "fa-brands fa-microsoft",
  "e-learning treinamentos": "fa-solid fa-graduation-cap",
  "jira/confluence": "fa-brands fa-jira",
  ferias: "fa-solid fa-umbrella-beach"
};

const QUICK_LINK_CLASS_ICONS = {
  sap: "fa-solid fa-industry",
  google: "fa-brands fa-google",
  service: "fa-solid fa-screwdriver-wrench",
  teams: "fa-brands fa-microsoft",
  learn: "fa-solid fa-graduation-cap",
  jira: "fa-brands fa-jira"
};

const PANEL_TITLE_FALLBACK_ICONS = {
  "minha jornada": "fa-solid fa-road",
  "meu painel": "fa-solid fa-bell",
  "sistemas corporativos": "fa-solid fa-sitemap",
  "indicadores rapidos": "fa-solid fa-chart-line",
  "acessos rapidos": "fa-solid fa-bolt",
  "meu perfil rh": "fa-solid fa-user-tie",
  agenda: "fa-solid fa-calendar-check",
  comunicados: "fa-solid fa-bullhorn"
};

function normalizePanelLabel(label) {
  return String(label || "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}

function resolvePanelItemIcon(label, { panelTitle = "" } = {}) {
  const normalizedLabel = normalizePanelLabel(label);
  if (PANEL_ITEM_ICONS[normalizedLabel]) {
    return PANEL_ITEM_ICONS[normalizedLabel];
  }

  if (normalizedLabel.includes("tarefa")) return "fa-solid fa-list-check";
  if (normalizedLabel.includes("solicit")) return "fa-solid fa-hourglass-half";
  if (normalizedLabel.includes("trilha") || normalizedLabel.includes("aprendiz")) return "fa-solid fa-route";
  if (normalizedLabel.includes("document")) return "fa-solid fa-file-lines";
  if (normalizedLabel.includes("notific")) return "fa-solid fa-bell";
  if (normalizedLabel.includes("comunicad")) return "fa-solid fa-bullhorn";
  if (normalizedLabel.includes("enquete")) return "fa-solid fa-square-poll-vertical";
  if (normalizedLabel.includes("salv")) return "fa-solid fa-bookmark";
  if (normalizedLabel.includes("google")) return "fa-brands fa-google";
  if (normalizedLabel.includes("chamado")) return "fa-solid fa-ticket";
  if (normalizedLabel.includes("projeto")) return "fa-solid fa-diagram-project";
  if (normalizedLabel.includes("evento") || normalizedLabel.includes("reuniao")) return "fa-solid fa-calendar-days";
  if (normalizedLabel.includes("trein")) return "fa-solid fa-chalkboard-user";
  if (normalizedLabel.includes("presen")) return "fa-solid fa-user-check";
  if (normalizedLabel.includes("ferias")) return "fa-solid fa-umbrella-beach";
  if (normalizedLabel.includes("holerite")) return "fa-solid fa-file-invoice-dollar";
  if (normalizedLabel.includes("benefic")) return "fa-solid fa-heart-pulse";
  if (normalizedLabel.includes("ponto")) return "fa-solid fa-clock";
  if (normalizedLabel.includes("avaliac")) return "fa-solid fa-star";
  if (normalizedLabel.includes("cadastr")) return "fa-solid fa-id-card";

  const normalizedPanelTitle = normalizePanelLabel(panelTitle);
  if (PANEL_TITLE_FALLBACK_ICONS[normalizedPanelTitle]) {
    return PANEL_TITLE_FALLBACK_ICONS[normalizedPanelTitle];
  }

  return "fa-solid fa-circle-dot";
}

function resolveQuickLinkIcon(item) {
  const byLabel = resolvePanelItemIcon(item.label);
  if (byLabel !== "fa-solid fa-circle-dot") {
    return byLabel;
  }

  return QUICK_LINK_CLASS_ICONS[item.className] || "fa-solid fa-arrow-up-right-from-square";
}

function resolveAgendaIcon(event) {
  const haystack = normalizePanelLabel(`${event.title} ${event.source} ${event.description} ${event.location}`);

  if (haystack.includes("daily")) return "fa-solid fa-users";
  if (haystack.includes("comite")) return "fa-solid fa-people-group";
  if (haystack.includes("trein")) return "fa-solid fa-chalkboard-user";
  if (haystack.includes("contrat") || haystack.includes("follow")) return "fa-solid fa-user-plus";
  if (haystack.includes("revisao") || haystack.includes("indicador")) return "fa-solid fa-chart-line";
  if (haystack.includes("encerr")) return "fa-solid fa-flag-checkered";
  if (haystack.includes("reuniao") || haystack.includes("alinh")) return "fa-solid fa-handshake";
  if (haystack.includes("comunic")) return "fa-solid fa-bullhorn";

  return "fa-solid fa-calendar-check";
}

function renderPanelItemIcon(label, context = {}) {
  const iconClass = resolvePanelItemIcon(label, context);
  return `<span class="panel-item-icon" aria-hidden="true"><i class="${escapeHtml(iconClass)}"></i></span>`;
}

function renderPanelPill(label, tone = "neutral") {
  return `<span class="panel-pill panel-pill--${tone}">${escapeHtml(label)}</span>`;
}

export function renderMenuCard(panel) {
  const items = Array.isArray(panel.items) ? panel.items : [];

  return `
    <section class="card">
      <div class="card-header">${escapeHtml(panel.title)}</div>
      ${items.length ? `
        <div class="menu-list">
          ${items.map((item) => {
            const label = typeof item === "string" ? item : item.label;
            const url = typeof item === "object" ? item.url : "";
            const content = `
              <span class="menu-item-label">
                ${renderPanelItemIcon(label, { panelTitle: panel.title })}
                <span>${escapeHtml(label)}</span>
              </span>
              ${typeof item === "object" && item.badge ? `<span class="menu-badge">${escapeHtml(item.badge)}</span>` : ""}
              ${typeof item === "object" && item.value ? `<strong>${escapeHtml(item.value)}</strong>` : ""}
            `;

            return url
              ? `<a class="menu-item menu-item--link" href="${escapeHtml(url)}" ${url.startsWith("http") ? 'target="_blank" rel="noopener noreferrer"' : ""}>${content}</a>`
              : `<div class="menu-item">${content}</div>`;
          }).join("")}
        </div>
      ` : renderEmptyState("Sem itens por enquanto", "Este painel será preenchido quando os dados do módulo estiverem disponíveis.")}
    </section>
  `;
}

export function renderNotificationsCard(panel) {
  const panelItems = Array.isArray(panel.items) ? panel.items : [];
  const linkItems = panelItems.filter((item) => typeof item === "object" && item.url);
  const notificationItems = panelItems.filter((item) => !(typeof item === "object" && item.url));
  const [totalItem, ...items] = notificationItems;
  const total = totalItem?.badge || totalItem?.value || "0";

  return `
    <section class="card notifications-card">
      <div class="card-header">${escapeHtml(panel.title)}</div>
      <div class="notifications-summary">
        <div class="notifications-total">
          <span class="notifications-total-icon" aria-hidden="true"><i class="fa-solid fa-bell"></i></span>
          <div class="notifications-total-copy">
            <strong class="notifications-total-count">${escapeHtml(total)}</strong>
            <span class="notifications-total-label">${escapeHtml(totalItem?.label || "Notificações")}</span>
          </div>
        </div>
      </div>
      <div class="notification-list">
        ${items.length ? items.map((item) => `
          <div class="notification-item">
            <span class="panel-item-icon panel-item-icon--compact" aria-hidden="true">
              <i class="${escapeHtml(resolvePanelItemIcon(item.label, { panelTitle: panel.title }))}" aria-hidden="true"></i>
            </span>
            <span class="notification-label">${escapeHtml(item.label)}</span>
            ${item.badge ? `<span class="notification-badge">${escapeHtml(item.badge)}</span>` : ""}
          </div>
        `).join("") : renderEmptyState("Nenhuma notificação", "Quando houver alertas do dia, eles aparecerão aqui.")}
      </div>
      ${linkItems.length ? `
        <div class="menu-list notifications-links">
          ${linkItems.map((item) => `
            <a class="menu-item menu-item--link" href="${escapeHtml(item.url)}">
              <span class="menu-item-label">
                ${renderPanelItemIcon(item.label, { panelTitle: panel.title })}
                <span>${escapeHtml(item.label)}</span>
              </span>
              ${item.badge ? `<span class="menu-badge">${escapeHtml(item.badge)}</span>` : ""}
            </a>
          `).join("")}
        </div>
      ` : ""}
    </section>
  `;
}

export function renderQuickLinksCard(panel) {
  const items = Array.isArray(panel.items) ? panel.items : [];

  return `
    <section class="card quick-links-card">
      <div class="card-header">${escapeHtml(panel.title)}</div>
      ${items.length ? `
        <div class="quick-grid">
          ${items.map((item) => {
            const href = item.url || "#";
            const isExternal = href.startsWith("http");
            const iconClass = resolveQuickLinkIcon(item);

            return `
            <a
              class="quick-item ${escapeHtml(item.className)}"
              href="${escapeHtml(href)}"
              ${isExternal ? 'target="_blank" rel="noopener noreferrer"' : ""}
              data-analytics="quick-link.open"
              data-analytics-label="${escapeHtml(item.label)}"
              aria-label="${escapeHtml(item.label)}"
            >
              <div class="quick-item-content">
                <strong class="quick-item-mark" aria-hidden="true">
                  <i class="${escapeHtml(iconClass)}"></i>
                </strong>
                <span class="quick-item-label">${escapeHtml(item.label)}</span>
              </div>
            </a>
          `;
          }).join("")}
        </div>
      ` : renderEmptyState("Sem atalhos configurados", "Os serviços rápidos serão exibidos aqui quando o catálogo estiver disponível.")}
    </section>
  `;
}

export function renderProfileCard(panel) {
  const items = Array.isArray(panel.items) ? panel.items : [];

  return `
    <section class="card profile-card">
      <div class="card-header">${escapeHtml(panel.title)}</div>
      <div class="profile-box">
        <div class="profile-head">
          <div class="avatar" aria-hidden="true"><i class="fa-solid fa-user"></i></div>
          <div class="profile-head-copy">
            <div class="profile-name">${escapeHtml(panel.name)}</div>
            ${panel.subtitle ? `<div class="user-area">${escapeHtml(panel.subtitle)}</div>` : ""}
            ${panel.description ? `<div class="profile-role">${escapeHtml(panel.description)}</div>` : ""}
            ${panel.manager ? `<div class="profile-role">Gestor: ${escapeHtml(panel.manager)}</div>` : ""}
            <div class="profile-meta">
              ${renderPanelPill("RH", "brand")}
              ${renderPanelPill("Perfil ativo", "success")}
            </div>
          </div>
        </div>
        <div class="profile-links">
          ${items.length ? items.map((item) => {
            const label = typeof item === "string" ? item : item.label;
            const url = typeof item === "object" ? item.url : "";
            const iconClass = resolvePanelItemIcon(label, { panelTitle: panel.title });
            const isExternal = Boolean(url && /^https?:/i.test(url));
            const rowContent = `
              <span class="panel-item-icon" aria-hidden="true">
                <i class="${escapeHtml(iconClass)}"></i>
              </span>
              <span>${escapeHtml(label)}</span>
            `;

            return url
              ? `<a class="profile-link-row profile-link-row--link" href="${escapeHtml(url)}" ${isExternal ? 'target="_blank" rel="noopener noreferrer"' : ""}>${rowContent}</a>`
              : `<div class="profile-link-row">${rowContent}</div>`;
          }).join("") : renderEmptyState("Perfil sem serviços", "Os atalhos de RH deste colaborador serão exibidos aqui.")}
        </div>
      </div>
    </section>
  `;
}

function isAgendaPanelTitle(title) {
  return title === "AGENDA" || title === "AGENDA DO DIA";
}

export function renderAgendaCard(panel) {
  const items = Array.isArray(panel.items) ? panel.items : [];

  return `
    <section class="card agenda-card">
      <div class="card-header">${escapeHtml(isAgendaPanelTitle(panel.title) ? "AGENDA" : panel.title)}</div>
      <div class="agenda-list">
        ${items.length ? items.map((item, index) => {
          const event = normalizeAgendaPanelItem(item, `agenda-item-${index}`);
          const preview = event.location || event.description || "";
          const iconClass = resolveAgendaIcon(event);

          return `
            <button
              type="button"
              class="agenda-item agenda-item--interactive"
              data-action="open-agenda-event-modal"
              data-agenda-id="${escapeHtml(event.id || `agenda-item-${index}`)}"
              aria-label="Ver detalhes de ${escapeHtml(event.title)}"
            >
              <div class="agenda-time">${escapeHtml(event.timeLabel)}</div>
              <span class="panel-item-icon panel-item-icon--compact agenda-item-icon" aria-hidden="true">
                <i class="${escapeHtml(iconClass)}"></i>
              </span>
              <div class="agenda-copy">
                <strong>${escapeHtml(event.title)}</strong>
                ${preview ? `<span>${escapeHtml(preview)}</span>` : ""}
              </div>
            </button>
          `;
        }).join("") : renderEmptyState("Agenda livre", "Nenhum compromisso programado nos proximos dias.")}
      </div>
    </section>
  `;
}

export function renderSidebarPanels(panels) {
  return panels.map((panel) => {
    if (panel.type === "quick-links") {
      return renderQuickLinksCard(panel);
    }

    if (panel.type === "profile") {
      return renderProfileCard(panel);
    }

    if (panel.title === "MEU PAINEL") {
      return renderNotificationsCard(panel);
    }

    if (isAgendaPanelTitle(panel.title)) {
      return renderAgendaCard(panel);
    }

    return renderMenuCard(panel);
  }).join("");
}
