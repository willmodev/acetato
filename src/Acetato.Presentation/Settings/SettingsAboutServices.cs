using Acetato.Application.Abstractions;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Lo que necesita el bloque «Acerca de» de Ajustes (HU-33): la versión, el portapapeles y
/// el reporte para soporte. Van juntos para no pasar de cinco parámetros (S107).
/// </summary>
/// <param name="AppInfo">Versión de Acetato.</param>
/// <param name="Clipboard">Portapapeles, para «Copiar versión».</param>
/// <param name="Reports">Arma el .zip de «Reportar problema».</param>
public sealed record SettingsAboutServices(IAppInfo AppInfo, IClipboardWriter Clipboard, IProblemReportWriter Reports);
