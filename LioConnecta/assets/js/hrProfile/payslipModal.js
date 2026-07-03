import { escapeHtml } from "../components/html.js";
import { showToast } from "../core/feedback.js";
import { getPayslipDetail } from "./service.js";
import {
  buildPayslipFilename,
  createPayslipExportHost,
  downloadPayslipPdf,
  getPayslipBuildLabel,
  printPayslipDocument
} from "./payslipExport.js";

let currentPayslipDetail = null;

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

function renderPayslipLinesTable(lines = [], type = "earning") {
  const items = Array.isArray(lines) ? lines : [];
  const toneClass = type === "earning" ? "payslip-doc__table--earnings" : "payslip-doc__table--deductions";

  return `
    <table class="payslip-doc__table ${toneClass}">
      <thead>
        <tr>
          <th scope="col">Cod.</th>
          <th scope="col">Descricao</th>
          <th scope="col">Referencia</th>
          <th scope="col" class="payslip-doc__amount-col">Valor</th>
        </tr>
      </thead>
      <tbody>
        ${items.length
          ? items.map((line) => `
            <tr>
              <td>${escapeHtml(line.code)}</td>
              <td>${escapeHtml(line.description)}</td>
              <td>${escapeHtml(line.reference)}</td>
              <td class="payslip-doc__amount-col">${escapeHtml(formatCurrency(line.amount))}</td>
            </tr>
          `).join("")
          : `<tr><td colspan="4" class="payslip-doc__empty-row">Sem lancamentos</td></tr>`}
      </tbody>
    </table>
  `;
}

function formatSignedCurrency(value, isDeduction = false) {
  const amount = Number(value ?? 0);
  const formatted = amount.toLocaleString("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  if (isDeduction && amount > 0) {
    return `-${formatted}`;
  }
  return formatted;
}

function renderRmLineItem(line, isDeduction = false) {
  const label = `${line.code}-${line.description}`;
  return `
    <div class="payslip-rm-line">
      <span>${escapeHtml(label)}</span>
      <strong data-sensitive-value>${escapeHtml(formatSignedCurrency(line.amount, isDeduction))}</strong>
    </div>
  `;
}

export function renderRmPayslipDetailView(detail = {}) {
  const earnings = Array.isArray(detail.earnings) ? detail.earnings : [];
  const deductions = Array.isArray(detail.deductions) ? detail.deductions : [];
  const totalEarnings = detail.totalEarnings ?? earnings.reduce((sum, line) => sum + Number(line.amount || 0), 0);
  const totalDeductions = detail.totalDeductions ?? deductions.reduce((sum, line) => sum + Number(line.amount || 0), 0);

  return `
    <div class="payslip-rm" data-payslip-values-visible="false">
      <header class="payslip-rm__header">
        <div>
          <h2>${escapeHtml(detail.competenceTitle || detail.periodLabel || "Holerite")}</h2>
          <p class="payslip-rm__payment-title">${escapeHtml(detail.paymentTypeTitle || "Pagamento em FOLHA")}</p>
          <p class="payslip-rm__payment-date">Realizado em ${escapeHtml(formatDate(detail.paymentDate))}</p>
        </div>
        <div class="payslip-rm__header-actions">
          <button type="button" class="comm-secondary-button" data-action="download-payslip-pdf">
            <i class="fa-solid fa-download" aria-hidden="true"></i>
          </button>
          <button type="button" class="comm-secondary-button" data-action="print-payslip">
            <i class="fa-solid fa-share-nodes" aria-hidden="true"></i>
          </button>
        </div>
      </header>

      <button
        type="button"
        class="comm-secondary-button payslip-envelope-toggle payslip-rm__toggle"
        data-action="toggle-payslip-values"
        aria-pressed="false"
      >
        <i class="fa-solid fa-eye-slash" aria-hidden="true"></i>
        Ocultar valores
      </button>

      <section class="payslip-rm-card payslip-rm-card--earnings">
        <div class="payslip-rm-card__head">
          <strong>Proventos</strong>
          <span data-sensitive-value>${escapeHtml(formatSignedCurrency(totalEarnings))}</span>
        </div>
        <div class="payslip-rm-card__body">
          ${earnings.length
            ? earnings.map((line) => renderRmLineItem(line, false)).join("")
            : `<p class="payslip-rm-card__empty">Sem proventos</p>`}
        </div>
      </section>

      <section class="payslip-rm-card payslip-rm-card--deductions">
        <div class="payslip-rm-card__head">
          <strong>Descontos</strong>
          <span data-sensitive-value>${escapeHtml(totalDeductions > 0 ? `-${formatSignedCurrency(totalDeductions)}` : "0,00")}</span>
        </div>
        <div class="payslip-rm-card__body">
          ${deductions.length
            ? deductions.map((line) => renderRmLineItem(line, true)).join("")
            : `<p class="payslip-rm-card__empty">0,00</p>`}
        </div>
      </section>

      <section class="payslip-rm-card payslip-rm-card--net">
        <div class="payslip-rm-card__head">
          <strong>Liquido</strong>
          <span data-sensitive-value>${escapeHtml(formatSignedCurrency(detail.netAmount))}</span>
        </div>
      </section>

      <section class="payslip-rm-bases">
        ${[
          ["Base para FGTS", detail.baseFgts],
          ["Base calc. IRRF", detail.baseIrrf ?? detail.baseFgts],
          ["Base IR PLR Anual", detail.baseIrPlr],
          ["Sal. Contribuicao INSS", detail.baseInss],
          ["FGTS do mes", detail.fgtsAmount],
          ["Pensao Alimenticia Judicial", detail.pensionAlimony]
        ].map(([label, value]) => `
          <article class="payslip-rm-card payslip-rm-card--base">
            <span>${escapeHtml(label)}</span>
            <strong data-sensitive-value>${escapeHtml(formatSignedCurrency(value))}</strong>
          </article>
        `).join("")}
      </section>
    </div>
  `;
}

export function renderPayslipDocument(detail = {}) {
  const earnings = Array.isArray(detail.earnings) ? detail.earnings : [];
  const deductions = Array.isArray(detail.deductions) ? detail.deductions : [];

  return `
    <article class="payslip-doc" aria-label="Recibo de pagamento ${escapeHtml(detail.periodLabel || "")}">
      <header class="payslip-doc__header">
        <div class="payslip-doc__company">
          <strong>${escapeHtml(detail.companyName || "Empresa")}</strong>
          <span>CNPJ ${escapeHtml(detail.companyCnpj || "—")}</span>
          <span>${escapeHtml(detail.companyAddress || "")}</span>
        </div>
        <div class="payslip-doc__title-block">
          <span class="payslip-doc__eyebrow">Recibo de pagamento de salario</span>
          <strong>${escapeHtml(detail.periodLabel || "—")}</strong>
          <span>Competencia ${escapeHtml(detail.referenceMonth || "—")}</span>
        </div>
      </header>

      <section class="payslip-doc__employee">
        <div class="payslip-doc__field">
          <span>Funcionario</span>
          <strong>${escapeHtml(detail.employeeName || "—")}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>Matricula</span>
          <strong>${escapeHtml(detail.employeeRegistration || "—")}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>CPF</span>
          <strong>${escapeHtml(detail.employeeCpf || "—")}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>Cargo</span>
          <strong>${escapeHtml(detail.employeeRole || "—")}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>Departamento</span>
          <strong>${escapeHtml(detail.employeeDepartment || "—")}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>Admissao</span>
          <strong>${escapeHtml(detail.employeeAdmissionDate || "—")}</strong>
        </div>
      </section>

      <div class="payslip-doc__columns">
        <section class="payslip-doc__column">
          <h3>Proventos</h3>
          ${renderPayslipLinesTable(earnings, "earning")}
        </section>
        <section class="payslip-doc__column">
          <h3>Descontos</h3>
          ${renderPayslipLinesTable(deductions, "deduction")}
        </section>
      </div>

      <section class="payslip-doc__bases">
        <div class="payslip-doc__field">
          <span>Salario base</span>
          <strong>${escapeHtml(formatCurrency(detail.baseSalary))}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>Base INSS</span>
          <strong>${escapeHtml(formatCurrency(detail.baseInss))}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>Base FGTS</span>
          <strong>${escapeHtml(formatCurrency(detail.baseFgts))}</strong>
        </div>
        <div class="payslip-doc__field">
          <span>FGTS do mes</span>
          <strong>${escapeHtml(formatCurrency(detail.fgtsAmount))}</strong>
        </div>
      </section>

      <footer class="payslip-doc__footer">
        <div class="payslip-doc__totals">
          <div class="payslip-doc__total-row">
            <span>Total de proventos</span>
            <strong>${escapeHtml(formatCurrency(detail.totalEarnings))}</strong>
          </div>
          <div class="payslip-doc__total-row">
            <span>Total de descontos</span>
            <strong>${escapeHtml(formatCurrency(detail.totalDeductions))}</strong>
          </div>
          <div class="payslip-doc__total-row payslip-doc__total-row--net">
            <span>Valor liquido a receber</span>
            <strong>${escapeHtml(formatCurrency(detail.netAmount))}</strong>
          </div>
        </div>
        <div class="payslip-doc__payment">
          <div class="payslip-doc__field">
            <span>Banco</span>
            <strong>${escapeHtml(detail.bankName || "—")}</strong>
          </div>
          <div class="payslip-doc__field">
            <span>Agencia / Conta</span>
            <strong>${escapeHtml(detail.bankAgency || "—")} / ${escapeHtml(detail.bankAccount || "—")}</strong>
          </div>
          <div class="payslip-doc__field">
            <span>Data de pagamento</span>
            <strong>${escapeHtml(formatDate(detail.paymentDate))}</strong>
          </div>
        </div>
      </footer>

      ${detail.isSimulated ? `<p class="payslip-doc__simulated">Documento simulado para validacao da experiencia.</p>` : ""}
    </article>
  `;
}

export function renderPayslipModalShell() {
  return `
    <div class="payslip-modal" id="payslip-modal" hidden aria-hidden="true">
      <div class="payslip-modal__dialog card" role="dialog" aria-modal="true" aria-labelledby="payslip-modal-title">
        <div class="card-header payslip-modal__header">
          <div class="payslip-modal__title-block">
            <strong id="payslip-modal-title">Holerite</strong>
            <span id="payslip-modal-subtitle">Visualizacao do comprovante</span>
          </div>
          <div class="payslip-modal__actions">
            <button type="button" class="comm-secondary-button" data-action="print-payslip" disabled>
              <i class="fa-solid fa-print" aria-hidden="true"></i>
              Imprimir
            </button>
            <button type="button" class="comm-secondary-button" data-action="download-payslip-pdf" disabled>
              <i class="fa-solid fa-file-pdf" aria-hidden="true"></i>
              Baixar PDF
            </button>
            <button
              type="button"
              class="comm-tertiary-button payslip-modal__close"
              data-action="close-payslip-modal"
              aria-label="Fechar holerite"
            >
              <i class="fa-solid fa-xmark" aria-hidden="true"></i>
            </button>
          </div>
        </div>
        <div class="payslip-modal__body" id="payslip-modal-body">
          <div class="payslip-modal__loading">Carregando holerite...</div>
        </div>
      </div>
    </div>
  `;
}

function setPayslipActionsEnabled(modal, enabled) {
  if (!modal) {
    return;
  }

  modal.querySelectorAll("[data-action='print-payslip'], [data-action='download-payslip-pdf']").forEach((button) => {
    button.disabled = !enabled;
  });
}

function getModalElements(root = document) {
  return {
    modal: root.getElementById("payslip-modal"),
    body: root.getElementById("payslip-modal-body"),
    title: root.getElementById("payslip-modal-title"),
    subtitle: root.getElementById("payslip-modal-subtitle")
  };
}

async function handleDownloadPayslipPdf(payslipId, root = document) {
  try {
    const detail = payslipId && payslipId === currentPayslipDetail?.id
      ? currentPayslipDetail
      : await getPayslipDetail(payslipId);

    const host = createPayslipExportHost(detail, renderPayslipDocument, root);
    const documentNode = host.querySelector(".payslip-doc");
    await downloadPayslipPdf(documentNode, buildPayslipFilename(detail), root);
    host.remove();
    showToast("PDF do holerite gerado com sucesso.", "success");
  } catch {
    showToast("Nao foi possivel gerar o PDF do holerite.", "danger");
  }
}

function handlePrintPayslip(root = document) {
  printPayslipDocument(root).catch(() => {
    showToast("Nao foi possivel imprimir o holerite.", "danger");
  });
}

export function closePayslipModal(root = document) {
  const { modal } = getModalElements(root);
  if (!modal) {
    return;
  }

  modal.hidden = true;
  modal.setAttribute("aria-hidden", "true");
  root.body.classList.remove("modal-open");
  currentPayslipDetail = null;
  setPayslipActionsEnabled(modal, false);
}

export async function openPayslipModal(payslipId, root = document) {
  const { modal, body, title, subtitle } = getModalElements(root);
  if (!modal || !body) {
    return;
  }

  modal.hidden = false;
  modal.setAttribute("aria-hidden", "false");
  root.body.classList.add("modal-open");
  currentPayslipDetail = null;
  setPayslipActionsEnabled(modal, false);
  body.innerHTML = `<div class="payslip-modal__loading">Carregando holerite...</div>`;

  try {
    const detail = await getPayslipDetail(payslipId);
    currentPayslipDetail = detail;
    if (title) {
      title.textContent = `Holerite ${detail.periodLabel || ""}`.trim();
    }
    if (subtitle) {
      subtitle.textContent = `${detail.paymentTypeTitle || "Pagamento"} • ${formatDate(detail.paymentDate)} • ${getPayslipBuildLabel()}`;
    }
    body.innerHTML = renderRmPayslipDetailView(detail);
    setPayslipValuesVisible(root, arePayslipValuesVisible(root));
    bindPayslipValueToggle(root);
    setPayslipActionsEnabled(modal, true);
  } catch {
    currentPayslipDetail = null;
    setPayslipActionsEnabled(modal, false);
    body.innerHTML = `
      <div class="payslip-modal__error">
        <strong>Nao foi possivel carregar o holerite.</strong>
        <p>Tente novamente em instantes ou entre em contato com o RH.</p>
        <button type="button" class="comm-secondary-button" data-action="close-payslip-modal">Fechar</button>
      </div>
    `;
  }
}

const PAYSLIP_VALUES_STORAGE_KEY = "payslipValuesVisible";

function readStoredPayslipValuesVisible() {
  try {
    return sessionStorage.getItem(PAYSLIP_VALUES_STORAGE_KEY) === "true";
  } catch {
    return false;
  }
}

function writeStoredPayslipValuesVisible(visible) {
  try {
    sessionStorage.setItem(PAYSLIP_VALUES_STORAGE_KEY, visible ? "true" : "false");
  } catch {
    // Ignora indisponibilidade de storage no navegador.
  }
}

export function arePayslipValuesVisible(root = document) {
  return root.body?.dataset?.payslipValuesVisible !== "false";
}

export function initPayslipValuesPrivacy(root = document) {
  setPayslipValuesVisible(root, readStoredPayslipValuesVisible());
}

function findPayslipValuesScope(button, root) {
  return button.closest("#center-content")
    || button.closest("#payslip-modal-body")
    || root;
}

function setPayslipValuesVisible(root, visible) {
  if (root.body) {
    root.body.dataset.payslipValuesVisible = visible ? "true" : "false";
  }

  root.querySelectorAll("[data-payslip-values-visible]").forEach((container) => {
    container.dataset.payslipValuesVisible = visible ? "true" : "false";
  });

  writeStoredPayslipValuesVisible(visible);

  root.querySelectorAll("[data-action='toggle-payslip-values']").forEach((button) => {
    button.setAttribute("aria-pressed", visible ? "false" : "true");
    button.innerHTML = visible
      ? `<i class="fa-solid fa-eye-slash" aria-hidden="true"></i> Ocultar valores`
      : `<i class="fa-solid fa-eye" aria-hidden="true"></i> Mostrar valores`;
  });
}

function bindPayslipValueToggle(root = document) {
  root.querySelectorAll("[data-action='toggle-payslip-values']").forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", () => {
      setPayslipValuesVisible(root, !arePayslipValuesVisible(root));
    });
  });

  setPayslipValuesVisible(root, arePayslipValuesVisible(root));
}

export function bindPayslipModal(root = document) {
  const centerContent = root.getElementById("center-content");
  const { modal } = getModalElements(root);

  if (!centerContent || !modal) {
    return;
  }

  centerContent.querySelectorAll("[data-action='open-payslip-modal']").forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", () => {
      const payslipId = button.getAttribute("data-payslip-id") || "";
      if (payslipId) {
        openPayslipModal(payslipId, root);
      }
    });
  });

  centerContent.querySelectorAll("[data-action='download-payslip-pdf-list']").forEach((button) => {
    if (button.dataset.bound === "true") {
      return;
    }

    button.dataset.bound = "true";
    button.addEventListener("click", () => {
      const payslipId = button.getAttribute("data-payslip-id") || "";
      if (payslipId) {
        handleDownloadPayslipPdf(payslipId, root);
      }
    });
  });

  bindPayslipValueToggle(root);

  if (modal.dataset.bound === "true") {
    return;
  }

  modal.dataset.bound = "true";

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closePayslipModal(root);
      return;
    }

    if (event.target.closest("[data-action='close-payslip-modal']")) {
      closePayslipModal(root);
      return;
    }

    if (event.target.closest("[data-action='print-payslip']")) {
      handlePrintPayslip(root);
      return;
    }

    if (event.target.closest("[data-action='download-payslip-pdf']")) {
      const payslipId = currentPayslipDetail?.id || "";
      if (payslipId) {
        handleDownloadPayslipPdf(payslipId, root);
      }
    }
  });

  root.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && !modal.hidden) {
      closePayslipModal(root);
    }
  });
}
