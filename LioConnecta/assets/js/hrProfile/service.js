import { getJson } from "../services/apiClient.js";
import { DATA_MODES, getRuntimeConfig, resolveApiEndpoint } from "../core/runtimeConfig.js?v=0.23.1";
import { getPortalAuthHeaders } from "../services/portalAuthService.js";
import { getHrProfileModule } from "./moduleCatalog.js";

const MOCK_PAYSLIP_ITEMS = Object.freeze([
  { id: "2026-05", periodLabel: "Maio/2026", referenceMonth: "2026-05", grossAmount: 8420.55, netAmount: 6234.18, paymentDate: "2026-05-30", status: "Disponivel" },
  { id: "2026-04", periodLabel: "Abril/2026", referenceMonth: "2026-04", grossAmount: 8420.55, netAmount: 6188.42, paymentDate: "2026-04-30", status: "Disponivel" },
  { id: "2026-03", periodLabel: "Marco/2026", referenceMonth: "2026-03", grossAmount: 8420.55, netAmount: 6201.07, paymentDate: "2026-03-31", status: "Disponivel" },
  { id: "2026-02", periodLabel: "Fevereiro/2026", referenceMonth: "2026-02", grossAmount: 8420.55, netAmount: 6195.33, paymentDate: "2026-02-28", status: "Disponivel" }
]);

const MOCK_PAYSLIP_DETAILS = Object.freeze({
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
  return getJson(endpoint, {
    headers: getPortalAuthHeaders(),
    ...options
  });
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
      items: MOCK_PAYSLIP_ITEMS.map((item) => ({ ...item }))
    };
  }

  return base;
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
    fgtsAmount: payload.fgtsAmount ?? payload.FgtsAmount ?? 0,
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

export async function getPayslipDetail(payslipId, options = {}) {
  const config = getRuntimeConfig();
  if (config.dataMode !== DATA_MODES.API) {
    const mock = MOCK_PAYSLIP_DETAILS[payslipId] || MOCK_PAYSLIP_DETAILS["2026-05"];
    return normalizePayslipDetail({
      ...mock,
      id: payslipId || mock.id
    });
  }

  const endpoint = `${resolveApiEndpoint("hrHolerite")}/${encodeURIComponent(payslipId)}`;
  const payload = await getJson(endpoint, {
    headers: getPortalAuthHeaders(),
    ...options
  });

  return normalizePayslipDetail(payload);
}
