// Hiển thị trạng thái đang xử lý khi gửi biểu mẫu, giữ nguyên giá trị của nút gửi cho MVC model binding.
document.addEventListener("submit", (event) => {
    const form = event.target;
    if (form.method.toLowerCase() !== "post" || event.defaultPrevented) return;
    if (form.getAttribute("aria-busy") === "true") {
        event.preventDefault();
        return;
    }
    const confirmation = event.submitter && event.submitter.dataset.confirm;
    if (confirmation && !window.confirm(confirmation)) {
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

// Đồng bộ nút Menu với trạng thái thật của Sidebar và trả tiêu điểm khi đóng bằng Escape.
const shell = document.querySelector(".app-authenticated");
const navigation = document.getElementById("appNavigation");
const navigationToggle = document.getElementById("navToggle");
if (shell && navigation && navigationToggle) {
    const smallScreen = matchMedia("(max-width: 900px)");
    const setNavigation = (open) => {
        shell.dataset.nav = open ? "open" : "closed";
        navigationToggle.setAttribute("aria-expanded", String(open));
    };

    const current = navigation.querySelector('a[href="' + location.pathname + '"]');
    if (current) {
        current.setAttribute("aria-current", "page");
        const group = current.closest("details");
        if (group) group.open = true;
    }
    setNavigation(!smallScreen.matches);
    smallScreen.addEventListener("change", () => setNavigation(!smallScreen.matches));
    navigationToggle.addEventListener("click", () => {
        const opening = navigationToggle.getAttribute("aria-expanded") !== "true";
        setNavigation(opening);
        if (opening && smallScreen.matches) navigation.querySelector("a").focus();
    });
    document.querySelector("[data-nav-close]").addEventListener("click", () => {
        setNavigation(false);
        navigationToggle.focus();
    });
    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && smallScreen.matches && shell.dataset.nav === "open") {
            setNavigation(false);
            navigationToggle.focus();
        }
    });
}

// Chỉ thay kiểu hiển thị. Giá trị mật khẩu, tên trường và CSRF giữ nguyên khi gửi.
document.querySelectorAll("[data-password]").forEach(button => {
    const input = document.getElementById(button.dataset.password);
    if (!input) return;
    const updateLabel = (visible) => {
        button.textContent = visible ? "Ẩn" : "Hiện";
        const label = input.labels && input.labels[0] ? input.labels[0].textContent.trim() : "mật khẩu";
        button.setAttribute("aria-label", (visible ? "Ẩn " : "Hiện ") + label);
        button.setAttribute("aria-pressed", String(visible));
    };
    updateLabel(false);
    button.addEventListener("click", () => {
        const visible = input.type === "password";
        input.type = visible ? "text" : "password";
        updateLabel(visible);
    });
});

// Bọc các bảng cũ còn đứng riêng để chỉ bảng cuộn ngang trên màn hình nhỏ.
document.querySelectorAll("main table").forEach(table => {
    if (table.closest(".table-responsive, .table-card")) return;
    const region = document.createElement("div");
    region.className = "table-responsive";
    region.tabIndex = 0;
    region.setAttribute("role", "region");
    region.setAttribute("aria-label", "Bảng dữ liệu có thể cuộn");
    table.before(region);
    region.append(table);
});