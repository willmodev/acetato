using Acetato.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Acetato.Application.Overlay;

/// <summary>
/// Lo que <see cref="OverlayBroadcaster"/> necesita para abrir el recorte de Windows
/// (HU-23): el lanzador, el registro donde anota si el sistema no lo pudo abrir (HU-32) y la
/// pizarra (HU-35), que debe seguir prendida mientras se recorta. Van juntos para no pasar de
/// cinco parámetros (S107).
/// </summary>
/// <param name="Launcher">Abre la herramienta Recortes del sistema.</param>
/// <param name="Logger">Registro del broadcaster.</param>
/// <param name="Whiteboard">La pizarra: el recorte la deja prendida al pasar a modo puntero.</param>
public sealed record ScreenClipServices(IScreenClipLauncher Launcher, ILogger<OverlayBroadcaster> Logger, IWhiteboard Whiteboard);
