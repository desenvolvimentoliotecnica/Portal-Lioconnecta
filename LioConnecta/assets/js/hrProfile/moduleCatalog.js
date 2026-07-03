export const HR_PROFILE_ROUTE = "perfil-rh";

export const HR_PROFILE_MODULES = Object.freeze({
  ferias: {
    key: "ferias",
    label: "Ferias (Consultar/Solicitar)",
    endpointKey: "hrFerias",
    heroDescription: "Acompanhe ferias."
  },
  holerite: {
    key: "holerite",
    label: "Holerite",
    endpointKey: "hrHolerite",
    heroDescription: "Acompanhe pagamentos."
  },
  beneficios: {
    key: "beneficios",
    label: "Beneficios (VR/VT)",
    endpointKey: "hrBeneficios",
    heroDescription: "Acompanhe beneficios."
  },
  avaliacao: {
    key: "avaliacao",
    label: "Minha Avaliacao",
    endpointKey: "hrAvaliacao",
    heroDescription: "Acompanhe avaliacoes."
  },
  cadastro: {
    key: "cadastro",
    label: "Dados Cadastrais",
    endpointKey: "hrCadastro",
    heroDescription: "Acompanhe seus dados."
  },
  ponto: {
    key: "ponto",
    label: "Ponto",
    endpointKey: "hrPonto",
    heroDescription: "Acompanhe seu ponto."
  }
});

export const HR_PROFILE_HERO_DESCRIPTIONS = Object.freeze(
  Object.fromEntries(
    Object.values(HR_PROFILE_MODULES).map((module) => [module.key, module.heroDescription || "Acompanhe suas informacoes de RH."])
  )
);

export function isHrProfileModuleSlug(slug = "") {
  return Boolean(HR_PROFILE_MODULES[slug]);
}

export function getHrProfileModule(slug = "") {
  return HR_PROFILE_MODULES[slug] || null;
}
