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

function buildPayslipPrintHtml(payslipHtml, root = document) {
  const stylesheetLinks = [...root.querySelectorAll("link[rel='stylesheet']")]
    .map((link) => `<link rel="stylesheet" href="${link.href}">`)
    .join("\n");

  return `<!DOCTYPE html>
<html lang="pt-BR">
  <head>
    <meta charset="UTF-8">
    <title>Holerite</title>
    ${stylesheetLinks}
    <style>
      body {
        background: #fff;
        margin: 0;
        padding: 16px;
      }

      .payslip-doc {
        box-shadow: none;
        margin: 0 auto;
        max-width: none;
      }
    </style>
  </head>
  <body>${payslipHtml}</body>
</html>`;
}

function waitForPrintFrame(frame, root = document) {
  return new Promise((resolve) => {
    const frameWindow = frame.contentWindow;
    const frameDocument = frame.contentDocument;
    if (!frameWindow || !frameDocument) {
      resolve();
      return;
    }

    const links = [...frameDocument.querySelectorAll("link[rel='stylesheet']")];
    if (!links.length) {
      root.defaultView?.setTimeout(resolve, 50);
      return;
    }

    let pending = links.length;
    const finish = () => {
      pending -= 1;
      if (pending <= 0) {
        root.defaultView?.setTimeout(resolve, 50);
      }
    };

    links.forEach((link) => {
      if (link.sheet) {
        finish();
        return;
      }

      link.addEventListener("load", finish, { once: true });
      link.addEventListener("error", finish, { once: true });
    });

    root.defaultView?.setTimeout(resolve, 1500);
  });
}

export async function printPayslipDocument(root = document) {
  const payslip = root.querySelector("#payslip-modal-body .payslip-doc");
  if (!payslip) {
    throw new Error("Holerite nao carregado para impressao.");
  }

  const frame = root.createElement("iframe");
  frame.setAttribute("aria-hidden", "true");
  frame.style.cssText = "border:0;height:0;position:fixed;right:0;bottom:0;width:0;";
  root.body.appendChild(frame);

  const frameDocument = frame.contentDocument;
  const frameWindow = frame.contentWindow;
  if (!frameDocument || !frameWindow) {
    frame.remove();
    throw new Error("Nao foi possivel preparar a impressao do holerite.");
  }

  frameDocument.open();
  frameDocument.write(buildPayslipPrintHtml(payslip.outerHTML, root));
  frameDocument.close();

  await waitForPrintFrame(frame, root);

  const cleanup = () => {
    frame.remove();
    frameWindow.removeEventListener("afterprint", cleanup);
  };

  frameWindow.addEventListener("afterprint", cleanup);
  frameWindow.focus();
  frameWindow.print();
  root.defaultView?.setTimeout(cleanup, 1000);
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
