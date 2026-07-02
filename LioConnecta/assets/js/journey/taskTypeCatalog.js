export const TASK_CATEGORIES = Object.freeze([
  { key: "rh", label: "RH e Pessoas" },
  { key: "treinamento", label: "Treinamento" },
  { key: "compliance", label: "Compliance" },
  { key: "ti", label: "TI" },
  { key: "operacional", label: "Operacional" },
  { key: "engajamento", label: "Engajamento" },
  { key: "geral", label: "Geral" }
]);

const COMMON_FIELDS = Object.freeze([
  {
    name: "title",
    label: "Titulo",
    type: "text",
    required: true,
    placeholder: "Ex.: Revisar politica de home office",
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
    name: "dueDate",
    label: "Prazo",
    type: "date",
    required: true,
    fullWidth: false
  },
  {
    name: "description",
    label: "Descricao",
    type: "textarea",
    required: true,
    placeholder: "Descreva os detalhes da tarefa ou atividade...",
    rows: 4,
    fullWidth: true
  }
]);

export const TASK_TYPES = Object.freeze({
  "rh-documento": {
    key: "rh-documento",
    categoryKey: "rh",
    label: "Documento RH",
    description: "Assinatura ou revisao de documentos do RH.",
    icon: "fa-solid fa-file-signature",
    listTypeLabel: "Documento RH",
    defaultStatus: "Pendente",
    fields: [
      {
        name: "documentType",
        label: "Tipo de documento",
        type: "select",
        required: true,
        options: ["Termo de uso", "Politica interna", "Contrato", "Outro"]
      }
    ]
  },
  "rh-cadastro": {
    key: "rh-cadastro",
    categoryKey: "rh",
    label: "Cadastro RH",
    description: "Atualizacao de dados cadastrais ou dependentes.",
    icon: "fa-solid fa-id-card",
    listTypeLabel: "Cadastro RH",
    defaultStatus: "Pendente",
    fields: [
      {
        name: "cadastroArea",
        label: "Area do cadastro",
        type: "select",
        required: true,
        options: ["Dados pessoais", "Dependentes", "Endereco", "Contato de emergencia"]
      }
    ]
  },
  "treinamento-curso": {
    key: "treinamento-curso",
    categoryKey: "treinamento",
    label: "Curso / trilha",
    description: "Conclusao de curso ou trilha de aprendizagem.",
    icon: "fa-solid fa-graduation-cap",
    listTypeLabel: "Curso / trilha",
    defaultStatus: "Em andamento",
    fields: [
      { name: "courseName", label: "Nome do curso", type: "text", required: true, placeholder: "Ex.: Seguranca da informacao" },
      { name: "estimatedHours", label: "Carga horaria (h)", type: "number", required: false, min: "0", step: "0.5" }
    ]
  },
  "treinamento-certificacao": {
    key: "treinamento-certificacao",
    categoryKey: "treinamento",
    label: "Certificacao",
    description: "Renovacao ou obtencao de certificacao.",
    icon: "fa-solid fa-certificate",
    listTypeLabel: "Certificacao",
    defaultStatus: "Pendente",
    fields: [
      { name: "certificationName", label: "Nome da certificacao", type: "text", required: true, placeholder: "Ex.: AWS Cloud Practitioner" }
    ]
  },
  "compliance-politica": {
    key: "compliance-politica",
    categoryKey: "compliance",
    label: "Politica / compliance",
    description: "Revisao ou ciencia de politicas corporativas.",
    icon: "fa-solid fa-scale-balanced",
    listTypeLabel: "Politica / compliance",
    defaultStatus: "Pendente",
    fields: [
      { name: "policyName", label: "Nome da politica", type: "text", required: true, placeholder: "Ex.: Politica de home office" }
    ]
  },
  "compliance-seguranca": {
    key: "compliance-seguranca",
    categoryKey: "compliance",
    label: "Seguranca da informacao",
    description: "Atividades de seguranca, LGPD ou onboarding tecnico.",
    icon: "fa-solid fa-shield-halved",
    listTypeLabel: "Seguranca da informacao",
    defaultStatus: "Em andamento",
    fields: [
      { name: "securityReference", label: "Referencia normativa", type: "text", required: false, placeholder: "Ex.: NR-01, LGPD" }
    ]
  },
  "ti-equipamento": {
    key: "ti-equipamento",
    categoryKey: "ti",
    label: "Equipamento TI",
    description: "Tarefas relacionadas a equipamentos corporativos.",
    icon: "fa-solid fa-laptop",
    listTypeLabel: "Equipamento TI",
    defaultStatus: "Pendente",
    fields: [
      {
        name: "equipmentType",
        label: "Equipamento",
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
    description: "Liberacao ou revisao de acesso a sistemas.",
    icon: "fa-solid fa-key",
    listTypeLabel: "Acesso a sistema",
    defaultStatus: "Pendente",
    fields: [
      { name: "systemName", label: "Sistema", type: "text", required: true, placeholder: "Ex.: SAP, ServiceNow" },
      { name: "desiredProfile", label: "Perfil desejado", type: "text", required: true, placeholder: "Ex.: Consultor" }
    ]
  },
  "operacional-reuniao": {
    key: "operacional-reuniao",
    categoryKey: "operacional",
    label: "Reuniao / compromisso",
    description: "Preparacao ou participacao em reunioes.",
    icon: "fa-solid fa-calendar-check",
    listTypeLabel: "Reuniao / compromisso",
    defaultStatus: "Pendente",
    fields: [
      { name: "meetingDate", label: "Data da reuniao", type: "date", required: true },
      { name: "participants", label: "Participantes", type: "text", required: false, placeholder: "Ex.: Equipe de produto" }
    ]
  },
  "operacional-entrega": {
    key: "operacional-entrega",
    categoryKey: "operacional",
    label: "Entrega / projeto",
    description: "Entregas operacionais ou marcos de projeto.",
    icon: "fa-solid fa-diagram-project",
    listTypeLabel: "Entrega / projeto",
    defaultStatus: "Em andamento",
    fields: [
      { name: "projectName", label: "Projeto / entregavel", type: "text", required: true, placeholder: "Ex.: Release Q3" }
    ]
  },
  "engajamento-pesquisa": {
    key: "engajamento-pesquisa",
    categoryKey: "engajamento",
    label: "Pesquisa / enquete",
    description: "Responder pesquisas internas ou enquetes.",
    icon: "fa-solid fa-square-poll-vertical",
    listTypeLabel: "Pesquisa / enquete",
    defaultStatus: "Pendente",
    fields: [
      { name: "surveyName", label: "Nome da pesquisa", type: "text", required: true, placeholder: "Ex.: Pesquisa de clima 2026" }
    ]
  },
  "geral-outros": {
    key: "geral-outros",
    categoryKey: "geral",
    label: "Outros",
    description: "Demais tarefas e atividades nao categorizadas.",
    icon: "fa-solid fa-circle-question",
    listTypeLabel: "Outros",
    defaultStatus: "Pendente",
    fields: []
  }
});

export function getTaskType(typeKey = "") {
  return TASK_TYPES[typeKey] || null;
}

export function getTaskTypesByCategory(categoryKey = "") {
  return Object.values(TASK_TYPES).filter((item) => item.categoryKey === categoryKey);
}

export function getFieldsForType(typeKey = "") {
  const type = getTaskType(typeKey);
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

export function listAllTaskTypes() {
  return Object.values(TASK_TYPES);
}

export function formatDueDateInput(value) {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return String(value).slice(0, 10);
  }

  return date.toISOString().slice(0, 10);
}
