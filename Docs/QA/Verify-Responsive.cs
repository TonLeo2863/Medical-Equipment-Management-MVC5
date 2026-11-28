using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

// Kiểm tra giao diện bằng Edge Runtime đã cài trên máy QA. Chạy trong cửa sổ ẩn.
internal static class VerifyResponsive
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
    private static readonly int[] ViewportWidths = { 360, 390, 768, 1024, 1280, 1366, 1920 };

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 2)
            return 2;
        var baseUrl = args[0];
        var log = args[1];
        var exitCode = 1;
        Application.EnableVisualStyles();
        using (var form = new Form { ShowInTaskbar = false, Opacity = 0, ClientSize = new Size(1280, 900) })
        using (var view = new WebView2 { Dock = DockStyle.Fill })
        {
            form.Controls.Add(view);
            form.Shown += async (sender, eventArgs) =>
            {
                try
                {
                    var cache = Path.Combine(Path.GetTempPath(), "MedBetaClean-WebView2-" + Guid.NewGuid().ToString("N"));
                    var environment = await CoreWebView2Environment.CreateAsync(null, cache);
                    await view.EnsureCoreWebView2Async(environment);
                    await Verify(view.CoreWebView2, baseUrl, log);
                    exitCode = 0;
                }
                catch (Exception error)
                {
                    Write(log, "FAIL Edge WebView2: " + error.Message);
                }
                finally
                {
                    form.Close();
                }
            };
            Application.Run(form);
        }
        return exitCode;
    }

    private static async Task Verify(CoreWebView2 browser, string baseUrl, string log)
    {
        await Resize(browser, 390);
        await Navigate(browser, baseUrl + "/Account/Login");
        await AssertViewport(browser, "Đăng nhập", 390);
        await Capture(browser, "classic-390-dang-nhap.png");
        var user = Json.Serialize(Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_USER"));
        var password = Json.Serialize(Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_PASSWORD"));
        var loginNavigation = ObserveNavigation(browser);
        await browser.ExecuteScriptAsync("(() => { const username = document.querySelector('[name=username]'); const password = document.querySelector('[name=password]'); username.value = "
            + user + "; password.value = " + password + "; username.form.requestSubmit(); })()");
        await loginNavigation;
        Require(await Check(browser, "!!window.Chart && !!Chart.getChart('asset-states') && !!Chart.getChart('asset-departments') && !!Chart.getChart('maintenance-costs') && Chart.getChart('asset-states').data.datasets[0].data.some(n => n > 0)"),
            "Ba biểu đồ cần khởi tạo với dữ liệu thiết bị thật.");
        Write(log, "PASS Edge WebView2 hiển thị cả ba biểu đồ với dữ liệu thực");

        if (Environment.GetEnvironmentVariable("MEDICAL_QA_ACCOUNT_BASELINE") == "1")
        {
            await VerifyAccounts(browser, baseUrl, log, true);
            return;
        }
        browser.Settings.AreDefaultScriptDialogsEnabled = false;
        browser.ScriptDialogOpening += (sender, dialog) => dialog.Accept();
        if (Environment.GetEnvironmentVariable("MEDICAL_QA_ACCOUNT_ONLY") == "1")
        {
            await VerifyAccounts(browser, baseUrl, log, false);
            return;
        }
        await VerifyAccounts(browser, baseUrl, log, false);
        await Login(browser, baseUrl, Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_USER"),
            Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_PASSWORD"), true);
        var fixtureAsset = Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_ASSET_ID");
        var routeSet = new[] { "/", "/Equipment", "/Equipment/Create", "/Equipment/Import", "/Category", "/Supplier",
            "/EquipmentModel", "/Department", "/Inventory", "/Inventory/Transfer", "/WorkOrder", "/Maintenance",
            "/Calibration", "/UserAdmin", "/UserAdmin/Edit", "/Account/ChangePassword", "/Error/AccessDenied",
            "/Equipment/Details/" + fixtureAsset, "/Equipment/Edit/" + fixtureAsset, "/Equipment/Delete/" + fixtureAsset,
            "/WorkOrder/Create", "/Maintenance/Create", "/Calibration/Create" };
        await browser.CallDevToolsProtocolMethodAsync("Emulation.setFocusEmulationEnabled", "{\"enabled\":true}");
        foreach (var width in ViewportWidths)
        {
            await Resize(browser, width);
            foreach (var path in routeSet)
            {
                await Navigate(browser, baseUrl + path, path == "/Error/AccessDenied");
                await AssertViewport(browser, path, width);
            }
            Write(log, "PASS Bố cục của " + routeSet.Length + " đường dẫn ở " + width + "px");
            await Navigate(browser, baseUrl + "/UserAdmin/Edit");
            await VerifyNavbar(browser, width);
            Write(log, "PASS Navbar, bốn nhóm, biên dropdown và bàn phím ở " + width + "px");
        }

        await Resize(browser, 360);
        await Navigate(browser, baseUrl + "/UserAdmin/Edit");
        Require(await Check(browser, "parseFloat(getComputedStyle(document.querySelector('[name=Username]')).fontSize) >= 16 && getComputedStyle(document.querySelector('select')).backgroundImage.includes('svg')"), "Ô nhập hoặc mũi tên SVG chưa đồng nhất.");
        await Capture(browser, "classic-360-tao-tai-khoan.png");
        await Resize(browser, 1366);
        await Navigate(browser, baseUrl + "/");
        await Capture(browser, "classic-1366-dashboard.png");
        Write(log, "PASS RC-008–010 SVG, ô nhập 16px, Navbar ngang và Menu thu gọn.");
        var prefix = Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_USER");
        prefix = prefix.Substring(0, prefix.Length - 6);
        var fixturePassword = Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_PASSWORD");
        foreach (var roleCode in new[] { "MANAGER", "ENGINEER", "STAFF" })
        {
            await Login(browser, baseUrl, prefix + "_" + roleCode, fixturePassword, true);
            Require(await Check(browser, "document.querySelector('#appNavigation a[href*=UserAdmin]') === null"), "Vai trò này không được thấy menu quản trị.");
            Require(await Check(browser, "(document.querySelector('#appNavigation a[href*=Calibration]') !== null) === " + (roleCode == "STAFF" ? "false" : "true")), "Menu kỹ thuật sai quyền.");
            Write(log, "PASS Menu trình duyệt theo vai trò " + roleCode);
        }

        // Kiểm tra riêng giao diện công khai sau khi xóa Cookie của các tài khoản fixture.
        browser.CookieManager.DeleteAllCookies();
        foreach (var width in ViewportWidths)
        {
            await Resize(browser, width);
            await Navigate(browser, baseUrl + "/");
            await AssertViewport(browser, "Trang chủ công khai", width);
            Require(await Check(browser, "document.querySelectorAll('#appNavigation a').length === 3 && !document.querySelector('#appNavigation details')"), "Menu công khai cần hiển thị Trang chủ, Giới thiệu và Đăng nhập.");
            await Navigate(browser, baseUrl + "/Account/Login");
            await AssertViewport(browser, "Đăng nhập", width);
            if (width == 1280) await Capture(browser, "classic-1280-dang-nhap.png");
        }
        Write(log, "PASS Trang chủ công khai và đăng nhập tại bảy viewport.");
    }

    /// <summary>Kiểm tra Navbar bằng bàn phím ở chiều rộng hiện tại, gồm cả vị trí thật của bảng chọn và liên kết bên trong.</summary>
    private static async Task VerifyNavbar(CoreWebView2 browser, int width)
    {
        var compact = width <= 960;
        var toggle = "document.getElementById('navToggle')";
        if (compact)
        {
            Require(await Check(browser, toggle + ".offsetWidth > 0 && !document.getElementById('appNavigation').offsetWidth"), "Menu di động phải thu gọn khi vào trang.");
            await browser.ExecuteScriptAsync(toggle + ".focus()");
            await Press(browser, "Enter", 13);
            Require(await Check(browser, toggle + ".getAttribute('aria-expanded') === 'true' && document.activeElement === document.querySelector('#appNavigation a')"), "Enter cần mở Menu và đưa tiêu điểm đến liên kết đầu.");
        }
        else
        {
            Require(await Check(browser, toggle + @".offsetWidth === 0 && (() => {
                const items = Array.from(document.getElementById('appNavigation').children);
                const top = items[0].getBoundingClientRect().top;
                return items.every(item => Math.abs(item.getBoundingClientRect().top - top) < 2);
            })()"), "Các mục Navbar desktop phải hiển thị trên cùng một hàng.");
        }

        var labels = new[] { "Danh mục", "Kho", "Kỹ thuật", "Tài khoản" };
        foreach (var label in labels)
        {
            var group = "Array.from(document.querySelectorAll('#appNavigation details')).find(g => g.querySelector('summary').textContent.trim() === " + Json.Serialize(label) + ")";
            await browser.ExecuteScriptAsync(group + ".querySelector('summary').focus()");
            await Press(browser, "Enter", 13);
            Require(await Check(browser, "(() => { const group = " + group + @";
                const panel = group.querySelector('.nav-panel');
                const bounds = panel.getBoundingClientRect();
                const link = panel.querySelector('a');
                const target = link.getBoundingClientRect();
                const hit = document.elementFromPoint(target.left + target.width / 2, target.top + target.height / 2);
                return group.open && document.querySelectorAll('#appNavigation details[open]').length === 1
                    && getComputedStyle(group.querySelector('summary'), '::after').backgroundImage.includes('svg')
                    && bounds.width > 0 && bounds.left >= 0 && bounds.right <= innerWidth
                    && bounds.top >= 0 && bounds.bottom <= innerHeight && link.contains(hit);
            })()"), "Bảng chọn bị che hoặc vượt màn hình: " + label + " ở " + width);
            if (width == 360 && label == "Danh mục") await Capture(browser, "classic-360-menu.png");
            if (width == 1280 && label == "Kỹ thuật") await Capture(browser, "classic-1280-ky-thuat.png");
            await browser.ExecuteScriptAsync(group + ".querySelector('.nav-panel a').focus()");
            await Press(browser, "Escape", 27);
            Require(await Check(browser, "!(" + group + ").open && document.activeElement === (" + group + ").querySelector('summary')"), "Escape cần đóng nhóm và trả tiêu điểm về nhãn.");
        }
        if (compact)
        {
            await Press(browser, "Escape", 27);
            Require(await Check(browser, "document.activeElement === " + toggle + " && " + toggle + ".getAttribute('aria-expanded') === 'false'"), "Escape cần đóng Menu di động và trả tiêu điểm.");
        }
        else
        {
            // Đổi thẳng từ nhóm đầu sang nhóm cuối rồi nhấp ra ngoài, như khi dùng chuột trên desktop.
            await browser.ExecuteScriptAsync("document.querySelector('#appNavigation details summary').click()");
            await browser.ExecuteScriptAsync("document.querySelector('#appNavigation details:last-child summary').click()");
            Require(await Check(browser, "document.querySelectorAll('#appNavigation details[open]').length === 1 && document.querySelector('#appNavigation details:last-child').open"), "Chuyển nhóm cần đóng dropdown trước đó.");
            await browser.ExecuteScriptAsync("document.querySelector('main').click()");
            Require(await Check(browser, "!document.querySelector('#appNavigation details[open]')"), "Nhấp ngoài Navbar cần đóng dropdown.");
        }
    }

    // Gửi phím đến trình duyệt để kiểm tra hành vi mặc định của nút và quản lý tiêu điểm.
    private static async Task Press(CoreWebView2 browser, string key, int virtualKey)
    {
        // WebView2 chạy ẩn cần sự kiện ký tự Enter riêng sau rawKeyDown để kích hoạt nút HTML.
        await browser.CallDevToolsProtocolMethodAsync("Input.dispatchKeyEvent",
            Json.Serialize(new { type = "rawKeyDown", key, code = key, windowsVirtualKeyCode = virtualKey }));
        if (key == "Enter")
            await browser.CallDevToolsProtocolMethodAsync("Input.dispatchKeyEvent",
                Json.Serialize(new { type = "char", key, code = key, text = "\r", unmodifiedText = "\r", windowsVirtualKeyCode = virtualKey }));
        await browser.CallDevToolsProtocolMethodAsync("Input.dispatchKeyEvent",
            Json.Serialize(new { type = "keyUp", key, code = key, windowsVirtualKeyCode = virtualKey }));
    }

    // Chỉ gọi ở biểu mẫu trống hoặc menu dùng tài khoản fixture để ảnh không chứa mật khẩu.
    private static async Task Capture(CoreWebView2 browser, string name)
    {
        var directory = Environment.GetEnvironmentVariable("MEDICAL_QA_SCREENSHOTS");
        if (string.IsNullOrEmpty(directory))
            return;
        Directory.CreateDirectory(directory);
        var result = Json.Deserialize<Dictionary<string, string>>(await browser.CallDevToolsProtocolMethodAsync("Page.captureScreenshot", "{\"format\":\"png\"}"));
        File.WriteAllBytes(Path.Combine(directory, name), Convert.FromBase64String(result["data"]));
    }


    /// <summary>Kiểm tra ba luồng tài khoản bằng biểu mẫu thật và thông tin fixture sinh khi chạy.</summary>
    private static async Task VerifyAccounts(CoreWebView2 browser, string baseUrl, string log, bool baseline)
    {
        var admin = Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_USER");
        var account = admin.Substring(0, admin.Length - 6) + "_BROWSER";
        var first = Guid.NewGuid().ToString("N") + "<qa>";
        var reset = Guid.NewGuid().ToString("N") + "<reset>";
        var changed = Guid.NewGuid().ToString("N") + "<change>";
        var normal = Guid.NewGuid().ToString("N");
        var create = baseUrl + "/UserAdmin/Edit";
        var role = "Array.from(document.querySelector('[name=RoleId]').options).find(option => option.text === 'STAFF').value";
        if (!baseline)
        {
            await Navigate(browser, create);
            await Submit(browser, "Username", new { Username = account, FullName = "Kiểm thử giao diện", InitialPassword = "ngan", InitialConfirmation = "khac" }, role);
            Require(await Check(browser, "document.querySelector('[data-valmsg-for=InitialPassword]').innerText.includes('12') && document.querySelector('[data-valmsg-for=InitialConfirmation]').innerText.includes('xác nhận') && document.querySelector('[name=InitialPassword]').value === ''"), "Cần báo lỗi đúng trường và bỏ mật khẩu khỏi form trả lại.");
            Require(await Check(browser, "(() => {const button=document.querySelector('[data-password=InitialPassword]');button.click();const visible=document.getElementById('InitialPassword').type==='text';button.click();return visible && document.getElementById('InitialPassword').type==='password';})()"), "Nút hiện/ẩn mật khẩu không hoạt động.");
            Write(log, "PASS RC-005/RC-011 Validation tiếng Việt theo từng trường, không hiển thị lại mật khẩu.");
        }
        foreach (var attempt in baseline ? new[] { first, normal } : new[] { first })
        {
            await Navigate(browser, create);
            await Submit(browser, "Username", new { Username = account, FullName = "Kiểm thử giao diện", InitialPassword = attempt, InitialConfirmation = attempt }, role, baseline);
            var saved = await Check(browser, "location.pathname === '/UserAdmin' && document.querySelector('main').innerText.includes(" + Json.Serialize(account) + ")");
            if (baseline && attempt == first)
            {
                Require(!saved, "Beta đã chấp nhận mật khẩu đặc biệt, cần xem lại giả thuyết RC-005.");
                Write(log, "REPRO RC-005 Mật khẩu chứa dấu ngoặc góc bị chặn khi tạo.");
            }
            else Require(saved, "Không lưu được tài khoản bằng form.");
        }
        if (!baseline)
        {
            await Login(browser, baseUrl, account, first, true);
            await Login(browser, baseUrl, admin, Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_PASSWORD"), true);
            await Navigate(browser, baseUrl + "/UserAdmin");
            Write(log, "PASS RC-005 Tài khoản mới đăng nhập bằng mật khẩu chứa dấu ngoặc góc.");
        }
        var resetUrl = await browser.ExecuteScriptAsync("Array.from(document.querySelectorAll('tr')).find(row => row.innerText.includes(" + Json.Serialize(account) + ")).querySelector('a[href*=ResetPassword]').href");
        await Navigate(browser, Json.Deserialize<string>(resetUrl));
        await Submit(browser, "NewPassword", new { NewPassword = reset, Confirmation = reset }, null, baseline);
        Require(await Check(browser, baseline ? "location.pathname !== '/UserAdmin'" : "location.pathname === '/UserAdmin' && document.querySelector('[role=status]').innerText.includes('Đã đặt lại')"),
            "Kết quả đặt lại mật khẩu không đúng kỳ vọng.");
        Write(log, baseline ? "REPRO RC-006 Request đặt lại bị chặn trước khi cập nhật hash." : "PASS RC-006 Đặt lại bằng biểu mẫu và nhận thông báo.");
        await Login(browser, baseUrl, account, baseline ? normal : first, baseline);
        if (!baseline) await Login(browser, baseUrl, account, reset, true);
        await Navigate(browser, baseUrl + "/Account/ChangePassword");
        await Submit(browser, "CurrentPassword", new { CurrentPassword = baseline ? normal : reset, NewPassword = changed, Confirmation = changed }, null, baseline);
        Require(await Check(browser, baseline ? "location.pathname !== '/'" : "location.pathname === '/' && document.querySelector('[role=status]').innerText.includes('Đã đổi')"),
            "Kết quả đổi mật khẩu không đúng kỳ vọng.");
        Write(log, baseline ? "REPRO RC-007 Request đổi mật khẩu bị chặn." : "PASS RC-007 Đổi mật khẩu bằng biểu mẫu và nhận thông báo.");
        if (!baseline)
        {
            await Login(browser, baseUrl, account, reset, false);
            await Login(browser, baseUrl, account, changed, true);
            Write(log, "PASS RC-005–007 Mật khẩu mới đăng nhập được và các mật khẩu cũ bị từ chối.");
        }
    }

    private static async Task Login(CoreWebView2 browser, string baseUrl, string username, string password, bool success)
    {
        browser.CookieManager.DeleteAllCookies();
        await Navigate(browser, baseUrl + "/Account/Login");
        await Submit(browser, "username", new { username, password });
        Require(await Check(browser, success ? "location.pathname === '/'" : "location.pathname.toLowerCase() === '/account/login'"),
            "Kết quả đăng nhập không đúng kỳ vọng.");
        if (!success)
            Require(await Check(browser, "document.querySelector('.validation-summary-errors').innerText.includes('Tên đăng nhập hoặc mật khẩu không đúng') && document.querySelector('[name=password]').value === ''"), "Biểu mẫu cần hiển thị lỗi đăng nhập và xóa giá trị mật khẩu.");
    }

    private static async Task Submit(CoreWebView2 browser, string field, object values, string role = null, bool allowError = false)
    {
        var navigation = ObserveNavigation(browser, allowError);
        var roleScript = role == null ? "" : "form.elements.RoleId.value=" + role + ";";
        await browser.ExecuteScriptAsync("(() => { const form=document.querySelector('[name=" + field + "]').form;"
            + "Object.entries(" + Json.Serialize(values) + ").forEach(([key,value])=>{if(form.elements[key]) form.elements[key].value=value;});"
            + roleScript + "form.querySelector('button[type=submit]').click(); })()");
        await navigation;
    }


    private static async Task AssertViewport(CoreWebView2 browser, string page, int width)
    {
        if (!await Check(browser, "window.innerWidth === " + width + " && document.documentElement.scrollWidth <= window.innerWidth && document.documentElement.lang === 'vi' && !document.body.innerText.includes(String.fromCharCode(65533))"))
        {
            var detail = await browser.ExecuteScriptAsync("({viewport:innerWidth, content:document.documentElement.scrollWidth, elements:[...document.querySelectorAll('main *')].filter(e=>!e.closest('.table-card,.table-responsive')&&e.getBoundingClientRect().right>innerWidth+1).slice(0,5).map(e=>e.tagName+'.'+e.className)})");
            throw new InvalidOperationException(page + " vi phạm bố cục ở " + width + "px. " + detail);
        }
    }

    private static async Task<bool> Check(CoreWebView2 browser, string script)
    {
        return await browser.ExecuteScriptAsync(script) == "true";
    }

    private static Task Resize(CoreWebView2 browser, int width)
    {
        return browser.CallDevToolsProtocolMethodAsync("Emulation.setDeviceMetricsOverride",
            "{\"width\":" + width + ",\"height\":900,\"deviceScaleFactor\":1,\"mobile\":false}");
    }

    private static async Task Navigate(CoreWebView2 browser, string url, bool allowError = false)
    {
        var finished = ObserveNavigation(browser, allowError);
        browser.Navigate(url);
        await finished;
    }

    private static async Task ObserveNavigation(CoreWebView2 browser, bool allowError = false)
    {
        var completion = new TaskCompletionSource<bool>();
        EventHandler<CoreWebView2NavigationCompletedEventArgs> listener =
            (sender, result) => completion.TrySetResult(result.IsSuccess || (allowError && result.HttpStatusCode >= 400));
        browser.NavigationCompleted += listener;
        try
        {
            if (await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(90))) != completion.Task)
                throw new TimeoutException("Quá thời gian chờ tải trang.");
            Require(await completion.Task, "Trình duyệt không tải được trang.");
        }
        finally
        {
            browser.NavigationCompleted -= listener;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void Write(string log, string text)
    {
        File.AppendAllText(log, text + Environment.NewLine, new UTF8Encoding(false));
    }
}
