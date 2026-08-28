
$dlls = Get-ChildItem -Path "$env:USERPROFILE\.nuget\packages\hotchocolate*\16.6.1\lib\net8.0\*.dll", "$env:USERPROFILE\.nuget\packages\hotchocolate*\16.6.1\lib\net10.0\*.dll"
foreach ($d in $dlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($d.FullName)
        foreach ($t in $asm.GetExportedTypes()) {
            if ($t.Name -like "*QueryContext*" -or $t.Name -like "*PagingArguments*" -or $t.Name -like "*Connection*") {
                Write-Host "$($t.FullName) ---> $($d.Name)"
            }
        }
    } catch {}
}
