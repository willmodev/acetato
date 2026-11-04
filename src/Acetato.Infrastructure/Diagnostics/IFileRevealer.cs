namespace Acetato.Infrastructure.Diagnostics;

/// <summary>Muestra un archivo seleccionado en el Explorador de Windows (HU-33).</summary>
public interface IFileRevealer
{
    /// <summary>Abre la carpeta del archivo con el archivo seleccionado.</summary>
    public void Reveal(string filePath);
}
