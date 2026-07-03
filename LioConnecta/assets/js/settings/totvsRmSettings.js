import { escapeHtml } from "../components/html.js";

function renderChecked(value) {
  return value ? "checked" : "";
}

function isLegacyModuleFlags(settings = {}) {
  return Boolean(settings.isEnabled)
    && !settings.enableCadastro
    && !settings.enableHolerite
    && !settings.enableFerias
    && !settings.enableBeneficios
    && !settings.enablePonto
    && !settings.enableTeamDashboard;
}

function renderModuleChecked(settings, value) {
  return renderChecked(isLegacyModuleFlags(settings) || value !== false);
}

export function collectTotvsRmSettingsPayload(form) {
  const formData = new FormData(form);
  return {
    isEnabled: Boolean(formData.get("isEnabled")),
    server: String(formData.get("server") || "").trim(),
    port: Number(formData.get("port") || 1433),
    database: String(formData.get("database") || "").trim(),
    userName: String(formData.get("userName") || "").trim(),
    password: String(formData.get("password") || ""),
    trustServerCertificate: Boolean(formData.get("trustServerCertificate")),
    codColigada: Number(formData.get("codColigada") || 1),
    enableCadastro: Boolean(formData.get("enableCadastro")),
    enableHolerite: Boolean(formData.get("enableHolerite")),
    enableFerias: Boolean(formData.get("enableFerias")),
    enableBeneficios: Boolean(formData.get("enableBeneficios")),
    enablePonto: Boolean(formData.get("enablePonto")),
    enableTeamDashboard: Boolean(formData.get("enableTeamDashboard"))
  };
}

export function initTotvsRmSettings(root = document) {
  const form = root.querySelector("#totvs-rm-settings-form");
  if (!form) {
    return;
  }

  const passwordToggle = form.querySelector("[data-action='totvs-rm-toggle-password']");
  const passwordInput = form.querySelector("[name='password']");

  passwordToggle?.addEventListener("click", () => {
    if (!passwordInput) {
      return;
    }

    const isPassword = passwordInput.type === "password";
    passwordInput.type = isPassword ? "text" : "password";
    passwordToggle.setAttribute("aria-label", isPassword ? "Ocultar senha" : "Mostrar senha");
    passwordToggle.innerHTML = isPassword
      ? '<i class="fa-solid fa-eye-slash" aria-hidden="true"></i>'
      : '<i class="fa-solid fa-eye" aria-hidden="true"></i>';
  });
}

export function renderTotvsRmSettingsPage(settings = {}) {
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
          <h1>TOTVS RM</h1>
          <p class="ldap-wizard__subtitle">Integracao read-only com TOTVS RM (Corpore)</p>
          <p class="ldap-wizard__description">
            Configure a conexao SQL Server read-only com o banco Corpore para consultar cadastro, holerite,
            ferias, beneficios e ponto na secao MEU PERFIL RH.
          </p>
        </div>
        <div class="ldap-wizard__hero-icon" aria-hidden="true">
          <i class="fa-solid fa-clock"></i>
        </div>
      </header>

      ${loadNotice}

      <form id="totvs-rm-settings-form" class="ldap-wizard__form" novalidate>
        <section class="ldap-wizard__panel">
          <div class="ldap-wizard__panel-head">
            <h2>Conexao SQL Server</h2>
            <p>Credenciais com permissao de leitura nas tabelas PFUNC, PPESSOA, PFFINANC, PFUFERIAS, ABATFUN e AAFHTFUN.</p>
          </div>

          <label class="ldap-wizard__field">
            <span>CodColigada</span>
            <input
              name="codColigada"
              type="number"
              min="1"
              max="32767"
              value="${escapeHtml(String(settings.codColigada || 1))}"
              required
            />
          </label>

          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="isEnabled" ${renderChecked(settings.isEnabled)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Habilitar integracao TOTVS RM</span>
          </label>

          <label class="ldap-wizard__field">
            <span>Servidor SQL</span>
            <input
              name="server"
              type="text"
              value="${escapeHtml(settings.server || "")}"
              placeholder="sqlserver.empresa.local"
              autocomplete="off"
              required
            />
          </label>

          <label class="ldap-wizard__field">
            <span>Porta</span>
            <input
              name="port"
              type="number"
              min="1"
              max="65535"
              value="${escapeHtml(String(settings.port || 1433))}"
              required
            />
          </label>

          <label class="ldap-wizard__field">
            <span>Database</span>
            <input
              name="database"
              type="text"
              value="${escapeHtml(settings.database || "")}"
              placeholder="Corpore"
              autocomplete="off"
              required
            />
          </label>

          <label class="ldap-wizard__field">
            <span>Usuario SQL</span>
            <input
              name="userName"
              type="text"
              value="${escapeHtml(settings.userName || "")}"
              placeholder="portal_rm_read"
              autocomplete="off"
              required
            />
          </label>

          <label class="ldap-wizard__field">
            <span>Senha SQL</span>
            <div class="ldap-wizard__password-wrap">
              <input
                name="password"
                type="password"
                placeholder="${settings.hasPassword ? "Senha ja cadastrada" : "Informe a senha do usuario SQL"}"
                autocomplete="new-password"
              />
              <button type="button" class="ldap-wizard__password-toggle" data-action="totvs-rm-toggle-password" aria-label="Mostrar senha">
                <i class="fa-solid fa-eye" aria-hidden="true"></i>
              </button>
            </div>
            <small class="ldap-wizard__hint">
              ${settings.hasPassword
    ? "Ja existe uma senha persistida com protecao no backend. Deixe em branco para manter o valor atual."
    : "A senha sera persistida com protecao no backend e nunca sera exibida novamente."}
            </small>
          </label>

          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="trustServerCertificate" ${renderChecked(settings.trustServerCertificate !== false)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Confiar no certificado do servidor (TrustServerCertificate)</span>
          </label>

          <div class="ldap-wizard__panel-head">
            <h2>Modulos habilitados</h2>
            <p>Controle quais consultas RH usam o RM neste ambiente.</p>
          </div>

          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="enableCadastro" ${renderModuleChecked(settings, settings.enableCadastro)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Dados cadastrais</span>
          </label>
          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="enableHolerite" ${renderModuleChecked(settings, settings.enableHolerite)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Holerite</span>
          </label>
          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="enableFerias" ${renderModuleChecked(settings, settings.enableFerias)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Ferias</span>
          </label>
          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="enableBeneficios" ${renderModuleChecked(settings, settings.enableBeneficios)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Beneficios</span>
          </label>
          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="enablePonto" ${renderModuleChecked(settings, settings.enablePonto)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Ponto</span>
          </label>
          <label class="ldap-wizard__toggle">
            <input type="checkbox" name="enableTeamDashboard" ${renderModuleChecked(settings, settings.enableTeamDashboard)} />
            <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
            <span class="ldap-wizard__toggle-label">Dashboard de equipe (gestores)</span>
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
