export const PAYSLIP_PRINT_CSS = `
  body {
    background: #fff;
    color: #1a2b3c;
    font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
    margin: 0;
    padding: 16px;
  }

  .payslip-doc {
    background: #fff;
    border: 1px solid #c8d6e5;
    border-radius: 4px;
    color: #1a2b3c;
    display: grid;
    gap: 0;
    margin: 0 auto;
    max-width: 860px;
  }

  .payslip-doc__header {
    align-items: start;
    background: linear-gradient(180deg, #f7fafc 0%, #eef4fa 100%);
    border-bottom: 2px solid #173154;
    display: grid;
    gap: 16px;
    grid-template-columns: minmax(0, 1fr) auto;
    padding: 18px 20px;
  }

  .payslip-doc__company {
    display: grid;
    gap: 4px;
  }

  .payslip-doc__company strong {
    color: #173154;
    font-size: 1rem;
  }

  .payslip-doc__company span {
    color: #5f7489;
    font-size: 0.78rem;
  }

  .payslip-doc__title-block {
    display: grid;
    gap: 2px;
    justify-items: end;
    text-align: right;
  }

  .payslip-doc__eyebrow {
    color: #5f7489;
    font-size: 0.72rem;
    letter-spacing: 0.04em;
    text-transform: uppercase;
  }

  .payslip-doc__title-block strong {
    color: #173154;
    font-size: 1.1rem;
  }

  .payslip-doc__employee,
  .payslip-doc__bases {
    border-bottom: 1px solid #dbe6f0;
    display: grid;
    gap: 12px;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    padding: 14px 20px;
  }

  .payslip-doc__field {
    display: grid;
    gap: 2px;
  }

  .payslip-doc__field span {
    color: #5f7489;
    font-size: 0.72rem;
    text-transform: uppercase;
  }

  .payslip-doc__field strong {
    font-size: 0.86rem;
  }

  .payslip-doc__columns {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .payslip-doc__column h3 {
    background: #173154;
    color: #fff;
    font-size: 0.78rem;
    letter-spacing: 0.04em;
    margin: 0;
    padding: 8px 12px;
    text-transform: uppercase;
  }

  .payslip-doc__column + .payslip-doc__column {
    border-left: 1px solid #dbe6f0;
  }

  .payslip-doc__table {
    border-collapse: collapse;
    width: 100%;
  }

  .payslip-doc__table th,
  .payslip-doc__table td {
    border-bottom: 1px solid #edf2f7;
    font-size: 0.78rem;
    padding: 7px 10px;
    text-align: left;
    vertical-align: top;
  }

  .payslip-doc__table th {
    background: #f8fbfd;
    color: #5f7489;
    font-weight: 600;
  }

  .payslip-doc__amount-col {
    text-align: right;
    white-space: nowrap;
  }

  .payslip-doc__empty-row {
    color: #5f7489;
    text-align: center;
  }

  .payslip-doc__footer {
    display: grid;
    gap: 16px;
    grid-template-columns: minmax(0, 1.2fr) minmax(0, 1fr);
    padding: 16px 20px 18px;
  }

  .payslip-doc__totals {
    display: grid;
    gap: 8px;
  }

  .payslip-doc__total-row {
    align-items: baseline;
    border-bottom: 1px dashed #dbe6f0;
    display: flex;
    justify-content: space-between;
    gap: 12px;
    padding-bottom: 6px;
  }

  .payslip-doc__total-row span {
    color: #5f7489;
    font-size: 0.82rem;
  }

  .payslip-doc__total-row strong {
    font-size: 0.92rem;
  }

  .payslip-doc__total-row--net {
    background: #eef8f0;
    border: 1px solid #b9dfc4;
    border-radius: 8px;
    margin-top: 4px;
    padding: 10px 12px;
  }

  .payslip-doc__total-row--net span,
  .payslip-doc__total-row--net strong {
    color: #17653a;
    font-size: 1rem;
    font-weight: 700;
  }

  .payslip-doc__payment {
    align-content: start;
    background: #f8fbfd;
    border: 1px solid #dbe6f0;
    border-radius: 10px;
    display: grid;
    gap: 10px;
    padding: 12px;
  }

  .payslip-doc__simulated {
    background: #fff8e8;
    border-top: 1px solid #f0dfaa;
    color: #8a6a1d;
    font-size: 0.78rem;
    margin: 0;
    padding: 10px 20px;
    text-align: center;
  }
`;
