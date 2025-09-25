# Cross-platform PowerShell (Windows + macOS/Linux with PowerShell 7+)
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Folders = @('Audio','Enemy','Inventory','Player','Weapon','Character','SkillTree','UI')

foreach ($name in $Folders) {
    $dir = Get-ChildItem -LiteralPath $Root -Directory | Where-Object { $_.Name -ieq $name } | Select-Object -First 1
    if ($dir) {
        $outFile = Join-Path $Root ("All{0}Scripts.txt" -f $dir.Name)
        Set-Content -Path $outFile -Value '' -NoNewline

        Get-ChildItem -LiteralPath $dir.FullName -Filter *.cs | ForEach-Object {
            Add-Content $outFile ("// ----- File: {0} -----" -f $_.Name)
            Add-Content $outFile (Get-Content -LiteralPath $_.FullName)
            Add-Content $outFile ''
        }
        Write-Host "Wrote $outFile"
    } else {
        Write-Host ("Skip {0} (not found under {1})" -f $name, $Root)
    }
}

Write-Host "All scripts processed."
