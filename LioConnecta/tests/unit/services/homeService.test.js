import test from "node:test";
import assert from "node:assert/strict";

import { getHomePageData } from "../../../assets/js/services/homeService.js";

test("homeService compõe a home a partir dos services de domínio", async () => {
  const originalFetch = global.fetch;
  const payloads = new Map([
    ["./assets/data/user.json", { brand: { name: "LIO" }, user: { name: "Leo" } }],
    ["http://localhost:3030/api/communications", [{ slug: "slide-api", title: "Slide API", imageUrl: "./slide.png", publishedAt: "2026-06-19T09:00:00Z" }]],
    ["http://localhost:3030/api/polls", []],
    ["./assets/data/feed.json", { posts: [{ author: "Ana", text: "Feed ok" }] }],
    ["./assets/data/panels.json", {
      leftPanels: [{ title: "L" }],
      rightPanels: [{ title: "AGENDA", items: [{ label: "09:00 • Daily RH" }] }]
    }]
  ]);

  global.fetch = async (url) => ({
    ok: true,
    async json() {
      return payloads.get(url);
    }
  });

  try {
    const result = await getHomePageData();

    assert.equal(result.brand.name, "LIO");
    assert.equal(result.user.name, "Leo");
    assert.equal(result.carousel.slides.length, 1);
    assert.equal(result.feed.posts.length, 1);
    assert.equal(result.leftPanels[0].title, "L");
    assert.equal(result.rightPanels[0].title, "AGENDA");
    assert.equal(result.rightPanels[0].items[0].label, "09:00 • Daily RH");
  } finally {
    global.fetch = originalFetch;
  }
});
