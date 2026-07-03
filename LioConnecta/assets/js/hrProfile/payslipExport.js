import { PAYSLIP_PRINT_CSS } from "./payslipPrintStyles.js";

const HTML2PDF_SRC = "https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js";
const PAYSLIP_BUILD = "holerite-v5";
let html2PdfPromise;

export function getPayslipBuildLabel() {
  return PAYSLIP_BUILD;
}

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

function buildPayslipPrintHtml(payslipHtml) {
  return `<!DOCTYPE html>
<html lang="pt-BR">
  <head>
    <meta charset="UTF-8">
    <title>Holerite</title>
    <style>${PAYSLIP_PRINT_CSS}</style>
  </head>
  <body>${payslipHtml}</body>
</html>`;
}

function waitForPrintDocument(printDocument, root = document) {
  return new Promise((resolve) => {
    if (!printDocument) {
      resolve();
      return;
    }

    if (printDocument.readyState === "complete") {
      root.defaultView?.setTimeout(resolve, 100);
      return;
    }

    printDocument.addEventListener("readystatechange", () => {
      if (printDocument.readyState === "complete") {
        root.defaultView?.setTimeout(resolve, 100);
      }
    }, { once: true });

    root.defaultView?.setTimeout(resolve, 500);
  });
}

function createPrintFrame(payslipHtml, root = document) {
  const frame = root.createElement("iframe");
  frame.setAttribute("aria-hidden", "true");
  frame.className = "payslip-print-frame";
  frame.style.cssText = "border:0;height:1200px;left:-10000px;position:fixed;top:0;width:900px;";
  root.body.appendChild(frame);

  const frameWindow = frame.contentWindow;
  const frameDocument = frame.contentDocument;
  if (!frameWindow || !frameDocument) {
    frame.remove();
    return null;
  }

  frameDocument.open();
  frameDocument.write(buildPayslipPrintHtml(payslipHtml));
  frameDocument.close();

  return { frame, frameWindow, frameDocument };
}

export async function printPayslipDocument(root = document) {
  const payslip = root.querySelector("#payslip-modal-body .payslip-doc");
  if (!payslip) {
    throw new Error("Holerite nao carregado para impressao.");
  }

  const printTarget = createPrintFrame(payslip.outerHTML, root);
  if (!printTarget) {
    throw new Error("Nao foi possivel preparar a impressao do holerite.");
  }

  const { frame, frameWindow, frameDocument } = printTarget;
  await waitForPrintDocument(frameDocument, root);

  const cleanup = () => {
    frame.remove();
    frameWindow.removeEventListener("afterprint", cleanup);
  };

  frameWindow.addEventListener("afterprint", cleanup);
  frameWindow.print();
  root.defaultView?.setTimeout(cleanup, 1000);
}

export function createPayslipExportHost(detail, renderPayslipDocument, root = document) {
  const host = root.createElement("div");
  host.className = "payslip-export-host";
  host.innerHTML = renderPayslipDocument(detail);
  root.body.appendChild(host);
  return host;
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
        backgroundColor: "#ffffff",
        scrollX: 0,
        scrollY: 0
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
