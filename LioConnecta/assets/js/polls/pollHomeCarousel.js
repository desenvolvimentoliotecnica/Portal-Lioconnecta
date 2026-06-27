import { escapeHtml } from "../components/html.js";
import { mapPollItem } from "../services/pollService.js";
import { renderPollInteractionBlock } from "./pollVoteUi.js";

function renderPollAttachment(poll, className = "") {
  if (!poll?.attachmentUrl || !poll?.attachmentLabel) {
    return "";
  }

  const classes = ["comm-inline-action", className].filter(Boolean).join(" ");

  return `
    <a href="${escapeHtml(poll.attachmentUrl)}" class="${classes}" target="_blank" rel="noreferrer noopener">
      <i class="fa-solid fa-paperclip"></i>
      ${escapeHtml(poll.attachmentLabel)}
    </a>
  `;
}

function renderPollMedia(poll, { cover = false } = {}) {
  if (!poll?.imageUrl) {
    return "";
  }

  return `
    <div class="poll-media ${cover ? "is-cover" : ""}">
      <img src="${escapeHtml(poll.imageUrl)}" alt="${escapeHtml(poll.title || "Imagem da enquete")}" loading="lazy" />
    </div>
  `;
}

function renderPollHomeSlide(poll, index, total) {
  return `
    <div
      class="poll-home-carousel__slide carousel-slide"
      role="group"
      aria-label="Enquete ${index + 1} de ${total}"
      data-poll-id="${escapeHtml(poll.id)}"
    >
      <div class="poll-home-card__body">
        <div class="poll-home-card__main">
          <div class="comm-meta-row">
            <span class="comm-tag comm-tag--solid">${escapeHtml(poll.statusLabel)}</span>
            <span class="comm-tag" data-poll-votes-count>${escapeHtml(String(poll.totalVotes))} voto(s)</span>
            ${poll.isFeatured ? '<span class="comm-tag">Destaque</span>' : ""}
            ${poll.allowMultipleChoices ? '<span class="comm-tag">Multipla escolha</span>' : ""}
          </div>
          <h3>${escapeHtml(poll.title)}</h3>
          <p>${escapeHtml(poll.summary)}</p>
          <div class="poll-home-card__vote">
            ${renderPollInteractionBlock(poll, { compact: true })}
          </div>
          ${renderPollAttachment(poll) ? `<div class="poll-home-card__actions">${renderPollAttachment(poll)}</div>` : ""}
        </div>
        ${renderPollMedia(poll, { cover: true })}
      </div>
    </div>
  `;
}

export function renderHomePollCarousel(polls = []) {
  const items = Array.isArray(polls) ? polls : [];
  if (!items.length) {
    return "";
  }

  const showControls = items.length > 1;

  return `
    <section class="card poll-home-card" id="poll-home-carousel-section">
      <div class="card-header poll-home-card__header">
        <span>ENQUETES ABERTAS</span>
        ${showControls ? `<span class="poll-home-card__counter">${escapeHtml(String(items.length))} disponiveis</span>` : ""}
      </div>
      <div class="poll-home-carousel">
        <div class="carousel poll-home-carousel__viewport" id="poll-home-carousel" aria-roledescription="carousel">
          <div class="carousel-track poll-home-carousel__track">
            ${items.map((poll, index) => renderPollHomeSlide(poll, index, items.length)).join("")}
          </div>
        </div>
        ${showControls ? `
          <div class="poll-home-carousel__controls">
            <button type="button" class="comm-secondary-button poll-home-carousel__nav" data-action="poll-home-prev" aria-label="Enquete anterior">
              <i class="fa-solid fa-chevron-left" aria-hidden="true"></i>
            </button>
            <div class="carousel-dots poll-home-carousel__dots" id="poll-home-carousel-dots">
              ${items.map((_, index) => `
                <button
                  class="carousel-dot ${index === 0 ? "active" : ""}"
                  type="button"
                  data-index="${index}"
                  aria-label="Ir para a enquete ${index + 1}"
                ></button>
              `).join("")}
            </div>
            <button type="button" class="comm-secondary-button poll-home-carousel__nav" data-action="poll-home-next" aria-label="Proxima enquete">
              <i class="fa-solid fa-chevron-right" aria-hidden="true"></i>
            </button>
          </div>
        ` : ""}
      </div>
    </section>
  `;
}

export function updateHomePollSlideAfterVote(slide, pollPayload) {
  if (!slide || !pollPayload) {
    return;
  }

  const poll = mapPollItem(pollPayload);
  const voteRegion = slide.querySelector(".poll-home-card__vote");
  const votesTag = slide.querySelector("[data-poll-votes-count]");

  if (votesTag) {
    votesTag.textContent = `${poll.totalVotes} voto(s)`;
  }

  if (voteRegion) {
    voteRegion.innerHTML = renderPollInteractionBlock(poll, { compact: true });
  }
}

export function initPollHomeCarousel() {
  const carousel = document.getElementById("poll-home-carousel");
  const dots = Array.from(document.querySelectorAll("#poll-home-carousel-dots .carousel-dot"));
  const prevButton = document.querySelector("[data-action='poll-home-prev']");
  const nextButton = document.querySelector("[data-action='poll-home-next']");

  if (!carousel) {
    return;
  }

  const track = carousel.querySelector(".carousel-track");
  const total = dots.length || carousel.querySelectorAll(".poll-home-carousel__slide").length;
  if (!track || total <= 1) {
    return;
  }

  let current = 0;
  let timer = null;

  const render = (index) => {
    current = (index + total) % total;
    track.style.transform = `translateX(-${current * 100}%)`;
    dots.forEach((dot, dotIndex) => {
      dot.classList.toggle("active", dotIndex === current);
      dot.setAttribute("aria-pressed", dotIndex === current ? "true" : "false");
    });
  };

  const restartTimer = () => {
    if (timer) {
      window.clearInterval(timer);
    }
    timer = window.setInterval(() => {
      render(current + 1);
    }, 8000);
  };

  dots.forEach((dot) => {
    dot.addEventListener("click", () => {
      render(Number(dot.getAttribute("data-index") || 0));
      restartTimer();
    });
  });

  prevButton?.addEventListener("click", () => {
    render(current - 1);
    restartTimer();
  });

  nextButton?.addEventListener("click", () => {
    render(current + 1);
    restartTimer();
  });

  restartTimer();
}
