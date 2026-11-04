namespace Acetato.Presentation.Settings;

/// <summary>Escribe texto en el portapapeles de Windows (HU-33, «Copiar versión»).</summary>
public interface IClipboardWriter
{
    /// <summary>Deja <paramref name="text"/> en el portapapeles. Nunca lanza.</summary>
    public void SetText(string text);
}
