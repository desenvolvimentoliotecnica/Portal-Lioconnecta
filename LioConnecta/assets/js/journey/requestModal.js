import { escapeHtml } from "../components/html.js";
import {
  REQUEST_CATEGORIES,
  getFieldsForType,
  getRequestType,
  getRequestTypesByCategory
} from "./requestTypeCatalog.js";

const MODAL_ID = "journey-request-modal";
const FORM_ID = "journey-request-form";
const STEP_PANELS = Object.freeze([
  { id: 1, label: "Tipo" },
  { id: 2, label: "Detalhes" }
]);

let selectedTypeKey = "";
let currentStep = 1;
let hooks = {};

function renderStepper(activeStep) {
  return `
    <nav class="journey-request-modal__stepper ldap-wizard__stepper" aria-label="Etapas da solicitacao">
      ${STEP_PANELS.map((step, index) => {
        const state = step.id < activeStep ? "is-complete" : step.id === activeStep ? "is-active" : "";
        const connector = index < STEP_PANELS.length - 1
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

function renderTypeCard(type, isSelected) {
  return `
    <button
      type="button"
      class="journey-request-type-card ${isSelected ? "is-selected" : ""}"
      data-action="select-request-type"
      data-type-key="${escapeHtml(type.key)}"
      aria-pressed="${isSelected ? "true" : "false"}"
    >
      <span class="journey-request-type-card__icon" aria-hidden="true">
        <i class="${escapeHtml(type.icon)}"></i>
      </span>
      <span class="journey-request-type-card__copy">
        <strong>${escapeHtml(type.label)}</strong>
        <small>${escapeHtml(type.description)}</small>
      </span>
    </button>
  `;
}

function renderTypeSelectionStep(activeStep) {
  return `
    <section class="journey-request-modal__panel ${activeStep !== 1 ? "is-hidden" : ""}" data-request-step-panel="1">
      <div class="journey-request-modal__categories">
        ${REQUEST_CATEGORIES.map((category) => {
          const types = getRequestTypesByCategory(category.key);
          if (!types.length) {
            return "";
          }

          return `
            <div class="journey-request-modal__category">
              <h3>${escapeHtml(category.label)}</h3>
              <div class="journey-request-modal__type-grid">
                ${types.map((type) => renderTypeCard(type, type.key === selectedTypeKey)).join("")}
              </div>
            </div>
          `;
        }).join("")}
      </div>
    </section>
  `;
}

function renderFormField(field) {
  const requiredMark = field.required ? " *" : "";
  const fullClass = field.fullWidth ? " communication-form-field--full" : "";
  const name = escapeHtml(field.name);
  const label = escapeHtml(field.label);

  if (field.type === "select") {
    return `
      <label class="communication-form-field${fullClass}">
        <span>${label}${requiredMark}</span>
        <select name="${name}" ${field.required ? "required" : ""}>
          <option value="">Selecione...</option>
          ${(field.options || []).map((option) => `
            <option value="${escapeHtml(option)}">${escapeHtml(option)}</option>
          `).join("")}
        </select>
      </label>
    `;
  }

  if (field.type === "textarea") {
    return `
      <label class="communication-form-field${fullClass}">
        <span>${label}${requiredMark}</span>
        <textarea
          name="${name}"
          rows="${field.rows || 4}"
          placeholder="${escapeHtml(field.placeholder || "")}"
          ${field.required ? "required" : ""}
        ></textarea>
      </label>
    `;
  }

  if (field.type === "file") {
    return `
      <div class="communication-form-field communication-form-field--full">
        <span>${label}${requiredMark}</span>
        <div class="journey-request-modal__upload">
          <i class="fa-solid fa-cloud-arrow-up" aria-hidden="true"></i>
          <input type="file" name="${name}" accept="${escapeHtml(field.accept || "")}" />
          <span>Arraste arquivos ou clique para selecionar</span>
          ${field.helper ? `<small class="communication-field-help">${escapeHtml(field.helper)}</small>` : ""}
        </div>
      </div>
    `;
  }

  return `
    <label class="communication-form-field${fullClass}">
      <span>${label}${requiredMark}</span>
      <input
        type="${escapeHtml(field.type)}"
        name="${name}"
        placeholder="${escapeHtml(field.placeholder || "")}"
        ${field.step ? `step="${escapeHtml(field.step)}"` : ""}
        ${field.min !== undefined ? `min="${escapeHtml(field.min)}"` : ""}
        ${field.required ? "required" : ""}
      />
    </label>
  `;
}

function renderDynamicFormStep(typeKey, activeStep) {
  const type = getRequestType(typeKey);
  const fields = getFieldsForType(typeKey);

  return `
    <section class="journey-request-modal__panel ${activeStep !== 2 ? "is-hidden" : ""}" data-request-step-panel="2">
      <div class="journey-request-modal__selected-type">
        <span class="journey-request-modal__selected-type-icon" aria-hidden="true">
          <i class="${escapeHtml(type?.icon || "fa-solid fa-file-lines")}"></i>
        </span>
        <div>
          <strong>${escapeHtml(type?.label || "Solicitacao")}</strong>
          <button type="button" class="comm-inline-action" data-action="request-modal-change-type">
            Alterar tipo
          </button>
        </div>
      </div>
      <div class="communication-form-grid admin-user-form-grid">
        ${fields.map((field) => renderFormField(field)).join("")}
      </div>
      <div class="journey-request-modal__notice" role="note">
        <i class="fa-solid fa-circle-info" aria-hidden="true"></i>
        <span>Simulacao: em producao, a solicitacao sera registrada no ServiceNow.</span>
      </div>
    </section>
  `;
}

function renderModalFooter(activeStep) {
  if (activeStep === 1) {
    return `
      <footer class="journey-request-modal__footer journey-request-modal__footer--step-one">
        <button type="button" class="comm-secondary-button" data-action="close-request-modal">Cancelar</button>
      </footer>
    `;
  }

  return `
    <footer class="journey-request-modal__footer">
      <span class="journey-request-modal__required-note">Campos com * sao obrigatorios</span>
      <div class="journey-request-modal__footer-actions">
        <button type="button" class="comm-secondary-button" data-action="request-modal-back">Voltar</button>
        <button type="button" class="comm-secondary-button" data-action="close-request-modal">Cancelar</button>
        <button type="submit" class="feed-composer-submit" data-action="submit-request-modal">Enviar solicitacao</button>
      </div>
    </footer>
  `;
}

function renderModalBody(activeStep) {
  return `
    ${renderStepper(activeStep)}
    ${renderTypeSelectionStep(activeStep)}
    ${selectedTypeKey ? renderDynamicFormStep(selectedTypeKey, activeStep) : ""}
    ${renderModalFooter(activeStep)}
  `;
}

export function renderRequestModalShell() {
  return `
    <div
      class="journey-request-modal"
      id="${MODAL_ID}"
      hidden
      aria-hidden="true"
    >
      <div
        class="journey-request-modal__dialog card"
        role="dialog"
        aria-modal="true"
        aria-labelledby="journey-request-modal-title"
      >
        <div class="card-header journey-request-modal__header">
          <div class="journey-request-modal__title-block">
            <strong id="journey-request-modal-title">Nova solicitacao</strong>
            <span>Registre um chamado no ServiceNow</span>
          </div>
          <button
            type="button"
            class="comm-tertiary-button journey-request-modal__close"
            data-action="close-request-modal"
            aria-label="Fechar modal"
          >
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
        </div>
        <form class="journey-request-modal__body" id="${FORM_ID}" novalidate>
          ${renderModalBody(currentStep)}
        </form>
      </div>
    </div>
  `;
}

function getModal(root = document) {
  return root.getElementById(MODAL_ID);
}

function getForm(root = document) {
  return root.getElementById(FORM_ID);
}

function updateStepPanels(form, activeStep) {
  form.querySelectorAll("[data-request-step-panel]").forEach((panel) => {
    const panelStep = Number(panel.dataset.requestStepPanel);
    panel.classList.toggle("is-hidden", panelStep !== activeStep);
  });

  form.querySelectorAll(".ldap-wizard__step").forEach((stepEl, index) => {
    const stepNumber = index + 1;
    stepEl.classList.toggle("is-active", stepNumber === activeStep);
    stepEl.classList.toggle("is-complete", stepNumber < activeStep);
  });

  form.querySelectorAll(".ldap-wizard__step-connector").forEach((connector, index) => {
    connector.classList.toggle("is-complete", index + 1 < activeStep);
  });
}

function advanceToFormStep(root = document) {
  if (!selectedTypeKey) {
    return;
  }

  currentStep = 2;
  refreshModalContent(root);
  getForm(root)?.querySelector("[name='subject']")?.focus();
}

function refreshModalContent(root = document) {
  const form = getForm(root);
  if (!form) {
    return;
  }

  form.innerHTML = renderModalBody(currentStep);
  updateStepPanels(form, currentStep);
}

function resetModalState() {
  selectedTypeKey = "";
  currentStep = 1;
}

export function closeRequestModal(root = document) {
  const modal = getModal(root);
  if (!modal) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  document.body.classList.remove("modal-open");
  resetModalState();
}

export function openRequestModal(root = document) {
  const modal = getModal(root);
  if (!modal) {
    return;
  }

  resetModalState();
  refreshModalContent(root);
  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  document.body.classList.add("modal-open");
  modal.querySelector("[data-action='select-request-type']")?.focus();
}

function readFormValues(form, typeKey) {
  const type = getRequestType(typeKey);
  const fields = getFieldsForType(typeKey);
  const values = {
    typeKey,
    typeLabel: type?.listTypeLabel || type?.label || typeKey,
    subject: "",
    description: "",
    priority: "",
    fields: {}
  };

  fields.forEach((field) => {
    const input = form.querySelector(`[name="${field.name}"]`);
    if (!input) {
      return;
    }

    let value = "";
    if (field.type === "file") {
      value = input.files?.[0]?.name || "";
    } else {
      value = String(input.value || "").trim();
    }

    if (field.name === "subject") {
      values.subject = value;
      return;
    }

    if (field.name === "description") {
      values.description = value;
      return;
    }

    if (field.name === "priority") {
      values.priority = value;
      return;
    }

    values.fields[field.name] = value;
  });

  return values;
}

function validateRequestForm(values, typeKey) {
  const fields = getFieldsForType(typeKey);

  for (const field of fields) {
    if (!field.required) {
      continue;
    }

    if (field.name === "subject" && !values.subject) {
      return "Informe o assunto da solicitacao.";
    }

    if (field.name === "description" && !values.description) {
      return "Informe a descricao da solicitacao.";
    }

    if (field.name === "priority" && !values.priority) {
      return "Selecione a prioridade.";
    }

    if (!["subject", "description", "priority"].includes(field.name) && !values.fields[field.name]) {
      return `Preencha o campo ${field.label}.`;
    }
  }

  const startDate = values.fields.startDate;
  const endDate = values.fields.endDate;
  if (startDate && endDate && startDate > endDate) {
    return "A data fim deve ser igual ou posterior a data inicio.";
  }

  return "";
}

function handleDocumentKeydown(event) {
  const modal = getModal(document);
  if (event.key === "Escape" && modal && !modal.hidden) {
    event.preventDefault();
    closeRequestModal(document);
  }
}

export function bindRequestModal(root = document, nextHooks = {}) {
  hooks = nextHooks;
  const modal = getModal(root);
  const form = getForm(root);

  if (!modal || !form || modal.dataset.bound === "true") {
    return;
  }

  modal.dataset.bound = "true";

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closeRequestModal(root);
    }
  });

  form.addEventListener("click", (event) => {
    const target = event.target.closest("[data-action]");
    if (!target) {
      return;
    }

    const action = target.getAttribute("data-action");

    if (action === "select-request-type") {
      event.preventDefault();
      selectedTypeKey = target.dataset.typeKey || "";
      advanceToFormStep(root);
      return;
    }

    if (action === "request-modal-next") {
      event.preventDefault();
      if (!selectedTypeKey) {
        hooks.onValidation?.("Selecione um tipo de solicitacao para continuar.");
        return;
      }
      advanceToFormStep(root);
      return;
    }

    if (action === "request-modal-back" || action === "request-modal-change-type") {
      event.preventDefault();
      currentStep = 1;
      refreshModalContent(root);
      return;
    }

    if (action === "close-request-modal") {
      event.preventDefault();
      closeRequestModal(root);
    }
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (currentStep !== 2 || !selectedTypeKey) {
      return;
    }

    const values = readFormValues(form, selectedTypeKey);
    const validationMessage = validateRequestForm(values, selectedTypeKey);
    if (validationMessage) {
      hooks.onValidation?.(validationMessage);
      return;
    }

    const submitButton = form.querySelector("[data-action='submit-request-modal']");
    const originalLabel = submitButton?.textContent || "Enviar solicitacao";
    if (submitButton) {
      submitButton.disabled = true;
      submitButton.textContent = "Enviando...";
    }

    try {
      await hooks.onSubmit?.(values);
      closeRequestModal(root);
    } catch (error) {
      console.error("Falha ao registrar solicitacao.", error);
      hooks.onValidation?.("Nao foi possivel registrar a solicitacao. Tente novamente.");
    } finally {
      if (submitButton) {
        submitButton.disabled = false;
        submitButton.textContent = originalLabel;
      }
    }
  });

  if (!document.documentElement.dataset.journeyRequestModalKeydownBound) {
    document.documentElement.dataset.journeyRequestModalKeydownBound = "true";
    document.addEventListener("keydown", handleDocumentKeydown);
  }

  if (!document.documentElement.dataset.journeyRequestModalClickBound) {
    document.documentElement.dataset.journeyRequestModalClickBound = "true";
    document.addEventListener("click", (event) => {
      handleOpenRequestModalClick(event);
    });
  }
}

export function handleOpenRequestModalClick(event) {
  const trigger = event.target.closest("[data-action='open-request-modal']");
  if (!trigger) {
    return false;
  }

  event.preventDefault();
  openRequestModal(document);
  return true;
}
