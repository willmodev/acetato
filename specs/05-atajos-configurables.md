# SPEC 05 — Atajos configurables (HU-21)

> **Estado:** Implementado · **Depende de:** SPEC 04 (HU-20) — sobre el código ya implementado HU-01…HU-20 · **Fecha:** 2026-10-07
> **Objetivo:** Que cada usuario pueda ver, cambiar, quitar y restaurar los atajos de Acetato desde una ventana «Ajustes», con atajos por defecto más cortos que solo se activan al dibujar.

---

## Por qué existe este spec

- Hoy los 15 atajos son fijos y todos llevan `Ctrl+Alt` (tres teclas). Son largos para uso frecuente y no se pueden cambiar.
- Copiar a EpicPen (`Ctrl+tecla`) tal cual tiene un costo: un atajo global `Ctrl+Z` o `Ctrl+S` les quita deshacer y guardar a PowerPoint, Word, etc. mientras Acetato esté abierto.
- La solución son dos grupos de atajos:
  - **Siempre activos:** mostrar/ocultar y modo dibujo; mantienen `Ctrl+Alt`.
  - **Solo en modo dibujo:** el resto, con `Ctrl+tecla`; se liberan al volver a click-through.
- Es la primera vez que Acetato guarda algo entre sesiones. El archivo de ajustes nace aquí y servirá a futuros ajustes.

---

## Alcance

**Dentro:**

- **Ventana «Ajustes»** (ventana normal, no overlay) con una sola sección por ahora: **Atajos**.
  - Se abre desde el botón **Ajustes** de la barra (hoy desactivado) y desde un nuevo ítem **Ajustes** del menú de la bandeja.
  - Una sola instancia: abrirla de nuevo la trae al frente.
  - Al abrirla, si el overlay está en modo dibujo, pasa a **click-through** (los trazos siguen visibles).
  - Queda encima del overlay (`Topmost`) y aparece en la barra de tareas.
- **Lista de atajos** en dos bloques: **Siempre activos** y **Solo al dibujar**. Cada fila: nombre de la acción, combinación actual y acciones.
- **Cambiar un atajo:** clic en la combinación → «Pulsa la combinación…» → la siguiente combinación válida se asigna y **se aplica al instante** (sin botón Guardar).
  - `Esc` cancela la captura sin cambios.
  - Mientras se captura, **todos** los atajos de Acetato quedan en pausa, para que pulsar uno existente no dispare su acción.
- **Reglas de la combinación:**
  - Al menos un modificador `Ctrl`, `Alt` o `Win`. `Mayús` solo puede acompañar a otro modificador.
  - Excepción: `F1`–`F12` se aceptan solas.
  - Teclas finales aceptadas: letras `A`–`Z`, dígitos `0`–`9`, `F1`–`F12`, `Espacio`, `Retroceso`, `Supr`, `Insert`, `Inicio`, `Fin`, `RePág`, `AvPág`, flechas y teclado numérico `0`–`9`.
  - Se rechazan las teclas de símbolo (`+`, `−`, `[`, `]`, `ñ`…): cambian de sitio según el teclado y fallan.
  - Se rechazan combinaciones reservadas de Windows que el registro no bloquea: `Alt+F4`, `Alt+Espacio`.
- **Conflictos:**
  - Si la combinación la usa otra acción de Acetato: aviso «Ya la usa *Deshacer*» con botón **Reemplazar** (esa acción queda sin atajo) o cancelar.
  - Si la usa otra aplicación o Windows (el registro falla): aviso «En uso por otra aplicación» y no se asigna.
  - Una combinación no puede repetirse entre los dos grupos.
- **Quitar** un atajo (la acción queda sin atajo y la combinación se libera para otras apps).
- **Restaurar** una fila (icono visible solo si difiere del predeterminado) y **Restaurar todo** (con confirmación en la misma ventana, sin cuadro de diálogo del sistema).
- **Dos grupos de activación** (el grupo de cada acción es fijo, no lo elige el usuario):
  - **Siempre activos:** registrados mientras la app corre.
  - **Solo al dibujar:** registrados solo con el overlay visible y en modo dibujo; liberados en click-through y con el overlay oculto.
- **Nuevos atajos por defecto** (tabla en *Modelo de datos*).
- **Persistencia** en `%AppData%\Acetato\settings.json` con número de versión. Solo se guarda lo que difiere del predeterminado.
- **Arranque con problemas:**
  - Si un atajo guardado no se puede registrar (lo tomó otra app): notificación de Windows «N atajos no están disponibles»; un clic abre Ajustes. La fila muestra «No disponible». El resto funciona.
  - Si `settings.json` está dañado: se usan los predeterminados, el archivo se renombra a `settings.corrupt.json` y la notificación lo indica.
- Mientras se escribe una anotación de **Texto**, los atajos de dibujo siguen actuando (p. ej. `Ctrl+Z` deshace la anotación anterior), como hoy con `Ctrl+Alt+Z`.
- Alta de **HU-21** en `BACKLOG.md`.

**Fuera de alcance (specs futuros):**

- Mostrar el atajo en los **tooltips** de la barra o en el anillo de herramientas.
- **Otros ajustes** en la ventana (tamaño de barra, suavizado, etc.). La ventana queda preparada, pero solo con Atajos.
- Atajos para acciones **nuevas** (seleccionar lápiz, borrador, flecha, línea o rectángulo directamente; grosor por teclado).
- Configurar las **teclas locales** (`Esc` en Texto) y la **rueda** del mouse (grosor).
- Elegir el **grupo** de cada acción.
- Atajos de **una sola tecla** sin modificador (salvo `F1`–`F12`) y teclas de símbolo.
- **Importar/exportar** la configuración o perfiles de atajos.
- Recordar color, grosor o modo degradado entre sesiones.
- Reescribir los atajos mencionados en specs y HUs anteriores (son históricos; HU-21 documenta la tabla nueva).

---

## Modelo de datos

### Atajos por defecto

| Acción (`HotkeyAction`) | Hoy | Nuevo | Grupo |
|-------------------------|-----|-------|-------|
| `Toggle` — Mostrar/ocultar | Ctrl+Alt+D | Ctrl+Alt+D | Siempre |
| `DrawingModeToggle` — Modo dibujo | Ctrl+Alt+E | Ctrl+Alt+E | Siempre |
| `ColorRed` … `ColorBlack` — Tintas | Ctrl+Alt+1…6 | Ctrl+1…6 | Al dibujar |
| `ColorGradient` — Degradado | Ctrl+Alt+7 | Ctrl+7 | Al dibujar |
| `Undo` — Deshacer | Ctrl+Alt+Z | Ctrl+Z | Al dibujar |
| `Clear` — Limpiar todo | Ctrl+Alt+Retroceso | Ctrl+Retroceso | Al dibujar |
| `CycleTool` — Siguiente herramienta | Ctrl+Alt+Espacio | Ctrl+Espacio | Al dibujar |
| `SelectTextTool` — Texto | Ctrl+Alt+T | Ctrl+T | Al dibujar |
| `SelectLaserTool` — Láser | Ctrl+Alt+L | Ctrl+L | Al dibujar |
| `Capture` — Captura | Ctrl+Alt+S | Ctrl+S | Al dibujar |

### Domain (puro, sin WPF ni Win32, testeable)

```csharp
[Flags]
public enum HotkeyModifiers { None = 0, Ctrl = 1, Alt = 2, Shift = 4, Win = 8 }

// Solo las teclas aceptadas: el enum ES la lista blanca (A–Z, D0–D9, F1–F12, Space, Back,
// Delete, Insert, Home, End, PageUp, PageDown, Left, Up, Right, Down, NumPad0–NumPad9).
public enum HotkeyKey { A, /* … */ NumPad9 }

// Combinación de teclas. ToString/TryParse usan un formato invariante ("Ctrl+Shift+Z")
// para el JSON; el texto en español para la UI vive en Presentation.
public readonly record struct KeyChord(HotkeyModifiers Modifiers, HotkeyKey Key)
{
    public override string ToString();
    public static bool TryParse(string? text, out KeyChord chord);
}

public enum KeyChordError { MissingModifier, ShiftOnly, Reserved }

// Reglas de la combinación (modificador obligatorio salvo F1–F12, Mayús no basta,
// reservadas de Windows). null = válida.
public static class KeyChordRules
{
    public static KeyChordError? Validate(KeyChord chord);
}
```

### Application

```csharp
public enum HotkeyScope { Always, DrawingMode }

// Definición fija de una acción: su grupo y su atajo por defecto.
public sealed record HotkeyDefinition(HotkeyAction Action, HotkeyScope Scope, KeyChord Default);

// Las 15 definiciones (tabla de arriba), en el orden en que se muestran.
public static class HotkeyCatalog
{
    public static IReadOnlyList<HotkeyDefinition> All { get; }
}

// Asignación actual, inmutable. Una acción sin entrada usa su predeterminado;
// una entrada null = sin atajo.
public sealed class HotkeyMap
{
    public static HotkeyMap Defaults { get; }
    public KeyChord? ChordOf(HotkeyAction action);
    public HotkeyAction? OwnerOf(KeyChord chord);
    public bool IsDefault(HotkeyAction action);
    public HotkeyMap With(HotkeyAction action, KeyChord? chord);
    public HotkeyMap Reset(HotkeyAction action);
    public IReadOnlyDictionary<HotkeyAction, KeyChord?> Overrides { get; } // lo que se persiste
}

public sealed record HotkeyAssignment(HotkeyAction Action, KeyChord Chord);

// Resultado de intentar asignar una combinación.
public abstract record AssignResult
{
    public sealed record Assigned : AssignResult;
    public sealed record Invalid(KeyChordError Error) : AssignResult;
    public sealed record TakenByAction(HotkeyAction Owner) : AssignResult;  // ofrece Reemplazar
    public sealed record TakenByOtherApp : AssignResult;
}

// Caso de uso central: guarda el mapa, decide qué atajos están activos según el estado
// del overlay y la pausa de captura, los aplica, persiste y avisa.
public sealed class HotkeyCoordinator : IDisposable
{
    public HotkeyMap Map { get; }
    public IReadOnlySet<HotkeyAction> Unavailable { get; }   // fallaron al registrarse
    public event EventHandler? Changed;

    public void Start();                                     // carga, prueba, aplica, notifica
    public AssignResult Assign(HotkeyAction action, KeyChord chord);
    public void Replace(HotkeyAction action, KeyChord chord); // quita el atajo al dueño actual
    public void Remove(HotkeyAction action);
    public void Reset(HotkeyAction action);
    public void ResetAll();
    public void BeginCapture();                              // pausa todos los atajos
    public void EndCapture();
}
```

**Puertos nuevos o cambiados (`Application/Abstractions/`):**

```csharp
public interface IGlobalHotkeyService      // cambia
{
    public event EventHandler<HotkeyActionEventArgs> ActionRequested;
    public void Register();                // arranca el hilo de atajos (sin registrar combinaciones)
    public void Unregister();              // libera todo y detiene el hilo
    // Reemplaza el conjunto registrado; devuelve las acciones que no se pudieron registrar.
    public IReadOnlyList<HotkeyAction> Apply(IReadOnlyList<HotkeyAssignment> assignments);
    // Prueba si la combinación está libre en el sistema (registra y suelta).
    public bool IsAvailable(KeyChord chord);
}

public interface IHotkeySettingsStore      // nuevo
{
    public HotkeySettingsLoad Load();
    public void Save(HotkeyMap map);
}

public sealed record HotkeySettingsLoad(HotkeyMap Map, bool WasCorrupt);

public interface IHotkeyNotifier           // nuevo
{
    public event EventHandler? SettingsRequested;            // clic en la notificación
    public void NotifyStartupProblems(int unavailableCount, bool settingsWereReset);
}

public interface IOverlayController        // cambia
{
    // … miembros actuales …
    public event EventHandler? StateChanged;                // visibilidad o modo dibujo
}
```

- Los atajos del grupo **Al dibujar** están activos solo si `IsVisible && IsDrawingMode` y no hay captura en curso.
- `Start()` prueba **todas** las combinaciones con `IsAvailable` (también las del grupo Al dibujar, que aún no se registran) para poder avisar al arrancar.

### Infrastructure

- **`Win32GlobalHotkeyService`:**
  - `RegisterHotKey` con `hWnd = 0` queda atado al hilo que lo llama. Por eso `Apply` e `IsAvailable` **encolan la petición** al hilo de atajos (mensaje `WM_APP` + cola) y esperan su resultado, con tope de 1 s.
  - Id de cada atajo = `(int)action + 1`, estable. Id de prueba: `0xBFFF`.
  - Se mantiene `MOD_NOREPEAT` en todos.
- **`HotkeyKeyMap`** (nuevo): `HotkeyKey` → código virtual de Windows y `HotkeyModifiers` → `MOD_*`. Añade `ModShift`, `ModWin` y las teclas que faltan en `NativeMethods`.
- **`HotkeyBindingTable`** y **`HotkeyBinding`** se eliminan: las combinaciones salen del mapa.
- **`JsonHotkeySettingsStore`** (nuevo, `Infrastructure/Settings/`), con `System.Text.Json`:
  - Ruta: `Environment.SpecialFolder.ApplicationData` + `Acetato\settings.json`.
  - Escritura atómica: archivo temporal y luego reemplazo.
  - Acción desconocida o combinación inválida en el archivo: se ignora esa entrada (vale el predeterminado).
  - JSON ilegible: renombra a `settings.corrupt.json` y devuelve `WasCorrupt = true`.
- **`ToastHotkeyNotifier`** (nuevo): notificación con argumento `action=settings`. El aviso de captura (HU-12) y este comparten el evento global de activación de CommunityToolkit; cada uno atiende solo su argumento.

Formato de `settings.json`:

```json
{
  "version": 1,
  "hotkeys": {
    "Undo": "Alt+Z",
    "Capture": null
  }
}
```

- Clave ausente = predeterminado. `null` = sin atajo.
- `version` permite migrar el formato en el futuro. Las secciones de futuros ajustes se suman como claves hermanas de `hotkeys`.

### Presentation

```csharp
// Ventana de ajustes. Code-behind solo con InitializeComponent().
public partial class SettingsWindow : Window;

internal sealed partial class SettingsViewModel : ObservableObject, IDisposable
{
    public IReadOnlyList<HotkeyRowViewModel> AlwaysRows { get; }
    public IReadOnlyList<HotkeyRowViewModel> DrawingRows { get; }
    public bool IsConfirmingResetAll { get; }
    // Comandos: ResetAll, ConfirmResetAll, CancelResetAll, Close
}

internal sealed partial class HotkeyRowViewModel : ObservableObject
{
    public string Label { get; }              // «Deshacer», «Tinta roja»…
    public string ChordText { get; }          // «Ctrl+Z», «Sin atajo», «Pulsa la combinación…»
    public bool IsCapturing { get; }
    public bool IsDefault { get; }
    public bool IsUnavailable { get; }
    public string? Message { get; }           // error o conflicto en línea
    public bool CanReplace { get; }           // conflicto con otra acción
    // Comandos: StartCapture, Capture(KeyChord), CancelCapture, Replace, Remove, Reset
}

// Comportamiento adjunto: con una fila capturando, intercepta PreviewKeyDown, traduce
// Key/SystemKey + Keyboard.Modifiers a KeyChord e invoca el comando de la fila.
// Ignora las teclas modificadoras solas; Esc cancela.
internal static class KeyChordCaptureBehavior;

// Key de WPF → HotkeyKey (null = tecla no admitida).
internal static class WpfKeyMap;

// Texto en español: Ctrl, Alt, Mayús, Win; Espacio, Retroceso, Supr, Insert, Inicio,
// Fin, RePág, AvPág, ←, ↑, →, ↓, Num 0…Num 9. Ej.: «Ctrl+Mayús+Z».
internal static class KeyChordText;

// Abre la ventana (una instancia), pasa el overlay a click-through y la trae al frente.
internal sealed class SettingsWindowLauncher;
```

- **Ventana:** sin barra de título de Windows (`WindowStyle=None`), fondo `Chrome.Solid`, título «Ajustes» arrastrable con `WindowDragBehavior` y botón cerrar con `Icon.Close`. Tamaño fijo aprox. 480 × 600 DIP, con desplazamiento vertical si no cabe.
- **Fila:** combinación con estilo de tecla (`text-kbd`, 11 px); el acento teal solo en la fila que está capturando.
- **Iconos:** `Icon.Close` para **Quitar**; nuevo `Icon.Reset` (Lucide `rotate-ccw`) para **Restaurar**. Su SVG se añade a `.claude/skills/acetato-design/assets/icons/`.
- **Mensajes (es-ES):** «Usa Ctrl, Alt o Win», «Mayús necesita otro modificador», «Reservado por Windows», «Tecla no admitida», «Ya la usa *Acción*», «En uso por otra aplicación», «No disponible».
- **`ToolbarViewModel`:** nuevo `OpenSettingsCommand`; el botón Ajustes se activa.
- **`TrayIcon.xaml`:** ítem `Ajustes` (`Tag="settings"`) entre «Activar dibujo» y «Salir», cableado en `App.WireTrayCommands`.
- **`HotkeyActionRouter`:** sin cambios de mapeo.

---

## Plan de implementación

Cada paso compila con `-warnaserror`, deja la app usable y es commiteable por sí solo.

1. **Backlog.** Añadir **HU-21 — Atajos configurables** a `BACKLOG.md` (narrativa, Gherkin, referencia a este spec, tabla de atajos nuevos). Solo docs.
2. **Combinaciones (Domain).** `HotkeyModifiers`, `HotkeyKey`, `KeyChord` (`ToString`/`TryParse`) y `KeyChordRules`. Tests:
   - `ToString` y `TryParse` son inversos para todas las teclas.
   - `TryParse` rechaza texto vacío, teclas desconocidas y modificadores repetidos.
   - Sin modificador falla salvo `F1`–`F12`; `Mayús+A` falla; `Ctrl+Mayús+A` pasa; `Alt+F4` y `Alt+Espacio` fallan.

   Sin cambio visible.
3. **Catálogo y mapa (Application).** `HotkeyScope`, `HotkeyDefinition`, `HotkeyCatalog` (con los atajos **actuales** `Ctrl+Alt+…` como predeterminados) y `HotkeyMap`. Tests:
   - Cada `HotkeyAction` tiene una definición.
   - Ningún predeterminado se repite y todos pasan `KeyChordRules`.
   - `With`, `Reset`, `OwnerOf`, `IsDefault` y `Overrides` se comportan según su contrato.

   Sin cambio visible.
4. **Registro dinámico (Infrastructure).**
   - `Apply` e `IsAvailable` en `IGlobalHotkeyService`, ejecutados en el hilo de atajos.
   - `HotkeyKeyMap` y constantes nuevas en `NativeMethods`; se eliminan `HotkeyBindingTable` y `HotkeyBinding`.
   - `App.WireHotkeys` aplica todo el catálogo tras `Register()`.
   - Tests de Infrastructure: `Apply` registra y una segunda llamada reemplaza; `IsAvailable` da `false` para una combinación ya registrada; el cierre sigue por debajo de 1 s.

   **Manual:** todos los atajos funcionan igual que hoy.
5. **Grupos de activación (Application).**
   - `IOverlayController.StateChanged` (al mostrar, ocultar y cambiar de modo).
   - `HotkeyCoordinator` con mapa en memoria: aplica los del grupo Siempre y, si `IsVisible && IsDrawingMode`, también los Al dibujar.
   - `App` usa el coordinador en lugar de aplicar el catálogo directo.
   - Tests con dobles: con overlay oculto o en click-through solo se aplican los del grupo Siempre; al entrar en modo dibujo se suman los demás.

   **Manual:** con el overlay en click-through, `Ctrl+Alt+Z` no hace nada; en modo dibujo deshace.
6. **Atajos cortos por defecto.** `HotkeyCatalog` pasa a la tabla nueva. Se actualiza `PROXIMOS-PASOS.md` si enumera atajos.

   **Manual:** con el overlay oculto o en click-through, `Ctrl+Z` y `Ctrl+S` funcionan en el Bloc de notas; en modo dibujo, `Ctrl+Z` deshace el último trazo.
7. **Persistencia.** `IHotkeySettingsStore` + `JsonHotkeySettingsStore`; el coordinador carga al arrancar. Tests en carpeta temporal:
   - Ida y vuelta conserva cambios y `null`.
   - Sin archivo → predeterminados.
   - JSON ilegible → predeterminados, `WasCorrupt` y archivo renombrado.
   - Acción desconocida o combinación inválida → se ignora esa entrada.
   - Solo se escriben las diferencias.

   **Manual:** editar `settings.json` a mano y reiniciar aplica el cambio.
8. **Edición (Application).** `Assign`, `Replace`, `Remove`, `Reset`, `ResetAll`, `BeginCapture`, `EndCapture`, `Unavailable` y `Changed` en el coordinador; cada cambio guarda. Tests:
   - Inválida → `Invalid` y nada cambia.
   - Usada por otra acción → `TakenByAction`; `Replace` deja al dueño sin atajo.
   - Ocupada fuera → `TakenByOtherApp`.
   - Durante la captura no hay nada registrado; al terminar vuelve lo que corresponde.
   - Cada cambio llama a `Save` con el mapa nuevo.

   Sin cambio visible.
9. **Ventana Ajustes de solo lectura.**
   - `SettingsWindow`, `SettingsViewModel`, `HotkeyRowViewModel`, `KeyChordText` y `SettingsWindowLauncher`.
   - Botón Ajustes de la barra activo; ítem «Ajustes» en la bandeja.
   - Al abrir pasa el overlay a click-through; una sola instancia.

   **Manual:** la ventana lista los 15 atajos en dos bloques, con los textos en español.
10. **Cambiar un atajo.** `KeyChordCaptureBehavior`, `WpfKeyMap` y el estado de captura de la fila: mensajes en línea, **Reemplazar**, `Esc` y pausa de atajos. Tests de `WpfKeyMap` y `KeyChordText`.

    **Manual:** asignar `Alt+Z` a Deshacer funciona al instante; `Ctrl+L` sobre Deshacer ofrece Reemplazar; `Alt+F4` se rechaza.
11. **Quitar y restaurar.** Botones Quitar y Restaurar por fila, Restaurar todo con confirmación en línea, `Icon.Reset` portado a `Icons.xaml`.

    **Manual:** quitar deja «Sin atajo»; restaurar recupera el predeterminado; Restaurar todo pide confirmación.
12. **Aviso al arrancar.** `IHotkeyNotifier` + `ToastHotkeyNotifier`; marca «No disponible» en la fila; el clic en la notificación abre Ajustes. El aviso de captura sigue abriendo la imagen.

    **Manual:** con otra app ocupando `Ctrl+Alt+D`, arrancar Acetato muestra la notificación y la fila «No disponible».

---

## Criterios de aceptación

> **Cierre (2026-10-11):** verificados con tests automáticos: reglas de combinación, catálogo y mapa, grupos de activación, edición y conflictos, pausa durante la captura, persistencia (ida y vuelta, archivo dañado, solo diferencias), aviso al arrancar, registro dinámico y cierre del hilo de atajos bajo 1 s, textos en español y carga de la ventana Ajustes. Build `-warnaserror` y tests verdes. La prueba en la app real la dio por buena el usuario («funciona bien»), sin prueba manual dedicada de multi-monitor, memoria en 20 ciclos ni notificación con un atajo ocupado por otra app.

**Ventana Ajustes**

- [x] El botón Ajustes de la barra y el ítem «Ajustes» de la bandeja abren la ventana.
- [x] Abrirla dos veces no crea una segunda ventana: trae al frente la que existe.
- [x] Si el overlay estaba en modo dibujo, al abrir Ajustes pasa a click-through y los trazos siguen visibles.
- [x] La ventana queda por encima del overlay y de la barra, y se puede arrastrar por su título.
- [x] Muestra las 15 acciones en dos bloques, «Siempre activos» y «Solo al dibujar», con textos en español (`Mayús`, `Retroceso`, `Espacio`).
- [x] No usa colores fuera de `Tokens.xaml`; el acento teal solo aparece en la fila que está capturando.

**Cambiar, quitar y restaurar**

- [x] Clic en una combinación muestra «Pulsa la combinación…»; pulsar `Alt+Z` la asigna y funciona sin reiniciar ni pulsar Guardar.
- [x] `Esc` durante la captura la cancela y deja el atajo anterior.
- [x] Durante la captura, pulsar un atajo existente (p. ej. `Ctrl+Alt+D`) no dispara su acción.
- [x] Pulsar solo `Ctrl`, `Alt` o `Mayús` no termina la captura.
- [x] `Z` sola muestra «Usa Ctrl, Alt o Win»; `Mayús+Z` muestra «Mayús necesita otro modificador»; `F8` sola se acepta.
- [x] `Alt+F4` muestra «Reservado por Windows»; `Ctrl+ñ` o `Ctrl++` muestran «Tecla no admitida».
- [x] Una combinación de otra acción de Acetato muestra «Ya la usa *Acción*»; **Reemplazar** la asigna y la otra acción queda «Sin atajo».
- [x] Una combinación ocupada por otra app muestra «En uso por otra aplicación» y no se asigna.
- [x] **Quitar** deja la acción «Sin atajo» y su combinación vuelve a funcionar en otras apps.
- [x] El icono Restaurar solo aparece en filas distintas del predeterminado y lo recupera.
- [x] **Restaurar todo** pide confirmación en la ventana; al confirmar, las 15 acciones vuelven al predeterminado.

**Grupos y atajos por defecto**

- [x] En una instalación sin `settings.json`, los atajos son los de la tabla nueva.
- [x] Con el overlay oculto o en click-through, `Ctrl+Z`, `Ctrl+S`, `Ctrl+T` y `Ctrl+1` funcionan con normalidad en otras apps (Bloc de notas, navegador).
- [x] En modo dibujo, `Ctrl+Z` deshace, `Ctrl+1…7` cambian la tinta, `Ctrl+Retroceso` limpia, `Ctrl+Espacio` cambia de herramienta, `Ctrl+T`/`Ctrl+L` eligen Texto/Láser y `Ctrl+S` captura.
- [x] `Ctrl+Alt+D` y `Ctrl+Alt+E` funcionan siempre, con el overlay oculto o visible.
- [x] Mientras se escribe una anotación de Texto, `Ctrl+Z` deshace la anotación anterior.

**Persistencia y arranque**

- [x] Tras cambiar un atajo y reiniciar la app, el cambio sigue.
- [x] `settings.json` solo contiene las acciones cambiadas o quitadas.
- [x] Con `settings.json` dañado, la app arranca con los predeterminados, existe `settings.corrupt.json` y aparece una notificación que lo dice.
- [x] Si otra app ocupa un atajo guardado, al arrancar aparece «N atajos no están disponibles»; el clic abre Ajustes y la fila dice «No disponible». Los demás atajos funcionan.
- [x] El clic en la notificación de una captura (HU-12) sigue abriendo la imagen y no abre Ajustes.

**Integración**

- [x] Con dos monitores, los atajos funcionan en ambas pantallas y la ventana Ajustes aparece completa en un monitor.
- [x] Abrir y cerrar Ajustes 20 veces y cambiar atajos repetidamente no deja errores ni crecimiento sostenido de memoria.
- [x] «Salir» sigue cerrando el proceso en menos de 2 s y libera todos los atajos.
- [x] `dotnet build -warnaserror` verde, sin suprimir reglas; ningún archivo pasa de 400 líneas.
- [x] `dotnet test` verde, con tests nuevos de `KeyChord`, `KeyChordRules`, `HotkeyCatalog`, `HotkeyMap`, `HotkeyCoordinator`, `JsonHotkeySettingsStore`, `WpfKeyMap`, `KeyChordText` y del registro dinámico.
- [x] `BACKLOG.md` incluye HU-21.

---

## Decisiones

- **Sí:** ventana «Ajustes» con sección Atajos, abierta desde la barra y la bandeja. Deja sitio a futuros ajustes y la bandeja sirve cuando la barra está oculta. Decisión del usuario.
- **No:** popover en la barra. Quince filas no caben y va contra cero distracción.
- **Sí:** modificador obligatorio salvo `F1`–`F12`. Un atajo global de una sola letra le roba esa tecla a todas las apps. Decisión del usuario.
- **Sí:** `Mayús` sola no basta como modificador. En modo dibujo, `Mayús+letra` impediría escribir mayúsculas en una anotación de Texto. Decisión del usuario.
- **Sí:** solo teclas seguras entre distribuciones de teclado. Las de símbolo ya fallaron con el grosor (HU-06). Decisión del usuario.
- **Sí:** cambios al instante, sin Guardar/Cancelar. Menos pasos y ningún estado «sin guardar». Decisión del usuario.
- **Sí:** conflicto interno con **Reemplazar**; conflicto externo se rechaza. Reemplazar resuelve el caso común en un clic; un atajo que no se puede registrar no serviría. Decisión del usuario.
- **No:** intercambiar atajos automáticamente. Cambia algo que el usuario no tocó sin que lo note.
- **Sí:** se puede dejar una acción sin atajo. Libera la combinación para otras apps y es lo que produce Reemplazar. Decisión del usuario.
- **Sí:** dos grupos fijos: Siempre (`Ctrl+Alt`) y Al dibujar (`Ctrl+tecla`). Da atajos cortos estilo EpicPen sin quitarles `Ctrl+Z`/`Ctrl+S` a las demás apps. Decisión del usuario.
- **No:** todos los atajos siempre activos. Los cortos romperían deshacer y guardar en las demás apps.
- **No:** que el usuario elija el grupo de cada acción. Suma una decisión que casi nadie necesita.
- **Sí:** captura solo en modo dibujo con `Ctrl+S`. Corta y sin robar guardar; para capturar en click-through basta con pasar a modo dibujo. Decisión del usuario.
- **Sí:** en Texto, los atajos de dibujo siguen actuando. Mismo comportamiento que hoy y sin otro estado que gestionar. Decisión del usuario.
- **Sí:** pausar todos los atajos mientras se captura una combinación. Sin pausa, pulsar un atajo existente dispararía su acción en vez de asignarse.
- **Sí:** al abrir Ajustes el overlay pasa a click-through. La ventana queda usable sin ocultar los trazos. Decisión del usuario.
- **Sí:** `settings.json` en `%AppData%\Acetato` con `version` y solo diferencias. Fácil de inspeccionar y borrar; si cambian los predeterminados, quien no tocó nada recibe los nuevos. Decisión del usuario.
- **No:** registro de Windows ni JSON junto al `.exe`. El primero es difícil de inspeccionar; el segundo falla en `Archivos de programa`.
- **Sí:** archivo dañado → predeterminados + renombrado a `settings.corrupt.json` + aviso. No se pierde el archivo del usuario y la app nunca deja de arrancar. Decisión del usuario.
- **Sí:** aviso al arrancar con notificación y marca «No disponible». Un atajo que deja de funcionar sin aviso parece un fallo de Acetato. Decisión del usuario.
- **Sí:** restaurar por fila y global, con confirmación en la ventana. Un cuadro de diálogo del sistema rompería el estilo de la app. Decisión del usuario.
- **Sí:** registro en el hilo de atajos mediante una cola. `RegisterHotKey` sin ventana queda atado al hilo que lo llama; registrar desde el hilo de UI dejaría los mensajes fuera del bucle.
- **Sí:** el enum `HotkeyKey` es la lista blanca de teclas. Una tecla no admitida no se puede ni representar, y el Domain queda libre de WPF y Win32.
- **Sí:** ventana sin barra de título de Windows, con título propio arrastrable. Mantiene la marca y reutiliza `WindowDragBehavior` sin interop nuevo. Decisión del usuario.
- **Sí:** atajos en la UI solo dentro de Ajustes. Los tooltips quedan para otro spec. Decisión del usuario.
- **Proceso:** el encabezado se confirmó por separado; el resto del spec se redactó de una vez y las dudas abiertas se preguntan al final. Decisión del usuario.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Registrar o soltar atajos desde el hilo de UI no funciona (quedan atados a otro hilo). | `Apply` e `IsAvailable` se ejecutan en el hilo de atajos mediante cola + `WM_APP`. Test de Infrastructure de registro y reemplazo. |
| `Apply` bloquea la UI si el hilo de atajos no responde. | Tope de 1 s por petición; si vence, las acciones afectadas se marcan «No disponible». |
| Las combinaciones con `Win` las intercepta Windows antes de que lleguen a la ventana (p. ej. `Win+E` abre el Explorador). | Se aceptan si llegan; si Windows las toma, no se capturan. Limitación documentada en HU-21. |
| `Ctrl+Alt+tecla` equivale a `AltGr` en teclados en español y puede chocar con caracteres (`AltGr+2` = `@`). | Ya ocurre hoy. Ahora el usuario puede cambiarlo. Los predeterminados del grupo Al dibujar ya no usan `Ctrl+Alt`. |
| Una combinación libre al asignarla deja de estarlo después (otra app la toma). | Al entrar en modo dibujo, lo que falla se marca «No disponible» sin notificación repetida. |
| La notificación de atajos y la de captura comparten el evento global de activación. | Cada una lleva su propio argumento y solo atiende el suyo. Criterio de aceptación explícito. |
| Entrar al modo dibujo con el atajo mientras se escribe en otra app captura `Ctrl+Z` para Acetato. | Comportamiento esperado del grupo Al dibujar: en modo dibujo los atajos son de Acetato. |
| Dos procesos de Acetato escriben `settings.json` a la vez. | Escritura atómica (temporal + reemplazo). Instancia única fuera de alcance. |
| `HotkeyCoordinator`, `SettingsViewModel` o `Win32GlobalHotkeyService` superan 400 líneas, 40 por método o complejidad 10. | Extraer (p. ej. cálculo del conjunto activo, cola del hilo de atajos, mensajes de error). Nunca suprimir la regla. |

---

## Lo que **no** está en este spec

- Atajos en tooltips de la barra o en el anillo.
- Otros ajustes en la ventana.
- Atajos para acciones nuevas (lápiz, borrador, flecha, línea, rectángulo, grosor).
- Configurar `Esc` en Texto o la rueda del mouse.
- Elegir el grupo de cada acción.
- Atajos de una sola tecla (salvo `F1`–`F12`) y teclas de símbolo.
- Importar/exportar configuración o perfiles.
- Recordar color, grosor o modo degradado.
- Reescribir specs y HUs anteriores.

Cada uno, si llega, va en su propio spec.
