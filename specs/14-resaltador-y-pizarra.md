# SPEC 14 — Resaltador y pizarra (HU-34, HU-35)

> **Estado:** Aprobado · **Depende de:** SPEC 03 (HU-19) — modo Degradado, SPEC 06 (HU-22) — atajos por herramienta, SPEC 08 (HU-24) — modo puntero, SPEC 09 (HU-26) — cursor de tinta, SPEC 12 (HU-29) — recordar la última sesión · **Fecha:** 2026-11-13
> **Objetivo:** Sumar a la barra un resaltador semitransparente y una pizarra blanca o negra, con hoja propia, que tapa la pantalla para explicar sin lo que hay detrás.

---

## Por qué existe este spec

- **Resaltador:** hoy, para señalar una frase en pantalla, hay que encerrarla con el lápiz o con un rectángulo, y eso tapa lo de abajo. Seis de los ocho competidores revisados tienen un marcador translúcido. Epic Pen lo da gratis.
- **Pizarra:** hoy no hay forma de explicar algo «desde cero» sin abrir otra app. Cuatro de los ocho competidores tienen pizarra. Epic Pen la cobra.
- Son los puntos F1 y F2 de `docs/lanzamiento-y-monetizacion.md`, marcados como casi obligatorios para lanzar.
- La presión del lápiz (F3) queda para otro spec. Hoy el trazo no la usa, y probarla exige un equipo con lápiz.

---

## Alcance

### Resaltador (HU-34)

- **Herramienta nueva «Resaltador»**, con su botón justo después del Lápiz y el ícono Lucide `highlighter`, que ya está reservado en el design system.
- Se pinta **a mano alzada**, con el mismo suavizado y el mismo trazo en vivo que el Lápiz.
- **Tinta:** la elegida, translúcida al **50 %** (el valor fijo del resaltador de WPF, ver decisión 4). Con el modo Degradado activo usa la última tinta sólida, como Texto y Láser.
- **Grosor:** tres veces el paso elegido. Con 3 px pinta 9 px, y con 18 px pinta 54 px.
- **Capa:** siempre queda debajo de lo dibujado con Lápiz, Línea, Flecha, Rectángulo, Elipse y Texto, aunque se haya dibujado después.
- **Cruces:** repasar con la misma tinta no oscurece, ni dentro de una pasada ni entre pasadas distintas. Dos tintas distintas sí se mezclan donde se cruzan.
- **Cursor de tinta:** muestra el ancho y el color translúcido del resaltador.
- **Atajo** `Ctrl+U`, solo en modo dibujo. Aparece en Ajustes como «Resaltador» y se puede cambiar.
- **Ciclo** `Ctrl+Espacio`: queda Lápiz → Resaltador → Láser → Línea → Flecha → Rectángulo → Elipse → Texto → Borrador.
- Se recuerda al reiniciar como cualquier herramienta (SPEC 12).
- Funciona igual que los demás trazos con Deshacer, Borrador, Limpiar todo, Recortar y Mover. Al redimensionarlo con Mover, su grosor tampoco cambia.

### Pizarra (HU-35)

- **Botón «Pizarra»** en la barra, entre Recortar y Ajustes.
- Cada clic pasa por **apagada → blanca → negra → apagada**.
- **Ícono:** un pizarrón pequeño, vacío si está apagada, relleno de blanco si está blanca y relleno de negro si está negra. No usa el acento teal, que sigue marcando solo la herramienta activa.
- **Atajo** `Ctrl+W`, solo en modo dibujo, con el mismo ciclo. Aparece en Ajustes como «Pizarra» y se puede cambiar.
- **Colores:** blanco hueso y gris casi negro, sin llegar a los puros. El tono exacto se elige en el design system. Es una familia de colores nueva, aparte de la barra (*Chrome*) y de las tintas (*Tintas*).
- **Un solo monitor:** la pizarra sale solo en el monitor donde está el cursor al prenderla. El botón y el atajo siguen cambiando esa pizarra aunque el cursor se vaya a otro monitor. No puede haber dos pizarras prendidas a la vez.
- **Hoja propia:** al prenderla se ve la hoja de la pizarra, vacía la primera vez. Lo dibujado sobre la pantalla de ese monitor se esconde y vuelve intacto al apagarla.
- **Cada monitor tiene su hoja de pizarra.** Dura mientras Acetato esté abierto, y la blanca y la negra comparten la misma hoja.
- **Deshacer, Borrador, Mover y Limpiar todo** actúan solo sobre la hoja visible. Con la pizarra prendida en el monitor 1, «Limpiar todo» vacía la pizarra del monitor 1 y lo dibujado sobre la pantalla en los demás monitores, sin tocar lo escondido debajo de la pizarra.
- **Tinta contraria:** cada vez que la pizarra cambia de color, si la tinta activa es igual al fondo, pasa a la contraria: sobre la pizarra blanca, la tinta blanca pasa a negra; sobre la negra, la tinta negra pasa a blanca. Con el Degradado activo no cambia nada. Al apagarla, la tinta queda como esté.
- **Al salir del modo dibujo** (`Esc`, botón Puntero o `Ctrl+Alt+E`), la pizarra se apaga y su hoja queda guardada.
- **Al ocultar Acetato** (`Ctrl+Alt+D`), la pizarra se oculta con todo lo demás. Al volver a mostrarlo, arranca apagada.
- Plegar la barra con el ojo no cambia la pizarra.
- **Recortar** captura la pizarra con lo dibujado en ella.
- Siempre arranca apagada. Ni la pizarra ni lo dibujado en ella se recuerdan al cerrar Acetato.

**Fuera de alcance (para otros specs):**

- Presión del lápiz (F3).
- Pizarra en todos los monitores a la vez.
- Más colores de pizarra, cuadrícula o renglones.
- Varias páginas de pizarra o guardarla como imagen aparte del recorte.
- Recordar la pizarra o sus dibujos entre sesiones.
- Resaltador recto por arrastre, resaltador con degradado o cambiar su transparencia en Ajustes.

---

## Modelo de datos y cambios por capa

`settings.json` **no gana secciones**. Lo nuevo entra por nombre en lo que ya existe:

- `hotkeys`: las acciones `SelectHighlighterTool` (`Ctrl+U`) y `CycleWhiteboard` (`Ctrl+W`). Como el archivo solo guarda diferencias, un `settings.json` viejo recibe los atajos nuevos por defecto.
- `session.tool`: puede valer `"Highlighter"`. Se guarda por nombre, así que agregarlo a la lista no rompe archivos guardados.
- La pizarra y su hoja **no** se guardan.

**Domain:**

- `ToolKind.Highlighter`, después de `Pencil`.
- `ToolRing`: `Highlighter` después de `Pencil`.
- `HighlighterWidth.From(double thickness)` → `thickness × 3`. Es puro y tiene tests.
- `WhiteboardMode` (enum): `Off`, `White`, `Black`. Con `WhiteboardCycle.Next(mode)`: `Off → White → Black → Off`.
- `WhiteboardInk.Contrast(TintaColor ink, WhiteboardMode mode)` → `TintaColor?`: devuelve `Black` si la tinta es `White` y el fondo `White`, devuelve `White` si la tinta es `Black` y el fondo `Black`, y si no, `null`. Es puro y tiene tests.

**Application:**

- `HotkeyAction.SelectHighlighterTool` y `HotkeyAction.CycleWhiteboard`, con su fila en `HotkeyCatalog` (solo en modo dibujo).
- `IOverlayCanvas` gana `ShowWhiteboard(WhiteboardMode mode)`. Con `Off` vuelve a la lámina de pantalla. `Clear`, `Undo`, `DeleteSelection` y `TryCommitText` pasan a actuar sobre la lámina visible.
- `IWhiteboard` (puerto) + `WhiteboardController` (caso de uso, en `Overlay/`):
  - Estado: `Mode` y el monitor donde está prendida. Evento `Changed` para la barra.
  - `Cycle()`: si está apagada, la prende en el lienzo del monitor del cursor. Si está prendida, avanza el color en ese mismo lienzo.
  - Al cambiar de color aplica `WhiteboardInk.Contrast` mediante `IDrawingSettings.SelectColor`, salvo con el Degradado activo.
  - Escucha `IOverlayController.StateChanged`: si se sale del modo dibujo o se oculta la capa, la apaga.
- `OverlayBroadcaster` no cambia: la pizarra vive en su propio caso de uso para no agrandarlo.

**Infrastructure:**

- Sin cambios. No hay interop nuevo.

**Presentation:**

- `Tokens.xaml`: familia nueva `Board.White` y `Board.Black` (con su `.Color`), con los tonos que se elijan en el design system. No es `Chrome.*` ni `Ink.*`.
- `Icons.xaml`: `Icon.Highlighter` (Lucide `highlighter`) y el ícono de pizarrón con el relleno que cambia según el estado. Su nombre y su forma salen del design system.
- `OverlayWindow.xaml`: dos `InkCanvas` apilados, el de pantalla y el de pizarra, que comparten un mismo `Style` con los behaviors de hoy para no duplicarlos. El de pizarra lleva el fondo `Board.*` y se muestra solo con la pizarra prendida.
- `OverlayPane`: tiene **dos** `OverlayViewModel`, uno por lámina, cada uno con sus trazos y su historial. Dirige las acciones a la lámina visible e implementa `ShowWhiteboard`.
- `FreehandSession` y `LiveStrokeLayer`: con `Highlighter`, el trazo lleva `DrawingAttributes.IsHighlighter = true`, ancho `HighlighterWidth.From(grosor)`, la tinta sólida, sin degradado y sin punta. Mientras se dibuja, se ve ya translúcido.
- `InkCursorTip` gana `bool Highlighter`. `ToolCursorBehavior` pinta la punta ancha y translúcida.
- `ToolbarViewModel`: botón Resaltador entre Lápiz y Láser. Botón Pizarra entre Recortar y Ajustes, enlazado a `IWhiteboard`, con el relleno del ícono según `Mode`.
- `HotkeyActionLabels`: «Resaltador» y «Pizarra». `HotkeyActionRouter`: las dos acciones nuevas.
- `App`: registra `WhiteboardController` en el contenedor.

---

## Plan de implementación

1. **Backlog y spec.** Se agregan la HU-34 (Resaltador) y la HU-35 (Pizarra) a `BACKLOG.md`. Solo documentación.
2. **Design system: el artefacto «Acetato».** Se agregan:
   - el botón Resaltador, que saca el ícono `highlighter` de la lista de reservados;
   - el botón Pizarra con sus tres estados (vacío, relleno blanco y relleno negro);
   - la familia de colores `Board.*`, con las muestras de blanco hueso y gris casi negro lado a lado;
   - el cursor del resaltador;
   - el orden nuevo de la barra, las etiquetas y los atajos.

   El usuario revisa el artefacto y lo aprueba antes de seguir.
3. **Design system: la skill local.** Se refleja en `.claude/skills/acetato-design/` lo aprobado en el paso 2 (`colors_and_type.css`, `README.md`, `preview/` y `ui_kits/toolbar/`). Las dos copias quedan iguales.
4. **Domain.** `ToolKind.Highlighter`, `ToolRing`, `HighlighterWidth`, `WhiteboardMode`, `WhiteboardCycle` y `WhiteboardInk`, con tests.
5. **Resaltador completo.** Incluye el trazo con `IsHighlighter` en `FreehandSession` y en `LiveStrokeLayer`, el cursor, `Icon.Highlighter`, el botón, el atajo `Ctrl+U`, la etiqueta y el router. Tests: atajo por defecto, lugar en el ciclo y `"Highlighter"` guardado y leído en `session.tool`. Prueba rápida: dibujar con el resaltador sobre una frase.
6. **Dos láminas por monitor, sin pizarra visible todavía.** `OverlayWindow.xaml` con el `Style` compartido y el segundo `InkCanvas` oculto. `OverlayPane` con dos `OverlayViewModel` y `ShowWhiteboard`. Para el usuario no cambia nada: todo sigue en la lámina de pantalla. Prueba de humo de la ventana, como `SettingsWindowSmokeTests`.
7. **Pizarra completa.** Incluye los tokens `Board.*`, `WhiteboardController`, el ícono con relleno según el estado, el botón, el atajo `Ctrl+W` y su etiqueta. Tests con lienzos de prueba:
   - el ciclo de colores;
   - que se prende en el monitor del cursor y sigue allí aunque el cursor se mueva;
   - la tinta contraria, y que no cambia con el Degradado activo;
   - que se apaga al salir del modo dibujo y al ocultar;
   - que la hoja de la pizarra conserva lo dibujado al apagar y prender.
8. **Prueba manual** (con permiso del usuario):
   - resaltar con varias tintas y grosores, y comprobar que queda debajo del lápiz y que no se mancha al repasar;
   - prender la pizarra blanca y la negra, dibujar, apagarla y ver que lo de la pantalla vuelve intacto;
   - Deshacer y Limpiar todo en cada lámina;
   - `Esc`, `Ctrl+Alt+D` y Recortar con la pizarra prendida;
   - todo en los dos monitores.
9. **Cierre del spec.** Notas de cierre y diferencias con lo escrito. Si la prueba manual cambió algo visual, se actualizan las dos copias del design system en el mismo orden: primero el artefacto y después la skill local.

---

## Criterios de aceptación

> **Prueba del 2026-11-13.** Pasos 1 a 7 hechos en la rama `spec-14-resaltador-y-pizarra`. `dotnet build -warnaserror` en verde y 678 tests en verde. Prueba en vivo con la app real en dos monitores, con el mouse y el teclado simulados sobre un fondo de prueba, mirando el color de los píxeles de la pantalla.
>
> **Comprobado en vivo:** el resaltador pinta al 50 % y 54 px con el grosor máximo, deja leer el texto, queda debajo del lápiz y repasar con la misma tinta no lo oscurece; la pizarra sale blanca (`244,241,234`) y negra (`27,29,31`), esconde la pantalla y la devuelve intacta, conserva su hoja al apagar y prender, Deshacer y Limpiar solo tocan la hoja visible, la tinta pasa a la contraria, `Esc` y `Ctrl+Alt+E` la apagan, ocultar con `Ctrl+Alt+D` y mostrar la deja apagada, el recorte de Windows la ve prendida, y con dos monitores sale solo en el del cursor y `Ctrl+W` sigue la del monitor donde se prendió. La herramienta Resaltador queda guardada en `settings.json`. El registro no anotó errores ni advertencias.
>
> **Cubierto por tests:** ancho ×3, `IsHighlighter`, tinta sólida sin degradado, 50 % al pintar y capa debajo del lápiz dentro del lienzo; el ciclo de la pizarra, el monitor del cursor, la tinta contraria, el apagado y el recorte; la ventana con sus dos hojas y sus fondos; los atajos `Ctrl+U` y `Ctrl+W` y sus filas en Ajustes.
>
> **Pendiente de mirar con los ojos del usuario (sin marcar):** cómo se ven y dónde quedan los botones Resaltador y Pizarra en la barra y el relleno del ícono (la barra no sale en las capturas de pantalla, a propósito); el cursor del resaltador; Borrador, Mover y Recortar sobre un resaltado; plegar la barra con la pizarra prendida; y cerrar y abrir con la pizarra prendida.
>
> **Diferencias con lo escrito arriba:**
> - **Recortar con la pizarra prendida.** El spec decía a la vez que Recortar captura la pizarra y que salir del modo dibujo la apaga, y Recortar pasa a modo puntero. Ahora la pizarra se queda prendida durante el recorte (`IWhiteboard.KeepThroughNextPointerMode`, que llama el broadcaster mediante `ScreenClipServices`) y cualquier otra salida del dibujo la apaga.
> - **Botón Pizarra desde el modo puntero.** Vuelve primero al modo dibujo, como los botones de herramienta; si la pizarra ya estaba prendida por un recorte, solo vuelve a dibujar y no cambia el color.
> - **Relleno del ícono.** `Icon.WhiteboardFill` lleva dos líneas sin área para compartir límites con el contorno y que los dos `Path` queden alineados. Está en el artefacto y en la skill local.
> - **Ventana del overlay.** `OverlayWindow` usa un contexto `OverlaySheets` (las dos hojas y el modo) en vez del ViewModel suelto. `ResizeEnabled` va directo en cada hoja porque no es una propiedad de dependencia y no puede ir en el estilo compartido.
> - **Código compartido.** `CanvasLocator` (buscar el lienzo bajo el cursor) sale de `OverlayBroadcaster` para que lo use también la pizarra.

**Resaltador**

- [ ] La barra muestra el botón «Resaltador» entre Lápiz y Láser, con su atajo `Ctrl+U` en la pista.
- [x] Con el resaltador, un trazo a mano alzada deja ver lo de abajo, al 50 %.
- [x] Con el grosor de 3 px pinta 9 px, y con el de 18 px pinta 54 px.
- [x] Usa la tinta elegida. Con el Degradado activo, usa la última tinta sólida.
- [x] Un resaltado hecho después de un trazo de Lápiz queda debajo del trazo de Lápiz.
- [x] Repasar varias veces la misma zona con la misma tinta no la oscurece.
- [x] Mientras se dibuja, el trazo ya se ve translúcido, igual que al soltarlo.
- [ ] El cursor muestra el ancho y el color translúcido del resaltador.
- [x] `Ctrl+Espacio` pasa de Lápiz a Resaltador y de Resaltador a Láser.
- [ ] Deshacer, Borrador, Limpiar todo y Recortar funcionan con el resaltado. Mover lo cambia de lugar y de tamaño sin cambiar su grosor.
- [x] Al cerrar Acetato con el resaltador elegido, al abrirlo sigue elegido.
- [x] Ajustes muestra la fila «Resaltador» con `Ctrl+U` y se puede cambiar.

**Pizarra**

- [ ] La barra muestra el botón «Pizarra» entre Recortar y Ajustes, con su atajo `Ctrl+W` en la pista.
- [ ] Cada clic o cada `Ctrl+W` pasa por apagada → blanca → negra → apagada, y el ícono queda vacío, relleno de blanco o relleno de negro.
- [ ] El botón Pizarra nunca usa el acento teal.
- [x] Con dos monitores, la pizarra sale solo en el monitor del cursor, y el otro sigue mostrando lo de siempre.
- [x] Con la pizarra prendida en el monitor 1 y el cursor en el monitor 2, `Ctrl+W` cambia la pizarra del monitor 1.
- [x] La primera vez, la pizarra sale vacía y esconde lo dibujado sobre la pantalla de ese monitor.
- [x] Al apagarla, lo dibujado sobre la pantalla vuelve intacto. Al prenderla de nuevo, lo dibujado en la pizarra sigue ahí.
- [x] Al pasar de blanca a negra, lo dibujado en la pizarra no cambia.
- [x] Con la pizarra prendida, `Ctrl+Z` quita el último trazo de la pizarra y no toca la lámina de pantalla.
- [x] Con la pizarra prendida en el monitor 1, «Limpiar todo» vacía la pizarra y lo dibujado en los demás monitores, y al apagarla vuelve intacto lo que había en la pantalla del monitor 1.
- [x] Con tinta blanca, al prender la pizarra blanca la tinta pasa a negra. Con tinta negra, al pasar a la negra la tinta pasa a blanca.
- [x] Con el Degradado activo, prender la pizarra no cambia la tinta.
- [x] `Esc`, el botón Puntero y `Ctrl+Alt+E` apagan la pizarra, y al prenderla otra vez sigue lo dibujado en ella.
- [x] Tras ocultar con `Ctrl+Alt+D` y volver a mostrar, la pizarra arranca apagada.
- [ ] Plegar la barra con el ojo no apaga la pizarra.
- [x] Recortar con la pizarra prendida captura la pizarra con lo dibujado en ella.
- [ ] Al cerrar y abrir Acetato, la pizarra arranca apagada y vacía.
- [x] Ajustes muestra la fila «Pizarra» con `Ctrl+W` y se puede cambiar.

**General**

- [x] Un `settings.json` de antes de este spec carga sin errores y recibe `Ctrl+U` y `Ctrl+W`.
- [x] `dotnet build -warnaserror` y `dotnet test` en verde, sin reglas suprimidas nuevas.
- [x] Ningún método, archivo ni clase supera los límites de Sonar (40 líneas, 400 líneas, complejidad 10, 5 parámetros, anidamiento 3).
- [x] No hay P/Invoke nuevo. El code-behind solo llama a `InitializeComponent()`.
- [x] Los colores de la pizarra salen de `Board.*` y no se mezclan con `Chrome.*` ni con `Ink.*`.
- [x] Las dos copias del design system describen el Resaltador, la Pizarra con sus tres estados y la familia `Board.*`, y se actualizaron antes del código.

---

## Decisiones tomadas

1. **Resaltador y pizarra en un mismo spec; la presión del lápiz va aparte** (decisión del usuario). Las dos funciones se prueban con mouse. La presión necesita un equipo con lápiz y hoy el trazo no la usa.
2. **El resaltador es una herramienta propia** (decisión del usuario), con botón al lado del Lápiz. Se descartó un interruptor que vuelva translúcida cualquier herramienta: es un modo escondido que se queda prendido sin que el usuario lo note.
3. **El resaltador usa la tinta elegida** (decisión del usuario). Con el Degradado usa la última tinta sólida, como Texto y Láser. Se descartó «siempre amarillo» porque impide resaltar en otros colores.
4. **Se usa el resaltador que ya trae WPF (`IsHighlighter`)** (decisión del usuario). Ya pinta debajo de los demás trazos y no se mancha al repasar. Implica 50 % fijo y que dos pasadas del mismo color no se sumen. Se descartó pintarlo a mano para lograr 40 % y pasadas que se suman: uno o dos días más y más riesgo con Deshacer, Mover y Recortar.
5. **Grosor del resaltador = tres veces el paso elegido.** Un marcador es más ancho que un lápiz. Reutilizar la escala de grosores evita un control nuevo.
6. **La pizarra pasa por apagada → blanca → negra con un solo botón** (decisión del usuario). Es lo más rápido en plena presentación. Se descartaron un menú (un clic más) y solo blanca (en modo oscuro o con proyector se prefiere la negra).
7. **La pizarra tiene hoja propia** (decisión del usuario). Al apagarla vuelve lo dibujado sobre la pantalla. Se descartó que la pizarra solo tape: es más simple, pero obliga a borrar lo de la pantalla para explicar algo aparte.
8. **La hoja de la pizarra dura mientras Acetato esté abierto, y la blanca y la negra la comparten** (decisión del usuario). Se descartaron tres hojas (al cambiar de color desaparece lo dibujado) y borrar la hoja al apagar (se pierde con un clic por error).
9. **Deshacer y Limpiar todo actúan solo sobre la hoja visible** (decisión del usuario). Así nunca se borra lo que no se ve. Se descartó que «Limpiar todo» vacíe las dos hojas.
10. **La pizarra sale solo en el monitor del cursor** (decisión del usuario). El otro monitor suele tener las notas o la presentación. Se descartó tapar todos.
11. **Una hoja de pizarra por monitor.** Sigue la regla que ya existe: cada monitor tiene sus propios dibujos (HU-09).
12. **El botón y el atajo siguen la pizarra prendida, no el cursor.** Así no puede haber dos pizarras a la vez, y `Ctrl+W` siempre apaga la que se ve.
13. **Salir del modo dibujo apaga la pizarra** (decisión del usuario). Quien sale del modo dibujo quiere usar la app de abajo. Se descartó dejarla prendida: se haría clic en una app que no se ve.
14. **Al volver a mostrar Acetato, la pizarra arranca apagada.** Es lo mismo que al salir del modo dibujo: una sola regla.
15. **Tinta contraria automática** (decisión del usuario). Así el primer trazo nunca es invisible. Al apagar la pizarra la tinta no se restaura, para no cambiar algo que el usuario quizás eligió a propósito. Con el Degradado no se toca nada, porque el neón se ve sobre los dos fondos.
16. **El botón Pizarra va entre Recortar y Ajustes** (decisión del usuario). No dibuja, así que no se mezcla con las herramientas.
17. **El estado se ve en el relleno del ícono, sin el acento teal** (decisión del usuario). El teal sigue marcando solo la herramienta activa, como manda la marca.
18. **Blanco hueso y gris casi negro** (decisión del usuario). El blanco puro encandila en proyectores. El tono exacto se elige en el design system.
19. **`Board.*` es una tercera familia de colores.** El fondo de la pizarra no es la barra (`Chrome.*`) ni un trazo (`Ink.*`). Mezclarlo con cualquiera de las dos rompe la regla de las paletas separadas.
20. **Dos láminas apiladas por monitor, cada una con su `OverlayViewModel`.** Se reutilizan sin cambios el lápiz, el texto, Mover, el borrador y el historial. Se descartó cambiar los trazos de una sola lámina al prender la pizarra: los textos viven como elementos dentro del lienzo y habría que moverlos uno por uno.
21. **La pizarra vive en su propio caso de uso (`WhiteboardController`)**, no en `OverlayBroadcaster`. Mantiene cada clase con una responsabilidad y lejos del límite de 400 líneas.
22. **Atajos `Ctrl+U` y `Ctrl+W`.** Están libres y se recuerdan fácil: «subrayar» en Word y *whiteboard*. Solo valen en modo dibujo, como los demás atajos de herramienta, y se pueden cambiar en Ajustes.
23. **El design system se ajusta antes de programar** (decisión del usuario): primero el artefacto, luego la skill local. En los SPEC 12 y 13 se hacía al cierre, y el código terminaba decidiendo el diseño.
24. **Recortar deja la pizarra prendida** (decisión tomada al implementar, para que Recortar la capture y a la vez cualquier otra salida del dibujo la apague). Se descartó apagarla siempre: el recorte no vería la pizarra.
25. **El botón Pizarra, desde el modo puntero, vuelve al dibujo** como los botones de herramienta. Con la pizarra ya prendida por un recorte solo vuelve a dibujar, para no cambiar el color sin que el usuario lo pida.
26. **El relleno del ícono comparte límites con el contorno** mediante dos líneas sin área en `Icon.WhiteboardFill`. Se descartó un `Viewbox` con rejilla de 24: el grosor del trazo no coincidiría con el de los demás íconos de la barra.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Varias partes del código buscan el lienzo por su nombre (`InkSurface`) y dan por hecho que hay uno solo por monitor: por ejemplo, quitar la selección al ocultar o el texto en edición. | En el paso 6, `OverlayPane` expone la lámina visible y se revisan todos los usos de `InkSurface` antes de agregar la segunda. |
| El `Style` compartido quizás no logra enlazar las capas del trazo en vivo y del láser (`LiveLayer`, `LaserLayer`) por su nombre. | Esas dos propiedades se ponen directamente en cada `InkCanvas`. Las dos capas quedan encima de ambas láminas y sirven a las dos. |
| La lámina escondida conserva una selección de Mover o un texto a medio escribir, y reaparece raro al volver. | Antes de cambiar de lámina se confirma el texto en edición (`TryCommitText`) y se quita la selección. La lámina escondida queda `Collapsed`, así que no recibe el mouse. |
| `StyledStroke` pinta el trazo a su manera (por el degradado), y eso podría saltarse la transparencia del resaltador de WPF. | El paso 5 se prueba primero con un resaltado sobre texto real. Si no sale translúcido, `StyledStroke` aplica el 50 % al pintar los trazos con `IsHighlighter`. |
| Dos `OverlayViewModel` por monitor duplican lo que escucha cambios de tinta y herramienta. Si no se liberan, hay fugas al reconstruir las láminas (por ejemplo, al conectar un monitor). | `OverlayPane.Dispose` libera los dos. Va en la prueba manual de conectar y desconectar un monitor. |
| Cambiar la tinta por la contraria también la guarda como «última tinta» de la sesión (SPEC 12). | Es lo esperado: el usuario ve esa tinta y así arranca la próxima vez. Queda escrito para que no parezca un error. |
| `Ctrl+W` en modo dibujo impide cerrar pestañas del navegador con ese atajo. | Pasa igual con `Ctrl+T`, `Ctrl+S` y `Ctrl+Z`, y solo en modo dibujo. Se puede cambiar en Ajustes. |

---

## Lo que **no** está en este spec

- Presión del lápiz (F3).
- Pizarra en todos los monitores a la vez.
- Más colores de pizarra, cuadrícula o renglones.
- Varias páginas de pizarra o guardarla como imagen aparte del recorte.
- Recordar la pizarra o sus dibujos entre sesiones.
- Resaltador recto por arrastre, resaltador con degradado o cambiar su transparencia en Ajustes.
- Un resaltador al 40 % o con pasadas que se suman (descartado en la decisión 4).

Cada uno, si llega, va en su propio spec.
