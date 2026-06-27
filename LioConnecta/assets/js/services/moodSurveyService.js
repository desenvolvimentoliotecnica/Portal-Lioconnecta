import { getJson, postJson } from "./apiClient.js";
import { resolveApiEndpoint } from "../core/runtimeConfig.js";
import { getPortalAuthHeaders } from "./portalAuthService.js";

const DEFAULT_MOOD_TITLE = "Como você está se sentindo hoje?";

const FALLBACK_ITEMS = [
  { key: "motivated", emoji: "😄", label: "Motivado", rank: "1º mais votado", voteCount: 0 },
  { key: "good", emoji: "🙂", label: "Bem", rank: "2º mais votado", voteCount: 0 },
  { key: "tired", emoji: "😴", label: "Cansado", rank: "3º mais votado", voteCount: 0 }
];

const THANK_YOU_MESSAGES = {
  motivated: "Que energia! Continue inspirando o time hoje.",
  good: "Ótimo! Um dia equilibrado começa com uma boa atitude.",
  tired: "Respire fundo. Cada passo conta — você não está sozinho."
};

function normalizeMoodSurveyPayload(payload = {}) {
  const items = Array.isArray(payload.items) ? payload.items : [];
  const selectedOptionKey = payload.selectedOptionKey || null;

  return {
    title: payload.title || DEFAULT_MOOD_TITLE,
    surveyDate: payload.surveyDate || null,
    hasVoted: Boolean(payload.hasVoted),
    selectedOptionKey,
    thankYouMessage: payload.thankYouMessage || THANK_YOU_MESSAGES[selectedOptionKey] || "",
    items: items.length
      ? items.map((item) => ({
          key: item.key,
          emoji: item.emoji,
          label: item.label,
          rank: item.rank,
          voteCount: Number(item.voteCount ?? 0) || 0
        }))
      : FALLBACK_ITEMS
  };
}

export async function getMoodSurveyToday() {
  const payload = await getJson(resolveApiEndpoint("moodSurveyToday"), {
    headers: getPortalAuthHeaders()
  });

  return normalizeMoodSurveyPayload(payload);
}

export async function submitMoodSurveyVote(optionKey) {
  const payload = await postJson(
    resolveApiEndpoint("moodSurveyVote"),
    { optionKey },
    { headers: getPortalAuthHeaders() }
  );

  return normalizeMoodSurveyPayload(payload);
}

export function mapMoodSurveyToViewModel(moodSurvey) {
  const normalized = normalizeMoodSurveyPayload(moodSurvey);

  return {
    title: normalized.title,
    hasVoted: normalized.hasVoted,
    selectedOptionKey: normalized.selectedOptionKey,
    thankYouMessage: normalized.thankYouMessage,
    items: normalized.items.map((item) => ({
      key: item.key,
      emoji: item.emoji,
      label: item.label,
      rank: item.rank
    })),
    selectedOption: normalized.items.find((item) => item.key === normalized.selectedOptionKey) || null
  };
}

export function replaceMoodCardElement(moodViewModel) {
  const moodCard = document.querySelector(".mood-card");
  if (!moodCard) {
    return;
  }

  moodCard.outerHTML = renderMoodCardHtml(moodViewModel);
}

export function renderMoodCardHtml(mood = {}) {
  const viewModel = mapMoodSurveyToViewModel(mood);

  if (viewModel.hasVoted) {
    const selectedEmoji = viewModel.selectedOption?.emoji || "✨";
    const selectedLabel = viewModel.selectedOption?.label || "registrado";
    const message = viewModel.thankYouMessage || "Obrigado por compartilhar como você está hoje.";

    return `
      <section class="card mood-card mood-card--completed" data-mood-state="completed">
        <h2>${escapeHtml(viewModel.title)}</h2>
        <div class="mood-thankyou" role="status" aria-live="polite">
          <span class="mood-thankyou-emoji" aria-hidden="true">${escapeHtml(selectedEmoji)}</span>
          <strong class="mood-thankyou-title">Humor registrado: ${escapeHtml(selectedLabel)}</strong>
          <p class="mood-thankyou-message">${escapeHtml(message)}</p>
          <span class="mood-thankyou-hint">Você poderá responder novamente amanhã.</span>
        </div>
      </section>
    `;
  }

  return `
    <section class="card mood-card" data-mood-state="voting">
      <h2>${escapeHtml(viewModel.title)}</h2>
      <div class="mood-options" role="list" aria-label="Pesquisa de humor do dia">
        ${viewModel.items.map((item) => `
          <button
            class="mood-option"
            type="button"
            role="listitem"
            data-mood-option-key="${escapeHtml(item.key)}"
            data-analytics="mood.vote"
            data-analytics-label="${escapeHtml(item.label)}"
            aria-label="${escapeHtml(item.label)} - ${escapeHtml(item.rank)}"
          >
            <span class="mood-option-emoji" aria-hidden="true">${escapeHtml(item.emoji)}</span>
            <strong>${escapeHtml(item.label)}</strong>
            <span class="mood-rank">${escapeHtml(item.rank)}</span>
          </button>
        `).join("")}
      </div>
    </section>
  `;
}

function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll("\"", "&quot;")
    .replaceAll("'", "&#39;");
}
