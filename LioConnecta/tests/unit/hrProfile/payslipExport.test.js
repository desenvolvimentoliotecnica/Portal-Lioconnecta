import test from "node:test";
import assert from "node:assert/strict";
import { buildPayslipFilename } from "../../../assets/js/hrProfile/payslipExport.js";

test("buildPayslipFilename usa competencia sanitizada", () => {
  assert.equal(
    buildPayslipFilename({ referenceMonth: "2026-05", periodLabel: "Maio/2026" }),
    "holerite-2026-05.pdf"
  );
});

test("buildPayslipFilename garante extensao pdf", () => {
  assert.equal(buildPayslipFilename({ id: "holerite" }), "holerite-holerite.pdf");
});
