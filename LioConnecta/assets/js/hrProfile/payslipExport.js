const HTML2PDF_SRC = "https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js";
let html2PdfPromise;

export function buildPayslipFilename(detail = {}) {
  const reference = String(detail.referenceMonth || detail.id || "holerite")
    .replace(/[^\w-]+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");

  return `holerite-${reference || "competencia"}.pdf`;
}

function loadHtml2PdfLibrary() {
  if (typeof window !== "undefined" && window.html2pdf) {
    return Promise.resolve(window.html2pdf);
  }

  if (!html2PdfPromise) {
    html2PdfPromise = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = HTML2PDF_SRC;
      script.async = true;
      script.onload = () => {
        if (window.html2pdf) {
          resolve(window.html2pdf);
          return;
        }

        reject(new Error("Biblioteca html2pdf indisponivel."));
      };
      script.onerror = () => reject(new Error("Nao foi possivel carregar a biblioteca de PDF."));
      document.head.appendChild(script);
    });
  }

  return html2PdfPromise;
}

export function printPayslipDocument(root = document) {
  const payslip = root.querySelector("#payslip-modal-body .payslip-doc");
  if (!payslip) {
    throw new Error("Holerite nao carregado para impressao.");
  }

  root.body.classList.add("payslip-printing");
  const cleanup = () => {
    root.body.classList.remove("payslip-printing");
    root.defaultView?.removeEventListener("afterprint", cleanup);
  };

  root.defaultView?.addEventListener("afterprint", cleanup);
  root.defaultView?.print();
}

export async function downloadPayslipPdf(element, filename, root = document) {
  if (!element) {
    throw new Error("Holerite nao encontrado para exportacao.");
  }

  const html2pdf = await loadHtml2PdfLibrary();
  const safeFilename = String(filename || "holerite.pdf").endsWith(".pdf")
    ? String(filename || "holerite.pdf")
    : `${filename || "holerite"}.pdf`;

  await html2pdf()
    .set({
      margin: [8, 8, 8, 8],
      filename: safeFilename,
      image: { type: "jpeg", quality: 0.98 },
      html2canvas: {
        scale: 2,
        useCORS: true,
        backgroundColor: "#ffffff"
      },
      jsPDF: {
        unit: "mm",
        format: "a4",
        orientation: "portrait"
      },
      pagebreak: { mode: ["avoid-all", "css", "legacy"] }
    })
    .from(element)
    .save();
}
