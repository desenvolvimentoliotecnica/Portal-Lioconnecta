import { escapeHtml } from "../components/html.js";

const USER_IDENTIFIER_OPTIONS = Object.freeze([
  { value: "userPrincipalName", label: "User Principal Name (UPN)" },
  { value: "mail", label: "E-mail (mail)" }
]);

function renderChecked(value) {
  return value ? "checked" : "";
}

function renderUserIdentifierOptions(selectedValue) {
  return USER_IDENTIFIER_OPTIONS.map((option) => `
    <option value="${escapeHtml(option.value)}" ${option.value === selectedValue ? "selected" : ""}>
      ${escapeHtml(option.label)}
    </option>
  `).join("");
}

export function collectMicrosoftGraphSettingsPayload(form) {
  const formData = new FormData(form);
  return {
    isEnabled: Boolean(formData.get("isEnabled")),
    tenantId: String(formData.get("tenantId") || "").trim(),
    clientId: String(formData.get("clientId") || "").trim(),
    clientSecret: String(formData.get("clientSecret") || ""),
    userIdentifier: String(formData.get("userIdentifier") || "userPrincipalName")
  };
}

export function initMicrosoftGraphSettings(root = document) {
  const form = root.querySelector("#microsoft-graph-settings-form");
  if (!form) {
    return;
  }

  const passwordToggle = form.querySelector("[data-action='microsoft-graph-toggle-secret']");
  const secretInput = form.querySelector("[name='clientSecret']");

  passwordToggle?.addEventListener("click", () => {
    if (!secretInput) {
      return;
    }

    const isPassword = secretInput.type === "password";
    secretInput.type = isPassword ? "text" : "password";
    passwordToggle.setAttribute("aria-label", isPassword ? "Ocultar segredo" : "Mostrar segredo");
    passwordToggle.innerHTML = isPassword
      ? '<i class="fa-solid fa-eye-slash" aria-hidden="true"></i>'
      : '<i class="fa-solid fa-eye" aria-hidden="true"></i>';
  });
}

export function renderMicrosoftGraphSettingsPage(settings = {}) {
  const loadNotice = settings.loadError
    ? `<div class="ldap-wizard__alert ldap-wizard__alert--danger">${escapeHtml(settings.loadError)}</div>`
    : "";

  const updatedAtLabel = settings.updatedAtUtc
    ? new Date(settings.updatedAtUtc).toLocaleString("pt-BR")
    : "Nunca salvo";

  return `
    <section class="ldap-wizard">
      <header class="ldap-wizard__hero">
        <div class="ldap-wizard__hero-copy">
          <h1>Microsoft 365 / Graph</h1>
          <p class="ldap-wizard__subtitle">Integracao da agenda corporativa</p>
          <p class="ldap-wizard__description">
            Informe o Client ID, Tenant ID e Client Secret fornecidos pela infraestrutura para
            habilitar a leitura da agenda do dia via Microsoft Graph (permissoes de aplicativo).
          </p>
        </div>
        <div class="ldap-wizard__hero-icon" aria-hidden="true">
          <i class="fa-regular fa-calendar-days"></i>
        </div>
      </header>

      ${loadNotice}

      <form id="microsoft-graph-settings-form" class="ldap-wizard__form" novalidate>
        <section class="ldap-wizard__panel">
          <div class="ldap-wizard__panel-head">
            <h2>Credenciais do App Registration</h2>
            <p>Valores entregues pelo time de infraestrutura apos o admin consent no Entra ID.</p>
          </div>

          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="isEnabled" ${renderChecked(settings.isEnabled)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Habilitar integracao Microsoft Graph</span>
          </label>

          <label class="ldap-wizard__field">
            <span>Tenant ID (Locatario)</span>
            <input
              name="tenantId"
              type="text"
              value="${escapeHtml(settings.tenantId || "")}"
              placeholder="b95b38fc-0302-4cf4-8c95-d45754f48411"
              autocomplete="off"
              required
            />
          </label>

          <label class="ldap-wizard__field">
            <span>Client ID (Aplicativo)</span>
            <input
              name="clientId"
              type="text"
              value="${escapeHtml(settings.clientId || "")}"
              placeholder="3e73d586-06c1-455c-86a7-07c2a89c383d"
              autocomplete="off"
              required
            />
          </label>

          <label class="ldap-wizard__field">
            <span>Client Secret (Valor segredo)</span>
            <div class="ldap-wizard__password-wrap">
              <input
                name="clientSecret"
                type="password"
                placeholder="${settings.hasClientSecret ? "Segredo ja cadastrado" : "Cole o valor segredo do App Registration"}"
                autocomplete="new-password"
              />
              <button type="button" class="ldap-wizard__password-toggle" data-action="microsoft-graph-toggle-secret" aria-label="Mostrar segredo">
                <i class="fa-solid fa-eye" aria-hidden="true"></i>
              </button>
            </div>
            <small class="ldap-wizard__hint">
              ${settings.hasClientSecret
    ? "Ja existe um segredo persistido com protecao no backend. Deixe em branco para manter o valor atual."
    : "O segredo sera persistido com protecao no backend e nunca sera exibido novamente."}
            </small>
          </label>

          <label class="ldap-wizard__field">
            <span>Identificador do usuario no Graph</span>
            <select name="userIdentifier">
              ${renderUserIdentifierOptions(settings.userIdentifier || "userPrincipalName")}
            </select>
            <small class="ldap-wizard__hint">
              Atributo usado para localizar o colaborador no Microsoft 365 a partir do cadastro LDAP.
            </small>
          </label>

          <div class="ldap-wizard__info ldap-wizard__info--inline">
            <i class="fa-solid fa-circle-info" aria-hidden="true"></i>
            <span>Ultima atualizacao: ${escapeHtml(updatedAtLabel)}</span>
          </div>
        </section>

        <footer class="ldap-wizard__footer">
          <a href="#configuracoes" class="ldap-wizard__button ldap-wizard__button--ghost">
            Cancelar
          </a>
          <div class="ldap-wizard__footer-actions">
            <button type="submit" name="submitMode" value="test" class="ldap-wizard__button ldap-wizard__button--secondary">
              Testar conexao
            </button>
            <button type="submit" name="submitMode" value="save" class="ldap-wizard__button ldap-wizard__button--primary">
              Salvar configuracao
            </button>
          </div>
        </footer>
      </form>
    </section>
  `;
}
