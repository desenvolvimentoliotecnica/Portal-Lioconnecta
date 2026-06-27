import { showToast } from "../core/feedback.js?v=0.12.10";
import { loginAdmin, fetchAdminSession, getStoredAdminSession, resolvePostLoginTarget } from "../services/adminAuthService.js?v=0.12.8";
import { getRuntimeConfig } from "../core/runtimeConfig.js?v=0.13.1";

function renderVersionBadge() {
  const badge = document.querySelector(".app-version-badge");
  if (!badge) {
    return;
  }

  const config = getRuntimeConfig();
  badge.textContent = `${config.version} • ADMIN`;
}

function normalizeHashRoute(rawHash = "") {
  return String(rawHash).replace(/^#/, "").replace(/^\/+/, "").trim();
}

function redirectToTarget(hash = "#comunicacao/restrita") {
  const normalized = normalizeHashRoute(hash) || "comunicacao/restrita";
  window.location.href = `../#${normalized}`;
}

function bindAdminLoginForm() {
  const form = document.getElementById("admin-login-form");
  if (!form) {
    return;
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();

    const formData = new FormData(form);
    const username = String(formData.get("username") || "").trim();
    const password = String(formData.get("password") || "");

    if (!username || !password) {
      showToast("Informe usuario e senha para continuar.", "info");
      return;
    }

    const submitButton = form.querySelector("[type='submit']");
    const originalLabel = submitButton?.textContent || "Entrar como administrador";

    if (submitButton) {
      submitButton.disabled = true;
      submitButton.textContent = "Validando...";
    }

    try {
      const session = await loginAdmin(username, password);
      if (!session) {
        throw new Error("Sessao administrativa nao retornada.");
      }

      showToast(`Acesso administrativo liberado para ${session.user.displayName}.`, "success");
      window.setTimeout(() => {
        redirectToTarget(resolvePostLoginTarget());
      }, 400);
    } catch (error) {
      console.error("Falha no login administrativo.", error);
      showToast("Nao foi possivel autenticar o super-admin com os dados informados.", "danger");
    } finally {
      if (submitButton) {
        submitButton.disabled = false;
        submitButton.textContent = originalLabel;
      }
    }
  });
}

async function tryRestoreAdminSession() {
  const session = getStoredAdminSession();
  if (!session) {
    return;
  }

  try {
    const validated = await fetchAdminSession();
    if (validated) {
      redirectToTarget(resolvePostLoginTarget());
    }
  } catch {
    // Mantem a tela de login exibida quando a sessao expira.
  }
}

function bootstrapAdminLogin() {
  renderVersionBadge();
  bindAdminLoginForm();
  tryRestoreAdminSession();
}

bootstrapAdminLogin();
