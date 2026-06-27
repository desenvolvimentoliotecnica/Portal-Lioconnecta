import { escapeHtml } from "../components/html.js";

const WIZARD_STEPS = Object.freeze([
  { id: 1, label: "Conteudo" },
  { id: 2, label: "Publicacao" },
  { id: 3, label: "Midia" },
  { id: 4, label: "Preferencias" },
  { id: 5, label: "Resumo" }
]);

const PRIORITY_OPTIONS = ["Alta prioridade", "Comunicado interno", "Programado", "Vigente"];
const AUDIENCE_OPTIONS = ["Toda a companhia", "Gestores e colaboradores", "Liderancas", "Publico interno"];
const CHANNEL_OPTIONS = ["Portal + email", "Portal", "Portal + Teams", "Portal + feed"];
const STATUS_OPTIONS = ["Publicado", "Rascunho", "Em revisao", "Arquivado"];

function renderWizardStepper(activeStep) {
  return `
    <nav class="poll-wizard__stepper ldap-wizard__stepper" aria-label="Etapas do comunicado">
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

function renderSelectOptions(options, selected) {
  return options.map((option) => `
    <option value="${escapeHtml(option)}" ${option === selected ? "selected" : ""}>${escapeHtml(option)}</option>
  `).join("");
}

function createDefaultFormCommunication() {
  const today = new Date().toISOString().slice(0, 10);

  return {
    id: "",
    title: "",
    summary: "",
    body: "",
    category: "RH",
    priority: "Comunicado interno",
    audience: "Toda a companhia",
    channel: "Portal",
    status: "Publicado",
    attachmentLabel: "Abrir anexo",
    owner: "Comunicacao Corporativa",
    imageUrl: "",
    isFeatured: false,
    publishedAtEditorValue: today
  };
}

function renderWizardPanels(formComm, categoryOptions) {
  const categories = categoryOptions.length
    ? categoryOptions
    : ["RH", "Corporativo", "Tecnologia", "Politicas", "Eventos"];

  const imagePreview = formComm.imageUrl
    ? `<div class="poll-asset-preview poll-asset-preview--image"><img src="${escapeHtml(formComm.imageUrl)}" alt="Imagem do comunicado" loading="lazy" /></div>`
    : `<div class="poll-asset-preview is-empty">Nenhuma imagem selecionada.</div>`;

  return `
    <section class="poll-wizard__panel" data-comm-step-panel="1">
      <div class="poll-wizard__panel-head">
        <h2>1. Conteudo do comunicado</h2>
        <p>Defina titulo, resumo e corpo que serao exibidos na central de comunicacao.</p>
      </div>
      <label class="communication-form-field communication-form-field--full">
        <span>Titulo</span>
        <input type="text" name="title" value="${escapeHtml(formComm.title || "")}" placeholder="Titulo oficial do comunicado" />
      </label>
      <label class="communication-form-field communication-form-field--full">
        <span>Resumo oficial</span>
        <textarea name="summary" rows="4" placeholder="Resumo exibido na listagem">${escapeHtml(formComm.summary || "")}</textarea>
      </label>
      <label class="communication-form-field communication-form-field--full">
        <span>Corpo do comunicado</span>
        <textarea name="body" rows="10" placeholder="Conteudo completo do comunicado">${escapeHtml(formComm.body || "")}</textarea>
      </label>
    </section>

    <section class="poll-wizard__panel is-hidden" data-comm-step-panel="2">
      <div class="poll-wizard__panel-head">
        <h2>2. Publicacao e classificacao</h2>
        <p>Configure categoria, prioridade, publico e status editorial.</p>
      </div>
      <div class="communication-form-grid">
        <label class="communication-form-field">
          <span>Categoria</span>
          <select name="category">
            ${categories.map((item) => `<option value="${escapeHtml(item)}" ${item === formComm.category ? "selected" : ""}>${escapeHtml(item)}</option>`).join("")}
          </select>
        </label>
        <label class="communication-form-field">
          <span>Prioridade</span>
          <select name="priority">${renderSelectOptions(PRIORITY_OPTIONS, formComm.priority)}</select>
        </label>
        <label class="communication-form-field">
          <span>Audiencia</span>
          <select name="audience">${renderSelectOptions(AUDIENCE_OPTIONS, formComm.audience)}</select>
        </label>
        <label class="communication-form-field">
          <span>Canal</span>
          <select name="channel">${renderSelectOptions(CHANNEL_OPTIONS, formComm.channel)}</select>
        </label>
        <label class="communication-form-field">
          <span>Data de publicacao</span>
          <input type="date" name="publishedAt" value="${escapeHtml(formComm.publishedAtEditorValue || "")}" />
        </label>
        <label class="communication-form-field">
          <span>Responsavel editorial</span>
          <input type="text" name="owner" value="${escapeHtml(formComm.owner || "")}" />
        </label>
        <label class="communication-form-field communication-form-field--full">
          <span>Status</span>
          <select name="status">${renderSelectOptions(STATUS_OPTIONS, formComm.status)}</select>
        </label>
      </div>
    </section>

    <section class="poll-wizard__panel is-hidden" data-comm-step-panel="3">
      <div class="poll-wizard__panel-head">
        <h2>3. Midia e anexos</h2>
        <p>Envie imagem de destaque e configure o rotulo do anexo.</p>
      </div>
      <input type="hidden" name="imageUrl" value="${escapeHtml(formComm.imageUrl || "")}" />
      <label class="communication-form-field communication-form-field--full">
        <span>Imagem do comunicado</span>
        <input type="file" name="imageFile" accept="image/*" />
        <small class="communication-field-help">Selecione uma imagem para destacar o comunicado na central e no carrossel.</small>
      </label>
      <div id="communication-image-preview">${imagePreview}</div>
      <label class="communication-form-field communication-form-field--full">
        <span>Texto do anexo</span>
        <input type="text" name="attachmentLabel" value="${escapeHtml(formComm.attachmentLabel || "")}" placeholder="Baixar diretrizes" />
      </label>
    </section>

    <section class="poll-wizard__panel is-hidden" data-comm-step-panel="4">
      <div class="poll-wizard__panel-head">
        <h2>4. Preferencias de exibicao</h2>
        <p>Defina como o comunicado sera destacado no portal.</p>
      </div>
      <div class="communication-form-toggles">
        <label class="communication-checkbox-wrap">
          <input type="checkbox" name="isFeatured" ${formComm.isFeatured ? "checked" : ""} />
          Destacar na central de comunicacao
        </label>
      </div>
      <section class="card communication-admin-card communication-admin-card--embedded">
        <div class="card-header">Preview resumido</div>
        <div class="communication-admin-preview" data-comm-preview>
          <div class="communication-admin-preview-media" data-comm-preview-image>
            ${formComm.imageUrl
              ? `<img src="${escapeHtml(formComm.imageUrl)}" alt="Preview do comunicado" loading="lazy" />`
              : `<span><i class="fa-regular fa-image"></i> Sem imagem selecionada</span>`}
          </div>
          <div class="comm-meta-row">
            <span class="comm-tag comm-tag--solid" data-comm-preview-category>${escapeHtml(formComm.category || "RH")}</span>
            <span class="comm-tag" data-comm-preview-priority>${escapeHtml(formComm.priority || "Comunicado interno")}</span>
          </div>
          <h3 data-comm-preview-title>${escapeHtml(formComm.title || "Titulo do comunicado")}</h3>
          <p data-comm-preview-summary>${escapeHtml(formComm.summary || "Resumo do comunicado")}</p>
        </div>
      </section>
    </section>

    <section class="poll-wizard__panel is-hidden" data-comm-step-panel="5">
      <div class="poll-wizard__panel-head">
        <h2>5. Resumo do comunicado</h2>
        <p>Revise os dados antes de salvar.</p>
      </div>
      <div class="poll-wizard__summary" data-comm-summary></div>
    </section>
  `;
}

export function renderCommunicationAdminWizardModal(communications, formComm = null, editing = false) {
  const communication = formComm || createDefaultFormCommunication();
  const categoryOptions = communications?.availableCategories || [];

  return `
    <div class="poll-admin-modal" id="communication-admin-modal" hidden aria-hidden="true">
      <div class="poll-admin-modal__dialog card" role="dialog" aria-modal="true" aria-labelledby="communication-admin-modal-title">
        <div class="poll-admin-modal__header card-header">
          <div>
            <strong id="communication-admin-modal-title">${editing ? "Editar comunicado" : "Novo comunicado"}</strong>
            <span>Assistente em etapas para criar ou atualizar comunicados oficiais.</span>
          </div>
          <button type="button" class="comm-tertiary-button poll-admin-modal__close" data-action="close-comm-wizard" aria-label="Fechar">
            <i class="fa-solid fa-xmark"></i>
          </button>
        </div>
        <div class="poll-admin-modal__body">
          ${renderWizardStepper(1)}
          <form
            id="communication-admin-form"
            class="poll-admin-form poll-wizard__form"
            data-mode="${editing ? "edit" : "create"}"
            data-communication-id="${escapeHtml(communication.id || "")}"
            data-comm-step="1"
            novalidate
          >
            ${renderWizardPanels(communication, categoryOptions)}
            <footer class="poll-wizard__footer ldap-wizard__footer">
              <button type="button" class="ldap-wizard__button ldap-wizard__button--ghost" data-action="close-comm-wizard">
                Cancelar
              </button>
              <div class="poll-wizard__footer-actions ldap-wizard__footer-actions">
                <button type="button" class="ldap-wizard__button ldap-wizard__button--ghost is-hidden" data-action="comm-wizard-prev">
                  Voltar
                </button>
                <button type="button" class="ldap-wizard__button ldap-wizard__button--primary" data-action="comm-wizard-next">
                  Proximo <i class="fa-solid fa-arrow-right" aria-hidden="true"></i>
                </button>
                <button type="submit" class="ldap-wizard__button ldap-wizard__button--primary is-hidden" data-action="comm-wizard-save">
                  ${editing ? "Salvar alteracoes" : "Publicar comunicado"}
                </button>
              </div>
            </footer>
          </form>
        </div>
      </div>
    </div>
  `;
}

export function readCommunicationWizardFormValues(form) {
  return {
    title: form.querySelector("[name='title']")?.value?.trim() || "",
    summary: form.querySelector("[name='summary']")?.value?.trim() || "",
    body: form.querySelector("[name='body']")?.value?.trim() || "",
    category: form.querySelector("[name='category']")?.value || "RH",
    priority: form.querySelector("[name='priority']")?.value || "Comunicado interno",
    audience: form.querySelector("[name='audience']")?.value || "Toda a companhia",
    channel: form.querySelector("[name='channel']")?.value || "Portal",
    status: form.querySelector("[name='status']")?.value || "Publicado",
    attachmentLabel: form.querySelector("[name='attachmentLabel']")?.value?.trim() || "Abrir anexo",
    owner: form.querySelector("[name='owner']")?.value?.trim() || "Comunicacao Corporativa",
    imageUrl: form.querySelector("[name='imageUrl']")?.value?.trim() || "",
    isFeatured: Boolean(form.querySelector("[name='isFeatured']")?.checked),
    publishedAt: form.querySelector("[name='publishedAt']")?.value || ""
  };
}

function renderSummaryHtml(values) {
  const rows = [
    ["Titulo", values.title || "—"],
    ["Categoria", values.category || "—"],
    ["Prioridade", values.priority || "—"],
    ["Audiencia", values.audience || "—"],
    ["Canal", values.channel || "—"],
    ["Status", values.status || "—"],
    ["Publicacao", values.publishedAt || "Nao definida"],
    ["Responsavel", values.owner || "—"],
    ["Destaque", values.isFeatured ? "Sim" : "Nao"],
    ["Imagem", values.imageUrl ? "Imagem anexada" : "Sem imagem"],
    ["Anexo", values.attachmentLabel || "—"]
  ];

  return `
    <dl class="ldap-wizard__summary-list poll-wizard__summary-list">
      ${rows.map(([label, value]) => `
        <div class="ldap-wizard__summary-row">
          <dt>${escapeHtml(label)}</dt>
          <dd>${escapeHtml(value)}</dd>
        </div>
      `).join("")}
    </dl>
  `;
}

function validateCommunicationWizardStep(form, step) {
  const values = readCommunicationWizardFormValues(form);

  if (step === 1) {
    if (!values.title) {
      return "Informe o titulo do comunicado para continuar.";
    }
    if (!values.summary) {
      return "Informe o resumo do comunicado para continuar.";
    }
    if (!values.body) {
      return "Informe o corpo do comunicado para continuar.";
    }
  }

  return "";
}

function updateCommunicationPreview(form) {
  const values = readCommunicationWizardFormValues(form);
  const preview = form.querySelector("[data-comm-preview]");
  if (!preview) {
    return;
  }

  preview.querySelector("[data-comm-preview-category]")?.replaceChildren(document.createTextNode(values.category));
  preview.querySelector("[data-comm-preview-priority]")?.replaceChildren(document.createTextNode(values.priority));
  preview.querySelector("[data-comm-preview-title]")?.replaceChildren(document.createTextNode(values.title || "Titulo do comunicado"));
  preview.querySelector("[data-comm-preview-summary]")?.replaceChildren(document.createTextNode(values.summary || "Resumo do comunicado"));

  const imageHost = preview.querySelector("[data-comm-preview-image]");
  if (imageHost) {
    imageHost.innerHTML = values.imageUrl
      ? `<img src="${values.imageUrl}" alt="Preview do comunicado" loading="lazy" />`
      : `<span><i class="fa-regular fa-image"></i> Sem imagem selecionada</span>`;
  }
}

function updateWizardStep(form, step) {
  const normalizedStep = Math.min(Math.max(step, 1), WIZARD_STEPS.length);
  form.dataset.commStep = String(normalizedStep);

  form.querySelectorAll("[data-comm-step-panel]").forEach((panel) => {
    const panelStep = Number(panel.getAttribute("data-comm-step-panel"));
    panel.classList.toggle("is-hidden", panelStep !== normalizedStep);
  });

  const modal = form.closest(".poll-admin-modal");
  const stepperHost = modal?.querySelector(".poll-wizard__stepper");
  if (stepperHost) {
    stepperHost.outerHTML = renderWizardStepper(normalizedStep);
  }

  const prevButton = form.querySelector("[data-action='comm-wizard-prev']");
  const nextButton = form.querySelector("[data-action='comm-wizard-next']");
  const saveButton = form.querySelector("[data-action='comm-wizard-save']");

  prevButton?.classList.toggle("is-hidden", normalizedStep <= 1);
  nextButton?.classList.toggle("is-hidden", normalizedStep >= WIZARD_STEPS.length);
  saveButton?.classList.toggle("is-hidden", normalizedStep < WIZARD_STEPS.length);

  if (normalizedStep === 4) {
    updateCommunicationPreview(form);
  }

  if (normalizedStep === WIZARD_STEPS.length) {
    const summaryHost = form.querySelector("[data-comm-summary]");
    if (summaryHost) {
      summaryHost.innerHTML = renderSummaryHtml(readCommunicationWizardFormValues(form));
    }
  }

  return normalizedStep;
}

export function closeCommunicationAdminWizard(root = document) {
  const modal = root.getElementById("communication-admin-modal");
  if (!modal) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  document.body.classList.remove("modal-open");
}

export function openCommunicationAdminWizard(root = document) {
  const modal = root.getElementById("communication-admin-modal");
  const form = root.getElementById("communication-admin-form");
  if (!modal || !form) {
    return;
  }

  updateWizardStep(form, 1);
  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  document.body.classList.add("modal-open");
  form.querySelector("[name='title']")?.focus();
}

export function initCommunicationAdminWizard(root = document, hooks = {}) {
  const modal = root.getElementById("communication-admin-modal");
  const form = root.getElementById("communication-admin-form");

  if (!modal || !form || form.dataset.wizardBound === "true") {
    return;
  }

  form.dataset.wizardBound = "true";
  let currentStep = 1;
  updateWizardStep(form, currentStep);

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closeCommunicationAdminWizard(root);
      hooks.onClose?.();
    }
  });

  form.addEventListener("input", () => {
    if (Number(form.dataset.commStep) === 4) {
      updateCommunicationPreview(form);
    }
  });

  form.addEventListener("change", async (event) => {
    const fileInput = event.target.closest("input[name='imageFile']");
    if (!fileInput) {
      return;
    }

    const file = fileInput.files?.[0];
    const valueInput = form.querySelector("[name='imageUrl']");
    const previewHost = form.querySelector("#communication-image-preview");

    if (!file || !valueInput) {
      valueInput.value = "";
      if (previewHost) {
        previewHost.innerHTML = `<div class="poll-asset-preview is-empty">Nenhuma imagem selecionada.</div>`;
      }
      updateCommunicationPreview(form);
      return;
    }

    const dataUrl = await hooks.readImageFile?.(file);
    valueInput.value = dataUrl || "";
    if (previewHost) {
      previewHost.innerHTML = dataUrl
        ? `<div class="poll-asset-preview poll-asset-preview--image"><img src="${dataUrl}" alt="Imagem do comunicado" loading="lazy" /></div>`
        : `<div class="poll-asset-preview is-empty">Nenhuma imagem selecionada.</div>`;
    }
    updateCommunicationPreview(form);
  });

  form.addEventListener("click", (event) => {
    const target = event.target.closest("[data-action]");
    if (!target) {
      return;
    }

    const action = target.getAttribute("data-action");

    if (action === "close-comm-wizard") {
      event.preventDefault();
      closeCommunicationAdminWizard(root);
      hooks.onClose?.();
      return;
    }

    if (action === "comm-wizard-prev") {
      event.preventDefault();
      currentStep = updateWizardStep(form, currentStep - 1);
      return;
    }

    if (action === "comm-wizard-next") {
      event.preventDefault();
      const validationMessage = validateCommunicationWizardStep(form, currentStep);
      if (validationMessage) {
        hooks.onValidation?.(validationMessage);
        return;
      }
      currentStep = updateWizardStep(form, currentStep + 1);
    }
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const validationMessage = validateCommunicationWizardStep(form, 1);
    if (validationMessage) {
      hooks.onValidation?.(validationMessage);
      currentStep = updateWizardStep(form, 1);
      return;
    }

    const mode = form.getAttribute("data-mode") || "create";
    const communicationId = form.getAttribute("data-communication-id") || "";
    const values = readCommunicationWizardFormValues(form);
    await hooks.onSubmit?.(values, mode, communicationId);
  });
}

export function mapCommunicationToForm(communication = {}) {
  return {
    id: communication.id || "",
    title: communication.title || "",
    summary: communication.summary || "",
    body: communication.bodyText || communication.body || "",
    category: communication.category || "RH",
    priority: communication.priority || "Comunicado interno",
    audience: communication.audience || "Toda a companhia",
    channel: communication.channel || "Portal",
    status: communication.status || "Publicado",
    attachmentLabel: communication.attachmentLabel || "Abrir anexo",
    owner: communication.owner || "Comunicacao Corporativa",
    imageUrl: communication.imageUrl || communication.image || "",
    isFeatured: Boolean(communication.isFeatured),
    publishedAtEditorValue: communication.publishedAtEditorValue
      || (communication.publishedAtRaw ? String(communication.publishedAtRaw).slice(0, 10) : "")
      || new Date().toISOString().slice(0, 10)
  };
}
