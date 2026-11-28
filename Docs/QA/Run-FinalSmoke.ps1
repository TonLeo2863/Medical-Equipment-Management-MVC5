param([int]$Port = 51123)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http, System.Data
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$app = Join-Path $root 'MedicalEquipmentManagement'
Add-Type -Path (Join-Path $app 'bin/MedicalEquipmentManagement.dll')
$connection = New-Object Data.SqlClient.SqlConnection 'Data Source=localhost\SQLEXPRESS;Initial Catalog=MedicalEquipmentDB_BetaTest;Integrated Security=True;Connect Timeout=5'
$connection.Open()
$prefix = 'rcqa_' + [Guid]::NewGuid().ToString('N').Substring(0,12)
$process = $null
$assetId = $null
$clients = @{}

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
            $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        } finally { $response.Dispose() }
    } finally { $form.Dispose() }
}

try {
    if ((Sql 'SELECT DB_NAME()') -ne 'MedicalEquipmentDB_BetaTest') { throw 'Sai database đích.' }
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
    Assert-Equal 'API xóa mềm thiết bị' (Invoke-Request $admin DELETE "/api/equipments/$apiId" $null $null $apiToken).Status 204
    Assert-Equal 'API ẩn thiết bị sau xóa mềm' (Invoke-Request $admin GET "/api/equipments/$apiId").Status 404
    Assert-Equal 'Chi tiết tài sản' (Invoke-Request $admin GET "/Equipment/Details/$assetId").Status 200
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
        $transfer = Post-Form $manager '/Inventory/Transfer' @{AssetId=$assetId;FromDepartmentId=$source;ToDepartmentId=$destination;Reason='Kiểm thử RC'}
        Assert-Equal 'Điều chuyển hợp lệ' $transfer.Status 302
        Assert-Equal 'Vị trí sau điều chuyển' (Sql 'SELECT DepartmentId FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}) $destination
        Assert-Equal 'Giao dịch kho sau điều chuyển' (Sql 'SELECT COUNT(*) FROM dbo.InventoryTransactions WHERE EquipmentId=@id' @{id=$assetId}) 1
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
    Assert-Equal 'ENGINEER hoàn thành sửa' (Post-Form $engineer "/WorkOrder/Edit/$ticket" @{command='complete';resolution='Đã thay phụ tùng';technicalPassed='true'}).Status 302
    Assert-Equal 'Trạng thái phiếu hoàn thành' (Sql 'SELECT Status FROM dbo.WorkOrders WHERE TicketId=@id' @{id=$ticket}) 'Đã xong'
    Assert-Equal 'ENGINEER ghi hiệu chuẩn không đạt' (Post-Form $engineer '/Calibration/Create' @{AssetId=$assetId;CalibrationDate=$today;NextCalibrationDate=$next;PerformedBy='Kỹ sư QA';CertificateNumber=$prefix;Result='Không đạt'}).Status 302
    Assert-Equal 'Hiệu chuẩn không đạt đánh dấu hỏng' (Sql 'SELECT Status FROM dbo.EquipmentAssets WHERE AssetId=@id' @{id=$assetId}) 'Hỏng'
    Assert-Equal 'Danh sách hiệu chuẩn 15 ngày' (Invoke-Request $engineer GET '/Calibration?days=15').Status 200
    $dash = Invoke-Request $manager GET '/'
    Assert-Equal 'Dashboard có cảnh báo dữ liệu fixture' $dash.Body.Contains($prefix) $true
    $nonDeletedCount = Sql 'SELECT COUNT(*) FROM dbo.EquipmentAssets WHERE IsDeleted=0'
    Assert-Equal 'Dashboard tổng số thiết bị khớp SQL' $dash.Body.Contains("<span>Tổng thiết bị</span><strong>$nonDeletedCount</strong>") $true
    Assert-Equal 'Chart.js cục bộ phục vụ được' (Invoke-Request $manager GET '/Scripts/vendor/chart.umd.min.js').Status 200
    $null = Sql 'UPDATE dbo.Users SET IsActive=0 WHERE Username=@user' @{user=$prefix + '_OTHERSTAFF'}
    Assert-Equal 'API thu hồi phiên khi tài khoản bị vô hiệu hóa' (Invoke-Request $clients['OTHERSTAFF'] GET '/api/equipments').Status 401
    $logoutToken = Read-Token $dash
    Assert-Equal 'Logout GET bị từ chối' (Invoke-Request $manager GET '/Account/Logout').Status 405
    Assert-Equal 'GET Logout giữ phiên' (Invoke-Request $manager GET '/api/equipments').Status 200
    Assert-Equal 'Logout POST có token' (Invoke-Request $manager POST '/Account/Logout' @{__RequestVerificationToken=$logoutToken}).Status 302
    Assert-Equal 'API sau logout' (Invoke-Request $manager GET '/api/equipments').Status 401
} finally {
    if ($process -and !$process.HasExited) { $process.Kill(); $process.WaitForExit() }
    foreach ($client in $clients.Values) { $client.Dispose() }
    if ($connection.State -eq 'Open') {
        foreach ($suffix in @('_API','_XLSX','_XLSX_BAD')) {
            $null = Sql 'DELETE dbo.EquipmentAssets WHERE AssetCode=@code' @{code=$prefix + $suffix}
        }
        if ($assetId) {
            $null = Sql 'SET XACT_ABORT ON; BEGIN TRAN; DELETE dbo.MaintenanceRecords WHERE AssetId=@id; DELETE dbo.CalibrationLogs WHERE AssetId=@id; DELETE dbo.WorkOrders WHERE AssetId=@id; DELETE dbo.InventoryTransactions WHERE EquipmentId=@id; DELETE dbo.AssetTransfers WHERE AssetId=@id; DELETE dbo.EquipmentAssets WHERE AssetId=@id AND AssetCode=@code; COMMIT;' @{id=$assetId;code=$prefix}
        }
        foreach ($role in @('ADMIN','MANAGER','ENGINEER','STAFF','DISABLED','OTHERSTAFF')) {
            $null = Sql 'DELETE dbo.Users WHERE Username=@user' @{user=$prefix + '_' + $role}
        }
    }
    $connection.Dispose()
    Remove-Item Env:MEDICAL_QA_TIMINGS -ErrorAction SilentlyContinue
}
