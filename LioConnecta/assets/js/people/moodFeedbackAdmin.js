import { escapeHtml } from "../components/html.js";
import { renderEmptyState } from "../components/cards.js";
import { MOOD_FEEDBACK_OPTION_GROUPS } from "../services/moodSurveyFeedbackService.js";

function renderFeedbackSummaryChip(summary) {
  return `
    <span class="mood-feedback-summary-chip mood-feedback-summary-chip--${escapeHtml(summary.optionKey)}">
      ${escapeHtml(summary.optionEmoji)} ${escapeHtml(summary.optionLabel)}
      <strong>${escapeHtml(String(summary.activeMessages))}</strong> ativas
    </span>
  `;
}

function renderFeedbackRow(item, { editingId = "" } = {}) {
  if (editingId && editingId === item.id) {
    return `
      <article class="mood-feedback-item mood-feedback-item--editing" data-feedback-id="${escapeHtml(item.id)}">
        <form class="mood-feedback-form" data-action="save-mood-feedback" data-feedback-id="${escapeHtml(item.id)}">
          <div class="communication-form-grid">
            <label class="communication-form-field">
              <span>Humor</span>
              <select name="optionKey" required>
                ${MOOD_FEEDBACK_OPTION_GROUPS.map((option) => `
                  <option value="${escapeHtml(option.key)}" ${option.key === item.optionKey ? "selected" : ""}>
                    ${escapeHtml(option.emoji)} ${escapeHtml(option.label)}
                  </option>
                `).join("")}
              </select>
            </label>
            <label class="communication-form-field">
              <span>Ordem</span>
              <input type="number" name="sortOrder" min="1" value="${escapeHtml(String(item.sortOrder || 1))}" />
            </label>
            <label class="communication-form-field communication-form-field--full">
              <span>Mensagem</span>
              <textarea name="message" rows="3" required>${escapeHtml(item.message)}</textarea>
            </label>
            <div class="communication-form-field communication-form-field--full">
              <label class="communication-checkbox-wrap">
                <input type="checkbox" name="isActive" ${item.isActive ? "checked" : ""} />
                Mensagem ativa no sorteio aleatorio
              </label>
            </div>
          </div>
          <div class="communication-form-action-group">
            <button type="submit" class="feed-composer-submit">Salvar alteracoes</button>
            <button type="button" class="comm-tertiary-button" data-action="cancel-mood-feedback-edit">Cancelar</button>
          </div>
        </form>
      </article>
    `;
  }

  return `
    <article class="mood-feedback-item" data-feedback-id="${escapeHtml(item.id)}">
      <div class="mood-feedback-item__top">
        <span class="mood-feedback-item__badge mood-feedback-item__badge--${escapeHtml(item.optionKey)}">
          ${escapeHtml(item.optionEmoji)} ${escapeHtml(item.optionLabel)}
        </span>
        <span class="comm-tag">${item.isActive ? "Ativa" : "Inativa"}</span>
        <span class="comm-tag">Ordem ${escapeHtml(String(item.sortOrder))}</span>
      </div>
      <p class="mood-feedback-item__message">${escapeHtml(item.message)}</p>
      <div class="mood-feedback-item__actions">
        <button type="button" class="comm-inline-action" data-action="edit-mood-feedback" data-feedback-id="${escapeHtml(item.id)}">
          Editar
        </button>
        <button type="button" class="comm-tertiary-button" data-action="delete-mood-feedback" data-feedback-id="${escapeHtml(item.id)}">
          Excluir
        </button>
      </div>
    </article>
  `;
}

export function renderMoodFeedbackAdminSection(feedbackPage, {
  selectedOptionKey = "motivated",
  editingId = "",
  loadError = ""
} = {}) {
  const items = Array.isArray(feedbackPage?.items) ? feedbackPage.items : [];
  const summaries = Array.isArray(feedbackPage?.optionSummaries) ? feedbackPage.optionSummaries : [];
  const filteredItems = selectedOptionKey
    ? items.filter((item) => item.optionKey === selectedOptionKey)
    : items;

  return `
    <section class="card comm-list-card mood-feedback-admin" id="mood-feedback-admin">
      <div class="card-header mood-feedback-admin__header">
        <div>
          <span>Mensagens de feedback</span>
          <p>Respostas exibidas aleatoriamente apos o colaborador registrar o humor do dia.</p>
        </div>
      </div>
      <div class="comm-list-body mood-feedback-admin__body">
        ${loadError
          ? renderEmptyState("Nao foi possivel carregar as mensagens", loadError)
          : `
            <div class="mood-feedback-admin__summary">
              ${summaries.map(renderFeedbackSummaryChip).join("")}
            </div>

            <div class="mood-feedback-admin__filters" role="tablist" aria-label="Filtrar por humor">
              ${MOOD_FEEDBACK_OPTION_GROUPS.map((option) => `
                <button
                  type="button"
                  class="mood-feedback-filter ${selectedOptionKey === option.key ? "is-active" : ""}"
                  data-action="filter-mood-feedback"
                  data-option-key="${escapeHtml(option.key)}"
                >
                  ${escapeHtml(option.emoji)} ${escapeHtml(option.label)}
                </button>
              `).join("")}
            </div>

            <form class="mood-feedback-form mood-feedback-form--create" data-action="create-mood-feedback">
              <div class="mood-feedback-form__head">
                <strong>Nova mensagem</strong>
                <span>Cadastre uma frase para o humor selecionado no filtro.</span>
              </div>
              <div class="communication-form-grid">
                <label class="communication-form-field">
                  <span>Humor</span>
                  <select name="optionKey" required>
                    ${MOOD_FEEDBACK_OPTION_GROUPS.map((option) => `
                      <option value="${escapeHtml(option.key)}" ${option.key === selectedOptionKey ? "selected" : ""}>
                        ${escapeHtml(option.emoji)} ${escapeHtml(option.label)}
                      </option>
                    `).join("")}
                  </select>
                </label>
                <label class="communication-form-field">
                  <span>Ordem</span>
                  <input type="number" name="sortOrder" min="1" placeholder="Automatica" />
                </label>
                <label class="communication-form-field communication-form-field--full">
                  <span>Mensagem</span>
                  <textarea name="message" rows="3" placeholder="Escreva a mensagem de retorno para o colaborador" required></textarea>
                </label>
                <div class="communication-form-field communication-form-field--full">
                  <label class="communication-checkbox-wrap">
                    <input type="checkbox" name="isActive" checked />
                    Mensagem ativa no sorteio aleatorio
                  </label>
                </div>
              </div>
              <div class="communication-form-action-group">
                <button type="submit" class="feed-composer-submit">Adicionar mensagem</button>
              </div>
            </form>

            <div class="mood-feedback-list" id="mood-feedback-list">
              ${filteredItems.length
                ? filteredItems.map((item) => renderFeedbackRow(item, { editingId })).join("")
                : renderEmptyState("Nenhuma mensagem cadastrada", "Adicione a primeira frase de feedback para este humor.")}
            </div>
          `}
      </div>
    </section>
  `;
}
