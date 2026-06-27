import test from "node:test";
import assert from "node:assert/strict";

import { mapUserHomeContextViewModel } from "../../../assets/js/mappers/userMapper.js";

test("userMapper aplica defaults para estrutura ausente", () => {
  const result = mapUserHomeContextViewModel({});

  assert.equal(result.brand.name, "LIOCONNECTA");
  assert.equal(result.user.greeting, "Olá,");
  assert.equal(result.hero.title, "Bem-vindo à LIOCONNECTA!");
  assert.equal(result.navItems.length, 6);
  assert.equal(result.mood.items.length, 3);
  assert.deepEqual(result.composer.actions, ["Adicionar fotos"]);
});

test("userMapper normaliza dados válidos do contexto da home", () => {
  const result = mapUserHomeContextViewModel({
    brand: { name: "Portal X", tagline: "Tag X" },
    user: { greeting: "Bem-vindo,", name: "Ana" },
    navItems: [{ label: "Dashboard", active: true }],
    hero: { title: "Título", subtitle: "Subtítulo" },
    mood: { title: "Humor", items: [{ emoji: "🔥", label: "Animado", rank: "Topo" }] },
    composer: { title: "Postar", placeholder: "Escreva", actions: ["Foto", "Vídeo"] }
  });

  assert.equal(result.brand.name, "Portal X");
  assert.equal(result.user.name, "Ana");
  assert.equal(result.navItems[0].label, "Dashboard");
  assert.equal(result.hero.subtitle, "Subtítulo");
  assert.equal(result.mood.items[0].emoji, "🔥");
  assert.deepEqual(result.composer.actions, ["Foto", "Vídeo"]);
});
