# SPEC 03 — Trazo estilo ScreenBrush (HU-19)

> **Estado:** Aprobado · **Depende de:** SPEC 01 (historial unificado), SPEC 02 (láser) — sobre el código ya implementado HU-01…HU-15 · **Fecha:** 2026-10-06
> **Objetivo:** Dar a los trazos el estilo de ScreenBrush: flechas a mano alzada con punta, rectángulos de esquinas redondeadas y un modo "Degradado" que pinta cada trazo con un par de colores neón al azar.

---

## Alcance

**Dentro:**

- **Flecha a mano alzada:** la herramienta Flecha (mismo lugar en barra y anillo `Ctrl+Alt+Espacio`) pasa a trazo libre suavizado, como el lápiz.
  - Al soltar se añade una **punta abierta de dos alas**, orientada según el **tramo final** del recorrido (no el último punto, para evitar el temblor).
  - El tamaño de la punta **crece con el grosor** activo.
  - Si el recorrido es **más corto que la punta**, se descarta (no deja anotación ni entrada en el historial).
  - La flecha completa (cuerpo + punta) es **un solo trazo**: un deshacer la quita entera; el borrador la borra entera.
- **Rectángulo con esquinas redondeadas:** sigue dibujándose por arrastre; radio que **crece con el grosor**, limitado a la mitad del lado menor.
- **Línea:** sigue recta (solo recibe el degradado).
- **Modo Degradado:**
  - Se activa con una **muestra redonda con degradado** en el popover de color (después de las 6 tintas) y con el atajo **`Ctrl+Alt+7`**.
  - Mientras está activo, el **botón Color** de la barra muestra el degradado.
  - Elegir una tinta sólida (popover o `Ctrl+Alt+1…6`) **sale** del modo.
- **6 pares neón:** azul→cian, amarillo→verde lima, rosa→rojo, violeta→rosa, verde→cian, naranja→rosa. Viven como tokens de **tinta** en `Tokens.xaml` (paleta `Ink.*`, nunca `Chrome.*`).
- Cada trazo toma un par **al azar, sin repetir el del trazo anterior**, y **conserva su par** para siempre (cambiar de modo después no lo altera).
- **Dirección del degradado:** lineal, del **punto inicial al punto final** del trazo. En figuras cerradas (inicio ≈ fin, como el rectángulo) corre en **diagonal** del recuadro que las encierra.
- **Aplica a:** lápiz, línea, flecha y rectángulo. **Texto y láser** usan la **última tinta sólida** elegida.
- **Vista en vivo:** línea y rectángulo muestran el degradado mientras se arrastran. Lápiz y flecha libre se ven en el **primer color del par** mientras se trazan y pasan a degradado **al soltar**.
- Funciona **multi-monitor**. El degradado aparece en el **PNG de captura** (HU-12) sin cambios en la captura.
- Deshacer, limpiar y borrador funcionan igual con los trazos nuevos.
- Alta de **HU-19** en `BACKLOG.md`.

**Fuera de alcance (specs futuros):**

- La **flecha recta por arrastre** actual **se reemplaza** (una flecha recta se hace trazando recto).
- Degradado **siguiendo el recorrido** del trazo.
- Degradado **en vivo** para lápiz y flecha libre.
- **Punta de flecha en vivo** (que aparezca mientras se traza, no al soltar). Candidata al **spec 04** junto con el degradado en vivo: ambas exigen un pintado propio del trazo en curso.
- Degradado en **texto** y **láser**.
- **Elegir o editar** los pares de colores desde la app.
- **Recordar** el modo degradado entre sesiones.
- **Reconocimiento de formas** (convertir un garabato en figura limpia).
- **Brillo/halo** neón en los trazos.
- Flecha con **punta en ambos extremos**.
- Elipse (HU-16), rehacer (HU-17), círculo/cuadrado perfecto con Shift.

---

## Modelo de datos

### Domain (puro, sin WPF, testeable)

```csharp
// Los 6 pares neón. Los hex viven en el design system (Tokens.xaml), igual que TintaColor.
public enum TintaGradiente { BlueCyan, YellowLime, PinkRed, VioletPink, GreenCyan, OrangePink }

// Fuente de enteros al azar inyectable (puerto). Producción: SecureRandomSource (RandomNumberGenerator).
public interface IRandomSource { int NextInt(int maxExclusive); }

// Elige el siguiente par al azar, siempre distinto del actual. Fuente inyectada → tests deterministas.
public static class GradientPicker
{
    public static TintaGradiente Next(TintaGradiente current, IRandomSource random);
}

// Punta abierta de la flecha libre: ala izquierda → vértice → ala derecha.
public readonly record struct ArrowHead(StrokePoint Left, StrokePoint Tip, StrokePoint Right);

public static class ArrowHeadBuilder
{
    // false si el recorrido es más corto que la punta (la flecha se descarta).
    public static bool TryBuild(IReadOnlyList<StrokePoint> path, double thickness, out ArrowHead head);
}
```

- **Largo de punta:** `max(16, 3 × grosor)` DIP. **Ancho de ala:** 0,6 × largo.
- **Dirección:** vector desde el primer punto del **tramo final** hasta el último punto. El tramo final son los últimos `1,5 × largo de punta` DIP del recorrido.
- **`ShapeBuilder.Build`** gana un 4.º parámetro `double thickness` y pierde el caso `Arrow`.
- **Rectángulo:** **radio** = `min(max(8, 2 × grosor), lado menor / 2)`. Cada esquina se aproxima con 6 segmentos; la figura sigue cerrada.
- Los números son **valores iniciales**, ajustables a ojo al probar. Los tests verifican las reglas (tope, descarte), no las cifras exactas.

### Application

`IDrawingSettings` / `DrawingSettings` ganan (mismo patrón que color/grosor, notifican por `Changed`):

```csharp
bool IsGradient { get; }                  // modo Degradado activo
TintaGradiente NextGradient { get; }      // par que tomará el PRÓXIMO trazo
void SelectGradient();                    // entra al modo (Ctrl+Alt+7 / muestra del popover)
void AdvanceGradient();                   // tras fijar un trazo: sortea un par nuevo
```

- **`SelectColor`** además pone `IsGradient = false`. **`Color`** conserva siempre la **última tinta sólida**.
- **`DrawingSettings`** recibe un `IRandomSource` por constructor (por defecto `SecureRandomSource.Instance`; los tests pasan una secuencia fija).
- **Instancia única compartida:** el sorteo no se repite aunque se dibuje en monitores distintos.
- Nuevo **`HotkeyAction.ColorGradient`**.

### Presentation

```csharp
// Trazo propio: conserva su par y, si es flecha, su punta. Override de DrawCore, Clone y GetBounds.
internal sealed class StyledStroke : Stroke
{
    public TintaGradiente? Gradient { get; }  // null = tinta sólida (DrawingAttributes.Color)
    public ArrowHead? Head { get; }           // null = no es flecha
}
```

- **`TintaGradienteMap`** (como `TintaColorMap`): resuelve cada par a sus dos `Color` (inicio/fin) desde los tokens.
- **Tokens nuevos en `Tokens.xaml`:** `Ink.Gradient.<Par>.Start.Color` / `Ink.Gradient.<Par>.End.Color` (12 colores).
- **`OverlayViewModel.SolidInkColor`:** lo leen **texto** y **láser**.
- **`ActiveAttributes.Color`:** con el degradado activo pasa a ser el **primer color de `NextGradient`** (vista en vivo). Por eso texto y láser dejan de leerlo y leen `SolidInkColor`.
- **`InkSwatchItem`** admite un ítem de degradado (con su pincel de muestra) para el popover.

**Convención:** el degradado se pinta en coordenadas del lienzo (`MappingMode` absoluto), del primer al último punto. Si inicio y fin distan menos de `2 × grosor`, el trazo se trata como figura cerrada: el degradado va de la esquina superior izquierda a la inferior derecha de su recuadro.

---

## Plan de implementación

Cada paso compila con `-warnaserror`, deja la app usable y es commiteable por sí solo.

1. **Backlog.** Añadir **HU-19 — Trazo estilo ScreenBrush** a `BACKLOG.md` (narrativa, Gherkin, referencia a este spec). Solo docs.
2. **Pares de degradado (Domain).** `TintaGradiente` + `IRandomSource` / `SecureRandomSource` + `GradientPicker.Next(current, random)`. Tests: con secuencia fija nunca devuelve el actual; en 100 sorteos aparecen los 6. Sin cambio visible.
3. **Punta de flecha (Domain).** `ArrowHead` + `ArrowHeadBuilder.TryBuild`. Tests:
   - Un recorrido recto apunta en su dirección.
   - Un recorrido en curva apunta según el tramo final.
   - Un recorrido más corto que la punta devuelve `false`.
   - La punta crece con el grosor.

   Sin cambio visible.
4. **Rectángulo redondeado (Domain + behavior).** `ShapeBuilder.Build` recibe `thickness` y genera esquinas con arco; `ShapeDrawingBehavior` le pasa el grosor activo. Tests: cerrado, radio crece con el grosor, respeta el tope de la mitad del lado menor. **Manual:** el rectángulo sale con esquinas redondeadas.
5. **Trazo propio con punta (Presentation).** `StyledStroke` (solo sólido por ahora) con override de `DrawCore` (cuerpo con el estilo normal + **punta** como polilínea de extremos y uniones redondeados), `Clone` y `GetBounds` (incluye la punta). Sin uso aún.
6. **Flecha a mano alzada.**
   - `ToEditingMode` mapea `Arrow` → `Ink`.
   - Se quita `Arrow` de `ShapeBuilder` y de `IsShapeTool`.
   - Nuevo `FreehandStrokeBehavior` (adjunto, sin code-behind) escucha `InkCanvas.StrokeCollected` con herramienta Flecha:
     - Si `TryBuild` da `false`, quita el trazo (nada queda en el historial).
     - Si no, lo **reemplaza** por un `StyledStroke` con punta (baja + alta = una sola entrada en `AnnotationHistory`).

   **Manual:** flecha curva con punta orientada; un deshacer la quita entera; un clic suelto no deja nada.
7. **Tokens y mapa.** Los 12 colores `Ink.Gradient.<Par>.Start/End.Color` en `Tokens.xaml` + `TintaGradienteMap`. Sin cambio visible.
8. **Modo degradado (Application + atajo).**
   - `IsGradient` / `NextGradient` / `SelectGradient` / `AdvanceGradient` en `IDrawingSettings` y `DrawingSettings` (con `IRandomSource` inyectado).
   - `SelectColor` apaga el modo.
   - `HotkeyAction.ColorGradient`, `Vk7` en `NativeMethods`, fila `Ctrl+Alt+7` en `HotkeyBindingTable`, ruta en `HotkeyActionRouter`.

   Tests de `DrawingSettings`: entra/sale del modo, `Changed` solo si cambia, `AdvanceGradient` nunca repite. **Manual:** `Ctrl+Alt+7` no rompe nada (trazos aún sólidos).
9. **Color en vivo vs. texto/láser.**
   - `OverlayViewModel` expone `SolidInkColor`; `TextAnnotationBehavior` y `LaserPointerBehavior` pasan a leerlo.
   - Con `IsGradient`, `ActiveAttributes.Color` = primer color de `NextGradient`.

   **Manual:** en modo degradado el lápiz dibuja en el primer color del par; texto y láser usan la última tinta sólida.
10. **Pintar el degradado.**
    - `StyledStroke.DrawCore` pinta con `LinearGradientBrush` absoluto (primer → último punto, o diagonal del recuadro si es figura cerrada), con pinceles congelados y cacheados por par.
    - El VM expone `ActiveGradient` (null en modo sólido) y `GradientUsedCommand` (llama `AdvanceGradient`); los behaviors los reciben por attached properties.
    - `ShapeDrawingBehavior` crea `StyledStroke` con degradado en el preview y avanza el par al soltar.
    - `FreehandStrokeBehavior` reemplaza **también los trazos del lápiz** por `StyledStroke` con degradado (como la flecha) y avanza el par.

    **Manual:** cada trazo sale en degradado y con par distinto al anterior.
11. **Barra.**
    - Popover de color: muestra de degradado después de las 6 tintas (ítem de `InkSwatchItem`, anillo de acento si activa).
    - **Botón Color:** muestra el pincel de degradado cuando `IsGradient`.

    **Manual:** elegir la muestra entra al modo; elegir una tinta lo apaga.

---

## Criterios de aceptación

**Flecha a mano alzada**

- [ ] Con la herramienta Flecha, trazar una curva deja una flecha curva y suavizada con punta abierta de dos alas en el extremo final.
- [ ] La punta apunta en la dirección del tramo final, aunque el recorrido sea curvo o termine con un temblor leve.
- [ ] Con un grosor mayor, la punta sale más grande.
- [ ] Un clic suelto o un trazo más corto que la punta no deja nada en pantalla, y un `Ctrl+Alt+Z` posterior quita la anotación anterior.
- [ ] Un solo `Ctrl+Alt+Z` quita la flecha entera (cuerpo y punta).
- [ ] El borrador borra la flecha entera de una pasada.
- [ ] La Flecha sigue en la barra y en el ciclo `Ctrl+Alt+Espacio`, en la misma posición.

**Rectángulo y línea**

- [ ] El rectángulo sale con las esquinas redondeadas al arrastrar en cualquier dirección.
- [ ] Con un grosor mayor, las esquinas se ven más redondeadas.
- [ ] Un rectángulo muy pequeño no se deforma: el radio nunca pasa de la mitad del lado menor.
- [ ] La línea sigue saliendo recta.

**Modo degradado**

- [ ] `Ctrl+Alt+7` y la muestra de degradado del popover activan el modo.
- [ ] Con el modo activo, la muestra tiene el anillo de acento y el botón Color de la barra muestra el degradado.
- [ ] Elegir una tinta sólida (popover o `Ctrl+Alt+1…6`) apaga el modo y los trazos siguientes salen sólidos.
- [ ] Con el modo activo, lápiz, línea, flecha y rectángulo salen en degradado.
- [ ] Dos trazos seguidos nunca salen con el mismo par.
- [ ] El degradado de una línea o flecha va del color inicial (donde empezó) al final (donde terminó).
- [ ] El degradado de un rectángulo corre en diagonal.
- [ ] Línea y rectángulo muestran el degradado mientras se arrastran.
- [ ] Lápiz y flecha libre se ven en el primer color del par mientras se trazan y quedan en degradado al soltar.
- [ ] Al salir del modo, los trazos ya hechos en degradado no cambian.
- [ ] Con el modo activo, el texto y el láser usan la última tinta sólida elegida.
- [ ] Deshacer, limpiar y borrador funcionan igual con trazos en degradado.

**Integración**

- [ ] Con dos monitores, todo lo anterior funciona en ambos y el par no se repite al alternar de monitor.
- [ ] El PNG de `Ctrl+Alt+S` incluye flechas curvas, esquinas redondeadas y degradados tal como se ven.
- [ ] Dibujar 50 trazos en degradado no produce lentitud perceptible al trazar.
- [ ] Activar/desactivar el overlay 20 veces con trazos en degradado no deja errores ni crecimiento sostenido de memoria.
- [ ] `dotnet build -warnaserror` verde, sin suprimir reglas.
- [ ] `dotnet test` verde, con tests nuevos de `GradientPicker`, `ArrowHeadBuilder`, `ShapeBuilder` y `DrawingSettings`.
- [ ] `BACKLOG.md` incluye HU-19.

---

## Decisiones

- **Sí:** un solo spec para flechas, esquinas y degradado. Decisión del usuario: las tres partes son un mismo cambio de estilo.
- **No:** dividir en 03 (formas) y 04 (degradado). Se propuso para aislar el riesgo del degradado; se mitiga con pasos commiteables por separado.
- **Sí:** flecha a mano alzada con punta añadida al soltar. Es como funciona ScreenBrush y lo que muestran las referencias.
- **No:** flecha curva con punto de control. Más precisa, pero mucho más compleja de usar y de construir.
- **Sí:** la flecha libre reemplaza a la recta. Una recta se logra trazando recto; evita sumar otra herramienta a la barra (principio cero distracción).
- **Sí:** dirección de la punta según el tramo final, no el último punto. El último tramo suele tener temblor y torcería la punta.
- **Sí:** descartar el trazo más corto que la punta. Un clic accidental no deja basura ni una entrada inútil en el historial.
- **Sí:** punta y radio de esquinas proporcionales al grosor. Un valor fijo se vería desproporcionado con trazos gruesos o finos.
- **Sí:** línea recta y rectángulo por arrastre se mantienen. Las referencias los muestran así y conserva lo que ya funciona.
- **Sí:** el degradado es una opción más del popover de color; las tintas sólidas se mantienen. No rompe el uso actual y respeta la paleta.
- **No:** degradado siempre activo. Quita el control del color sólido, útil para señalar con precisión.
- **No:** degradado derivado de la tinta activa. No se parece a las referencias, que alternan pares contrastantes.
- **Sí:** 6 pares neón fijos, al azar sin repetir el anterior. Repetir haría que dos trazos vecinos se confundan.
- **Sí:** pares como tokens de tinta en `Tokens.xaml`. La regla "sin degradados" del design system aplica al chrome, no a los trazos del usuario; las paletas siguen separadas.
- **Sí:** degradado lineal inicio → fin, diagonal en figuras cerradas. Simple, rápido de pintar y visualmente cercano a las referencias.
- **No:** degradado siguiendo el recorrido. Exige pintar tramo por tramo, con riesgo de rendimiento, para una mejora visual menor.
- **Sí:** lápiz y flecha libre en el primer color mientras se trazan, degradado al soltar. La pintura en vivo de Windows solo admite un color.
- **No:** punta de flecha en vivo en este spec. El usuario la quiere (como ScreenBrush) pero exige el mismo pintado propio del trazo en curso que el degradado en vivo; se agrupa en el spec 04. Decisión del usuario al probar el paso 10.
- **No:** degradado en vivo con un renderizador propio. Riesgo de rendimiento, y el color final cambiaría mientras se dibuja.
- **Sí:** texto y láser con la última tinta sólida. El texto con degradado requiere otro tipo de pintado; el láser es efímero.
- **Sí:** trazo propio (`StyledStroke`) que guarda par y punta, reemplazando al que entrega el lienzo al soltar. Única forma de pintar degradado y una punta no suavizada sin tocar historial ni borrador.
- **No:** añadir la punta como puntos del mismo trazo. El suavizado del lápiz la deformaría y redondearía el vértice.
- **Sí:** el sorteo vive en `DrawingSettings` (instancia compartida). No repite entre monitores y reutiliza el aviso `Changed` existente.
- **Sí:** fuente de azar inyectada mediante `IRandomSource` (no `Random`). El analizador CA5394 rechaza `Random` y la regla es no suprimir; `SecureRandomSource` usa `RandomNumberGenerator`, sin supresiones. Tests deterministas con secuencia fija. Acordado con el usuario en el paso 2.
- **No:** recordar el modo entre sesiones. Hoy nada persiste (ni color ni grosor); sería un cambio aparte.
- **Sí:** atajo `Ctrl+Alt+7`. Sigue a los de colores (`Ctrl+Alt+1…6`) y respeta la convención de solo Ctrl+Alt.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Reemplazar el trazo al soltar deja dos entradas, o ninguna, en `AnnotationHistory`. | El historial ya sincroniza bajas y altas, así que el neto es una entrada. Se verifica a mano en el paso 6 (un deshacer quita la flecha entera). |
| Quitar el trazo dentro de `StrokeCollected` choca con el lienzo, que aún lo está procesando. | Patrón habitual en WPF. Si da problemas, posponer el reemplazo con `Dispatcher.BeginInvoke`. |
| La punta se pinta fuera del recuadro calculado del trazo y deja restos al borrar o deshacer. | Override de `GetBounds` en `StyledStroke` para incluir la punta. |
| El borrador solo detecta el cuerpo: pasar solo por la punta no borra la flecha. | Limitación aceptada (tocar el cuerpo la borra entera). Documentada. |
| Muchos trazos en degradado ralentizan el pintado. | Pinceles congelados (`Freeze`) y cacheados por par. Criterio de aceptación de 50 trazos. |
| `Clone` mal implementado pierde par y punta cuando WPF copia el trazo (afectaría a HU-14). | Override de `Clone` desde el paso 5. |
| El láser sigue leyendo `DefaultDrawingAttributes.Color` y se pinta con el color del par. | El paso 9 lo cambia a `SolidInkColor`; hay criterio de aceptación explícito. |
| Los tonos neón (cian, lima) tienen bajo contraste sobre fondos blancos. | Las tintas sólidas siguen disponibles. Halo de contraste fuera de alcance. |
| `OverlayViewModel` o `ToolbarViewModel` superan 400 líneas, o un método supera 40. | Extraer a clases auxiliares (p. ej. la construcción de muestras del popover). Nunca suprimir la regla. |

---

## Lo que **no** está en este spec

- Flecha recta por arrastre (se reemplaza por la flecha libre).
- Degradado siguiendo el recorrido del trazo.
- Degradado en vivo para lápiz y flecha libre.
- Punta de flecha en vivo (spec 04).
- Degradado en texto y láser.
- Elegir o editar los pares de colores desde la app.
- Recordar el modo degradado entre sesiones.
- Reconocimiento de formas.
- Brillo/halo neón en los trazos.
- Flecha con punta en ambos extremos.
- Elipse (HU-16), rehacer (HU-17), círculo/cuadrado perfecto con Shift.

Cada uno, si llega, va en su propio spec.
