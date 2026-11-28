(() => {
    const source = document.getElementById("dashboard-data");
    if (!source || !window.Chart) return;

    // Dùng cùng dữ liệu tổng hợp với bảng điều khiển, giữ biểu đồ hoạt động khi máy demo mất mạng.
    function draw(id, type, labels, values, backgroundColor) {
        const canvas = document.getElementById(id);
        if (!canvas) return;
        new Chart(canvas, {
            type,
            data: { labels, datasets: [{ data: values, backgroundColor }] },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: { display: type === "doughnut" } },
                scales: type === "bar" ? { y: { beginAtZero: true } } : {}
            }
        });
    }

    const states = JSON.parse(source.dataset.states);
    const departments = JSON.parse(source.dataset.departments);
    const costs = JSON.parse(source.dataset.costs);
    draw("asset-states", "doughnut", states.map(row => row.Label), states.map(row => row.Count),
        ["#06766c", "#b86a15", "#b43649", "#687487", "#3c69aa"]);
    draw("asset-departments", "bar", departments.map(row => row.Label), departments.map(row => row.Count), "#06766c");
    draw("maintenance-costs", "bar", costs.map((_, index) => "Tháng " + (index + 1)), costs, "#316594");
})();