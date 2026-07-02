import { escapeHtml } from "../components/html.js";
import {
  TASK_CATEGORIES,
  formatDueDateInput,
  getFieldsForType,
  getTaskType,
  getTaskTypesByCategory
} from "./taskTypeCatalog.js";

const MODAL_ID = "journey-task-modal";
const FORM_ID = "journey-task-form";
const STEP_PANELS = Object.freeze([
  { id: 1, label: "Tipo" },
  { id: 2, label: "Detalhes" }
]);

let selectedTypeKey = "";
let currentStep = 1;
let modalMode = "create";
let editingTaskId = "";
let initialFormValues = null;
let hooks = {};

function renderStepper(activeStep) {
  const selectedType = getTaskType(selectedTypeKey);
  const stepOneLabel = activeStep === 2 && selectedType
    ? `Tipo (${selectedType.label})`
    : "Tipo";

  return `
    <nav class="journey-request-modal__stepper ldap-wizard__stepper" aria-label="Etapas da tarefa">
      ${STEP_PANELS.map((step, index) => {
        const state = step.id < activeStep ? "is-complete" : step.id === activeStep ? "is-active" : "";
        const label = step.id === 1 ? stepOneLabel : step.label;
        const connector = index < STEP_PANELS.length - 1
          ? `<span class="ldap-wizard__step-connector ${step.id < activeStep ? "is-complete" : ""}" aria-hidden="true"></span>`
          : "";

        return `
          <div class="ldap-wizard__step ${state}">
            <span class="ldap-wizard__step-badge">${step.id}</span>
            <span class="ldap-wizard__step-label">${escapeHtml(label)}</span>
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
      data-action="select-task-type"
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
  if (modalMode === "edit") {
    return "";
  }

  return `
    <section class="journey-request-modal__panel ${activeStep !== 1 ? "is-hidden" : ""}" data-task-step-panel="1">
      <div class="journey-request-modal__categories">
        ${TASK_CATEGORIES.map((category) => {
          const types = getTaskTypesByCategory(category.key);
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

function renderFormField(field, values = {}) {
  const requiredMark = field.required ? " *" : "";
  const fullClass = field.fullWidth ? " communication-form-field--full" : "";
  const name = escapeHtml(field.name);
  const label = escapeHtml(field.label);
  const value = values[field.name] ?? "";

  if (field.type === "select") {
    return `
      <label class="communication-form-field${fullClass}">
        <span>${label}${requiredMark}</span>
        <select name="${name}" ${field.required ? "required" : ""}>
          <option value="">Selecione...</option>
          ${(field.options || []).map((option) => `
            <option value="${escapeHtml(option)}" ${value === option ? "selected" : ""}>${escapeHtml(option)}</option>
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
        >${escapeHtml(value)}</textarea>
      </label>
    `;
  }

  return `
    <label class="communication-form-field${fullClass}">
      <span>${label}${requiredMark}</span>
      <input
        type="${escapeHtml(field.type)}"
        name="${name}"
        value="${escapeHtml(value)}"
        placeholder="${escapeHtml(field.placeholder || "")}"
        ${field.step ? `step="${escapeHtml(field.step)}"` : ""}
        ${field.min !== undefined ? `min="${escapeHtml(field.min)}"` : ""}
        ${field.required ? "required" : ""}
      />
    </label>
  `;
}

function buildInitialFieldValues(initialValues = {}) {
  const values = {
    title: initialValues.title || "",
    priority: initialValues.priority || "",
    dueDate: formatDueDateInput(initialValues.dueDate),
    description: initialValues.description || ""
  };

  const fields = initialValues.fields && typeof initialValues.fields === "object"
    ? initialValues.fields
    : {};

  Object.entries(fields).forEach(([key, value]) => {
    values[key] = value;
  });

  return values;
}

function renderDynamicFormStep(typeKey, activeStep) {
  const fields = getFieldsForType(typeKey);
  const values = buildInitialFieldValues(initialFormValues || {});

  return `
    <section class="journey-request-modal__panel ${activeStep !== 2 ? "is-hidden" : ""}" data-task-step-panel="2">
      <div class="communication-form-grid admin-user-form-grid">
        ${fields.map((field) => renderFormField(field, values)).join("")}
      </div>
    </section>
  `;
}

function renderModalFooter(activeStep) {
  const submitLabel = modalMode === "edit" ? "Salvar alteracoes" : "Registrar tarefa";

  if (activeStep === 1) {
    return `
      <footer class="journey-request-modal__footer journey-request-modal__footer--step-one">
        <button type="button" class="comm-secondary-button" data-action="close-task-modal">Cancelar</button>
      </footer>
    `;
  }

  const backButton = modalMode === "edit"
    ? ""
    : `<button type="button" class="comm-secondary-button" data-action="task-modal-back">Voltar</button>`;

  return `
    <footer class="journey-request-modal__footer">
      <span class="journey-request-modal__required-note">Campos com * sao obrigatorios</span>
      <div class="journey-request-modal__footer-actions">
        ${backButton}
        <button type="button" class="comm-secondary-button" data-action="close-task-modal">Cancelar</button>
        <button type="submit" class="feed-composer-submit" data-action="submit-task-modal">${escapeHtml(submitLabel)}</button>
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

function getModalTitle() {
  return modalMode === "edit" ? "Editar tarefa" : "Nova tarefa / atividade";
}

function getModalSubtitle() {
  return modalMode === "edit"
    ? "Atualize os detalhes da tarefa registrada"
    : "Registre uma tarefa ou atividade no ServiceNow";
}

export function renderTaskModalShell() {
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
        aria-labelledby="journey-task-modal-title"
      >
        <div class="card-header journey-request-modal__header">
          <div class="journey-request-modal__title-block">
            <strong id="journey-task-modal-title">${escapeHtml(getModalTitle())}</strong>
            <span>${escapeHtml(getModalSubtitle())}</span>
          </div>
          <button
            type="button"
            class="comm-tertiary-button journey-request-modal__close"
            data-action="close-task-modal"
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

function updateModalHeader(root = document) {
  const modal = getModal(root);
  if (!modal) {
    return;
  }

  const title = modal.querySelector("#journey-task-modal-title");
  const subtitle = modal.querySelector(".journey-request-modal__title-block span");
  if (title) {
    title.textContent = getModalTitle();
  }
  if (subtitle) {
    subtitle.textContent = getModalSubtitle();
  }
}

function updateStepPanels(form, activeStep) {
  form.querySelectorAll("[data-task-step-panel]").forEach((panel) => {
    const panelStep = Number(panel.dataset.taskStepPanel);
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
  getForm(root)?.querySelector("[name='title']")?.focus();
}

function refreshModalContent(root = document) {
  const form = getForm(root);
  if (!form) {
    return;
  }

  form.innerHTML = renderModalBody(currentStep);
  updateStepPanels(form, currentStep);
  updateModalHeader(root);
}

function resetModalState() {
  selectedTypeKey = "";
  currentStep = 1;
  modalMode = "create";
  editingTaskId = "";
  initialFormValues = null;
}

export function closeTaskModal(root = document) {
  const modal = getModal(root);
  if (!modal) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  document.body.classList.remove("modal-open");
  resetModalState();
}

export function openTaskModal(root = document, options = {}) {
  const modal = getModal(root);
  if (!modal) {
    return;
  }

  resetModalState();
  modalMode = options.mode === "edit" ? "edit" : "create";
  editingTaskId = options.initialValues?.id || "";
  selectedTypeKey = options.initialValues?.typeKey || "";
  initialFormValues = options.initialValues || null;
  currentStep = modalMode === "edit" && selectedTypeKey ? 2 : 1;

  refreshModalContent(root);
  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  document.body.classList.add("modal-open");

  if (currentStep === 2) {
    getForm(root)?.querySelector("[name='title']")?.focus();
  } else {
    modal.querySelector("[data-action='select-task-type']")?.focus();
  }
}

function readFormValues(form, typeKey) {
  const type = getTaskType(typeKey);
  const fields = getFieldsForType(typeKey);
  const values = {
    id: editingTaskId,
    typeKey,
    typeLabel: type?.listTypeLabel || type?.label || typeKey,
    title: "",
    description: "",
    priority: "",
    dueDate: "",
    fields: {}
  };

  fields.forEach((field) => {
    const input = form.querySelector(`[name="${field.name}"]`);
    if (!input) {
      return;
    }

    const value = String(input.value || "").trim();

    if (field.name === "title") {
      values.title = value;
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

    if (field.name === "dueDate") {
      values.dueDate = value;
      return;
    }

    values.fields[field.name] = value;
  });

  return values;
}

function validateTaskForm(values, typeKey) {
  const fields = getFieldsForType(typeKey);

  for (const field of fields) {
    if (!field.required) {
      continue;
    }

    if (field.name === "title" && !values.title) {
      return "Informe o titulo da tarefa.";
    }

    if (field.name === "description" && !values.description) {
      return "Informe a descricao da tarefa.";
    }

    if (field.name === "priority" && !values.priority) {
      return "Selecione a prioridade.";
    }

    if (field.name === "dueDate" && !values.dueDate) {
      return "Informe o prazo da tarefa.";
    }

    if (!["title", "description", "priority", "dueDate"].includes(field.name) && !values.fields[field.name]) {
      return `Preencha o campo ${field.label}.`;
    }
  }

  if (values.dueDate) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const dueDate = new Date(`${values.dueDate}T00:00:00`);
    if (dueDate < today) {
      return "O prazo nao pode ser anterior a hoje.";
    }
  }

  const meetingDate = values.fields.meetingDate;
  if (meetingDate && values.dueDate && meetingDate > values.dueDate) {
    return "A data da reuniao deve ser igual ou anterior ao prazo da tarefa.";
  }

  return "";
}

function handleDocumentKeydown(event) {
  const modal = getModal(document);
  if (event.key === "Escape" && modal && !modal.hidden) {
    event.preventDefault();
    closeTaskModal(document);
  }
}

export function bindTaskModal(root = document, nextHooks = {}) {
  hooks = nextHooks;
  const modal = getModal(root);
  const form = getForm(root);

  if (!modal || !form || modal.dataset.taskBound === "true") {
    return;
  }

  modal.dataset.taskBound = "true";

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closeTaskModal(root);
    }
  });

  form.addEventListener("click", (event) => {
    const target = event.target.closest("[data-action]");
    if (!target) {
      return;
    }

    const action = target.getAttribute("data-action");

    if (action === "select-task-type") {
      event.preventDefault();
      selectedTypeKey = target.dataset.typeKey || "";
      advanceToFormStep(root);
      return;
    }

    if (action === "task-modal-back") {
      event.preventDefault();
      currentStep = 1;
      refreshModalContent(root);
      return;
    }

    if (action === "close-task-modal") {
      event.preventDefault();
      closeTaskModal(root);
    }
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (currentStep !== 2 || !selectedTypeKey) {
      return;
    }

    const values = readFormValues(form, selectedTypeKey);
    const validationMessage = validateTaskForm(values, selectedTypeKey);
    if (validationMessage) {
      hooks.onValidation?.(validationMessage);
      return;
    }

    const submitButton = form.querySelector("[data-action='submit-task-modal']");
    const originalLabel = submitButton?.textContent || "Registrar tarefa";
    if (submitButton) {
      submitButton.disabled = true;
      submitButton.textContent = modalMode === "edit" ? "Salvando..." : "Registrando...";
    }

    try {
      if (modalMode === "edit") {
        await hooks.onSubmitEdit?.(values);
      } else {
        await hooks.onSubmitCreate?.(values);
      }
      closeTaskModal(root);
    } catch (error) {
      console.error("Falha ao salvar tarefa.", error);
      hooks.onValidation?.("Nao foi possivel salvar a tarefa. Tente novamente.");
    } finally {
      if (submitButton) {
        submitButton.disabled = false;
        submitButton.textContent = originalLabel;
      }
    }
  });

  if (!document.documentElement.dataset.journeyTaskModalKeydownBound) {
    document.documentElement.dataset.journeyTaskModalKeydownBound = "true";
    document.addEventListener("keydown", handleDocumentKeydown);
  }

  if (!document.documentElement.dataset.journeyTaskModalClickBound) {
    document.documentElement.dataset.journeyTaskModalClickBound = "true";
    document.addEventListener("click", (event) => {
      handleOpenTaskModalClick(event);
    });
  }
}

export function handleOpenTaskModalClick(event) {
  const trigger = event.target.closest("[data-action='open-task-modal']");
  if (!trigger) {
    return false;
  }

  event.preventDefault();
  openTaskModal(document, { mode: "create" });
  return true;
}

let rowActionHooks = {};

export function bindTaskRowActions(root = document, nextHooks = {}) {
  rowActionHooks = nextHooks;

  if (document.documentElement.dataset.journeyTaskRowActionsBound === "true") {
    return;
  }

  document.documentElement.dataset.journeyTaskRowActionsBound = "true";

  root.addEventListener("click", async (event) => {
    const target = event.target.closest("[data-action]");
    if (!target) {
      return;
    }

    const action = target.getAttribute("data-action");

    if (action === "edit-task") {
      event.preventDefault();
      const taskId = target.dataset.taskId || "";
      const task = rowActionHooks.getTaskById?.(taskId);
      if (!task) {
        rowActionHooks.onValidation?.("Tarefa nao encontrada.");
        return;
      }

      openTaskModal(root, {
        mode: "edit",
        initialValues: {
          id: task.id,
          typeKey: task.typeKey,
          title: task.title,
          description: task.description || "",
          priority: task.priority,
          dueDate: task.dueDate,
          fields: task.fields || {}
        }
      });
      return;
    }

    if (action === "complete-task") {
      event.preventDefault();
      const taskId = target.dataset.taskId || "";
      if (!window.confirm("Deseja marcar esta tarefa como concluida?")) {
        return;
      }

      try {
        await rowActionHooks.onComplete?.(taskId);
      } catch (error) {
        console.error("Falha ao concluir tarefa.", error);
        rowActionHooks.onValidation?.("Nao foi possivel concluir a tarefa.");
      }
      return;
    }

    if (action === "cancel-task") {
      event.preventDefault();
      const taskId = target.dataset.taskId || "";
      if (!window.confirm("Deseja cancelar esta tarefa?")) {
        return;
      }

      try {
        await rowActionHooks.onCancel?.(taskId);
      } catch (error) {
        console.error("Falha ao cancelar tarefa.", error);
        rowActionHooks.onValidation?.("Nao foi possivel cancelar a tarefa.");
      }
    }
  });
}
