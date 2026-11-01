# SPEC 10 — Tintas degradadas (HU-27)

> **Estado:** Implementado · **Depende de:** SPEC 03 (HU-19) — modo Degradado, SPEC 05 (HU-21) — Ajustes y `settings.json`, SPEC 09 (HU-26) — cursor de tinta · **Fecha:** 2026-11-01
> **Objetivo:** Que un interruptor en Ajustes haga que cada tinta sólida pinte un degradado fijo hacia su tono vecino, recordado entre sesiones.

---

## Por qué existe este spec

- Hoy la única forma de pintar con degradado es el modo neón (`Ctrl+7`), que sortea el par en cada trazo: no se puede elegir el color.
- Con «Tintas degradadas» el color sigue lo elegido: el rojo pinta rojo → rosa, con el acabado del degradado y sin sorpresas.
- El design system ya lo dejó diseñado como «propuesto»: el interruptor, la pista, los colores vecinos y cómo se ven las muestras y el cursor.

---

## Alcance

**Dentro:**

- **Interruptor «Tintas degradadas»** en Ajustes, dentro de un bloque nuevo **«Dibujo»** que va encima de «Siempre activos»:
  - Debajo lleva la pista «Cada tinta pasa a un tono vecino» (11 px, `Fg.3`).
  - Viene apagado de entrada y no tiene atajo.
  - Apagado se ve neutro y encendido lleva el acento teal, la misma receta que la tecla que captura un atajo. El cambio es instantáneo, sin animación.
  - Se aplica al momento: la barra, el cursor y el próximo trazo cambian sin cerrar Ajustes.
  - Se guarda en ese mismo momento en `settings.json` y se recuerda al reiniciar.
  - «Restaurar todo» no lo toca: solo restaura atajos.
- **Pares fijos**, de la tinta a su vecina:

  | Tinta | Vecina | Hex inicial de la vecina |
  |-------|--------|--------------------------|
  | Roja | Rosa | `#FF2D92` |
  | Azul | Índigo | `#5E5CE6` |
  | Amarilla | Naranja | `#FF9F0A` |
  | Verde | Lima | `#A3E635` |
  | Blanca | Gris claro | `#AEAEB2` |
  | Negra | Gris oscuro | `#636366` |

  - Los hex son un primer intento y se ajustan a ojo en la app.
  - Viven como tokens de **tinta** (`Ink.<Tinta>.End.Color`), nunca de chrome.
- **Cómo pinta:**
  - El mismo eje y las mismas herramientas que el modo Degradado: Lápiz, Línea, Flecha y Rectángulo, del primer al último punto (en figuras cerradas, en diagonal).
  - El degradado se ve en vivo mientras se dibuja, como el neón (HU-20).
  - El par es siempre el mismo para cada tinta: no hay azar.
  - Texto y Láser siguen con la tinta plana.
- **Lo que se ve fuera del trazo:**
  - Las 6 muestras del popover de color enseñan su par en diagonal: 135°, el inicio arriba a la izquierda. La forma no cambia: cuadradas las tintas, redonda la muestra del modo.
  - El botón Color de la barra enseña el par de la tinta activa.
  - El cursor de tinta (HU-26) se rellena con el par.
- **Convivencia con el modo Degradado (`Ctrl+7`):** el modo neón sigue sorteando pares y no depende del interruptor. Mientras el modo neón está activo, manda él.
- **Lo ya dibujado no cambia** al tocar el interruptor. Cada trazo conserva el color con el que se pintó.
- Deshacer, Borrador y Limpiar todo funcionan igual que con cualquier trazo. El degradado sale en el recorte de pantalla sin cambios en el recorte.
- **Documentación:**
  - Alta de **HU-27** en `BACKLOG.md` (EP-02 · Could · 3).
  - Commit de la propuesta del design system que hoy está sin guardar en la skill local (11 archivos modificados y `preview/tintas-degradadas.html`).
  - Al cerrar, se quita la marca «propuesto» del interruptor, de los vecinos y de *Tintas degradadas*. Primero en el artefacto «Acetato» y después en la skill local, con los hex finales.

**Fuera de alcance (specs futuros):**

- Elegir a mano la vecina de cada tinta, o dos colores libres.
- Varios tonos por tinta, o una segunda fila de muestras.
- Un atajo o un botón en la barra para encender el interruptor.
- Degradado en Texto o Láser.
- Recordar el modo neón (`Ctrl+7`) entre sesiones.
- Repintar los trazos ya dibujados al tocar el interruptor.
- Tintas nuevas, distintas de las 6 actuales.

---

## Modelo de datos y cambios por capa

**`settings.json`** gana una sección. La versión sigue en 1, porque solo se añade una clave.

```json
{
  "version": 1,
  "hotkeys": { "Undo": "Alt+Z" },
  "drawing": { "inkPairs": true }
}
```

- Si falta `drawing`, falta `inkPairs` o no es `true`/`false`, el interruptor arranca apagado.
- Guardar los atajos no borra `drawing`, y guardar `drawing` no borra los atajos.

**Domain:**

- `GradientPair` (nuevo): el par de colores de un trazo en degradado, sea neón o de tinta.

  ```csharp
  // Par de un trazo en degradado: neón (modo Degradado) o tinta → su vecina (Tintas degradadas).
  public readonly record struct GradientPair
  {
      public TintaGradiente? Neon { get; }
      public TintaColor? Ink { get; }
      public static GradientPair FromNeon(TintaGradiente pair);
      public static GradientPair FromInk(TintaColor ink);
  }
  ```

  Es un valor: dos pares iguales comparten el pincel y el cursor en caché.

**Application:**

- `IDrawingSettings` / `DrawingSettings`:
  - `bool InkPairsEnabled { get; }` y `void SetInkPairs(bool enabled)`. Solo dispara `Changed` si el valor cambia de verdad.
  - `GradientPair? NextStrokePair { get; }`, con esta regla:
    - Modo neón activo → `FromNeon(NextGradient)`.
    - Si no, con el interruptor encendido → `FromInk(Color)`.
    - Si no → `null` (tinta plana).
  - `AdvanceGradient()` no hace nada fuera del modo neón: un trazo de tinta degradada no sortea pares.
- `DrawingPreferences` (nuevo, en `Abstractions/`): `public sealed record DrawingPreferences(bool InkPairs)`, con `Defaults` = apagado.
- `IDrawingPreferencesStore` (nuevo puerto, en `Abstractions/`): `DrawingPreferences Load()` y `void Save(DrawingPreferences preferences)`. Nunca lanza.

**Infrastructure:**

- `SettingsFile` (nuevo, `internal`):
  - Lee `settings.json` entero.
  - Reemplaza solo una sección y conserva el resto.
  - Escribe de forma atómica (archivo temporal y luego reemplazo), como hoy.
- `JsonHotkeySettingsStore` y `HotkeySettingsJson`:
  - Escriben su sección a través de `SettingsFile`.
  - Siguen siendo los únicos que apartan un archivo ilegible a `settings.corrupt.json` y avisan.
- `JsonDrawingPreferencesStore` y `DrawingPreferencesJson` (nuevos): leen y escriben la sección `drawing`. Con un archivo ilegible devuelven `Defaults` sin renombrar ni avisar, para no duplicar el aviso.

**Presentation:**

- `Tokens.xaml`:
  - Seis tintas vecinas (`Ink.Red.End.Color` … `Ink.Black.End.Color`) en la paleta de tintas.
  - Tamaños del interruptor: `Size.Switch.Width` (32), `Size.Switch.Height` (18) y `Size.Switch.Knob` (12).
- `Resources/GradientPairMap.cs` (nuevo): traduce `GradientPair` a sus dos `Color`. Un par neón usa `TintaGradienteMap`; un par de tinta usa `TintaColorMap` y el token `Ink.<Tinta>.End.Color`.
- **`TintaGradiente?` pasa a `GradientPair?`**, sin cambiar el comportamiento, en: `InkGradient.Pair`, `FreehandSession`, `FreehandStrokeBehavior`, `ShapeDrawingBehavior`, `StyledStroke.Gradient`, `StrokeGradientBrush` (caché por `GradientPair`), `InkCursorTip.Gradient` e `InkCursorFactory`.
- `OverlayViewModel.ActiveGradient` pasa a ser `_settings.NextStrokePair`. `SolidInkColor`, que usan Texto y Láser, no cambia.
- Barra:
  - `InkSwatchItem.Fill` pasa a observable.
  - `InkSwatchFactory` da el relleno de cada tinta: plano o con su par a 135°, congelado y en caché.
  - `ToolbarViewModel.SyncActiveState` refresca las muestras y `ActiveInkBrush` cuando cambia el interruptor.
- Ajustes:
  - `SettingsStyles.xaml`: estilo `SettingsSwitch` (un `ToggleButton` con el aspecto del design system) y estilo de la pista.
  - `SettingsWindow.xaml`: bloque «Dibujo» con la fila del interruptor. Nombre accesible «Tintas degradadas».
  - `SettingsViewModel`:
    - Recibe `IDrawingSettings` e `IDrawingPreferencesStore`.
    - Expone `InkPairs`. Al cambiar, llama a `SetInkPairs` y guarda.
    - `ResetAllCommand` no lo toca.
  - `SettingsWindowLauncher`: pasa las dos dependencias nuevas.
- `App.BuildServiceProvider`:
  - Registra `IDrawingPreferencesStore` → `JsonDrawingPreferencesStore`.
  - Al crear `DrawingSettings`, aplica `Load().InkPairs`.

---

## Plan de implementación

1. **Backlog y propuesta del design system.** HU-27 en `BACKLOG.md` (EP-02 · Could · 3) y commit de los cambios sin guardar de `.claude/skills/acetato-design/`. Solo docs.
2. **Par y colores vecinos.** `GradientPair` en Domain, los seis tokens `Ink.*.End.Color` y `GradientPairMap`.
   - Tests: igualdad de pares; cada tinta resuelve su vecina; un par neón resuelve lo mismo que hoy.
   - Sin cambios visibles.
3. **Cambio de tipo en la tubería del degradado.** `TintaGradiente?` → `GradientPair?` en los behaviors, el trazo, el pincel y el cursor. Comportamiento idéntico.
   - Tests existentes en verde.
   - **Manual:** el modo neón se ve igual que antes, con el par al azar y el cursor.
4. **Regla del próximo par.** `InkPairsEnabled`, `SetInkPairs`, `NextStrokePair` y la guarda en `AdvanceGradient`. `OverlayViewModel` usa `NextStrokePair`.
   - Tests: las tres ramas de la regla; el neón manda sobre el interruptor; `SetInkPairs` solo avisa si cambia; elegir otra tinta con el interruptor encendido cambia el par; `AdvanceGradient` no sortea fuera del modo neón.
5. **Guardado.** `DrawingPreferences`, el puerto, `SettingsFile`, `JsonDrawingPreferencesStore` y el ajuste del almacén de atajos. `App` aplica lo guardado al arrancar.
   - Tests:
     - `inkPairs` se guarda y se lee.
     - Si falta la clave o el valor no es booleano, arranca apagado.
     - Un `settings.json` solo con atajos sigue funcionando.
     - Guardar atajos conserva `drawing`, y guardar `drawing` conserva los atajos.
     - Un archivo ilegible deja el interruptor apagado sin un segundo aviso.
6. **Interruptor en Ajustes.** Tokens de tamaño, estilo `SettingsSwitch`, la fila con la pista y `SettingsViewModel`.
   - Tests: encender llama a `SetInkPairs(true)` y guarda; «Restaurar todo» no lo cambia.
   - **Manual:**
     - Encender el interruptor y trazar en rojo: sale rojo → rosa, en vivo.
     - Reiniciar la app: sigue encendido.
     - Las flechas dibujadas antes no cambian.
7. **Barra.** Muestras del popover y botón Color con el par.
   - Tests: con el interruptor encendido, el relleno de cada muestra y de `ActiveInkBrush` es el par a 135°; apagado, el color plano; en modo neón, el botón muestra el degradado de muestra.
   - **Manual:** abrir el popover con el interruptor encendido y apagado.
8. **Cursor.** Comprobar que el círculo enseña el par de la tinta.
   - Test: `CursorTip` lleva `FromInk(Color)` con el interruptor encendido.
   - **Manual:** con `Ctrl+3` (amarilla), el círculo va de amarillo a naranja y coincide con el inicio del trazo.
9. **Ajuste a ojo de las vecinas.** Probar las seis sobre fondo claro y oscuro y corregir los hex en `Tokens.xaml`.
10. **Design system.** Quitar «propuesto» y poner los hex finales: primero en el artefacto «Acetato» y después en la skill local. Las dos copias quedan iguales. Solo docs.

---

## Criterios de aceptación

> **Cierre (2026-11-01):** verificados con tests automáticos: regla del próximo par, guardado de la sección `drawing` sin pisar los atajos, interruptor de Ajustes, muestras y botón Color, cursor y trazo con el par de su tinta. Build `-warnaserror` y tests verdes. En la app real (prueba con mouse y teclado simulados) se comprobaron: bloque «Dibujo» y su interruptor, apagado de entrada, encendido al instante con Ajustes abierto, memoria tras reiniciar, «Restaurar todo» sin apagarlo, las seis tintas con su vecina (Lápiz y Rectángulo), modo neón, tinta elegida desde el neón, Texto y Láser planos, trazos previos sin cambios, Deshacer, Limpiar todo y el cursor. Las muestras y el botón Color solo se comprobaron con tests: la barra no sale en capturas por diseño. Quedan sin prueba manual los tres criterios sin marcar: el degradado en vivo mientras se traza, el Borrador y el recorte de pantalla. Los hex de las vecinas siguen siendo los iniciales; el ajuste a ojo (paso 9) queda a criterio del usuario.

**Ajustes**

- [x] Ajustes muestra el bloque «Dibujo» encima de «Siempre activos», con «Tintas degradadas», un interruptor a la derecha y debajo la pista «Cada tinta pasa a un tono vecino».
- [x] En una instalación nueva el interruptor está apagado.
- [x] Apagado se ve neutro y encendido con el acento teal; el cambio es instantáneo.
- [x] Al encenderlo con Ajustes abierto, el siguiente trazo ya sale degradado, sin cerrar la ventana.
- [x] Encendido y con la app reiniciada, sigue encendido.
- [x] «Restaurar todo» devuelve los atajos y deja el interruptor como estaba.
- [x] Un `settings.json` anterior, que solo tiene atajos, conserva sus atajos y arranca con el interruptor apagado.
- [x] Cambiar un atajo con el interruptor encendido no lo apaga al reiniciar, y cambiar el interruptor no borra los atajos.

**Dibujo**

- [x] Con el interruptor encendido, Lápiz, Línea, Flecha y Rectángulo pintan cada tinta hacia su vecina: roja → rosa, azul → índigo, amarilla → naranja, verde → lima, blanca → gris claro y negra → gris oscuro.
- [x] El degradado corre del primer al último punto; en el rectángulo, en diagonal.
- [ ] Con Lápiz y Flecha, el degradado se ve mientras se traza y queda igual al soltar.
- [x] Dos trazos seguidos con la misma tinta tienen exactamente el mismo par.
- [x] Texto y Láser pintan con la tinta plana con el interruptor encendido.
- [x] Con el modo neón (`Ctrl+7`) activo, cada trazo sigue tomando un par neón al azar, esté como esté el interruptor.
- [x] Al elegir una tinta sólida desde el modo neón con el interruptor encendido, el siguiente trazo sale con el par de esa tinta.
- [x] Encender o apagar el interruptor no cambia los trazos que ya están en pantalla.
- [ ] Deshacer, Borrador y Limpiar todo quitan un trazo de tinta degradada entero, como cualquier otro.
- [ ] El recorte de pantalla muestra los trazos con su degradado.

**Barra y cursor**

- [x] Con el interruptor encendido, las 6 muestras del popover enseñan su par en diagonal y la muestra del modo neón sigue redonda; apagado, son planas.
- [x] El botón Color muestra el par de la tinta activa; en modo neón, el degradado de muestra.
- [x] El cursor de tinta se rellena con el par de la tinta activa y cambia al instante al cambiar de tinta o al tocar el interruptor.

**General**

- [x] `dotnet build -warnaserror` y `dotnet test` en verde.
- [x] `BACKLOG.md` incluye HU-27.
- [x] Las dos copias del design system describen *Tintas degradadas* sin la marca «propuesto» y con los mismos hex que `Tokens.xaml`.

---

## Decisiones tomadas

1. **Se sigue el design system al pie de la letra:** vecino fijo, interruptor en Ajustes → Dibujo, apagado de entrada, recordado, sin atajo, y Texto y Láser planos. Se descartaron dos colores libres y varios tonos por tinta, porque piden un selector nuevo.
2. **El neón manda cuando está activo.** Así lo dice el design system: el modo Degradado no depende del interruptor. Se descartó que el interruptor sustituyera al modo neón.
3. **Lo dibujado no cambia** al tocar el interruptor (elección del usuario). Es coherente con el modo neón, donde cada trazo conserva su par, y no complica «Deshacer». Se descartó repintar los trazos.
4. **Se aplica y se guarda al instante**, sin botón «Guardar», igual que los atajos.
5. **Un solo tipo `GradientPair` para los dos degradados.** La tubería que ya pinta el neón (en vivo, al soltar, el cursor y la caché) sirve sin duplicarla. Se descartó añadir seis valores al enum `TintaGradiente`: mezclaría pares de tinta con pares neón, y el sorteo tendría que excluirlos a mano.
6. **El sorteo solo avanza en modo neón.** Un trazo de tinta degradada no gasta pares neón ni refresca la barra y el cursor sin motivo.
7. **`settings.json` sigue en la versión 1, con la sección `drawing`.** Solo se añade una clave y un archivo viejo se lee igual. Se descartó subir la versión y se descartó un segundo archivo de ajustes.
8. **Un solo lector y escritor del archivo (`SettingsFile`)**, que conserva lo que no le toca. Sin él, guardar los atajos borraría el interruptor y al revés.
9. **Solo el almacén de atajos aparta un archivo dañado y avisa.** Así no salen dos avisos por el mismo archivo.
10. **«Restaurar todo» solo restaura atajos.** Lo dice el design system, y la etiqueta de la confirmación ya es «Restaurar todos los atajos».
11. **Los tooltips de las muestras no cambian** («Tinta roja», etc.). El par se ve en la muestra, y una palabra más rompería la regla de una o dos palabras.
12. **HU-27 en EP-02 · Could · 3** (elección del usuario). Es un gusto visual, no imprescindible, y reutiliza casi toda la tubería del neón. Se descartó Should.
13. **La propuesta del design system se guarda en git en el paso 1** (elección del usuario), para que el paso 10 solo quite la marca «propuesto» y no se mezclen los cambios.
14. **Definición rápida.** El usuario pidió el spec completo de una vez, sin confirmar sección por sección; las dudas surgidas al redactarlo se resolvieron al final, antes de guardarlo.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Algún vecino casi no se distingue de su tinta (amarilla → naranja) o se pierde sobre el fondo (blanca → gris claro sobre blanco). | El paso 9 los prueba sobre fondo claro y oscuro y corrige los hex antes de cerrar. |
| Cambiar el tipo en la tubería del degradado (paso 3) rompe el modo neón. | El paso 3 no cambia el comportamiento, usa los tests existentes y tiene una prueba manual del modo neón antes de seguir. |
| Una versión anterior de Acetato, al guardar atajos, borra la sección `drawing`. | Se acepta: solo pasa al volver a una versión vieja, y el efecto es que el interruptor vuelve a estar apagado. |
| `ToolbarViewModel` (327 líneas) supera el tope de 400 al sumar el refresco de las muestras. | La construcción de rellenos vive en `InkSwatchFactory`. Si aun así se pasa, se extrae a una clase propia. |
| La caché de cursores y pinceles crece. | Está acotada: 6 pares neón + 6 de tinta + 6 tintas planas, por 5 grosores y por escala. |

---

## Lo que **no** entra en este spec

- Elegir la vecina a mano, dos colores libres o varios tonos por tinta.
- Atajo o botón en la barra para el interruptor.
- Degradado en Texto o Láser.
- Recordar el modo neón entre sesiones o repintar lo ya dibujado.
- Tintas nuevas.

Cada una de esas, si llega, va en su propio spec.
