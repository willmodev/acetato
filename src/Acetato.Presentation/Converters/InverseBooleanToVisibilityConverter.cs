using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Acetato.Presentation.Converters;

/// <summary>
/// <c>true</c> → Collapsed, <c>false</c> → Visible (HU-21): muestra Restaurar
/// solo en filas que difieren del predeterminado y alterna el pie de Ajustes.
/// </summary>
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
