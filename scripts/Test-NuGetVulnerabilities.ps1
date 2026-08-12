param(
    [string]$SolutionPath = "DatabaseMastery.HotCoffeePostgreSQL.sln"
)

$ErrorActionPreference = "Stop"

$webProject = Get-ChildItem -Path . -Filter "DatabaseMastery.HotCoffeePostgreSQL.csproj" -Recurse |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    Select-Object -First 1

$testProject = Get-ChildItem -Path . -Filter "DatabaseMastery.HotCoffeePostgreSQL.Tests.csproj" -Recurse |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    Select-Object -First 1

if (-not $webProject -or -not $testProject) {
    throw "Could not locate web and test projects for vulnerability audit."
}

$auditTargets = @(
    @{ Name = "web"; Path = $webProject.FullName },
    @{ Name = "tests"; Path = $testProject.FullName }
)

$totalVulnerabilities = 0

foreach ($target in $auditTargets) {
    $outputFile = Join-Path $env:TEMP ("hotcoffee-vulnerable-{0}.json" -f $target.Name)

    dotnet list $target.Path package --vulnerable --include-transitive --format json --output-version 1 |
        Out-File -FilePath $outputFile -Encoding utf8

    $json = Get-Content -Raw -Path $outputFile | ConvertFrom-Json

    $vulnerablePackages = @()
    foreach ($project in $json.projects) {
        foreach ($framework in $project.frameworks) {
            if ($null -ne $framework.topLevelPackages) {
                foreach ($package in $framework.topLevelPackages) {
                    if ($null -ne $package.vulnerabilities -and $package.vulnerabilities.Count -gt 0) {
                        $vulnerablePackages += $package
                    }
                }
            }

            if ($null -ne $framework.transitivePackages) {
                foreach ($package in $framework.transitivePackages) {
                    if ($null -ne $package.vulnerabilities -and $package.vulnerabilities.Count -gt 0) {
                        $vulnerablePackages += $package
                    }
                }
            }
        }
    }

    if ($vulnerablePackages.Count -gt 0) {
        Write-Host ("Vulnerabilities detected in {0} project:" -f $target.Name)
        foreach ($package in $vulnerablePackages) {
            Write-Host (" - {0} {1}" -f $package.id, $package.resolvedVersion)
        }

        $totalVulnerabilities += $vulnerablePackages.Count
    }
    else {
        Write-Host ("No vulnerable packages detected in {0} project." -f $target.Name)
    }
}

if ($totalVulnerabilities -gt 0) {
    throw "NuGet vulnerability gate failed."
}

Write-Host "NuGet vulnerability gate passed."
