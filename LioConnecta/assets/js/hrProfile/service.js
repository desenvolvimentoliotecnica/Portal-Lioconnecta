import { getJson } from "../services/apiClient.js";
import { DATA_MODES, getRuntimeConfig, resolveApiEndpoint } from "../core/runtimeConfig.js?v=0.23.1";
import { getPortalAuthHeaders } from "../services/portalAuthService.js";
import { getHrProfileModule } from "./moduleCatalog.js";

const MOCK_PAYSLIP_ITEMS = Object.freeze([
  { id: "2026-06", periodLabel: "Junho/2026", referenceMonth: "2026-06", grossAmount: 11687.46, netAmount: 2686.42, paymentDate: "2026-06-30", status: "Disponivel", paymentType: "FOLHA", competenceYear: "2026", referenceMonthShort: "JUN." },
  { id: "2026-06-ADIANTAMENTO", periodLabel: "Junho/2026", referenceMonth: "2026-06", grossAmount: 4644.58, netAmount: 4644.58, paymentDate: "2026-06-15", status: "Disponivel", paymentType: "ADIANTAMENTO", competenceYear: "2026", referenceMonthShort: "JUN." },
  { id: "2026-05", periodLabel: "Maio/2026", referenceMonth: "2026-05", grossAmount: 8420.55, netAmount: 6780.69, paymentDate: "2026-05-30", status: "Disponivel", paymentType: "FOLHA", competenceYear: "2026", referenceMonthShort: "MAI." },
  { id: "2026-05-ADIANTAMENTO", periodLabel: "Maio/2026", referenceMonth: "2026-05", grossAmount: 4056.40, netAmount: 4056.40, paymentDate: "2026-05-15", status: "Disponivel", paymentType: "ADIANTAMENTO", competenceYear: "2026", referenceMonthShort: "MAI." }
]);

const MOCK_PAYSLIP_DETAILS = Object.freeze({
  "2026-06": {
    id: "2026-06",
    periodLabel: "Junho/2026",
    referenceMonth: "2026-06",
    grossAmount: 11687.46,
    netAmount: 2686.42,
    paymentDate: "2026-06-30",
    status: "Disponivel",
    paymentType: "FOLHA",
    paymentTypeTitle: "Pagamento em FOLHA",
    competenceTitle: "Junho 2026",
    companyName: "LIOTECNICA TECNOLOGIA EM ALIMENTOS S/A",
    companyCnpj: "12.345.678/0001-90",
    companyAddress: "Av. Paulista, 1000 - Bela Vista - Sao Paulo/SP",
    employeeName: "Colaborador Portal",
    employeeRegistration: "00012345",
    employeeCpf: "123.456.789-00",
    employeeRole: "Analista de Sistemas",
    employeeDepartment: "Tecnologia da Informacao",
    employeeAdmissionDate: "15/03/2019",
    bankName: "Banco Itau Unibanco S.A.",
    bankAgency: "1234",
    bankAccount: "56789-0",
    baseSalary: 9676.03,
    baseInss: 11687.46,
    baseFgts: 11687.46,
    baseIrrf: 11687.46,
    baseIrPlr: 0,
    fgtsAmount: 934.99,
    pensionAlimony: 0,
    earnings: [
      { code: "0001", description: "Horas Trabalhadas", reference: "—", amount: 9676.03 },
      { code: "0002", description: "DSR Horas Trabalhadas", reference: "—", amount: 1935.42 },
      { code: "101", description: "Hrs Extras Diurnas 60%", reference: "—", amount: 63.34 },
      { code: "37", description: "Repouso Remunerado Adicionais", reference: "—", amount: 12.67 }
    ],
    deductions: [
      { code: "290", description: "BS FU Refeicao Funcionario", reference: "—", amount: 65.66 },
      { code: "404", description: "Adiantamento Normal Desconto", reference: "—", amount: 4644.58 },
      { code: "511", description: "INSS Normal", reference: "—", amount: 988.07 },
      { code: "561", description: "IRF (Normal)", reference: "—", amount: 1981.47 }
    ],
    totalEarnings: 11687.46,
    totalDeductions: 9001.04,
    provider: "TOTVS RM",
    isSimulated: true
  },
  "2026-06-ADIANTAMENTO": {
    id: "2026-06-ADIANTAMENTO",
    periodLabel: "Junho/2026",
    referenceMonth: "2026-06",
    grossAmount: 4644.58,
    netAmount: 4644.58,
    paymentDate: "2026-06-15",
    status: "Disponivel",
    paymentType: "ADIANTAMENTO",
    paymentTypeTitle: "Pagamento em ADIANTAMENTO",
    competenceTitle: "Junho 2026",
    companyName: "LIOTECNICA TECNOLOGIA EM ALIMENTOS S/A",
    companyCnpj: "12.345.678/0001-90",
    companyAddress: "Av. Paulista, 1000 - Bela Vista - Sao Paulo/SP",
    employeeName: "Colaborador Portal",
    employeeRegistration: "00012345",
    employeeCpf: "123.456.789-00",
    employeeRole: "Analista de Sistemas",
    employeeDepartment: "Tecnologia da Informacao",
    employeeAdmissionDate: "15/03/2019",
    bankName: "Banco Itau Unibanco S.A.",
    bankAgency: "1234",
    bankAccount: "56789-0",
    baseSalary: 0,
    baseInss: 0,
    baseFgts: 0,
    baseIrrf: 0,
    baseIrPlr: 0,
    fgtsAmount: 0,
    pensionAlimony: 0,
    earnings: [
      { code: "401", description: "Adiantamento Normal Vencimento", reference: "—", amount: 4644.58 }
    ],
    deductions: [],
    totalEarnings: 4644.58,
    totalDeductions: 0,
    provider: "TOTVS RM",
    isSimulated: true
  },
  "2026-05": {
    id: "2026-05",
    periodLabel: "Maio/2026",
    referenceMonth: "2026-05",
    grossAmount: 8420.55,
    netAmount: 6234.18,
    paymentDate: "2026-05-30",
    status: "Disponivel",
    companyName: "LIOCONNECTA Tecnologia LTDA",
    companyCnpj: "12.345.678/0001-90",
    companyAddress: "Av. Paulista, 1000 - Bela Vista - Sao Paulo/SP",
    employeeName: "Colaborador Portal",
    employeeRegistration: "00012345",
    employeeCpf: "123.456.789-00",
    employeeRole: "Analista de Sistemas",
    employeeDepartment: "Tecnologia da Informacao",
    employeeAdmissionDate: "15/03/2019",
    bankName: "Banco Itau Unibanco S.A.",
    bankAgency: "1234",
    bankAccount: "56789-0",
    baseSalary: 7200,
    baseInss: 8420.55,
    baseFgts: 8420.55,
    fgtsAmount: 673.64,
    earnings: [
      { code: "001", description: "Salario base", reference: "30,00 d", amount: 7200 },
      { code: "020", description: "Horas extras 50%", reference: "8,00 h", amount: 480.55 },
      { code: "035", description: "Adicional noturno", reference: "12,00 h", amount: 240 },
      { code: "050", description: "Premio produtividade", reference: "—", amount: 500 }
    ],
    deductions: [
      { code: "101", description: "INSS", reference: "11,00 %", amount: 924.26 },
      { code: "102", description: "IRRF", reference: "15,00 %", amount: 612.11 },
      { code: "120", description: "Vale transporte", reference: "6,00 %", amount: 432 },
      { code: "130", description: "Plano de saude", reference: "—", amount: 218 }
    ],
    totalEarnings: 8420.55,
    totalDeductions: 2186.37,
    provider: "TOTVS RM",
    isSimulated: true
  }
});

export async function getHrProfileModuleData(slug, options = {}) {
  const module = getHrProfileModule(slug);
  if (!module) {
    throw new Error("Modulo de perfil RH invalido.");
  }

  const config = getRuntimeConfig();
  if (config.dataMode !== DATA_MODES.API) {
    return buildMockPayload(slug);
  }

  const endpoint = resolveApiEndpoint(module.endpointKey);
  const query = slug === "ponto" && options.month && options.year
    ? `?month=${encodeURIComponent(options.month)}&year=${encodeURIComponent(options.year)}`
    : "";

  const payload = await getJson(`${endpoint}${query}`, {
    headers: options.headers || getPortalAuthHeaders(),
    ...options
  });

  if (slug === "holerite" && Array.isArray(payload.items || payload.Items)) {
    return {
      ...payload,
      title: payload.title || payload.Title || "Envelope de pagamento",
      items: (payload.items || payload.Items).map(normalizePayslipItem)
    };
  }

  return payload;
}

function buildMockPayload(slug) {
  const module = getHrProfileModule(slug);
  const base = {
    title: module?.label || "Perfil RH",
    provider: "TOTVS RM",
    isSimulated: true
  };

  if (slug === "holerite") {
    return {
      ...base,
      title: "Envelope de pagamento",
      items: MOCK_PAYSLIP_ITEMS.map((item) => normalizePayslipItem(item))
    };
  }

  return base;
}

function inferPaymentType(item = {}, rawId = "") {
  const explicit = item.paymentType || item.PaymentType;
  if (explicit) {
    return explicit;
  }

  const id = String(rawId || item.id || item.Id || "");
  if (id.toUpperCase().includes("ADIANTAMENTO")) {
    return "ADIANTAMENTO";
  }

  if (/^\d{4}-\d{2}$/.test(id)) {
    return "FOLHA";
  }

  const gross = Number(item.grossAmount ?? item.GrossAmount ?? 0);
  const net = Number(item.netAmount ?? item.NetAmount ?? 0);
  const paymentDate = new Date(item.paymentDate || item.PaymentDate || "");
  const hasNoDeductions = gross > 0 && Math.abs(gross - net) < 0.01;
  const isMidMonthPayment = !Number.isNaN(paymentDate.getTime()) && paymentDate.getDate() <= 20;

  if (hasNoDeductions && isMidMonthPayment) {
    return "ADIANTAMENTO";
  }

  return "FOLHA";
}

function normalizePayslipListId(rawId) {
  const legacyMatch = /^(\d{4}-\d{2})-(\d+)$/.exec(String(rawId || ""));
  if (legacyMatch && Number(legacyMatch[2]) > 1) {
    return `${legacyMatch[1]}-ADIANTAMENTO`;
  }

  return rawId || "";
}

function normalizePayslipItem(item = {}) {
  const rawId = item.id || item.Id || "";
  const paymentType = inferPaymentType(item, rawId);
  const anoComp = Number(item.competenceYear || item.CompetenceYear || String(item.referenceMonth || item.ReferenceMonth || "").split("-")[0] || 0);
  const mesComp = Number(String(item.referenceMonth || item.ReferenceMonth || "").split("-")[1] || 0);
  const id = rawId
    ? normalizePayslipListId(rawId)
    : HrPayslipFallbackId(anoComp, mesComp, paymentType);

  return {
    id,
    periodLabel: item.periodLabel || item.PeriodLabel || "",
    referenceMonth: item.referenceMonth || item.ReferenceMonth || "",
    grossAmount: item.grossAmount ?? item.GrossAmount ?? 0,
    netAmount: item.netAmount ?? item.NetAmount ?? 0,
    paymentDate: item.paymentDate || item.PaymentDate || "",
    status: item.status || item.Status || "",
    paymentType,
    competenceYear: item.competenceYear || item.CompetenceYear || (anoComp ? String(anoComp) : ""),
    referenceMonthShort: item.referenceMonthShort || item.ReferenceMonthShort || ""
  };
}

function HrPayslipFallbackId(anoComp, mesComp, paymentType) {
  if (!anoComp || !mesComp) {
    return "";
  }

  if (paymentType === "ADIANTAMENTO") {
    return `${anoComp}-${String(mesComp).padStart(2, "0")}-ADIANTAMENTO`;
  }

  return `${anoComp}-${String(mesComp).padStart(2, "0")}`;
}

function normalizePayslipDetail(payload = {}) {
  return {
    id: payload.id || payload.Id || "",
    periodLabel: payload.periodLabel || payload.PeriodLabel || "",
    referenceMonth: payload.referenceMonth || payload.ReferenceMonth || "",
    grossAmount: payload.grossAmount ?? payload.GrossAmount ?? 0,
    netAmount: payload.netAmount ?? payload.NetAmount ?? 0,
    paymentDate: payload.paymentDate || payload.PaymentDate || "",
    status: payload.status || payload.Status || "",
    paymentType: payload.paymentType || payload.PaymentType || "FOLHA",
    paymentTypeTitle: payload.paymentTypeTitle || payload.PaymentTypeTitle || "Pagamento em FOLHA",
    competenceTitle: payload.competenceTitle || payload.CompetenceTitle || payload.periodLabel || payload.PeriodLabel || "",
    companyName: payload.companyName || payload.CompanyName || "",
    companyCnpj: payload.companyCnpj || payload.CompanyCnpj || "",
    companyAddress: payload.companyAddress || payload.CompanyAddress || "",
    employeeName: payload.employeeName || payload.EmployeeName || "",
    employeeRegistration: payload.employeeRegistration || payload.EmployeeRegistration || "",
    employeeCpf: payload.employeeCpf || payload.EmployeeCpf || "",
    employeeRole: payload.employeeRole || payload.EmployeeRole || "",
    employeeDepartment: payload.employeeDepartment || payload.EmployeeDepartment || "",
    employeeAdmissionDate: payload.employeeAdmissionDate || payload.EmployeeAdmissionDate || "",
    bankName: payload.bankName || payload.BankName || "",
    bankAgency: payload.bankAgency || payload.BankAgency || "",
    bankAccount: payload.bankAccount || payload.BankAccount || "",
    baseSalary: payload.baseSalary ?? payload.BaseSalary ?? 0,
    baseInss: payload.baseInss ?? payload.BaseInss ?? 0,
    baseFgts: payload.baseFgts ?? payload.BaseFgts ?? 0,
    baseIrrf: payload.baseIrrf ?? payload.BaseIrrf ?? 0,
    baseIrPlr: payload.baseIrPlr ?? payload.BaseIrPlr ?? 0,
    fgtsAmount: payload.fgtsAmount ?? payload.FgtsAmount ?? 0,
    pensionAlimony: payload.pensionAlimony ?? payload.PensionAlimony ?? 0,
    earnings: (payload.earnings || payload.Earnings || []).map((line) => ({
      code: line.code || line.Code || "",
      description: line.description || line.Description || "",
      reference: line.reference || line.Reference || "",
      amount: line.amount ?? line.Amount ?? 0
    })),
    deductions: (payload.deductions || payload.Deductions || []).map((line) => ({
      code: line.code || line.Code || "",
      description: line.description || line.Description || "",
      reference: line.reference || line.Reference || "",
      amount: line.amount ?? line.Amount ?? 0
    })),
    totalEarnings: payload.totalEarnings ?? payload.TotalEarnings ?? 0,
    totalDeductions: payload.totalDeductions ?? payload.TotalDeductions ?? 0,
    provider: payload.provider || payload.Provider || "TOTVS RM",
    isSimulated: payload.isSimulated ?? payload.IsSimulated ?? true
  };
}

function resolvePayslipDetailId(payslipId) {
  const legacyMatch = /^(\d{4}-\d{2})-(\d+)$/.exec(String(payslipId || ""));
  if (legacyMatch && Number(legacyMatch[2]) > 1) {
    return `${legacyMatch[1]}-ADIANTAMENTO`;
  }

  return payslipId;
}

function buildPayslipDetailCandidates(payslipId) {
  const normalizedId = resolvePayslipDetailId(payslipId);
  const baseMonthId = String(normalizedId || payslipId || "").replace(/-ADIANTAMENTO$/i, "");
  const monthMatch = /^(\d{4}-\d{2})$/.exec(baseMonthId);
  const periodCandidates = monthMatch
    ? [1, 2, 3, 4].map((period) => `${monthMatch[1]}-${period}`)
    : [];

  return [...new Set([
    normalizedId,
    payslipId,
    baseMonthId,
    ...periodCandidates
  ].filter(Boolean))];
}

export async function getPayslipDetail(payslipId, options = {}) {
  const config = getRuntimeConfig();
  const candidates = buildPayslipDetailCandidates(payslipId);

  if (config.dataMode !== DATA_MODES.API) {
    const mock = candidates
      .map((candidateId) => MOCK_PAYSLIP_DETAILS[candidateId])
      .find(Boolean)
      || MOCK_PAYSLIP_DETAILS["2026-05"];
    return normalizePayslipDetail({
      ...mock,
      id: candidates[0] || payslipId || mock.id
    });
  }

  let lastError;
  for (const candidateId of candidates) {
    try {
      const endpoint = `${resolveApiEndpoint("hrHolerite")}/${encodeURIComponent(candidateId)}`;
      const payload = await getJson(endpoint, {
        headers: getPortalAuthHeaders(),
        ...options
      });

      return normalizePayslipDetail({
        ...payload,
        id: payload.id || payload.Id || candidateId
      });
    } catch (error) {
      lastError = error;
    }
  }

  throw lastError ?? new Error("Holerite nao encontrado.");
}
