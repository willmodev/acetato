using Acetato.Application.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Contenido del tooltip de un botón de la barra (HU-22): el nombre y, si la
/// acción del botón tiene atajo, su combinación («Lápiz  Ctrl+P»). El texto del
/// atajo lo mantiene al día <see cref="ToolbarShortcuts"/>.
/// </summary>
public sealed partial class ShortcutHint : ObservableObject
{
    public ShortcutHint(string label, HotkeyAction? action)
    {
        Label = label;
        Action = action;
    }

    /// <summary>Nombre del botón, en español.</summary>
    public string Label { get; }

    /// <summary>Acción con atajo que dispara el botón; <c>null</c> si no tiene.</summary>
    public HotkeyAction? Action { get; }

    /// <summary>Texto de la combinación actual; <c>null</c> = solo se muestra el nombre.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasShortcut))]
    private string? _shortcut;

    /// <summary>Hay atajo que mostrar.</summary>
    public bool HasShortcut => Shortcut is not null;

    /// <summary>Texto plano del tooltip (accesibilidad y respaldo sin plantilla).</summary>
    public override string ToString() => HasShortcut ? $"{Label}  {Shortcut}" : Label;
}
