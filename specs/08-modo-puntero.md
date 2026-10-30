# SPEC 08 — Modo puntero y arranque en modo dibujo (HU-24)

> **Estado:** Implementado · **Depende de:** HU-02 (click-through), SPEC 05 (HU-21) — atajos configurables, SPEC 07 (HU-23) — recorte de Windows · **Fecha:** 2026-10-29
> **Objetivo:** Que la app arranque lista para dibujar y que el botón Puntero o la tecla `Esc` dejen usar el computador con normalidad sin ocultar la barra ni los trazos.

---

## Por qué existe este spec

- Hoy la app arranca **oculta**: solo aparece el icono de la bandeja. Para dibujar hay que pulsar `Ctrl+Alt+D` o elegir «Activar dibujo» en la bandeja.
- El modo «los clics pasan a la app de abajo» ya existe (HU-02), pero solo se alcanza con `Ctrl+Alt+E`. Nadie lo descubre.
- El primer botón de la barra (icono `mouse-pointer-2`, «Seleccionar») está **apagado**: era un hueco reservado para HU-14. Es el lugar natural para el puntero, como en EpicPen.
- `Esc` es la tecla universal para «salir». Hoy, en modo dibujo, `Esc` le llega a la app de abajo (p. ej. termina la presentación de PowerPoint) y Acetato no hace nada con ella.

---

## Alcance

**Dentro:**

- **Arranque en modo dibujo.** Al abrir la app se muestran la capa y la barra, en modo dibujo, con la herramienta inicial (Lápiz). El icono de la bandeja se queda.
- **Botón Puntero** (primer botón de la barra, icono `mouse-pointer-2`):
  - Se habilita y se llama **«Puntero»**. Tooltip: `Puntero  Esc`.
  - Un clic sale del modo dibujo: los clics pasan a las ventanas de abajo. La barra y los trazos **siguen visibles**.
  - En modo puntero el botón Puntero lleva el acento teal y ninguna herramienta de dibujo se resalta.
  - Un clic en Puntero estando ya en modo puntero no hace nada.
  - Al pasar a puntero (botón o `Esc`) se cierran los popovers de tinta y grosor si están abiertos.
  - Si se pasa a puntero a mitad de un trazo (botón del mouse apretado), el trazo queda tal como iba.
- **Volver a dibujar** desde el modo puntero:
  - Clic en cualquier herramienta de la barra (Lápiz, Láser, Borrador, Línea, Flecha, Rectángulo, Texto): entra en modo dibujo con esa herramienta.
  - `Ctrl+Alt+E` (Modo dibujo), como hoy: vuelve con la última herramienta.
  - Elegir tinta o grosor en la barra **no** cambia el modo.
  - Los atajos del grupo «Solo al dibujar» (`Ctrl+P`, `Ctrl+R`…) siguen sin funcionar en modo puntero, como hoy.
- **Tecla `Esc`** hace lo mismo que el botón Puntero:
  - Solo se toma con la capa visible y en modo dibujo. En modo puntero, con la capa oculta o con Ajustes abierto, `Esc` llega normal a las demás apps.
  - Es **fija**: no aparece en Ajustes y no se puede cambiar.
  - Mientras se escribe un Texto, el primer `Esc` confirma el texto y se sigue en modo dibujo. El siguiente `Esc` pasa a puntero.
- **Recortar pasa a puntero.** El botón de la cámara y su atajo (`Ctrl+S`) pasan primero a modo puntero y luego abren el recorte de Windows. Así un solo `Esc` cancela el recorte.
- **Documentación:**
  - Alta de **HU-24** en `BACKLOG.md`.
  - Nota en HU-08: el escenario «Inicio en segundo plano» cambia (ahora arranca visible).
  - Nota en HU-14: el botón `mouse-pointer-2` pasa a Puntero; la herramienta Seleccionar tendrá su propio botón e icono cuando se implemente.
  - Design system: `README.md` del skill y `ui_kits/toolbar/README.md` describen Puntero en lugar de «Seleccionar, apagado».

**Fuera de alcance (specs futuros):**

- La herramienta **Seleccionar** de HU-14 (marcar y mover trazos) y su botón.
- El **botón de ojo** (Ocultar barra / Mostrar barra) del design system.
- Hacer `Esc` configurable, o cualquier otro atajo de una sola tecla.
- Arrancar la app con Windows, o recordar el modo entre sesiones.
- Cambiar el botón **Cerrar** (sigue ocultando capa y barra) o el menú de la bandeja.
- Actualizar la maqueta interactiva del design system (`ui_kits/toolbar/*.jsx`).

---

## Modelo de datos y cambios por capa

No hay datos nuevos que se guarden. El modo (dibujo o puntero) ya existe en `IOverlayController.IsDrawingMode`; este spec solo lo expone en la barra y lo cambia desde más sitios.

- **Domain:**
  - `HotkeyKey.Escape` (nuevo valor). Lo usa solo el atajo fijo; `KeyChordRules` sigue rechazando `Esc` sin modificador en Ajustes, así que no hay choque posible con el mapa del usuario.
  - `ToolKind.Select` **no se toca**: sigue reservado para HU-14. El Puntero no es una herramienta de dibujo.
- **Application:**
  - `HotkeyAction.EnterPointerMode` (nuevo valor). **No** entra en `HotkeyCatalog` (por eso no sale en Ajustes ni se guarda en `settings.json`).
  - Atajo fijo `Esc` → `EnterPointerMode`, sumado por `ActiveHotkeys.Select` solo cuando se incluyen los de dibujo.
  - `OverlayBroadcaster`:
    - `LaunchScreenClip()` llama antes a `IOverlayController.SetDrawingMode(false)`.
    - `TryCommitText()`: confirma el texto en edición del lienzo que lo tenga y devuelve si había uno.
  - `IOverlayCanvas.TryCommitText()` (nuevo método del puerto).
- **Infrastructure:**
  - `HotkeyKeyMap`: `HotkeyKey.Escape` → `VK_ESCAPE` (constante en `NativeMethods`).
- **Presentation:**
  - `ToolbarViewModel`:
    - Quita `Select` de `Tools`. El Puntero pasa a ser un botón fijo antes de la lista, con su `PointerHint`, un `EnterPointerModeCommand` y una propiedad `IsPointerMode`.
    - Escucha `IOverlayController.StateChanged` para refrescar el resaltado: en modo puntero ninguna `ToolItem` está seleccionada y los popovers se cierran.
    - `PickTool` elige la herramienta y además llama a `SetDrawingMode(true)`.
  - `ToolbarShortcuts`: un tooltip con tecla fija (`Esc`) que no depende del mapa.
  - `HotkeyActionRouter`: `EnterPointerMode` → si `broadcaster.TryCommitText()` es falso, `controller.SetDrawingMode(false)`.
  - `OverlayPane` / comportamiento de Texto: implementan `TryCommitText()` reutilizando el «fijar texto» que ya usa `Esc` hoy.
  - `App.OnStartup`: tras conectar atajos y bandeja, muestra la capa (`controller.Toggle()`).
  - `ToolbarWindow.xaml`: botón Puntero con el estilo de los botones de herramienta y resaltado por `IsPointerMode`.

---

## Plan de implementación

1. **Backlog.** HU-24 en `BACKLOG.md`, notas en HU-08 y HU-14. Solo docs.
2. **Arranque en modo dibujo.** `App.OnStartup` muestra la capa al final. **Manual:** abrir la app y dibujar sin tocar nada; `Ctrl+Z` funciona desde el primer momento.
3. **Botón Puntero.** Botón fijo habilitado, `IsPointerMode`, resaltado y vuelta al dibujo al elegir herramienta. Tests del ViewModel: Puntero apaga el modo dibujo; elegir herramienta lo enciende; tinta y grosor no lo cambian; resaltado correcto en cada modo; pasar a puntero cierra los popovers. **Manual:** clic en Puntero y clic en una ventana de abajo.
4. **`Esc` fijo.** `HotkeyKey.Escape`, su tecla virtual, `EnterPointerMode`, el atajo fijo en `ActiveHotkeys` y el enrutado. Tests: `Esc` solo está activo en modo dibujo y no aparece en el catálogo ni en Ajustes. **Manual:** `Esc` en modo dibujo pasa a puntero; un segundo `Esc` llega a la app de abajo.
5. **`Esc` con Texto en edición.** `TryCommitText()` en el puerto, el lienzo y el broadcaster. Tests: con texto en edición, `Esc` lo confirma y no cambia el modo. **Manual:** escribir un Texto, `Esc`, `Esc`.
6. **Recortar pasa a puntero.** `LaunchScreenClip()` apaga el modo dibujo antes de abrir. Test: tras recortar, el modo es puntero. **Manual:** Recortar y cancelar con un solo `Esc`.
7. **Design system.** Puntero en `README.md` del skill (tabla de iconos, estado deshabilitado, orden de la barra) y en `ui_kits/toolbar/README.md`. Solo docs.

---

## Criterios de aceptación

> **Cierre (2026-10-30):** verificados con tests automáticos: tooltip «Puntero  Esc», resaltado en cada modo, cierre de popovers, vuelta al dibujo al elegir herramienta, tinta y grosor sin cambio de modo, Esc registrado solo en modo dibujo y fuera de Ajustes, Esc con texto en edición que solo confirma, y Recortar que pasa a puntero antes de abrir. Build `-warnaserror` y tests verdes. En la app real el usuario probó el arranque en modo dibujo, el Puntero y la vuelta al Lápiz en los dos monitores. Durante esa prueba salió un fallo previo (HU-09/HU-10): la barra arrastrada a otro monitor quedaba tapada por su lienzo. Se corrigió en esta rama reamarrando la barra a la pane del monitor donde queda. Quedan sin probar en la app real los criterios sin marcar.

- [x] Al abrir la app aparecen la capa y la barra en modo dibujo, con el Lápiz activo, sin pulsar nada; el icono de la bandeja está.
- [x] El primer botón de la barra está habilitado, se llama «Puntero» y su tooltip muestra `Esc` en monoespaciada.
- [x] Clic en Puntero: los clics llegan a las ventanas de abajo; la barra y los trazos siguen visibles.
- [x] En modo puntero el botón Puntero lleva el acento teal y ninguna herramienta de dibujo está resaltada.
- [x] Con el popover de tinta o de grosor abierto, pasar a puntero (botón o `Esc`) lo cierra.
- [ ] Pulsar `Esc` a mitad de un trazo deja el trazo tal como iba y pasa a puntero.
- [x] En modo puntero, clic en una herramienta de la barra vuelve al modo dibujo con esa herramienta.
- [ ] En modo puntero, `Ctrl+Alt+E` vuelve al modo dibujo con la última herramienta.
- [x] En modo puntero, elegir tinta o grosor en la barra no cambia el modo.
- [x] En modo dibujo, `Esc` hace lo mismo que el botón Puntero.
- [x] En modo puntero, con la capa oculta o con Ajustes abierto, `Esc` llega a la app en foco (p. ej. termina una presentación de PowerPoint).
- [ ] Escribiendo un Texto, el primer `Esc` lo confirma y se sigue en modo dibujo; el segundo pasa a puntero.
- [ ] Recortar (botón o `Ctrl+S`) deja Acetato en modo puntero, y un solo `Esc` cancela el recorte de Windows.
- [x] `Esc` no aparece en Ajustes; un `settings.json` existente sigue funcionando sin cambios.
- [x] `dotnet build -warnaserror` y `dotnet test` verdes; `BACKLOG.md` incluye HU-24 y las notas en HU-08 y HU-14.

---

## Decisiones tomadas

1. **Nombre «Puntero», no «Seleccionar».** El botón no selecciona trazos; «Seleccionar» queda para HU-14 con otro botón e icono.
2. **El Puntero no es una `ToolKind`.** Es un modo de la capa, no una herramienta de dibujo. Mezclarlo con `ToolKind.Select` habría roto el anillo de herramientas (`Ctrl+Espacio`) y obligado a renombrar algo reservado para HU-14.
3. **Volver a dibujar solo al elegir herramienta o con `Ctrl+Alt+E`.** Tinta y grosor no cambian el modo, para poder preparar el color sin perder el mouse.
4. **`Esc` fijo y fuera de Ajustes.** Es la convención de «salir» y evita abrir una excepción en las reglas de atajos (que prohíben teclas sueltas). Se descartó una fila «Puntero» configurable.
5. **`Esc` solo en modo dibujo.** En modo puntero se suelta, así que llega a las demás apps. En PowerPoint el primer `Esc` saca del dibujo y el segundo termina la presentación.
6. **Con Texto en edición, `Esc` confirma primero.** Se respeta la costumbre actual; se descartó confirmar y salir con un solo `Esc`.
7. **Recortar pasa a puntero antes de abrir.** Si no, Acetato se queda con el `Esc` y cancelar el recorte pide dos pulsaciones. Al terminar se queda en puntero; para seguir dibujando, clic en una herramienta.
8. **Clic en Puntero estando en puntero no hace nada.** No es un interruptor; evita volver al dibujo por un doble clic accidental.
9. **Pasar a puntero cierra los popovers.** La pantalla queda limpia para usar el computador; se descartó dejarlos abiertos.
10. **`Esc` a mitad de un trazo lo conserva.** El trazo queda como iba; se descartó descartarlo.
11. **Arranque siempre en modo dibujo, con bandeja.** Se descartó quitar el icono de la bandeja: es la única vía a Salir y Ajustes con la barra cerrada.
12. **Definición rápida.** El usuario pidió generar el spec completo de una vez, sin confirmar sección por sección; las dudas abiertas van al final.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Otra app ya tiene `Esc` como atajo global y el registro falla. | El botón Puntero sigue funcionando. Sin aviso: `Esc` no está en Ajustes y no hay nada que reasignar. |
| Pulsar `Esc` a mitad de un trazo (botón del mouse aún apretado): el lienzo pierde el mouse de golpe y el trazo podría quedar a medias o perderse. | Se comprueba en el paso 4; si el trazo se pierde, se fija antes de cambiar de modo. |
| Al arrancar, la capa tapa el escritorio y el usuario no sabe salir. | El tooltip del Puntero muestra `Esc`, y la barra está a la vista desde el primer segundo. |

---

## Lo que **no** entra en este spec

- La herramienta Seleccionar de HU-14 y su botón.
- El botón de ojo (Ocultar barra / Mostrar barra).
- `Esc` configurable u otros atajos de una sola tecla.
- Arrancar con Windows o recordar el modo entre sesiones.
- Cambios en Cerrar, en la bandeja o en la maqueta interactiva del design system.
