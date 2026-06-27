import { renderEmptyState } from "../components/cards.js";
import { initCarousel, renderCarouselSection } from "../components/carousel.js";
import { escapeHtml } from "../components/html.js";
import { renderRhAdminHero } from "../people/adminNav.js";
import {
  renderCommunicationAdminWizardModal,
  mapCommunicationToForm
} from "./communicationAdminWizard.js";

function renderKpiCard(item) {
  return `
    <article class="comm-kpi comm-kpi--${escapeHtml(item.tone || "brand")}">
      <span class="comm-kpi-label">${escapeHtml(item.label)}</span>
      <strong>${escapeHtml(item.value)}</strong>
      <span class="comm-kpi-detail">${escapeHtml(item.detail)}</span>
    </article>
  `;
}

function renderFilterChip(item) {
  return `
    <button
      type="button"
      class="comm-filter-chip ${item.active ? "is-active" : ""}"
      data-feedback-message="Filtro ${escapeHtml(item.label)} selecionado em modo demonstrativo."
      data-feedback-tone="info"
    >
      <span>${escapeHtml(item.label)}</span>
      <strong>${escapeHtml(String(item.count))}</strong>
    </button>
  `;
}

function renderReadLink(slug, label = "Ler comunicado", toneClass = "feed-composer-submit") {
  return `
    <a
      href="#comunicacao/leitura/${escapeHtml(slug)}"
      class="${toneClass}"
      data-analytics="communication.read"
      data-analytics-label="${escapeHtml(slug)}"
    >
      ${escapeHtml(label)}
    </a>
  `;
}

function renderCommunicationItem(item) {
  return `
    <article class="comm-item-card">
      <div class="comm-item-top">
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(item.category)}</span>
          <span class="comm-tag">${escapeHtml(item.priority)}</span>
        </div>
        <span class="comm-status">${escapeHtml(item.status)}</span>
      </div>
      <h3>${escapeHtml(item.title)}</h3>
      <p>${escapeHtml(item.summary)}</p>
      <div class="comm-item-facts">
        <span><i class="fa-regular fa-calendar"></i>${escapeHtml(item.publishedAt)}</span>
        <span><i class="fa-solid fa-users"></i>${escapeHtml(item.audience)}</span>
        <span><i class="fa-solid fa-tower-broadcast"></i>${escapeHtml(item.channel)}</span>
      </div>
      <div class="comm-item-actions">
        ${renderReadLink(item.slug, "Ler comunicado", "comm-inline-action")}
        <button
          type="button"
          class="comm-inline-action"
          data-feedback-message="${escapeHtml(item.attachmentLabel)} iniciado em modo demonstrativo."
          data-feedback-tone="info"
        >
          ${escapeHtml(item.attachmentLabel)}
        </button>
      </div>
    </article>
  `;
}

function renderBodyParagraphs(body = []) {
  return body.map((paragraph) => `<p>${escapeHtml(paragraph)}</p>`).join("");
}

function renderSelected(value, expected) {
  return value === expected ? "selected" : "";
}

function renderChecked(value) {
  return value ? "checked" : "";
}

function renderLdapSettingsCard() {
  return `
    <section class="card communication-form-card ldap-settings-card">
      <div class="card-header">Active Directory / LDAP</div>
      <div class="communication-section-intro">
        <strong>Configure autenticacao corporativa</strong>
        <p>Defina os parametros do diretorio para o login por e-mail e senha dos colaboradores em um assistente guiado por etapas.</p>
      </div>
      <div class="comm-item-actions">
        <a href="#configuracoes/ldap" class="feed-composer-submit">
          <i class="fa-solid fa-sliders" aria-hidden="true"></i>
          Abrir assistente LDAP
        </a>
      </div>
    </section>
  `;
}

function renderMicrosoftGraphSettingsCard() {
  return `
    <section class="card communication-form-card ldap-settings-card">
      <div class="card-header">Microsoft 365 / Graph</div>
      <div class="communication-section-intro">
        <strong>Configure a agenda corporativa</strong>
        <p>Informe Tenant ID, Client ID e Client Secret para habilitar a leitura da agenda do dia via Microsoft Graph.</p>
      </div>
      <div class="comm-item-actions">
        <a href="#configuracoes/microsoft-graph" class="feed-composer-submit">
          <i class="fa-regular fa-calendar-days" aria-hidden="true"></i>
          Configurar Microsoft Graph
        </a>
      </div>
    </section>
  `;
}

function renderPortalUserStatCard(label, value, detail, tone = "brand") {
  return `
    <article class="comm-kpi comm-kpi--${escapeHtml(tone)}">
      <span class="comm-kpi-label">${escapeHtml(label)}</span>
      <strong>${escapeHtml(String(value))}</strong>
      <span class="comm-kpi-detail">${escapeHtml(detail)}</span>
    </article>
  `;
}

export function renderAdminUsersKpiSection(summary) {
  return `
    <section class="comm-kpi-grid">
      ${renderPortalUserStatCard("Usuarios registrados", summary.registeredUsers, "Cadastros persistidos", "brand")}
      ${renderPortalUserStatCard("Usuarios ativos", summary.activeUsers, "Com acesso liberado", "success")}
      ${renderPortalUserStatCard("Usuarios inativos", summary.inactiveUsers, "Bloqueados manualmente", "danger")}
      ${renderPortalUserStatCard("Tentativas falhas", summary.failedLoginEvents, "Alertas de autenticacao", "danger")}
      ${renderPortalUserStatCard("Logins concluidos", summary.loginEvents, "Entradas validadas", "success")}
      ${renderPortalUserStatCard("Logouts registrados", summary.logoutEvents, "Encerramentos de sessao", "info")}
      ${renderPortalUserStatCard("Humor registrado", summary.moodSurveyEvents, "Respostas da pesquisa diaria", "info")}
    </section>
  `;
}

function renderPortalUsersSortButton(label, sortKey, currentSortBy, currentSortDirection) {
  const isActive = currentSortBy === sortKey;
  const nextDirection = isActive && currentSortDirection === "asc" ? "desc" : "asc";
  const icon = !isActive
    ? "fa-solid fa-sort"
    : currentSortDirection === "asc"
      ? "fa-solid fa-sort-up"
      : "fa-solid fa-sort-down";

  return `
    <button
      type="button"
      class="admin-users-sort-button ${isActive ? "is-active" : ""}"
      data-action="admin-users-sort"
      data-sort-by="${escapeHtml(sortKey)}"
      data-sort-direction="${escapeHtml(nextDirection)}"
    >
      <span>${escapeHtml(label)}</span>
      <i class="${icon}" aria-hidden="true"></i>
    </button>
  `;
}

function createEmptyPortalUsersPage() {
  return {
    items: [],
    summary: {
      registeredUsers: 0,
      activeUsers: 0,
      inactiveUsers: 0,
      departmentsMapped: 0,
      portalAdmins: 0,
      loginEvents: 0,
      failedLoginEvents: 0,
      logoutEvents: 0,
      moodSurveyEvents: 0
    },
    roleOptions: [],
    departmentOptions: [],
    moduleOptions: [],
    accessLevelOptions: [],
    recentLogins: [],
    recentAuditEntries: [],
    recentMoodSurveyEntries: [],
    page: 1,
    pageSize: 8,
    totalItems: 0,
    totalPages: 1,
    query: "",
    status: "all",
    role: "",
    department: "all",
    sortBy: "displayName",
    sortDirection: "asc"
  };
}

function renderPortalUserPermission(permission) {
  return `<span class="comm-tag">${escapeHtml(permission)}</span>`;
}

function renderPortalUserAccessLevelOptions(currentAccessLevel, accessLevelOptions = []) {
  return accessLevelOptions
    .map((option) => `
      <option value="${escapeHtml(option.key)}" ${renderSelected(currentAccessLevel, option.key)}>
        ${escapeHtml(option.label)}
      </option>
    `)
    .join("");
}

function renderPortalUserRoleOptions(currentRole, roleOptions = []) {
  return roleOptions
    .map((option) => `
      <option value="${escapeHtml(option.key)}" ${renderSelected(currentRole, option.key)}>
        ${escapeHtml(option.label)}
      </option>
    `)
    .join("");
}

function renderPortalUserModulePermissionControl(user, permission, accessLevelOptions = [], isDisabled = false) {
  return `
    <label class="communication-form-field">
      <span>${escapeHtml(permission.moduleLabel)}</span>
      <select
        class="admin-user-role-select"
        data-action="update-portal-user-permission"
        data-user-id="${escapeHtml(user.id)}"
        data-user-name="${escapeHtml(user.displayName)}"
        data-module-key="${escapeHtml(permission.moduleKey)}"
        data-module-label="${escapeHtml(permission.moduleLabel)}"
        data-access-level="${escapeHtml(permission.accessLevel)}"
        ${isDisabled ? "disabled" : ""}
      >
        ${renderPortalUserAccessLevelOptions(permission.accessLevel, accessLevelOptions)}
      </select>
    </label>
  `;
}

function renderPortalUserStatusPill(user) {
  return `
    <span class="admin-user-status-pill ${user.isActive ? "is-active" : "is-inactive"}">
      <i class="fa-solid ${user.isActive ? "fa-circle-check" : "fa-circle-pause"}"></i>
      ${escapeHtml(user.isActive ? "Ativo" : "Inativo")}
    </span>
  `;
}

function renderPortalUserRow(user) {
  const lastAccess = user.lastLoginLabel || "Sem login";

  return `
    <tr class="admin-users-table__row">
      <td>
        <div class="admin-user-name-cell">
          <strong>${escapeHtml(user.displayName)}</strong>
          <span>${escapeHtml(user.login)}</span>
        </div>
      </td>
      <td>${user.email ? escapeHtml(user.email) : '<span class="admin-user-muted">Nao informado</span>'}</td>
      <td>${user.department ? escapeHtml(user.department) : '<span class="admin-user-muted">Nao informado</span>'}</td>
      <td>${user.title ? escapeHtml(user.title) : '<span class="admin-user-muted">Nao informado</span>'}</td>
      <td>${user.managerDisplayName ? escapeHtml(user.managerDisplayName) : '<span class="admin-user-muted">Nao informado</span>'}</td>
      <td>${escapeHtml(user.roleLabel)}</td>
      <td>${renderPortalUserStatusPill(user)}</td>
      <td>${escapeHtml(lastAccess)}</td>
      <td>${escapeHtml(String(user.failedLoginCount || 0))}</td>
      <td>
        <div class="admin-user-actions">
          <button
            type="button"
            class="comm-secondary-button"
            data-action="open-portal-user-modal"
            data-user-id="${escapeHtml(user.id)}"
            data-user-mode="view"
          >
            Visualizar
          </button>
          <button
            type="button"
            class="feed-composer-submit"
            data-action="open-portal-user-modal"
            data-user-id="${escapeHtml(user.id)}"
            data-user-mode="edit"
          >
            Editar
          </button>
        </div>
      </td>
    </tr>
  `;
}

function renderPortalUsersGrid(items = [], currentSortBy = "displayName", currentSortDirection = "asc") {
  return `
    <div class="admin-users-grid-shell">
      <div class="admin-users-grid-head">
        <div>
          <strong>Base de usuarios do portal</strong>
          <p>Use a grade para localizar usuarios rapidamente e abrir o modal com visualizacao ou edicao.</p>
        </div>
      </div>
      <div class="admin-users-table-wrap">
        <table class="admin-users-table">
          <thead>
            <tr>
              <th>${renderPortalUsersSortButton("Usuario", "displayName", currentSortBy, currentSortDirection)}</th>
              <th>${renderPortalUsersSortButton("E-mail", "email", currentSortBy, currentSortDirection)}</th>
              <th>${renderPortalUsersSortButton("Departamento", "department", currentSortBy, currentSortDirection)}</th>
              <th>Cargo</th>
              <th>Gestor</th>
              <th>${renderPortalUsersSortButton("Perfil", "role", currentSortBy, currentSortDirection)}</th>
              <th>${renderPortalUsersSortButton("Status", "status", currentSortBy, currentSortDirection)}</th>
              <th>${renderPortalUsersSortButton("Ultimo login", "lastLogin", currentSortBy, currentSortDirection)}</th>
              <th>${renderPortalUsersSortButton("Falhas", "failedLogins", currentSortBy, currentSortDirection)}</th>
              <th>Acoes</th>
            </tr>
          </thead>
          <tbody>
            ${items.map(renderPortalUserRow).join("")}
          </tbody>
        </table>
      </div>
    </div>
  `;
}

function renderPortalUsersModalShell() {
  return `
    <div class="admin-user-modal" id="portal-user-modal" hidden aria-hidden="true">
      <div class="admin-user-modal__dialog card" role="dialog" aria-modal="true" aria-labelledby="portal-user-modal-title">
        <div class="card-header admin-user-modal__header">
          <div class="admin-user-modal__title-block">
            <strong id="portal-user-modal-title">Detalhes do usuario</strong>
            <span>Visualize ou edite o cadastro sem sair da grade.</span>
          </div>
          <button
            type="button"
            class="comm-tertiary-button admin-user-modal__close"
            data-action="close-portal-user-modal"
            aria-label="Fechar modal"
          >
            <i class="fa-solid fa-xmark"></i>
          </button>
        </div>
        <div class="admin-user-modal__body" id="portal-user-modal-body"></div>
      </div>
    </div>
  `;
}

export function renderPortalUserModal(user, roleOptions = [], accessLevelOptions = [], mode = "view") {
  const isEditing = mode === "edit";

  return `
    <div class="admin-user-modal__content">
      <section class="admin-user-modal__hero">
        <div class="admin-user-modal__identity">
          <div class="admin-user-modal__avatar">
            <i class="fa-solid fa-user"></i>
          </div>
          <div class="admin-user-modal__identity-copy">
            <strong>${escapeHtml(user.displayName)}</strong>
            <span>${escapeHtml(user.login)}${user.email ? ` • ${escapeHtml(user.email)}` : ""}</span>
          </div>
        </div>
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(user.authenticationProvider)}</span>
          <span class="comm-tag">${escapeHtml(user.roleLabel)}</span>
          ${renderPortalUserStatusPill(user)}
        </div>
      </section>

      <section class="admin-user-modal__section">
        <div class="admin-user-modal__section-head">
          <strong>Resumo do cadastro</strong>
          <span>${isEditing ? "Modo de edicao ativo." : "Modo somente leitura."}</span>
        </div>
        <div class="communication-form-grid">
          <label class="communication-form-field">
            <span>Nome completo</span>
            <input type="text" value="${escapeHtml(user.displayName)}" disabled />
          </label>
          <label class="communication-form-field">
            <span>Login</span>
            <input type="text" value="${escapeHtml(user.login)}" disabled />
          </label>
          <label class="communication-form-field">
            <span>E-mail</span>
            <input type="text" value="${escapeHtml(user.email || "")}" placeholder="Nao informado" disabled />
          </label>
          <label class="communication-form-field">
            <span>Departamento</span>
            <input type="text" value="${escapeHtml(user.department || "")}" placeholder="Nao informado" disabled />
          </label>
          <label class="communication-form-field">
            <span>Cargo</span>
            <input type="text" value="${escapeHtml(user.title || "")}" placeholder="Nao informado" disabled />
          </label>
          <label class="communication-form-field">
            <span>Gestor</span>
            <input type="text" value="${escapeHtml(user.managerDisplayName || "")}" placeholder="Nao informado" disabled />
          </label>
          <label class="communication-form-field">
            <span>Ultimo login</span>
            <input type="text" value="${escapeHtml(user.lastLoginLabel || "Sem login registrado")}" disabled />
          </label>
          <label class="communication-form-field">
            <span>Ultima falha</span>
            <input type="text" value="${escapeHtml(user.lastFailedLoginLabel || "Sem falha registrada")}" disabled />
          </label>
          <label class="communication-form-field">
            <span>Ultima origem / IP</span>
            <input type="text" value="${escapeHtml([user.lastOrigin, user.lastKnownIpAddress].filter(Boolean).join(" • "))}" placeholder="Nao identificado" disabled />
          </label>
        </div>
      </section>

      <section class="admin-user-modal__section">
        <div class="admin-user-modal__section-head">
          <strong>Perfil e permissoes</strong>
          <span>${isEditing ? "As alteracoes sao aplicadas imediatamente ao selecionar um novo valor." : "Abra em edicao para alterar perfil, status e modulos."}</span>
        </div>
        <div class="communication-form-grid admin-user-form-grid">
          <label class="communication-form-field">
            <span>Perfil de acesso</span>
            <select
              class="admin-user-role-select"
              data-action="update-portal-user-role"
              data-user-id="${escapeHtml(user.id)}"
              data-user-name="${escapeHtml(user.displayName)}"
              data-user-role="${escapeHtml(user.role)}"
              ${isEditing ? "" : "disabled"}
            >
              ${renderPortalUserRoleOptions(user.role, roleOptions)}
            </select>
          </label>
          <label class="communication-form-field">
            <span>Status do usuario</span>
            <input type="text" value="${escapeHtml(user.isActive ? "Acesso liberado" : "Acesso bloqueado")}" disabled />
          </label>
        </div>
        <div class="communication-form-grid admin-user-form-grid">
          ${user.modulePermissions.length
            ? user.modulePermissions.map((permission) => renderPortalUserModulePermissionControl(user, permission, accessLevelOptions, !isEditing)).join("")
            : `
              <div class="admin-user-modal__empty">
                <i class="fa-regular fa-folder-open"></i>
                <span>Nenhuma permissao modular configurada para este usuario.</span>
              </div>
            `}
        </div>
      </section>

      <div class="admin-user-modal__actions">
        <button
          type="button"
          class="${isEditing ? "comm-secondary-button" : "feed-composer-submit"}"
          data-action="portal-user-modal-switch-mode"
          data-user-id="${escapeHtml(user.id)}"
          data-user-mode="${isEditing ? "view" : "edit"}"
        >
          ${escapeHtml(isEditing ? "Visualizar dados" : "Editar usuario")}
        </button>
        <button
          type="button"
          class="comm-inline-action"
          data-action="toggle-portal-user-status"
          data-user-id="${escapeHtml(user.id)}"
          data-user-active="${user.isActive ? "true" : "false"}"
          data-user-name="${escapeHtml(user.displayName)}"
          ${isEditing ? "" : "disabled"}
        >
          ${escapeHtml(user.isActive ? "Desativar acesso" : "Reativar acesso")}
        </button>
        <button
          type="button"
          class="comm-secondary-button"
          data-action="close-portal-user-modal"
        >
          Fechar
        </button>
      </div>
    </div>
  `;
}

function renderPortalUserCard(user, roleOptions = [], accessLevelOptions = []) {
  const searchBlob = [
    user.displayName,
    user.login,
    user.email,
    user.department,
    user.title
  ].join(" ").toLowerCase();

  return `
    <article class="comm-item-card admin-user-card" data-user-card data-user-search="${escapeHtml(searchBlob)}" data-user-status="${user.isActive ? "active" : "inactive"}">
      <div class="comm-item-top">
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(user.authenticationProvider)}</span>
          <span class="comm-tag">${escapeHtml(user.roleLabel)}</span>
          <span class="comm-tag">${user.isActive ? "Ativo" : "Inativo"}</span>
        </div>
        <span class="comm-status">${escapeHtml(user.isActive ? "LIBERADO" : "BLOQUEADO")}</span>
      </div>
      <h3>${escapeHtml(user.displayName)}</h3>
      <p>${escapeHtml(user.login)}${user.email ? ` • ${escapeHtml(user.email)}` : ""}</p>
      <div class="comm-item-facts">
        ${user.department ? `<span><i class="fa-solid fa-building-user"></i>${escapeHtml(user.department)}</span>` : ""}
        ${user.title ? `<span><i class="fa-solid fa-id-badge"></i>${escapeHtml(user.title)}</span>` : ""}
        ${user.managerDisplayName ? `<span><i class="fa-solid fa-user-tie"></i>${escapeHtml(user.managerDisplayName)}</span>` : ""}
        <span><i class="fa-solid fa-fingerprint"></i>${escapeHtml(`${user.loginCount} login(s)`)}</span>
        <span><i class="fa-solid fa-triangle-exclamation"></i>${escapeHtml(`${user.failedLoginCount} falha(s)`)}</span>
        ${user.lastLoginLabel ? `<span><i class="fa-regular fa-clock"></i>Ultimo login em ${escapeHtml(user.lastLoginLabel)}</span>` : `<span><i class="fa-regular fa-clock"></i>Ainda sem login registrado</span>`}
        ${user.lastFailedLoginLabel ? `<span><i class="fa-solid fa-ban"></i>Ultima falha em ${escapeHtml(user.lastFailedLoginLabel)}</span>` : ""}
        ${user.lastKnownIpAddress ? `<span><i class="fa-solid fa-network-wired"></i>${escapeHtml(user.lastKnownIpAddress)}</span>` : ""}
        ${user.lastOrigin ? `<span><i class="fa-solid fa-link"></i>${escapeHtml(user.lastOrigin)}</span>` : ""}
      </div>
      ${user.permissions.length ? `
        <div class="comm-meta-row admin-user-permissions">
          ${user.permissions.map(renderPortalUserPermission).join("")}
        </div>
      ` : ""}
      <div class="communication-form-grid admin-user-form-grid">
        <label class="communication-form-field">
          <span>Perfil de acesso</span>
          <select
            class="admin-user-role-select"
            data-action="update-portal-user-role"
            data-user-id="${escapeHtml(user.id)}"
            data-user-name="${escapeHtml(user.displayName)}"
            data-user-role="${escapeHtml(user.role)}"
          >
            ${renderPortalUserRoleOptions(user.role, roleOptions)}
          </select>
        </label>
        <label class="communication-form-field">
          <span>Status do usuario</span>
          <input type="text" value="${escapeHtml(user.isActive ? "Acesso liberado" : "Acesso bloqueado")}" disabled />
        </label>
      </div>
      <div class="communication-form-grid admin-user-form-grid">
        ${user.modulePermissions.map((permission) => renderPortalUserModulePermissionControl(user, permission, accessLevelOptions)).join("")}
      </div>
      <div class="comm-item-actions">
        <button
          type="button"
          class="comm-inline-action"
          data-action="toggle-portal-user-status"
          data-user-id="${escapeHtml(user.id)}"
          data-user-active="${user.isActive ? "true" : "false"}"
          data-user-name="${escapeHtml(user.displayName)}"
        >
          ${escapeHtml(user.isActive ? "Desativar acesso" : "Reativar acesso")}
        </button>
      </div>
    </article>
  `;
}

function renderPortalUsersPagination(pageData) {
  const page = Number(pageData?.page ?? 1);
  const totalPages = Math.max(1, Number(pageData?.totalPages ?? 1));
  const totalItems = Number(pageData?.totalItems ?? 0);
  const pageSize = Number(pageData?.pageSize ?? 8);
  const startItem = totalItems === 0 ? 0 : ((page - 1) * pageSize) + 1;
  const endItem = Math.min(totalItems, page * pageSize);

  return `
    <div class="admin-users-pagination">
      <div class="admin-users-pagination__summary">
        Exibindo <strong>${escapeHtml(String(startItem))}</strong> a <strong>${escapeHtml(String(endItem))}</strong> de <strong>${escapeHtml(String(totalItems))}</strong> usuarios.
      </div>
      <div class="admin-users-pagination__actions">
        <button
          type="button"
          class="comm-secondary-button"
          data-action="admin-users-page"
          data-page="${escapeHtml(String(page - 1))}"
          ${page <= 1 ? "disabled" : ""}
        >
          Pagina anterior
        </button>
        <span class="comm-tag">Pagina ${escapeHtml(String(page))} de ${escapeHtml(String(totalPages))}</span>
        <button
          type="button"
          class="comm-secondary-button"
          data-action="admin-users-page"
          data-page="${escapeHtml(String(page + 1))}"
          ${page >= totalPages ? "disabled" : ""}
        >
          Proxima pagina
        </button>
      </div>
    </div>
  `;
}

function renderAdminRecentLoginItem(item) {
  return `
    <article class="admin-activity-item">
      <div class="admin-activity-item__top">
        <strong>${escapeHtml(item.displayName)}</strong>
        <span class="comm-tag">${escapeHtml(item.authenticationProvider)}</span>
      </div>
                <p>${escapeHtml(item.login)}${item.department ? ` • ${escapeHtml(item.department)}` : ""}${item.title ? ` • ${escapeHtml(item.title)}` : ""}</p>
      <span class="admin-activity-item__meta">${escapeHtml(item.loggedAtLabel || "Sem horario registrado")}</span>
    </article>
  `;
}

function renderAdminAuditItem(item) {
  const details = [item.previousValue, item.newValue].filter(Boolean).join(" -> ");

  return `
    <article class="admin-activity-item">
      <div class="admin-activity-item__top">
        <strong>${escapeHtml(item.portalUserDisplayName)}</strong>
        <span class="comm-tag">${escapeHtml(item.actionType)}</span>
      </div>
      <p>${escapeHtml(item.actorDisplayName || item.actorUsername)}${item.actorRole ? ` • ${escapeHtml(item.actorRole)}` : ""}</p>
      ${details ? `<p>${escapeHtml(details)}</p>` : ""}
      ${item.notes ? `<p>${escapeHtml(item.notes)}</p>` : ""}
      <span class="admin-activity-item__meta">${escapeHtml(item.createdAtLabel || "Sem horario registrado")}</span>
    </article>
  `;
}

export function renderAdminUsersResultsSection(pageData = createEmptyPortalUsersPage(), loadError = "") {
  const viewModel = {
    ...createEmptyPortalUsersPage(),
    ...(pageData || {})
  };
  const items = Array.isArray(viewModel.items) ? viewModel.items : [];

  return `
    <section class="card comm-list-card">
      <div class="card-header">Usuarios do portal</div>
      <div class="comm-list-body" id="admin-users-list">
        ${items.length
          ? renderPortalUsersGrid(items, viewModel.sortBy || "displayName", viewModel.sortDirection || "asc")
          : loadError
            ? renderEmptyState(
              "Nao foi possivel carregar os usuarios",
              loadError
            )
            : renderEmptyState(
              "Nenhum usuario registrado",
              "Assim que colaboradores entrarem pela intranet, eles passarao a ser persistidos e gerenciados aqui."
            )}
      </div>
      ${items.length ? renderPortalUsersPagination(viewModel) : ""}
    </section>
  `;
}

export function renderAdminUsersActivitySection(pageData = createEmptyPortalUsersPage()) {
  const viewModel = {
    ...createEmptyPortalUsersPage(),
    ...(pageData || {})
  };
  const recentLogins = Array.isArray(viewModel.recentLogins) ? viewModel.recentLogins : [];
  const recentAuditEntries = Array.isArray(viewModel.recentAuditEntries) ? viewModel.recentAuditEntries : [];
  const recentMoodSurveyEntries = Array.isArray(viewModel.recentMoodSurveyEntries) ? viewModel.recentMoodSurveyEntries : [];

  return `
    <section class="communication-admin-layout admin-users-activity-layout admin-users-activity-layout--three">
      <section class="card comm-list-card">
        <div class="card-header">Atividade de autenticacao</div>
        <div class="comm-list-body">
          ${recentLogins.length
            ? recentLogins.map((item) => `
              <article class="admin-activity-item">
                <div class="admin-activity-item__top">
                  <strong>${escapeHtml(item.displayName)}</strong>
                  <span class="comm-tag">${escapeHtml(item.eventTypeLabel)}</span>
                </div>
                <p>${escapeHtml(item.login)}${item.department ? ` - ${escapeHtml(item.department)}` : ""}</p>
                <p>${escapeHtml(item.authenticationProvider)}${item.origin ? ` - ${escapeHtml(item.origin)}` : ""}${item.ipAddress ? ` - IP ${escapeHtml(item.ipAddress)}` : ""}</p>
                ${item.failureReason ? `<p>${escapeHtml(item.failureReason)}</p>` : ""}
                <span class="admin-activity-item__meta">${escapeHtml(item.loggedAtLabel || "Sem horario registrado")}</span>
              </article>
            `).join("")
            : renderEmptyState(
              "Sem eventos de autenticacao",
              "Logins, falhas, logout e origem/IP passarao a aparecer nesta trilha."
            )}
        </div>
      </section>

      <section class="card comm-list-card">
        <div class="card-header">Auditoria administrativa</div>
        <div class="comm-list-body">
          ${recentAuditEntries.length
            ? recentAuditEntries.map(renderAdminAuditItem).join("")
            : renderEmptyState(
              "Sem acoes administrativas",
              "Mudancas de status e perfil passarao a ficar registradas nesta trilha."
            )}
        </div>
      </section>

      <section class="card comm-list-card">
        <div class="card-header">Auditoria de humor</div>
        <div class="comm-list-body">
          ${recentMoodSurveyEntries.length
            ? recentMoodSurveyEntries.map(renderMoodAuditActivityItem).join("")
            : renderEmptyState(
              "Sem registros de humor",
              "Quando colaboradores responderem a pesquisa diaria, a trilha aparecera aqui."
            )}
        </div>
      </section>
    </section>
  `;
}

function renderMoodAuditActivityItem(item) {
  return `
    <article class="admin-activity-item">
      <div class="admin-activity-item__top">
        <strong>${escapeHtml(item.portalUserDisplayName)}</strong>
        <span class="comm-tag">${escapeHtml(item.actionTypeLabel || "Humor registrado")}</span>
      </div>
      <p>${escapeHtml(item.optionEmoji || "🙂")} ${escapeHtml(item.optionLabel || item.optionKey)}${item.department ? ` • ${escapeHtml(item.department)}` : ""}</p>
      <p>${item.origin ? escapeHtml(item.origin) : "Portal"}${item.ipAddress ? ` • IP ${escapeHtml(item.ipAddress)}` : ""}</p>
      <span class="admin-activity-item__meta">${escapeHtml(item.surveyDateLabel || item.createdAtLabel || "Sem horario registrado")}</span>
    </article>
  `;
}

export function renderCommunicationsHub(communications) {
  const hasItems = Array.isArray(communications.items) && communications.items.length > 0;

  return `
    <section class="card communications-hero-card">
      <div class="communications-hero">
        <div class="communications-hero-copy">
          <span class="communications-eyebrow">${escapeHtml(communications.intro.eyebrow)}</span>
          <h1>${escapeHtml(communications.intro.title)}</h1>
          <p>${escapeHtml(communications.intro.subtitle)}</p>
        </div>
      </div>
    </section>

    <section class="comm-kpi-grid">
      ${(communications.kpis || []).map(renderKpiCard).join("")}
    </section>

    <section class="card comm-filters-card">
      <div class="card-header">Navegue por categoria</div>
      <div class="comm-filter-list">
        ${(communications.filters || []).map(renderFilterChip).join("")}
      </div>
    </section>

    <section class="card comm-list-card">
      <div class="card-header">Todos os comunicados</div>
      <div class="comm-list-body">
        ${hasItems
          ? communications.items.map(renderCommunicationItem).join("")
          : communications.loadError
            ? renderEmptyState(
              "Não foi possível carregar os comunicados",
              communications.loadError
            )
            : renderEmptyState(
              "Nenhum comunicado publicado",
              "Quando o primeiro comunicado for persistido no banco, ele aparecerá nesta central."
            )}
      </div>
    </section>
  `;
}

export function renderCommunicationDetailPage(communication) {
  if (!communication) {
    return `
      <section class="card">
        <div class="card-header">Comunicado</div>
        ${renderEmptyState(
          "Comunicado nao encontrado",
          "O item solicitado nao esta disponivel ou ainda nao foi publicado na central oficial."
        )}
      </section>
    `;
  }

  return `
    <section class="card communication-detail-card">
      <div class="card-header">
        <a href="#comunicacao" class="comm-breadcrumb">Comunicacao</a>
        <span>/</span>
        <span>Leitura do comunicado</span>
      </div>
      <div class="communication-detail-body">
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(communication.category)}</span>
          <span class="comm-tag">${escapeHtml(communication.priority)}</span>
          <span class="comm-status">${escapeHtml(communication.status)}</span>
        </div>

        <h1>${escapeHtml(communication.title)}</h1>

        <div class="communication-detail-facts">
          <span><i class="fa-regular fa-calendar"></i>${escapeHtml(communication.publishedAt)}</span>
          <span><i class="fa-solid fa-users"></i>${escapeHtml(communication.audience)}</span>
          <span><i class="fa-solid fa-tower-broadcast"></i>${escapeHtml(communication.channel)}</span>
        </div>

        ${communication.image ? `
          <div class="communication-detail-media">
            <img src="${escapeHtml(communication.image)}" alt="${escapeHtml(communication.imageAlt || communication.title)}">
          </div>
        ` : ""}

        <div class="communication-detail-summary">
          <strong>Resumo oficial</strong>
          <p>${escapeHtml(communication.summary)}</p>
        </div>

        <div class="communication-detail-content">
          ${renderBodyParagraphs(communication.body)}
        </div>

        <div class="communication-detail-actions">
          <a href="#comunicacao" class="comm-secondary-button">Voltar para central</a>
          ${communication.id ? `
            <button
              type="button"
              class="comm-secondary-button ${communication.hasLiked ? "is-active" : ""}"
              data-action="toggle-feed-like"
              data-feed-item-id="${escapeHtml(communication.id)}"
              data-feed-source="Communication"
              aria-pressed="${communication.hasLiked ? "true" : "false"}"
            >
              <i class="fa-solid fa-thumbs-up" aria-hidden="true"></i>
              Curtir (<span data-communication-like-count>${escapeHtml(String(communication.likeCount ?? 0))}</span>)
            </button>
            <button
              type="button"
              class="comm-secondary-button ${communication.hasSaved ? "is-active" : ""}"
              data-action="toggle-feed-save"
              data-feed-item-id="${escapeHtml(communication.id)}"
              data-feed-source="Communication"
              aria-pressed="${communication.hasSaved ? "true" : "false"}"
            >
              <i class="fa-regular fa-bookmark" aria-hidden="true"></i>
              Salvar
            </button>
          ` : ""}
          <button
            type="button"
            class="feed-composer-submit"
            data-feedback-message="${escapeHtml(communication.attachmentLabel)} iniciado em modo demonstrativo."
            data-feedback-tone="info"
          >
            ${escapeHtml(communication.attachmentLabel)}
          </button>
        </div>
      </div>
    </section>
  `;
}

export function renderCommunicationAdminPage(communications, { layout = "legacy", selectedCommunication = null } = {}) {
  const isRhLayout = layout === "rh";
  const items = Array.isArray(communications.items) ? communications.items : [];
  const editing = Boolean(selectedCommunication?.id);
  const formComm = selectedCommunication
    ? mapCommunicationToForm(selectedCommunication)
    : null;

  const listSection = `
    <section class="card comm-list-card">
      <div class="card-header poll-admin-list__header">
        <span>Comunicados publicados e rascunhos</span>
        <button type="button" class="feed-composer-submit" data-action="admin-communication-create">
          <i class="fa-solid fa-plus" aria-hidden="true"></i>
          Novo comunicado
        </button>
      </div>
      <div class="comm-list-body poll-admin-list">
        ${items.length
          ? items.map(renderCommunicationAdminListItem).join("")
          : communications.loadError
            ? renderEmptyState("Nao foi possivel carregar os comunicados", communications.loadError)
            : renderEmptyState("Nenhum comunicado cadastrado", "Publique o primeiro comunicado para iniciar o modulo editorial de RH.")}
      </div>
    </section>
  `;
  const kpiSection = `
    <section class="comm-kpi-grid">
      ${(communications.kpis || []).map(renderKpiCard).join("")}
    </section>
  `;
  const wizardModal = renderCommunicationAdminWizardModal(communications, formComm, editing);

  if (isRhLayout) {
    return `
      ${renderRhAdminHero({
        title: "Comunicados",
        description: "Publique comunicados oficiais, acompanhe indicadores editoriais e gerencie o ciclo de vida das publicacoes institucionais."
      })}
      ${kpiSection}
      <section class="poll-admin-layout poll-admin-layout--list-only">
        ${listSection}
      </section>
      ${wizardModal}
    `;
  }

  return `
    <section class="card communication-admin-hero-card">
      <div class="communication-admin-hero">
        <div class="communication-admin-copy">
          <span class="communications-eyebrow">AREA RESTRITA</span>
          <h1>Publicacao de comunicados oficiais</h1>
          <p>Ambiente editorial reservado para criacao, revisao e publicacao de comunicados institucionais.</p>
        </div>
        <div class="communication-admin-meta">
          <a href="#configuracoes" class="comm-secondary-button">Abrir configuracoes</a>
          <button type="button" class="comm-secondary-button" data-action="admin-logout">Sair da area admin</button>
        </div>
      </div>
    </section>

    ${kpiSection}

    <section class="poll-admin-layout poll-admin-layout--list-only">
      ${listSection}
    </section>
    ${wizardModal}
  `;
}

function renderCommunicationAdminListItem(item) {
  const isArchived = String(item.status || "").toLowerCase() === "arquivado";
  const statusAction = isArchived
    ? { action: "admin-communication-reactivate", label: "Reativar", nextStatus: "Publicado" }
    : { action: "admin-communication-archive", label: "Inativar", nextStatus: "Arquivado" };

  return `
    <article class="comm-item-card poll-admin-card" data-communication-id="${escapeHtml(item.id)}">
      <div class="comm-item-top poll-admin-card__top">
        <div class="comm-meta-row">
          <span class="comm-tag comm-tag--solid">${escapeHtml(item.category)}</span>
          <span class="comm-tag">${escapeHtml(item.priority)}</span>
        </div>
        <span class="comm-status">${escapeHtml(item.status)}</span>
      </div>
      <h3>${escapeHtml(item.title)}</h3>
      <p>${escapeHtml(item.summary)}</p>
      <div class="comm-item-facts">
        <span><i class="fa-regular fa-calendar"></i>${escapeHtml(item.publishedAt)}</span>
        <span><i class="fa-solid fa-users"></i>${escapeHtml(item.audience)}</span>
        <span><i class="fa-solid fa-tower-broadcast"></i>${escapeHtml(item.channel)}</span>
      </div>
      <div class="comm-item-actions poll-admin-card__actions">
        ${renderReadLink(item.slug, "Ver publico", "comm-tertiary-button")}
        <button type="button" class="comm-inline-action" data-action="admin-communication-edit" data-communication-id="${escapeHtml(item.id)}">
          Editar
        </button>
        <button
          type="button"
          class="comm-secondary-button"
          data-action="${statusAction.action}"
          data-communication-id="${escapeHtml(item.id)}"
          data-next-status="${escapeHtml(statusAction.nextStatus)}"
        >
          ${statusAction.label}
        </button>
        <button type="button" class="comm-tertiary-button" data-action="admin-communication-delete" data-communication-id="${escapeHtml(item.id)}">
          Excluir
        </button>
      </div>
    </article>
  `;
}

export function renderAdminSettingsPage() {
  return `
    <section class="card communication-admin-hero-card">
      <div class="communication-admin-hero">
        <div class="communication-admin-copy">
          <span class="communications-eyebrow">CONFIGURACOES RESTRITAS</span>
          <h1>Parametros de acesso administrativo</h1>
          <p>Centralize as configuracoes de autenticacao corporativa e demais ajustes tecnicos fora do fluxo editorial de publicacao.</p>
        </div>
        <div class="communication-admin-meta">
          <a href="#comunicacao/restrita" class="comm-secondary-button">Voltar para editorial</a>
          <button type="button" class="comm-secondary-button" data-action="admin-logout">Sair da area admin</button>
        </div>
      </div>
    </section>

    <section class="communication-admin-layout">
      <div class="communication-admin-main">
        <section class="card communication-form-card">
          <div class="card-header">Governanca administrativa</div>
          <div class="communication-section-intro">
            <strong>Atalhos de gestao</strong>
            <p>Organize os acessos sensiveis da intranet em areas dedicadas para configuracao e administracao operacional.</p>
          </div>
          <div class="comm-item-actions">
            <a href="#admin/usuarios" class="feed-composer-submit">Gerenciar usuarios do portal</a>
            <a href="#comunicacao/restrita" class="comm-secondary-button">Abrir editorial</a>
          </div>
        </section>
        ${renderLdapSettingsCard()}
        ${renderMicrosoftGraphSettingsCard()}
      </div>
    </section>
  `;
}

export function renderAdminUsersPage(pageData = createEmptyPortalUsersPage(), loadError = "") {
  const viewModel = {
    ...createEmptyPortalUsersPage(),
    ...(pageData || {})
  };
  const items = Array.isArray(viewModel.items) ? viewModel.items : [];
  const roleOptions = Array.isArray(viewModel.roleOptions) ? viewModel.roleOptions : [];
  const accessLevelOptions = Array.isArray(viewModel.accessLevelOptions) ? viewModel.accessLevelOptions : [];
  const recentLogins = Array.isArray(viewModel.recentLogins) ? viewModel.recentLogins : [];
  const recentAuditEntries = Array.isArray(viewModel.recentAuditEntries) ? viewModel.recentAuditEntries : [];
  const recentMoodSurveyEntries = Array.isArray(viewModel.recentMoodSurveyEntries) ? viewModel.recentMoodSurveyEntries : [];
  const summary = {
    ...createEmptyPortalUsersPage().summary,
    ...(viewModel.summary || {})
  };
  const departmentOptions = Array.isArray(viewModel.departmentOptions) ? viewModel.departmentOptions : [];

  return `
    <section class="card communication-admin-hero-card">
      <div class="communication-admin-hero">
        <div class="communication-admin-copy">
          <span class="communications-eyebrow">GESTAO DE ACESSO</span>
          <h1>Usuarios registrados na intranet</h1>
          <p>Visualize quem ja acessou a LIOCONNECTA via LDAP e administre o status de acesso de cada colaborador registrado.</p>
        </div>
        <div class="communication-admin-meta">
          <a href="#configuracoes" class="comm-secondary-button">Voltar para configuracoes</a>
          <button type="button" class="comm-secondary-button" data-action="admin-logout">Sair da area admin</button>
        </div>
      </div>
    </section>

    <div id="admin-users-kpis-host">
      ${renderAdminUsersKpiSection(summary)}
    </div>

    <section class="card communication-form-card">
      <div class="card-header">Busca e filtros</div>
      <div class="communication-form-grid">
        <label class="communication-form-field communication-form-field--full">
          <span>Pesquisar usuario</span>
          <input id="admin-user-search" type="text" value="${escapeHtml(viewModel.query || "")}" placeholder="Nome, login, e-mail, departamento ou cargo" />
        </label>
        <label class="communication-form-field">
          <span>Status</span>
          <select id="admin-user-status-filter">
            <option value="all" ${renderSelected(viewModel.status, "all")}>Todos</option>
            <option value="active" ${renderSelected(viewModel.status, "active")}>Somente ativos</option>
            <option value="inactive" ${renderSelected(viewModel.status, "inactive")}>Somente inativos</option>
          </select>
        </label>
        <label class="communication-form-field">
          <span>Perfil</span>
          <select id="admin-user-role-filter">
            <option value="all">Todos os perfis</option>
            ${roleOptions.map((option) => `
              <option value="${escapeHtml(option.key)}" ${renderSelected(viewModel.role, option.key)}>
                ${escapeHtml(option.label)}
              </option>
            `).join("")}
          </select>
        </label>
        <label class="communication-form-field">
          <span>Departamento</span>
          <select id="admin-user-department-filter">
            <option value="all">Todos os departamentos</option>
            ${departmentOptions.map((option) => `
              <option value="${escapeHtml(option.key || option.label || "")}" ${renderSelected(viewModel.department, option.key || option.label || "")}>
                ${escapeHtml(option.label || option.key || "")}${typeof option.count === "number" ? ` (${escapeHtml(String(option.count))})` : ""}
              </option>
            `).join("")}
          </select>
        </label>
      </div>
    </section>

    <div id="admin-users-results-host">
      ${renderAdminUsersResultsSection(viewModel, loadError)}
    </div>

    ${renderPortalUsersModalShell()}

    <div id="admin-users-activity-host">
      ${renderAdminUsersActivitySection({
        ...viewModel,
        recentLogins,
        recentAuditEntries,
        recentMoodSurveyEntries
      })}
    </div>
  `;
}

export { initCarousel, renderCarouselSection };
