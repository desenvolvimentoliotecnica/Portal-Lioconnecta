import { DEFAULT_CAROUSEL_TITLE, DEFAULT_SLIDES } from "../view-models/defaults.js";
import { asArray, asString } from "./shared.js";

function mapSlide(slide, index) {
  return {
    src: asString(slide?.src, ""),
    alt: asString(slide?.alt, `Slide ${index + 1}`),
    href: asString(slide?.href, "")
  };
}

export function mapCarouselViewModel(raw = {}, { allowDefaults = true } = {}) {
  const slides = asArray(raw.slides)
    .map(mapSlide)
    .filter((slide) => slide.src);

  return {
    title: asString(raw.title, DEFAULT_CAROUSEL_TITLE),
    errorMessage: asString(raw.errorMessage, ""),
    slides: slides.length > 0 ? slides : (allowDefaults ? [...DEFAULT_SLIDES] : [])
  };
}
