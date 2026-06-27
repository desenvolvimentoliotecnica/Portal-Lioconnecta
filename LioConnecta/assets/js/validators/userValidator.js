import { ensureArray, ensureBoolean, ensureObject, ensureString, isObject, throwIfInvalid } from "./shared.js";

export function validateUserContract(raw) {
  const issues = [];

  if (!ensureObject("user", raw, issues)) {
    throwIfInvalid("user", issues);
  }

  if (raw.brand !== undefined) {
    if (isObject(raw.brand)) {
      ensureString(raw.brand.name, issues, "brand.name");
      ensureString(raw.brand.tagline, issues, "brand.tagline");
    } else {
      issues.push("brand deve ser um objeto");
    }
  }

  if (raw.user !== undefined) {
    if (isObject(raw.user)) {
      ensureString(raw.user.name, issues, "user.name");
      ensureString(raw.user.greeting, issues, "user.greeting");
      ensureString(raw.user.area, issues, "user.area");
      if (raw.user.notificationCount !== undefined && typeof raw.user.notificationCount !== "number") {
        issues.push("user.notificationCount deve ser numérico");
      }
    } else {
      issues.push("user deve ser um objeto");
    }
  }

  if (raw.navItems !== undefined && ensureArray("user", raw.navItems, issues, "navItems")) {
    raw.navItems.forEach((item, index) => {
      if (!isObject(item)) {
        issues.push(`navItems[${index}] deve ser um objeto`);
        return;
      }

      ensureString(item.label, issues, `navItems[${index}].label`, { required: true });
      ensureString(item.route, issues, `navItems[${index}].route`);
      ensureString(item.moduleKey, issues, `navItems[${index}].moduleKey`);
      ensureBoolean(item.active, issues, `navItems[${index}].active`);
    });
  }

  if (raw.hero !== undefined) {
    if (isObject(raw.hero)) {
      ensureString(raw.hero.title, issues, "hero.title");
      ensureString(raw.hero.subtitle, issues, "hero.subtitle");
    } else {
      issues.push("hero deve ser um objeto");
    }
  }

  if (raw.mood !== undefined) {
    if (isObject(raw.mood)) {
      ensureString(raw.mood.title, issues, "mood.title");
      if (raw.mood.items !== undefined && ensureArray("user", raw.mood.items, issues, "mood.items")) {
        raw.mood.items.forEach((item, index) => {
          if (!isObject(item)) {
            issues.push(`mood.items[${index}] deve ser um objeto`);
            return;
          }

          ensureString(item.emoji, issues, `mood.items[${index}].emoji`);
          ensureString(item.label, issues, `mood.items[${index}].label`);
          ensureString(item.rank, issues, `mood.items[${index}].rank`);
        });
      }
    } else {
      issues.push("mood deve ser um objeto");
    }
  }

  if (raw.composer !== undefined) {
    if (isObject(raw.composer)) {
      ensureString(raw.composer.title, issues, "composer.title");
      ensureString(raw.composer.placeholder, issues, "composer.placeholder");
      if (raw.composer.actions !== undefined && ensureArray("user", raw.composer.actions, issues, "composer.actions")) {
        raw.composer.actions.forEach((item, index) => ensureString(item, issues, `composer.actions[${index}]`, { required: true }));
      }
      if (raw.composer.enabled !== undefined && typeof raw.composer.enabled !== "boolean") {
        issues.push("composer.enabled deve ser booleano");
      }
    } else {
      issues.push("composer deve ser um objeto");
    }
  }

  throwIfInvalid("user", issues);
  return raw;
}
