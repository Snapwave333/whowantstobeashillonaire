Param(
  [Parameter(Mandatory=$true)][string]$PngPath,
  [Parameter(Mandatory=$true)][string]$OutIcoPath,
  [int[]]$Sizes = @(16,24,32,48,64,128,256)
)

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $PngPath)) { throw "PNG not found: $PngPath" }
$bmp = [System.Drawing.Bitmap]::FromFile($PngPath)

function Resize-Bitmap($bmp, $size) {
  $target = New-Object System.Drawing.Bitmap($size, $size)
  $g = [System.Drawing.Graphics]::FromImage($target)
  $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
  $g.DrawImage($bmp, 0, 0, $size, $size)
  $g.Dispose()
  return $target
}

function Save-Ico($bitmaps, $path) {
  $fs = [System.IO.File]::Create($path)
  $writer = New-Object System.IO.BinaryWriter($fs)
  # ICO header
  $writer.Write([UInt16]0) # reserved
  $writer.Write([UInt16]1) # type = icon
  $writer.Write([UInt16]$bitmaps.Count)

  $imageDataList = @()
  $offset = 6 + (16 * $bitmaps.Count)

  foreach ($b in $bitmaps) {
    $ms = New-Object System.IO.MemoryStream
    $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $ms.ToArray()
    $imageDataList += $pngBytes

    $w = ($b.Width -eq 256) ? 0 : [Byte]$b.Width
    $h = ($b.Height -eq 256) ? 0 : [Byte]$b.Height
    $writer.Write([Byte]$w)
    $writer.Write([Byte]$h)
    $writer.Write([Byte]0) # colors
    $writer.Write([Byte]0) # reserved
    $writer.Write([UInt16]0) # planes
    $writer.Write([UInt16]32) # bit count
    $writer.Write([UInt32]$pngBytes.Length)
    $writer.Write([UInt32]$offset)
    $offset += $pngBytes.Length
  }

  foreach ($pngBytes in $imageDataList) {
    $writer.Write($pngBytes)
  }
  $writer.Dispose()
  $fs.Dispose()
}

$icons = New-Object System.Collections.Generic.List[System.Drawing.Bitmap]
foreach ($s in $Sizes) { $icons.Add((Resize-Bitmap $bmp $s)) }
Save-Ico -bitmaps $icons -path $OutIcoPath
Write-Host "ICO saved: $OutIcoPath"