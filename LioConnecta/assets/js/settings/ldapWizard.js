import { escapeHtml } from "../components/html.js";

const WIZARD_STEPS = Object.freeze([
  { id: 1, label: "Conexao" },
  { id: 2, label: "Busca e autenticacao" },
  { id: 3, label: "Atributos" },
  { id: 4, label: "Resumo" }
]);

const LOGIN_FORMAT_LABELS = Object.freeze({
  "domain-backslash-samaccountname": "Dominio\\usuario (sAMAccountName)",
  "email-or-upn-or-samaccountname": "E-mail, UPN ou sAMAccountName",
  userprincipalname: "userPrincipalName (UPN)",
  mail: "E-mail (mail)"
});

function renderChecked(value) {
  return value ? "checked" : "";
}

function renderSelected(current, value) {
  return current === value ? "selected" : "";
}

function renderStepper(activeStep) {
  return `
    <nav class="ldap-wizard__stepper" aria-label="Etapas da configuracao LDAP">
      ${WIZARD_STEPS.map((step, index) => {
        const state = step.id < activeStep ? "is-complete" : step.id === activeStep ? "is-active" : "";
        const connector = index < WIZARD_STEPS.length - 1
          ? `<span class="ldap-wizard__step-connector ${step.id < activeStep ? "is-complete" : ""}" aria-hidden="true"></span>`
          : "";

        return `
          <div class="ldap-wizard__step ${state}">
            <span class="ldap-wizard__step-badge">${step.id}</span>
            <span class="ldap-wizard__step-label">${escapeHtml(step.label)}</span>
          </div>
          ${connector}
        `;
      }).join("")}
    </nav>
  `;
}

function renderSecurityOption(name, label, checked, iconClass, tone = "neutral") {
  return `
    <label class="ldap-wizard__security-option ldap-wizard__security-option--${tone}">
      <input type="checkbox" name="${escapeHtml(name)}" ${renderChecked(checked)} />
      <span class="ldap-wizard__security-icon" aria-hidden="true"><i class="${iconClass}"></i></span>
      <span>${escapeHtml(label)}</span>
    </label>
  `;
}

function renderStepConnection(ldapSettings) {
  return `
    <section class="ldap-wizard__panel" data-ldap-step-panel="1">
      <div class="ldap-wizard__panel-head">
        <div>
          <h2>1. Conexao com o servidor LDAP</h2>
          <p>Configure como conectar ao seu servidor de diretorio.</p>
        </div>
        <label class="ldap-wizard__toggle">
          <input type="checkbox" name="isEnabled" ${renderChecked(ldapSettings.isEnabled)} />
          <span class="ldap-wizard__toggle-track" aria-hidden="true"></span>
          <span class="ldap-wizard__toggle-label">Habilitar login LDAP</span>
        </label>
      </div>

      <div class="ldap-wizard__field-grid ldap-wizard__field-grid--server-port">
        <label class="ldap-wizard__field ldap-wizard__field--grow">
          <span>Servidor LDAP</span>
          <input name="server" type="text" value="${escapeHtml(ldapSettings.server || "")}" placeholder="dc-virtual-02.liotecnica.com.br" autocomplete="off" />
        </label>
        <label class="ldap-wizard__field ldap-wizard__field--port">
          <span>Porta</span>
          <input name="port" type="number" min="1" max="65535" value="${escapeHtml(String(ldapSettings.port || 389))}" />
        </label>
      </div>

      <div class="ldap-wizard__security-group">
        ${renderSecurityOption("useLdaps", "Usar LDAPS (SSL) - recomendado na porta 636", ldapSettings.useLdaps, "fa-solid fa-lock", "success")}
        ${renderSecurityOption("useStartTls", "Usar StartTLS na porta 389 (nao marque junto com LDAPS)", ldapSettings.useStartTls, "fa-solid fa-shield-halved", "info")}
        ${renderSecurityOption("ignoreCertificateValidation", "Ignorar validacao do certificado (somente ambientes internos/HMG)", ldapSettings.ignoreCertificateValidation, "fa-solid fa-triangle-exclamation", "warning")}
      </div>

      <label class="ldap-wizard__field">
        <span class="ldap-wizard__field-label-row">
          <span>Base DN</span>
          <button type="button" class="ldap-wizard__field-help" title="Distinguished Name base do diretorio" aria-label="Ajuda sobre Base DN">
            <i class="fa-regular fa-circle-question"></i>
          </button>
        </span>
        <input name="baseDn" type="text" value="${escapeHtml(ldapSettings.baseDn || "")}" placeholder="DC=liotecnica,DC=com,DC=br" autocomplete="off" />
      </label>

      <label class="ldap-wizard__field">
        <span class="ldap-wizard__field-label-row">
          <span>Base de busca de usuarios (opcional)</span>
          <button type="button" class="ldap-wizard__field-help" title="OU ou container onde os usuarios serao pesquisados" aria-label="Ajuda sobre base de busca">
            <i class="fa-regular fa-circle-question"></i>
          </button>
        </span>
        <input name="userSearchBase" type="text" value="${escapeHtml(ldapSettings.userSearchBase || "")}" placeholder="OU=Departamentos,DC=liotecnica,DC=com,DC=br" autocomplete="off" />
      </label>

      <label class="ldap-wizard__field">
        <span>Dominio Windows (NETBIOS)</span>
        <input name="netbiosDomain" type="text" value="${escapeHtml(ldapSettings.netbiosDomain || "")}" placeholder="LIOTECNICA" autocomplete="off" />
        <small class="ldap-wizard__hint">Obrigatorio quando o formato de login for DOMINIO\\usuario.</small>
      </label>

      <label class="ldap-wizard__field">
        <span>Conta de servico (Bind DN)</span>
        <input name="bindDn" type="text" value="${escapeHtml(ldapSettings.bindDn || "")}" placeholder="CN=servico-hub,OU=ServiceAccounts,DC=..." autocomplete="off" />
        <small class="ldap-wizard__hint">Obrigatorio no modo de busca no diretorio. Opcional no teste inicial de conexao.</small>
      </label>

      <label class="ldap-wizard__field">
        <span>Senha da conta de servico</span>
        <div class="ldap-wizard__password-wrap">
          <input
            name="serviceAccountPassword"
            type="password"
            value=""
            placeholder="${ldapSettings.hasServiceAccountPassword ? "Senha ja cadastrada" : "Digite a senha da conta de servico"}"
            autocomplete="new-password"
          />
          <button type="button" class="ldap-wizard__password-toggle" data-action="ldap-toggle-password" aria-label="Mostrar senha">
            <i class="fa-regular fa-eye-slash"></i>
          </button>
        </div>
        <small class="ldap-wizard__hint">${ldapSettings.hasServiceAccountPassword ? "Ja existe uma senha persistida. Deixe em branco para manter." : "A senha sera persistida com protecao no backend."}</small>
      </label>
    </section>
  `;
}

function renderStepSearch(ldapSettings) {
  const loginFormat = ldapSettings.loginFormat || "email-or-upn-or-samaccountname";

  return `
    <section class="ldap-wizard__panel is-hidden" data-ldap-step-panel="2">
      <div class="ldap-wizard__panel-head">
        <div>
          <h2>2. Busca e autenticacao</h2>
          <p>Defina como o portal localiza e autentica os colaboradores no diretorio.</p>
        </div>
      </div>

      <label class="ldap-wizard__field">
        <span>Formato de login</span>
        <select name="loginFormat">
          <option value="domain-backslash-samaccountname" ${renderSelected(loginFormat, "domain-backslash-samaccountname")}>Dominio\\usuario (sAMAccountName)</option>
          <option value="email-or-upn-or-samaccountname" ${renderSelected(loginFormat, "email-or-upn-or-samaccountname")}>E-mail, UPN ou sAMAccountName</option>
          <option value="userprincipalname" ${renderSelected(loginFormat, "userprincipalname")}>userPrincipalName (UPN)</option>
          <option value="mail" ${renderSelected(loginFormat, "mail")}>E-mail (mail)</option>
        </select>
      </label>

      <label class="ldap-wizard__field">
        <span class="ldap-wizard__field-label-row">
          <span>Filtro LDAP de busca</span>
          <button type="button" class="ldap-wizard__field-help" title="Use {0} para o login informado pelo colaborador" aria-label="Ajuda sobre filtro LDAP">
            <i class="fa-regular fa-circle-question"></i>
          </button>
        </span>
        <input
          name="searchFilter"
          type="text"
          value="${escapeHtml(ldapSettings.searchFilter || "(|(mail={0})(userPrincipalName={0})(sAMAccountName={0}))")}"
          placeholder="(|(mail={0})(userPrincipalName={0})(sAMAccountName={0}))"
          autocomplete="off"
        />
        <small class="ldap-wizard__hint">Use {0} para o login informado pelo colaborador.</small>
      </label>

      <div class="ldap-wizard__info ldap-wizard__info--inline">
        <i class="fa-solid fa-circle-info" aria-hidden="true"></i>
        <span>Voce podera testar a conexao ao avancar para o resumo ou salvar a configuracao.</span>
      </div>
    </section>
  `;
}

function renderStepAttributes(ldapSettings) {
  return `
    <section class="ldap-wizard__panel is-hidden" data-ldap-step-panel="3">
      <div class="ldap-wizard__panel-head">
        <div>
          <h2>3. Atributos do diretorio</h2>
          <p>Escolha quais atributos do AD serao usados para exibir o nome do colaborador.</p>
        </div>
      </div>

      <label class="ldap-wizard__field">
        <span>Atributo de nome exibido</span>
        <input name="displayNameAttribute" type="text" value="${escapeHtml(ldapSettings.displayNameAttribute || "displayName")}" placeholder="displayName" autocomplete="off" />
        <small class="ldap-wizard__hint">Atributo LDAP usado para preencher o nome exibido no portal.</small>
      </label>
    </section>
  `;
}

function renderStepSummary() {
  return `
    <section class="ldap-wizard__panel is-hidden" data-ldap-step-panel="4">
      <div class="ldap-wizard__panel-head">
        <div>
          <h2>4. Resumo da configuracao</h2>
          <p>Revise os parametros antes de salvar no banco de dados do portal.</p>
        </div>
      </div>
      <div class="ldap-wizard__summary" data-ldap-summary></div>
    </section>
  `;
}

export function renderLdapWizardPage(ldapSettings = {}) {
  const loadNotice = ldapSettings.loadError
    ? `<div class="ldap-wizard__alert ldap-wizard__alert--danger">${escapeHtml(ldapSettings.loadError)}</div>`
    : "";

  return `
    <section class="ldap-wizard">
      <header class="ldap-wizard__hero">
        <div class="ldap-wizard__hero-copy">
          <h1>Active Directory / LDAP</h1>
          <p class="ldap-wizard__subtitle">Configure autenticacao corporativa</p>
          <p class="ldap-wizard__description">
            Defina os parametros do diretorio para o login por e-mail e senha dos colaboradores,
            mantendo o acesso da intranet restrito ao AD da empresa.
          </p>
        </div>
        <div class="ldap-wizard__hero-icon" aria-hidden="true">
          <i class="fa-solid fa-user-shield"></i>
        </div>
      </header>

      ${loadNotice}
      ${renderStepper(1)}

      <form id="ldap-settings-form" class="ldap-wizard__form" data-ldap-step="1" novalidate>
        ${renderStepConnection(ldapSettings)}
        ${renderStepSearch(ldapSettings)}
        ${renderStepAttributes(ldapSettings)}
        ${renderStepSummary()}

        <div class="ldap-wizard__info" data-ldap-step-info="1">
          <i class="fa-solid fa-circle-info" aria-hidden="true"></i>
          <span>Voce podera testar a conexao na proxima etapa.</span>
        </div>

        <footer class="ldap-wizard__footer">
          <button type="button" class="ldap-wizard__button ldap-wizard__button--ghost" data-action="ldap-wizard-cancel">
            Cancelar
          </button>
          <div class="ldap-wizard__footer-actions">
            <button type="button" class="ldap-wizard__button ldap-wizard__button--ghost is-hidden" data-action="ldap-wizard-prev">
              Voltar
            </button>
            <button type="button" class="ldap-wizard__button ldap-wizard__button--primary" data-action="ldap-wizard-next">
              Proximo <i class="fa-solid fa-arrow-right" aria-hidden="true"></i>
            </button>
            <button type="submit" name="submitMode" value="save-test" class="ldap-wizard__button ldap-wizard__button--secondary is-hidden" data-action="ldap-wizard-save-test">
              Salvar e testar conexao
            </button>
            <button type="submit" name="submitMode" value="save" class="ldap-wizard__button ldap-wizard__button--primary is-hidden" data-action="ldap-wizard-save">
              Salvar configuracao
            </button>
          </div>
        </footer>
      </form>
    </section>
  `;
}

function readFormValues(form) {
  const formData = new FormData(form);
  return {
    isEnabled: Boolean(formData.get("isEnabled")),
    server: String(formData.get("server") || "").trim(),
    port: Number(formData.get("port") || 389),
    useLdaps: Boolean(formData.get("useLdaps")),
    useStartTls: Boolean(formData.get("useStartTls")),
    ignoreCertificateValidation: Boolean(formData.get("ignoreCertificateValidation")),
    baseDn: String(formData.get("baseDn") || "").trim(),
    userSearchBase: String(formData.get("userSearchBase") || "").trim(),
    netbiosDomain: String(formData.get("netbiosDomain") || "").trim(),
    loginFormat: String(formData.get("loginFormat") || ""),
    bindDn: String(formData.get("bindDn") || "").trim(),
    serviceAccountPassword: String(formData.get("serviceAccountPassword") || ""),
    searchFilter: String(formData.get("searchFilter") || "").trim(),
    displayNameAttribute: String(formData.get("displayNameAttribute") || "").trim()
  };
}

function renderSummaryHtml(values) {
  const rows = [
    ["Login LDAP", values.isEnabled ? "Habilitado" : "Desabilitado"],
    ["Servidor", values.server || "—"],
    ["Porta", String(values.port || 389)],
    ["LDAPS", values.useLdaps ? "Sim" : "Nao"],
    ["StartTLS", values.useStartTls ? "Sim" : "Nao"],
    ["Ignorar certificado", values.ignoreCertificateValidation ? "Sim" : "Nao"],
    ["Base DN", values.baseDn || "—"],
    ["Base de busca", values.userSearchBase || "—"],
    ["Dominio NETBIOS", values.netbiosDomain || "—"],
    ["Bind DN", values.bindDn || "—"],
    ["Formato de login", LOGIN_FORMAT_LABELS[values.loginFormat] || values.loginFormat || "—"],
    ["Filtro LDAP", values.searchFilter || "—"],
    ["Atributo de nome", values.displayNameAttribute || "displayName"]
  ];

  return `
    <dl class="ldap-wizard__summary-list">
      ${rows.map(([label, value]) => `
        <div class="ldap-wizard__summary-row">
          <dt>${escapeHtml(label)}</dt>
          <dd>${escapeHtml(value)}</dd>
        </div>
      `).join("")}
    </dl>
  `;
}

function updateWizardStep(form, step) {
  const normalizedStep = Math.min(Math.max(step, 1), WIZARD_STEPS.length);
  form.dataset.ldapStep = String(normalizedStep);

  form.querySelectorAll("[data-ldap-step-panel]").forEach((panel) => {
    const panelStep = Number(panel.getAttribute("data-ldap-step-panel"));
    panel.classList.toggle("is-hidden", panelStep !== normalizedStep);
  });

  const stepper = form.closest(".ldap-wizard")?.querySelector(".ldap-wizard__stepper");
  if (stepper) {
    stepper.outerHTML = renderStepper(normalizedStep);
  }

  const info = form.querySelector("[data-ldap-step-info='1']");
  if (info) {
    info.classList.toggle("is-hidden", normalizedStep !== 1);
  }

  const prevButton = form.querySelector("[data-action='ldap-wizard-prev']");
  const nextButton = form.querySelector("[data-action='ldap-wizard-next']");
  const saveButton = form.querySelector("[data-action='ldap-wizard-save']");
  const saveTestButton = form.querySelector("[data-action='ldap-wizard-save-test']");

  prevButton?.classList.toggle("is-hidden", normalizedStep <= 1);
  nextButton?.classList.toggle("is-hidden", normalizedStep >= WIZARD_STEPS.length);
  saveButton?.classList.toggle("is-hidden", normalizedStep < WIZARD_STEPS.length);
  saveTestButton?.classList.toggle("is-hidden", normalizedStep < WIZARD_STEPS.length);

  if (normalizedStep === WIZARD_STEPS.length) {
    const summaryHost = form.querySelector("[data-ldap-summary]");
    if (summaryHost) {
      summaryHost.innerHTML = renderSummaryHtml(readFormValues(form));
    }
  }

  return normalizedStep;
}

function validateStep(form, step) {
  const values = readFormValues(form);

  if (step === 1 && values.isEnabled) {
    if (!values.server) {
      return "Informe o servidor LDAP para continuar.";
    }
    if (!values.baseDn) {
      return "Informe a Base DN para continuar.";
    }
  }

  if (step === 2 && values.isEnabled && !values.searchFilter) {
    return "Informe o filtro LDAP de busca para continuar.";
  }

  if (step === 3 && values.isEnabled && !values.displayNameAttribute) {
    return "Informe o atributo de nome exibido para continuar.";
  }

  return "";
}

export function initLdapWizard(root = document) {
  const form = root.querySelector("#ldap-settings-form");
  if (!form || form.dataset.bound === "true") {
    return;
  }

  form.dataset.bound = "true";
  let currentStep = Number(form.dataset.ldapStep || 1);
  updateWizardStep(form, currentStep);

  form.addEventListener("click", (event) => {
    const togglePassword = event.target.closest("[data-action='ldap-toggle-password']");
    if (togglePassword) {
      const input = form.querySelector("[name='serviceAccountPassword']");
      if (!input) {
        return;
      }

      const isPassword = input.type === "password";
      input.type = isPassword ? "text" : "password";
      togglePassword.innerHTML = isPassword
        ? '<i class="fa-regular fa-eye" aria-hidden="true"></i>'
        : '<i class="fa-regular fa-eye-slash" aria-hidden="true"></i>';
      togglePassword.setAttribute("aria-label", isPassword ? "Ocultar senha" : "Mostrar senha");
      return;
    }

    const cancelButton = event.target.closest("[data-action='ldap-wizard-cancel']");
    if (cancelButton) {
      window.location.hash = "#configuracoes";
      return;
    }

    const prevButton = event.target.closest("[data-action='ldap-wizard-prev']");
    if (prevButton) {
      currentStep = updateWizardStep(form, currentStep - 1);
      return;
    }

    const nextButton = event.target.closest("[data-action='ldap-wizard-next']");
    if (nextButton) {
      const validationMessage = validateStep(form, currentStep);
      if (validationMessage) {
        event.preventDefault();
        root.dispatchEvent(new CustomEvent("ldap-wizard:validation", { detail: { message: validationMessage }, bubbles: true }));
        return;
      }

      currentStep = updateWizardStep(form, currentStep + 1);
    }
  });
}

export function collectLdapWizardPayload(form) {
  return readFormValues(form);
}
