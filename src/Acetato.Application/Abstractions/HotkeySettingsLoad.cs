using Acetato.Application.Hotkeys;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Resultado de leer los atajos guardados (HU-21). <see cref="WasCorrupt"/>
/// indica que el archivo no se pudo leer y se volvió a los predeterminados.
/// </summary>
public sealed record HotkeySettingsLoad(HotkeyMap Map, bool WasCorrupt);
