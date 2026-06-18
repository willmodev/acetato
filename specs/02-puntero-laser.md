# SPEC 02 — Puntero láser (HU-15)

> **Estado:** Implementado · **Depende de:** — (sobre el código ya implementado HU-01…HU-14) · **Fecha:** 2026-06-17
> **Objetivo:** Añadir una herramienta **láser efímera** que muestra un punto/halo luminoso fijo en el cursor y, al arrastrar con el botón izquierdo, dibuja una estela que se desvanece sola sin dejar trazo persistente ni entrada en el historial.

---

## Alcance

**Dentro:**

- Renombrar `ToolKind.Marker` → `ToolKind.Laser` (el enum existente; `Marker` hoy no está en la barra).
- Nueva herramienta **Láser** en la barra: botón habilitado con `Icon.Laser` (nuevo, estilo Lucide *pointer*, portado a `Resources/Icons.xaml` derivándolo del set existente).
- **Punto/halo** luminoso que sigue al cursor **siempre** que la herramienta láser esté activa (visible aunque el cursor esté quieto), con el color de la **tinta activa** (paleta `Ink.*`) y un halo/glow.
- **Estela** luminosa al **mantener el botón izquierdo y arrastrar**: una traza que se **desvanece sola** en poco tiempo (efímera) con el color de la tinta activa.
- **Efímero por completo:** el láser **no** crea `Stroke`, **no** entra al historial (`AnnotationHistory`/`UndoStack`) y **no** deja nada que deshacer ni limpiar.
- La capa **captura los clics** mientras el láser está activo (no es click-through), igual que el resto de herramientas de dibujo; el dibujo de tinta del `InkCanvas` queda **inhibido** (`EditingMode → None`).
- Activación: **botón de la barra** + entrada en el **anillo** `Ctrl+Alt+Espacio` (`ToolRing`) + **atajo dedicado** `Ctrl+Alt+L`.
- El **acento teal** resalta el botón Láser cuando está activo (igual que las demás herramientas).
- Funciona **multi-monitor**: el punto y la estela se renderizan en la pane del monitor donde está el cursor.
- Cambiar de herramienta o desactivar el overlay **limpia** el punto y la estela por completo.

**Fuera de alcance (specs futuros):**

- Que el punto **desaparezca al quedarse quieto** (modelo alternativo del Gherkin literal) — se decidió punto persistente.
- Estela **al solo mover** el cursor sin presionar — se decidió clic+arrastre.
- **Persistir** o **capturar** la estela en el PNG (HU-12): es efímera y por diseño no debe quedar en la imagen.
- Ajustes de usuario para **duración del desvanecido, longitud de estela o tamaño del halo** (selector propio en la barra).
- **Sonido**, modos "spotlight"/oscurecer el fondo, o lupa.
- Que el láser entre al historial de **deshacer/rehacer** (HU-17): por diseño no aplica.

---

## Modelo de datos

El láser es **efímero y puramente visual**: no entra en `StrokeCollection`, ni en `AnnotationHistory`, ni en el `UndoStack`. La única lógica **pura** (testeable, sin WPF) es la curva de desvanecido; el resto es un *behavior* de Presentation que dibuja sobre una capa propia encima del `InkCanvas`.

**Domain (puro, testeable):**

```csharp
// Renombrado: Marker → Laser en el enum existente.
public enum ToolKind { Select, Pencil, Laser, Eraser, Arrow, Line, Rectangle, Ellipse, Text }

// Mapea la edad de un punto de la estela a su opacidad [0..1]. Pura, sin WPF.
public static class LaserFade
{
    // edad y vida en ms; opacidad 1.0 recién creado → 0.0 al expirar (curva a afinar).
    public static double OpacityForAge(double ageMs, double lifetimeMs);
}
```

- `ToolRing.Order` (existente) pasa a incluir `Laser`. Posición propuesta: **justo tras `Pencil`** →
  `[Pencil, Laser, Line, Arrow, Rectangle, Text, Eraser]`.

**Application:**

- `HotkeyAction` gana `SelectLaserTool` (atajo `Ctrl+Alt+L`).
- `HotkeyBindingTable` (Infrastructure) gana su fila (`VkL`).

**Presentation — capa efímera (sin historial):**

```csharp
// Un punto de la estela: posición en DIP del InkCanvas + marca de tiempo de creación.
internal readonly record struct LaserPoint(Point Position, long CreatedAtTicks);

// Behavior attached (patrón ShapeDrawingBehavior / TextAnnotationBehavior, sin code-behind).
internal sealed class LaserPointerBehavior
{
    // Con la herramienta Laser activa:
    //  - Punto/halo que sigue al cursor SIEMPRE (MouseMove, aunque el botón no esté presionado).
    //  - Botón izq. mantenido + arrastre → acumula LaserPoint en la estela.
    //  - Render por frame (CompositionTarget.Rendering): redibuja punto + estela
    //    aplicando LaserFade.OpacityForAge; descarta puntos expirados.
    //  - Cambio de herramienta / overlay oculto → vacía la estela y el punto.
}
```

- Superficie de dibujo: una capa propia (`Canvas`, p. ej. `LaserLayer`) **encima** del `InkCanvas` en `OverlayWindow.xaml`, gestionada por el behavior. No es un `UIElement` hijo del `InkCanvas` (no debe contaminar `Strokes`/`Children` ni el historial).
- Color: **tinta activa** (`OverlayViewModel.ActiveInkColor`), pintada como halo con `RadialGradientBrush` (centro brillante → borde con glow del color).
- `OverlayViewModel.ToEditingMode(Laser) → None` (lo maneja el behavior; inhibe el dibujo de tinta).

**Convenciones:**

- Coordenadas de la estela en **DIP** del `InkCanvas`.
- El tiempo se mide en Presentation (p. ej. `Stopwatch`/ticks); `LaserFade` recibe la edad ya calculada y devuelve opacidad — **pura y testeable**.
- Una pane = su propia capa de láser **local**; no se difunde por `OverlayBroadcaster` (la selección de herramienta ya se difunde; el render del láser es local al monitor del cursor).

---

## Plan de implementación

1. **Domain — renombrar `Marker` → `Laser`.** Cambiar el valor del enum en `ToolKind.cs` y arreglar referencias rotas (compilación). Ajustar tests existentes que nombren `Marker`. Build/test verdes. *Sin efecto visible aún.*
2. **Domain — `LaserFade`.** Crear `LaserFade.OpacityForAge(ageMs, lifetimeMs)` + tests (opacidad `1.0` en edad 0, `0.0` al alcanzar la vida, clamp fuera de rango, monotonía decreciente). Build/test verdes.
3. **Domain — anillo.** Añadir `Laser` a `ToolRing.Order` en la posición acordada (`[Pencil, Laser, Line, Arrow, Rectangle, Text, Eraser]`). Ajustar tests de `ToolRing`. Verifica: `Ctrl+Alt+Espacio` ya cicla por Láser (aunque todavía no dibuje).
4. **Application/Infrastructure — atajo.** Añadir `HotkeyAction.SelectLaserTool` + fila `Ctrl+Alt+L` (`VkL`) en `HotkeyBindingTable` + enrutado en `HotkeyActionRouter` a `SelectTool(Laser)`. Verifica: `Ctrl+Alt+L` selecciona la herramienta Láser (el botón de la barra se resalta).
5. **Presentation — ícono.** Portar `Icon.Laser` (Lucide *pointer*/*locate*, línea 2px/20px) a `Resources/Icons.xaml` como `StreamGeometry`, coherente con los demás iconos. Verifica: el glyph carga sin error (visible en el paso siguiente).
6. **Presentation — botón de barra.** En `ToolbarViewModel.BuildTools`, añadir el `ToolItem` de `Laser` (`IsEnabled=true`, label *Láser*, `Icon.Laser`). Verifica: el botón Láser aparece, es clicable y se **resalta** con el acento al activarse.
7. **Presentation — inhibir tinta.** En `OverlayViewModel.ToEditingMode`, mapear `Laser → InkCanvasEditingMode.None`. Verifica: con Láser activo, arrastrar **no** dibuja trazos de tinta.
8. **Presentation — capa y punto.** Añadir `LaserLayer` (Canvas) encima del `InkCanvas` en `OverlayWindow.xaml` y crear `LaserPointerBehavior` enganchado ahí (patrón `ShapeDrawingBehavior`). Render del **punto/halo** que sigue al cursor (`MouseMove`) con `RadialGradientBrush` de la tinta activa, solo cuando la herramienta es `Laser`. Verifica: el punto luminoso sigue al cursor y permanece visible en reposo; desaparece al cambiar de herramienta.
9. **Presentation — estela.** En el behavior, acumular `LaserPoint` con botón izq. mantenido + arrastre; redibujar por frame (`CompositionTarget.Rendering`) aplicando `LaserFade.OpacityForAge`; descartar puntos expirados; vaciar la estela al cambiar de herramienta u ocultar el overlay. Verifica: arrastrar deja una estela que se desvanece sola; soltar/parar no deja trazo permanente; nada que deshacer/limpiar.
10. **Presentation — afinado visual.** Ajustar tamaño del halo, intensidad del glow, grosor y vida de la estela (defaults sensatos, ~500 ms) tomando tokens del design system (`Tokens.xaml`); confirmar que el color sigue a la tinta activa. Verifica: el láser se ve de marca y respeta el color activo.
11. **Commit con `/commit`.** Usar el comando `/commit` del repo (mensaje en español, Conventional Commits + emoji), tras dejar `dotnet build -warnaserror` verde (0 warnings) + `dotnet test` verde.

---

## Criterios de aceptación

- [x] Con la herramienta **Láser** activa, aparece un punto/halo luminoso que **sigue al cursor**.
- [x] El punto **permanece visible** en el cursor aunque el cursor esté quieto (mientras la herramienta esté activa).
- [x] El punto y la estela usan el color de la **tinta activa** (con halo/glow).
- [x] Al cambiar el color de tinta, el láser refleja el **nuevo color**.
- [x] **Manteniendo el botón izquierdo y arrastrando** se dibuja una estela luminosa que **sigue el recorrido** del cursor.
- [x] La estela **se desvanece sola** en poco tiempo (efímera), sin acción del usuario.
- [x] El láser **no crea ningún `Stroke`** ni anotación persistente.
- [x] Tras usar el láser **no hay nada que deshacer** (`Ctrl+Alt+Z`) ni que limpiar relacionado con la estela.
- [x] Con el láser activo, arrastrar **no dibuja trazos de tinta** (`EditingMode = None`).
- [x] Al **cambiar de herramienta** o **desactivar el overlay**, el punto y la estela **desaparecen por completo**.
- [x] El botón **Láser** de la barra está habilitado, es clicable y se **resalta** (acento teal) cuando está activo.
- [x] `Ctrl+Alt+Espacio` cicla incluyendo la herramienta Láser, en la posición tras Lápiz.
- [x] `Ctrl+Alt+L` selecciona la herramienta Láser.
- [x] En **multi-monitor**, el punto y la estela se renderizan en el monitor donde está el cursor.
- [x] El renombrado `Marker → Laser` **no rompe** otras herramientas ni atajos existentes (sin regresión).
- [x] `LaserFade.OpacityForAge` tiene tests: `1.0` en edad 0, `0.0` al expirar, clamp fuera de rango, decreciente.
- [x] `dotnet build -warnaserror` verde (0 warnings) y `dotnet test` verde.

---

## Decisiones

- **Sí:** punto/halo **persistente** en el cursor mientras la herramienta esté activa. Es lo que pidió el usuario ("un punto ahí en el cursor, como el lápiz") y es el comportamiento de las herramientas láser que conoce.
- **No:** que el punto **desaparezca al quedarse quieto** (2º escenario del Gherkin literal). Se descartó conscientemente; solo la **estela** se desvanece. El Gherkin se reinterpreta a favor de la decisión del usuario.
- **Sí:** estela solo al **mantener clic + arrastrar**. Coherente con "la capa captura clics" y con "dibujar como un láser"; evita el ruido visual de dejar estela con cada movimiento del mouse.
- **No:** estela **al solo mover** el cursor sin presionar. Generaría rastro constante e involuntario al desplazar el mouse.
- **Sí:** láser **totalmente efímero** — sin `Stroke`, sin `AnnotationHistory`, sin `UndoStack`. Lo exige el Gherkin ("no queda nada que deshacer") y lo marca el backlog.
- **Sí:** **capa propia** (`LaserLayer`, Canvas) encima del `InkCanvas`, gestionada por el behavior. Mantiene la estela fuera de `Strokes`/`Children` y del historial; no contamina captura PNG ni deshacer.
- **No:** modelar la estela como `Stroke` de preview (patrón de `ShapeDrawingBehavior`). Un `Stroke` vive en `StrokeCollection` y arrastraría hit-testing, borrador e historial; justo lo contrario de lo efímero.
- **Sí:** **renombrar** `ToolKind.Marker → Laser` en vez de añadir un valor nuevo. `Marker` no estaba en la barra ni en uso real; el backlog dice explícitamente "Reemplaza al antiguo resaltador".
- **Sí:** `LaserFade` **pura en Domain** + tests; el resto (timing, render) en Presentation. Sigue el patrón de `FontScale` (SPEC 01) y respeta la regla de capas (sin WPF en Domain).
- **Sí:** render por frame con `CompositionTarget.Rendering`. Da desvanecido fluido sincronizado al refresco; el proyecto aún no usaba timers/animación, así que se introduce aquí de forma contenida.
- **No:** `DispatcherTimer` para el fade. `CompositionTarget.Rendering` es más fluido y se autodetiene al limpiar la capa; el timer añadiría jitter y gestión de ciclo de vida extra.
- **Sí:** Láser entra al **anillo** (tras Lápiz) + atajo `Ctrl+Alt+L` + botón de barra. Coherente con cómo se integró Texto en SPEC 01.
- **No:** difundir la estela por `OverlayBroadcaster`. El render es local al monitor del cursor; solo la **selección** de herramienta se difunde (ya existe).
- **No:** incluir la estela en el **PNG de captura** (HU-12). Es efímera por diseño; capturarla contradeciría "no deja marca".
- **No:** selector de **duración/tamaño/longitud** en la barra. Amplía la UI sin necesidad; defaults afinados con el design system bastan para esta HU.
- **No:** modos extra (spotlight, oscurecer fondo, lupa, sonido). Fuera de alcance; cada uno sería su propia HU.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Renombrar `Marker → Laser` rompe referencias (switches, mapeos, tests) y podría regresionar otras herramientas. | Paso 1 aislado: compilar tras el rename, arreglar todas las referencias y dejar build/test verdes **antes** de añadir comportamiento. |
| `CompositionTarget.Rendering` se ejecuta cada frame; si no se desuscribe al ocultar el overlay o cambiar de herramienta, **fuga de memoria/CPU** (rompe el DoD #5). | Suscribir solo mientras la herramienta es `Laser` y hay overlay visible; desuscribir y vaciar la capa al cambiar de herramienta/ocultar. Probar activar/desactivar repetidas veces. |
| La capa `LaserLayer` encima del `InkCanvas` podría **interceptar** los eventos de mouse y romper el dibujo de otras herramientas. | La capa es `IsHitTestVisible=false`; el behavior escucha los eventos en el `InkCanvas`, no en la capa de render. |
| Desfase del punto/estela respecto al cursor en **monitores con DPI mixto** (problema histórico, HU-09). | Trabajar en DIP del `InkCanvas` (igual que trazos/texto); reutilizar el pipeline de coordenadas ya probado en multi-monitor. |
| `LaserPointerBehavior` puede crecer y superar límites SonarLint (S104 ≤400 líneas, S138 ≤40/método, S1541 complejidad ≤10). | Extraer el render del halo y de la estela a métodos/tipos auxiliares; mantener `LaserFade` (pura) separada. |
| La palabra española *"todo"* en comentarios dispara **S1135**. | Evitarla en comentarios C# (gotcha conocido del repo). |
| El glow/halo a pantalla completa cada frame podría **costar rendimiento** sobre fondos pesados. | Limitar el nº de puntos de la estela (vida corta ~500 ms) y dibujar solo el halo + segmentos vivos; afinar en el paso 10. |

---

## Lo que **no** está en este spec

- Que el punto desaparezca al quedarse quieto (modelo alternativo del Gherkin) — se decidió punto persistente.
- Estela al solo mover el cursor sin presionar — se decidió clic+arrastre.
- Persistir o capturar la estela en el **PNG** (HU-12).
- Selector de duración/tamaño/longitud de la estela en la barra.
- Sonido, modos spotlight/oscurecer fondo, lupa.
- Entrada del láser en el historial de **deshacer/rehacer** (HU-17): por diseño no aplica.

Cada uno, si llega, va en su propio spec.
