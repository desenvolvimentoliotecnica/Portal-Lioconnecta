import { renderEmptyState } from "../components/cards.js";
import { escapeHtml } from "../components/html.js";
import { renderRhAdminHero } from "../people/adminNav.js";
import { getHrProfileModule } from "./moduleCatalog.js";

function formatCurrency(value) {
  const amount = Number(value ?? 0);
  return amount.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

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
  const providerLabel = provider || "TOTVS RM";
  const description = isSimulated
    ? `Consulta integrada ao ${providerLabel}. Os dados exibidos nesta fase sao simulados para validacao da experiencia.`
    : `Consulta integrada ao ${providerLabel}.`;

  return `
    <div class="hr-profile-main">
      ${renderRhAdminHero({
        eyebrow: "PERFIL RH",
        title: title || "Perfil RH",
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
  const tone = normalized.includes("aprov")
    ? "success"
    : normalized.includes("analise") || normalized.includes("andamento")
      ? "warning"
      : normalized.includes("antecipada") || normalized.includes("atras")
        ? "danger"
        : "info";

  return `<span class="panel-pill panel-pill--${tone}">${escapeHtml(status)}</span>`;
}

function renderVacationPage(data = {}) {
  const balance = data.balance || {};
  const requests = Array.isArray(data.requests) ? data.requests : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: `
      ${renderContentCard({
        title: "Saldo de ferias",
        bodyHtml: renderMetricCards([
          { label: "Saldo disponivel", value: `${balance.availableDays ?? 0} dias` },
          { label: "Agendadas", value: `${balance.scheduledDays ?? 0} dias` },
          { label: "Utilizadas", value: `${balance.usedDays ?? 0} dias` },
          { label: "Proximo periodo", value: formatDate(balance.nextAcquisitionDate) }
        ])
      })}
      ${renderContentCard({
        title: "Solicitacoes recentes",
        headerActionHtml: data.canRequest
          ? `<button type="button" class="feed-composer-submit" disabled title="Integracao futura com TOTVS RM">Solicitar ferias</button>`
          : "",
        bodyHtml: requests.length
          ? `
            <div class="hr-profile-table-wrap">
              <table class="hr-profile-table">
                <thead>
                  <tr>
                    <th>Periodo</th>
                    <th>Dias</th>
                    <th>Solicitado em</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  ${requests.map((item) => `
                    <tr>
                      <td>${escapeHtml(formatDate(item.startDate))} - ${escapeHtml(formatDate(item.endDate))}</td>
                      <td>${escapeHtml(String(item.days ?? 0))}</td>
                      <td>${escapeHtml(formatDate(item.requestedAtUtc))}</td>
                      <td>${renderStatusPill(item.status)}</td>
                    </tr>
                  `).join("")}
                </tbody>
              </table>
            </div>
          `
          : renderEmptyState("Nenhuma solicitacao", "Suas solicitacoes de ferias aparecerao aqui.")
      })}
    `
  });
}

function renderPayslipPage(data = {}) {
  const items = Array.isArray(data.items) ? data.items : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: renderContentCard({
      title: "Comprovantes disponiveis",
      bodyHtml: items.length
        ? `
          <div class="hr-profile-list">
            ${items.map((item) => `
              <article class="hr-profile-list-item">
                <div>
                  <strong>${escapeHtml(item.periodLabel)}</strong>
                  <span>Pagamento em ${escapeHtml(formatDate(item.paymentDate))}</span>
                </div>
                <div class="hr-profile-list-item__meta">
                  <span>Liquido ${escapeHtml(formatCurrency(item.netAmount))}</span>
                  ${renderStatusPill(item.status)}
                  <button type="button" class="comm-secondary-button" disabled>Baixar PDF</button>
                </div>
              </article>
            `).join("")}
          </div>
        `
        : renderEmptyState("Nenhum holerite", "Os holerites liberados pelo RH aparecerao aqui.")
    })
  });
}

function renderBenefitsPage(data = {}) {
  const items = Array.isArray(data.items) ? data.items : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: renderContentCard({
      title: "Beneficios ativos",
      bodyHtml: `
        <div class="hr-profile-grid">
          ${items.map((item) => `
            <article class="hr-profile-benefit-card">
              <div class="hr-profile-benefit-card__head">
                <span class="panel-pill panel-pill--brand">${escapeHtml(item.category)}</span>
                ${renderStatusPill(item.status)}
              </div>
              <h3>${escapeHtml(item.label)}</h3>
              <strong>${escapeHtml(item.value)}</strong>
              <p>${escapeHtml(item.details)}</p>
            </article>
          `).join("")}
        </div>
      `
    })
  });
}

function renderEvaluationPage(data = {}) {
  const competencies = Array.isArray(data.competencies) ? data.competencies : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: `
      ${renderContentCard({
        title: "Resumo da avaliacao",
        bodyHtml: renderMetricCards([
          { label: "Ciclo", value: data.cycleLabel || "—" },
          { label: "Status", value: data.status || "—" },
          { label: "Nota geral", value: String(data.overallScore ?? "—"), detail: data.overallLabel || "" }
        ])
      })}
      ${renderContentCard({
        title: "Competencias avaliadas",
        bodyHtml: `
          <div class="hr-profile-competencies">
            ${competencies.map((item) => `
              <article class="hr-profile-competency">
                <div class="hr-profile-competency__head">
                  <strong>${escapeHtml(item.name)}</strong>
                  <span>${escapeHtml(String(item.score))}/${escapeHtml(String(item.maxScore))}</span>
                </div>
                <div class="hr-profile-progress" aria-hidden="true">
                  <span style="width:${Math.min(100, (Number(item.score) / Number(item.maxScore || 1)) * 100)}%"></span>
                </div>
                <small>${escapeHtml(item.levelLabel)}</small>
              </article>
            `).join("")}
          </div>
        `
      })}
      ${data.managerFeedback ? renderContentCard({
        title: "Feedback do gestor",
        bodyHtml: `<p class="hr-profile-feedback">${escapeHtml(data.managerFeedback)}</p>`
      }) : ""}
    `
  });
}

function renderPersonalDataPage(data = {}) {
  const sections = Array.isArray(data.sections) ? data.sections : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: renderContentCard({
      title: "Informacoes cadastrais",
      bodyHtml: `
        <div class="hr-profile-sections">
          ${sections.map((section) => `
            <section class="hr-profile-data-section">
              <h3>${escapeHtml(section.title)}</h3>
              <div class="hr-profile-fields">
                ${(section.fields || []).map((field) => `
                  <div class="hr-profile-field">
                    <span>${escapeHtml(field.label)}</span>
                    <strong>${escapeHtml(field.value)}</strong>
                    ${field.isEditable ? `<small>Editavel via RH</small>` : `<small>Somente leitura</small>`}
                  </div>
                `).join("")}
              </div>
            </section>
          `).join("")}
        </div>
      `
    })
  });
}

function renderTimesheetPage(data = {}) {
  const summary = data.summary || {};
  const entries = Array.isArray(data.entries) ? data.entries : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    bodyHtml: `
      ${renderContentCard({
        title: "Resumo do periodo",
        bodyHtml: renderMetricCards([
          { label: "Periodo", value: summary.periodLabel || "—" },
          { label: "Horas trabalhadas", value: summary.workedHours || "—" },
          { label: "Horas previstas", value: summary.expectedHours || "—" },
          { label: "Banco de horas", value: summary.balanceHours || "—" },
          { label: "Faltas", value: String(summary.absences ?? 0) },
          { label: "Atrasos", value: String(summary.delays ?? 0) }
        ])
      })}
      ${renderContentCard({
        title: "Registros recentes",
        bodyHtml: entries.length
          ? `
            <div class="hr-profile-table-wrap">
              <table class="hr-profile-table">
                <thead>
                  <tr>
                    <th>Data</th>
                    <th>Entrada</th>
                    <th>Saida</th>
                    <th>Intervalo</th>
                    <th>Trabalhado</th>
                    <th>Saldo</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  ${entries.map((item) => `
                    <tr>
                      <td>${escapeHtml(formatDate(item.date))} • ${escapeHtml(item.weekdayLabel)}</td>
                      <td>${escapeHtml(item.clockIn)}</td>
                      <td>${escapeHtml(item.clockOut)}</td>
                      <td>${escapeHtml(item.breakMinutes)} min</td>
                      <td>${escapeHtml(item.workedHours)}</td>
                      <td>${escapeHtml(item.balanceHours)}</td>
                      <td>${renderStatusPill(item.status)}</td>
                    </tr>
                  `).join("")}
                </tbody>
              </table>
            </div>
          `
          : renderEmptyState("Sem registros", "Os apontamentos de ponto aparecerao aqui.")
      })}
    `
  });
}

const PAGE_RENDERERS = Object.freeze({
  ferias: renderVacationPage,
  holerite: renderPayslipPage,
  beneficios: renderBenefitsPage,
  avaliacao: renderEvaluationPage,
  cadastro: renderPersonalDataPage,
  ponto: renderTimesheetPage
});

export function renderHrProfileModulePage(slug, data = {}) {
  const renderer = PAGE_RENDERERS[slug];
  if (!renderer) {
    const module = getHrProfileModule(slug);
    return renderPageShell({
      title: module?.label || "Perfil RH",
      provider: data.provider,
      isSimulated: data.isSimulated,
      bodyHtml: renderContentCard({
        title: "Modulo indisponivel",
        bodyHtml: renderEmptyState("Modulo indisponivel", "Este item do perfil RH ainda nao possui tela configurada.")
      })
    });
  }

  return renderer(data);
}
