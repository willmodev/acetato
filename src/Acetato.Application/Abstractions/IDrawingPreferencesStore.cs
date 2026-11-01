namespace Acetato.Application.Abstractions;

/// <summary>
/// Persistencia de las opciones de dibujo entre sesiones (HU-27). La implementación (la
/// sección <c>drawing</c> de settings.json) vive en Infrastructure. Nunca lanza: un
/// archivo ausente o dañado se resuelve con <see cref="DrawingPreferences.Defaults"/>.
/// </summary>
public interface IDrawingPreferencesStore
{
    /// <summary>Lee las opciones guardadas.</summary>
    public DrawingPreferences Load();

    /// <summary>Guarda las opciones sin tocar el resto del archivo (los atajos).</summary>
    public void Save(DrawingPreferences preferences);
}
