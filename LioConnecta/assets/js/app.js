import { bindAnalytics, trackInteraction } from "./analytics.js?v=0.12.8";
import { renderHeaderShell, renderSidebarPanels } from "./layout/index.js?v=0.12.8";
import {
  renderHero,
  renderMoodCard,
  renderErrorCard,
  renderEmptyState,
  renderLoadingHeader,
  renderLoadingPanel,
  renderLoadingHero,
  renderLoadingMoodCard,
  renderLoadingCarousel,
  renderLoadingFeed,
  getHomePageData
} from "./home/index.js?v=0.12.8";
import {
  renderCarouselSection,
  initCarousel,
  renderCommunicationsHub,
  renderCommunicationDetailPage,
  renderCommunicationAdminPage,
  initCommunicationAdminWizard,
  openCommunicationAdminWizard,
  closeCommunicationAdminWizard,
  renderAdminSettingsPage,
  renderAdminUsersPage,
  renderAdminUsersKpiSection,
  renderAdminUsersResultsSection,
  renderAdminUsersActivitySection,
  renderPortalUserModal,
  getCommunicationCenterData,
} from "./communications/index.js?v=0.15.3";
import {
  canManageCommunications,
  createCommunication,
  updateCommunication,
  deleteCommunication,
  getCommunicationEditorHeaders
} from "./services/communicationService.js?v=0.15.4";
import {
  renderHomePollCarousel,
  initPollHomeCarousel,
  updateHomePollSlideAfterVote,
  renderPollsHub,
  renderPollDetailPage,
  renderAdminPollsPage,
  initPollAdminWizard,
  openPollAdminWizard,
  closePollAdminWizard,
  getPollCenterData,
  getPollDetailData,
  getAdminPollData,
  canManagePolls,
  createPoll,
  updatePoll,
  updatePollStatus,
  uploadPollAsset,
  votePoll
} from "./polls/index.js?v=0.15.0";
import { renderFeed, renderSavedFeed } from "./feed/index.js?v=0.22.2";
import { updateFeedLikeUi, createFeedPost, toggleFeedLike, toggleFeedShare, toggleFeedSave, getSavedFeedData, uploadFeedAsset, deleteFeedPost, updateFeedShareUi, updateFeedSaveUi } from "./services/feedService.js?v=0.22.2";
import { bindFeedPhotoComposerActions, clearPendingFeedPhotos, getPendingFeedPhotos } from "./components/feedPhotoModal.js?v=0.21.4";
import { bindFeedPhotoViewerActions } from "./components/feedPhotoViewerModal.js?v=0.21.4";
import { bindFeedPostCommentActions } from "./components/feedPostCommentComposer.js?v=0.21.4";
import { bindMentionField } from "./components/feedMentions.js?v=0.21.7";
import { bindInteractionFeedback, showToast, confirmAction } from "./core/feedback.js?v=0.22.1";
import { DATA_MODES, getRuntimeConfig } from "./core/runtimeConfig.js?v=0.21.4";
import { getPanelData } from "./services/panelService.js?v=0.12.8";
import { getUserHomeContext } from "./services/userService.js?v=0.12.8";
import { applyAgendaToShellData, collectAgendaEventsFromPanels, getAgendaDayData } from "./services/agendaService.js?v=0.23.7";
import { bindAgendaEventModalActions, setAgendaEvents } from "./components/agendaEventModal.js?v=0.23.9";
import { applyNotificationsToShellData, getNotificationCenterData } from "./services/notificationService.js?v=0.13.0";
import { fetchAdminSession, getAdminAuthHeaders, getStoredAdminSession, isSuperAdminSession, redirectToAdminLogin } from "./services/adminAuthService.js?v=0.12.8";
import { ensureValidPortalSession, getPortalAuthHeaders, getStoredPortalSession, logoutPortal, redirectToPortalLogin } from "./services/portalAuthService.js?v=0.13.0";
import { canInteractWithFeed, canViewRoute } from "./services/portalPermissionService.js?v=0.17.0";
import { renderLdapWizardPage, initLdapWizard, renderMicrosoftGraphSettingsPage, initMicrosoftGraphSettings } from "./settings/index.js?v=0.15.0";
import { getLdapSettingsData } from "./services/ldapSettingsService.js?v=0.12.8";
import { getMicrosoftGraphSettingsData } from "./services/microsoftGraphSettingsService.js?v=0.23.2";
import { listPortalUsers } from "./services/portalUsersAdminService.js?v=0.12.8";
import { renderRhMoodDashboardPage, initMoodDashboardCharts, destroyMoodDashboardCharts, wrapRhAdminShell } from "./people/index.js?v=0.14.5";
import {
  canAccessHrMoodDashboard,
  getMoodSurveyDashboard,
  resolveMoodDashboardPeriod
} from "./services/moodSurveyDashboardService.js?v=0.14.1";
import {
  HR_PROFILE_ROUTE,
  isHrProfileModuleSlug,
  getHrProfileModuleData,
  renderHrProfileModulePage
} from "./hrProfile/index.js?v=0.23.3";
import {
  JOURNEY_ROUTE,
  isJourneyModuleSlug,
  getJourneyModuleData,
  renderJourneyModulePage
} from "./journey/index.js?v=0.23.7";
import {
  canManageMoodSurveyFeedback,
  listMoodFeedbackMessages,
  createMoodFeedbackMessage,
  updateMoodFeedbackMessage,
  deleteMoodFeedbackMessage
} from "./services/moodSurveyFeedbackService.js?v=0.14.1";

const ROUTES = Object.freeze({
  HOME: "inicio",
  SAVED: "inicio/salvos",
  COMMUNICATIONS: "comunicacao",
  COMMUNICATION_READ: "comunicacao/leitura",
  POLLS: "enquetes",
  POLL_READ: "enquetes/leitura",
  COMMUNICATION_ADMIN: "comunicacao/restrita",
  SETTINGS: "configuracoes",
  SETTINGS_LDAP: "configuracoes/ldap",
  SETTINGS_MICROSOFT_GRAPH: "configuracoes/microsoft-graph",
  ADMIN_USERS: "admin/usuarios",
  ADMIN_POLLS: "admin/enquetes",
  PEOPLE: "pessoas-rh",
  HR_PROFILE: HR_PROFILE_ROUTE,
  JOURNEY: JOURNEY_ROUTE,
  SYSTEMS: "sistemas",
  PROJECTS: "projetos",
  RESOURCES: "recursos"
});

const BASE_NAV_ITEMS = Object.freeze([
  { route: ROUTES.HOME, label: "INICIO" },
  { route: ROUTES.COMMUNICATIONS, label: "COMUNICACAO" },
  { route: ROUTES.POLLS, label: "ENQUETES" },
  { route: ROUTES.PEOPLE, label: "PESSOAS (RH)" },
  { route: ROUTES.SYSTEMS, label: "SISTEMAS" },
  { route: ROUTES.PROJECTS, label: "PROJETOS" },
  { route: ROUTES.RESOURCES, label: "RECURSOS" }
]);

let currentShellNavItems = [];

function getNavRoutes() {
  if (currentShellNavItems.length && currentShellNavItems[0]?.route) {
    const routes = currentShellNavItems.map((item) => ({
      route: item.route,
      label: item.label
    }));

    if (isSuperAdminSession() && !routes.some((item) => item.route === ROUTES.SETTINGS)) {
      routes.push({ route: ROUTES.SETTINGS, label: "CONFIGURACOES" });
    }

    return routes;
  }

  return isSuperAdminSession()
    ? [...BASE_NAV_ITEMS, { route: ROUTES.SETTINGS, label: "CONFIGURACOES" }]
    : [...BASE_NAV_ITEMS];
}

let shellInitialized = false;
let adminUsersRefreshBound = false;
let moodDashboardQueryState = {
  periodPreset: "7d",
  department: "all"
};
let moodFeedbackQueryState = {
  optionKey: "motivated",
  editingId: ""
};
let currentPeopleRhData = {
  moodDashboard: null,
  moodDashboardLoadError: "",
  moodFeedbackPage: null,
  moodFeedbackLoadError: ""
};
let adminUsersSearchDebounce = 0;
let currentAdminUsersPage = createEmptyPortalUsersPage();
let currentAdminPollsPage = createEmptyAdminPollsPage();
let currentAdminPollEditingId = "";
let pollWizardAutoOpen = false;
let currentCommunicationsPage = null;
let currentCommunicationEditingId = "";
let commWizardAutoOpen = false;
let adminUsersQueryState = {
  query: "",
  status: "all",
  role: "all",
  department: "all",
  sortBy: "displayName",
  sortDirection: "asc",
  page: 1,
  pageSize: 8
};

function createEmptyPortalUsersPage() {
  return {
    items: [],
    summary: {
      registeredUsers: 0,
      activeUsers: 0,
      inactiveUsers: 0,
      departmentsMapped: 0,
      portalAdmins: 0,
      loginEvents: 0,
      failedLoginEvents: 0,
      logoutEvents: 0,
      moodSurveyEvents: 0
    },
    roleOptions: [],
    departmentOptions: [],
    moduleOptions: [],
    accessLevelOptions: [],
    recentLogins: [],
    recentAuditEntries: [],
    recentMoodSurveyEntries: [],
    page: 1,
    pageSize: 8,
    totalItems: 0,
    totalPages: 1,
    query: "",
    status: "all",
    role: "",
    department: "all",
    sortBy: "displayName",
    sortDirection: "asc"
  };
}

function createEmptyAdminPollsPage() {
  return {
    intro: {
      eyebrow: "ADMINISTRATIVO",
      title: "Enquetes",
      subtitle: "Publique novas pesquisas e acompanhe a participacao do portal.",
      loadError: ""
    },
    items: [],
    summary: {
      totalPolls: 0,
      publishedPolls: 0,
      draftPolls: 0,
      closedPolls: 0,
      archivedPolls: 0,
      totalVotes: 0
    },
    statusOptions: [],
    resultsVisibilityOptions: []
  };
}

function buildAdminUsersQuery() {
  return {
    query: adminUsersQueryState.query,
    status: adminUsersQueryState.status,
    role: adminUsersQueryState.role,
    department: adminUsersQueryState.department,
    sortBy: adminUsersQueryState.sortBy,
    sortDirection: adminUsersQueryState.sortDirection,
    page: adminUsersQueryState.page,
    pageSize: adminUsersQueryState.pageSize
  };
}

function bindPortalTopbarActions() {
  document.querySelectorAll(".avatar__photo").forEach((image) => {
    if (image.dataset.bound === "true") {
      return;
    }

    image.dataset.bound = "true";
    image.addEventListener("error", () => {
      image.classList.add("is-hidden");
    }, { once: true });
  });

  const logoutButton = document.querySelector("[data-action='portal-logout']");
  if (!logoutButton || logoutButton.dataset.bound === "true") {
    return;
  }

  logoutButton.dataset.bound = "true";
  logoutButton.addEventListener("click", async () => {
    await logoutPortal();
    showToast("Sessao encerrada com sucesso.", "info");

    window.setTimeout(() => {
      redirectToPortalLogin(window.location.hash || "#inicio");
    }, 250);
  });
}

function isRestrictedAdminRoute(route) {
  return (
    route === ROUTES.SETTINGS ||
    route === ROUTES.SETTINGS_LDAP ||
    route === ROUTES.SETTINGS_MICROSOFT_GRAPH ||
    route === ROUTES.ADMIN_USERS
  );
}

function isRhWorkspaceRoute(route) {
  return (
    route === ROUTES.PEOPLE ||
    route === ROUTES.ADMIN_POLLS ||
    route === ROUTES.COMMUNICATION_ADMIN
  );
}

function isSidebarlessRoute(route) {
  return isRestrictedAdminRoute(route) || isRhWorkspaceRoute(route);
}

function applyLayoutMode(route) {
  const content = document.getElementById("main-content");
  if (!content) {
    return;
  }

  content.classList.toggle("content--single", isSidebarlessRoute(route));
}

async function setupServiceWorker() {
  if (!("serviceWorker" in navigator)) {
    return;
  }

  const isLocalHost = ["127.0.0.1", "localhost"].includes(window.location.hostname);

  if (isLocalHost) {
    const registrations = await navigator.serviceWorker.getRegistrations();
    await Promise.all(registrations.map((registration) => registration.unregister()));

    if ("caches" in window) {
      const cacheKeys = await caches.keys();
      await Promise.all(cacheKeys.map((key) => caches.delete(key)));
    }

    return;
  }

  await navigator.serviceWorker.register("./service-worker.js");
}

function normalizeHashRoute(rawHash = "") {
  return String(rawHash).replace(/^#/, "").replace(/^\/+/, "").trim();
}

function createAdminShellContext() {
  return {
    brand: { name: "LIOCONNECTA", tagline: "" },
    user: { greeting: "", name: "", area: "", notificationCount: 0 },
    navItems: getNavRoutes().map((item) => ({ ...item, active: false, moduleKey: "" })),
    hero: { title: "", subtitle: "" },
    mood: { title: "", items: [] },
    composer: { enabled: false, title: "", placeholder: "", actions: [] },
    leftPanels: [],
    rightPanels: []
  };
}

async function loadShellContext() {
  const config = getRuntimeConfig();
  const hasPortalSession = Boolean(getStoredPortalSession()?.token);

  if (config.dataMode === DATA_MODES.API && !hasPortalSession) {
    return createAdminShellContext();
  }

  const [userContext, panels] = await Promise.all([
    getUserHomeContext(),
    getPanelData()
  ]);

  return {
    ...userContext,
    ...panels
  };
}

function parseRoute() {
  const hash = normalizeHashRoute(window.location.hash);

  if (!hash) {
    return { route: ROUTES.HOME, slug: "" };
  }

  if (hash.startsWith(`${ROUTES.COMMUNICATION_READ}/`)) {
    return {
      route: ROUTES.COMMUNICATION_READ,
      slug: hash.slice(`${ROUTES.COMMUNICATION_READ}/`.length)
    };
  }

  if (hash.startsWith(`${ROUTES.POLL_READ}/`)) {
    return {
      route: ROUTES.POLL_READ,
      slug: hash.slice(`${ROUTES.POLL_READ}/`.length)
    };
  }

  if (hash === ROUTES.COMMUNICATION_ADMIN) {
    return { route: ROUTES.COMMUNICATION_ADMIN, slug: "" };
  }

  if (hash === ROUTES.ADMIN_USERS) {
    return { route: ROUTES.ADMIN_USERS, slug: "" };
  }

  if (hash === ROUTES.ADMIN_POLLS) {
    return { route: ROUTES.ADMIN_POLLS, slug: "" };
  }

  if (hash === ROUTES.SETTINGS_LDAP) {
    return { route: ROUTES.SETTINGS_LDAP, slug: "" };
  }

  if (hash === ROUTES.SETTINGS_MICROSOFT_GRAPH) {
    return { route: ROUTES.SETTINGS_MICROSOFT_GRAPH, slug: "" };
  }

  if (hash === ROUTES.SAVED) {
    return { route: ROUTES.SAVED, slug: "" };
  }

  if (hash.startsWith(`${ROUTES.HR_PROFILE}/`)) {
    const moduleSlug = hash.slice(`${ROUTES.HR_PROFILE}/`.length);
    if (isHrProfileModuleSlug(moduleSlug)) {
      return { route: ROUTES.HR_PROFILE, slug: moduleSlug };
    }
  }

  if (hash.startsWith(`${ROUTES.JOURNEY}/`)) {
    const moduleSlug = hash.slice(`${ROUTES.JOURNEY}/`.length);
    if (isJourneyModuleSlug(moduleSlug)) {
      return { route: ROUTES.JOURNEY, slug: moduleSlug };
    }
  }

  if (getNavRoutes().some((item) => item.route === hash)) {
    return { route: hash, slug: "" };
  }

  return { route: ROUTES.HOME, slug: "" };
}

function buildNavItems(navItems = [], route = ROUTES.HOME) {
  const activeRoute = route === ROUTES.COMMUNICATION_READ
    ? ROUTES.COMMUNICATIONS
    : route === ROUTES.POLL_READ
      ? ROUTES.POLLS
      : route === ROUTES.SAVED || route === ROUTES.HR_PROFILE || route === ROUTES.JOURNEY
        ? ROUTES.HOME
        : route === ROUTES.ADMIN_POLLS || route === ROUTES.COMMUNICATION_ADMIN
        ? ROUTES.PEOPLE
        : route === ROUTES.SETTINGS_LDAP || route === ROUTES.SETTINGS_MICROSOFT_GRAPH
          ? ROUTES.SETTINGS
          : route;
  const routes = navItems.length && navItems[0]?.route
    ? navItems
    : getNavRoutes();

  return routes.map((item) => ({
    label: item.label,
    href: `#${item.route}`,
    active: item.route === activeRoute
  }));
}

function renderShell(data, route) {
  const header = document.getElementById("page-header");
  const leftSidebar = document.getElementById("left-sidebar");
  const rightSidebar = document.getElementById("right-sidebar");
  const hideDefaultSidebars = isSidebarlessRoute(route);

  currentShellNavItems = Array.isArray(data.navItems) ? data.navItems : [];
  applyLayoutMode(route);

  header.innerHTML = renderHeaderShell({
    ...data,
    navItems: buildNavItems(data.navItems, route)
  });
  leftSidebar.innerHTML = hideDefaultSidebars ? "" : renderSidebarPanels(data.leftPanels);
  rightSidebar.innerHTML = hideDefaultSidebars ? "" : renderSidebarPanels(data.rightPanels);
  setAgendaEvents(
    data.agenda?.items?.length
      ? data.agenda.items
      : collectAgendaEventsFromPanels([...(data.leftPanels || []), ...(data.rightPanels || [])])
  );
  bindAgendaEventModalActions();
  bindPortalTopbarActions();
}

function renderHomePage(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);

  const composer = {
    ...data.composer,
    enabled: data.composer?.enabled !== false && canInteractWithFeed(),
    photoEnabled: canInteractWithFeed() && getRuntimeConfig().dataMode === DATA_MODES.API
  };
  const currentUserId = String(getStoredPortalSession()?.user?.id || "");

  centerContent.innerHTML = [
    renderHero(data.hero),
    renderMoodCard(data.mood),
    renderHomePollCarousel(data.pollHomeCarousel),
    renderCarouselSection(data.carousel),
    renderFeed(data.feed, composer, { currentUserId })
  ].join("");

  initCarousel();
  initPollHomeCarousel();
  bindPublicPollActions(route);
  bindFeedLikeActions();
  bindFeedShareActions();
  bindFeedSaveActions();
  bindFeedPostMenuActions();
  bindFeedPostCommentActions();
  if (composer.enabled) {
    bindFeedComposerActions();
    if (composer.photoEnabled) {
      bindFeedPhotoComposerActions();
    }
  }
}

function renderSavedFeedPage(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  const currentUserId = String(getStoredPortalSession()?.user?.id || "");

  centerContent.innerHTML = renderSavedFeed(data.feed, { currentUserId });

  bindFeedLikeActions();
  bindFeedShareActions();
  bindFeedSaveActions();
  bindFeedPostMenuActions(document, { savedList: true });
}

function renderHrProfilePage(data, route, slug) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = renderHrProfileModulePage(slug, data.hrModule);
}

function renderJourneyPage(data, route, slug) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = renderJourneyModulePage(slug, data.journeyModule);
}

function renderCommunicationsPage(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = renderCommunicationsHub(data.communications);
}

function renderPollsPage(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = renderPollsHub(data.polls, {
    canManage: canManagePolls()
  });
  bindPublicPollActions(route);
}

function renderCommunicationReadPage(data, route, slug) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);

  const allCommunications = [...(data.communications.items || [])];
  const currentCommunication = allCommunications.find((item) => item?.slug === slug);

  centerContent.innerHTML = renderCommunicationDetailPage(currentCommunication);
  bindFeedLikeActions();
  bindFeedSaveActions();
}

function renderPollReadPage(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = renderPollDetailPage(data.pollDetail);
  bindPublicPollActions(route);
}

function renderCommunicationAdminCurrentView() {
  const centerContent = document.getElementById("center-content");
  const selectedCommunication = currentCommunicationsPage?.items?.find(
    (item) => item?.id === currentCommunicationEditingId
  ) || null;

  const pageContent = !canManageCommunications()
    ? renderEmptyState(
      "Acesso restrito ao RH",
      "A gestao de comunicados e exclusiva para Gestores de RH. Se o perfil foi alterado recentemente, encerre a sessao e faca login novamente."
    )
    : renderCommunicationAdminPage(currentCommunicationsPage, {
      layout: "rh",
      selectedCommunication
    });

  centerContent.innerHTML = wrapRhAdminShell(pageContent, "comunicados");

  if (canManageCommunications()) {
    bindCommunicationAdminActions();
    if (commWizardAutoOpen) {
      openCommunicationAdminWizard(document);
      commWizardAutoOpen = false;
    }
  }
}

function buildCommunicationWizardPayload(values) {
  const publishedValue = values.publishedAt || "";
  return {
    title: values.title,
    category: values.category,
    priority: values.priority,
    summary: values.summary,
    body: values.body,
    publishedAt: publishedValue ? new Date(`${publishedValue}T09:00:00`).toISOString() : new Date().toISOString(),
    audience: values.audience,
    channel: values.channel,
    status: values.status,
    owner: values.owner,
    attachmentLabel: values.attachmentLabel,
    imageUrl: values.imageUrl || null,
    isFeatured: values.isFeatured
  };
}

function readCommunicationImageFile(file) {
  return new Promise((resolve) => {
    if (!file) {
      resolve("");
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      resolve(typeof reader.result === "string" ? reader.result : "");
    };
    reader.onerror = () => resolve("");
    reader.readAsDataURL(file);
  });
}

async function refreshCommunicationAdminRoute(feedbackMessage = "", feedbackTone = "success") {
  if (parseRoute().route !== ROUTES.COMMUNICATION_ADMIN) {
    return;
  }

  const data = await loadPageData(ROUTES.COMMUNICATION_ADMIN);
  currentCommunicationsPage = data.communications;
  renderCommunicationAdminCurrentView();

  if (feedbackMessage) {
    showToast(feedbackMessage, feedbackTone);
  }
}

function bindCommunicationAdminActions() {
  document.querySelectorAll("[data-action='admin-communication-create']").forEach((button) => {
    button.addEventListener("click", () => {
      currentCommunicationEditingId = "";
      commWizardAutoOpen = true;
      renderCommunicationAdminCurrentView();
    });
  });

  document.querySelectorAll("[data-action='admin-communication-edit']").forEach((button) => {
    button.addEventListener("click", () => {
      currentCommunicationEditingId = button.getAttribute("data-communication-id") || "";
      commWizardAutoOpen = true;
      renderCommunicationAdminCurrentView();
    });
  });

  document.querySelectorAll("[data-action='admin-communication-archive'], [data-action='admin-communication-reactivate']").forEach((button) => {
    button.addEventListener("click", async () => {
      const communicationId = button.getAttribute("data-communication-id") || "";
      const nextStatus = button.getAttribute("data-next-status") || "";
      const item = currentCommunicationsPage?.items?.find((entry) => entry.id === communicationId);
      if (!communicationId || !nextStatus || !item) {
        return;
      }

      try {
        await updateCommunication(communicationId, {
          title: item.title,
          category: item.category,
          priority: item.priority,
          summary: item.summary,
          body: item.bodyText || item.body?.join?.("\n\n") || "",
          publishedAt: item.publishedAtRaw || new Date().toISOString(),
          audience: item.audience,
          channel: item.channel,
          status: nextStatus,
          owner: item.owner,
          attachmentLabel: item.attachmentLabel,
          imageUrl: item.imageUrl || item.image || null,
          isFeatured: item.isFeatured
        }, { headers: getCommunicationEditorHeaders() });

        await refreshCommunicationAdminRoute(
          nextStatus === "Arquivado" ? "Comunicado inativado com sucesso." : "Comunicado reativado com sucesso."
        );
      } catch (error) {
        console.error("Falha ao atualizar status do comunicado.", error);
        showToast("Nao foi possivel atualizar o status do comunicado.", "danger");
      }
    });
  });

  document.querySelectorAll("[data-action='admin-communication-delete']").forEach((button) => {
    button.addEventListener("click", async () => {
      const communicationId = button.getAttribute("data-communication-id") || "";
      if (!communicationId || !window.confirm("Deseja excluir este comunicado permanentemente?")) {
        return;
      }

      try {
        await deleteCommunication(communicationId, { headers: getCommunicationEditorHeaders() });
        currentCommunicationEditingId = "";
        await refreshCommunicationAdminRoute("Comunicado excluido com sucesso.");
      } catch (error) {
        console.error("Falha ao excluir comunicado.", error);
        showToast("Nao foi possivel excluir o comunicado.", "danger");
      }
    });
  });

  initCommunicationAdminWizard(document, {
    onClose: () => {
      currentCommunicationEditingId = "";
    },
    onValidation: (message) => {
      showToast(message, "info");
    },
    readImageFile: readCommunicationImageFile,
    onSubmit: async (values, mode, communicationId) => {
      const payload = buildCommunicationWizardPayload(values);
      const headers = getCommunicationEditorHeaders();

      try {
        if (mode === "edit" && communicationId) {
          await updateCommunication(communicationId, payload, { headers });
        } else {
          await createCommunication(payload, { headers });
        }

        closeCommunicationAdminWizard(document);
        currentCommunicationEditingId = "";
        await refreshCommunicationAdminRoute(
          mode === "edit" ? "Comunicado atualizado com sucesso." : "Comunicado publicado com sucesso."
        );
      } catch (error) {
        console.error("Falha ao salvar comunicado.", error);
        const message = error instanceof Error && error.message.includes("HTTP 403")
          ? "Seu perfil nao possui permissao para publicar comunicados."
          : "Nao foi possivel salvar o comunicado agora.";
        showToast(message, "danger");
      }
    }
  });
}

function renderCommunicationAdminRoute(data, route) {
  renderShell(data, route);
  currentCommunicationsPage = data.communications;

  if (currentCommunicationEditingId && !currentCommunicationsPage?.items?.some((item) => item?.id === currentCommunicationEditingId)) {
    currentCommunicationEditingId = "";
  }

  renderCommunicationAdminCurrentView();
}

function renderAdminSettingsRoute(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = renderAdminSettingsPage();
}

function renderAdminLdapRoute(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = `
    <div class="ldap-wizard-layout">
      <div class="ldap-wizard-layout__toolbar">
        <a href="#configuracoes" class="comm-secondary-button">
          <i class="fa-solid fa-arrow-left" aria-hidden="true"></i>
          Voltar para configuracoes
        </a>
      </div>
      ${renderLdapWizardPage(data.ldapSettings)}
    </div>
  `;
  initLdapWizard(centerContent);
}

function renderAdminMicrosoftGraphRoute(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  centerContent.innerHTML = `
    <div class="ldap-wizard-layout">
      <div class="ldap-wizard-layout__toolbar">
        <a href="#configuracoes" class="comm-secondary-button">
          <i class="fa-solid fa-arrow-left" aria-hidden="true"></i>
          Voltar para configuracoes
        </a>
      </div>
      ${renderMicrosoftGraphSettingsPage(data.microsoftGraphSettings)}
    </div>
  `;
  initMicrosoftGraphSettings(centerContent);
}

function renderAdminPollsCurrentView() {
  const centerContent = document.getElementById("center-content");
  const selectedPoll = currentAdminPollsPage.items.find((item) => item?.id === currentAdminPollEditingId) || null;
  const pageContent = !canManagePolls()
    ? renderEmptyState(
      "Acesso restrito ao RH",
      "A gestao de enquetes e exclusiva para Gestores de RH. Se o perfil foi alterado recentemente, encerre a sessao e faca login novamente."
    )
    : renderAdminPollsPage(currentAdminPollsPage, selectedPoll);

  centerContent.innerHTML = wrapRhAdminShell(pageContent, "enquetes");

  if (canManagePolls()) {
    bindAdminPollActions();
    if (pollWizardAutoOpen) {
      openPollAdminWizard(document);
      pollWizardAutoOpen = false;
    }
  }
}

function renderAdminPollsRoute(data, route) {
  renderShell(data, route);
  currentAdminPollsPage = {
    ...createEmptyAdminPollsPage(),
    ...(data.adminPollsPage || {})
  };

  if (currentAdminPollEditingId && !currentAdminPollsPage.items.some((item) => item?.id === currentAdminPollEditingId)) {
    currentAdminPollEditingId = "";
  }

  renderAdminPollsCurrentView();
}

function buildPollWizardPayload(values) {
  return {
    ...values,
    publishedAtUtc: normalizeDateTimeInput(values.publishedAtUtc),
    closesAtUtc: normalizeDateTimeInput(values.closesAtUtc)
  };
}

function normalizeDateTimeInput(value) {
  if (!value) {
    return null;
  }

  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : parsed.toISOString();
}

function replacePollAssetPreview(container, assetType, url) {
  const existing = container.querySelector(".poll-asset-preview");
  if (existing) {
    existing.remove();
  }

  const previewHtml = assetType === "image"
    ? `
      <div class="poll-asset-preview poll-asset-preview--image">
        <img src="${url}" alt="Imagem da enquete" loading="lazy" />
      </div>
    `
    : `
      <div class="poll-asset-preview">
        <i class="fa-solid fa-paperclip"></i>
        <span>${url}</span>
      </div>
    `;

  container.insertAdjacentHTML("beforeend", previewHtml);
}

async function uploadSelectedPollAsset(form, assetType, triggerButton) {
  const container = form.querySelector(`[data-poll-asset='${assetType}']`);
  const fileInput = container?.querySelector("input[type='file']");
  const valueInput = container?.querySelector(`input[name='${assetType === "image" ? "imageUrl" : "attachmentUrl"}']`);
  const file = fileInput?.files?.[0];

  if (!container || !fileInput || !valueInput || !file) {
    showToast("Selecione um arquivo antes de enviar.", "info");
    return;
  }

  const originalLabel = triggerButton.textContent;
  triggerButton.disabled = true;
  triggerButton.textContent = "Enviando...";

  try {
    const response = await uploadPollAsset(assetType, file, {
      headers: getPortalAuthHeaders()
    });

    valueInput.value = response?.url || "";
    replacePollAssetPreview(container, assetType, response?.url || "");
    fileInput.value = "";
    showToast(assetType === "image" ? "Imagem enviada com sucesso." : "Anexo enviado com sucesso.", "success");
  } catch (error) {
    console.error("Falha ao enviar ativo da enquete.", error);
    showToast(assetType === "image" ? "Nao foi possivel enviar a imagem." : "Nao foi possivel enviar o anexo.", "danger");
  } finally {
    triggerButton.disabled = false;
    triggerButton.textContent = originalLabel;
  }
}

async function refreshAdminPollsRoute(feedbackMessage = "", feedbackTone = "success", selectedPollId = currentAdminPollEditingId) {
  if (parseRoute().route !== ROUTES.ADMIN_POLLS) {
    return;
  }

  const data = await loadPageData(ROUTES.ADMIN_POLLS);
  currentAdminPollEditingId = selectedPollId || "";
  renderAdminPollsRoute(data, ROUTES.ADMIN_POLLS);

  if (feedbackMessage) {
    showToast(feedbackMessage, feedbackTone);
  }
}

const feedComposerMentions = new WeakMap();

function bindFeedComposerActions() {
  const forms = Array.from(document.querySelectorAll("[data-action='submit-feed-post']"));

  forms.forEach((form) => {
    if (form.dataset.bound === "true") {
      return;
    }

    form.dataset.bound = "true";

    const editor = form.querySelector(".feed-mention-editor");
    const fieldRoot = form.querySelector(".feed-composer-mention-field");
    if (editor && fieldRoot) {
      feedComposerMentions.set(form, bindMentionField({ fieldRoot, editor }));
    }

    form.addEventListener("submit", async (event) => {
      event.preventDefault();

      const mentionControl = feedComposerMentions.get(form);
      const text = String(mentionControl?.getText() || "").trim();
      const pendingPhotos = getPendingFeedPhotos();
      const submitButton = form.querySelector(".feed-composer-submit");

      if (!text && !pendingPhotos.length) {
        showToast("Escreva algo ou adicione ao menos uma foto antes de publicar.", "danger");
        return;
      }

      if (submitButton) {
        submitButton.disabled = true;
      }

      try {
        const headers = getPortalAuthHeaders();
        const media = [];

        for (const photo of pendingPhotos) {
          const upload = await uploadFeedAsset(
            new File([photo.blob], photo.fileName || "feed-photo.jpg", { type: photo.blob.type || "image/jpeg" }),
            { headers }
          );

          media.push({
            url: String(upload?.url || ""),
            description: String(photo.description || ""),
            aspectRatio: String(photo.aspectRatio || "free")
          });
        }

        await createFeedPost({
          text,
          media,
          mentionedUserIds: feedComposerMentions.get(form)?.getMentionedUserIds() ?? []
        }, { headers });
        feedComposerMentions.get(form)?.resetMentions();
        clearPendingFeedPhotos();
        showToast("Publicacao enviada ao feed.", "success");
        await renderCurrentRoute();
      } catch (error) {
        console.error("Falha ao publicar no feed.", error);
        const message = error instanceof Error && error.message.includes("HTTP 401")
          ? "Sua sessao expirou. Faca login novamente para publicar no feed."
          : error instanceof Error && error.message.includes("HTTP 400")
            ? "Nao foi possivel publicar. Verifique o texto e as fotos informados."
            : "Nao foi possivel publicar no feed agora.";

        showToast(message, "danger");

        if (error instanceof Error && error.message.includes("HTTP 401")) {
          window.setTimeout(() => {
            redirectToPortalLogin(window.location.hash || "#inicio");
          }, 700);
        }
      } finally {
        if (submitButton) {
          submitButton.disabled = false;
        }
      }
    });
  });
}

function bindFeedLikeActions() {
  const buttons = Array.from(document.querySelectorAll("[data-action='toggle-feed-like'], [data-action='toggle-communication-like']"));

  buttons.forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", async () => {
      const itemId = button.getAttribute("data-feed-item-id")
        || button.getAttribute("data-communication-id")
        || "";
      const source = button.getAttribute("data-feed-source")
        || (button.getAttribute("data-communication-id") ? "Communication" : "");
      const scope = button.closest(".post, .communication-detail-card");

      if (!itemId || !source) {
        showToast("Esta publicacao ainda nao esta disponivel para curtidas.", "info");
        return;
      }

      if (button.disabled) {
        return;
      }

      button.disabled = true;

      try {
        const result = await toggleFeedLike(itemId, source, {
          headers: getPortalAuthHeaders()
        });
        updateFeedLikeUi(scope, result);
        showToast(result.hasLiked ? "Curtida registrada." : "Curtida removida.", "success");
      } catch (error) {
        console.error("Falha ao registrar curtida.", error);
        const message = error instanceof Error && error.message.includes("HTTP 401")
          ? "Sua sessao expirou. Faca login novamente para curtir publicacoes."
          : "Nao foi possivel registrar a curtida nesta publicacao.";

        showToast(message, "danger");

        if (error instanceof Error && error.message.includes("HTTP 401")) {
          window.setTimeout(() => {
            redirectToPortalLogin(window.location.hash || "#inicio");
          }, 700);
        }
      } finally {
        button.disabled = false;
      }
    });
  });
}

function bindFeedShareActions() {
  const buttons = Array.from(document.querySelectorAll("[data-action='toggle-feed-share']"));

  buttons.forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", async () => {
      const itemId = button.getAttribute("data-feed-item-id") || "";
      const source = button.getAttribute("data-feed-source") || "";
      const scope = button.closest(".post");

      if (!itemId || !source) {
        showToast("Esta publicacao ainda nao esta disponivel para compartilhamento.", "info");
        return;
      }

      if (button.disabled) {
        return;
      }

      button.disabled = true;

      try {
        const result = await toggleFeedShare(itemId, source, {
          headers: getPortalAuthHeaders()
        });
        updateFeedShareUi(scope, result);
        showToast(result.hasShared ? "Compartilhamento registrado." : "Compartilhamento removido.", "success");
      } catch (error) {
        console.error("Falha ao registrar compartilhamento.", error);
        const message = error instanceof Error && error.message.includes("HTTP 401")
          ? "Sua sessao expirou. Faca login novamente para compartilhar publicacoes."
          : error instanceof Error && error.message.includes("HTTP 403")
            ? "Voce so pode compartilhar publicacoes de outros colaboradores."
            : "Nao foi possivel registrar o compartilhamento nesta publicacao.";

        showToast(message, "danger");

        if (error instanceof Error && error.message.includes("HTTP 401")) {
          window.setTimeout(() => {
            redirectToPortalLogin(window.location.hash || "#inicio");
          }, 700);
        }
      } finally {
        button.disabled = false;
      }
    });
  });
}

function bindFeedSaveActions() {
  const buttons = Array.from(document.querySelectorAll("[data-action='toggle-feed-save']"));

  buttons.forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", async () => {
      const itemId = button.getAttribute("data-feed-item-id") || "";
      const source = button.getAttribute("data-feed-source") || "";
      const scope = button.closest(".post, .communication-detail-card");

      if (!itemId || !source) {
        showToast("Esta publicacao ainda nao esta disponivel para salvamento.", "info");
        return;
      }

      if (button.disabled) {
        return;
      }

      button.disabled = true;

      try {
        const result = await toggleFeedSave(itemId, source, {
          headers: getPortalAuthHeaders()
        });
        updateFeedSaveUi(scope, result);
        showToast(result.hasSaved ? "Item salvo com sucesso." : "Item removido dos salvos.", "success");
      } catch (error) {
        console.error("Falha ao salvar publicacao.", error);
        const message = error instanceof Error && error.message.includes("HTTP 401")
          ? "Sua sessao expirou. Faca login novamente para salvar publicacoes."
          : "Nao foi possivel atualizar os itens salvos agora.";
        showToast(message, "danger");

        if (error instanceof Error && error.message.includes("HTTP 401")) {
          window.setTimeout(() => {
            redirectToPortalLogin(window.location.hash || "#inicio");
          }, 700);
        }
      } finally {
        button.disabled = false;
      }
    });
  });
}

let feedPostMenuOutsideClickBound = false;

function closeOpenPostMenus(exceptMenu = null) {
  document.querySelectorAll(".post-more-menu").forEach((menu) => {
    if (exceptMenu && menu === exceptMenu) {
      return;
    }

    const dropdown = menu.querySelector(".post-more-dropdown");
    const trigger = menu.querySelector(".post-more-trigger");
    if (dropdown) {
      dropdown.hidden = true;
    }
    if (trigger) {
      trigger.setAttribute("aria-expanded", "false");
    }
  });
}

function bindFeedPostMenuActions(root = document, { savedList = false } = {}) {
  if (getRuntimeConfig().dataMode !== DATA_MODES.API) {
    return;
  }

  if (savedList) {
    if (!getStoredPortalSession()?.token) {
      return;
    }
  } else if (!canInteractWithFeed()) {
    return;
  }

  if (!feedPostMenuOutsideClickBound) {
    feedPostMenuOutsideClickBound = true;
    document.addEventListener("click", (event) => {
      if (event.target.closest(".post-more-menu")) {
        return;
      }
      closeOpenPostMenus();
    });
  }

  const menus = Array.from(root.querySelectorAll(".post-more-menu"));
  menus.forEach((menu) => {
    const trigger = menu.querySelector("[data-action='toggle-post-menu']");
    const dropdown = menu.querySelector(".post-more-dropdown");
    if (!trigger || !dropdown || trigger.dataset.bound === "true") {
      return;
    }

    trigger.dataset.bound = "true";
    trigger.addEventListener("click", (event) => {
      event.preventDefault();
      event.stopPropagation();
      const willOpen = dropdown.hidden;
      closeOpenPostMenus(menu);
      dropdown.hidden = !willOpen;
      trigger.setAttribute("aria-expanded", willOpen ? "true" : "false");
    });
  });

  if (!savedList) {
    const deleteButtons = Array.from(root.querySelectorAll("[data-action='delete-feed-post']"));
    deleteButtons.forEach((button) => {
      if (button.dataset.bound === "true") {
        return;
      }

      button.dataset.bound = "true";
      button.addEventListener("click", async (event) => {
        event.preventDefault();
        event.stopPropagation();

        const postId = button.getAttribute("data-post-id") || "";
        const postEl = button.closest(".post");
        if (!postId || !postEl) {
          return;
        }

        const confirmed = await confirmAction({
          title: "Excluir publicacao?",
          text: "Ela deixara de aparecer no feed.",
          confirmButtonText: "Excluir",
          cancelButtonText: "Cancelar",
          icon: "warning",
          tone: "danger"
        });
        if (!confirmed) {
          return;
        }

        closeOpenPostMenus();
        button.disabled = true;

        try {
          await deleteFeedPost(postId, { headers: getPortalAuthHeaders() });
          postEl.remove();

          const feedList = document.querySelector(".feed-list");
          if (feedList && !feedList.querySelector(".post")) {
            feedList.innerHTML = renderEmptyState(
              "Ainda não há posts publicados.",
              "Assim que a comunicação interna ou os times compartilharem novidades, o mural aparecerá aqui."
            );
          }

          showToast("Publicacao removida do feed.", "success");
        } catch (error) {
          console.error("Falha ao excluir publicacao do feed.", error);
          const message = error instanceof Error && error.message.includes("HTTP 403")
            ? "Voce so pode excluir suas proprias publicacoes."
            : error instanceof Error && error.message.includes("HTTP 401")
              ? "Sua sessao expirou. Faca login novamente para excluir a publicacao."
              : "Nao foi possivel excluir a publicacao agora.";
          showToast(message, "error");
          button.disabled = false;
        }
      });
    });
  }

  if (savedList) {
    const removeButtons = Array.from(root.querySelectorAll("[data-action='remove-saved-feed-item']"));
    removeButtons.forEach((button) => {
      if (button.dataset.bound === "true") {
        return;
      }

      button.dataset.bound = "true";
      button.addEventListener("click", async (event) => {
        event.preventDefault();
        event.stopPropagation();

        const itemId = button.getAttribute("data-feed-item-id") || "";
        const source = button.getAttribute("data-feed-source") || "";
        const postEl = button.closest(".post");
        if (!itemId || !source || !postEl) {
          return;
        }

        closeOpenPostMenus();
        button.disabled = true;

        try {
          const result = await toggleFeedSave(itemId, source, {
            headers: getPortalAuthHeaders()
          });

          if (!result.hasSaved) {
            postEl.remove();
            const feedList = document.querySelector(".feed-list");
            if (feedList && !feedList.querySelector(".post")) {
              feedList.innerHTML = renderEmptyState(
                "Nenhum item salvo ainda.",
                "Use o botao Salvar em posts e comunicados para acompanhar o que importa para voce."
              );
            }
          }

          showToast("Item removido dos salvos.", "success");
        } catch (error) {
          console.error("Falha ao remover item dos salvos.", error);
          const message = error instanceof Error && error.message.includes("HTTP 401")
            ? "Sua sessao expirou. Faca login novamente."
            : "Nao foi possivel remover o item dos salvos agora.";
          showToast(message, "danger");
          button.disabled = false;
        }
      });
    });
  }
}

function bindPublicPollActions(route) {
  const forms = Array.from(document.querySelectorAll("[data-action='submit-poll-vote']"));

  forms.forEach((form) => {
    if (form.dataset.bound === "true") {
      return;
    }

    form.dataset.bound = "true";
    form.addEventListener("submit", async (event) => {
      event.preventDefault();

      const inputs = Array.from(form.querySelectorAll("input:checked"));
      const optionIds = inputs.map((input) => input.value).filter(Boolean);
      const pollId = form.getAttribute("data-poll-id") || "";

      if (!pollId || optionIds.length === 0) {
        showToast("Selecione ao menos uma opcao para registrar seu voto.", "danger");
        return;
      }

      try {
        const updatedPoll = await votePoll(pollId, optionIds, {
          headers: getPortalAuthHeaders()
        });
        showToast("Voto registrado com sucesso.", "success");

        const homeSlide = form.closest(".poll-home-carousel__slide");
        if (homeSlide) {
          updateHomePollSlideAfterVote(homeSlide, updatedPoll);
          return;
        }

        await renderCurrentRoute();
      } catch (error) {
        console.error("Falha ao registrar voto na enquete.", error);
        showToast("Nao foi possivel registrar o voto nesta enquete.", "danger");
      }
    });
  });
}

function bindAdminPollActions() {
  document.querySelectorAll("[data-action='admin-poll-create']").forEach((button) => {
    button.addEventListener("click", () => {
      currentAdminPollEditingId = "";
      pollWizardAutoOpen = true;
      renderAdminPollsCurrentView();
    });
  });

  document.querySelectorAll("[data-action='admin-poll-edit']").forEach((button) => {
    button.addEventListener("click", () => {
      currentAdminPollEditingId = button.getAttribute("data-poll-id") || "";
      pollWizardAutoOpen = true;
      renderAdminPollsCurrentView();
    });
  });

  document.querySelectorAll("[data-action='admin-poll-status']").forEach((button) => {
    button.addEventListener("click", async () => {
      const pollId = button.getAttribute("data-poll-id") || "";
      const nextStatus = button.getAttribute("data-next-status") || "";

      if (!pollId || !nextStatus) {
        return;
      }

      try {
        await updatePollStatus(pollId, nextStatus, {
          headers: getPortalAuthHeaders()
        });
        await refreshAdminPollsRoute("Status da enquete atualizado com sucesso.", "success", currentAdminPollEditingId || pollId);
      } catch (error) {
        console.error("Falha ao atualizar status da enquete.", error);
        showToast("Nao foi possivel atualizar o status da enquete.", "danger");
      }
    });
  });

  initPollAdminWizard(document, {
    onClose: () => {
      currentAdminPollEditingId = "";
    },
    onValidation: (message) => {
      showToast(message, "info");
    },
    onUploadAsset: uploadSelectedPollAsset,
    onSubmit: async (values, mode, pollId) => {
      const payload = buildPollWizardPayload(values);

      try {
        await (mode === "edit" && pollId
          ? updatePoll(pollId, payload, { headers: getPortalAuthHeaders() })
          : createPoll(payload, { headers: getPortalAuthHeaders() }));

        closePollAdminWizard(document);
        currentAdminPollEditingId = "";
        await refreshAdminPollsRoute(
          mode === "edit" ? "Enquete atualizada com sucesso." : "Enquete criada com sucesso.",
          "success",
          ""
        );
      } catch (error) {
        console.error("Falha ao salvar enquete administrativa.", error);
        showToast("Nao foi possivel salvar a enquete.", "danger");
      }
    }
  });
}

async function refreshAdminUsersRoute(feedbackMessage = "", feedbackTone = "success") {
  if (parseRoute().route !== ROUTES.ADMIN_USERS) {
    return;
  }

  const data = await loadPageData(ROUTES.ADMIN_USERS);
  renderAdminUsersRoute(data, ROUTES.ADMIN_USERS);

  if (feedbackMessage) {
    showToast(feedbackMessage, feedbackTone);
  }
}

function renderAdminUsersDynamicSections(loadError = "") {
  const kpisHost = document.getElementById("admin-users-kpis-host");
  const resultsHost = document.getElementById("admin-users-results-host");
  const activityHost = document.getElementById("admin-users-activity-host");

  if (kpisHost) {
    kpisHost.innerHTML = renderAdminUsersKpiSection(currentAdminUsersPage.summary || createEmptyPortalUsersPage().summary);
  }

  if (resultsHost) {
    resultsHost.innerHTML = renderAdminUsersResultsSection(currentAdminUsersPage, loadError);
  }

  if (activityHost) {
    activityHost.innerHTML = renderAdminUsersActivitySection(currentAdminUsersPage);
  }
}

async function refreshAdminUsersDataOnly({
  feedbackMessage = "",
  feedbackTone = "success",
  preserveModalUserId = "",
  preserveModalMode = "edit"
} = {}) {
  const portalUsersPage = await listPortalUsers(buildAdminUsersQuery(), {
    headers: getAdminAuthHeaders()
  });

  currentAdminUsersPage = {
    ...createEmptyPortalUsersPage(),
    ...portalUsersPage
  };

  adminUsersQueryState = {
    ...adminUsersQueryState,
    query: portalUsersPage.query ?? "",
    status: portalUsersPage.status || "all",
    role: portalUsersPage.role || "all",
    department: portalUsersPage.department || "all",
    sortBy: portalUsersPage.sortBy || "displayName",
    sortDirection: portalUsersPage.sortDirection || "asc",
    page: portalUsersPage.page || adminUsersQueryState.page,
    pageSize: portalUsersPage.pageSize || adminUsersQueryState.pageSize
  };

  renderAdminUsersDynamicSections("");
  bindAdminUsersFilters();
  bindAdminUsersModal();

  if (preserveModalUserId) {
    const stillExists = currentAdminUsersPage.items.some((item) => item?.id === preserveModalUserId);
    if (stillExists) {
      openPortalUserModal(preserveModalUserId, preserveModalMode);
    } else {
      closePortalUserModal();
    }
  }

  if (feedbackMessage) {
    showToast(feedbackMessage, feedbackTone);
  }
}

function bindAdminUsersFilters() {
  const searchInput = document.getElementById("admin-user-search");
  const statusFilter = document.getElementById("admin-user-status-filter");
  const roleFilter = document.getElementById("admin-user-role-filter");
  const departmentFilter = document.getElementById("admin-user-department-filter");
  const pageButtons = Array.from(document.querySelectorAll("[data-action='admin-users-page']"));
  const sortButtons = Array.from(document.querySelectorAll("[data-action='admin-users-sort']"));

  if (!searchInput || !statusFilter || !roleFilter || !departmentFilter) {
    return;
  }

  if (searchInput.dataset.bound !== "true") {
    searchInput.dataset.bound = "true";
    searchInput.addEventListener("input", () => {
      window.clearTimeout(adminUsersSearchDebounce);
      adminUsersSearchDebounce = window.setTimeout(() => {
        adminUsersQueryState = {
          ...adminUsersQueryState,
          query: searchInput.value.trim(),
          page: 1
        };

        refreshAdminUsersRoute().catch((error) => {
          console.error("Falha ao pesquisar usuarios administrativos.", error);
          showToast("Nao foi possivel atualizar a busca de usuarios.", "danger");
        });
      }, 260);
    });
  }

  if (statusFilter.dataset.bound !== "true") {
    statusFilter.dataset.bound = "true";
    statusFilter.addEventListener("change", () => {
      adminUsersQueryState = {
        ...adminUsersQueryState,
        status: statusFilter.value,
        page: 1
      };

      refreshAdminUsersRoute().catch((error) => {
        console.error("Falha ao filtrar usuarios por status.", error);
        showToast("Nao foi possivel aplicar o filtro de status.", "danger");
      });
    });
  }

  if (roleFilter.dataset.bound !== "true") {
    roleFilter.dataset.bound = "true";
    roleFilter.addEventListener("change", () => {
      adminUsersQueryState = {
        ...adminUsersQueryState,
        role: roleFilter.value,
        page: 1
      };

      refreshAdminUsersRoute().catch((error) => {
        console.error("Falha ao filtrar usuarios por perfil.", error);
        showToast("Nao foi possivel aplicar o filtro de perfil.", "danger");
      });
    });
  }

  if (departmentFilter.dataset.bound !== "true") {
    departmentFilter.dataset.bound = "true";
    departmentFilter.addEventListener("change", () => {
      adminUsersQueryState = {
        ...adminUsersQueryState,
        department: departmentFilter.value,
        page: 1
      };

      refreshAdminUsersRoute().catch((error) => {
        console.error("Falha ao filtrar usuarios por departamento.", error);
        showToast("Nao foi possivel aplicar o filtro de departamento.", "danger");
      });
    });
  }

  pageButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const nextPage = Number(button.getAttribute("data-page") || adminUsersQueryState.page);
      if (!nextPage || Number.isNaN(nextPage) || nextPage < 1 || nextPage === adminUsersQueryState.page) {
        return;
      }

      adminUsersQueryState = {
        ...adminUsersQueryState,
        page: nextPage
      };

      refreshAdminUsersRoute().catch((error) => {
        console.error("Falha ao paginar usuarios administrativos.", error);
        showToast("Nao foi possivel trocar de pagina.", "danger");
      });
    });
  });

  sortButtons.forEach((button) => {
    button.addEventListener("click", () => {
      adminUsersQueryState = {
        ...adminUsersQueryState,
        sortBy: button.getAttribute("data-sort-by") || "displayName",
        sortDirection: button.getAttribute("data-sort-direction") || "asc",
        page: 1
      };

      refreshAdminUsersRoute().catch((error) => {
        console.error("Falha ao ordenar usuarios administrativos.", error);
        showToast("Nao foi possivel aplicar a ordenacao.", "danger");
      });
    });
  });
}

function closePortalUserModal() {
  const modal = document.getElementById("portal-user-modal");
  const body = document.getElementById("portal-user-modal-body");

  if (!modal || !body) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  body.innerHTML = "";
  document.body.classList.remove("modal-open");
}

function openPortalUserModal(userId, mode = "view") {
  const modal = document.getElementById("portal-user-modal");
  const body = document.getElementById("portal-user-modal-body");

  if (!modal || !body) {
    return;
  }

  const pageViewModel = {
    ...createEmptyPortalUsersPage(),
    ...(currentAdminUsersPage || {})
  };
  const items = Array.isArray(pageViewModel.items) ? pageViewModel.items : [];
  const roleOptions = Array.isArray(pageViewModel.roleOptions) ? pageViewModel.roleOptions : [];
  const accessLevelOptions = Array.isArray(pageViewModel.accessLevelOptions) ? pageViewModel.accessLevelOptions : [];
  const selectedUser = items.find((item) => item?.id === userId);

  if (!selectedUser) {
    showToast("Nao foi possivel localizar o usuario selecionado.", "danger");
    return;
  }

  body.innerHTML = renderPortalUserModal(selectedUser, roleOptions, accessLevelOptions, mode);
  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  document.body.classList.add("modal-open");
}

function bindAdminUsersModal() {
  const centerContent = document.getElementById("center-content");
  const modal = document.getElementById("portal-user-modal");

  if (!centerContent || !modal) {
    return;
  }

  centerContent.querySelectorAll("[data-action='open-portal-user-modal']").forEach((button) => {
    button.addEventListener("click", () => {
      openPortalUserModal(
        button.getAttribute("data-user-id") || "",
        button.getAttribute("data-user-mode") || "view"
      );
    });
  });

  if (modal.dataset.bound !== "true") {
    modal.dataset.bound = "true";

    modal.addEventListener("click", (event) => {
      if (event.target === modal) {
        closePortalUserModal();
        return;
      }

      const closeButton = event.target.closest("[data-action='close-portal-user-modal']");
      if (closeButton) {
        closePortalUserModal();
        return;
      }

      const switchModeButton = event.target.closest("[data-action='portal-user-modal-switch-mode']");
      if (switchModeButton) {
        openPortalUserModal(
          switchModeButton.getAttribute("data-user-id") || "",
          switchModeButton.getAttribute("data-user-mode") || "view"
        );
      }
    });
  }
}

function renderAdminUsersRoute(data, route) {
  const centerContent = document.getElementById("center-content");
  renderShell(data, route);
  currentAdminUsersPage = {
    ...createEmptyPortalUsersPage(),
    ...(data.portalUsersPage || {})
  };
  centerContent.innerHTML = renderAdminUsersPage(currentAdminUsersPage, data.portalUsersLoadError);
  bindAdminUsersFilters();
  bindAdminUsersModal();
}

async function ensureCommunicationEditorAccess() {
  if (getStoredPortalSession()) {
    const hasPortalAccess = await ensurePortalAccess();
    return hasPortalAccess;
  }

  const adminSession = getStoredAdminSession();
  if (!adminSession) {
    redirectToAdminLogin(window.location.hash || "#comunicacao/restrita");
    return false;
  }

  try {
    const validatedSession = await fetchAdminSession();
    if (!validatedSession) {
      redirectToAdminLogin(window.location.hash || "#comunicacao/restrita");
      return false;
    }

    return true;
  } catch (error) {
    console.error("Falha ao validar sessao administrativa para comunicados.", error);
    redirectToAdminLogin(window.location.hash || "#comunicacao/restrita");
    return false;
  }
}

async function ensureRestrictedAdminAccess(route = ROUTES.SETTINGS) {
  const session = getStoredAdminSession();
  if (!session) {
    redirectToAdminLogin(`#${route}`);
    return false;
  }

  try {
    const validatedSession = await fetchAdminSession();
    if (!validatedSession) {
      redirectToAdminLogin(`#${route}`);
      return false;
    }

    if ((route === ROUTES.SETTINGS || route === ROUTES.SETTINGS_LDAP || route === ROUTES.SETTINGS_MICROSOFT_GRAPH || route === ROUTES.ADMIN_USERS) && !isSuperAdminSession(validatedSession)) {
      window.location.hash = "#comunicacao/restrita";
      showToast("Esta area e restrita ao super-admin.", "danger");
      return false;
    }

    return true;
  } catch (error) {
    console.error("Falha ao validar sessao administrativa.", error);
    redirectToAdminLogin(`#${route}`);
    return false;
  }
}

async function ensurePortalAccess() {
  const session = getStoredPortalSession();
  if (!session) {
    redirectToPortalLogin(window.location.hash || "#inicio");
    return false;
  }

  try {
    const validatedSession = await ensureValidPortalSession();
    if (!validatedSession) {
      redirectToPortalLogin(window.location.hash || "#inicio");
      return false;
    }

    return true;
  } catch (error) {
    console.error("Falha ao validar sessao do portal.", error);
    if (getStoredPortalSession()) {
      return true;
    }

    redirectToPortalLogin(window.location.hash || "#inicio");
    return false;
  }
}

function buildMoodDashboardQuery() {
  const period = resolveMoodDashboardPeriod(moodDashboardQueryState.periodPreset);
  return {
    startDate: period.startDate,
    endDate: period.endDate,
    department: moodDashboardQueryState.department === "all" ? "" : moodDashboardQueryState.department
  };
}

function bindMoodDashboardFilters() {
  const periodFilter = document.getElementById("mood-dashboard-period-filter");
  const departmentFilter = document.getElementById("mood-dashboard-department-filter");

  if (periodFilter) {
    periodFilter.addEventListener("change", () => {
      moodDashboardQueryState = {
        ...moodDashboardQueryState,
        periodPreset: periodFilter.value || "7d"
      };

      renderCurrentRoute().catch((error) => {
        console.error("Falha ao atualizar dashboard de humor.", error);
        showToast("Nao foi possivel atualizar o dashboard de humor.", "danger");
      });
    });
  }

  if (departmentFilter) {
    departmentFilter.addEventListener("change", () => {
      moodDashboardQueryState = {
        ...moodDashboardQueryState,
        department: departmentFilter.value || "all"
      };

      renderCurrentRoute().catch((error) => {
        console.error("Falha ao atualizar dashboard de humor.", error);
        showToast("Nao foi possivel atualizar o dashboard de humor.", "danger");
      });
    });
  }
}

function renderPeopleRhCurrentView() {
  const centerContent = document.getElementById("center-content");
  const pageContent = !canAccessHrMoodDashboard()
    ? renderRhMoodDashboardPage(null, { accessDenied: true })
    : renderRhMoodDashboardPage(currentPeopleRhData.moodDashboard, {
      periodPreset: moodDashboardQueryState.periodPreset,
      department: moodDashboardQueryState.department,
      loadError: currentPeopleRhData.moodDashboardLoadError || "",
      feedbackPage: canManageMoodSurveyFeedback() ? currentPeopleRhData.moodFeedbackPage : null,
      feedbackLoadError: currentPeopleRhData.moodFeedbackLoadError || "",
      feedbackOptionKey: moodFeedbackQueryState.optionKey,
      feedbackEditingId: moodFeedbackQueryState.editingId
    });

  centerContent.innerHTML = wrapRhAdminShell(pageContent, "humor");
  bindMoodDashboardFilters();

  if (!canAccessHrMoodDashboard()) {
    destroyMoodDashboardCharts();
    return;
  }

  if (canManageMoodSurveyFeedback()) {
    bindMoodFeedbackAdminActions();
  }

  initMoodDashboardCharts(currentPeopleRhData.moodDashboard).catch((error) => {
    console.error("Falha ao renderizar graficos do dashboard de humor.", error);
    showToast("Nao foi possivel carregar os graficos do dashboard.", "danger");
  });
}

function readMoodFeedbackFormPayload(form) {
  const sortOrderValue = form.querySelector("[name='sortOrder']")?.value?.trim();
  const sortOrder = sortOrderValue ? Number(sortOrderValue) : null;

  return {
    optionKey: form.querySelector("[name='optionKey']")?.value || "motivated",
    message: form.querySelector("[name='message']")?.value?.trim() || "",
    sortOrder: Number.isFinite(sortOrder) && sortOrder > 0 ? sortOrder : null,
    isActive: Boolean(form.querySelector("[name='isActive']")?.checked)
  };
}

async function refreshPeopleRhRoute(feedbackMessage = "", feedbackTone = "success") {
  if (parseRoute().route !== ROUTES.PEOPLE) {
    return;
  }

  const data = await loadPageData(ROUTES.PEOPLE);
  currentPeopleRhData = {
    moodDashboard: data.moodDashboard,
    moodDashboardLoadError: data.moodDashboardLoadError || "",
    moodFeedbackPage: data.moodFeedbackPage,
    moodFeedbackLoadError: data.moodFeedbackLoadError || ""
  };
  renderPeopleRhCurrentView();

  if (feedbackMessage) {
    showToast(feedbackMessage, feedbackTone);
  }
}

function bindMoodFeedbackAdminActions() {
  const adminSection = document.getElementById("mood-feedback-admin");
  if (!adminSection || adminSection.dataset.bound === "true") {
    return;
  }

  adminSection.dataset.bound = "true";

  adminSection.addEventListener("click", async (event) => {
    const target = event.target.closest("[data-action]");
    if (!target) {
      return;
    }

    const action = target.getAttribute("data-action");

    if (action === "filter-mood-feedback") {
      event.preventDefault();
      moodFeedbackQueryState = {
        ...moodFeedbackQueryState,
        optionKey: target.getAttribute("data-option-key") || "motivated",
        editingId: ""
      };
      renderPeopleRhCurrentView();
      return;
    }

    if (action === "edit-mood-feedback") {
      event.preventDefault();
      moodFeedbackQueryState = {
        ...moodFeedbackQueryState,
        editingId: target.getAttribute("data-feedback-id") || ""
      };
      renderPeopleRhCurrentView();
      return;
    }

    if (action === "cancel-mood-feedback-edit") {
      event.preventDefault();
      moodFeedbackQueryState = {
        ...moodFeedbackQueryState,
        editingId: ""
      };
      renderPeopleRhCurrentView();
      return;
    }

    if (action === "delete-mood-feedback") {
      event.preventDefault();
      const feedbackId = target.getAttribute("data-feedback-id") || "";
      if (!feedbackId || !window.confirm("Deseja excluir esta mensagem de feedback?")) {
        return;
      }

      try {
        await deleteMoodFeedbackMessage(feedbackId);
        moodFeedbackQueryState = {
          ...moodFeedbackQueryState,
          editingId: ""
        };
        await refreshPeopleRhRoute("Mensagem excluida com sucesso.");
      } catch (error) {
        console.error("Falha ao excluir mensagem de feedback do humor.", error);
        showToast("Nao foi possivel excluir a mensagem.", "danger");
      }
    }
  });

  adminSection.addEventListener("submit", async (event) => {
    const form = event.target.closest("form[data-action]");
    if (!form) {
      return;
    }

    event.preventDefault();
    const action = form.getAttribute("data-action");
    const payload = readMoodFeedbackFormPayload(form);

    if (!payload.message) {
      showToast("Informe o texto da mensagem.", "info");
      return;
    }

    try {
      if (action === "create-mood-feedback") {
        await createMoodFeedbackMessage(payload);
        moodFeedbackQueryState = {
          optionKey: payload.optionKey,
          editingId: ""
        };
        await refreshPeopleRhRoute("Mensagem cadastrada com sucesso.");
        return;
      }

      if (action === "save-mood-feedback") {
        const feedbackId = form.getAttribute("data-feedback-id") || "";
        if (!feedbackId) {
          return;
        }

        await updateMoodFeedbackMessage(feedbackId, payload);
        moodFeedbackQueryState = {
          optionKey: payload.optionKey,
          editingId: ""
        };
        await refreshPeopleRhRoute("Mensagem atualizada com sucesso.");
      }
    } catch (error) {
      console.error("Falha ao salvar mensagem de feedback do humor.", error);
      showToast("Nao foi possivel salvar a mensagem.", "danger");
    }
  });
}

function renderPeopleRhPage(data, route) {
  renderShell(data, route);
  currentPeopleRhData = {
    moodDashboard: data.moodDashboard,
    moodDashboardLoadError: data.moodDashboardLoadError || "",
    moodFeedbackPage: data.moodFeedbackPage,
    moodFeedbackLoadError: data.moodFeedbackLoadError || ""
  };
  renderPeopleRhCurrentView();
}

function renderPlaceholderPage(data, route) {
  const centerContent = document.getElementById("center-content");
  const activeItem = buildNavItems(data.navItems, route).find((item) => item.active);
  renderShell(data, route);
  centerContent.innerHTML = `
    <section class="card">
      <div class="card-header">${activeItem?.label ?? "Modulo"}</div>
      ${renderEmptyState(
        "Modulo em estruturacao",
        "Esta area ainda sera detalhada no MVP, mas ja esta reservada na navegacao da LIOCONNECTA."
      )}
    </section>
  `;
}

function renderLoadingApp(route = parseRoute().route) {
  const header = document.getElementById("page-header");
  const leftSidebar = document.getElementById("left-sidebar");
  const centerContent = document.getElementById("center-content");
  const rightSidebar = document.getElementById("right-sidebar");

  applyLayoutMode(route);
  header.innerHTML = renderLoadingHeader();

  if (isSidebarlessRoute(route)) {
    leftSidebar.innerHTML = "";
    rightSidebar.innerHTML = "";
    centerContent.innerHTML = isRhWorkspaceRoute(route)
      ? `<div class="rh-admin-shell"><aside class="rh-admin-nav card rh-admin-nav--loading"><div class="card-header">Administrativo</div></aside><div class="rh-admin-main">${renderLoadingPanel("Carregando area de RH")}</div></div>`
      : renderLoadingPanel("Carregando area administrativa");
    return;
  }

  leftSidebar.innerHTML = [
    renderLoadingPanel("Carregando jornada"),
    renderLoadingPanel("Carregando painel"),
    renderLoadingPanel("Carregando indicadores")
  ].join("");
  rightSidebar.innerHTML = [
    renderLoadingPanel("Carregando atalhos"),
    renderLoadingPanel("Carregando perfil"),
    renderLoadingPanel("Carregando agenda")
  ].join("");
  centerContent.innerHTML = [
    renderLoadingHero(),
    renderLoadingMoodCard(),
    renderLoadingCarousel(),
    renderLoadingFeed()
  ].join("");
}

function renderBootstrapError(error) {
  console.error("Falha ao iniciar a LIOCONNECTA.", error);
  document.getElementById("center-content").innerHTML = renderErrorCard(
    "Erro ao carregar o prototipo",
    getRuntimeConfig().dataMode === DATA_MODES.API
      ? "Nao foi possivel carregar os dados da API. Verifique se a API esta ativa, se a sessao e valida e tente novamente."
      : "Nao conseguimos montar o mock tecnico neste momento. Revise os arquivos de dados, o modo ativo da aplicacao e tente novamente."
  );
}

function renderRuntimeBadge() {
  const badge = document.querySelector(".app-version-badge");
  if (!badge) {
    return;
  }

  const config = getRuntimeConfig();
  const modeLabel = config.dataMode === "mock" ? "HYBRID" : config.dataMode.toUpperCase();
  badge.textContent = `${config.version} • ${modeLabel}`;
}

async function loadPageData(route, slug = "") {
  if (route === ROUTES.HOME) {
    return getHomePageData();
  }

  if (route === ROUTES.SAVED) {
    const shellContext = await loadShellContext();
    const feed = await getSavedFeedData();

    return {
      ...shellContext,
      feed
    };
  }

  const shellContext = await loadShellContext();
  const baseShellData = {
    ...shellContext
  };
  const isAdminRoute =
    route === ROUTES.SETTINGS ||
    route === ROUTES.SETTINGS_LDAP ||
    route === ROUTES.SETTINGS_MICROSOFT_GRAPH ||
    route === ROUTES.ADMIN_USERS;
  const config = getRuntimeConfig();
  const usesApiShell = config.dataMode === "api";
  const shellData = isAdminRoute || isRhWorkspaceRoute(route) || usesApiShell
    ? baseShellData
    : applyAgendaToShellData(
        applyNotificationsToShellData(baseShellData, await getNotificationCenterData()),
        await getAgendaDayData()
      );

  if (
    route === ROUTES.COMMUNICATIONS ||
    route === ROUTES.COMMUNICATION_READ
  ) {
    const communications = await getCommunicationCenterData();
    return {
      ...shellData,
      communications
    };
  }

  if (route === ROUTES.POLLS) {
    const polls = await getPollCenterData({
      headers: getPortalAuthHeaders()
    });

    return {
      ...shellData,
      polls
    };
  }

  if (route === ROUTES.POLL_READ) {
    const pollDetail = await getPollDetailData(slug, {
      headers: getPortalAuthHeaders()
    });

    return {
      ...shellData,
      pollDetail
    };
  }

  if (route === ROUTES.HR_PROFILE) {
    const hrModule = await getHrProfileModuleData(slug, {
      headers: getPortalAuthHeaders()
    });

    return {
      ...shellData,
      hrModule,
      hrModuleSlug: slug
    };
  }

  if (route === ROUTES.JOURNEY) {
    const journeyModule = await getJourneyModuleData(slug, {
      headers: getPortalAuthHeaders()
    });

    return {
      ...shellData,
      journeyModule,
      journeyModuleSlug: slug
    };
  }

  if (route === ROUTES.COMMUNICATION_ADMIN) {
    const communications = await getCommunicationCenterData();

    return {
      ...shellData,
      communications
    };
  }

  if (route === ROUTES.SETTINGS || route === ROUTES.SETTINGS_LDAP || route === ROUTES.SETTINGS_MICROSOFT_GRAPH) {
    const adminHeaders = { headers: getAdminAuthHeaders() };
    const [ldapSettings, microsoftGraphSettings] = await Promise.all([
      getLdapSettingsData(adminHeaders),
      getMicrosoftGraphSettingsData(adminHeaders)
    ]);

    return {
      ...shellData,
      ldapSettings,
      microsoftGraphSettings
    };
  }

  if (route === ROUTES.ADMIN_USERS) {
    try {
      const portalUsersPage = await listPortalUsers(buildAdminUsersQuery(), {
        headers: getAdminAuthHeaders()
      });

      adminUsersQueryState = {
        ...adminUsersQueryState,
        query: portalUsersPage.query ?? "",
        status: portalUsersPage.status || "all",
        role: portalUsersPage.role || "all",
        department: portalUsersPage.department || "all",
        sortBy: portalUsersPage.sortBy || "displayName",
        sortDirection: portalUsersPage.sortDirection || "asc",
        page: portalUsersPage.page || adminUsersQueryState.page,
        pageSize: portalUsersPage.pageSize || adminUsersQueryState.pageSize
      };

      return {
        ...shellData,
        portalUsersPage,
        portalUsersLoadError: ""
      };
    } catch (error) {
      console.error("Falha ao carregar usuarios administrativos do portal.", error);

      return {
        ...shellData,
        portalUsersPage: createEmptyPortalUsersPage(),
        portalUsersLoadError: "Nao foi possivel consultar a API administrativa de usuarios. Verifique se a API do ambiente esta ativa."
      };
    }
  }

  if (route === ROUTES.ADMIN_POLLS) {
    if (!canManagePolls()) {
      return {
        ...shellData,
        adminPollsPage: createEmptyAdminPollsPage()
      };
    }

    const adminPollsPage = await getAdminPollData({
      headers: getPortalAuthHeaders()
    });

    return {
      ...shellData,
      adminPollsPage
    };
  }

  if (route === ROUTES.PEOPLE) {
    if (!canAccessHrMoodDashboard()) {
      return {
        ...shellData,
        moodDashboard: null,
        moodDashboardLoadError: "",
        moodFeedbackPage: null,
        moodFeedbackLoadError: ""
      };
    }

    let moodDashboard = null;
    let moodDashboardLoadError = "";
    let moodFeedbackPage = null;
    let moodFeedbackLoadError = "";

    try {
      moodDashboard = await getMoodSurveyDashboard(buildMoodDashboardQuery());
    } catch (error) {
      console.error("Falha ao carregar dashboard de humor do RH.", error);
      moodDashboardLoadError = "Nao foi possivel consultar a distribuicao de humor. Verifique se a API do ambiente esta ativa.";
    }

    if (canManageMoodSurveyFeedback()) {
      try {
        moodFeedbackPage = await listMoodFeedbackMessages();
      } catch (error) {
        console.error("Falha ao carregar mensagens de feedback do humor.", error);
        moodFeedbackLoadError = "Nao foi possivel carregar as mensagens de feedback.";
      }
    }

    return {
      ...shellData,
      moodDashboard,
      moodDashboardLoadError,
      moodFeedbackPage,
      moodFeedbackLoadError
    };
  }

  return shellData;
}

async function ensureRoutePermission(route) {
  const session = getStoredPortalSession();
  if (!session || canViewRoute(session, route)) {
    return true;
  }

  showToast("Voce nao possui permissao para acessar esta area do portal.", "danger");
  window.location.hash = "#inicio";
  return false;
}

async function renderCurrentRoute() {
  const { route, slug } = parseRoute();
  renderRuntimeBadge();
  renderLoadingApp(route);

  if (route === ROUTES.COMMUNICATION_ADMIN) {
    const hasEditorAccess = await ensureCommunicationEditorAccess();
    if (!hasEditorAccess) {
      return;
    }
  } else if (route === ROUTES.SETTINGS || route === ROUTES.SETTINGS_LDAP || route === ROUTES.SETTINGS_MICROSOFT_GRAPH || route === ROUTES.ADMIN_USERS) {
    const hasAdminAccess = await ensureRestrictedAdminAccess(route);
    if (!hasAdminAccess) {
      return;
    }
  } else {
    const hasPortalAccess = await ensurePortalAccess();
    if (!hasPortalAccess) {
      return;
    }

    const permissionRoute = route === ROUTES.HR_PROFILE
      ? `${ROUTES.HR_PROFILE}/${slug}`
      : route === ROUTES.JOURNEY
        ? `${ROUTES.JOURNEY}/${slug}`
        : route;
    const hasRoutePermission = await ensureRoutePermission(permissionRoute);
    if (!hasRoutePermission) {
      return;
    }
  }

  const data = await loadPageData(route, slug);

  if (route !== ROUTES.PEOPLE && route !== ROUTES.ADMIN_POLLS) {
    destroyMoodDashboardCharts();
  }

  if (route === ROUTES.HOME) {
    renderHomePage(data, route);
  } else if (route === ROUTES.SAVED) {
    renderSavedFeedPage(data, route);
  } else if (route === ROUTES.COMMUNICATIONS) {
    renderCommunicationsPage(data, route);
  } else if (route === ROUTES.COMMUNICATION_READ) {
    renderCommunicationReadPage(data, route, slug);
  } else if (route === ROUTES.POLLS) {
    renderPollsPage(data, route);
  } else if (route === ROUTES.POLL_READ) {
    renderPollReadPage(data, route);
  } else if (route === ROUTES.COMMUNICATION_ADMIN) {
    renderCommunicationAdminRoute(data, route);
  } else if (route === ROUTES.SETTINGS) {
    renderAdminSettingsRoute(data, route);
  } else if (route === ROUTES.SETTINGS_LDAP) {
    renderAdminLdapRoute(data, route);
  } else if (route === ROUTES.SETTINGS_MICROSOFT_GRAPH) {
    renderAdminMicrosoftGraphRoute(data, route);
  } else if (route === ROUTES.ADMIN_USERS) {
    renderAdminUsersRoute(data, route);
  } else if (route === ROUTES.ADMIN_POLLS) {
    renderAdminPollsRoute(data, route);
  } else if (route === ROUTES.PEOPLE) {
    renderPeopleRhPage(data, route);
  } else if (route === ROUTES.HR_PROFILE) {
    renderHrProfilePage(data, route, slug);
  } else if (route === ROUTES.JOURNEY) {
    renderJourneyPage(data, route, slug);
  } else {
    renderPlaceholderPage(data, route);
  }

  bindAnalytics(document);
  trackInteraction("page.view", { route, slug });
}

async function bootstrap() {
  bindInteractionFeedback(document);
  bindFeedPhotoViewerActions();

  if (!adminUsersRefreshBound) {
    document.addEventListener("portal-users:refresh", (event) => {
      const detail = event.detail || {};
      refreshAdminUsersDataOnly({
        feedbackMessage: detail.message || "",
        feedbackTone: detail.tone || "success",
        preserveModalUserId: detail.preserveModalUserId || "",
        preserveModalMode: detail.preserveModalMode || "edit"
      }).catch((error) => {
        console.error("Falha ao atualizar a tela de usuarios administrativos.", error);
        showToast("Nao foi possivel recarregar os usuarios apos a atualizacao.", "danger");
      });
    });

    adminUsersRefreshBound = true;
  }

  if (!shellInitialized) {
    window.addEventListener("hashchange", () => {
      renderCurrentRoute().catch((error) => {
        console.error("Falha ao trocar de rota na LIOCONNECTA.", error);
        renderBootstrapError(error);
      });
    });

    shellInitialized = true;
  }

  await renderCurrentRoute();
  await setupServiceWorker();
  trackInteraction("app.loaded", { source: "bootstrap" });
}

bootstrap().catch((error) => {
  renderBootstrapError(error);
});
