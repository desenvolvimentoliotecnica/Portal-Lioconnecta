import { renderEmptyState } from "../components/cards.js";
import { escapeHtml } from "../components/html.js";
import { renderRhAdminHero } from "../people/adminNav.js";
import { getHrProfileModule } from "./moduleCatalog.js";
import { renderPayslipModalShell } from "./payslipModal.js";

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

function renderPageShell({ title, provider, isSimulated, bodyHtml, heroImage = "", heroImageLabel = "" }) {
  const providerLabel = provider || "TOTVS RM";
  const description = isSimulated
    ? `Consulta integrada ao ${providerLabel}. Os dados exibidos nesta fase sao simulados para validacao da experiencia.`
    : `Consulta integrada ao ${providerLabel}.`;

  return `
    <div class="hr-profile-main">
      ${renderRhAdminHero({
        eyebrow: "PERFIL RH",
        title: title || "Perfil RH",
        description,
        heroImage,
        heroImageLabel
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
    heroImage: "./assets/img/hero-ferias-perfil-rh.png",
    heroImageLabel: "Paisagem serena simbolizando descanso e periodo de ferias",
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
  const groupedByYear = items.reduce((groups, item) => {
    const year = item.competenceYear || String(item.referenceMonth || "").split("-")[0] || "—";
    if (!groups[year]) {
      groups[year] = [];
    }
    groups[year].push(item);
    return groups;
  }, {});

  const years = Object.keys(groupedByYear).sort((left, right) => Number(right) - Number(left));

  function formatPaymentDateShort(value) {
    if (!value) {
      return "—";
    }
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return String(value);
    }
    return date.toLocaleDateString("pt-BR", { day: "2-digit", month: "2-digit" });
  }

  function renderEnvelopeRow(item) {
    return `
      <tr class="payslip-envelope-row">
        <td>${escapeHtml(item.periodLabel || item.referenceMonthShort || "—")}</td>
        <td>
          <button
            type="button"
            class="payslip-envelope-link"
            data-action="open-payslip-modal"
            data-payslip-id="${escapeHtml(item.id)}"
          >
            ${escapeHtml(item.paymentType || "FOLHA")}
          </button>
        </td>
        <td>${escapeHtml(formatPaymentDateShort(item.paymentDate))}</td>
        <td class="payslip-envelope-row__amount" data-sensitive-value>${escapeHtml(formatCurrency(item.netAmount))}</td>
        <td class="payslip-envelope-row__actions">
          <button
            type="button"
            class="comm-secondary-button"
            data-action="download-payslip-pdf-list"
            data-payslip-id="${escapeHtml(item.id)}"
            aria-label="Baixar PDF"
          >
            <i class="fa-solid fa-file-pdf" aria-hidden="true"></i>
          </button>
        </td>
      </tr>
    `;
  }

  return renderPageShell({
    title: data.title || "Envelope de pagamento",
    provider: data.provider,
    isSimulated: data.isSimulated,
    heroImage: "./assets/img/hero-holerite-perfil-rh.png",
    heroImageLabel: "Mesa financeira simbolizando holerite e remuneracao",
    bodyHtml: `
      ${renderContentCard({
        title: "Envelope de pagamento",
        bodyHtml: items.length
          ? `
            <div class="payslip-envelope-panel" data-payslip-values-visible="true">
              <div class="payslip-envelope-toolbar">
                <button
                  type="button"
                  class="comm-secondary-button payslip-envelope-toggle"
                  data-action="toggle-payslip-values"
                  aria-pressed="false"
                >
                  <i class="fa-solid fa-eye-slash" aria-hidden="true"></i>
                  Ocultar valores
                </button>
              </div>
              <div class="hr-profile-table-wrap payslip-envelope-table-wrap">
                <table class="hr-profile-table payslip-envelope-table">
                <thead>
                  <tr>
                    <th scope="col">Ref.</th>
                    <th scope="col">Tipo</th>
                    <th scope="col">Pgto.</th>
                    <th scope="col">Liquido</th>
                    <th scope="col"><span class="sr-only">Acoes</span></th>
                  </tr>
                </thead>
                <tbody>
                  ${years.map((year) => `
                    <tr class="payslip-envelope-year">
                      <td colspan="5">${escapeHtml(year)}</td>
                    </tr>
                    ${groupedByYear[year].map(renderEnvelopeRow).join("")}
                  `).join("")}
                </tbody>
              </table>
            </div>
            </div>
          `
          : renderEmptyState("Nenhum holerite", "Os envelopes liberados pelo RH aparecerao aqui.")
      })}
      ${renderPayslipModalShell()}
    `
  });
}

function renderBenefitsPage(data = {}) {
  const items = Array.isArray(data.items) ? data.items : [];

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    heroImage: "./assets/img/hero-beneficios-perfil-rh.png",
    heroImageLabel: "Elementos de bem-estar simbolizando beneficios corporativos",
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
    heroImage: "./assets/img/hero-avaliacao-perfil-rh.png",
    heroImageLabel: "Ambiente de feedback simbolizando avaliacao de desempenho",
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
    heroImage: "./assets/img/hero-cadastro-perfil-rh.png",
    heroImageLabel: "Documentos organizados simbolizando dados cadastrais do colaborador",
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

function renderTimesheetPage(data = {}, options = {}) {
  const summary = data.summary || {};
  const entries = Array.isArray(data.entries) ? data.entries : [];
  const userMessage = data.userMessage || data.UserMessage || "";
  const availabilityStatus = data.availabilityStatus || data.AvailabilityStatus || "ok";
  const periodOptions = Array.isArray(data.periodOptions || data.PeriodOptions)
    ? (data.periodOptions || data.PeriodOptions)
    : [];
  const selectedEndMonth = data.selectedPeriodEndMonth ?? data.SelectedPeriodEndMonth ?? options.timesheetPeriod?.month ?? null;
  const selectedEndYear = data.selectedPeriodEndYear ?? data.SelectedPeriodEndYear ?? options.timesheetPeriod?.year ?? null;
  const selectedPeriodValue = selectedEndMonth && selectedEndYear
    ? `${selectedEndYear}-${String(selectedEndMonth).padStart(2, "0")}`
    : "";

  const alertHtml = userMessage
    ? `<div class="ldap-wizard__alert ldap-wizard__alert--danger hr-profile-alert">${escapeHtml(userMessage)}</div>`
    : "";

  const periodSelectHtml = periodOptions.length
    ? `
      <select id="hr-timesheet-period-select" class="hr-profile-period-select" aria-label="Selecione o periodo de ponto">
        ${periodOptions.map((item) => {
          const endMonth = item.endMonth ?? item.EndMonth;
          const endYear = item.endYear ?? item.EndYear;
          const label = item.label || item.Label || "";
          const value = `${endYear}-${String(endMonth).padStart(2, "0")}`;
          const selected = value === selectedPeriodValue ? "selected" : "";
          return `<option value="${escapeHtml(value)}" data-end-month="${endMonth}" data-end-year="${endYear}" ${selected}>${escapeHtml(label)}</option>`;
        }).join("")}
      </select>
    `
    : "";

  const summaryCards = availabilityStatus === "ok"
    ? renderMetricCards([
      { label: "Saldo anterior", value: summary.previousBankBalance || summary.PreviousBankBalance || "—" },
      { label: "Saldo do periodo", value: summary.periodBankBalance || summary.PeriodBankBalance || "—" },
      { label: "Total banco", value: summary.totalBankBalance || summary.TotalBankBalance || "—" }
    ])
    : renderMetricCards([
      { label: "Saldo anterior", value: "—" },
      { label: "Saldo do periodo", value: "—" },
      { label: "Total banco", value: "—" }
    ]);

  return renderPageShell({
    title: data.title,
    provider: data.provider,
    isSimulated: data.isSimulated,
    heroImage: "./assets/img/hero-ponto-perfil-rh.png",
    heroImageLabel: "Relogio e ambiente corporativo simbolizando controle de ponto",
    bodyHtml: `
      ${alertHtml}
      ${renderContentCard({
        title: "Resumo do periodo",
        headerActionHtml: periodSelectHtml,
        bodyHtml: summaryCards
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
                    <th>Entrada 1</th>
                    <th>Saida 1</th>
                    <th>Entrada 2</th>
                    <th>Saida 2</th>
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
                      <td>${escapeHtml(item.lunchOut)}</td>
                      <td>${escapeHtml(item.lunchIn)}</td>
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
          : renderEmptyState(
            userMessage ? "Consulta indisponivel" : "Sem registros",
            userMessage || "Os apontamentos de ponto aparecerao aqui."
          )
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

export function renderHrProfileModulePage(slug, data = {}, options = {}) {
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

  return renderer(data, options);
}
