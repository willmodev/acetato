# Genera el icono de bandeja (acetato.ico) a partir del isotipo de marca
# (.claude/skills/acetato-design/assets/acetato-isotype.svg): dos rectangulos
# redondeados, gris arriba-derecha y teal (#2FD9C4) abajo-izquierda, sobre un
# lienzo 40x40. Se renderiza con WPF a varios tamanos y se ensambla un .ico
# multi-resolucion con marcos PNG. Requiere Windows PowerShell (STA).
#
# Uso:  powershell -ExecutionPolicy Bypass -File tools\generate-tray-icon.ps1

Add-Type -AssemblyName PresentationCore, PresentationFramework, WindowsBase

$ErrorActionPreference = 'Stop'

function New-IsotypeDrawing {
    $group = New-Object System.Windows.Media.DrawingGroup

    # Rectangulo gris (arriba-derecha): relleno casi transparente + borde gris.
    $rect1 = New-Object System.Windows.Media.RectangleGeometry(
        (New-Object System.Windows.Rect(12, 1, 26, 26)), 8, 8)
    $fill1 = New-Object System.Windows.Media.SolidColorBrush(
        [System.Windows.Media.Color]::FromArgb(10, 255, 255, 255))
    $pen1 = New-Object System.Windows.Media.Pen(
        (New-Object System.Windows.Media.SolidColorBrush(
            [System.Windows.Media.Color]::FromRgb(0x8A, 0x92, 0x9E))), 2)
    [void]$group.Children.Add(
        (New-Object System.Windows.Media.GeometryDrawing($fill1, $pen1, $rect1)))

    # Rectangulo teal (abajo-izquierda): relleno teal tenue + borde teal de marca.
    $rect2 = New-Object System.Windows.Media.RectangleGeometry(
        (New-Object System.Windows.Rect(1, 13, 26, 26)), 8, 8)
    $fill2 = New-Object System.Windows.Media.SolidColorBrush(
        [System.Windows.Media.Color]::FromArgb(41, 47, 217, 196))
    $pen2 = New-Object System.Windows.Media.Pen(
        (New-Object System.Windows.Media.SolidColorBrush(
            [System.Windows.Media.Color]::FromRgb(0x2F, 0xD9, 0xC4))), 2)
    [void]$group.Children.Add(
        (New-Object System.Windows.Media.GeometryDrawing($fill2, $pen2, $rect2)))

    return $group
}

function Get-PngBytes($drawing, [int]$size) {
    $visual = New-Object System.Windows.Media.DrawingVisual
    $context = $visual.RenderOpen()
    $scale = $size / 40.0
    $context.PushTransform((New-Object System.Windows.Media.ScaleTransform($scale, $scale)))
    $context.DrawDrawing($drawing)
    $context.Pop()
    $context.Close()

    $bitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap(
        $size, $size, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)

    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    [void]$encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream = New-Object System.IO.MemoryStream
    $encoder.Save($stream)
    # La coma evita que PowerShell desenrolle el byte[] al devolverlo.
    return , $stream.ToArray()
}

$sizes = 16, 24, 32, 48, 64, 128, 256
$drawing = New-IsotypeDrawing
$frames = @{}
foreach ($size in $sizes) { $frames[$size] = Get-PngBytes $drawing $size }

$output = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($output)

# ICONDIR
$writer.Write([uint16]0)            # reservado
$writer.Write([uint16]1)            # tipo: icono
$writer.Write([uint16]$sizes.Count) # numero de imagenes

$offset = 6 + (16 * $sizes.Count)
foreach ($size in $sizes) {
    [byte[]]$data = $frames[$size]
    $dim = if ($size -ge 256) { 0 } else { $size } # 0 codifica 256 en el formato ICO
    $writer.Write([byte]$dim)        # ancho
    $writer.Write([byte]$dim)        # alto
    $writer.Write([byte]0)           # colores de paleta
    $writer.Write([byte]0)           # reservado
    $writer.Write([uint16]1)         # planos
    $writer.Write([uint16]32)        # bits por pixel
    $writer.Write([uint32]$data.Length)
    $writer.Write([uint32]$offset)
    $offset += $data.Length
}
foreach ($size in $sizes) { $writer.Write([byte[]]$frames[$size]) }
$writer.Flush()

$target = Join-Path $PSScriptRoot '..\src\Acetato.Presentation\Resources\acetato.ico'
$target = [System.IO.Path]::GetFullPath($target)
[System.IO.File]::WriteAllBytes($target, $output.ToArray())
Write-Output "Icono generado: $target ($($output.Length) bytes, $($sizes.Count) tamanos)"
