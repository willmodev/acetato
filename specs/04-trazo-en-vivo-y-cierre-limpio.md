# SPEC 04 — Trazo libre en vivo, flecha suavizada y cierre limpio (HU-20)

> **Estado:** Implementado · **Depende de:** SPEC 03 (HU-19) — sobre el código ya implementado HU-01…HU-19 · **Fecha:** 2026-10-07
> **Objetivo:** Que el lápiz y la flecha se pinten en vivo con su estilo final (punta y degradado), que la flecha salga con curvas suaves y que «Salir» termine el proceso.

---

## Por qué existe este spec

- El spec 03 dejó fuera la **punta en vivo** y el **degradado en vivo** del trazo libre. La pintura en vivo de Windows (la del `InkCanvas`) solo admite un color sólido y no permite dibujar la punta, así que ambas cosas exigen un pintado propio del trazo en curso.
- El pulso a mano alzada deja curvas temblorosas en la flecha. Se quiere un acabado estético sin perder la intención del trazo.
- «Salir» en la bandeja deja vivo `Acetato.exe`, que bloquea el build. **Causa confirmada** con un volcado de pilas del proceso colgado (`dotnet-stack`):
  - El hilo de UI queda bloqueado en `Win32GlobalHotkeyService.Unregister()` → `Thread.Join()`, llamado desde `App.OnExit`.
  - El hilo de atajos sigue en `GetMessage(..., WmHotkey, WmHotkey)`: su filtro solo acepta `WM_HOTKEY`.
  - `Unregister` le envía `WM_QUIT` con `PostThreadMessage`. Un `WM_QUIT` **enviado como mensaje** queda en la cola como cualquier otro y el filtro lo descarta. (Solo el estado de salida de `PostQuitMessage` salta el filtro.)
  - Resultado: `Join()` espera para siempre. `OnExit` ya quitó el icono (por eso desaparece), pero el menú contextual queda congelado en pantalla y el proceso no termina.

---

## Alcance

**Dentro:**

- **Captura propia del trazo libre:** el lápiz y la flecha dejan el trazado nativo de Windows. La app sigue el mouse (o el lápiz digital, convertido en mouse) y pinta el trazo en curso en una capa propia, como hoy hace con línea y rectángulo.
- **Punta de flecha en vivo:** aparece mientras se traza, orientada según el tramo final. Se oculta mientras el recorrido sea más corto que la punta.
- **Degradado en vivo** para lápiz y flecha: va del punto inicial a la punta actual del trazo. Al soltar queda igual a lo que se vio.
- **Suavizado de la flecha:**
  - Mientras se traza, un filtro leve quita el temblor.
  - Al soltar, el recorrido se ajusta a curvas Bézier y queda como trazo final.
  - El lápiz **no** se suaviza más allá de lo actual: conserva el pulso real.
- **Al soltar**, el trazo entra a la pantalla una sola vez: un deshacer lo quita entero; un clic suelto con la flecha no deja nada.
- **Si se pierde el mouse a mitad del trazo** (cambio a click-through, overlay oculto, otra ventana roba la captura), lo dibujado hasta ahí se fija como si se hubiera soltado.
- **Medición del rendimiento** solo en compilación Debug: tiempo típico y peor (p50/p95) de cada repintado, escrito en la salida de depuración al soltar.
- **Cierre limpio:**
  - El hilo de atajos recibe la orden de terminar.
  - La espera al cerrar tiene un tope de 2 s.
  - El icono de bandeja se quita al final del cierre, no al principio.
- **Nuevo proyecto de tests de Infrastructure** con una prueba del cierre de atajos.
- Alta de **HU-20** en `BACKLOG.md`.

**Fuera de alcance (specs futuros):**

- Suavizado del **lápiz**.
- **Presión** del lápiz digital (grosor variable).
- Degradado **siguiendo el recorrido** (cada tramo conserva su color).
- **Reconocimiento de formas** (convertir un garabato en figura limpia).
- Ajuste de la **intensidad del suavizado** desde la app.
- Benchmarks automáticos (BenchmarkDotNet).
- Degradado en texto y láser, brillo/halo, flecha con dos puntas.
- Elipse (HU-16), rehacer (HU-17).

---

## Modelo de datos

### Domain (puro, sin WPF, testeable)

```csharp
// Curva cúbica: extremos P0/P3 y controles P1/P2.
public readonly record struct CubicBezier(StrokePoint P0, StrokePoint P1, StrokePoint P2, StrokePoint P3)
{
    public StrokePoint At(double t);   // punto en t ∈ [0, 1]
}

// Filtro de temblor en vivo (incremental): media exponencial + distancia mínima.
public sealed class JitterFilter
{
    public JitterFilter(double smoothing, double minDistance);
    public bool TryAdd(StrokePoint raw, out StrokePoint filtered); // false = punto descartado (demasiado cerca)
    public void Reset();
}

// Ajuste de un recorrido a curvas Bézier (algoritmo de Schneider, Graphics Gems 1990):
// tangentes en los extremos, parametrización por longitud de cuerda, mínimos cuadrados,
// reparametrización de Newton y división recursiva en el punto de mayor error.
public static class BezierFitter
{
    public static IReadOnlyList<CubicBezier> Fit(IReadOnlyList<StrokePoint> path, double tolerance);
}

// Vuelve a convertir las curvas en puntos densos (el trazo se guarda como puntos).
public static class BezierSampler
{
    public static IReadOnlyList<StrokePoint> Sample(IReadOnlyList<CubicBezier> curves, double spacing);
}

// Orquesta el suavizado final de la flecha: Fit + Sample con la tolerancia según el grosor.
public static class ArrowPathSmoother
{
    public static IReadOnlyList<StrokePoint> Smooth(IReadOnlyList<StrokePoint> path, double thickness);
}
```

- **Filtro en vivo:** `smoothing` = 0,5 (peso del punto nuevo), `minDistance` = 1 DIP.
- **Tolerancia del ajuste:** `4 + 0,5 × grosor` DIP (error máximo entre la curva y los puntos). Mayor tolerancia = curva más limpia y menos fiel.
- **Muestreo:** un punto cada 2 DIP sobre la curva.
- Si el ajuste necesita partirse en un tramo (más error que la tolerancia), se divide en el punto de mayor error. Así una **esquina marcada se conserva** en vez de redondearse.
- Los números son **valores iniciales**, ajustables a ojo al probar. Los tests verifican reglas (extremos, error dentro de la tolerancia, esquinas), no cifras exactas.
- `ArrowHeadBuilder` y `GradientAxis` no cambian: la punta y el eje del degradado se calculan igual en vivo que al soltar.

### Application

Sin cambios de puertos ni de `IDrawingSettings`.

### Infrastructure

- **`Win32GlobalHotkeyService.PumpMessages`:** `GetMessage(out msg, 0, 0, 0)` (sin filtro). Solo actúa ante `WM_HOTKEY`; ignora el resto. Devuelve 0 con `WM_QUIT` y el bucle termina.
- **`Unregister`:**
  - Comprueba el resultado de `PostThreadMessage`.
  - Espera con tope: `Join(TimeSpan.FromSeconds(2))`. El hilo ya es de fondo (`IsBackground = true`), así que, si el tope vence, el proceso termina igual.

### Presentation

```csharp
// Capa de dibujo del trazo en curso (encima del InkCanvas, IsHitTestVisible = False).
// Pinta un Stroke prestado en OnRender; no guarda nada en el historial.
internal sealed class LiveStrokeLayer : FrameworkElement
{
    public void Show(Stroke stroke);   // reemplaza el trazo mostrado e invalida el pintado
    public void Clear();
}

// Estado de un trazo libre en curso (por InkCanvas).
internal sealed class FreehandSession
{
    public void Add(StrokePoint raw);          // filtra (solo flecha) y acumula
    public Stroke? BuildPreview();             // trazo en vivo: cuerpo + punta + degradado al punto actual
    public Stroke? BuildFinal();               // trazo fijo: flecha suavizada; null = flecha descartada
}

// Medición solo en Debug ([Conditional("DEBUG")]); cero costo en Release.
internal sealed class LiveRenderStats
{
    public void Record(TimeSpan elapsed);
    public void Report(int pointCount);        // Debug.WriteLine: n, p50, p95, máx, puntos
}
```

- **`FreehandStrokeBehavior`** cambia de mecanismo: deja de escuchar `StrokeCollected` y pasa a capturar el mouse (`PreviewMouseLeftButtonDown/Move/Up` + `LostMouseCapture`), como `ShapeDrawingBehavior`.
  - Nueva attached property `FreehandStrokeBehavior.Layer` (enlazada por `ElementName`, igual que el láser).
  - Repinta **como máximo una vez por fotograma** (`CompositionTarget.Rendering`, solo si hubo puntos nuevos), igual que `LaserSession`.
- **`OverlayViewModel.ToEditingMode`:** `Pencil` y `Arrow` → `InkCanvasEditingMode.None`.
- **`OverlayViewModel.ActiveAttributes.Color`** vuelve a ser **siempre la tinta sólida**: el truco del "primer color del par" ya no hace falta, porque el degradado se pinta en vivo.
- **`OverlayWindow.xaml`:** nuevo `LiveStrokeLayer` en el `Grid`, entre el `InkCanvas` y la capa del láser.
- El lápiz sólido sigue fijándose como `Stroke` normal con `FitToCurve = true` (mismo aspecto que hoy). Flecha y lápiz en degradado se fijan como `StyledStroke`.
- La flecha final se fija con `FitToCurve = false`: sus puntos ya vienen de curvas Bézier.

### Tests

- Nuevo proyecto **`tests/Acetato.Infrastructure.Tests`** (`net9.0-windows10.0.19041.0`, xUnit + FluentAssertions, referencia a `Acetato.Infrastructure`), añadido a `Acetato.sln`.

**Convención:** todas las coordenadas en DIP del lienzo de cada pane, como en el spec 03.

---

## Plan de implementación

Cada paso compila con `-warnaserror`, deja la app usable y es commiteable por sí solo.

1. **Backlog.** Añadir **HU-20 — Trazo libre en vivo y flecha suavizada** a `BACKLOG.md` (narrativa, Gherkin, referencia a este spec, criterio del cierre limpio). Solo docs.
2. **Cierre limpio (Infrastructure + App).**
   - Crear `tests/Acetato.Infrastructure.Tests` y añadirlo a la solución.
   - Test: `Register` y luego `Unregister` termina en menos de 1 s. Con el código actual se cuelga.
   - `PumpMessages` sin filtro; `Unregister` con `Join` de 2 s y comprobación de `PostThreadMessage`.
   - `App.OnExit` en este orden: liberar atajos → liberar servicios → quitar el icono de bandeja.

   **Manual:** «Salir» cierra el menú y `Acetato.exe` desaparece del Administrador de tareas; `dotnet build` no da MSB3026.
3. **Filtro de temblor (Domain).** `JitterFilter`. Tests:
   - Descarta puntos más cerca que la distancia mínima.
   - Atenúa un zigzag (el desvío filtrado es menor que el original).
   - Una recta sigue siendo recta.

   Sin cambio visible.
4. **Curvas Bézier (Domain).** `CubicBezier` + `BezierSampler`. Tests: `At(0)`/`At(1)` son los extremos; el muestreo empieza y termina en los extremos y respeta el espaciado. Sin cambio visible.
5. **Ajuste de curvas (Domain).** `BezierFitter` + `ArrowPathSmoother`. Si un archivo pasa de 400 líneas o un método de 40, extraer los cálculos a `BezierFitMath`. Tests:
   - Conserva el primer y el último punto.
   - Ningún punto del recorrido queda a más de la tolerancia de la curva.
   - Una recta ruidosa da una sola curva casi recta.
   - Una "L" conserva la esquina (dos curvas, vértice cerca de la esquina original).
   - Recorridos de 0, 1 y 2 puntos no fallan.

   Sin cambio visible.
6. **Capa en vivo y medición (Presentation).** `LiveStrokeLayer` y `LiveRenderStats` (Debug). Se añade la capa a `OverlayWindow.xaml`. Sin uso aún; sin cambio visible.
7. **Lápiz con captura propia.**
   - `FreehandSession` (solo lápiz) y el nuevo mecanismo de `FreehandStrokeBehavior` (captura, repintado por fotograma, fijado al soltar, `LostMouseCapture` fija lo dibujado).
   - `ToEditingMode`: `Pencil` → `None`.
   - `ActiveAttributes.Color` = tinta sólida siempre.
   - La sesión fija el grosor al empezar; la rueda solo afecta al próximo trazo.
   - Cursor en cruz (`Cursors.Cross` + `ForceCursor`) con lápiz, flecha, línea y rectángulo.
   - Medición: `Record` en cada repintado, `Report` al soltar.

   **Manual:** el lápiz sólido se ve igual que antes; el cursor es una cruz; en modo degradado se ve en degradado mientras se traza; un deshacer quita el trazo entero; la salida de depuración muestra p50/p95.
8. **Flecha en vivo.**
   - `FreehandSession` para flecha: filtro de temblor y punta en vivo (oculta mientras el recorrido sea más corto que la punta).
   - `ToEditingMode`: `Arrow` → `None`.
   - Al soltar: descarte si es más corta que la punta; si no, se fija **sin suavizado final** todavía.

   **Manual:** la punta sigue al trazo mientras se dibuja; un clic suelto no deja nada.
9. **Flecha suavizada al soltar.** `BuildFinal` aplica `ArrowPathSmoother` y recalcula la punta sobre la curva suavizada. **Manual:** una curva temblorosa queda limpia al soltar, con poco salto respecto de lo visto en vivo.

---

## Criterios de aceptación

> **Cierre (2026-10-07):** verificados en la app real con entrada simulada: degradado y punta en vivo, flecha suavizada, clic suelto, deshacer, p95 = 3,86 ms con 1.909 puntos, memoria plana en 60 ciclos y «Salir» sin proceso vivo ni MSB3026. Build y tests verdes. El resto (multi-monitor, PNG, rueda/deshacer/click-through a mitad del trazo, cursor, 5 cierres seguidos) lo dio por cerrado el usuario sin prueba manual dedicada.

**Cierre limpio**

- [x] «Salir» en la bandeja cierra el menú y `Acetato.exe` desaparece del Administrador de tareas en menos de 2 s.
- [x] Tras salir, `dotnet build` no falla con MSB3026 (archivo en uso).
- [x] Repetir 5 veces abrir → dibujar → «Salir» no deja ningún proceso `Acetato.exe` vivo.
- [x] Tras salir y volver a abrir la app, los atajos `Ctrl+Alt+…` funcionan (se liberaron bien).
- [x] El test de Infrastructure del cierre de atajos pasa.

**Lápiz y flecha en vivo**

- [x] El lápiz sólido se ve igual que antes (mismo grosor, color y suavizado).
- [x] En modo degradado, lápiz y flecha se ven en degradado mientras se trazan; el color final está siempre en la punta del trazo.
- [x] Al soltar, el degradado del lápiz no cambia respecto de lo que se veía en vivo.
- [x] La punta de la flecha aparece mientras se traza y sigue la dirección del tramo final.
- [x] Mientras el recorrido es más corto que la punta, no se ve punta; un clic suelto con la flecha no deja nada.
- [x] Un solo `Ctrl+Alt+Z` quita el trazo libre entero (lápiz o flecha).
- [x] Cambiar a click-through (`Ctrl+Alt+E`) a mitad del trazo deja fijo lo dibujado y no deja restos en la capa en vivo.
- [x] Dos trazos seguidos en degradado nunca salen con el mismo par.
- [x] Con lápiz, flecha, línea y rectángulo el cursor es una cruz fina.
- [x] Girar la rueda a mitad del trazo no cambia el grosor del trazo en curso; el siguiente sale con el grosor nuevo.
- [x] `Ctrl+Alt+Z` a mitad del trazo quita la anotación anterior y el trazo en curso sigue dibujándose.
- [x] Texto, láser, borrador, línea y rectángulo funcionan igual que antes.

**Flecha suavizada**

- [x] Una flecha curva trazada con temblor queda con una curva continua y sin dientes al soltar.
- [x] Una flecha recta trazada a mano queda recta.
- [x] Una flecha con una esquina marcada (forma de "L") conserva la esquina.
- [x] La punta final apunta según el tramo final de la curva suavizada.
- [x] El lápiz no recibe el suavizado de la flecha (una firma a mano conserva su forma).

**Rendimiento e integración**

- [x] En Debug, al soltar un trazo de ~2.000 puntos, la salida de depuración muestra p95 ≤ 8 ms por repintado.
- [x] Dibujar 50 trazos en degradado seguidos no produce lentitud perceptible al trazar.
- [x] Con dos monitores, todo lo anterior funciona en ambos.
- [x] El PNG de `Ctrl+Alt+S` incluye las flechas suavizadas y los degradados tal como se ven.
- [x] Activar/desactivar el overlay 20 veces dibujando no deja errores ni crecimiento sostenido de memoria.
- [x] `dotnet build -warnaserror` verde, sin suprimir reglas; ningún archivo pasa de 400 líneas.
- [x] `dotnet test` verde, con tests nuevos de `JitterFilter`, `CubicBezier`, `BezierSampler`, `BezierFitter` y del cierre de atajos.
- [x] `BACKLOG.md` incluye HU-20.

---

## Decisiones

- **Sí:** captura propia del trazo libre (como las formas). Mismo patrón que ya funciona; permite pintar punta y degradado en vivo con menos piezas. Decisión del usuario.
- **No:** mantener el trazado nativo y pintar una capa encima. Conserva el lápiz digital nativo, pero exige subclasear el `InkCanvas`, apagar su pintado en vivo y coordinar dos fuentes de puntos.
- **Sí:** el trazo en curso vive en una capa propia y entra a la colección de trazos una sola vez al soltar. Reemplazar el trazo en la colección en cada movimiento (como la vista previa de formas) generaría cientos de altas y bajas por trazo en el historial.
- **Sí:** repintar como máximo una vez por fotograma. El mouse puede dar más eventos que fotogramas; pintar los sobrantes es trabajo perdido.
- **Sí:** degradado en vivo del inicio a la punta actual. El resultado al soltar es idéntico a lo visto; es como funciona ScreenBrush. Decisión del usuario.
- **No:** degradado fijo por distancia recorrida. No coincide con el degradado lineal del spec 03 y exige pintar tramo por tramo.
- **Cambio respecto del spec 03:** allí se descartó el degradado en vivo por riesgo de rendimiento y porque "el color cambiaría mientras se dibuja". Se acepta ahora: el riesgo se mide (criterio p95) y el cambio de color es el comportamiento deseado.
- **Sí:** suavizado solo para la flecha. El lápiz se usa para escribir o subrayar; suavizarlo deformaría la letra. Decisión del usuario.
- **Sí:** filtro leve en vivo + ajuste Bézier al soltar. Quita el temblor sin un salto visible grande al soltar. Decisión del usuario.
- **No:** solo ajuste al soltar (el salto se vería) ni solo filtro en vivo (curvas menos limpias).
- **Sí:** algoritmo de Schneider para el ajuste. Es el estándar para curvas a mano alzada (lo usan editores vectoriales), conserva esquinas al dividir donde hay más error y es puro cálculo, testeable en Domain.
- **No:** Chaikin o media móvil como suavizado final. Más simples, pero redondean las esquinas y encogen las curvas.
- **Sí:** guardar la flecha suavizada como puntos muestreados. El historial, el borrador y la captura siguen funcionando sin cambios.
- **No:** guardar las curvas Bézier en el trazo. Obligaría a tocar el borrador y la geometría del trazo.
- **Sí:** al perder el mouse a mitad del trazo, se fija lo dibujado. Perder un trazo largo por un atajo sería frustrante.
- **Sí:** medir con cronómetro solo en Debug (p50/p95 al soltar). Mide el pintado real con cero costo en Release. Decisión del usuario.
- **No:** proyecto de benchmarks. No mide el pintado en pantalla y suma una dependencia.
- **Sí:** cierre con arreglo de la causa + tope de espera de 2 s. Si un fallo futuro repite el cuelgue, el proceso termina igual. Decisión del usuario.
- **Sí:** quitar el icono de bandeja al final del cierre. Mientras el icono esté, la app sigue cerrándose; hoy se quita primero y oculta el cuelgue.
- **Sí:** test automático del cierre en un proyecto nuevo de Infrastructure. Hoy fallaría (se cuelga) y evita que el error vuelva. Decisión del usuario.
- **Sí:** un solo spec para trazo en vivo, suavizado y cierre. Decisión del usuario: el cierre se pidió aquí aunque es un área distinta; va en su propio paso y commit.
- **Sí:** nueva HU-20. Mantiene HU-19 cerrada. Decisión del usuario.
- **Sí:** el lápiz digital funciona como mouse. Se pierde la presión, que hoy la app no aprovecha.
- **Sí:** cursor en cruz fina (`Cursors.Cross`) para lápiz, flecha, línea y rectángulo. Sin el trazado nativo desaparece el cursor de pluma del `InkCanvas`; la cruz marca dónde cae el trazo sin tapar lo de debajo.
- **Sí:** valores iniciales del suavizado (tolerancia `4 + 0,5 × grosor` DIP, filtro 0,5 / 1 DIP), ajustables a ojo en el paso 9. No hay referencia mejor que probarlos con trazos reales.
- **Sí:** el trazo en curso conserva el grosor con el que empezó; la rueda a mitad del trazo cambia el del próximo. Un trazo que cambia de grosor a medias se vería roto.
- **Sí:** `Ctrl+Alt+Z` a mitad del trazo deshace la anotación anterior y el trazo en curso sigue. El trazo en curso aún no está en el historial; cancelarlo perdería lo dibujado.
- **Proceso:** las secciones de modelo, plan, criterios, decisiones y riesgos se redactaron de una vez, sin confirmación una por una. Decisión del usuario; las cuatro dudas abiertas (cursor, valores del suavizado, rueda y deshacer a mitad del trazo) las aprobó al releer.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Repintar el trazo completo en cada fotograma se vuelve lento con trazos largos. | Medición p95 en Debug con criterio de 8 ms. Si no se cumple: cachear la geometría ya fija y repintar solo el pincel y la cola. |
| La captura con mouse da menos puntos que el trazado nativo con lápiz digital (trazo más anguloso). | El lápiz mantiene `FitToCurve`; la flecha se suaviza. Presión y alta frecuencia quedan fuera de alcance. |
| El suavizado cambia la intención (borra una esquina querida o curva una recta). | Tolerancia proporcional al grosor y división en el punto de mayor error. Tests de "L" y de recta. Valores ajustables a ojo. |
| El salto entre la flecha en vivo y la suavizada se nota al soltar. | El filtro en vivo acerca lo visto al resultado. Si molesta, se ajusta la tolerancia (está fuera de alcance exponerla en la UI). |
| La capa en vivo queda con restos si el trazo se interrumpe (overlay oculto, cambio de herramienta). | `LostMouseCapture` y el cambio de herramienta fijan o limpian la sesión y llaman `LiveStrokeLayer.Clear()`. Criterio de aceptación explícito. |
| El test del cierre registra atajos globales reales y choca con una instancia abierta de la app. | El registro es best-effort; el test mide el cierre, no el registro. Se documenta cerrar la app antes de `dotnet test`. |
| El tope de 2 s oculta un cuelgue futuro del hilo de atajos. | El test del cierre exige menos de 1 s; un cuelgue lo hace fallar aunque el tope salve el cierre real. |
| `BezierFitter` o `FreehandStrokeBehavior` superan 400 líneas, 40 por método o complejidad 10. | Extraer a `BezierFitMath` y `FreehandSession`. Nunca suprimir la regla. |

---

## Lo que **no** está en este spec

- Suavizado del lápiz.
- Presión del lápiz digital.
- Degradado siguiendo el recorrido.
- Reconocimiento de formas.
- Ajuste de la intensidad del suavizado desde la app.
- Benchmarks automáticos.
- Degradado en texto y láser, brillo/halo, flecha con dos puntas.
- Elipse (HU-16), rehacer (HU-17).

Cada uno, si llega, va en su propio spec.
