import { renderEmptyState } from "../components/cards.js";
import { escapeHtml } from "../components/html.js";
import { renderRhAdminHero } from "../people/adminNav.js";
import { getJourneyModule } from "./moduleCatalog.js";

function formatDate(value) {
  if (!value) {
    return "—";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return String(value);
  }

  return date.toLocaleDateString("pt-BR");
}

function renderPageShell({ title, provider, isSimulated, bodyHtml }) {
  const providerLabel = provider || "ServiceNow";
  const description = isSimulated
    ? `Consulta integrada ao ${providerLabel}. Os dados exibidos nesta fase sao simulados para validacao da experiencia.`
    : `Consulta integrada ao ${providerLabel}.`;

  return `
    <div class="hr-profile-main">
      ${renderRhAdminHero({
        eyebrow: "MINHA JORNADA",
        title: title || "Minha Jornada",
        description
      })}
      ${bodyHtml}
    </div>
  `;
}

function renderContentCard({ title, headerActionHtml = "", bodyHtml }) {
  return `
    <section class="card comm-list-card">
      <div class="card-header hr-profile-card-header">
        <span>${escapeHtml(title)}</span>
        ${headerActionHtml ? `<div class="hr-profile-card-header__actions">${headerActionHtml}</div>` : ""}
      </div>
      <div class="comm-list-body">
        ${bodyHtml}
      </div>
    </section>
  `;
}

function renderMetricCards(items = []) {
  return `
    <div class="hr-profile-metrics">
      ${items.map((item) => `
        <article class="hr-profile-metric">
          <span>${escapeHtml(item.label)}</span>
          <strong>${escapeHtml(item.value)}</strong>
          ${item.detail ? `<small>${escapeHtml(item.detail)}</small>` : ""}
        </article>
      `).join("")}
    </div>
  `;
}

function renderStatusPill(status = "") {
  const normalized = String(status).toLowerCase();
  const tone = normalized.includes("aprov") || normalized.includes("dispon") || normalized.includes("assin")
    ? "success"
    : normalized.includes("analise") || normalized.includes("andamento") || normalized.includes("aguard")
      ? "warning"
      : normalized.includes("atras") || normalized.includes("pend")
        ? "danger"
        : "info";

  return `<span class="panel-pill panel-pill--${tone}">${escapeHtml(status)}</span>`;
}

function renderPriorityPill(priority = "") {
  const normalized = String(priority).toLowerCase();
  const tone = normalized.includes("alta")
    ? "danger"
    : normalized.includes("media")
      ? "warning"
      : "info";

  return `<span class="panel-pill panel-pill--${tone}">${escapeHtml(priority)}</span>`;
}

function renderTasksPage(data = {}) {
  const summary = data.summary || {};
  const items = Array.isArray(data.items) ? data.items : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: `
      ${renderContentCard({
        title: "Resumo de tarefas",
        bodyHtml: renderMetricCards([
          { label: "Abertas", value: String(summary.openCount ?? 0) },
          { label: "Atrasadas", value: String(summary.overdueCount ?? 0) },
          { label: "Vencem hoje", value: String(summary.dueTodayCount ?? 0) }
        ])
      })}
      ${renderContentCard({
        title: "Tarefas pendentes",
        bodyHtml: items.length
          ? `
            <div class="hr-profile-table-wrap">
              <table class="hr-profile-table">
                <thead>
                  <tr>
                    <th>Tarefa</th>
                    <th>Prioridade</th>
                    <th>Prazo</th>
                    <th>Responsavel</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  ${items.map((item) => `
                    <tr>
                      <td>${escapeHtml(item.title)}</td>
                      <td>${renderPriorityPill(item.priority)}</td>
                      <td>${escapeHtml(formatDate(item.dueDate))}</td>
                      <td>${escapeHtml(item.assignee)}</td>
                      <td>${renderStatusPill(item.status)}</td>
                    </tr>
                  `).join("")}
                </tbody>
              </table>
            </div>
          `
          : renderEmptyState("Nenhuma tarefa", "Suas tarefas pendentes aparecerao aqui.")
      })}
    `
  });
}

function renderRequestsPage(data = {}) {
  const summary = data.summary || {};
  const items = Array.isArray(data.items) ? data.items : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: `
      ${renderContentCard({
        title: "Resumo de solicitacoes",
        bodyHtml: renderMetricCards([
          { label: "Total em andamento", value: String(summary.totalCount ?? 0) },
          { label: "Aguardando aprovacao", value: String(summary.pendingApprovalCount ?? 0) },
          { label: "Em processamento", value: String(summary.inProgressCount ?? 0) }
        ])
      })}
      ${renderContentCard({
        title: "Solicitacoes recentes",
        bodyHtml: items.length
          ? `
            <div class="hr-profile-list">
              ${items.map((item) => `
                <article class="hr-profile-list-item">
                  <div>
                    <strong>${escapeHtml(item.type)}</strong>
                    <span>${escapeHtml(item.description)}</span>
                  </div>
                  <div class="hr-profile-list-item__meta">
                    <span>Aberta em ${escapeHtml(formatDate(item.openedAtUtc))}</span>
                    <span>${escapeHtml(item.stage)}</span>
                    ${renderStatusPill(item.status)}
                  </div>
                </article>
              `).join("")}
            </div>
          `
          : renderEmptyState("Nenhuma solicitacao", "Suas solicitacoes em andamento aparecerao aqui.")
      })}
    `
  });
}

function renderLearningPathsPage(data = {}) {
  const summary = data.summary || {};
  const items = Array.isArray(data.items) ? data.items : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: `
      ${renderContentCard({
        title: "Resumo de trilhas",
        bodyHtml: renderMetricCards([
          { label: "Trilhas ativas", value: String(summary.enrolledCount ?? 0) },
          { label: "Concluidas", value: String(summary.completedCount ?? 0) },
          { label: "Carga horaria", value: summary.hoursLabel || "—" }
        ])
      })}
      ${renderContentCard({
        title: "Trilhas em andamento",
        bodyHtml: `
          <div class="hr-profile-competencies">
            ${items.length ? items.map((item) => `
              <article class="hr-profile-competency">
                <div class="hr-profile-competency__head">
                  <strong>${escapeHtml(item.title)}</strong>
                  <span>${escapeHtml(String(item.progressPercent ?? 0))}%</span>
                </div>
                <div class="hr-profile-progress" aria-hidden="true">
                  <span style="width:${Math.min(100, Number(item.progressPercent ?? 0))}%"></span>
                </div>
                <small>
                  ${escapeHtml(item.durationLabel || "—")}
                  ${item.dueDate ? ` • Prazo ${escapeHtml(formatDate(item.dueDate))}` : ""}
                  • ${escapeHtml(item.status || "—")}
                </small>
              </article>
            `).join("") : renderEmptyState("Nenhuma trilha", "Suas trilhas de aprendizagem aparecerao aqui.")}
          </div>
        `
      })}
    `
  });
}

function renderDocumentsPage(data = {}) {
  const items = Array.isArray(data.items) ? data.items : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: renderContentCard({
      title: "Documentos recentes",
      bodyHtml: items.length
        ? `
          <div class="hr-profile-list">
            ${items.map((item) => `
              <article class="hr-profile-list-item">
                <div>
                  <strong>${escapeHtml(item.title)}</strong>
                  <span>${escapeHtml(item.category)} • Atualizado em ${escapeHtml(formatDate(item.updatedAtUtc))}</span>
                </div>
                <div class="hr-profile-list-item__meta">
                  <span>${escapeHtml(item.sizeLabel)}</span>
                  ${renderStatusPill(item.status)}
                  <button type="button" class="comm-secondary-button" disabled>Baixar</button>
                </div>
              </article>
            `).join("")}
          </div>
        `
        : renderEmptyState("Nenhum documento", "Seus documentos recentes aparecerao aqui.")
    })
  });
}

const PAGE_RENDERERS = Object.freeze({
  tarefas: renderTasksPage,
  solicitacoes: renderRequestsPage,
  trilhas: renderLearningPathsPage,
  documentos: renderDocumentsPage
});

export function renderJourneyModulePage(slug, data = {}) {
  const renderer = PAGE_RENDERERS[slug];
  if (!renderer) {
    const module = getJourneyModule(slug);
    return renderPageShell({
      title: module?.label || "Minha Jornada",
      provider: data.provider,
      isSimulated: data.isSimulated,
      bodyHtml: renderContentCard({
        title: "Modulo indisponivel",
        bodyHtml: renderEmptyState("Modulo indisponivel", "Este item da jornada ainda nao possui tela configurada.")
      })
    });
  }

  return renderer(data);
}
