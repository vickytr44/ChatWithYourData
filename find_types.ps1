$files = Get-ChildItem -Path "$env:USERPROFILE\.nuget\packages\hotchocolate*" -Recurse -Filter "*.dll"
foreach ($f in $files) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFile($f.FullName)
        foreach ($t in $asm.GetTypes()) {
            if ($t.Name -eq "QueryContext" -or $t.Name -eq "PagingArguments" -or $t.Name -eq "PageConnection" -or $t.Name -like "*QueryContext*") {
                Write-Host "$($t.FullName) ---> $($f.Name)"
            }
        }
    } catch {}
}
