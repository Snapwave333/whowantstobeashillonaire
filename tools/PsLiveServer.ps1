Param(
  [Parameter(Mandatory=$true)][string]$Root,
  [int]$Port = 8000
)

# MaxSoft Live Server: PowerShell edition — because typing less is a lifestyle
# Using HttpListener directly; no need to Add-Type for most Windows setups.

if (-not (Test-Path $Root)) { throw "Root not found: $Root" }
Set-Location $Root

$listener = New-Object System.Net.HttpListener
$prefix = "http://127.0.0.1:$Port/"
$listener.Prefixes.Add($prefix)
$listener.Start()
Write-Host "Live server running at $prefix"

# Thread-safe list of SSE clients
$sseClients = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))

function Get-MimeType([string]$ext) {
  switch ($ext.ToLower()) {
    '.html' { return 'text/html' }
    '.htm'  { return 'text/html' }
    '.ico'  { return 'image/x-icon' }
    '.png'  { return 'image/png' }
    '.jpg'  { return 'image/jpeg' }
    '.jpeg' { return 'image/jpeg' }
    '.webp' { return 'image/webp' }
    '.js'   { return 'application/javascript' }
    '.css'  { return 'text/css' }
    '.svg'  { return 'image/svg+xml' }
    '.json' { return 'application/json' }
    '.woff' { return 'font/woff' }
    '.woff2' { return 'font/woff2' }
    '.ttf'  { return 'font/ttf' }
    '.otf'  { return 'font/otf' }
    '.eot'  { return 'application/vnd.ms-fontobject' }
    '.mp3'  { return 'audio/mpeg' }
    '.wav'  { return 'audio/wav' }
    default { return 'application/octet-stream' }
  }
}

function Inject-LiveReload([string]$html) {
  $snippet = @"
<script>
// MaxSoft Live Reload — smoother than a fresh Windows install
(function(){
  try {
    var es = new EventSource('/__events');
    es.onmessage = function(ev){
      if (ev && ev.data === 'reload') {
        console.log('[LiveReload] Change detected. Reloading...');
        location.reload();
      }
    };
    es.onerror = function(){ console.warn('[LiveReload] SSE connection error'); };
  } catch (e) { console.error('[LiveReload] Failed to initialize', e); }
})();
</script>
"@
  if ($html -match '</body>') {
    return ($html -replace '</body>', "$snippet`n</body>")
  } else {
    return "$html`n$snippet"
  }
}

function Broadcast([string]$data) {
  foreach ($client in @($sseClients)) {
    try {
      $writer = New-Object System.IO.StreamWriter($client.Stream)
      $writer.AutoFlush = $true
      if ($data -eq ':') {
        # SSE comment keepalive
        $writer.WriteLine(': ping')
        $writer.WriteLine('')
      } else {
        $writer.WriteLine("data: $data")
        $writer.WriteLine("")
      }
    } catch {
      # Client likely disconnected; prune
      [void]$sseClients.Remove($client)
    }
  }
}

function Should-Ignore([string]$p) {
  $lower = $p.ToLower()
  return (
    $lower -match '\\.git\\' -or
    $lower -match '\\node_modules\\' -or
    $lower -match '\\obj\\' -or
    $lower -match '\\bin\\' -or
    $lower -match '\\.trae\\'
  )
}

# File change watcher — let's automate that like it's 1999
$fsw = New-Object IO.FileSystemWatcher (Get-Location), '*'
$fsw.IncludeSubdirectories = $true
$fsw.EnableRaisingEvents = $true
$fsw.NotifyFilter = [IO.NotifyFilters]'FileName, LastWrite'

# Debounced reload timer
$reloadTimer = New-Object System.Timers.Timer 300
$reloadTimer.AutoReset = $false
Register-ObjectEvent -InputObject $reloadTimer -EventName Elapsed -Action { Broadcast 'reload' } | Out-Null

# SSE keepalive timer
$keepaliveTimer = New-Object System.Timers.Timer 15000
$keepaliveTimer.AutoReset = $true
Register-ObjectEvent -InputObject $keepaliveTimer -EventName Elapsed -Action { Broadcast ':' } | Out-Null
$keepaliveTimer.Start()

# On any relevant change, schedule a reload
Register-ObjectEvent -InputObject $fsw -EventName Changed -Action { if (-not (Should-Ignore $event.SourceEventArgs.FullPath)) { $reloadTimer.Stop(); $reloadTimer.Start() } } | Out-Null
Register-ObjectEvent -InputObject $fsw -EventName Created -Action { if (-not (Should-Ignore $event.SourceEventArgs.FullPath)) { $reloadTimer.Stop(); $reloadTimer.Start() } } | Out-Null
Register-ObjectEvent -InputObject $fsw -EventName Deleted -Action { if (-not (Should-Ignore $event.SourceEventArgs.FullPath)) { $reloadTimer.Stop(); $reloadTimer.Start() } } | Out-Null
Register-ObjectEvent -InputObject $fsw -EventName Renamed -Action { if (-not (Should-Ignore $event.SourceEventArgs.FullPath)) { $reloadTimer.Stop(); $reloadTimer.Start() } } | Out-Null

while ($true) {
  $ctx = $listener.GetContext()
  $req = $ctx.Request
  $res = $ctx.Response
  $keepOpen = $false
  try {
    $local = [Uri]::UnescapeDataString($req.Url.LocalPath.TrimStart('/'))
    if ([string]::IsNullOrWhiteSpace($local)) { $local = 'index.html' }

    if ($req.Url.AbsolutePath -eq '/__events') {
      # SSE endpoint
      $res.StatusCode = 200
      $res.Headers['Cache-Control'] = 'no-cache'
      $res.Headers['Connection'] = 'keep-alive'
      $res.ContentType = 'text/event-stream'
      $res.KeepAlive = $true
      $res.SendChunked = $true
      $keepOpen = $true

      # Send initial event and register client
      $writer = New-Object System.IO.StreamWriter($res.OutputStream)
      $writer.AutoFlush = $true
      $writer.WriteLine(': connected')
      $writer.WriteLine('data: connected')
      $writer.WriteLine('')

      # Track client
      [void]$sseClients.Add(@{ Stream = $res.OutputStream })
      continue
    }

    $path = Join-Path (Get-Location) $local
    if (-not (Test-Path $path)) {
      $res.StatusCode = 404
      $bytesNF = [Text.Encoding]::UTF8.GetBytes('Not Found')
      $res.OutputStream.Write($bytesNF,0,$bytesNF.Length)
      $res.Close()
      continue
    }

    $ext = [IO.Path]::GetExtension($path)
    $mime = Get-MimeType $ext

    if ($ext -in '.html','.htm') {
      $text = [IO.File]::ReadAllText($path)
      $text = Inject-LiveReload $text
      $bytes = [Text.Encoding]::UTF8.GetBytes($text)
      $res.ContentType = $mime
      $res.ContentLength64 = $bytes.Length
      $res.OutputStream.Write($bytes,0,$bytes.Length)
    } else {
      $bytes = [IO.File]::ReadAllBytes($path)
      $res.ContentType = $mime
      $res.ContentLength64 = $bytes.Length
      $res.OutputStream.Write($bytes,0,$bytes.Length)
    }
  } catch {
    $res.StatusCode = 500
    $err = "Error: " + $_.Exception.Message
    $bytesErr = [Text.Encoding]::UTF8.GetBytes($err)
    $res.OutputStream.Write($bytesErr,0,$bytesErr.Length)
  } finally {
    if (-not $keepOpen) { $res.Close() }
  }
}