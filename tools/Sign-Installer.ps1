Param(
    [string]$ExecutablePath = "Installer/bin/Release/net8.0-windows/win-x64/publish/Shillionaire-Setup.exe",
    [string]$PfxPath = $null,
    [string]$PfxPassword = $null,
    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

function Ensure-DevCert {
    $certName = "Shillionaire Studios Dev";
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My","CurrentUser");
    $store.Open("ReadWrite");
    $codeSigningOid = "1.3.6.1.5.5.7.3.3" # Code Signing
    $cert = $store.Certificates |
        Where-Object {
            $_.Subject -like "*CN=$certName*" -and $_.HasPrivateKey -and ($_.Extensions | Where-Object { $_.Oid.Value -eq "2.5.29.37" -and $_.Format(1) -like "*${codeSigningOid}*" })
        } | Select-Object -First 1
    if (-not $cert) {
        Write-Host "Creating self-signed development certificate (code signing)...";
        try {
            $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=$certName" -CertStoreLocation "Cert:\CurrentUser\My" -KeyAlgorithm RSA -KeyLength 2048 -NotAfter (Get-Date).AddYears(1) -FriendlyName $certName
        } catch {
            # Fallback: create cert with EKU for code signing
            $eku = "2.5.29.37={text}1.3.6.1.5.5.7.3.3" # Code Signing
            $cert = New-SelfSignedCertificate -Subject "CN=$certName" -CertStoreLocation "Cert:\CurrentUser\My" -KeyAlgorithm RSA -KeyLength 2048 -NotAfter (Get-Date).AddYears(1) -FriendlyName $certName -TextExtension $eku -KeyUsage DigitalSignature
        }
    }
    $store.Close();
    return $cert;
}

function Export-DevCertPfx($cert, $pfxPath, $password) {
    if (-not $pfxPath) { $pfxPath = Join-Path $env:TEMP "shillionaire-dev.pfx" }
    if (-not $password) { $password = "Shill1onaire!" }
    Write-Host "Exporting PFX to $pfxPath";
    $secure = ConvertTo-SecureString -String $password -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $secure | Out-Null
    return @{ Path = $pfxPath; Password = $password }
}

function Sign-WithCert($exePath, $pfxPath, $password, $timestampUrl) {
    if (-not (Test-Path $exePath)) { throw "Executable not found: $exePath" }
    $signtool = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($signtool) {
        $args = @("sign", "/fd", "SHA256", "/tr", $timestampUrl, "/td", "SHA256")
        if ($pfxPath) {
            $args += @("/f", $pfxPath)
            if ($password) { $args += @("/p", $password) }
        } else {
            # Use cert in CurrentUser\My store
            $args += @("/n", "Shillionaire Studios Dev")
        }
        $args += $exePath
        Write-Host "Running signtool with args: $($args -join ' ')"
        & $signtool.Source $args
    } else {
        Write-Warning "signtool.exe not found. Falling back to Set-AuthenticodeSignature."
        # Import to store if we have a PFX
        $cert = $null
        if ($pfxPath) {
            $secure = $null
            if ($password) { $secure = ConvertTo-SecureString -String $password -Force -AsPlainText }
            $info = Import-PfxCertificate -FilePath $pfxPath -CertStoreLocation Cert:\CurrentUser\My -Password $secure -Exportable
            $cert = $info | Select-Object -First 1
        }
        if (-not $cert) { $cert = Ensure-DevCert }
        if (-not $cert) { throw "No certificate found to sign with." }
        Set-AuthenticodeSignature -FilePath $exePath -Certificate $cert -TimeStampServer $timestampUrl | Out-Null
    }
}

try {
    if (-not $PfxPath) {
        $cert = Ensure-DevCert
        $info = Export-DevCertPfx -cert $cert -pfxPath $null -password $PfxPassword
        $PfxPath = $info.Path
        if (-not $PfxPassword) { $PfxPassword = $info.Password }
    }
    Sign-WithCert -exePath $ExecutablePath -pfxPath $PfxPath -password $PfxPassword -timestampUrl $TimestampUrl
    Write-Host "Signing complete."
} catch {
    Write-Error $_
    exit 1
}