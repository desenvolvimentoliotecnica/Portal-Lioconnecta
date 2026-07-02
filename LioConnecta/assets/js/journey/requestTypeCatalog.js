export const REQUEST_CATEGORIES = Object.freeze([
  { key: "rh", label: "RH" },
  { key: "ti", label: "TI / Service Desk" },
  { key: "financeiro", label: "Financeiro" },
  { key: "facilities", label: "Facilities" },
  { key: "geral", label: "Geral" }
]);

const COMMON_FIELDS = Object.freeze([
  {
    name: "subject",
    label: "Assunto",
    type: "text",
    required: true,
    placeholder: "Ex.: Solicitacao de 10 dias em julho/2026",
    fullWidth: false
  },
  {
    name: "priority",
    label: "Prioridade",
    type: "select",
    required: true,
    options: ["Baixa", "Media", "Alta"],
    fullWidth: false
  },
  {
    name: "description",
    label: "Descricao",
    type: "textarea",
    required: true,
    placeholder: "Descreva os detalhes da sua solicitacao...",
    rows: 4,
    fullWidth: true
  }
]);

export const REQUEST_TYPES = Object.freeze({
  "rh-ferias": {
    key: "rh-ferias",
    categoryKey: "rh",
    label: "Ferias",
    description: "Solicitar periodo de ferias ou abono.",
    icon: "fa-solid fa-umbrella-beach",
    listTypeLabel: "Ferias",
    defaultStage: "Aprovacao do gestor",
    defaultStatus: "Em analise",
    fields: [
      { name: "startDate", label: "Data inicio", type: "date", required: true },
      { name: "endDate", label: "Data fim", type: "date", required: true }
    ]
  },
  "rh-reembolso": {
    key: "rh-reembolso",
    categoryKey: "rh",
    label: "Reembolso",
    description: "Despesas corporativas e reembolsos de viagem.",
    icon: "fa-solid fa-receipt",
    listTypeLabel: "Reembolso",
    defaultStage: "Validacao financeira",
    defaultStatus: "Em andamento",
    fields: [
      { name: "amount", label: "Valor (R$)", type: "number", required: true, placeholder: "0,00", step: "0.01", min: "0" },
      { name: "expenseDate", label: "Data da despesa", type: "date", required: true }
    ]
  },
  "rh-atestado": {
    key: "rh-atestado",
    categoryKey: "rh",
    label: "Atestado medico",
    description: "Registro de afastamento por motivo de saude.",
    icon: "fa-solid fa-file-medical",
    listTypeLabel: "Atestado medico",
    defaultStage: "Conferencia RH",
    defaultStatus: "Aguardando",
    fields: [
      { name: "startDate", label: "Data inicio", type: "date", required: true },
      { name: "endDate", label: "Data fim", type: "date", required: true },
      { name: "attachment", label: "Anexo (opcional)", type: "file", required: false, accept: ".pdf,.jpg,.jpeg,.png", helper: "PDF ou imagem ate 5 MB (simulacao)." }
    ]
  },
  "ti-chamado": {
    key: "ti-chamado",
    categoryKey: "ti",
    label: "Chamado / Service Desk",
    description: "Incidentes, duvidas e solicitacoes de suporte de TI.",
    icon: "fa-solid fa-headset",
    listTypeLabel: "Chamado TI",
    defaultStage: "Triagem de TI",
    defaultStatus: "Aguardando",
    fields: [
      {
        name: "category",
        label: "Categoria",
        type: "select",
        required: true,
        options: ["Hardware", "Software", "Acesso", "Rede"]
      },
      {
        name: "urgency",
        label: "Urgencia",
        type: "select",
        required: true,
        options: ["Baixa", "Media", "Alta", "Critica"]
      }
    ]
  },
  "ti-equipamento": {
    key: "ti-equipamento",
    categoryKey: "ti",
    label: "Equipamento",
    description: "Troca, manutencao ou solicitacao de equipamentos.",
    icon: "fa-solid fa-laptop",
    listTypeLabel: "Equipamento",
    defaultStage: "Triagem de TI",
    defaultStatus: "Aguardando",
    fields: [
      {
        name: "equipmentType",
        label: "Tipo de equipamento",
        type: "select",
        required: true,
        options: ["Notebook", "Monitor", "Celular", "Periferico", "Outro"]
      },
      { name: "justification", label: "Justificativa", type: "textarea", required: true, rows: 3, fullWidth: true }
    ]
  },
  "ti-acesso": {
    key: "ti-acesso",
    categoryKey: "ti",
    label: "Acesso a sistema",
    description: "Liberacao ou alteracao de acesso a sistemas.",
    icon: "fa-solid fa-key",
    listTypeLabel: "Acesso a sistema",
    defaultStage: "Aprovacao do gestor",
    defaultStatus: "Em analise",
    fields: [
      { name: "systemName", label: "Sistema", type: "text", required: true, placeholder: "Ex.: SAP, Salesforce" },
      { name: "desiredProfile", label: "Perfil desejado", type: "text", required: true, placeholder: "Ex.: Consultor, Aprovador" }
    ]
  },
  "financeiro-nf": {
    key: "financeiro-nf",
    categoryKey: "financeiro",
    label: "Nota fiscal / pagamento",
    description: "Envio de NF e solicitacoes de pagamento.",
    icon: "fa-solid fa-file-invoice-dollar",
    listTypeLabel: "Nota fiscal",
    defaultStage: "Validacao financeira",
    defaultStatus: "Em andamento",
    fields: [
      { name: "amount", label: "Valor (R$)", type: "number", required: true, placeholder: "0,00", step: "0.01", min: "0" },
      { name: "costCenter", label: "Centro de custo", type: "text", required: true, placeholder: "Ex.: CC-1234" }
    ]
  },
  "facilities-manutencao": {
    key: "facilities-manutencao",
    categoryKey: "facilities",
    label: "Manutencao / espaco",
    description: "Problemas estruturais, salas e areas comuns.",
    icon: "fa-solid fa-screwdriver-wrench",
    listTypeLabel: "Manutencao",
    defaultStage: "Triagem Facilities",
    defaultStatus: "Aguardando",
    fields: [
      { name: "location", label: "Local", type: "text", required: true, placeholder: "Ex.: Sala 302, Andar 3" },
      {
        name: "problemType",
        label: "Tipo de problema",
        type: "select",
        required: true,
        options: ["Ar-condicionado", "Iluminacao", "Mobiliario", "Limpeza", "Outro"]
      }
    ]
  },
  "geral-outros": {
    key: "geral-outros",
    categoryKey: "geral",
    label: "Outros",
    description: "Demais solicitacoes que nao se encaixam nas categorias.",
    icon: "fa-solid fa-circle-question",
    listTypeLabel: "Outros",
    defaultStage: "Triagem inicial",
    defaultStatus: "Aguardando",
    fields: []
  }
});

export function getRequestType(typeKey = "") {
  return REQUEST_TYPES[typeKey] || null;
}

export function getRequestTypesByCategory(categoryKey = "") {
  return Object.values(REQUEST_TYPES).filter((item) => item.categoryKey === categoryKey);
}

export function getFieldsForType(typeKey = "") {
  const type = getRequestType(typeKey);
  if (!type) {
    return [];
  }

  const specificFields = type.fields.map((field) => ({
    ...field,
    fullWidth: field.fullWidth ?? (field.type === "textarea" || field.type === "file")
  }));

  const commonFields = COMMON_FIELDS.map((field) => ({
    ...field,
    fullWidth: field.fullWidth ?? field.type === "textarea"
  }));

  return [...specificFields, ...commonFields];
}

export function listAllRequestTypes() {
  return Object.values(REQUEST_TYPES);
}
