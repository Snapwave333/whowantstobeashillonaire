param(
    [string]$AppIconPath = "WhoWantsToBeAShillionaire/Assets/app.ico",
    [string]$InstallerIconPath = "Installer/Assets/installer.ico"
)

# PowerShell: because typing less is a lifestyle.
Add-Type -AssemblyName System.Drawing

function New-UniqueBitmap {
    param([int]$Size = 256)
    $bmp = New-Object System.Drawing.Bitmap $Size, $Size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::FromArgb(20,20,30))

    # Gradient background
    $rect = New-Object System.Drawing.Rectangle 0,0,$Size,$Size
    $rand = New-Object System.Random (Get-Random)
    $c1 = [System.Drawing.Color]::FromArgb(255, (100+$rand.Next(0,155)), (50+$rand.Next(0,205)), (120+$rand.Next(0,135)))
    $c2 = [System.Drawing.Color]::FromArgb(255, (50+$rand.Next(0,155)), (120+$rand.Next(0,135)), (50+$rand.Next(0,205)))
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $c1, $c2, 45
    $g.FillRectangle($brush, $rect)

    # Random glass circles
    for($i=0; $i -lt 8; $i++){
        $r = $rand.Next(20, [math]::Floor($Size/2))
        $x = $rand.Next(0, $Size-$r)
        $y = $rand.Next(0, $Size-$r)
        $alpha = $rand.Next(30, 100)
        $circleColor = [System.Drawing.Color]::FromArgb($alpha, 255,255,255)
        $g.FillEllipse((New-Object System.Drawing.SolidBrush $circleColor), $x, $y, $r, $r)
    }

    # Title text
    $text = "WWS"
    try {
        $font = New-Object System.Drawing.Font('Segoe UI Semibold', [single]($Size*0.33), [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    } catch {
        $font = New-Object System.Drawing.Font('Segoe UI', [single]($Size*0.33), [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    }
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = 'Center'
    $sf.LineAlignment = 'Center'
    $textRect = New-Object System.Drawing.RectangleF 0, 0, $Size, $Size
    $shadow = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(80,0,0,0))
    if($font){
        $g.DrawString($text, $font, $shadow, ($textRect.X+4), ($textRect.Y+4), $sf)
        $fg = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(245, 255, 215, 0))
        $g.DrawString($text, $font, $fg, $textRect, $sf)
    }

    $g.Dispose()
    return $bmp
}

function Save-PngToIco {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [string]$Path
    )
    # ICO with embedded PNG (Vista+ supported)
    $ms = New-Object System.IO.MemoryStream
    $Bitmap.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $ms.ToArray()
    $ms.Dispose()

    $bw = New-Object System.IO.BinaryWriter([System.IO.File]::Open($Path, [System.IO.FileMode]::Create))
    try {
        # ICONDIR
        $bw.Write([UInt16]0)    # Reserved
        $bw.Write([UInt16]1)    # Type = Icon
        $bw.Write([UInt16]1)    # Count

        # ICONDIRENTRY
        $width = 256
        $height = 256
        $bw.Write([Byte]0)      # 0 means 256
        $bw.Write([Byte]0)      # 0 means 256
        $bw.Write([Byte]0)      # ColorCount
        $bw.Write([Byte]0)      # Reserved
        $bw.Write([UInt16]0)    # Planes
        $bw.Write([UInt16]32)   # BitCount
        $bw.Write([UInt32]$pngBytes.Length) # BytesInRes
        $bw.Write([UInt32]22)   # ImageOffset (6+16)

        # PNG data
        $bw.Write($pngBytes)
    }
    finally {
        $bw.Flush(); $bw.Close(); $bw.Dispose()
    }
}

function Ensure-Directory {
    param([string]$Dir)
    if(-not (Test-Path $Dir)) { New-Item -ItemType Directory -Path $Dir | Out-Null }
}

Ensure-Directory (Split-Path -Parent $AppIconPath)
Ensure-Directory (Split-Path -Parent $InstallerIconPath)

$bmp1 = New-UniqueBitmap -Size 256
Save-PngToIco -Bitmap $bmp1 -Path $AppIconPath
$bmp1.Dispose()

$bmp2 = New-UniqueBitmap -Size 256
Save-PngToIco -Bitmap $bmp2 -Path $InstallerIconPath
$bmp2.Dispose()
Write-Host "Generated icons:`n - $AppIconPath`n - $InstallerIconPath"