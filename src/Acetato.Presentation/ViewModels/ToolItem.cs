using System.Windows.Media;
using Acetato.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Ítem de herramienta de la barra (HU-11): la herramienta, su tooltip (nombre
/// es-ES y atajo, HU-22), el icono Lucide y si está activa (para resaltarla con
/// el acento) o deshabilitada (placeholder aún no implementado).
/// </summary>
public sealed partial class ToolItem : ObservableObject
{
    public ToolItem(ToolKind kind, ShortcutHint hint, Geometry icon, bool isEnabled)
    {
        Kind = kind;
        Hint = hint;
        Icon = icon;
        IsEnabled = isEnabled;
    }

    /// <summary>Herramienta que selecciona este botón.</summary>
    public ToolKind Kind { get; }

    /// <summary>Tooltip: nombre en español y, si lo hay, el atajo.</summary>
    public ShortcutHint Hint { get; }

    /// <summary>Etiqueta accesible, en español.</summary>
    public string Label => Hint.Label;

    /// <summary>Geometría del icono Lucide.</summary>
    public Geometry Icon { get; }

    /// <summary>Si la herramienta está implementada (false = placeholder).</summary>
    public bool IsEnabled { get; }

    /// <summary>Indica si es la herramienta activa.</summary>
    [ObservableProperty]
    private bool _isSelected;
}
