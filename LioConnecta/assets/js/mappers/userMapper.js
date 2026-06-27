import {
  DEFAULT_BRAND,
  DEFAULT_COMPOSER,
  DEFAULT_HERO,
  DEFAULT_MOOD_ITEMS,
  DEFAULT_MOOD_TITLE,
  DEFAULT_NAV_ITEMS,
  DEFAULT_USER
} from "../view-models/defaults.js";
import { asArray, asBoolean, asString } from "./shared.js";

function mapNavItem(item, index) {
  const fallback = DEFAULT_NAV_ITEMS[index] ?? { label: `Item ${index + 1}`, active: false, route: "", moduleKey: "" };

  return {
    label: asString(item?.label, fallback.label),
    route: asString(item?.route, fallback.route || ""),
    moduleKey: asString(item?.moduleKey, fallback.moduleKey || ""),
    active: asBoolean(item?.active, fallback.active)
  };
}

function mapMoodItem(item, index) {
  const fallback = DEFAULT_MOOD_ITEMS[index] ?? DEFAULT_MOOD_ITEMS[0];

  return {
    emoji: asString(item?.emoji, fallback.emoji),
    label: asString(item?.label, fallback.label),
    rank: asString(item?.rank, fallback.rank)
  };
}

export function mapUserHomeContextViewModel(raw = {}) {
  return {
    brand: {
      name: asString(raw.brand?.name, DEFAULT_BRAND.name),
      tagline: asString(raw.brand?.tagline, DEFAULT_BRAND.tagline)
    },
    user: {
      greeting: asString(raw.user?.greeting, DEFAULT_USER.greeting),
      name: asString(raw.user?.name, DEFAULT_USER.name),
      area: asString(raw.user?.area, ""),
      notificationCount: Number(raw.user?.notificationCount ?? 0) || 0,
      photoUrl: asString(raw.user?.photoUrl, "")
    },
    navItems: asArray(raw.navItems).length
      ? asArray(raw.navItems).map(mapNavItem)
      : DEFAULT_NAV_ITEMS.map((item) => ({ ...item })),
    hero: {
      title: asString(raw.hero?.title, DEFAULT_HERO.title),
      subtitle: asString(raw.hero?.subtitle, DEFAULT_HERO.subtitle)
    },
    mood: {
      title: asString(raw.mood?.title, DEFAULT_MOOD_TITLE),
      items: asArray(raw.mood?.items).length
        ? asArray(raw.mood.items).map(mapMoodItem)
        : DEFAULT_MOOD_ITEMS.map((item) => ({ ...item }))
    },
    composer: {
      enabled: raw.composer?.enabled !== false,
      title: asString(raw.composer?.title, DEFAULT_COMPOSER.title),
      placeholder: asString(raw.composer?.placeholder, DEFAULT_COMPOSER.placeholder),
      actions: asArray(raw.composer?.actions).length
        ? asArray(raw.composer.actions).map((action) => asString(action)).filter(Boolean)
        : [...DEFAULT_COMPOSER.actions]
    }
  };
}
