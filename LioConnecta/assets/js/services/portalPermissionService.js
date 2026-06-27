import { getStoredPortalSession } from "./portalAuthService.js";

const ACCESS_LEVEL_RANK = Object.freeze({
  None: 0,
  View: 1,
  Interact: 2,
  Manage: 3
});

export function getModuleAccessLevel(session = getStoredPortalSession(), moduleKey = "") {
  const normalizedKey = String(moduleKey ?? "").trim();
  if (!normalizedKey) {
    return "None";
  }

  const permissions = session?.user?.modulePermissions || [];
  const match = permissions.find((item) => item.moduleKey === normalizedKey);
  return String(match?.accessLevel || "None");
}

export function hasModuleAccess(session, moduleKey, minimumLevel = "View") {
  const currentRank = ACCESS_LEVEL_RANK[getModuleAccessLevel(session, moduleKey)] ?? 0;
  const minimumRank = ACCESS_LEVEL_RANK[String(minimumLevel || "View")] ?? 1;
  return currentRank >= minimumRank;
}

export function canInteractWithFeed(session = getStoredPortalSession()) {
  return hasModuleAccess(session, "feed", "Interact");
}

export function canViewRoute(session, route, minimumLevel = "View") {
  const requirement = ROUTE_MODULE_ACCESS[route];
  if (!requirement) {
    return true;
  }

  return hasModuleAccess(session, requirement.moduleKey, requirement.minimumLevel || minimumLevel);
}

export const ROUTE_MODULE_ACCESS = Object.freeze({
  inicio: { moduleKey: "home", minimumLevel: "View" },
  "inicio/salvos": { moduleKey: "home", minimumLevel: "View" },
  "perfil-rh/ferias": { moduleKey: "hr-profile", minimumLevel: "View" },
  "perfil-rh/holerite": { moduleKey: "hr-profile", minimumLevel: "View" },
  "perfil-rh/beneficios": { moduleKey: "hr-profile", minimumLevel: "View" },
  "perfil-rh/avaliacao": { moduleKey: "hr-profile", minimumLevel: "View" },
  "perfil-rh/cadastro": { moduleKey: "hr-profile", minimumLevel: "View" },
  "perfil-rh/ponto": { moduleKey: "hr-profile", minimumLevel: "View" },
  "minha-jornada/tarefas": { moduleKey: "home", minimumLevel: "View" },
  "minha-jornada/solicitacoes": { moduleKey: "home", minimumLevel: "View" },
  "minha-jornada/trilhas": { moduleKey: "home", minimumLevel: "View" },
  "minha-jornada/documentos": { moduleKey: "home", minimumLevel: "View" },
  comunicacao: { moduleKey: "communications", minimumLevel: "View" },
  "comunicacao/leitura": { moduleKey: "communications", minimumLevel: "View" },
  enquetes: { moduleKey: "polls", minimumLevel: "View" },
  "enquetes/leitura": { moduleKey: "polls", minimumLevel: "View" },
  "pessoas-rh": { moduleKey: "hr-profile", minimumLevel: "View" },
  "comunicacao/restrita": { moduleKey: "communication-admin", minimumLevel: "Manage" },
  "admin/enquetes": { moduleKey: "poll-admin", minimumLevel: "Manage" },
  sistemas: { moduleKey: "home", minimumLevel: "View" },
  projetos: { moduleKey: "home", minimumLevel: "View" },
  recursos: { moduleKey: "home", minimumLevel: "View" },
  configuracoes: { moduleKey: "settings", minimumLevel: "Manage" },
  "configuracoes/ldap": { moduleKey: "settings", minimumLevel: "Manage" },
  "configuracoes/microsoft-graph": { moduleKey: "settings", minimumLevel: "Manage" },
  "admin/usuarios": { moduleKey: "user-admin", minimumLevel: "Manage" }
});
