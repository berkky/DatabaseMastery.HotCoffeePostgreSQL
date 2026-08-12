(function () {
  "use strict";

  if (typeof Chart === "undefined") {
    return;
  }

  Chart.defaults.font.family = "'DM Sans', sans-serif";
  Chart.defaults.font.size = 12;
  Chart.defaults.color = "#9a9590";

  var lineCanvas = document.getElementById("lineChart");
  if (lineCanvas && window.lineChartLabels && window.lineChartData) {
    new Chart(lineCanvas, {
      type: "line",
      data: {
        labels: window.lineChartLabels,
        datasets: [{
          label: "Rezervasyon",
          data: window.lineChartData,
          borderColor: "#e6a03c",
          backgroundColor: "rgba(230,160,60,0.08)",
          borderWidth: 2.5,
          pointBackgroundColor: "#e6a03c",
          pointBorderColor: "#fff",
          pointBorderWidth: 2,
          pointRadius: 5,
          tension: 0.4,
          fill: true
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { color: "#f5f2ee" }, border: { display: false } },
          y: {
            grid: { color: "#f5f2ee" },
            border: { display: false },
            ticks: { stepSize: 1 },
            beginAtZero: true
          }
        }
      }
    });
  }

  var barCanvas = document.getElementById("barChart");
  if (barCanvas && window.barChartLabels) {
    var barColors = [
      "rgba(139,92,246,0.7)",
      "rgba(230,160,60,0.8)",
      "rgba(34,197,94,0.7)",
      "rgba(59,130,246,0.7)",
      "rgba(20,184,166,0.7)",
      "rgba(239,68,68,0.7)",
      "rgba(245,158,11,0.7)"
    ];
    var labels = window.barChartLabels || [];
    new Chart(barCanvas, {
      type: "bar",
      data: {
        labels: labels,
        datasets: [{
          label: "Ürün Sayısı",
          data: window.barChartData || [],
          backgroundColor: labels.map(function (_, i) {
            return barColors[i % barColors.length];
          }),
          borderRadius: 7,
          borderSkipped: false
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { display: false }, border: { display: false } },
          y: {
            grid: { color: "#f5f2ee" },
            border: { display: false },
            beginAtZero: true,
            ticks: { stepSize: 1 }
          }
        }
      }
    });
  }

  var pieCanvas = document.getElementById("pieChart");
  if (pieCanvas && window.pieChartLabels) {
    var pieColors = ["#e6a03c", "#3b82f6", "#22c55e", "#8b5cf6", "#14b8a6", "#ef4444", "#f59e0b"];
    var pieLabels = window.pieChartLabels || [];
    new Chart(pieCanvas, {
      type: "doughnut",
      data: {
        labels: pieLabels,
        datasets: [{
          data: window.pieChartData || [],
          backgroundColor: pieLabels.map(function (_, i) {
            return pieColors[i % pieColors.length];
          }),
          borderWidth: 0,
          hoverOffset: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: "68%",
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: function (ctx) {
                return "₺" + parseFloat(ctx.parsed).toFixed(2);
              }
            }
          }
        }
      }
    });
  }
})();
