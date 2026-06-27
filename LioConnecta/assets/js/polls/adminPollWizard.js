import { escapeHtml } from "../components/html.js";

const WIZARD_STEPS = Object.freeze([
  { id: 1, label: "Conteudo" },
  { id: 2, label: "Midia" },
  { id: 3, label: "Publicacao" },
  { id: 4, label: "Alternativas" },
  { id: 5, label: "Resumo" }
]);

const STATUS_LABELS = Object.freeze({
  Draft: "Rascunho",
  Published: "Publicada",
  Closed: "Encerrada",
  Archived: "Arquivada"
});

const RESULTS_VISIBILITY_LABELS = Object.freeze({
  AfterVote: "Exibir apos voto",
  Always: "Sempre exibir",
  AfterClose: "Exibir apos encerramento"
});

function renderPollWizardStepper(activeStep) {
  return `
    <nav class="poll-wizard__stepper ldap-wizard__stepper" aria-label="Etapas da enquete">
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

function renderAdminOptionEditor(option = {}, index = 0) {
  return `
    <div class="poll-option-editor" data-option-row>
      <input type="hidden" name="optionId" value="${escapeHtml(option.id || "")}" />
      <label class="communication-form-field">
        <span>Opcao ${index + 1}</span>
        <input type="text" name="optionLabel" value="${escapeHtml(option.label || "")}" placeholder="Descreva a alternativa" />
      </label>
      <button type="button" class="comm-tertiary-button" data-action="remove-poll-option">
        <i class="fa-solid fa-trash"></i>
      </button>
    </div>
  `;
}

function createDefaultFormPoll() {
  return {
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
}

function renderWizardPanels(formPoll, data) {
  return `
    <section class="poll-wizard__panel" data-poll-step-panel="1">
      <div class="poll-wizard__panel-head">
        <h2>1. Conteudo da enquete</h2>
        <p>Defina titulo, resumo e descricao que os colaboradores verao no portal.</p>
      </div>
      <label class="communication-form-field communication-form-field--full">
        <span>Titulo</span>
        <input type="text" name="title" value="${escapeHtml(formPoll.title || "")}" placeholder="Qual pauta devemos priorizar?" />
      </label>
      <label class="communication-form-field communication-form-field--full">
        <span>Resumo</span>
        <textarea name="summary" rows="3" placeholder="Contextualize o objetivo da enquete">${escapeHtml(formPoll.summary || "")}</textarea>
      </label>
      <label class="communication-form-field communication-form-field--full">
        <span>Descricao completa</span>
        <textarea name="body" rows="6" placeholder="Explique o motivo, o criterio de escolha e o prazo da enquete">${escapeHtml(formPoll.body || "")}</textarea>
      </label>
    </section>

    <section class="poll-wizard__panel is-hidden" data-poll-step-panel="2">
      <div class="poll-wizard__panel-head">
        <h2>2. Midia e anexos</h2>
        <p>Envie imagem de destaque e material complementar opcional.</p>
      </div>
      ${renderPollAssetUploader({
        assetType: "image",
        label: "Imagem da enquete",
        inputName: "pollImageFile",
        valueName: "imageUrl",
        value: formPoll.imageUrl || "",
        accept: "image/*",
        buttonLabel: "Enviar imagem",
        helper: "Use PNG, JPG, WEBP ou GIF.",
        previewType: "image"
      })}
      <label class="communication-form-field communication-form-field--full">
        <span>Rotulo do anexo</span>
        <input type="text" name="attachmentLabel" value="${escapeHtml(formPoll.attachmentLabel || "")}" placeholder="Baixar material complementar" />
      </label>
      ${renderPollAssetUploader({
        assetType: "attachment",
        label: "Arquivo anexo",
        inputName: "pollAttachmentFile",
        valueName: "attachmentUrl",
        value: formPoll.attachmentUrl || "",
        accept: ".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.zip",
        buttonLabel: "Enviar anexo",
        helper: "Use PDF, Office, TXT ou ZIP.",
        previewType: "attachment"
      })}
    </section>

    <section class="poll-wizard__panel is-hidden" data-poll-step-panel="3">
      <div class="poll-wizard__panel-head">
        <h2>3. Publicacao e regras</h2>
        <p>Configure publico, status, datas e preferencias de exibicao.</p>
      </div>
      <div class="communication-form-grid">
        <label class="communication-form-field">
          <span>Publico</span>
          <input type="text" name="audience" value="${escapeHtml(formPoll.audience || "Toda a companhia")}" />
        </label>
        <label class="communication-form-field">
          <span>Status</span>
          <select name="status">
            ${data.statusOptions.map((option) => `<option value="${escapeHtml(option.key)}" ${option.key === formPoll.status ? "selected" : ""}>${escapeHtml(option.label)}</option>`).join("")}
          </select>
        </label>
        <label class="communication-form-field">
          <span>Publicacao</span>
          <input type="datetime-local" name="publishedAtUtc" value="${escapeHtml(formPoll.publishedAtEditorValue || "")}" />
        </label>
        <label class="communication-form-field">
          <span>Encerramento</span>
          <input type="datetime-local" name="closesAtUtc" value="${escapeHtml(formPoll.closesAtEditorValue || "")}" />
        </label>
        <label class="communication-form-field communication-form-field--full">
          <span>Visibilidade dos resultados</span>
          <select name="resultsVisibility">
            ${data.resultsVisibilityOptions.map((option) => `<option value="${escapeHtml(option.key)}" ${option.key === formPoll.resultsVisibility ? "selected" : ""}>${escapeHtml(option.label)}</option>`).join("")}
          </select>
        </label>
        <div class="communication-form-field communication-form-field--full">
          <span>Preferencias</span>
          <label class="communication-checkbox-wrap"><input type="checkbox" name="allowMultipleChoices" ${formPoll.allowMultipleChoices ? "checked" : ""} /> Permitir multipla escolha</label>
          <label class="communication-checkbox-wrap"><input type="checkbox" name="isFeatured" ${formPoll.isFeatured ? "checked" : ""} /> Destacar na home</label>
        </div>
      </div>
    </section>

    <section class="poll-wizard__panel is-hidden" data-poll-step-panel="4">
      <div class="poll-wizard__panel-head">
        <h2>4. Alternativas da enquete</h2>
        <p>Inclua pelo menos duas opcoes de resposta para os colaboradores.</p>
      </div>
      <div class="poll-admin-options-editor">
        <div class="poll-admin-options-editor__head">
          <strong>Alternativas</strong>
          <button type="button" class="comm-secondary-button" data-action="add-poll-option">Adicionar opcao</button>
        </div>
        <div id="poll-option-list">
          ${(formPoll.options || []).map(renderAdminOptionEditor).join("")}
        </div>
      </div>
    </section>

    <section class="poll-wizard__panel is-hidden" data-poll-step-panel="5">
      <div class="poll-wizard__panel-head">
        <h2>5. Resumo da enquete</h2>
        <p>Revise os dados antes de salvar no portal.</p>
      </div>
      <div class="poll-wizard__summary" data-poll-summary></div>
    </section>
  `;
}

export function renderPollAdminWizardModal(data, formPoll = null, editing = false) {
  const poll = formPoll || createDefaultFormPoll();

  return `
    <div class="poll-admin-modal" id="poll-admin-modal" hidden aria-hidden="true">
      <div class="poll-admin-modal__dialog card" role="dialog" aria-modal="true" aria-labelledby="poll-admin-modal-title">
        <div class="poll-admin-modal__header card-header">
          <div>
            <strong id="poll-admin-modal-title">${editing ? "Editar enquete" : "Nova enquete"}</strong>
            <span>Assistente em etapas para criar ou atualizar enquetes internas.</span>
          </div>
          <button type="button" class="comm-tertiary-button poll-admin-modal__close" data-action="close-poll-wizard" aria-label="Fechar">
            <i class="fa-solid fa-xmark"></i>
          </button>
        </div>
        <div class="poll-admin-modal__body">
          ${renderPollWizardStepper(1)}
          <form
            id="admin-poll-form"
            class="poll-admin-form poll-wizard__form"
            data-mode="${editing ? "edit" : "create"}"
            data-poll-id="${escapeHtml(poll.id || "")}"
            data-poll-step="1"
            novalidate
          >
            ${renderWizardPanels(poll, data)}
            <footer class="poll-wizard__footer ldap-wizard__footer">
              <button type="button" class="ldap-wizard__button ldap-wizard__button--ghost" data-action="close-poll-wizard">
                Cancelar
              </button>
              <div class="poll-wizard__footer-actions ldap-wizard__footer-actions">
                <button type="button" class="ldap-wizard__button ldap-wizard__button--ghost is-hidden" data-action="poll-wizard-prev">
                  Voltar
                </button>
                <button type="button" class="ldap-wizard__button ldap-wizard__button--primary" data-action="poll-wizard-next">
                  Proximo <i class="fa-solid fa-arrow-right" aria-hidden="true"></i>
                </button>
                <button type="submit" class="ldap-wizard__button ldap-wizard__button--primary is-hidden" data-action="poll-wizard-save">
                  ${editing ? "Salvar alteracoes" : "Criar enquete"}
                </button>
              </div>
            </footer>
          </form>
        </div>
      </div>
    </div>
  `;
}

export function readPollWizardFormValues(form) {
  const optionRows = Array.from(form.querySelectorAll("[data-option-row]"));
  const options = optionRows.map((row) => ({
    id: row.querySelector("input[name='optionId']")?.value?.trim() || null,
    label: row.querySelector("input[name='optionLabel']")?.value?.trim() || ""
  })).filter((item) => item.label);

  return {
    title: form.querySelector("[name='title']")?.value?.trim() || "",
    summary: form.querySelector("[name='summary']")?.value?.trim() || "",
    body: form.querySelector("[name='body']")?.value?.trim() || "",
    imageUrl: form.querySelector("[name='imageUrl']")?.value?.trim() || "",
    attachmentLabel: form.querySelector("[name='attachmentLabel']")?.value?.trim() || "",
    attachmentUrl: form.querySelector("[name='attachmentUrl']")?.value?.trim() || "",
    audience: form.querySelector("[name='audience']")?.value?.trim() || "Toda a companhia",
    status: form.querySelector("[name='status']")?.value || "Draft",
    resultsVisibility: form.querySelector("[name='resultsVisibility']")?.value || "AfterVote",
    allowMultipleChoices: Boolean(form.querySelector("[name='allowMultipleChoices']")?.checked),
    isFeatured: Boolean(form.querySelector("[name='isFeatured']")?.checked),
    publishedAtUtc: form.querySelector("[name='publishedAtUtc']")?.value || "",
    closesAtUtc: form.querySelector("[name='closesAtUtc']")?.value || "",
    options
  };
}

function renderSummaryHtml(values) {
  const rows = [
    ["Titulo", values.title || "—"],
    ["Resumo", values.summary || "—"],
    ["Publico", values.audience || "—"],
    ["Status", STATUS_LABELS[values.status] || values.status || "—"],
    ["Visibilidade dos resultados", RESULTS_VISIBILITY_LABELS[values.resultsVisibility] || values.resultsVisibility || "—"],
    ["Publicacao", values.publishedAtUtc || "Nao definida"],
    ["Encerramento", values.closesAtUtc || "Nao definido"],
    ["Multipla escolha", values.allowMultipleChoices ? "Sim" : "Nao"],
    ["Destaque na home", values.isFeatured ? "Sim" : "Nao"],
    ["Imagem", values.imageUrl || "Sem imagem"],
    ["Anexo", values.attachmentUrl || "Sem anexo"],
    ["Alternativas", values.options.map((item) => item.label).join(" • ") || "—"]
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

function validatePollWizardStep(form, step) {
  const values = readPollWizardFormValues(form);

  if (step === 1) {
    if (!values.title) {
      return "Informe o titulo da enquete para continuar.";
    }
    if (!values.summary) {
      return "Informe o resumo da enquete para continuar.";
    }
  }

  if (step === 4 && values.options.length < 2) {
    return "Inclua pelo menos duas alternativas na enquete.";
  }

  return "";
}

function updatePollWizardStep(form, step) {
  const normalizedStep = Math.min(Math.max(step, 1), WIZARD_STEPS.length);
  form.dataset.pollStep = String(normalizedStep);

  form.querySelectorAll("[data-poll-step-panel]").forEach((panel) => {
    const panelStep = Number(panel.getAttribute("data-poll-step-panel"));
    panel.classList.toggle("is-hidden", panelStep !== normalizedStep);
  });

  const modal = form.closest(".poll-admin-modal");
  const stepperHost = modal?.querySelector(".poll-wizard__stepper");
  if (stepperHost) {
    stepperHost.outerHTML = renderPollWizardStepper(normalizedStep);
  }

  const prevButton = form.querySelector("[data-action='poll-wizard-prev']");
  const nextButton = form.querySelector("[data-action='poll-wizard-next']");
  const saveButton = form.querySelector("[data-action='poll-wizard-save']");

  prevButton?.classList.toggle("is-hidden", normalizedStep <= 1);
  nextButton?.classList.toggle("is-hidden", normalizedStep >= WIZARD_STEPS.length);
  saveButton?.classList.toggle("is-hidden", normalizedStep < WIZARD_STEPS.length);

  if (normalizedStep === WIZARD_STEPS.length) {
    const summaryHost = form.querySelector("[data-poll-summary]");
    if (summaryHost) {
      summaryHost.innerHTML = renderSummaryHtml(readPollWizardFormValues(form));
    }
  }

  return normalizedStep;
}

export function closePollAdminWizard(root = document) {
  const modal = root.getElementById("poll-admin-modal");
  if (!modal) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  document.body.classList.remove("modal-open");
}

export function openPollAdminWizard(root = document) {
  const modal = root.getElementById("poll-admin-modal");
  const form = root.getElementById("admin-poll-form");
  if (!modal || !form) {
    return;
  }

  updatePollWizardStep(form, 1);
  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  document.body.classList.add("modal-open");
  form.querySelector("[name='title']")?.focus();
}

export function initPollAdminWizard(root = document, hooks = {}) {
  const modal = root.getElementById("poll-admin-modal");
  const form = root.getElementById("admin-poll-form");
  const optionList = root.getElementById("poll-option-list");

  if (!modal || !form || form.dataset.wizardBound === "true") {
    return;
  }

  form.dataset.wizardBound = "true";
  let currentStep = 1;
  updatePollWizardStep(form, currentStep);

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closePollAdminWizard(root);
      hooks.onClose?.();
    }
  });

  form.addEventListener("click", async (event) => {
    const target = event.target.closest("[data-action]");
    if (!target) {
      return;
    }

    const action = target.getAttribute("data-action");

    if (action === "close-poll-wizard") {
      event.preventDefault();
      closePollAdminWizard(root);
      hooks.onClose?.();
      return;
    }

    if (action === "poll-wizard-prev") {
      event.preventDefault();
      currentStep = updatePollWizardStep(form, currentStep - 1);
      return;
    }

    if (action === "poll-wizard-next") {
      event.preventDefault();
      const validationMessage = validatePollWizardStep(form, currentStep);
      if (validationMessage) {
        hooks.onValidation?.(validationMessage);
        return;
      }
      currentStep = updatePollWizardStep(form, currentStep + 1);
      return;
    }

    if (action === "add-poll-option") {
      event.preventDefault();
      const nextIndex = optionList?.querySelectorAll("[data-option-row]").length || 0;
      optionList?.insertAdjacentHTML("beforeend", renderAdminOptionEditor({}, nextIndex));
      return;
    }

    if (action === "remove-poll-option") {
      event.preventDefault();
      const rows = Array.from(optionList?.querySelectorAll("[data-option-row]") || []);
      if (rows.length <= 2) {
        hooks.onValidation?.("A enquete precisa manter pelo menos duas opcoes.");
        return;
      }

      target.closest("[data-option-row]")?.remove();
      Array.from(optionList?.querySelectorAll("[data-option-row]") || []).forEach((row, index) => {
        const label = row.querySelector("label span");
        if (label) {
          label.textContent = `Opcao ${index + 1}`;
        }
      });
      return;
    }

    if (action === "upload-poll-asset") {
      event.preventDefault();
      const assetType = target.getAttribute("data-asset-type") || "";
      await hooks.onUploadAsset?.(form, assetType, target);
    }
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const validationMessage = validatePollWizardStep(form, 4);
    if (validationMessage) {
      hooks.onValidation?.(validationMessage);
      currentStep = updatePollWizardStep(form, 4);
      return;
    }

    const mode = form.getAttribute("data-mode") || "create";
    const pollId = form.getAttribute("data-poll-id") || "";
    const values = readPollWizardFormValues(form);
    await hooks.onSubmit?.(values, mode, pollId);
  });
}

export { renderAdminOptionEditor as buildPollOptionRowHtml };
