import test from "node:test";
import assert from "node:assert/strict";

import { mapCarouselViewModel } from "../../../assets/js/mappers/carouselMapper.js";

test("carouselMapper remove slides sem src e aplica título default", () => {
  const result = mapCarouselViewModel({
    slides: [
      { src: "./ok.png", alt: "Ok" },
      { alt: "Inválido" }
    ]
  });

  assert.equal(result.title, "COMUNICAÇÃO CENTRALIZADA");
  assert.equal(result.slides.length, 1);
  assert.equal(result.slides[0].src, "./ok.png");
});

test("carouselMapper aplica alt fallback por índice", () => {
  const result = mapCarouselViewModel({
    title: "Carrossel",
    slides: [{ src: "./slide.png" }]
  });

  assert.equal(result.title, "Carrossel");
  assert.equal(result.slides[0].alt, "Slide 1");
});

test("carouselMapper preserva href do slide", () => {
  const result = mapCarouselViewModel({
    slides: [{ src: "./slide.png", href: "#comunicacao/leitura/exemplo" }]
  });

  assert.equal(result.slides[0].href, "#comunicacao/leitura/exemplo");
});

test("carouselMapper nao usa slides default quando allowDefaults e false", () => {
  const result = mapCarouselViewModel({ slides: [] }, { allowDefaults: false });

  assert.equal(result.slides.length, 0);
});
