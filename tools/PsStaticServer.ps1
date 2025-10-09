Param(
  [Parameter(Mandatory=$true)][string]$Root,
  [int]$Port = 8000
)

Add-Type -AssemblyName System.Net.HttpListener

if (-not (Test-Path $Root)) { throw "Root not found: $Root" }
Set-Location $Root

$listener = New-Object System.Net.HttpListener
$prefix = "http://localhost:$Port/"
$listener.Prefixes.Add($prefix)
$listener.Start()
Write-Host "Static server running at $prefix"

function Get-MimeType([string]$ext) {
  switch ($ext.ToLower()) {
    '.html' { return 'text/html' }
    '.htm'  { return 'text/html' }
    '.ico'  { return 'image/x-icon' }
    '.png'  { return 'image/png' }
    '.jpg'  { return 'image/jpeg' }
    '.jpeg' { return 'image/jpeg' }
    default { return 'application/octet-stream' }
  }
}

while ($true) {
  $ctx = $listener.GetContext()
  $req = $ctx.Request
  $res = $ctx.Response
  try {
    $local = $req.Url.LocalPath.TrimStart('/')
    if ([string]::IsNullOrWhiteSpace($local)) { $local = 'icon-preview.html' }
    $path = Join-Path (Get-Location) $local
    if (-not (Test-Path $path)) {
      $res.StatusCode = 404
      $bytes = [Text.Encoding]::UTF8.GetBytes('Not Found')
      $res.OutputStream.Write($bytes,0,$bytes.Length)
      $res.Close()
      continue
    }
    $ext = [IO.Path]::GetExtension($path)
    $mime = Get-MimeType $ext
    $bytes = [IO.File]::ReadAllBytes($path)
    $res.ContentType = $mime
    $res.ContentLength64 = $bytes.Length
    $res.OutputStream.Write($bytes,0,$bytes.Length)
  } catch {
    $res.StatusCode = 500
    $bytes = [Text.Encoding]::UTF8.GetBytes($_.Exception.Message)
    $res.OutputStream.Write($bytes,0,$bytes.Length)
  } finally {
    $res.Close()
  }
}