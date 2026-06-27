import { escapeHtml } from "../components/html.js";
import { renderRhAdminHero } from "../people/adminNav.js";
import { renderEmptyState, renderErrorCard } from "../components/cards.js";
import { renderMoodFeedbackAdminSection } from "./moodFeedbackAdmin.js";

const MOOD_KPI_DEFINITIONS = [
  { key: "totalVotes", label: "Votos no periodo", detail: "Registros persistidos", tone: "brand", icon: "fa-solid fa-clipboard-check" },
  { key: "uniqueUsers", label: "Colaboradores unicos", detail: "Participantes distintos", tone: "indigo", icon: "fa-solid fa-users" },
  { key: "participationRate", label: "Taxa de participacao", detailKey: "activeUsers", detailPrefix: "Base:", detailSuffix: "ativos", tone: "info", icon: "fa-solid fa-chart-pie", suffix: "%" },
  { key: "motivatedCount", label: "Motivados", detail: "Respostas positivas", tone: "motivated", icon: "fa-solid fa-face-smile-beam" },
  { key: "goodCount", label: "Bem", detail: "Respostas equilibradas", tone: "good", icon: "fa-solid fa-face-smile" },
  { key: "tiredCount", label: "Cansados", detail: "Sinal de atencao", tone: "tired", icon: "fa-solid fa-moon" }
];

function renderDepartmentRow(item) {
  return `
    <tr>
      <td>${escapeHtml(item.department)}</td>
      <td>${escapeHtml(String(item.totalVotes))}</td>
      <td>${escapeHtml(String(item.motivatedCount))}</td>
      <td>${escapeHtml(String(item.goodCount))}</td>
      <td>${escapeHtml(String(item.tiredCount))}</td>
    </tr>
  `;
}

function renderDailyTrendChartCanvas(hasData) {
  if (!hasData) {
    return "";
  }

  return `
    <div class="mood-dashboard-chart-wrap">
      <div class="mood-dashboard-chart-canvas mood-dashboard-chart-canvas--trend">
        <canvas id="mood-dashboard-trend-chart" aria-label="Grafico de evolucao diaria dos votos de humor"></canvas>
      </div>
    </div>
  `;
}

function renderDistributionChartCanvas(hasData) {
  if (!hasData) {
    return "";
  }

  return `
    <div class="mood-dashboard-chart-wrap">
      <div class="mood-dashboard-chart-canvas mood-dashboard-chart-canvas--distribution">
        <canvas id="mood-dashboard-distribution-chart" aria-label="Grafico de distribuicao geral do humor"></canvas>
      </div>
    </div>
  `;
}

function renderMoodKpiCard(definition, summary = {}) {
  const rawValue = summary[definition.key] ?? 0;
  const value = definition.suffix ? `${rawValue}${definition.suffix}` : String(rawValue);
  const detail = definition.detailKey
    ? `${definition.detailPrefix} ${summary[definition.detailKey] ?? 0} ${definition.detailSuffix}`
    : definition.detail;

  return `
    <article class="mood-dashboard-kpi mood-dashboard-kpi--${escapeHtml(definition.tone)}">
      <div class="mood-dashboard-kpi__icon" aria-hidden="true">
        <i class="${escapeHtml(definition.icon)}"></i>
      </div>
      <div class="mood-dashboard-kpi__body">
        <span class="mood-dashboard-kpi__label">${escapeHtml(definition.label)}</span>
        <strong class="mood-dashboard-kpi__value">${escapeHtml(value)}</strong>
        <span class="mood-dashboard-kpi__detail">${escapeHtml(detail)}</span>
      </div>
    </article>
  `;
}

function renderMoodKpiGrid(summary = {}) {
  return `
    <section class="mood-dashboard-kpi-grid" aria-label="Indicadores do periodo">
      ${MOOD_KPI_DEFINITIONS.map((definition) => renderMoodKpiCard(definition, summary)).join("")}
    </section>
  `;
}

export function renderRhMoodDashboardPage(dashboard, {
  periodPreset = "7d",
  department = "all",
  loadError = "",
  accessDenied = false,
  feedbackPage = null,
  feedbackLoadError = "",
  feedbackOptionKey = "motivated",
  feedbackEditingId = ""
} = {}) {
  if (accessDenied) {
    return `
      ${renderRhAdminHero({
        eyebrow: "PESSOAS (RH)",
        title: "Humor da Companhia",
        description: "Esta visao e destinada ao time de RH com permissao no painel de pessoas."
      })}
      ${renderEmptyState(
        "Acesso restrito",
        "Seu perfil ainda nao possui permissao para consultar a distribuicao de humor da companhia."
      )}
    `;
  }

  if (loadError) {
    return renderErrorCard("Erro ao carregar dashboard de humor", loadError);
  }

  const viewModel = dashboard || {
    summary: {},
    options: [],
    departments: [],
    dailyTrend: [],
    departmentOptions: []
  };
  const summary = viewModel.summary || {};
  const hasTrendData = viewModel.dailyTrend.length > 0;
  const hasDistributionData = viewModel.options.length > 0;

  return `
    ${renderRhAdminHero({
      title: "Humor da Companhia",
      description: "Acompanhe como os colaboradores estao se sentindo por periodo e por area, com base nos registros da pesquisa diaria."
    })}

    <section class="card communication-form-card">
      <div class="card-header">Filtros do periodo</div>
      <div class="communication-form-grid mood-dashboard-filters">
        <label class="communication-form-field">
          <span>Periodo</span>
          <select id="mood-dashboard-period-filter">
            <option value="7d" ${periodPreset === "7d" ? "selected" : ""}>Ultimos 7 dias</option>
            <option value="30d" ${periodPreset === "30d" ? "selected" : ""}>Ultimos 30 dias</option>
          </select>
        </label>
        <label class="communication-form-field">
          <span>Area / departamento</span>
          <select id="mood-dashboard-department-filter">
            <option value="all" ${department === "all" ? "selected" : ""}>Todas as areas</option>
            ${(viewModel.departmentOptions || []).map((item) => `
              <option value="${escapeHtml(item.key)}" ${department === item.key ? "selected" : ""}>
                ${escapeHtml(item.label)} (${escapeHtml(String(item.count))})
              </option>
            `).join("")}
          </select>
        </label>
        <div class="communication-form-field communication-form-field--full mood-dashboard-period-summary">
          <span>Intervalo analisado</span>
          <strong>${escapeHtml(viewModel.startDateLabel || "")} até ${escapeHtml(viewModel.endDateLabel || "")}</strong>
        </div>
      </div>
    </section>

    ${renderMoodKpiGrid(summary)}

    <section class="card comm-list-card mood-dashboard-chart-card">
      <div class="card-header">Evolucao diaria</div>
      <div class="comm-list-body mood-dashboard-chart-body">
        ${hasTrendData
          ? renderDailyTrendChartCanvas(true)
          : renderEmptyState("Sem serie temporal", "Nao ha registros diarios para o intervalo selecionado.")}
      </div>
    </section>

    <section class="communication-admin-layout mood-dashboard-layout">
      <section class="card comm-list-card">
        <div class="card-header">Distribuicao geral</div>
        <div class="comm-list-body mood-dashboard-chart-body">
          ${hasDistributionData
            ? renderDistributionChartCanvas(true)
            : renderEmptyState("Sem votos no periodo", "Quando os colaboradores responderem a pesquisa diaria, os indicadores aparecerao aqui.")}
        </div>
      </section>

      <section class="card comm-list-card mood-dashboard-summary-card">
        <div class="card-header">Resumo do periodo</div>
        <div class="comm-list-body mood-dashboard-summary-list">
          ${(viewModel.options || []).map((option) => `
            <article class="mood-dashboard-summary-item mood-dashboard-summary-item--${escapeHtml(option.key)}">
              <span class="mood-dashboard-summary-item__emoji" aria-hidden="true">${escapeHtml(option.emoji)}</span>
              <div>
                <strong>${escapeHtml(option.label)}</strong>
                <p>${escapeHtml(String(option.count))} votos • ${escapeHtml(String(option.percentage))}% do total</p>
              </div>
            </article>
          `).join("")}
        </div>
      </section>
    </section>

    <section class="card comm-list-card">
      <div class="card-header">Distribuicao por area</div>
      <div class="comm-list-body">
        ${viewModel.departments.length
          ? `
            <div class="mood-dashboard-table-wrap">
              <table class="mood-dashboard-table">
                <thead>
                  <tr>
                    <th>Area</th>
                    <th>Total</th>
                    <th>Motivado</th>
                    <th>Bem</th>
                    <th>Cansado</th>
                  </tr>
                </thead>
                <tbody>
                  ${viewModel.departments.map(renderDepartmentRow).join("")}
                </tbody>
              </table>
            </div>
          `
          : renderEmptyState("Sem dados por area", "Os votos ainda nao foram agrupados por departamento neste periodo.")}
      </div>
    </section>
    ${feedbackPage || feedbackLoadError
      ? renderMoodFeedbackAdminSection(feedbackPage, {
        selectedOptionKey: feedbackOptionKey,
        editingId: feedbackEditingId,
        loadError: feedbackLoadError
      })
      : ""}
  `;
}

export function renderMoodAuditItem(item) {
  return `
    <article class="admin-activity-item">
      <div class="admin-activity-item__top">
        <strong>${escapeHtml(item.portalUserDisplayName)}</strong>
        <span class="comm-tag">${escapeHtml(item.actionTypeLabel)}</span>
      </div>
      <p>${escapeHtml(item.optionEmoji)} ${escapeHtml(item.optionLabel)}${item.department ? ` • ${escapeHtml(item.department)}` : ""}</p>
      <p>${item.origin ? `${escapeHtml(item.origin)}` : "Portal"}${item.ipAddress ? ` • IP ${escapeHtml(item.ipAddress)}` : ""}</p>
      <span class="admin-activity-item__meta">${escapeHtml(item.surveyDateLabel || item.createdAtLabel || "Sem horario registrado")}</span>
    </article>
  `;
}
