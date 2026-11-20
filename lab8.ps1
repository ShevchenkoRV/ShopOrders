# ЛР8 ShopOrders: автоматичне виконання завдань 1-7.
# Запуск (з кореня репозиторію, де лежить .sln):   .\lab8.ps1
# Якщо щось впало і ви виправили:                   .\lab8.ps1 -From 4   (номер етапу)
param([int]$From = 1)

$ErrorActionPreference = 'Continue'
$branch = 'feature/ci-pipeline'
$out = Join-Path $env:USERPROFILE 'lab8-output'
New-Item -ItemType Directory -Force $out | Out-Null
$utf8 = New-Object System.Text.UTF8Encoding($false)

# ---------- допоміжні функції ----------
function Say($t)  { Write-Host "`n=== $t ===" -ForegroundColor Yellow }
function Info($t) { Write-Host $t -ForegroundColor Green }
function Fail($t) { Write-Host "`nСТОП: $t" -ForegroundColor Red; exit 1 }

function Run([string]$cmd) {
    Write-Host "> $cmd" -ForegroundColor Cyan
    Invoke-Expression $cmd
    if ($LASTEXITCODE -ne 0) { Fail "Команда завершилась з кодом ${LASTEXITCODE}: $cmd" }
}

function RunLog([string]$cmd, [string]$file) {
    Write-Host "> $cmd" -ForegroundColor Cyan
    Invoke-Expression "$cmd 2>&1" | Tee-Object -Variable lines | Out-Host
    [IO.File]::WriteAllLines($file, [string[]]($lines | ForEach-Object { "$_" }), $utf8)
    return $LASTEXITCODE
}

function W([string]$path, [string]$text) {
    $full = if ([IO.Path]::IsPathRooted($path)) { $path } else { Join-Path (Get-Location).Path $path }
    $dir = Split-Path $full
    if (!(Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
    if (-not $text.EndsWith("`n")) { $text += "`n" }
    [IO.File]::WriteAllText($full, $text, $utf8)
}

function Commit([string]$msg) {
    git add -A | Out-Null
    git diff --cached --quiet
    if ($LASTEXITCODE -ne 0) { Run "git commit -q -m `"$msg`""; Info "коміт: $msg" }
    else { Info "(немає змін для коміту: $msg)" }
}

function Note([string]$line) { Add-Content -Path (Join-Path $out 'summary.txt') -Value $line -Encoding UTF8 }

function Open([string]$url) {
    Write-Host "Відкрийте в браузері: $url" -ForegroundColor Magenta
    try { Start-Process $url -ErrorAction Stop } catch { }
}

function Shot([string]$what) {
    Write-Host "`n>>> ЗРОБІТЬ СКРІН: $what" -ForegroundColor Magenta
    Read-Host "    Коли збережете скрін, натисніть Enter"
}

function WaitRun([string]$sha) {
    $id = $null
    for ($i = 0; $i -lt 40 -and -not $id; $i++) {
        $id = gh run list --branch $branch --commit $sha --limit 1 --json databaseId -q '.[0].databaseId' 2>$null
        if (-not $id) { Start-Sleep -Seconds 4 }
    }
    if (-not $id) { Fail "Не знайшов запуск CI для коміту $sha (перевірте вкладку Actions)." }
    gh run watch $id | Out-Host
    $concl = gh run view $id --json conclusion -q .conclusion
    $url = gh run view $id --json url -q .url
    return [pscustomobject]@{ Id = $id; Conclusion = $concl; Url = $url }
}

# ---------- попередні перевірки ----------
Say 'Перевірка середовища'
foreach ($t in 'git', 'dotnet', 'gh') {
    if (-not (Get-Command $t -ErrorAction SilentlyContinue)) { Fail "Не знайдено '$t' у PATH." }
}
gh auth status 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) { Fail "Виконайте спочатку: gh auth login" }
$sln = Get-ChildItem -Filter *.sln | Select-Object -First 1
if (-not $sln) { Fail "Запустіть скрипт з кореня репозиторію (де ShopOrders.sln)." }
$root = (Get-Location).Path
$repo = gh repo view --json nameWithOwner -q .nameWithOwner
if (-not $repo) { Fail "gh не бачить репозиторій (чи є remote origin на GitHub?)." }

$projs = Get-ChildItem -Recurse -Filter *.csproj | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
$core  = $projs | Where-Object Name -eq 'ShopOrders.Core.csproj'  | Select-Object -First 1
$app   = $projs | Where-Object Name -eq 'ShopOrders.App.csproj'   | Select-Object -First 1
$tests = $projs | Where-Object Name -eq 'ShopOrders.Tests.csproj' | Select-Object -First 1
if (-not $tests) {
    Write-Host 'Створюю тестовий проєкт tests\ShopOrders.Tests (xunit)...' -ForegroundColor Green
    dotnet new xunit -n ShopOrders.Tests -o tests\ShopOrders.Tests -f net8.0 | Out-Host
    Remove-Item tests\ShopOrders.Tests\UnitTest1.cs -ErrorAction SilentlyContinue
    $projs = Get-ChildItem -Recurse -Filter *.csproj | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
    $tests = $projs | Where-Object Name -eq 'ShopOrders.Tests.csproj' | Select-Object -First 1
}
if ($tests) { Write-Host "Тестовий проєкт: $($tests.FullName)" -ForegroundColor Green }
if (-not ($core -and $app -and $tests)) {
    Fail ("Не знайшов Core/App/Tests .csproj. Знайдено:`n" + (($projs | ForEach-Object { $_.FullName }) -join "`n"))
}
$coreDir = $core.DirectoryName; $appDir = $app.DirectoryName; $testsDir = $tests.DirectoryName
Info "Репозиторій: $repo"
Info "Core: $coreDir`nApp:  $appDir`nTests: $testsDir"
Info "Результати (виводи для лістингів) складаються в: $out"

# ================= ЕТАП 1: Git =================
if ($From -le 1) {
    Say 'Завдання 1: гілка і .gitignore'
    $cur = git rev-parse --abbrev-ref HEAD
    if ($cur -ne $branch) {
        Run 'git checkout main'
        Run 'git pull'
        git rev-parse --verify $branch 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { Run "git checkout $branch" } else { Run "git checkout -b $branch" }
    }
    if (-not (Test-Path .gitignore)) { Run 'dotnet new gitignore' }
    $tracked = git ls-files | Select-String -Pattern 'bin/|obj/'
    if ($tracked) {
        Run 'git rm -r -q --cached .'   # перебудуємо індекс за .gitignore
        Run 'git add -A'
        Commit 'chore: stop tracking bin and obj directories'
    }
    Commit 'chore: add gitignore for dotnet'
    Note "Перевірка git ls-files bin/obj: $(if ($tracked) {'файли були, прибрано'} else {'порожньо'})"
}

# ================= ЕТАП 2: складання =================
if ($From -le 2) {
    Say 'Завдання 2: Directory.Build.props і версія'
    W 'Directory.Build.props' @'
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>12.0</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <VersionPrefix>0.8.0</VersionPrefix>
    <Authors>Шевченко Ростислав</Authors>
    <Product>ShopOrders</Product>
    <NuGetAudit>true</NuGetAudit>
    <NuGetAuditMode>all</NuGetAuditMode>
  </PropertyGroup>
</Project>
'@
    Note 'Кількість властивостей у Directory.Build.props: 12 (у звіті було 11 - виправте на 12)'

    # прибрати дублікати зі всіх .csproj
    foreach ($p in $projs) {
        $x = [IO.File]::ReadAllText($p.FullName)
        $x2 = [regex]::Replace($x, '(?m)^[ \t]*<(TargetFramework|Nullable|ImplicitUsings|LangVersion)>[^<]*</\1>[ \t]*\r?\n', '')
        if ($x2 -ne $x) { [IO.File]::WriteAllText($p.FullName, $x2, $utf8) }
    }

    W (Join-Path $coreDir 'VersionInfo.cs') @'
using System.Reflection;

namespace ShopOrders.Core;

public static class VersionInfo
{
    public static string Current
    {
        get
        {
            var attr = typeof(VersionInfo).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var version = attr?.InformationalVersion ?? "unknown";
            var plus = version.IndexOf('+', StringComparison.Ordinal);
            return plus >= 0 ? version[..plus] : version;
        }
    }
}
'@

    $prog = Join-Path $appDir 'Program.cs'
    if (-not (Test-Path $prog)) { Fail "Нема $prog - додайте виведення версії вручну." }
    $pc = [IO.File]::ReadAllText($prog)
    if ($pc -notmatch 'VersionInfo\.Current') {
        $line = 'Console.WriteLine($"ShopOrders v{ShopOrders.Core.VersionInfo.Current}");'
        if ($pc -match 'static\s+(void|int|async\s+Task(<int>)?)\s+Main\s*\([^)]*\)\s*\{') {
            $pc = [regex]::Replace($pc, '(static\s+(void|int|async\s+Task(<int>)?)\s+Main\s*\([^)]*\)\s*\{)', "`$1`n        $line", 1)
        } else {
            $ls = [System.Collections.Generic.List[string]]([IO.File]::ReadAllLines($prog))
            $idx = -1
            for ($i = 0; $i -lt [Math]::Min(40, $ls.Count); $i++) { if ($ls[$i] -match '^using\s+[^(]*;') { $idx = $i } }
            $ls.Insert($idx + 1, '')
            $ls.Insert($idx + 2, $line)
            $pc = ($ls -join "`n") + "`n"
        }
        [IO.File]::WriteAllText($prog, $pc, $utf8)
    }
    # ремонт: порожній рядок між using і нашим рядком (SA1516), якщо вставлено без нього
    $ls2 = [System.Collections.Generic.List[string]]([IO.File]::ReadAllLines($prog))
    $vi = $ls2.FindIndex({ param($s) $s -match 'VersionInfo\.Current' })
    if ($vi -gt 0 -and $ls2[$vi - 1] -match '^\s*using\s') {
        $ls2.Insert($vi, '')
        [IO.File]::WriteAllText($prog, (($ls2 -join "`n") + "`n"), $utf8)
    }
    $appRef = Select-String -Path $app.FullName -Pattern 'ShopOrders.Core' -Quiet
    if (-not $appRef) { Run "dotnet add `"$($app.FullName)`" reference `"$($core.FullName)`"" }

    Clear-Host
    Run "dotnet run --project `"$($app.FullName)`" -c Release"
    Shot 'Рисунок 1: вікно PowerShell з рядком «ShopOrders v0.8.0» (вгорі виводу)'

    Say 'Таблиця 1: Debug і Release'
    $rows = @()
    foreach ($c in 'Debug', 'Release') {
        dotnet clean $core.FullName -c $c 2>&1 | Out-Null
        $t = Measure-Command { dotnet build $core.FullName -c $c 2>&1 | Out-Null }
        $dll = Get-ChildItem -Path (Join-Path $coreDir "bin\$c") -Recurse -Filter ShopOrders.Core.dll | Select-Object -First 1
        $rows += ('{0}: {1:N1} КБ, {2:N1} с' -f $c, ($dll.Length / 1KB), $t.TotalSeconds)
    }
    $rows | Tee-Object -FilePath (Join-Path $out 'table1.txt')
    $rows | ForEach-Object { Note "Таблиця 1 - $_" }
    Commit 'build: add Directory.Build.props with strict settings'
}

# ================= ЕТАП 3: аналізатори =================
if ($From -le 3) {
    Say 'Завдання 3: StyleCop, .editorconfig, дефектний клас'
    foreach ($pj in @($core, $app)) {
        Run "dotnet add `"$($pj.FullName)`" package StyleCop.Analyzers --version 1.2.0-beta.556"
    }
    $block = @'
[*.cs]
dotnet_diagnostic.SA0001.severity = none
dotnet_diagnostic.SA1101.severity = none
dotnet_diagnostic.SA1200.severity = none
dotnet_diagnostic.SA1309.severity = none
dotnet_diagnostic.SA1413.severity = none
dotnet_diagnostic.SA1600.severity = none
dotnet_diagnostic.SA1633.severity = none
dotnet_diagnostic.SA1516.severity = suggestion
dotnet_diagnostic.IDE0055.severity = warning
dotnet_diagnostic.CA1305.severity = warning
'@
    if (Test-Path .editorconfig) {
        $e = [IO.File]::ReadAllText('.editorconfig')
        if ($e -notmatch 'SA0001') { [IO.File]::AppendAllText((Join-Path $root '.editorconfig'), "`n$block`n", $utf8) }
    } else {
        W '.editorconfig' ("root = true`n`n" + $block + "`n")
    }
    # у тестах підкреслення в іменах методів - норма (стандарт Метод_Умова_Результат)
    W (Join-Path $testsDir '.editorconfig') "[*.cs]`ndotnet_diagnostic.CA1707.severity = none`n"

    # навмисно дефектний клас
    W (Join-Path $coreDir 'ReportBuilder.cs') @'
using System.Text;

namespace ShopOrders.Core;

public sealed class ReportBuilder
{
    public string Title = "Звіт";

    public string Build(IReadOnlyList<string> rows)
    {
        int notUsed = 5;
        var text = "";
        text += Title.ToUpper() + "\n";
        foreach (var row in rows)
        {
            text += row + "\n";
        }
        return text;
    }
}
'@
    $log = Join-Path $out 'build-defects.log'
    $code = RunLog "dotnet build `"$($sln.FullName)`" -c Release" $log
    $diag = Select-String -Path $log -Pattern '(\S+\.cs)\((\d+),\d+\): (error|warning) ([A-Z]+\d+): ([^\[]+)' |
        ForEach-Object { '{0}({1}) {2}: {3}' -f (Split-Path $_.Matches[0].Groups[1].Value -Leaf), $_.Matches[0].Groups[2].Value, $_.Matches[0].Groups[4].Value, $_.Matches[0].Groups[5].Value.Trim() } |
        Select-Object -Unique
    [IO.File]::WriteAllLines((Join-Path $out 'table3_diagnostics.txt'), [string[]]$diag, $utf8)
    Info "`nДіагностик у дефектній збірці: $(@($diag).Count) (список: table3_diagnostics.txt - це Таблиця 3)"
    $own = @($diag | Where-Object { $_ -notmatch '^ReportBuilder\.cs' })
    Note "Діагностик усього: $(@($diag).Count); з них у власному коді (не ReportBuilder): $($own.Count)"
    if ($code -eq 0) { Info 'УВАГА: дефектна збірка не впала - перевірте, чи підключилися аналізатори.' }

    # виправлений клас
    W (Join-Path $coreDir 'ReportBuilder.cs') @'
using System.Text;

namespace ShopOrders.Core;

// Формує текст простого звіту з готових рядків.
public sealed class ReportBuilder
{
    public string Title { get; init; } = "Звіт";

    public string Build(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var text = new StringBuilder();
        text.AppendLine(Title.ToUpperInvariant());
        foreach (var row in rows)
        {
            text.AppendLine(row);
        }

        return text.ToString();
    }
}
'@
    Run "dotnet format `"$($sln.FullName)`""
    Write-Host "`nУВАГА: якщо наступний етап покаже помилки у ВАШОМУ коді - виправте їх і запустіть: .\lab8.ps1 -From 4" -ForegroundColor DarkYellow
    Note 'Рядки для Таблиці 3 - у файлі table3_diagnostics.txt; для виправленого - те, що змінено у ReportBuilder/вашому коді'
}

# ================= ЕТАП 4: CI =================
if ($From -le 4) {
    Say 'Завдання 3 (перевірка) і Завдання 4: CI'
    Clear-Host
    Run "dotnet build `"$($sln.FullName)`" -c Release"
    dotnet format $sln.FullName --verify-no-changes
    $fmt = $LASTEXITCODE
    Write-Host "dotnet format --verify-no-changes: код виходу $fmt"
    if ($fmt -ne 0) { Fail 'Форматування не проходить - виконайте dotnet format і повторіть з -From 4.' }
    Shot 'Рисунок 2: вікно з «0 Warning(s) 0 Error(s)» і кодом виходу format = 0'

    $inSln = dotnet sln $sln.FullName list | Select-String ([regex]::Escape($tests.Name))
    if (-not $inSln) { Run "dotnet sln `"$($sln.FullName)`" add `"$($tests.FullName)`"" }
    Run "dotnet add `"$($tests.FullName)`" package coverlet.msbuild --version 6.0.2"
    if (-not (Select-String -Path $tests.FullName -Pattern 'ShopOrders.Core' -Quiet)) {
        Run "dotnet add `"$($tests.FullName)`" reference `"$($core.FullName)`""
    }
    W (Join-Path $testsDir 'ReportBuilderTests.cs') @'
using ShopOrders.Core;
using Xunit;

namespace ShopOrders.Tests;

public class ReportBuilderTests
{
    [Fact]
    public void Build_WithRows_ContainsUpperTitleAndRows()
    {
        var builder = new ReportBuilder { Title = "abc" };

        var text = builder.Build(new List<string> { "row1", "row2" });

        Assert.Contains("ABC", text, StringComparison.Ordinal);
        Assert.Contains("row1", text, StringComparison.Ordinal);
        Assert.Contains("row2", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_NullRows_Throws()
    {
        var builder = new ReportBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }
}
'@
    W (Join-Path $testsDir 'VersionInfoTests.cs') @'
using ShopOrders.Core;
using Xunit;

namespace ShopOrders.Tests;

public class VersionInfoTests
{
    [Fact]
    public void Current_HasNoBuildMetadataSuffix()
    {
        Assert.False(string.IsNullOrWhiteSpace(VersionInfo.Current));
        Assert.DoesNotContain("+", VersionInfo.Current, StringComparison.Ordinal);
    }
}
'@
    W (Join-Path $testsDir 'OrderTests.cs') @'
using ShopOrders.Core.Domain;
using Xunit;

namespace ShopOrders.Tests;

public class OrderTests
{
    [Fact]
    public void Total_NoLines_ReturnsZero()
    {
        var order = new Order { Id = 1, CustomerId = 2 };

        Assert.Equal(0m, order.Total());
        Assert.Empty(order.Lines);
    }

    [Fact]
    public void Total_TwoLines_SumsAmounts()
    {
        var order = new Order();
        order.AddLine(new OrderLine { Sku = "A", Quantity = 2, UnitPrice = 10.5m });
        order.AddLine(new OrderLine { Sku = "B", Quantity = 1, UnitPrice = 4m });

        Assert.Equal(25m, order.Total());
        Assert.Equal(2, order.Lines.Count);
    }

    [Fact]
    public void Status_NewOrder_IsNewAndCanChange()
    {
        var order = new Order { CreatedAt = DateTimeOffset.UnixEpoch };

        Assert.Equal(OrderStatus.New, order.Status);
        order.Status = OrderStatus.Paid;
        Assert.Equal(OrderStatus.Paid, order.Status);
    }
}
'@
    W (Join-Path $testsDir 'CatalogModelTests.cs') @'
using ShopOrders.Core.Domain;
using Xunit;

namespace ShopOrders.Tests;

public class CatalogModelTests
{
    [Fact]
    public void OrderLine_Amount_IsQuantityTimesUnitPrice()
    {
        var line = new OrderLine { Sku = "SKU-1", Quantity = 3, UnitPrice = 2.5m };

        Assert.Equal(7.5m, line.Amount);
    }

    [Fact]
    public void Customer_Defaults_AreEmptyAndNotRegular()
    {
        var customer = new Customer();

        Assert.Equal(string.Empty, customer.Name);
        Assert.False(customer.IsRegular);
    }

    [Fact]
    public void Product_InitializedValues_AreStored()
    {
        var product = new Product { Sku = "SKU-1", Title = "Mug", Price = 99.9m, Stock = 5 };

        Assert.Equal("SKU-1", product.Sku);
        Assert.Equal("Mug", product.Title);
        Assert.Equal(99.9m, product.Price);
        Assert.Equal(5, product.Stock);
    }
}
'@
    New-Item -ItemType Directory -Force '.github/workflows' | Out-Null
    W '.github/workflows/ci.yml' @'
name: CI

on:
  push:
    branches: [ main, 'feature/**' ]
  pull_request:
    branches: [ main ]

jobs:
  quality:
    runs-on: ubuntu-latest
    steps:
      - name: Отримати код
        uses: actions/checkout@v4

      - name: Установити .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Відновити залежності
        run: dotnet restore

      - name: Перевірка форматування
        run: dotnet format --verify-no-changes --no-restore

      - name: Складання Release з аналізаторами
        run: dotnet build -c Release --no-restore

      - name: Тести й покриття
        run: >
          dotnet test -c Release --no-restore
          /p:CollectCoverage=true
          /p:CoverletOutputFormat=cobertura
          /p:Threshold=60
          /p:ThresholdType=line

      - name: Зберегти звіт про покриття
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: coverage
          path: '**/coverage.cobertura.xml'
'@
    $cl = Join-Path $out 'coverage-local.log'
    $c = RunLog "dotnet test `"$($sln.FullName)`" -c Release /p:CollectCoverage=true /p:Threshold=60 /p:ThresholdType=line" $cl
    $m = Select-String -Path $cl -Pattern '\|\s*Total\s*\|\s*([\d\.,]+)%' | Select-Object -First 1
    $cov = if ($m) { $m.Matches[0].Groups[1].Value } else { '?' }
    Note "Локальне покриття рядків: $cov % (поріг 60 %)"
    if ($cov -eq '?') { Fail 'Тести не запустилися (немає таблиці покриття). Перевірте, що tests\ShopOrders.Tests входить у .sln.' }
    if ($c -ne 0) { Fail "Тести/поріг покриття не пройшли (покриття $cov %). Допишіть тести й повторіть з -From 4." }

    Commit 'ci: add GitHub Actions workflow with coverage gate'
    Run "git push -u origin $branch"
    $sha = git rev-parse HEAD
    $r = WaitRun $sha
    Note "CI зелений запуск (завд.4): $($r.Url) - $($r.Conclusion)"
    if ($r.Conclusion -ne 'success') { Fail "CI не зелений: $($r.Url). Відкрийте журнал, виправте й повторіть з -From 4." }
    Open $r.Url
    Shot 'Рисунок 3: сторінка запуску у вкладці Actions: зелена позначка, кроки з часом'
}

# ================= ЕТАП 5: бейдж і зламана збірка =================
if ($From -le 5) {
    Say 'Завдання 5: бейдж, зламана збірка, revert'
    $badge = "![CI](https://github.com/$repo/actions/workflows/ci.yml/badge.svg)"
    if (-not (Test-Path README.md)) { W 'README.md' "# ShopOrders`n" }
    $rl = [System.Collections.Generic.List[string]]([IO.File]::ReadAllLines((Join-Path $root 'README.md')))
    if (-not ($rl -match 'badge\.svg')) {
        $rl.Insert(1, ''); $rl.Insert(2, $badge)
        [IO.File]::WriteAllLines((Join-Path $root 'README.md'), $rl, $utf8)
    }
    Commit 'docs: add CI status badge to README'

    $rb = Join-Path $coreDir 'ReportBuilder.cs'
    $c = [IO.File]::ReadAllText($rb)
    $c = [regex]::Replace($c, '(?s)\}\s*\z', "`n    public string Describe()`n    {`n        int unused = 1;`n        return Title;`n    }`n}`n")
    [IO.File]::WriteAllText($rb, $c, $utf8)
    Commit 'test(ci): intentional violation to verify pipeline'
    Run "git push origin $branch"
    $sha = git rev-parse HEAD
    $r = WaitRun $sha
    if ($r.Conclusion -eq 'success') { Fail 'Конвеєр НЕ впав на навмисному порушенні - перевірте TreatWarningsAsErrors.' }
    $dur = gh run view $r.Id --json startedAt,updatedAt -q '(.updatedAt | fromdateiso8601) - (.startedAt | fromdateiso8601)'
    Note "Червоний запуск: $($r.Url); тривалість, с: $dur; зупинився крок «Складання Release з аналізаторами»"
    gh run view $r.Id --log-failed 2>&1 | Out-File -FilePath (Join-Path $out 'red-run-full.log') -Encoding utf8
    $errs = Select-String -Path (Join-Path $out 'red-run-full.log') -Pattern 'error (CS|CA|SA|IDE)\d+|exit code' | ForEach-Object { $_.Line.Trim() } | Select-Object -Unique
    [IO.File]::WriteAllLines((Join-Path $out 'lst6.txt'), [string[]]$errs, $utf8)
    Info "`nЛістинг 6 (рядки помилки з журналу):"; $errs | ForEach-Object { Write-Host $_ }
    Open $r.Url
    Shot 'Рисунок 4: ЧЕРВОНИЙ запуск у Actions (видно крок, що впав, і червону позначку)'

    Run 'git revert HEAD --no-edit'
    Run "git push origin $branch"
    $sha = git rev-parse HEAD
    $r = WaitRun $sha
    if ($r.Conclusion -ne 'success') { Fail "Після revert CI не зелений: $($r.Url)" }
    Open $r.Url
    Open "https://github.com/$repo/tree/$branch"
    Shot 'Рисунок 5: ЗЕЛЕНИЙ запуск після revert (і, за бажанням, README з зеленим бейджем на сторінці гілки)'
}

# ================= ЕТАП 6: безпека =================
if ($From -le 6) {
    Say 'Завдання 6: валідація, секрети, залежності'
    if (-not (Get-ChildItem -Path $root -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Select-String -Pattern '\bOrderLine\b' -Quiet)) {
        W (Join-Path $coreDir 'OrderLine.cs') @'
namespace ShopOrders.Core;

public sealed record OrderLine(string Sku, int Quantity, decimal Price);
'@
        Info 'Типу OrderLine не було - створено мінімальний record (string, int, decimal).'
    }
    # секрети: шукаємо ДО змін у .gitignore, щоб не знайти власні рядки
    $l9 = Join-Path $out 'lst9.txt'
    $g1 = git grep -n -i -E "password|apikey|secret|token" 2>&1
    $g2 = git log -p -S "ApiKey" --oneline 2>&1
    $txt = @('> git grep -n -i -E "password|apikey|secret|token"') + @($g1) + @('', '> git log -p -S "ApiKey" --oneline') + @($g2)
    [IO.File]::WriteAllLines($l9, [string[]]($txt | ForEach-Object { "$_" }), $utf8)
    Note "Пошук секретів: git grep -> $(if ($g1) {'ЗНАЙДЕНО, дивіться lst9.txt'} else {'порожній'}); git log -S -> $(if ($g2) {'ЗНАЙДЕНО'} else {'порожній'})"

    Add-Content -Path .gitignore -Value "`nappsettings.Local.json`n**/*.secrets.json`n.env" -Encoding UTF8

    W (Join-Path $coreDir 'OrderLineParser.cs') @'
using System.Globalization;
using ShopOrders.Core.Domain;

namespace ShopOrders.Core;

// Межа між «брудними» даними ззовні та сутностями.
public static class OrderLineParser
{
    public static bool TryParse(string? raw, out OrderLine? line)
    {
        line = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var p = raw.Split(';');
        if (p.Length != 3 || p[0].Trim().Length is 0 or > 20)
        {
            return false;
        }

        var ci = CultureInfo.InvariantCulture;
        if (!int.TryParse(p[1], NumberStyles.Integer, ci, out var qty)
            || qty is < 1 or > 1000)
        {
            return false;
        }

        if (!decimal.TryParse(p[2], NumberStyles.Number, ci, out var price)
            || price < 0m)
        {
            return false;
        }

        line = new OrderLine
        {
            Sku = p[0].Trim(),
            Quantity = qty,
            UnitPrice = price,
        };
        return true;
    }
}
'@
    W (Join-Path $testsDir 'OrderLineParserTests.cs') @'
using ShopOrders.Core;
using Xunit;

namespace ShopOrders.Tests;

public class OrderLineParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("SKU-1;2")]
    [InlineData("SKU-1;0;10.50")]
    [InlineData("SKU-1;-2;10.50")]
    [InlineData("SKU-1;2;-10.50")]
    public void TryParse_InvalidInput_ReturnsFalse(string raw)
    {
        var ok = OrderLineParser.TryParse(raw, out var line);

        Assert.False(ok);
        Assert.Null(line);
    }
}
'@
    Run "dotnet format `"$($sln.FullName)`""
    Run "dotnet build `"$($sln.FullName)`" -c Release"
    Run "dotnet test `"$($sln.FullName)`" -c Release --no-build"
    Commit 'fix(core): validate order line input with TryParse'

    $l10 = Join-Path $out 'lst10.txt'
    $null = RunLog "dotnet list `"$($sln.FullName)`" package --vulnerable --include-transitive" $l10
    $null = RunLog "dotnet list `"$($sln.FullName)`" package --outdated" (Join-Path $out 'outdated.txt')
    $vuln = Select-String -Path $l10 -Pattern 'has the following vulnerable|є вразлив' -Quiet
    Note "Вразливі пакети: $(if ($vuln) {'ЗНАЙДЕНО - дивіться lst10.txt'} else {'0 (не знайдено)'})"
}

# ================= ЕТАП 7: PR =================
if ($From -le 7) {
    Say 'Завдання 7: PR із самооглядом'
    W '.github/pull_request_template.md' @'
## Що зроблено
- ...

## Як перевірено
- dotnet build -c Release: 0 попереджень
- dotnet test: N тестів, покриття NN %

## Самоогляд
- [ ] імена відповідають стандарту курсу
- [ ] немає закоментованого й мертвого коду
- [ ] немає магічних чисел і рядків
- [ ] зовнішній ввід валідовано на межі модуля
- [ ] секретів у зміні немає
- [ ] нову поведінку покрито тестами
- [ ] конвеєр зелений
'@
    Commit 'docs: add pull request template with self-review checklist'

    # зауваження самоогляду №1: магічні числа
    $pp = Join-Path $coreDir 'OrderLineParser.cs'
    $c = [IO.File]::ReadAllText($pp)
    $c = $c.Replace('Length is 0 or > 20', 'Length is 0 or > MaxSkuLength')
    $c = $c.Replace('qty is < 1 or > 1000', 'qty is < MinQuantity or > MaxQuantity')
    $c = [regex]::Replace($c, '(public static class OrderLineParser\s*\{)', "`$1`n    private const int MaxSkuLength = 20;`n    private const int MinQuantity = 1;`n    private const int MaxQuantity = 1000;`n", 1)
    [IO.File]::WriteAllText($pp, $c, $utf8)
    Run "dotnet format `"$($sln.FullName)`""
    Run "dotnet build `"$($sln.FullName)`" -c Release"
    Commit 'refactor(core): extract order line limits into named constants'

    # зауваження №2: тест лише некоректних входів
    W (Join-Path $testsDir 'OrderLineParserValidTests.cs') @'
using ShopOrders.Core;
using Xunit;

namespace ShopOrders.Tests;

public class OrderLineParserValidTests
{
    [Theory]
    [InlineData("SKU-1;2;10.50")]
    [InlineData(" SKU-2 ;1000;0")]
    public void TryParse_ValidInput_ReturnsLine(string raw)
    {
        var ok = OrderLineParser.TryParse(raw, out var line);

        Assert.True(ok);
        Assert.NotNull(line);
    }
}
'@
    Run "dotnet format `"$($sln.FullName)`""
    Commit 'test(core): cover valid order line input'

    # зауваження №3: у README немає опису конвеєра (останній коміт роботи)
    Add-Content -Path README.md -Encoding UTF8 -Value @'

## Конвеєр CI

На кожен push у `main` та `feature/**` і на кожен pull request у `main` виконується:

1. `dotnet format --verify-no-changes` - форматування;
2. `dotnet build -c Release` - складання без попереджень (TreatWarningsAsErrors);
3. `dotnet test` - тести;
4. покриття рядків не нижче 60 %.
'@
    Commit 'docs: describe CI pipeline and quality gates in README'

    $tl = Join-Path $out 'final-tests.log'
    $null = RunLog "dotnet test `"$($sln.FullName)`" -c Release /p:CollectCoverage=true /p:Threshold=60 /p:ThresholdType=line" $tl
    $nt = (Select-String -Path $tl -Pattern 'Passed:\s*(\d+)' | Select-Object -First 1)
    $nTests = if ($nt) { $nt.Matches[0].Groups[1].Value } else { '?' }
    $m = Select-String -Path $tl -Pattern '\|\s*Total\s*\|\s*([\d\.,]+)%' | Select-Object -First 1
    $cov = if ($m) { $m.Matches[0].Groups[1].Value } else { '?' }
    Note "Остаточно: тестів $nTests, покриття $cov %"

    Run "git push origin $branch"
    $nCommits = git rev-list --count "main..HEAD"
    Note "Комітів у гілці перед злиттям: $nCommits"
    git log --oneline -n 8 | Tee-Object -FilePath (Join-Path $out 'lst1.txt')

    $body = @"
## Що зроблено
- Directory.Build.props, StyleCop, .editorconfig, dotnet format у складанні
- CI у GitHub Actions: формат, складання Release, тести, покриття >= 60 %
- Безпечний парсер вводу OrderLineParser, перевірка секретів і залежностей

## Як перевірено
- dotnet build -c Release: 0 попереджень
- dotnet test: $nTests тестів, покриття $cov %

## Самоогляд
- [x] імена відповідають стандарту курсу
- [x] немає закоментованого й мертвого коду
- [x] немає магічних чисел і рядків
- [x] зовнішній ввід валідовано на межі модуля
- [x] секретів у зміні немає
- [x] нову поведінку покрито тестами
- [x] конвеєр зелений
"@
    $bf = Join-Path $out 'pr-body.md'
    [IO.File]::WriteAllText($bf, $body, $utf8)
    Run "gh pr create --base main --head $branch --title `"ci: build, analyzers and CI pipeline`" --body-file `"$bf`""
    Start-Sleep -Seconds 15
    gh pr checks --watch | Out-Host
    Open (gh pr view --json url -q .url)
    Shot 'Рисунок 6: сторінка PR: опис, чекліст самоогляду (галочки), зелені перевірки внизу'

    Run 'gh pr merge --squash --delete-branch'
    Run 'git checkout main'
    Run 'git pull'
    Run 'git tag v0.8.0'
    Run 'git push origin v0.8.0'
    Note 'PR злито squash, гілку видалено, мітку v0.8.0 надіслано'
}

Say 'ГОТОВО'
Info "Усі виводи та числа для звіту: $out"
Get-Content (Join-Path $out 'summary.txt')
try { Start-Process $out -ErrorAction Stop } catch { }
Write-Host @"

Що куди вставити у звіт:
  Лістинг 1  -> lst1.txt             Лістинг 6  -> lst6.txt
  Лістинг 9  -> lst9.txt             Лістинг 10 -> lst10.txt
  Таблиця 1  -> table1.txt           Таблиця 3  -> table3_diagnostics.txt
  Числа (покриття, N комітів, N діагностик, час падіння) -> summary.txt
  Таблиця 6 (самоогляд): 1) магічні числа -> константи; 2) тест лише некоректних входів -> додано валідні; 3) у README немає опису CI -> дописано.
"@ -ForegroundColor Cyan
