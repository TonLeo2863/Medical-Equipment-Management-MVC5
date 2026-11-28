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

// Navbar ngang dùng details để Enter và Space mở từng nhóm bằng hành vi gốc của trình duyệt.
const navigation = document.getElementById("appNavigation");
const navigationToggle = document.getElementById("navToggle");
if (navigation && navigationToggle) {
    const navbar = navigation.closest(".navbar");
    const compact = matchMedia("(max-width: 60rem)");
    const groups = Array.from(navigation.querySelectorAll("details"));
    const closeGroups = (except) => groups.forEach(group => {
        if (group !== except) group.open = false;
    });
    const setMenu = (open) => {
        navigationToggle.setAttribute("aria-expanded", String(open));
        if (!open) closeGroups();
    };

    navbar.classList.add("has-toggle");
    const current = Array.from(navigation.querySelectorAll("a")).find(link => link.pathname === location.pathname);
    if (current) current.setAttribute("aria-current", "page");

    navigationToggle.addEventListener("click", () => {
        const open = navigationToggle.getAttribute("aria-expanded") !== "true";
        setMenu(open);
        if (open) navigation.querySelector("a").focus();
    });
    // Bàn phím cũng kích hoạt click trên summary. Đóng nhóm cũ trước khi trình duyệt mở nhóm mới.
    groups.forEach(group => {
        group.querySelector("summary").addEventListener("click", () => closeGroups(group));
    });

    // Escape đóng nhóm trong cùng trước. Khi đóng Menu di động, đưa tiêu điểm về nút đã mở.
    navbar.addEventListener("keydown", event => {
        if (event.key !== "Escape") return;
        const group = groups.find(item => item.open);
        if (group) {
            group.open = false;
            group.querySelector("summary").focus();
        } else if (compact.matches && navigationToggle.getAttribute("aria-expanded") === "true") {
            setMenu(false);
            navigationToggle.focus();
        }
    });
    document.addEventListener("click", event => {
        if (!navbar.contains(event.target)) setMenu(false);
    });
    navbar.addEventListener("focusout", event => {
        if (event.relatedTarget && !navbar.contains(event.relatedTarget)) setMenu(false);
    });
    compact.addEventListener("change", () => setMenu(false));
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
