import test from "node:test";
import assert from "node:assert/strict";

import { APP_VERSION, getRuntimeConfig, resolveDataSource, usesEnvelope } from "../../../assets/js/core/runtimeConfig.js";

test("runtimeConfig usa mock por padrão", () => {
  const config = getRuntimeConfig();

  assert.equal(config.version, APP_VERSION);
  assert.equal(config.dataMode, "mock");
  assert.equal(resolveDataSource("feed"), "./assets/data/feed.json");
  assert.equal(resolveDataSource("communications"), "./assets/data/communications.json");
  assert.equal(usesEnvelope(config.dataMode), false);
});

test("runtimeConfig mantém shell estático mesmo em modo api", () => {
  const previousWindow = globalThis.window;
  globalThis.window = {
    location: { hostname: "10.0.0.80", protocol: "http:", search: "?dataMode=api" },
    localStorage: {
      getItem: () => null
    }
  };

  try {
    assert.equal(resolveDataSource("user"), "http://10.0.0.80:3030/api/me-ui");
    assert.equal(resolveDataSource("panels"), "http://10.0.0.80:3030/api/panels");
    assert.equal(resolveDataSource("feed"), "http://10.0.0.80:3030/api/feed");
    assert.equal(resolveDataSource("carousel"), "./assets/data/carousel.json");
  } finally {
    globalThis.window = previousWindow;
  }
});
