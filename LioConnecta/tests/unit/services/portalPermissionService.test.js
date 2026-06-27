import test from "node:test";
import assert from "node:assert/strict";

import { canViewRoute, hasModuleAccess } from "../../../assets/js/services/portalPermissionService.js";

const baseSession = {
  user: {
    modulePermissions: [
      { moduleKey: "home", accessLevel: "View" },
      { moduleKey: "communications", accessLevel: "View" },
      { moduleKey: "polls", accessLevel: "Interact" },
      { moduleKey: "feed", accessLevel: "Interact" },
      { moduleKey: "settings", accessLevel: "View" }
    ]
  }
};

test("portalPermissionService valida niveis minimos por modulo", () => {
  assert.equal(hasModuleAccess(baseSession, "feed", "Interact"), true);
  assert.equal(hasModuleAccess(baseSession, "feed", "Manage"), false);
  assert.equal(hasModuleAccess(baseSession, "settings", "Manage"), false);
});

test("portalPermissionService bloqueia rotas sem permissao adequada", () => {
  assert.equal(canViewRoute(baseSession, "inicio"), true);
  assert.equal(canViewRoute(baseSession, "comunicacao"), true);
  assert.equal(canViewRoute(baseSession, "configuracoes"), false);
});
