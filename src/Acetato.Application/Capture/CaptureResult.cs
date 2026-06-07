namespace Acetato.Application.Capture;

/// <summary>Resultado de una captura (HU-12): la ruta del PNG guardado.</summary>
public sealed record CaptureResult(string FilePath);
