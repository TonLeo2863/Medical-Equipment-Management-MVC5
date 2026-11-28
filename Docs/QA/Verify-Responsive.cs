using System;
using System.Drawing;
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
        var user = Json.Serialize(Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_USER"));
        var password = Json.Serialize(Environment.GetEnvironmentVariable("MEDICAL_QA_BROWSER_PASSWORD"));
        var loginNavigation = ObserveNavigation(browser);
        await browser.ExecuteScriptAsync("(() => { const username = document.querySelector('[name=username]'); const password = document.querySelector('[name=password]'); username.value = "
            + user + "; password.value = " + password + "; username.form.requestSubmit(); })()");
        await loginNavigation;
        Require(await Check(browser, "!!window.Chart && !!Chart.getChart('asset-states') && !!Chart.getChart('asset-departments') && !!Chart.getChart('maintenance-costs') && Chart.getChart('asset-states').data.datasets[0].data.some(n => n > 0)"),
            "Ba biểu đồ cần khởi tạo với dữ liệu thiết bị thật.");
        Write(log, "PASS Edge WebView2 hiển thị cả ba biểu đồ với dữ liệu thực");

        foreach (var width in new[] { 390, 768, 1280 })
        {
            await Resize(browser, width);
            await Navigate(browser, baseUrl + "/");
            await AssertViewport(browser, "Dashboard", width);
            await Navigate(browser, baseUrl + "/Equipment");
            await AssertViewport(browser, "Thiết bị", width);
            Write(log, "PASS Edge WebView2 tiếng Việt và bố cục Dashboard/Thiết bị ở " + width + "px");
        }

        await Resize(browser, 390);
        await Navigate(browser, baseUrl + "/Equipment");
        Require(await Check(browser, "(() => {document.querySelector('label[for=navToggle]').click(); return getComputedStyle(document.querySelector('.nav-links')).display !== 'none';})()"),
            "Điều hướng di động cần mở được bằng nút Menu.");
        Write(log, "PASS Edge WebView2 mở điều hướng di động");
    }

    private static async Task AssertViewport(CoreWebView2 browser, string page, int width)
    {
        Require(await Check(browser, "window.innerWidth === " + width + " && document.documentElement.scrollWidth <= window.innerWidth && document.documentElement.lang === 'vi' && !document.body.innerText.includes(String.fromCharCode(65533))"),
            page + " có tràn ngang, sai chiều rộng hoặc lỗi hiển thị tiếng Việt ở " + width + "px.");
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

    private static async Task Navigate(CoreWebView2 browser, string url)
    {
        var finished = ObserveNavigation(browser);
        browser.Navigate(url);
        await finished;
    }

    private static async Task ObserveNavigation(CoreWebView2 browser)
    {
        var completion = new TaskCompletionSource<bool>();
        EventHandler<CoreWebView2NavigationCompletedEventArgs> listener =
            (sender, result) => completion.TrySetResult(result.IsSuccess);
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
