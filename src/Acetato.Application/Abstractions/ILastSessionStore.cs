namespace Acetato.Application.Abstractions;

/// <summary>
/// Persistencia de la última sesión (HU-29). La implementación (la sección
/// <c>session</c> de settings.json) vive en Infrastructure. Nunca lanza: un archivo
/// ausente o dañado se resuelve con <see cref="LastSession.Defaults"/>.
/// </summary>
public interface ILastSessionStore
{
    /// <summary>Lee la última sesión guardada.</summary>
    public LastSession Load();

    /// <summary>Guarda la sesión sin tocar el resto del archivo.</summary>
    public void Save(LastSession session);
}
