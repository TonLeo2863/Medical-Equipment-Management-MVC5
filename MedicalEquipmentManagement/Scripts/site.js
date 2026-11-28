// Hiển thị trạng thái đang xử lý khi gửi biểu mẫu, giữ nguyên giá trị của nút gửi cho MVC model binding.
document.addEventListener("submit", (event) => {
    const form = event.target;
    if (form.method.toLowerCase() !== "post" || event.defaultPrevented) return;
    if (form.getAttribute("aria-busy") === "true") {
        event.preventDefault();
        return;
    }
    form.setAttribute("aria-busy", "true");
    const status = document.createElement("span");
    status.className = "submit-status";
    status.setAttribute("role", "status");
    status.textContent = "Đang xử lý…";
    form.append(status);
});

window.addEventListener("pageshow", () => {
    document.querySelectorAll("form[aria-busy]").forEach(form => {
        form.removeAttribute("aria-busy");
        const status = form.querySelector(".submit-status");
        if (status) status.remove();
    });
});
