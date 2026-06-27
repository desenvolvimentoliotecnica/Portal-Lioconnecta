export async function getJson(url, options = {}) {
  const response = await fetch(url, {
    cache: "no-store",
    ...options
  });
  if (!response.ok) {
    throw new Error(`Falha ao carregar ${url}: HTTP ${response.status}`);
  }

  return response.json();
}

export async function postJson(url, payload, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers ?? {})
  };

  const response = await fetch(url, {
    method: "POST",
    ...options,
    headers,
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    throw new Error(`Falha ao publicar em ${url}: HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export async function postFormData(url, formData, options = {}) {
  const headers = {
    ...(options.headers ?? {})
  };

  const response = await fetch(url, {
    method: "POST",
    ...options,
    headers,
    body: formData
  });

  if (!response.ok) {
    throw new Error(`Falha ao publicar em ${url}: HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export async function postWithoutBody(url, options = {}) {
  const response = await fetch(url, {
    method: "POST",
    ...options
  });

  if (!response.ok) {
    throw new Error(`Falha ao executar POST em ${url}: HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export async function putJson(url, payload, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers ?? {})
  };

  const response = await fetch(url, {
    method: "PUT",
    ...options,
    headers,
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    throw new Error(`Falha ao atualizar em ${url}: HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export async function patchJson(url, payload, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers ?? {})
  };

  const response = await fetch(url, {
    method: "PATCH",
    ...options,
    headers,
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    throw new Error(`Falha ao atualizar em ${url}: HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export async function deleteJson(url, options = {}) {
  const response = await fetch(url, {
    method: "DELETE",
    ...options
  });

  if (!response.ok) {
    throw new Error(`Falha ao excluir em ${url}: HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export function unwrapDataEnvelope(payload, { fallback = {} } = {}) {
  if (payload && typeof payload === "object" && "data" in payload) {
    return payload.data ?? fallback;
  }

  return payload ?? fallback;
}
