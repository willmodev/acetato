# SPEC 13 — Base para producción: .NET 10, una sola copia, registro de errores y versión (HU-30 a HU-33)

> **Estado:** Aprobado · **Depende de:** SPEC 10 (HU-27) — bloque Dibujo de Ajustes, SPEC 12 (HU-29) — `settings.json` por secciones, HU-12 — avisos de Windows · **Fecha:** 2026-11-03
> **Objetivo:** Que Acetato corra sobre .NET 10, abra una sola copia, anote cada error en un registro que el usuario envía con un clic desde Ajustes y muestre su versión.

---

## Por qué existe este spec

- **.NET 10:** .NET 9 deja de recibir parches de seguridad el **10-11-2026**. .NET 10 es LTS, con soporte hasta noviembre de 2028.
- **Una sola copia:** si Acetato se abre dos veces, la segunda copia se queda sin atajos y el usuario cree que «no funciona».
- **Registro de errores:** hoy un fallo cierra Acetato sin dejar rastro. Con clientes que pagan, soporte necesita saber qué pasó.
- **Versión:** el proyecto no declara versión. Soporte y la Store la necesitan.
- Es la primera parte de `docs/lanzamiento-y-monetizacion.md` (puntos B1, B4, B5 y B6). El resto de ese documento va en otros specs.

### Cómo se manejan los errores en otras apps (investigado el 2026-11-03)

Hay tres capas habituales. Este spec construye **solo la primera**:

1. **Registro local que el usuario envía al pedir ayuda.** PowerToys (Microsoft) guarda sus registros en el equipo, no los sube solo, y tiene un botón «Report Bug» que arma un `.zip` en el escritorio con registros e información del sistema, quitando datos personales.
2. **Reportes automáticos de Windows.** Cuando una app de la Store se cierra por un error, Windows lo reporta y el desarrollador lo ve en el *Health report* de Partner Center (cierres, congelamientos, detalle del fallo). Llega sola al publicar en la Store. Solo cuenta a usuarios que comparten datos de diagnóstico opcionales.
3. **Servicio propio de reportes (p. ej. Sentry).** Exige pedir permiso al usuario y declararlo en la política de privacidad. Queda fuera.

Fuentes: [Bug Report Tool de PowerToys](https://microsoft.github.io/PowerToys/tools/bug-report-tool/) · [Registros de PowerToys](https://microsoft.github.io/PowerToys/development/logging/) · [Health report de Partner Center](https://learn.microsoft.com/en-us/windows/apps/publish/analyze-app-performance/health-report) · [Windows Error Reporting](https://learn.microsoft.com/en-us/windows/desktop/wer)

---

## Alcance

### .NET 10 (HU-30)

- Todos los proyectos (4 de `src/` y 4 de `tests/`) pasan de `net9.0` a `net10.0`, y de `net9.0-windows10.0.19041.0` a `net10.0-windows10.0.19041.0`.
- `global.json` pide el SDK 10.
- Los paquetes `Microsoft.Extensions.*` pasan a su versión 10.0.x estable más reciente en nuget.org al implementar.
- Los demás paquetes solo se actualizan si el build con .NET 10 lo exige.
- Las advertencias nuevas de los analizadores de .NET 10 se **corrigen**, no se suprimen.
- Para el usuario no cambia nada visible.

### Una sola copia (HU-31)

- Al abrir Acetato con una copia ya abierta **en la misma sesión de Windows**, la nueva copia se cierra sola, sin mostrar nada.
- La copia que ya estaba abierta hace lo mismo que «Activar dibujo» del menú de la bandeja: muestra la barra y entra en modo dibujo. Si ya estaba dibujando, no cambia nada.
- Dos usuarios de Windows con sesión abierta en el mismo equipo pueden usar Acetato a la vez, cada uno con su copia.
- El registro anota «Ya había otra copia abierta; esta se cierra».

### Registro y manejo de errores (HU-32)

**El registro:**

- Archivos de texto en `%LocalAppData%\Acetato\logs\`. Es la carpeta local del usuario, la que también usa PowerToys. A diferencia de `%AppData%` (donde está `settings.json`), no se copia entre equipos en redes de empresa.
- Un archivo por día: `acetato-AAAAMMDD.log`. Se conservan **los últimos 7**; los más viejos se borran solos.
- Tope de 10 MB por archivo. Si se llena, se abre otro ese mismo día.
- Cada línea lleva fecha y hora, nivel, de dónde viene y el mensaje. Los errores llevan además el detalle técnico completo.
- Se anota:
  - **Al arrancar:** versión completa de Acetato, versión de Windows, versión de .NET, idioma del sistema y cantidad de monitores.
  - **Al salir bien:** «Salida normal».
  - **Advertencias:** los casos que hoy se resuelven en silencio, por ejemplo un `settings.json` dañado, un atajo que no se pudo registrar o un aviso de Windows que no salió.
  - **Errores:** todo error inesperado (ver abajo).
- **Nunca** se anota lo dibujado, el texto escrito en las anotaciones ni lo que hay en pantalla.
- Nada sale del equipo.

**Qué pasa ante un error inesperado:**

| Dónde ocurre | Qué ve el usuario | Qué queda anotado |
|---|---|---|
| En la interfaz (barra, lienzo, Ajustes) | Acetato **sigue abierto**. La primera vez en la sesión sale un aviso de Windows: «Algo falló, pero Acetato sigue abierto» / «Si se repite, usa Ajustes › Reportar problema». Las siguientes, solo se anota. | Error con detalle. |
| En una tarea de fondo | Nada. | Advertencia con detalle. |
| Fuera de la interfaz (no se puede recuperar) | Un mensaje: «Acetato tuvo un problema y se va a cerrar. Se guardó un registro». Luego se cierra. | Error fatal con detalle. Windows lo reporta como cierre (capa 2). |
| Al arrancar, antes de que exista la barra | El mismo mensaje y se cierra. | Error fatal con detalle. |
| **Racha:** 3 errores de interfaz en menos de 10 segundos | Se trata como fatal: mensaje y cierre. Evita una app trabada repitiendo el mismo fallo. | Error fatal con el último detalle. |

- El aviso de Windows no tiene botones y hacer clic en él no abre nada.
- Si el usuario tiene los avisos de Windows apagados, no pasa nada más: el error queda anotado igual.

### Acerca de y Reportar problema (HU-33)

- La versión del proyecto es **0.9.0**. La 1.0.0 queda para el lanzamiento en la Store.
- Bloque nuevo **«Acerca de»** al final de la ventana **Ajustes**, después de los atajos:

  | Fila | Qué hace |
  |---|---|
  | «Acetato 0.9.0» + botón **«Copiar versión»** | Copia «0.9.0» al portapapeles. |
  | Botón **«Reportar problema»** + pista «Crea un .zip en el escritorio para enviarlo a soporte» | Arma el reporte (ver abajo). |

- **Reportar problema:**
  - Crea en el escritorio `Acetato-reporte-AAAAMMDD-HHMM.zip`.
  - Mientras lo arma, el botón queda desactivado.
  - Al terminar, abre el Explorador de Windows con el archivo seleccionado, y la pista cambia a «Reporte guardado en el escritorio».
  - Si falla, la pista dice «No se pudo crear el reporte», y el fallo queda anotado en el registro.
- **Contenido del `.zip`:**
  - `logs/`: los registros que haya (hasta 7 días).
  - `settings.json`: atajos, tintas, grosor y posición de la barra.
  - `sistema.txt`: versión completa de Acetato, versión y compilación de Windows, versión de .NET, idioma, y monitores con resolución y escala.
- **Antes de guardar**, en todo el contenido se cambia el nombre de usuario de Windows por `<usuario>` y el nombre del equipo por `<equipo>`.

**Fuera de alcance (para otros specs):**

- Enviar errores a un servidor propio o a un servicio como Sentry (capa 3).
- El *Health report* de la Store (capa 2): llega solo al publicar.
- Opción «Acerca de» en el menú de la bandeja o en la barra (decisión del usuario).
- Buscar actualizaciones, enlace a la web, correo de soporte o licencias en «Acerca de».
- Idioma inglés (B3), instalador MSIX, iniciar con Windows, primera ejecución, licencia y cobro.
- Actualizar paquetes que .NET 10 no obliga a cambiar.

---

## Modelo de datos y cambios por capa

`settings.json` **no cambia**. Lo nuevo vive en archivos aparte:

```text
%AppData%\Acetato\
└── settings.json                  (sin cambios)

%LocalAppData%\Acetato\
└── logs\
    ├── acetato-20261102.log
    └── acetato-20261103.log

Escritorio\Acetato-reporte-20261103-1530.zip
├── logs\acetato-2026110x.log
├── settings.json
└── sistema.txt
```

Línea de registro (formato de texto):

```text
2026-11-03 15:30:12.481 -05:00 [ERR] Acetato.Presentation.Diagnostics.UnhandledErrorHandler: Error en la interfaz
System.InvalidOperationException: …
   at …
```

**Proyecto (build):**

- `Directory.Build.props`: `<Version>0.9.0</Version>`. .NET agrega el código del commit a la versión completa (`0.9.0+0ba354c…`). Esa versión completa va al registro y a `sistema.txt`, y en pantalla se ve «0.9.0».
- `Directory.Packages.props`: `Microsoft.Extensions.*` en 10.0.x. Se agregan `Serilog`, `Serilog.Sinks.File`, `Serilog.Extensions.Logging`, `Microsoft.Extensions.Logging` y `Microsoft.Extensions.Logging.Abstractions`.
- `global.json`: SDK `10.0.100` con `rollForward: latestFeature`.

**Domain:** sin cambios.

**Application:**

- `AppVersion` (record `Display`, `Full`) con `AppVersion.Parse(string informationalVersion)`. Corta en el `+`. Es puro y tiene tests.
- `IAppInfo` (puerto): `AppVersion Version`.
- `ISingleInstanceGuard` (puerto, `IDisposable`): `bool TryClaim()`, `void NotifyOwner()` y `event EventHandler? ActivationRequested`.
- `ErrorPolicy` (caso de uso, usa `TimeProvider`): recibe cada error de interfaz y responde `Notify` (primera vez en la sesión), `Silent` o `Close` (racha: 3 en 10 s).
- `IErrorNotifier` (puerto): `void NotifyRecovered()`, el aviso de Windows.
- `IProblemReportWriter` (puerto): `Task<ProblemReportResult> CreateAsync(CancellationToken)`.
- `ProblemReportResult` (record `bool Succeeded`, `string? FilePath`).
- `ReportRedactor.Redact(string text, string userName, string machineName)`: puro y con tests. No toca palabras que solo contienen el nombre como parte de otra.
- `OverlayBroadcaster`: su `catch` anota una advertencia con `ILogger<T>`.

**Infrastructure (`Diagnostics/`):**

- `LogDirectory`: `%LocalAppData%\Acetato\logs` (`Environment.SpecialFolder.LocalApplicationData`).
- `SerilogSetup.Create(string directory)`: un archivo por día, 7 archivos, 10 MB, nivel mínimo `Information`.
- `AssemblyAppInfo : IAppInfo`: lee la versión completa del ensamblado de entrada.
- `NamedSingleInstanceGuard : ISingleInstanceGuard`: un `Mutex` y un `EventWaitHandle` con nombre en el espacio `Local\` (la sesión de Windows). La espera de la señal se hace en el pool de hilos, no en el de la interfaz. Es .NET administrado, sin P/Invoke.
- `ToastErrorNotifier : IErrorNotifier`: aviso de Windows sin argumentos ni botones. Nunca lanza.
- `SystemInfoText`: arma `sistema.txt` (usa `IMonitorService` para los monitores).
- `ZipProblemReportWriter : IProblemReportWriter`:
  - Lee los registros aunque Serilog tenga abierto el del día (lectura compartida).
  - Redacta el contenido y escribe el `.zip` en el escritorio.
  - Abre el Explorador con el archivo seleccionado.
  - Nunca lanza: devuelve `Succeeded = false`.
- `SettingsFile`, `HotkeySettingsJson` y `ToastHotkeyNotifier`: sus `catch` anotan una advertencia. Reciben `ILogger<T>` por constructor; el composition root se lo pasa.

**Presentation:**

- `Diagnostics/UnhandledErrorHandler`:
  - Se suscribe a `DispatcherUnhandledException` (interfaz), `TaskScheduler.UnobservedTaskException` (fondo) y `AppDomain.CurrentDomain.UnhandledException` (fatal).
  - Aplica `ErrorPolicy`.
  - En el caso fatal: anota, vacía el registro, muestra el mensaje y llama a `Environment.FailFast`, para que Windows lo reporte.
- `App.OnStartup`, en este orden:
  1. Registro.
  2. Manejador de errores.
  3. Una sola copia (si no es la primera, avisa a la dueña y sale).
  4. Lo de hoy.
  5. Línea «Inicio».

  Todo el arranque va envuelto: un error ahí es fatal. `OnExit` anota «Salida normal» y vacía el registro. Para no pasar de 400 líneas, el cableado nuevo vive en clases aparte, no en `App.xaml.cs`.
- La activación desde la segunda copia llega en otro hilo. Se pasa al de la interfaz y ejecuta lo mismo que `TrayViewModel.ActivateDrawingCommand`.
- `SettingsViewModel`: `VersionText`, `CopyVersionCommand`, `ReportProblemCommand` (asíncrono, desactivado mientras corre) y `ReportStatus`.
- `ClipboardWriter` (nuevo, pequeño): envuelve `Clipboard.SetText` para poder probar el VM.
- `SettingsWindow.xaml`: bloque «Acerca de» con los estilos actuales (`SettingsCaption`, `SettingsRowLabel`, `SettingsTextButton`, `SettingsHint`). Sin colores nuevos.

---

## Plan de implementación

1. **Backlog y spec.** Ya hechos al redactar el spec: HU-30 a HU-33 están en `BACKLOG.md`. Solo se incluyen en el primer commit.
2. **.NET 10.** `global.json`, los 8 `.csproj`, `Microsoft.Extensions.*` 10.0.x y el comentario de `Directory.Build.props`. Se corrigen las advertencias nuevas de los analizadores. `dotnet build -warnaserror` y `dotnet test` en verde. Fuera del repo: actualizar `.vscode/launch.json` local si apunta a la carpeta `net9.0-…`.
3. **Versión.** `<Version>0.9.0</Version>`, `AppVersion` con tests y `AssemblyAppInfo`.
4. **Registro.** Paquetes, `SerilogSetup`, `LogDirectory`, registro en el contenedor y líneas «Inicio» y «Salida normal». Test: escribe en una carpeta temporal y respeta el nombre por día.
5. **Advertencias en los `catch` actuales.** `SettingsFile`, `HotkeySettingsJson`, `ToastHotkeyNotifier` y `OverlayBroadcaster`. Tests: un `settings.json` dañado deja una advertencia.
6. **Manejo de errores.** `ErrorPolicy` con tests (primera vez, silencio, racha con tiempo simulado), `ToastErrorNotifier`, `UnhandledErrorHandler` y el arranque envuelto.
7. **Una sola copia.** `NamedSingleInstanceGuard` con tests (dos guardias con un nombre único por test: el segundo no reclama y su señal llega al primero), y cableado en `App` con la activación.
8. **Reporte.** `ReportRedactor` con tests, `SystemInfoText` y `ZipProblemReportWriter`. Tests en carpetas temporales: contenido del `.zip`, redacción y registro abierto por Serilog.
9. **Ajustes «Acerca de».** VM, `ClipboardWriter`, XAML y prueba de humo de la ventana (como `SettingsWindowSmokeTests`).
10. **Prueba manual** (con permiso del usuario). Abrir dos veces. Forzar un error de interfaz y uno fatal con una versión de prueba que no se sube. Copiar la versión. Reportar problema y revisar el `.zip`. Revisar que no queden archivos de más de 7 días.
11. **Design system** (primero el artefacto «Acetato» y luego la skill local): bloque «Acerca de» de Ajustes y el aviso de error. Cierre del spec.

---

## Criterios de aceptación

**.NET 10**

- [ ] Los 8 proyectos apuntan a `net10.0` o `net10.0-windows10.0.19041.0`.
- [ ] `dotnet build -warnaserror` y `dotnet test` en verde, sin reglas suprimidas nuevas.
- [ ] Acetato arranca y dibuja igual que antes en Windows 11.

**Una sola copia**

- [ ] Con Acetato abierto, abrirlo otra vez no deja un segundo proceso `Acetato.exe` vivo.
- [ ] Tras ese segundo intento, la barra está visible y en modo dibujo, aunque antes estuviera oculta o en modo puntero.
- [ ] Los atajos siguen funcionando después del segundo intento.
- [ ] El registro tiene la línea «Ya había otra copia abierta; esta se cierra».

**Registro y errores**

- [ ] Tras abrir y cerrar Acetato existe `%LocalAppData%\Acetato\logs\acetato-<fecha de hoy>.log` con «Inicio» (incluye la versión) y «Salida normal».
- [ ] Con 9 archivos de registro de días distintos, al arrancar quedan solo 7.
- [ ] Un `settings.json` dañado deja una advertencia en el registro.
- [ ] Un error forzado en la interfaz no cierra Acetato, muestra el aviso una sola vez por sesión y queda anotado cada vez.
- [ ] Tres errores de interfaz en menos de 10 segundos muestran el mensaje de cierre y Acetato se cierra.
- [ ] Un error fatal muestra el mensaje, cierra Acetato y queda anotado.
- [ ] Con los avisos de Windows apagados, un error de interfaz se anota y Acetato sigue abierto.
- [ ] Ningún registro contiene texto escrito en una anotación.

**Acerca de y reporte**

- [ ] Ajustes muestra el bloque «Acerca de» con «Acetato 0.9.0».
- [ ] «Copiar versión» deja «0.9.0» en el portapapeles.
- [ ] «Reportar problema» crea `Acetato-reporte-AAAAMMDD-HHMM.zip` en el escritorio y abre el Explorador con el archivo seleccionado.
- [ ] El `.zip` tiene `logs/`, `settings.json` y `sistema.txt` con versión completa, Windows, .NET, idioma y monitores.
- [ ] Ni el nombre de usuario de Windows ni el nombre del equipo aparecen en ningún archivo del `.zip`.
- [ ] Si el escritorio no se puede escribir, la pista dice «No se pudo crear el reporte» y Acetato sigue abierto.

**General**

- [ ] Ningún método, archivo ni clase supera los límites de Sonar (40 líneas, 400 líneas, complejidad 10, 5 parámetros, anidamiento 3).
- [ ] No hay P/Invoke nuevo fuera de Infrastructure. El code-behind solo llama a `InitializeComponent()`.
- [ ] Las dos copias del design system describen el bloque «Acerca de» y el aviso de error.

---

## Decisiones tomadas

1. **Solo B1, B4, B5 y B6 del documento de lanzamiento** (decisión del usuario). El resto va en otros specs, sin reparto fijado de antemano.
2. **Registro local con envío manual (capa 1)** (decisión del usuario). Es lo que hacen PowerToys y la mayoría. Se descartó un servicio propio de reportes por ahora: exige permiso del usuario, política de privacidad y un costo, antes de tener clientes.
3. **Seguir abierto ante errores de interfaz, con un aviso** (decisión del usuario). Se descartaron «siempre cerrar» (un fallo menor tumba la sesión) y «silencio total» (el usuario no sabe que puede reportar).
4. **Aviso una sola vez por sesión, sin clic.** Respeta el principio de cero distracción. Se descartó que el clic abra Ajustes: duplicaría la lógica del aviso de atajos de HU-21.
5. **Racha de 3 errores en 10 segundos = cierre.** Un error que se repite sin parar suele ser un bucle. Seguir abierto así es peor que cerrar. El usuario aprobó estos números; se ajustan si la prueba manual lo pide.
6. **El cierre fatal pasa por `Environment.FailFast`.** Así Windows lo cuenta como cierre y aparecerá en el *Health report* cuando se publique en la Store. Un error de interfaz recuperado no llega a Microsoft, porque la app no se cerró.
7. **Serilog** para escribir los registros. Es la librería estándar de .NET para registros en archivo y ya trae «un archivo por día, los últimos N». Se descartó escribir el archivo a mano.
8. **Registros en `%LocalAppData%\Acetato\logs`** (decisión del usuario, tras revisar lo estándar). Primero se eligió `%AppData%`, junto a `settings.json`. Se cambió porque los ajustes son del usuario y pueden viajar entre equipos, pero los registros son de cada equipo y no deben copiarse en redes de empresa. PowerToys los guarda igual.
9. **Una sola copia por sesión de Windows**, con mutex y señal con nombre en `Local\`, como un timbre que solo la primera copia escucha. Sin P/Invoke.
10. **La segunda copia activa el dibujo en la primera** (decisión del usuario). El usuario ve el efecto que buscaba al abrirla.
11. **«Acerca de» en Ajustes, no en la bandeja ni en la barra** (decisión del usuario). PowerToys también pone la versión en sus ajustes.
12. **«Reportar problema» arma un `.zip` en el escritorio** (decisión del usuario), como el Bug Report Tool de PowerToys. Se descartó «Abrir registros»: el usuario no sabría qué enviar.
13. **El `.zip` incluye `settings.json`.** Solo tiene atajos, tintas y posición de la barra, y ayuda a reproducir el fallo. Se quitan usuario y equipo, como hace PowerToys.
14. **Versión 0.9.0** (decisión del usuario). En pantalla, sin el código del commit; completa en el registro y en el reporte.
15. **Solo se actualizan los paquetes que .NET 10 exige.** Mantiene el cambio pequeño y fácil de revisar.
16. **Spec redactado completo de una vez, a pedido del usuario**, sin confirmar sección por sección. Las dudas que surgieron al redactarlo se resolvieron después: HU-30 a HU-33 en el backlog, registros en `%LocalAppData%`, racha de 3 en 10 s y aviso una vez por sesión.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Los analizadores de .NET 10 traen reglas nuevas y el build con `-warnaserror` falla en muchos archivos. | Se corrigen en el paso 2, antes de cualquier función nueva. Si son demasiadas, se separan en commits por regla. |
| El `launch.json` local sigue apuntando a la carpeta `net9.0-…` y se prueba un binario viejo (ya pasó con HU-12). | Paso 2: revisar `launch.json` y borrar `bin/` y `obj/` de `net9.0-…`. |
| En desarrollo, una copia «zombi» hace que la nueva sesión de depuración se cierre sola, y parece que «no arranca». | La línea del registro lo dice. Matar las copias vivas antes de depurar, como ya indica el flujo del proyecto. |
| `Environment.FailFast` no ejecuta `OnExit`: el ícono de la bandeja puede quedar «fantasma» hasta pasar el mouse. | Es el comportamiento estándar de Windows tras un cierre. Los atajos los libera el sistema al morir el proceso. |
| Windows puede mostrar su propio diálogo de «dejó de funcionar» después de nuestro mensaje. | Depende de la configuración de Windows Error Reporting del equipo. Se revisa en la prueba manual y se documenta lo que pase. |
| El escritorio está redirigido a OneDrive o no se puede escribir. | Se usa la carpeta Escritorio que reporta Windows. Si falla, la pista lo dice y queda anotado. |
| El nombre de usuario es una palabra común (p. ej. «ana») y la redacción estropea otras palabras. | La redacción reemplaza el nombre dentro de rutas (`\Users\ana\`) y como palabra completa. Hay tests con nombres cortos. |
| `App.xaml.cs` pasa de 400 líneas. | El cableado de registro, errores y una sola copia vive en clases propias. |

---

## Lo que **no** está en este spec

- Envío de errores a un servidor o a Sentry.
- *Health report* de la Store (llega al publicar).
- «Acerca de» en la bandeja o en la barra.
- Buscar actualizaciones, web, correo de soporte o licencia en «Acerca de».
- Inglés, MSIX, iniciar con Windows, primera ejecución, licencia y cobro.
- Actualizar paquetes que .NET 10 no exige.

Cada uno, si llega, va en su propio spec.
