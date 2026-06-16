# SPEC 01 — Anotar con texto (HU-13)

> **Estado:** Implementado · **Depende de:** — (sobre el código ya implementado HU-01…HU-12) · **Fecha:** 2026-06-15
> **Objetivo:** Permitir escribir cuadros de texto sobre el overlay, fijos en pantalla, que usan la tinta activa, derivan su tamaño del grosor y se quitan con deshacer/limpiar igual que un trazo.

---

## Alcance

**Dentro:**

- Nueva herramienta **Texto** (`ToolKind.Text`, ya en el enum): al elegirla y hacer clic en el overlay aparece un cuadro de edición en ese punto.
- Cuadro de texto **multilínea** (`Enter` = salto de línea); se **fija** al hacer clic fuera o pulsar `Esc`.
- Cuadro **vacío al confirmar → se descarta** (no deja anotación ni entrada en el historial).
- El texto usa la **tinta activa** (paleta `Ink.*`) como color y deriva su **tamaño de fuente del grosor activo** (escala `[3,5,8,12,18]`); la **tipografía (familia/peso) sale del design system** (`/acetato-design`).
- **Deshacer unificado y cronológico** por pantalla: `Ctrl+Alt+Z` (y botón de la barra) quita la última anotación, sea texto o trazo.
- **Limpiar** (`Ctrl+Alt+Retroceso` / botón) borra también el texto, en todas las pantallas.
- Activación: habilitar el **botón Texto de la barra** (hoy placeholder), sumar Texto al **anillo** `Ctrl+Alt+Espacio` y añadir un **atajo dedicado** `Ctrl+Alt+T`.
- Funciona **multi-monitor**: el texto se coloca en la pane del monitor donde se hace clic.
- El texto aparece en el **PNG de captura** (HU-12) automáticamente, por estar compuesto en pantalla.

**Fuera de alcance (specs futuros):**

- **Re-editar, mover o redimensionar** un texto ya fijado → HU-14 (Seleccionar).
- **Borrar texto individual con el borrador** (EraseByStroke solo opera sobre trazos) → vía deshacer/limpiar, o HU-14.
- **Rehacer** el texto deshecho → HU-17.
- **Persistir** el texto entre sesiones (serialización/versionado).
- Selector de **tamaño de fuente propio** en la barra (se deriva del grosor).
- Formato de texto enriquecido (negrita, cursiva, alineación, viñetas).

---

## Modelo de datos

El texto **no** cabe en `StrokeCollection` (solo guarda `Stroke`). Se modela como `UIElement` hijo del `InkCanvas` (su propiedad `Children`), y el *undo* pasa de "quitar el último `Stroke`" a una **línea de tiempo unificada** por pane.

**Domain (puro, testeable):**

```csharp
// Mapea el grosor activo a un tamaño de fuente. Pura, sin WPF.
public static class FontScale
{
    // Índice de grosor [0..4] → puntos de fuente. Valores a afinar con el design system.
    public static double FromThicknessIndex(int index); // p. ej. [16, 22, 30, 42, 60]
}
```

- `ToolRing.Order` (existente) pasa a incluir `Text`:
  `[Pencil, Line, Arrow, Rectangle, Text, Eraser]` (orden a confirmar en el plan).

**Application:**

- `HotkeyAction` gana `SelectTextTool` (atajo `Ctrl+Alt+T`).
- `HotkeyBindingTable` (Infrastructure) gana su fila (`VkT`).

**Presentation — historial unificado (reemplaza `UndoStack`):**

```csharp
// Una entrada deshacible por pane, en orden cronológico.
internal interface IAnnotation
{
    void Remove(); // trazo → Strokes.Remove; texto → Children.Remove
}

// Reemplaza UndoStack: registra trazos Y textos en el orden en que se crean.
internal sealed class AnnotationHistory
{
    public void RecordStroke(Stroke stroke);
    public void RecordText(UIElement textElement);
    public void UndoLast();   // quita la última anotación, sea del tipo que sea
    public void Clear();      // vacía trazos, textos e historial
}
```

- **`TextAnnotationBehavior`** (attached, sin code-behind, patrón `ShapeDrawingBehavior`): con la herramienta `Text`, en clic crea un `TextBox` multilínea hijo del `InkCanvas` posicionado en el punto, le da foco; al confirmar (clic fuera / `Esc`) lo reemplaza por un `TextBlock` fijo y lo registra en `AnnotationHistory` (si tiene contenido).
- `OverlayViewModel`: `ToEditingMode(Text) → None` (lo maneja el behavior); expone los atributos de texto (color de la tinta activa + tamaño vía `FontScale`) y la familia/peso del design system.

**Convenciones:**

- Color del texto = tinta activa (`TintaColorMap.Resolve`), igual que los trazos.
- Coordenadas del texto en DIP del `InkCanvas` (`InkCanvas.SetLeft/SetTop`).
- Una pane = un `AnnotationHistory` (trazos + texto propios), como hoy con los trazos.

Punto delicado: `ShapeDrawingBehavior` añade/quita un `Stroke` de *preview* durante el arrastre; el `AnnotationHistory` debe registrar solo el trazo **final**, no el churn del preview (ver Riesgos).

---

## Plan de implementación

1. **Domain — `FontScale`.** Crear `FontScale.FromThicknessIndex` + tests (mapea cada índice `[0..4]` a su tamaño; clamp fuera de rango). Build/test verdes. *Sin efecto visible aún.*
2. **Domain — anillo.** Añadir `Text` a `ToolRing.Order` en su posición. Ajustar/añadir tests de `ToolRing`. Verifica: `Ctrl+Alt+Espacio` ya cicla por Texto (aunque todavía no dibuje).
3. **Application/Infrastructure — atajo.** Añadir `HotkeyAction.SelectTextTool` + fila `Ctrl+Alt+T` en `HotkeyBindingTable` + enrutado en `HotkeyActionRouter` a `SelectTool(Text)`. Verifica: el atajo selecciona la herramienta Texto (el botón de la barra se resalta).
4. **Presentation — habilitar la barra.** En `ToolbarViewModel.BuildTools`, pasar `Text` a `IsEnabled=true` (icono `Icon.Text` ya existe). Verifica: el botón Texto es clicable y se resalta como activo.
5. **Presentation — historial unificado.** Introducir `IAnnotation` + `AnnotationHistory` reemplazando `UndoStack`; los trazos se registran (vía `StrokesChanged`, ignorando el preview de formas) y `UndoLast`/`Clear` operan sobre la línea de tiempo. Re-cablear `OverlayViewModel.Undo/Clear`. Tests del historial (orden, deshacer mixto, clear). Verifica: deshacer/limpiar de trazos sigue idéntico (sin regresión HU-04/07/11).
6. **Presentation — `TextAnnotationBehavior`.** Crear el behavior attached: clic con herramienta Texto → `TextBox` multilínea hijo enfocado en el punto; `Esc`/clic-fuera fija a `TextBlock` (descarta si vacío) y lo registra en `AnnotationHistory`. Color (tinta activa) + tamaño (`FontScale` del grosor) + familia/peso (design system). Enganchar el behavior en `OverlayWindow.xaml` (como `ShapeDrawingBehavior`). `ToEditingMode(Text) → None`. Verifica: escribir, fijar, deshacer y limpiar texto.
7. **Tipografía de marca.** Consultar `/acetato-design` y portar familia/peso/estilo del texto a `Resources/` (token o estilo del `TextBlock`/`TextBox`). Verifica: el texto se ve con la fuente de marca y la tinta activa.
8. **Commit con `/commit`.** Usar el comando `/commit` del repo (genera el mensaje en español, Conventional Commits + emoji), tras dejar build `-warnaserror` verde + `dotnet test` verde.

---

## Criterios de aceptación

- [x] Con la herramienta Texto activa, hacer clic en el overlay muestra un cursor de edición en ese punto.
- [x] Al escribir, el texto aparece en el punto del clic con la **tinta activa** como color.
- [x] `Enter` inserta un **salto de línea** dentro del cuadro (multilínea).
- [x] Hacer **clic fuera** del cuadro fija el texto en pantalla en esa posición.
- [x] Pulsar **`Esc`** fija el texto en pantalla en esa posición.
- [x] Confirmar un cuadro **vacío** no deja ninguna anotación ni entrada en el historial.
- [x] El **tamaño de fuente** del texto refleja el grosor activo (mayor grosor → fuente mayor).
- [x] La **tipografía** (familia/peso) corresponde al design system de marca.
- [x] Un texto nuevo usa el color activo; los textos **anteriores conservan** su color original.
- [x] **Deshacer** (`Ctrl+Alt+Z` o botón) quita la última anotación hecha, sea **texto o trazo**, en orden cronológico.
- [x] **Limpiar todo** (`Ctrl+Alt+Retroceso` o botón) elimina los textos junto con los trazos, en todas las pantallas.
- [x] El botón **Texto** de la barra está habilitado, es clicable y se **resalta** cuando está activo.
- [x] `Ctrl+Alt+Espacio` cicla incluyendo la herramienta Texto.
- [x] `Ctrl+Alt+T` selecciona la herramienta Texto.
- [x] En **multi-monitor**, el texto se coloca en el monitor donde se hizo clic.
- [x] El **PNG de captura** (HU-12) incluye el texto visible en pantalla.
- [x] Deshacer/limpiar de **trazos** sigue funcionando igual que antes (sin regresión HU-04/07/11).
- [x] `dotnet build -warnaserror` verde (0 warnings) y `dotnet test` verde.

---

## Decisiones

- **Sí:** texto como `UIElement` hijo del `InkCanvas` (`Children`). El `InkCanvas` ya admite hijos posicionados y puede recibir foco de teclado (la barra no, por `WS_EX_NOACTIVATE`).
- **No:** rasterizar el texto a un `Stroke`. Imposible limpio; un `Stroke` es una colección de puntos, no soporta glifos.
- **Sí:** historial unificado cronológico (`AnnotationHistory`) que reemplaza `UndoStack`. Deshacer "lo último, sea lo que sea" es el contrato intuitivo y lo exige el Gherkin.
- **No:** dos pilas separadas (trazos vs texto). Deshacer "saltaría" anotaciones — comportamiento sorprendente.
- **Sí:** tamaño de fuente derivado del grosor activo. Reutiliza el ajuste y la rueda del mouse; sin control nuevo. Lo sugiere el backlog.
- **No:** selector de tamaño propio en la barra. Amplía la UI sin necesidad para esta HU.
- **Sí:** multilínea con `Enter` = salto y confirmación por clic-fuera/`Esc`. Coincide con el Gherkin ("clic fuera o Esc").
- **Sí:** texto inmutable tras fijarse. Mover/editar/redimensionar es HU-14 (encaja con `InkCanvasEditingMode.Select`).
- **No:** borrador sobre texto. `EraseByStroke` solo opera sobre `Stroke`; añadir hit-testing del `UIElement` amplía alcance → HU-14.
- **Sí:** tipografía desde el design system. Coherencia de marca; el usuario no es diseñador y `/acetato-design` es la referencia.
- **No:** persistir el texto entre sesiones. Igual que los trazos hoy (solo en memoria); serializar/versionar sería otra HU.
- **Sí:** Texto entra al anillo + atajo `Ctrl+Alt+T` + botón de barra. Coherente con las demás herramientas.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| El preview de `ShapeDrawingBehavior` añade/quita un `Stroke` repetidamente; `AnnotationHistory` podría registrar ruido del preview. | Registrar solo trazos **finales**: escuchar `StrokesChanged` sincronizando añadidos/quitados, o registrar al fijar (no en cada move). Verificar que tras dibujar una forma queda **una** entrada. |
| Refactor de `UndoStack → AnnotationHistory` puede regresionar deshacer/limpiar de trazos (HU-04/07/11). | Paso 5 aislado y verificado antes de tocar texto; mantener los tests de trazos verdes. |
| El overlay debe tomar **foco de teclado** para escribir; podría chocar con el modo dibujo o robar foco a la app de abajo. | El overlay ya es activable (no tiene `WS_EX_NOACTIVATE`). Dar foco solo al `TextBox` al colocarlo; soltarlo al fijar. |
| Atajos `Ctrl+Alt+*` mientras se escribe (p. ej. `Ctrl+Alt+Z`, colores). | Son combos globales con `Ctrl+Alt`, no teclas sueltas; no interfieren con la escritura normal. Verificar que no disparan acción inesperada con el `TextBox` enfocado. |
| Límites SonarLint: `TextAnnotationBehavior` y `AnnotationHistory` pueden crecer (S104 ≤400 líneas, S138 ≤40/método). | Extraer a tipos/archivos separados si se acercan al límite (patrón ya usado). |
| La palabra española *"todo"* en comentarios dispara **S1135**. | Evitarla en comentarios C# (gotcha ya conocido del repo). |

---

## Lo que **no** está en este spec

- Re-editar, mover o redimensionar un texto ya fijado → **HU-14** (Seleccionar).
- Borrar texto individual con el borrador → vía deshacer/limpiar, o **HU-14**.
- Rehacer el texto deshecho → **HU-17**.
- Persistir el texto entre sesiones (serialización/versionado).
- Selector de tamaño de fuente propio en la barra.
- Texto enriquecido (negrita, cursiva, alineación, viñetas).

Cada uno, si llega, va en su propio spec.
