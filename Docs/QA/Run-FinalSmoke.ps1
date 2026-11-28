param([int]$Port = 51123, [switch]$BrowserOnly, [switch]$CatalogOnly)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http, System.Data
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$app = Join-Path $root 'MedicalEquipmentManagement'
[xml]$webConfig = [IO.File]::ReadAllText((Join-Path $app 'Web.config'))
$appConnection = $webConfig.configuration.connectionStrings.add | Where-Object name -eq 'MedicalEquipmentConnection'
$appTarget = [Data.SqlClient.SqlConnectionStringBuilder]::new([string]$appConnection.connectionString)
$allowedServers = @('localhost\SQLEXPRESS', '.\SQLEXPRESS', '(local)\SQLEXPRESS', ($env:COMPUTERNAME + '\SQLEXPRESS'))
if ($appTarget.InitialCatalog -ne 'MedicalEquipmentDB_BetaTest' -or $appTarget.DataSource -notin $allowedServers) {
    throw 'Cấu hình IIS phải trỏ đến SQLEXPRESS cục bộ và MedicalEquipmentDB_BetaTest.'
}
Add-Type -Path (Join-Path $app 'bin/MedicalEquipmentManagement.dll')
$connection = New-Object Data.SqlClient.SqlConnection 'Data Source=localhost\SQLEXPRESS;Initial Catalog=MedicalEquipmentDB_BetaTest;Integrated Security=True;Connect Timeout=5'
$connection.Open()
$prefix = 'rcqa_' + [Guid]::NewGuid().ToString('N').Substring(0,12)
$process = $null
$assetId = $null
$clients = @{}
$sampleCodes = New-Object 'System.Collections.Generic.HashSet[string]'
$cookieJars = @{}

# Tất cả câu ghi đi qua kết nối đã xác minh và chỉ tác động đến fixture của lần chạy này.
function Sql($query, [hashtable]$parameters = @{}) {
    $command = $connection.CreateCommand()
    try {
        $command.CommandText = $query
        foreach ($key in $parameters.Keys) {
            [void]$command.Parameters.AddWithValue('@' + $key, $parameters[$key])
        }
        $command.ExecuteScalar()
    } finally { $command.Dispose() }
}

function Assert-Equal($label, $actual, $expected) {
    if ($actual -ne $expected) { throw "FAIL $label. Thực tế $actual, kỳ vọng $expected." }
    Write-Output "PASS $label"
}

function Invoke-Request($client, [string]$method, [string]$path, [hashtable]$form, [string]$json, [string]$token) {
    $request = New-Object Net.Http.HttpRequestMessage ([Net.Http.HttpMethod]::new($method)), "http://localhost:$Port$path"
    if ($form) {
        $formEncoded = ($form.GetEnumerator() | ForEach-Object {
            [Uri]::EscapeDataString([string]$_.Key) + '=' + [Uri]::EscapeDataString([string]$_.Value)
        }) -join '&'
        $request.Content = New-Object Net.Http.StringContent $formEncoded, ([Text.Encoding]::UTF8), 'application/x-www-form-urlencoded'
    }
    if ($json) { $request.Content = New-Object Net.Http.StringContent $json, ([Text.Encoding]::UTF8), 'application/json' }
    if ($token) { [void]$request.Headers.TryAddWithoutValidation('RequestVerificationToken', $token) }
    $timer = [Diagnostics.Stopwatch]::StartNew()
    try {
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        try {
            $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            [pscustomobject]@{
                Status = [int]$response.StatusCode; Path = [string]$response.Headers.Location
                Body = $body; Milliseconds = $timer.ElapsedMilliseconds
                Timing = [regex]::Match($response.Headers.ToString(), '(?im)^Server-Timing: (.*)$').Groups[1].Value.Trim()
            }
        } finally { $response.Dispose() }
    } finally { $request.Dispose() }
}

function Read-Token($page) {
    if ($page.Status -ne 200) { throw "Không lấy được trang token, HTTP $($page.Status)." }
    $match = [regex]::Match($page.Body, '<input[^>]*name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (!$match.Success) { throw 'Trang phản hồi thiếu token chống CSRF.' }
    [Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}

function Post-Form($client, $path, $fields) {
    $fields['__RequestVerificationToken'] = Read-Token (Invoke-Request $client GET $path)
    Invoke-Request $client POST $path $fields
}

function Post-Excel($client, $filename, [byte[]]$bytes) {
    $token = Read-Token (Invoke-Request $client GET '/Equipment/Import')
    $form = New-Object Net.Http.MultipartFormDataContent
    $form.Add([Net.Http.StringContent]::new($token), '__RequestVerificationToken')
    $form.Add([Net.Http.ByteArrayContent]::new($bytes), 'excelFile', $filename)
    try {
        $response = $client.PostAsync("http://localhost:$Port/Equipment/Import", $form).GetAwaiter().GetResult()
        try {
            Assert-Equal "HTTP nhập $filename" ([int]$response.StatusCode) 200 | Write-Host
            [Net.WebUtility]::HtmlDecode($response.Content.ReadAsStringAsync().GetAwaiter().GetResult())
        } finally { $response.Dispose() }
    } finally { $form.Dispose() }
}

function Invoke-BrowserCheck {
    if ($env:MEDICAL_QA_WEBVIEW_EXECUTABLE) {
        $env:MEDICAL_QA_BROWSER_USER = $prefix + '_ADMIN'
        $env:MEDICAL_QA_BROWSER_PASSWORD = $secret
        $env:MEDICAL_QA_BROWSER_ASSET_ID = [string]$assetId
        try {
            $browserLog = [IO.Path]::GetTempFileName()
            $browser = Start-Process -FilePath $env:MEDICAL_QA_WEBVIEW_EXECUTABLE -ArgumentList @("http://localhost:$Port", ('"' + $browserLog + '"')) -WindowStyle Hidden -Wait -PassThru
            Get-Content -Encoding UTF8 $browserLog
            if ($browser.ExitCode -ne 0) { throw 'Kiểm thử Edge WebView2 thất bại.' }
        } finally {
            Remove-Item Env:MEDICAL_QA_BROWSER_USER, Env:MEDICAL_QA_BROWSER_PASSWORD, Env:MEDICAL_QA_BROWSER_ASSET_ID -ErrorAction SilentlyContinue
        }
    } else { Write-Output 'NOT RUN Kiểm thử bố cục vì chưa cấp trình duyệt QA.' }
}

function Test-AcceptanceWorkbooks($client) {
    # Đổi tiền tố mã/Serial trong bộ nhớ để retest tệp bàn giao mà không đụng dữ liệu thành viên khác.
    $cases = @(
        @{File='hop-le.xlsx'; Success=1; Message='Đã nhập vào mẫu'; Package=$true},
        @{File='trung-ma-tai-san.xlsx'; Success=1; Message='đã tồn tại'; Package=$true},
        @{File='trung-serial.xlsx'; Success=1; Message='đã tồn tại'; Package=$true},
        @{File='gia-am.xlsx'; Success=0; Message='Ngày mua, giá mua hoặc trạng thái không hợp lệ'; Package=$true},
        @{File='sai-model-hoac-khoa.xlsx'; Success=0; Message='RCUI-KHOA-SAI'; Package=$true},
        @{File='tep-hong.xlsx'; Success=0; Message='Không đọc được file Excel'; Package=$false},
        @{File='qua-2mb.xlsx'; Success=0; Message='vượt quá 2 MB'; Package=$false}
    )
    foreach ($case in $cases) {
        $codes = @()
        $bytes = [IO.File]::ReadAllBytes((Join-Path $PSScriptRoot ('TestData/' + $case.File)))
        if ($case.Package) {
            $inputStream = [IO.MemoryStream]::new($bytes)
            $outputStream = [IO.MemoryStream]::new()
            $sampleBook = [ClosedXML.Excel.XLWorkbook]::new($inputStream)
            try {
                $sheet = $sampleBook.Worksheet(1)
                for ($row = 2; $row -le $sheet.LastRowUsed().RowNumber(); $row++) {
                    $code = $prefix + '_' + $sheet.Cell($row,1).GetString()
                    $sheet.Cell($row,1).Value = $code
                    $sheet.Cell($row,2).Value = $prefix + '_' + $sheet.Cell($row,2).GetString()
                    [void]$sampleCodes.Add($code)
                    $codes += $code
                }
                $sampleBook.SaveAs($outputStream)
                $bytes = $outputStream.ToArray()
            } finally { $sampleBook.Dispose(); $inputStream.Dispose(); $outputStream.Dispose() }
        }
        $body = Post-Excel $client $case.File $bytes
        Assert-Equal "Tệp nghiệm thu $($case.File) báo đúng kết quả" $body.Contains($case.Message) $true
        if ($case.Package) {
            $saved = 0
            foreach ($code in ($codes | Select-Object -Unique)) {
                $saved += Sql 'SELECT COUNT(*) FROM dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$code}
            }
            Assert-Equal "Tệp nghiệm thu $($case.File) lưu đúng số dòng" $saved $case.Success
        }
    }
}

function Assert-MvcWritePermissions {
    # Dùng ID không tồn tại và form trống để phát hiện route mất phân quyền mà không ghi vào dữ liệu hiện có.
    $tests = @(
        @{ Allowed=@('ADMIN','MANAGER'); Paths=@('/Category/Create','/Category/Edit/2147483647','/Category/Delete/2147483647','/Supplier/Create','/Supplier/Edit/2147483647','/Supplier/Delete/2147483647','/EquipmentModel/Create','/EquipmentModel/Edit/2147483647','/EquipmentModel/Delete/2147483647','/Inventory/Transfer') },
        @{ Allowed=@('ADMIN'); Paths=@('/Department/Create','/Department/Edit/2147483647','/Department/Delete/2147483647','/UserAdmin/Edit/2147483647','/UserAdmin/ResetPassword/2147483647') },
        @{ Allowed=@('ADMIN','ENGINEER'); Paths=@('/Maintenance/Create','/Calibration/Create') },
        @{ Allowed=@('ADMIN','MANAGER','STAFF'); Paths=@('/WorkOrder/Create') },
        @{ Allowed=@('ADMIN','MANAGER','ENGINEER'); Paths=@('/WorkOrder/Edit/2147483647') }
    )
    foreach ($role in @('ADMIN','MANAGER','ENGINEER','STAFF')) {
        $browser = $clients[$role]
        $token = Read-Token (Invoke-Request $browser GET '/')
        foreach ($test in $tests) {
            if ($role -in $test.Allowed) { continue }
            foreach ($route in $test.Paths) {
                $get = Invoke-Request $browser GET $route
                $post = Invoke-Request $browser POST $route @{__RequestVerificationToken=$token}
                Assert-Equal "$role bị từ chối GET/POST $route" ($get.Status -eq 302 -and $post.Status -eq 302 -and $get.Path -like '*AccessDenied*' -and $post.Path -like '*AccessDenied*') $true
            }
        }
    }
}

function Invoke-CatalogChecks($admin) {
    $category = @{CategoryCode=$prefix+'C';Name='Danh mục kiểm thử';IsActive='true'}
    Assert-Equal 'Tạo Category' (Post-Form $admin '/Category/Create' $category).Status 302
    $categoryId = Sql 'SELECT CategoryId FROM dbo.EquipmentCategories WHERE CategoryCode=@code' @{code=$category.CategoryCode}
    Assert-Equal 'Đọc Category' (Invoke-Request $admin GET "/Category/Details/$categoryId").Status 200
    $category['CategoryId'] = $categoryId
    $category['Name'] = 'Danh mục cập nhật'
    Assert-Equal 'Sửa Category' (Post-Form $admin "/Category/Edit/$categoryId" $category).Status 302
    Assert-Equal 'Category cập nhật trong SQL' (Sql 'SELECT Name FROM dbo.EquipmentCategories WHERE CategoryId=@id' @{id=$categoryId}) $category.Name

    $supplier = @{Name=$prefix+'_SUPPLIER'; SupplierType='Manufacturer'; IsActive='true'}
    Assert-Equal 'Tạo Supplier' (Post-Form $admin '/Supplier/Create' $supplier).Status 302
    $supplierId = Sql 'SELECT SupplierId FROM dbo.Suppliers WHERE Name=@name' @{name=$supplier.Name}
    Assert-Equal 'Đọc Supplier' (Invoke-Request $admin GET "/Supplier/Details/$supplierId").Status 200
    $supplier['SupplierId'] = $supplierId
    $supplier['Phone'] = '0281234567'
    Assert-Equal 'Sửa Supplier' (Post-Form $admin "/Supplier/Edit/$supplierId" $supplier).Status 302
    Assert-Equal 'Supplier cập nhật trong SQL' (Sql 'SELECT Phone FROM dbo.Suppliers WHERE SupplierId=@id' @{id=$supplierId}) $supplier.Phone

    $model = @{ModelCode=$prefix+'M';ModelName='Mẫu máy kiểm thử';CategoryId=$categoryId;ManufacturerId=$supplierId;IsActive='true'}
    Assert-Equal 'Tạo EquipmentModel' (Post-Form $admin '/EquipmentModel/Create' $model).Status 302
    $createdModelId = Sql 'SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode=@code' @{code=$model.ModelCode}
    Assert-Equal 'Đọc EquipmentModel' (Invoke-Request $admin GET "/EquipmentModel/Details/$createdModelId").Status 200
    $model['ModelId'] = $createdModelId
    $model['ModelName'] = 'Mẫu máy đã cập nhật'
    Assert-Equal 'Sửa EquipmentModel' (Post-Form $admin "/EquipmentModel/Edit/$createdModelId" $model).Status 302
    Assert-Equal 'EquipmentModel cập nhật trong SQL' (Sql 'SELECT Name FROM dbo.EquipmentModels WHERE ModelId=@id' @{id=$createdModelId}) $model.ModelName
    # Dùng danh mục fixture đang có mẫu để kiểm tra banner lỗi thật, kể cả sau khi tải lại.
    $refusedDelete = Post-Form $admin "/Category/Delete/$categoryId" @{}
    Assert-Equal 'Chuyển trang sau khi từ chối xóa danh mục' $refusedDelete.Status 302
    $errorPage = Invoke-Request $admin GET $refusedDelete.Path
    $errorBanner = '<div class="notice notice-error" role="alert"><strong>Lỗi</strong>'
    Assert-Equal 'Banner lỗi có nhãn và lý do không thể xóa' ($errorPage.Body.Contains($errorBanner) -and [Net.WebUtility]::HtmlDecode($errorPage.Body).Contains('Không thể xóa danh mục')) $true
    Assert-Equal 'Banner lỗi chỉ hiển thị một lần sau redirect' (Invoke-Request $admin GET $refusedDelete.Path).Body.Contains($errorBanner) $false
    Assert-Equal 'Category còn mẫu thiết bị không thể xóa' (Sql 'SELECT COUNT(*) FROM dbo.EquipmentCategories WHERE CategoryId=@id' @{id=$categoryId}) 1
    Assert-Equal 'Xóa EquipmentModel' (Post-Form $admin "/EquipmentModel/Delete/$createdModelId" @{}).Status 302
    Assert-Equal 'Xóa Supplier' (Post-Form $admin "/Supplier/Delete/$supplierId" @{}).Status 302
    Assert-Equal 'Xóa Category' (Post-Form $admin "/Category/Delete/$categoryId" @{}).Status 302
    Assert-Equal 'Ba bản ghi danh mục đã được xóa' (Sql 'SELECT (SELECT COUNT(*) FROM dbo.EquipmentModels WHERE ModelId=@m)+(SELECT COUNT(*) FROM dbo.Suppliers WHERE SupplierId=@s)+(SELECT COUNT(*) FROM dbo.EquipmentCategories WHERE CategoryId=@c)' @{m=$createdModelId;s=$supplierId;c=$categoryId}) 0

    $department = @{DepartmentCode=$prefix+'D';Name='Khoa kiểm thử';Location='Khu A';IsActive='true'}
    Assert-Equal 'Tạo Department' (Post-Form $admin '/Department/Create' $department).Status 302
    $createdDepartmentId = Sql 'SELECT DepartmentId FROM dbo.Departments WHERE DepartmentCode=@code' @{code=$department.DepartmentCode}
    Assert-Equal 'Đọc Department' (Invoke-Request $admin GET "/Department/Details/$createdDepartmentId").Status 200
    $department['DepartmentId'] = $createdDepartmentId
    $department['Location'] = 'Khu B'
    Assert-Equal 'Sửa Department' (Post-Form $admin "/Department/Edit/$createdDepartmentId" $department).Status 302
    Assert-Equal 'Department cập nhật trong SQL' (Sql 'SELECT Location FROM dbo.Departments WHERE DepartmentId=@id' @{id=$createdDepartmentId}) $department.Location
    Assert-Equal 'Xóa Department' (Post-Form $admin "/Department/Delete/$createdDepartmentId" @{}).Status 302
    Assert-Equal 'Department đã được xóa' (Sql 'SELECT COUNT(*) FROM dbo.Departments WHERE DepartmentId=@id' @{id=$createdDepartmentId}) 0

    $mvcModelId = Sql 'SELECT ModelId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}
    $deviceCode = $prefix + '_MVC'
    $device = @{AssetCode=$deviceCode;SerialNumber=$deviceCode;ModelId=$mvcModelId;DepartmentId=$source;PurchasePrice='1200';Status='Đang hoạt động'}
    Assert-Equal 'Tạo EquipmentAsset qua MVC' (Post-Form $admin '/Equipment/Create' $device).Status 302
    $mvcId = Sql 'SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$deviceCode}
    Assert-Equal 'Đọc EquipmentAsset qua MVC' (Invoke-Request $admin GET "/Equipment/Details/$mvcId").Status 200
    $device['AssetId'] = $mvcId
    $device['PurchasePrice'] = '1560'
    Assert-Equal 'Sửa EquipmentAsset qua MVC' (Post-Form $admin "/Equipment/Edit/$mvcId" $device).Status 302
    Assert-Equal 'MVC cập nhật đúng giá thiết bị' (Sql 'SELECT PurchasePrice FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$mvcId}) 1560
    $device['DepartmentId'] = $destination
    Assert-Equal 'MVC từ chối đổi khoa trực tiếp' (Post-Form $admin "/Equipment/Edit/$mvcId" $device).Status 200
    Assert-Equal 'MVC giữ khoa nguồn sau yêu cầu bị từ chối' (Sql 'SELECT DepartmentId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$mvcId}) $source
    Assert-Equal 'MVC xóa mềm EquipmentAsset' (Post-Form $admin "/Equipment/Delete/$mvcId" @{}).Status 302
    Assert-Equal 'MVC giữ bản ghi với cờ IsDeleted' (Sql 'SELECT IsDeleted FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$mvcId}) $true
    Assert-Equal 'MVC ẩn thiết bị đã xóa mềm' (Invoke-Request $admin GET "/Equipment/Details/$mvcId").Status 404

}

try {
    if ((Sql 'SELECT DB_NAME()') -ne 'MedicalEquipmentDB_BetaTest') { throw 'Sai database đích.' }
    if (!$BrowserOnly -and !$CatalogOnly) {
        $salt = [byte[]](0..15)
        $phrase = 'Chuỗi kiểm thử cố định'
        $managed = New-Object Security.Cryptography.Rfc2898DeriveBytes $phrase, $salt, 600000, ([Security.Cryptography.HashAlgorithmName]::SHA256)
        $watch = [Diagnostics.Stopwatch]::StartNew()
        try { $expected = $managed.GetBytes(32) } finally { $managed.Dispose() }
        $managedMs = $watch.ElapsedMilliseconds
        $vector = 'pbkdf2-sha256-v1$' + [Convert]::ToBase64String($salt) + '$' + [Convert]::ToBase64String($expected)
        $watch.Restart()
        $same = [MedicalEquipmentManagement.Helpers.PasswordHasher]::Verify($phrase, $vector)
        $nativeMs = $watch.ElapsedMilliseconds
        Assert-Equal 'PBKDF2 CNG tương thích triển khai cũ' $same $true
        Assert-Equal 'PBKDF2 từ chối mật khẩu sai' ([MedicalEquipmentManagement.Helpers.PasswordHasher]::Verify('Sai', $vector)) $false
        Write-Output "BENCH trực tiếp managed=${managedMs}ms cng=${nativeMs}ms"

    }

    $secret = [Guid]::NewGuid().ToString('N') + [Guid]::NewGuid().ToString('N')
    $hash = [MedicalEquipmentManagement.Helpers.PasswordHasher]::Hash($secret)
    foreach ($role in @('ADMIN', 'MANAGER', 'ENGINEER', 'STAFF', 'DISABLED', 'OTHERSTAFF')) {
        $roleCode = if ($role -in @('DISABLED','OTHERSTAFF')) { 'STAFF' } else { $role }
        $null = Sql 'INSERT dbo.Users(Username,PasswordHash,FullName,RoleId,IsActive,CreatedAt) SELECT @user,@hash,@user,RoleId,@active,GETDATE() FROM dbo.Roles WHERE RoleCode=@role' @{
            user = $prefix + '_' + $role; hash = $hash; active = ($role -ne 'DISABLED'); role = $roleCode
        }
        $handler = New-Object Net.Http.HttpClientHandler
        $handler.AllowAutoRedirect = $false
        $handler.UseProxy = $false
        $cookieJars[$role] = $handler.CookieContainer
        $clients[$role] = New-Object Net.Http.HttpClient $handler
    }
    $assetId = Sql 'INSERT dbo.EquipmentAssets(AssetCode,SerialNumber,ModelId,DepartmentId,Status,IsDeleted) OUTPUT inserted.AssetId SELECT TOP (1) @code,@code,ModelId,(SELECT TOP (1) DepartmentId FROM dbo.Departments WHERE IsActive=1 ORDER BY DepartmentId),N''Đang hoạt động'',0 FROM dbo.EquipmentModels WHERE IsActive=1 ORDER BY ModelId' @{code=$prefix}
    if (!$assetId) { throw 'Thiếu mẫu thiết bị hoạt động để tạo fixture.' }
    $source = Sql 'SELECT DepartmentId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}
    $destination = Sql 'SELECT TOP (1) DepartmentId FROM dbo.Departments WHERE IsActive=1 AND DepartmentId<>@src ORDER BY DepartmentId' @{src=$source}
    $rawSql = [Diagnostics.Stopwatch]::StartNew()
    $null = Sql 'SELECT UserId FROM dbo.Users WHERE Username=@user' @{user=$prefix + '_ADMIN'}
    Write-Output "BENCH SQL truy vấn trực tiếp, kết nối mở sẵn $($rawSql.ElapsedMilliseconds)ms"

    $env:MEDICAL_QA_TIMINGS = '1'
    $exe = 'C:\Program Files\IIS Express\iisexpress.exe'
    $process = Start-Process $exe -ArgumentList @("/path:$app", "/port:$Port", '/clr:v4.0') -PassThru -WindowStyle Hidden
    $guest = $clients['DISABLED']
    $up = $false
    for ($retry = 0; $retry -lt 30; $retry++) {
        if ($process.HasExited) { throw 'IIS Express đã dừng trước khi nhận request.' }
        try {
            if ((Invoke-Request $guest GET '/Account/Login').Status -eq 200) { $up = $true; break }
        } catch [Net.Http.HttpRequestException] {}
        Start-Sleep -Milliseconds 400
    }
    if (!$up) { throw 'IIS Express không phục vụ trang đăng nhập.' }
    if ($BrowserOnly) {
        if (!$env:MEDICAL_QA_WEBVIEW_EXECUTABLE) { throw 'Thiếu trình duyệt QA.' }
        Invoke-BrowserCheck
        return
    }
    if ($CatalogOnly) {
        $admin = $clients['ADMIN']
        Assert-Equal 'ADMIN đăng nhập cho nhóm kiểm tra danh mục' (Post-Form $admin '/Account/Login' @{username=$prefix+'_ADMIN';password=$secret}).Status 302
        Invoke-CatalogChecks $admin
        return
    }
    Assert-Equal 'API khách chưa đăng nhập' (Invoke-Request $guest GET '/api/equipments').Status 401

    foreach ($role in @('ADMIN','MANAGER','ENGINEER','STAFF','OTHERSTAFF','DISABLED')) {
        $client = $clients[$role]
        $login = Post-Form $client '/Account/Login' @{ username = $prefix + '_' + $role; password = $secret }
        $expectedStatus = if ($role -eq 'DISABLED') { 200 } else { 302 }
        Assert-Equal "Đăng nhập $role" $login.Status $expectedStatus
        Write-Output "BENCH HTTP $role $($login.Milliseconds)ms $($login.Timing)"
        if ($role -ne 'DISABLED') {
            Assert-Equal "Dashboard $role" (Invoke-Request $client GET '/').Status 200
            Assert-Equal "API đọc $role" (Invoke-Request $client GET "/api/equipments/$assetId").Status 200
        }
    }

    $admin = $clients['ADMIN']; $manager = $clients['MANAGER']; $engineer = $clients['ENGINEER']; $staff = $clients['STAFF']
    $loopback = [Uri]"http://localhost:$Port"
    $sessionCookie = $cookieJars['ADMIN'].GetCookies($loopback)['ASP.NET_SessionId']
    $bindingCookie = $cookieJars['ADMIN'].GetCookies($loopback)['MedicalEquipmentLogin']
    Assert-Equal 'Cookie đăng nhập có HttpOnly' $bindingCookie.HttpOnly $true
    $singleCookieHandler = [Net.Http.HttpClientHandler]::new()
    $singleCookieHandler.UseProxy = $false
    $singleCookieHandler.CookieContainer.Add($loopback, [Net.Cookie]::new($sessionCookie.Name, $sessionCookie.Value, '/'))
    $singleCookieClient = [Net.Http.HttpClient]::new($singleCookieHandler)
    try {
        Assert-Equal 'Chỉ có Session ID không đủ để truy cập API' (Invoke-Request $singleCookieClient GET '/api/equipments').Status 401
    } finally { $singleCookieClient.Dispose() }
    Assert-MvcWritePermissions
    Assert-Equal 'Sai mật khẩu' (Post-Form $guest '/Account/Login' @{username=$prefix + '_STAFF';password='Sai'}).Status 200
    Assert-Equal 'Quản trị tài khoản ADMIN' (Invoke-Request $admin GET '/UserAdmin').Status 200
    Assert-Equal 'Quản trị tài khoản MANAGER bị từ chối' (Invoke-Request $manager GET '/UserAdmin').Status 302
    foreach ($role in @('ENGINEER','STAFF')) {
        $client = $clients[$role]
        foreach ($path in @('/Equipment/Create',"/Equipment/Edit/$assetId","/Equipment/Delete/$assetId",'/Equipment/Import')) {
            $blocked = Invoke-Request $client GET $path
            Assert-Equal "$role bị từ chối $path" $blocked.Status 302
            if ($blocked.Path -notlike '*AccessDenied*') { throw "Redirect quyền không đúng ở $path." }
            $validToken = Read-Token (Invoke-Request $client GET '/')
            $blockedPost = Invoke-Request $client POST $path @{__RequestVerificationToken=$validToken;AssetId=$assetId}
            Assert-Equal "$role bị từ chối POST $path" $blockedPost.Status 302
            if ($blockedPost.Path -notlike '*AccessDenied*') { throw "Redirect quyền POST không đúng ở $path." }
        }
        Assert-Equal "API ghi bị từ chối $role" (Invoke-Request $client POST '/api/equipments' $null '{}').Status 403
    }
    Assert-Equal 'API ghi thiếu CSRF' (Invoke-Request $admin POST '/api/equipments' $null '{}').Status 400
    $modelId = Sql 'SELECT ModelId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}
    $apiCode = $prefix + '_API'
    $apiPayload = @{AssetCode=$apiCode;SerialNumber=$apiCode;ModelId=$modelId;Status='Đang hoạt động';PurchasePrice=1200}
    $apiToken = Read-Token (Invoke-Request $admin GET '/Equipment/Create')
    Assert-Equal 'API tạo thiết bị' (Invoke-Request $admin POST '/api/equipments' $null ($apiPayload | ConvertTo-Json -Compress) $apiToken).Status 201
    $apiId = Sql 'SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$apiCode}
    Assert-Equal 'API từ chối mã trùng' (Invoke-Request $admin POST '/api/equipments' $null ($apiPayload | ConvertTo-Json -Compress) $apiToken).Status 409
    $apiPayload['PurchasePrice'] = 1500
    Assert-Equal 'API cập nhật giá' (Invoke-Request $admin PUT "/api/equipments/$apiId" $null ($apiPayload | ConvertTo-Json -Compress) $apiToken).Status 204
    Assert-Equal 'Giá mới lưu vào SQL Server' (Sql 'SELECT PurchasePrice FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$apiId}) 1500
    $apiPayload['PurchasePrice'] = -1
    Assert-Equal 'API từ chối giá âm' (Invoke-Request $admin PUT "/api/equipments/$apiId" $null ($apiPayload | ConvertTo-Json -Compress) $apiToken).Status 400
    Assert-Equal 'API xóa mềm thiết bị' (Invoke-Request $admin DELETE "/api/equipments/$apiId" $null $null $apiToken).Status 204
    Assert-Equal 'API ẩn thiết bị sau xóa mềm' (Invoke-Request $admin GET "/api/equipments/$apiId").Status 404
    Assert-Equal 'Chi tiết tài sản' (Invoke-Request $admin GET "/Equipment/Details/$assetId").Status 200
    foreach ($page in @(1,2)) {
        $assetPage = Invoke-Request $admin GET "/Equipment?sortBy=price&sortDir=desc&page=$page"
        Assert-Equal "Danh sách trang $page" $assetPage.Status 200
        $codes = [regex]::Matches($assetPage.Body, '<td><strong>([^<]+)</strong></td>') | ForEach-Object { [Net.WebUtility]::HtmlDecode($_.Groups[1].Value) }
        $pageSize = 8
        $expectedCodes = Sql 'SELECT STRING_AGG(AssetCode,N''|'') WITHIN GROUP (ORDER BY Seq) FROM (SELECT AssetCode, ROW_NUMBER() OVER (ORDER BY PurchasePrice DESC,AssetId) AS Seq FROM dbo.EquipmentAssets WHERE IsDeleted=0) AS q WHERE Seq>@skip AND Seq<=@skip+@size' @{skip=($page-1)*$pageSize;size=$pageSize}
        Assert-Equal "Thứ tự giá và ranh giới trang $page khớp SQL" ($codes -join '|') $expectedCodes
    }
    Assert-Equal 'Mẫu Excel tải được' (Invoke-Request $admin GET '/Equipment/DownloadImportTemplate').Status 200

    Add-Type -Path (Join-Path $app 'bin/ClosedXML.dll')
    $modelCode = Sql 'SELECT ModelCode FROM dbo.EquipmentModels WHERE ModelId=@id' @{id=$modelId}
    $workbook = New-Object ClosedXML.Excel.XLWorkbook
    $stream = New-Object IO.MemoryStream
    try {
        $sheet = $workbook.Worksheets.Add('QA')
        $sheet.Cell(1,1).Value = 'AssetCode'
        $sheet.Cell(1,2).Value = 'SerialNumber'
        $sheet.Cell(1,3).Value = 'ModelCode'
        $sheet.Cell(2,1).Value = $prefix + '_XLSX'
        $sheet.Cell(2,2).Value = $prefix + '_XLSX'
        $sheet.Cell(2,3).Value = $modelCode
        $sheet.Cell(3,1).Value = $prefix + '_XLSX_BAD'
        $sheet.Cell(3,2).Value = $prefix + '_XLSX_BAD'
        $sheet.Cell(3,3).Value = $modelCode
        $sheet.Cell(3,6).Value = '-10'
        $workbook.SaveAs($stream)
        $bytes = $stream.ToArray()
    } finally { $stream.Dispose(); $workbook.Dispose() }
    $valid = Post-Excel $admin 'qa.xlsx' $bytes
    Assert-Equal 'Excel có thông báo dòng có giá âm' $valid.Contains('Ngày mua, giá mua hoặc trạng thái không hợp lệ.') $true
    Assert-Equal 'Excel lưu duy nhất dòng hợp lệ' (Sql 'SELECT COUNT(*) FROM dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$prefix+'_XLSX'}) 1
    Assert-Equal 'Excel bỏ qua dòng giá âm' (Sql 'SELECT COUNT(*) FROM dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$prefix+'_XLSX_BAD'}) 0
    $duplicate = Post-Excel $admin 'dup.xlsx' $bytes
    Assert-Equal 'Excel báo dòng trùng' $duplicate.Contains('đã tồn tại') $true
    $invalid = Post-Excel $admin 'loi.xlsx' ([byte[]](1,2,3))
    Assert-Equal 'Excel sai định dạng bị từ chối' $invalid.Contains('Không đọc được file Excel') $true
    $oversize = Post-Excel $admin 'qua-lon.xlsx' ([byte[]]::new(2097153))
    Assert-Equal 'Excel vượt 2 MB bị từ chối' $oversize.Contains('File Excel không được vượt quá 2 MB.') $true
    Test-AcceptanceWorkbooks $admin

    if ($env:MEDICAL_QA_BARCODE_DECODER) {
        Add-Type -AssemblyName System.Drawing
        Add-Type -Path $env:MEDICAL_QA_BARCODE_DECODER
        $reader = New-Object ZXing.BarcodeReader
        foreach ($kind in @('QrCode','Barcode')) {
            $content = $admin.GetByteArrayAsync("http://localhost:$Port/Equipment/$kind/$assetId").GetAwaiter().GetResult()
            $imageStream = [IO.MemoryStream]::new([byte[]]$content)
            $bitmap = [Drawing.Bitmap]::new($imageStream)
            try { Assert-Equal "$kind mã hóa đúng Serial" ($reader.Decode($bitmap).Text) $prefix }
            finally { $bitmap.Dispose(); $imageStream.Dispose() }
        }
    } else { Write-Output 'NOT RUN Giải mã QR và Barcode vì chưa cấp đường dẫn ZXing.' }
    if ($destination) {
        $tampered = @{AssetCode=$prefix;SerialNumber=$prefix;ModelId=$modelId;DepartmentId=$destination;Status='Đang hoạt động'}
        Assert-Equal 'API từ chối đổi khoa ngoài điều chuyển' (Invoke-Request $admin PUT "/api/equipments/$assetId" $null ($tampered | ConvertTo-Json -Compress) $apiToken).Status 409
        Assert-Equal 'API giữ khoa nguồn sau yêu cầu bị từ chối' (Sql 'SELECT DepartmentId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}) $source
        $pending = @()
        foreach ($client in @($admin,$manager)) {
            $token = Read-Token (Invoke-Request $client GET '/Inventory/Transfer')
            $fields = @{ AssetId=[string]$assetId; FromDepartmentId=[string]$source; ToDepartmentId=[string]$destination; Reason='Kiểm thử RC'; __RequestVerificationToken=$token }
            $fieldsTyped = New-Object 'System.Collections.Generic.Dictionary[string,string]'
            foreach ($key in $fields.Keys) { $fieldsTyped.Add($key, $fields[$key]) }
            $pending += [pscustomobject]@{ Client=$client; Body=[Net.Http.FormUrlEncodedContent]::new($fieldsTyped) }
        }
        $requests = @($pending | ForEach-Object { $_.Client.PostAsync("http://localhost:$Port/Inventory/Transfer", $_.Body) })
        try {
            $statuses = @($requests | ForEach-Object {
                $response = $_.GetAwaiter().GetResult()
                try { [int]$response.StatusCode } finally { $response.Dispose() }
            })
            Assert-Equal 'Hai yêu cầu đồng thời chỉ có một điều chuyển' (($statuses | Sort-Object) -join ',') '200,302'
        } finally { $pending | ForEach-Object { $_.Body.Dispose() } }
        Assert-Equal 'Vị trí sau điều chuyển' (Sql 'SELECT DepartmentId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}) $destination
        Assert-Equal 'Giao dịch kho sau điều chuyển' (Sql 'SELECT COUNT(*) FROM dbo.InventoryTransactions WHERE EquipmentId=@id' @{id=$assetId}) 1
        $rollback = Sql 'SET XACT_ABORT ON; BEGIN TRY BEGIN TRAN; UPDATE dbo.EquipmentAssets SET DepartmentId=@src WHERE AssetId=@id; UPDATE dbo.EquipmentAssets SET DepartmentId=-1 WHERE AssetId=@id; COMMIT; SELECT 0; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; SELECT ERROR_NUMBER(); END CATCH;' @{id=$assetId;src=$source}
        Assert-Equal 'Lỗi khóa ngoại kích hoạt rollback trên fixture SQL' $rollback 547
        Assert-Equal 'Rollback giữ vị trí trước transaction lỗi' (Sql 'SELECT DepartmentId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}) $destination
    }
    $employee = Sql 'SELECT UserId FROM dbo.Users WHERE Username=@user' @{user=$prefix + '_STAFF'}
    $technician = Sql 'SELECT UserId FROM dbo.Users WHERE Username=@user' @{user=$prefix + '_ENGINEER'}
    $create = Post-Form $staff '/WorkOrder/Create' @{AssetId=$assetId;IssueDescription='Báo hỏng QA';ReportedByUserId=$technician;AssignedToUserId=$technician;Status='Đã xong'}
    Assert-Equal 'STAFF báo hỏng' $create.Status 302
    $ticket = Sql 'SELECT TicketId FROM dbo.WorkOrders WHERE AssetId=@id AND ReportedBy=@uid AND ReporterVerified=1 AND AssignedToUserId IS NULL AND Status=N''Chờ xử lý''' @{id=$assetId;uid=$employee}
    if (!$ticket) { throw 'Phiếu mới sai người báo, người phân công hoặc trạng thái.' }
    Write-Output 'PASS SQL xác nhận phiếu lấy người báo từ Session và bỏ qua dữ liệu ngoài phạm vi'
    Assert-Equal 'STAFF khác không xem được phiếu' (Invoke-Request $clients['OTHERSTAFF'] GET "/WorkOrder/Details/$ticket").Status 404
    Assert-Equal 'ENGINEER chưa được phân công không xem phiếu' (Invoke-Request $engineer GET "/WorkOrder/Details/$ticket").Status 404
    Assert-Equal 'MANAGER phân công ENGINEER' (Post-Form $manager "/WorkOrder/Edit/$ticket" @{command='assign';engineerId=$technician}).Status 302
    Assert-Equal 'ENGINEER bắt đầu sửa' (Post-Form $engineer "/WorkOrder/Edit/$ticket" @{command='start'}).Status 302
    $today = (Get-Date).ToString('yyyy-MM-dd')
    $next = (Get-Date).AddDays(10).ToString('yyyy-MM-dd')
    $maintenance = Post-Form $engineer '/Maintenance/Create' @{AssetId=$assetId;MaintenanceType='Sửa chữa sự cố';MaintenanceDate=$today;NextMaintenanceDate=$next;Cost='1200';ResultStatus='Hoàn thành';TicketId=$ticket}
    Assert-Equal 'ENGINEER ghi bảo trì' $maintenance.Status 302
    Assert-Equal 'Bảo trì liên kết phiếu và người thực hiện' (Sql 'SELECT COUNT(*) FROM dbo.MaintenanceRecords WHERE AssetId=@id AND TicketId=@ticket AND PerformedByUserId=@user' @{id=$assetId;ticket=$ticket;user=$technician}) 1
    Assert-Equal 'STAFF tạo phiếu thứ hai cho cùng thiết bị' (Post-Form $staff '/WorkOrder/Create' @{AssetId=$assetId;IssueDescription='Sự cố thứ hai'}).Status 302
    $otherTicket = Sql 'SELECT TicketId FROM dbo.WorkOrders WHERE AssetId=@asset AND TicketId<>@ticket' @{asset=$assetId;ticket=$ticket}
    Assert-Equal 'ENGINEER hoàn thành sửa' (Post-Form $engineer "/WorkOrder/Edit/$ticket" @{command='complete';resolution='Đã thay phụ tùng';technicalPassed='true'}).Status 302
    Assert-Equal 'Trạng thái phiếu hoàn thành' (Sql 'SELECT Status FROM dbo.WorkOrders WHERE TicketId=@id' @{id=$ticket}) 'Đã xong'
    Assert-Equal 'Thiết bị chưa hoạt động lại khi còn phiếu mở' (Sql 'SELECT Status FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}) 'Đang bảo trì'
    $staffToken = Read-Token (Invoke-Request $staff GET "/WorkOrder/Details/$otherTicket")
    $deniedCancel = Invoke-Request $staff POST "/WorkOrder/Cancel/$otherTicket" @{__RequestVerificationToken=$staffToken;reason='Thử phân quyền'}
    Assert-Equal 'STAFF bị từ chối khi tự hủy phiếu' ($deniedCancel.Path -like '*AccessDenied*') $true
    $cancelToken = Read-Token (Invoke-Request $admin GET "/WorkOrder/Details/$otherTicket")
    Assert-Equal 'ADMIN hủy phiếu chưa phân công' (Invoke-Request $admin POST "/WorkOrder/Cancel/$otherTicket" @{__RequestVerificationToken=$cancelToken;reason='Lỗi báo trùng'}).Status 302
    Assert-Equal 'Hủy phiếu giữ lịch sử trong SQL' (Sql 'SELECT Status FROM dbo.WorkOrders WHERE TicketId=@id' @{id=$otherTicket}) 'Đã hủy'
    Assert-Equal 'API bảo toàn thiết bị có lịch sử bảo trì' (Invoke-Request $admin DELETE "/api/equipments/$assetId" $null $null $apiToken).Status 409
    Assert-Equal 'ENGINEER ghi hiệu chuẩn không đạt' (Post-Form $engineer '/Calibration/Create' @{AssetId=$assetId;CalibrationDate=$today;NextCalibrationDate=$next;PerformedBy='Kỹ sư QA';CertificateNumber=$prefix;Result='Không đạt'}).Status 302
    Assert-Equal 'Hiệu chuẩn không đạt đánh dấu hỏng' (Sql 'SELECT Status FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}) 'Hỏng'
    $currentDept = Sql 'SELECT DepartmentId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}
    $reactivate = @{AssetCode=$prefix;SerialNumber=$prefix;ModelId=$modelId;DepartmentId=$currentDept;Status='Đang hoạt động'}
    Assert-Equal 'API từ chối đưa thiết bị hiệu chuẩn không đạt vào hoạt động' (Invoke-Request $admin PUT "/api/equipments/$assetId" $null ($reactivate | ConvertTo-Json -Compress) $apiToken).Status 409
    Assert-Equal 'Danh sách hiệu chuẩn 15 ngày' (Invoke-Request $engineer GET '/Calibration?days=15').Status 200
    $dash = Invoke-Request $manager GET '/'
    Assert-Equal 'Dashboard có cảnh báo dữ liệu fixture' $dash.Body.Contains($prefix) $true
    $nonDeletedCount = Sql 'SELECT COUNT(*) FROM dbo.EquipmentAssets WHERE IsDeleted=0'
    Assert-Equal 'Dashboard tổng số thiết bị khớp SQL' $dash.Body.Contains("<span>Tổng thiết bị</span><strong>$nonDeletedCount</strong>") $true
    $rawStates = [regex]::Match($dash.Body, 'data-states="([^"]+)"').Groups[1].Value
    $states = [Net.WebUtility]::HtmlDecode($rawStates) | ConvertFrom-Json
    foreach ($item in $states) {
        Assert-Equal "Dashboard chỉ số $($item.Label) khớp SQL" $item.Count (Sql 'SELECT COUNT(*) FROM dbo.EquipmentAssets WHERE Status=@st AND IsDeleted=0' @{st=$item.Label})
    }
    $currentYear = (Get-Date).Year
    $rawCosts = [regex]::Match($dash.Body, 'data-costs="([^"]+)"').Groups[1].Value
    $monthlyCosts = [Net.WebUtility]::HtmlDecode($rawCosts) | ConvertFrom-Json
    for ($month=1; $month -le 12; $month++) {
        $actualCost = Sql 'SELECT COALESCE(SUM(Cost),0) FROM dbo.MaintenanceRecords WHERE YEAR(MaintenanceDate)=@year AND MONTH(MaintenanceDate)=@month' @{year=$currentYear;month=$month}
        Assert-Equal "Dashboard chi phí tháng $month khớp SQL" $monthlyCosts[$month-1] $actualCost
    }
    Assert-Equal 'Chart.js cục bộ phục vụ được' (Invoke-Request $manager GET '/Scripts/vendor/chart.umd.min.js').Status 200
    $null = Sql 'UPDATE dbo.Users SET IsActive=0 WHERE Username=@user' @{user=$prefix + '_OTHERSTAFF'}
    Assert-Equal 'API thu hồi phiên khi tài khoản bị vô hiệu hóa' (Invoke-Request $clients['OTHERSTAFF'] GET '/api/equipments').Status 401
    $logoutToken = Read-Token $dash
    Assert-Equal 'Logout GET bị từ chối' (Invoke-Request $manager GET '/Account/Logout').Status 405
    Assert-Equal 'GET Logout giữ phiên' (Invoke-Request $manager GET '/api/equipments').Status 200
    Assert-Equal 'Logout POST có token' (Invoke-Request $manager POST '/Account/Logout' @{__RequestVerificationToken=$logoutToken}).Status 302
    Assert-Equal 'API sau logout' (Invoke-Request $manager GET '/api/equipments').Status 401

    $staffRole = Sql 'SELECT RoleId FROM dbo.Roles WHERE RoleCode=@role' @{role='STAFF'}
    $username = $prefix + '_ACCOUNT'
    $createAccount = Post-Form $admin '/UserAdmin/Edit' @{Username=$username;FullName='Người dùng kiểm thử';RoleId=$staffRole;IsActive='true';InitialPassword=$secret;InitialConfirmation=$secret}
    Assert-Equal 'ADMIN tạo tài khoản' $createAccount.Status 302
    $accountId = Sql 'SELECT UserId FROM dbo.Users WHERE Username=@username' @{username=$username}
    $resetSecret = [Guid]::NewGuid().ToString('N')
    Assert-Equal 'ADMIN đặt lại mật khẩu' (Post-Form $admin "/UserAdmin/ResetPassword/$accountId" @{NewPassword=$resetSecret;Confirmation=$resetSecret}).Status 302
    Assert-Equal 'Mật khẩu ban đầu hết hiệu lực sau đặt lại' (Post-Form $guest '/Account/Login' @{username=$username;password=$secret}).Status 200
    Assert-Equal 'Đăng nhập bằng mật khẩu đặt lại' (Post-Form $guest '/Account/Login' @{username=$username;password=$resetSecret}).Status 302
    $changedSecret = [Guid]::NewGuid().ToString('N')
    Assert-Equal 'Đổi mật khẩu cá nhân' (Post-Form $guest '/Account/ChangePassword' @{CurrentPassword=$resetSecret;NewPassword=$changedSecret;Confirmation=$changedSecret;RoleId='1'}).Status 302
    Assert-Equal 'Trường vai trò giả bị bỏ qua khi đổi mật khẩu' (Sql 'SELECT RoleId FROM dbo.Users WHERE UserId=@id' @{id=$accountId}) $staffRole
    Assert-Equal 'ADMIN vô hiệu hóa tài khoản QA' (Post-Form $admin "/UserAdmin/Edit/$accountId" @{Username=$username;FullName='Người dùng kiểm thử';RoleId=$staffRole;IsActive='false'}).Status 302
    Assert-Equal 'Phiên bị vô hiệu hóa không thể đọc API' (Invoke-Request $guest GET '/api/equipments').Status 401
    Assert-Equal 'ADMIN kích hoạt lại tài khoản QA' (Post-Form $admin "/UserAdmin/Edit/$accountId" @{Username=$username;FullName='Người dùng kiểm thử';RoleId=$staffRole;IsActive='true'}).Status 302
    Assert-Equal 'Đăng nhập bằng mật khẩu mới sau khi kích hoạt' (Post-Form $guest '/Account/Login' @{username=$username;password=$changedSecret}).Status 302

    Invoke-CatalogChecks $admin
    Invoke-BrowserCheck
} finally {
    if ($process -and !$process.HasExited) { $process.Kill(); $process.WaitForExit() }
    foreach ($client in $clients.Values) { $client.Dispose() }
    if ($connection.State -eq 'Open') {
        foreach ($code in $sampleCodes) {
            $null = Sql 'DELETE dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$code}
        }
        foreach ($suffix in @('_API','_XLSX','_XLSX_BAD','_MVC')) {
            $null = Sql 'DELETE dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$prefix + $suffix}
        }
        if ($assetId) {
            $null = Sql 'SET XACT_ABORT ON; BEGIN TRAN; DELETE dbo.MaintenanceRecords WHERE AssetId=@id; DELETE dbo.CalibrationLogs WHERE AssetId=@id; DELETE dbo.WorkOrders WHERE AssetId=@id; DELETE dbo.InventoryTransactions WHERE EquipmentId=@id; DELETE dbo.AssetTransfers WHERE AssetId=@id; DELETE dbo.EquipmentAssets WHERE AssetId=@id AND AssetCode=@code; COMMIT;' @{id=$assetId;code=$prefix}
        }
        foreach ($role in @('ADMIN','MANAGER','ENGINEER','STAFF','DISABLED','OTHERSTAFF')) {
            $null = Sql 'DELETE dbo.Users WHERE Username=@user' @{user=$prefix + '_' + $role}
        }
        $null = Sql 'DELETE dbo.Users WHERE Username=@user' @{user=$prefix+'_ACCOUNT'}
        $null = Sql 'DELETE dbo.Users WHERE Username=@user' @{user=$prefix+'_BROWSER'}
        $null = Sql 'DELETE dbo.EquipmentModels WHERE ModelCode=@code' @{code=$prefix+'M'}
        $null = Sql 'DELETE dbo.Suppliers WHERE Name=@name' @{name=$prefix+'_SUPPLIER'}
        $null = Sql 'DELETE dbo.EquipmentCategories WHERE CategoryCode=@code' @{code=$prefix+'C'}
        $null = Sql 'DELETE dbo.Departments WHERE DepartmentCode=@code' @{code=$prefix+'D'}
    }
    $connection.Dispose()
    Remove-Item Env:MEDICAL_QA_TIMINGS -ErrorAction SilentlyContinue
}
