import { escapeHtml } from "../components/html.js";

function renderPollOptionBar(option, showResults = false) {
  const extra = showResults
    ? `<span class="poll-option-result">${escapeHtml(String(option.votes))} voto(s) • ${escapeHtml(String(option.percentage))}%</span>`
    : `<span class="poll-option-result">${option.isSelected ? "Seu voto" : "Escolha disponivel"}</span>`;

  return `
    <div class="poll-option-bar ${option.isSelected ? "is-selected" : ""}">
      <div class="poll-option-bar__top">
        <strong>${escapeHtml(option.label)}</strong>
        ${extra}
      </div>
      <div class="poll-option-bar__track">
        <span style="width:${showResults ? Math.min(option.percentage, 100) : 0}%"></span>
      </div>
    </div>
  `;
}

function renderPollVoteInput(poll, option) {
  const type = poll.allowMultipleChoices ? "checkbox" : "radio";

  return `
    <label class="poll-vote-choice ${option.isSelected ? "is-selected" : ""}">
      <input
        type="${type}"
        name="poll-choice-${escapeHtml(poll.id)}"
        value="${escapeHtml(option.id)}"
        ${option.isSelected ? "checked" : ""}
      />
      <span>${escapeHtml(option.label)}</span>
    </label>
  `;
}

export function renderPollVoteForm(poll, compact = false) {
  return `
    <form class="poll-vote-form" data-action="submit-poll-vote" data-poll-id="${escapeHtml(poll.id)}">
      <div class="poll-vote-options ${compact ? "is-compact" : ""}">
        ${poll.options.map((option) => renderPollVoteInput(poll, option)).join("")}
      </div>
      <div class="poll-vote-actions">
        <button type="submit" class="feed-composer-submit">${compact ? "Votar" : "Registrar voto"}</button>
        ${poll.allowMultipleChoices
          ? '<span class="poll-form-hint">Voce pode selecionar mais de uma opcao.</span>'
          : '<span class="poll-form-hint">Escolha apenas uma alternativa.</span>'}
      </div>
    </form>
  `;
}

export function renderPollResults(poll) {
  return `
    <div class="poll-results-list">
      ${poll.options.map((option) => renderPollOptionBar(option, true)).join("")}
    </div>
  `;
}

export function renderPollInteractionBlock(poll, { compact = false } = {}) {
  if (poll.status === "Published" && !poll.hasVoted) {
    return renderPollVoteForm(poll, compact);
  }

  if (poll.resultsVisible) {
    return renderPollResults(poll);
  }

  return `
    <div class="poll-results-locked">
      <i class="fa-solid fa-lock"></i>
      <span>Os resultados serao exibidos ${escapeHtml(poll.resultsVisibilityLabel.toLowerCase())}.</span>
    </div>
  `;
}
