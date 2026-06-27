export const HR_PROFILE_ROUTE = "perfil-rh";

export const HR_PROFILE_MODULES = Object.freeze({
  ferias: {
    key: "ferias",
    label: "Ferias (Consultar/Solicitar)",
    endpointKey: "hrFerias"
  },
  holerite: {
    key: "holerite",
    label: "Holerite",
    endpointKey: "hrHolerite"
  },
  beneficios: {
    key: "beneficios",
    label: "Beneficios (VR/VT)",
    endpointKey: "hrBeneficios"
  },
  avaliacao: {
    key: "avaliacao",
    label: "Minha Avaliacao",
    endpointKey: "hrAvaliacao"
  },
  cadastro: {
    key: "cadastro",
    label: "Dados Cadastrais",
    endpointKey: "hrCadastro"
  },
  ponto: {
    key: "ponto",
    label: "Ponto",
    endpointKey: "hrPonto"
  }
});

export function isHrProfileModuleSlug(slug = "") {
  return Boolean(HR_PROFILE_MODULES[slug]);
}

export function getHrProfileModule(slug = "") {
  return HR_PROFILE_MODULES[slug] || null;
}
