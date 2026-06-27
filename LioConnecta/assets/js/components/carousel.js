import { escapeHtml } from "./html.js";

export function renderCarouselSection(carousel) {
  const slides = Array.isArray(carousel?.slides) ? carousel.slides : [];

  if (!slides.length) {
    return "";
  }

  return `
    <section class="card news-card">
      <div class="card-header">${escapeHtml(carousel.title)}</div>
      <div class="news-grid">
        <div class="carousel" id="connecta-carousel" aria-roledescription="carousel">
          <div class="carousel-track">
            ${slides.map((slide, index) => `
              <div
                class="carousel-slide"
                role="group"
                aria-label="Slide ${index + 1} de ${slides.length}"
              >
                ${slide.href ? `
                  <a
                    href="${escapeHtml(slide.href)}"
                    class="carousel-slide-link"
                    data-analytics="carousel.open"
                    data-analytics-label="${escapeHtml(slide.alt || `Slide ${index + 1}`)}"
                    aria-label="Abrir comunicado relacionado ao slide ${index + 1}"
                  >
                    <img src="${escapeHtml(slide.src)}" alt="${escapeHtml(slide.alt)}">
                  </a>
                ` : `
                  <img src="${escapeHtml(slide.src)}" alt="${escapeHtml(slide.alt)}">
                `}
              </div>
            `).join("")}
          </div>
        </div>
      </div>
      <div class="carousel-dots" id="connecta-carousel-dots">
        ${slides.map((_, index) => `
          <button
            class="carousel-dot ${index === 0 ? "active" : ""}"
            type="button"
            data-index="${index}"
            data-analytics="carousel.navigate"
            data-analytics-label="Slide ${index + 1}"
            aria-label="Ir para o slide ${index + 1}"
          ></button>
        `).join("")}
      </div>
    </section>
  `;
}

export function initCarousel() {
  const carousel = document.getElementById("connecta-carousel");
  const dots = Array.from(document.querySelectorAll("#connecta-carousel-dots .carousel-dot"));

  if (!carousel || dots.length === 0) {
    return;
  }

  const track = carousel.querySelector(".carousel-track");
  const total = dots.length;
  let current = 0;
  let timer = null;

  const render = (index) => {
    current = index;
    track.style.transform = `translateX(-${index * 100}%)`;
    dots.forEach((dot, dotIndex) => {
      dot.classList.toggle("active", dotIndex === index);
      dot.setAttribute("aria-pressed", dotIndex === index ? "true" : "false");
    });
  };

  const next = () => render((current + 1) % total);

  const stop = () => {
    if (timer) {
      clearInterval(timer);
      timer = null;
    }
  };

  const start = () => {
    stop();
    timer = setInterval(next, 3500);
  };

  dots.forEach((dot) => {
    dot.addEventListener("click", () => {
      render(Number(dot.dataset.index));
      start();
    });
  });

  carousel.addEventListener("mouseenter", stop);
  carousel.addEventListener("mouseleave", start);
  carousel.addEventListener("focusin", stop);
  carousel.addEventListener("focusout", start);

  render(0);
  start();
}
