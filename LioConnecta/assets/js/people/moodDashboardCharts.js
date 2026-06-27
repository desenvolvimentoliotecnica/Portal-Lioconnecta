const CHART_JS_URL = "https://cdn.jsdelivr.net/npm/chart.js@4.4.8/dist/chart.umd.min.js";

const MOOD_COLORS = {
  motivated: "#22c55e",
  good: "#0ea5e9",
  tired: "#f59e0b"
};

let chartJsPromise = null;
let trendChart = null;
let distributionChart = null;

function formatShortDateLabel(dateLabel = "", date = "") {
  const source = dateLabel || date;
  if (!source) {
    return "";
  }

  const parts = String(source).split("/");
  if (parts.length === 3) {
    return `${parts[0]}/${parts[1]}`;
  }

  return source;
}

function loadChartJs() {
  if (typeof window !== "undefined" && window.Chart) {
    return Promise.resolve(window.Chart);
  }

  if (!chartJsPromise) {
    chartJsPromise = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = CHART_JS_URL;
      script.async = true;
      script.onload = () => {
        if (window.Chart) {
          resolve(window.Chart);
          return;
        }

        reject(new Error("Chart.js nao ficou disponivel apos o carregamento."));
      };
      script.onerror = () => reject(new Error("Falha ao carregar Chart.js."));
      document.head.appendChild(script);
    });
  }

  return chartJsPromise;
}

function resolveColor(optionKey, fallback) {
  return MOOD_COLORS[optionKey] || fallback;
}

function buildTrendChartConfig(Chart, dailyTrend = []) {
  const labels = dailyTrend.map((item) => formatShortDateLabel(item.dateLabel, item.date));

  return {
    type: "bar",
    data: {
      labels,
      datasets: [
        {
          label: "Motivado",
          data: dailyTrend.map((item) => item.motivatedCount),
          backgroundColor: MOOD_COLORS.motivated,
          borderRadius: 6,
          borderSkipped: false
        },
        {
          label: "Bem",
          data: dailyTrend.map((item) => item.goodCount),
          backgroundColor: MOOD_COLORS.good,
          borderRadius: 6,
          borderSkipped: false
        },
        {
          label: "Cansado",
          data: dailyTrend.map((item) => item.tiredCount),
          backgroundColor: MOOD_COLORS.tired,
          borderRadius: 6,
          borderSkipped: false
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      interaction: {
        mode: "index",
        intersect: false
      },
      scales: {
        x: {
          stacked: true,
          grid: {
            display: false
          },
          ticks: {
            color: "#64748b",
            font: {
              size: 12
            }
          }
        },
        y: {
          stacked: true,
          beginAtZero: true,
          ticks: {
            precision: 0,
            color: "#64748b",
            font: {
              size: 12
            }
          },
          grid: {
            color: "#e2e8f0"
          }
        }
      },
      plugins: {
        legend: {
          position: "bottom",
          labels: {
            usePointStyle: true,
            boxWidth: 8,
            color: "#475569"
          }
        },
        tooltip: {
          backgroundColor: "#173154",
          padding: 12,
          cornerRadius: 10
        }
      }
    }
  };
}

function buildDistributionChartConfig(Chart, options = []) {
  const items = options.filter((item) => Number(item.count) > 0);
  const chartItems = items.length ? items : options;

  return {
    type: "doughnut",
    data: {
      labels: chartItems.map((item) => item.label),
      datasets: [
        {
          data: chartItems.map((item) => item.count),
          backgroundColor: chartItems.map((item, index) =>
            resolveColor(item.key, ["#22c55e", "#0ea5e9", "#f59e0b"][index % 3])
          ),
          borderWidth: 0,
          hoverOffset: 8
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: "68%",
      plugins: {
        legend: {
          position: "bottom",
          labels: {
            usePointStyle: true,
            boxWidth: 8,
            color: "#475569"
          }
        },
        tooltip: {
          backgroundColor: "#173154",
          padding: 12,
          cornerRadius: 10,
          callbacks: {
            label(context) {
              const value = context.parsed || 0;
              const total = context.dataset.data.reduce((sum, current) => sum + current, 0);
              const percentage = total > 0 ? Math.round((value / total) * 1000) / 10 : 0;
              return `${context.label}: ${value} (${percentage}%)`;
            }
          }
        }
      }
    }
  };
}

export function destroyMoodDashboardCharts() {
  if (trendChart) {
    trendChart.destroy();
    trendChart = null;
  }

  if (distributionChart) {
    distributionChart.destroy();
    distributionChart = null;
  }
}

export async function initMoodDashboardCharts(dashboard = {}) {
  destroyMoodDashboardCharts();

  const trendCanvas = document.getElementById("mood-dashboard-trend-chart");
  const distributionCanvas = document.getElementById("mood-dashboard-distribution-chart");

  if (!trendCanvas && !distributionCanvas) {
    return;
  }

  const Chart = await loadChartJs();
  const dailyTrend = Array.isArray(dashboard.dailyTrend) ? dashboard.dailyTrend : [];
  const options = Array.isArray(dashboard.options) ? dashboard.options : [];

  if (trendCanvas && dailyTrend.length) {
    trendChart = new Chart(trendCanvas, buildTrendChartConfig(Chart, dailyTrend));
  }

  if (distributionCanvas && options.length) {
    distributionChart = new Chart(distributionCanvas, buildDistributionChartConfig(Chart, options));
  }
}
