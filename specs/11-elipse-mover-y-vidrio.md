# SPEC 11 — Elipse, Mover y vidrio real (HU-16, HU-14, HU-18)

> **Estado:** Implementado (vidrio real descartado, HU-18) · **Depende de:** HU-11 (formas), SPEC 01 (HU-13) — historial unificado, SPEC 05 (HU-21) — atajos configurables, SPEC 06 (HU-22) — atajos por herramienta, SPEC 08 (HU-24) — modo puntero y Esc fijo, SPEC 09 (HU-26) — cursor de tinta · **Fecha:** 2026-11-01
> **Objetivo:** Cerrar las funciones pendientes del backlog: dibujar elipses, agarrar con una mano lo ya dibujado para moverlo o borrarlo (también con Ctrl desde cualquier herramienta), que Deshacer revierta cualquier acción y probar, con un límite de tiempo, el vidrio real en la barra.

---

## Por qué existe este spec

- Son las últimas historias abiertas del backlog. Con ellas cerradas, el trabajo pasa a la preparación para producción (instalador, una sola instancia, registro de errores).
- **Elipse:** el valor `ToolKind.Ellipse` y el icono `circle` existen desde el principio, pero no se puede dibujar.
- **Mover:** hoy, para corregir algo dibujado hace cinco trazos, hay que deshacer los cinco o borrarlo con el Borrador, y eso no tiene vuelta atrás.
- **Vidrio real:** la barra usa un fondo translúcido de marca. Falta saber si el vidrio de Windows 11 es posible sin perder lo que ya funciona.
- **Rehacer (HU-17) queda descartado** por decisión del usuario. No se implementa.

### Cómo lo hacen otras apps de dibujar en pantalla

Investigación del 2026-11-01, con la que se eligió el modelo de Mover:

| App | Volver al mouse normal | Mover lo dibujado |
|-----|------------------------|-------------------|
| Zoom | Botón «Mouse» | Botón «Select» (mueve y cambia tamaño) |
| Teams | — | Botón de seleccionar |
| Epic Pen | Botón «cursor» | No se puede |
| ppInk (Windows) | Botón «pointer» | Botones «Lasso» (elegir) y «Move» (mover) |
| ScreenBrush (Mac) | Ghost Mode | **Botón con mano**, o **mantener Cmd y arrastrar** desde cualquier herramienta |
| Presentify (Mac) | Interactive Mode | Mantener Cmd y elegir dibujos para mover o borrar |
| ZoomIt | Esc | No se puede |

- El Puntero de Acetato coincide con el «Mouse» de Zoom, el «cursor» de Epic Pen y el «pointer» de ppInk: su flecha está bien puesta.
- **Se adopta el modelo de ScreenBrush** (elección del usuario): botón con mano y tecla mantenida. En Windows la tecla equivalente a Cmd es **Ctrl**.
- Ninguna de estas apps usa la barra espaciadora para mover.

Fuentes: [Zoom](https://support.zoom.com/hc/en/article?id=zm_kb&sysparm_article=0067931) · [Epic Pen](https://epicpen.com/userguide) · [ppInk](https://github.com/pubpub-zz/ppInk) · [ScreenBrush](https://apps.apple.com/us/app/screenbrush/id1233965871) · [Presentify](https://setapp.com/apps/presentify) · [Teams](https://www.uctoday.com/?p=54238) · [ZoomIt](https://learn.microsoft.com/sysinternals/downloads/zoomit)

---

## Alcance

**Dentro:**

### Elipse (HU-16)

- Botón **«Elipse»** en la barra, justo después de Rectángulo, con el icono `circle` (`Icon.Ellipse`).
- Se arrastra y sale una elipse dentro del recuadro del arrastre. Con **Mayús** pulsada sale un círculo; la medida la manda el lado más largo del arrastre.
- La previsualización se ve mientras se arrastra, como en Rectángulo.
- Es **un solo trazo**: Deshacer, Borrador y Limpiar todo la quitan entera.
- Usa la tinta, el grosor, el degradado (neón y *Tintas degradadas*) y el cursor de tinta, igual que Rectángulo. Al ser figura cerrada, el degradado corre en diagonal.
- Atajo predeterminado **`Ctrl+O`** (solo en modo dibujo), configurable en Ajustes.
- Entra en el ciclo de herramientas (`Ctrl+Espacio`) después de Rectángulo.

### Mover (HU-14)

**Botón «Mover»:**

- Va en la barra como **la primera de las herramientas**: después de Puntero y antes de Lápiz. Icono Lucide **`hand`** (mano abierta).
- Atajo predeterminado **`Ctrl+M`** (solo en modo dibujo), configurable en Ajustes.
- **No** entra en el ciclo de herramientas: el ciclo es para dibujar.
- El tooltip del asa de la barra pasa de «Mover» a **«Mover barra»**, para no repetir el nombre.

**Cómo se usa:**

- **Agarrar y arrastrar:** apretar sobre un trazo, una forma o un texto y arrastrar lo mueve en el acto, sin seleccionarlo antes. Al soltar, queda seleccionado.
- **Varios a la vez:** apretar en un sitio vacío y arrastrar dibuja un lazo. Lo que queda dentro se selecciona y se mueve arrastrándolo.
- **Borrar:** **`Supr`** borra lo seleccionado y nada más.
- Clic en un sitio vacío quita la selección.
- **Sin cambiar el tamaño:** la selección no tiene tiradores de tamaño.
- Aplica a **trazos, formas y textos**.
- **Cursor:** mano abierta; mano cerrada mientras se arrastra.
- La selección se pierde al cambiar de herramienta, al pasar a Puntero (`Esc`), al ocultar la capa y al pulsar Limpiar todo.
- Cada monitor tiene su propia selección. No se puede arrastrar algo de un monitor a otro.

**Atajo de mantener Ctrl, desde cualquier herramienta:**

- Con cualquier herramienta de dibujo activa (Lápiz, Láser, Borrador, Línea, Flecha, Rectángulo, Elipse o Texto), **mantener Ctrl y arrastrar** un trazo, una forma o un texto lo mueve.
- Al soltar, sigue la herramienta que estaba. No queda nada seleccionado.
- Con Ctrl pulsada, apretar en un sitio vacío no dibuja nada.
- Con Ctrl pulsada, el cursor pasa a mano en cuanto se mueve el mouse. Al soltar Ctrl, vuelve el cursor de la herramienta.
- Si hay un texto en edición, el primer clic lo confirma como hoy. Ctrl+arrastrar funciona a partir del siguiente.

**`Supr` como atajo fijo:**

- Es un atajo fijo, como `Esc`: no sale en Ajustes.
- Solo se registra en modo dibujo **y** con Mover activa. Fuera de eso, `Supr` llega a las demás apps y al cuadro de texto mientras se escribe.

### Deshacer revierte cualquier acción

- Hoy Deshacer solo quita lo último que se dibujó. Pasa a revertir **la última acción, sea cual sea**:

  | Acción | Qué hace Deshacer |
  |--------|-------------------|
  | Dibujar un trazo, forma o texto | Lo quita (como hoy). |
  | Borrar con el Borrador | Devuelve el trazo, en su sitio y en su misma capa (delante o detrás de los demás). |
  | Mover (con la mano o con Ctrl) | Lo devuelve a donde estaba. |
  | Borrar una selección con `Supr` | Devuelve todo lo que se borró de una vez. |

- **Limpiar todo sigue sin poder deshacerse** (como hoy) y vacía el historial.
- Cada monitor sigue con su propio historial. Deshacer actúa sobre el monitor donde está el cursor.

### Vidrio real en la barra (HU-18) — prueba con límite

- **Una sola prueba de 1 día**, al final del spec, en una rama aparte. No bloquea Elipse ni Mover.
- **Qué se prueba:** reconstruir la ventana de la barra sin transparencia por píxel para que Windows 11 le pinte el acrílico del sistema, con esquinas redondeadas y sombra del propio Windows.
- **Se adopta solo si se cumple todo esto en Windows 11:**
  1. El acrílico se ve con la barra **inactiva** (la barra nunca se activa, para no robar el foco).
  2. La barra sigue sin robar el foco a la app de abajo.
  3. Los popovers de color y grosor, el plegado con el ojo y el arrastre funcionan igual.
  4. La barra sigue fuera de los recortes de pantalla.
  5. La barra sigue encima del dibujo en todos los monitores.
  6. El usuario acepta el radio de esquina de Windows (unos 8 px) en lugar de los 16 px de marca.
- **Si se adopta:** la barra elige el modo al arrancar. Windows 11 22H2 o posterior usa el acrílico; versiones anteriores siguen con el vidrio actual (`Chrome.Glass`).
- **Si no se adopta:** se deshace la rama, la barra queda como hoy y HU-18 pasa a **Descartada** en el backlog, con el motivo.

### Documentación

- `BACKLOG.md`:
  - HU-17 → **Descartada (2026-11-01)**: el usuario no quiere Rehacer.
  - HU-14 pasa a llamarse «Mover anotaciones» y recoge el modelo de mano y Ctrl, con la investigación resumida. Su nota técnica «Mover/borrar deberían integrarse con Rehacer» se cambia por «con Deshacer».
  - HU-16: notas técnicas al día (atajo, ciclo, Mayús).
  - HU-18: resultado de la prueba (adoptada o descartada).
- **Design system**, primero en el artefacto «Acetato» y después en la skill local (las dos copias iguales):
  - Iconos nuevos `hand` (Mover) y `grab` (cursor al arrastrar; en Lucide la mano cerrada se llama `grab`).
  - `circle` deja de estar reservado: es Elipse.
  - `redo-2` se quita de los reservados: Rehacer no se hará.
  - El asa de la barra pasa a llamarse «Mover barra».
  - Orden de la barra con Mover y Elipse.
  - Comportamiento: Elipse, Mover (mano y Ctrl) y «Deshacer revierte cualquier acción, también el Borrador».
  - Se quita «There is no Redo yet (HU-17)».
  - Vidrio: lo que salga de la prueba.

**Fuera de alcance (specs futuros):**

- Rehacer (descartado).
- Cambiar el tamaño o girar lo seleccionado.
- Copiar, cortar, pegar o duplicar (ScreenBrush copia con Option+Cmd; aquí no).
- Mover con las flechas del teclado.
- Cambiar el color o el grosor de algo ya dibujado.
- Mover algo de un monitor a otro.
- Seleccionar todo de golpe.
- Lazo con Ctrl desde otras herramientas: con Ctrl solo se agarra un dibujo.
- Deshacer Limpiar todo.
- Mayús para cuadrados en Rectángulo.
- Un estilo de marca para el lazo y el recuadro de selección (se usa el de Windows).

---

## Modelo de datos y cambios por capa

**`settings.json`** no cambia de formato: solo guarda los atajos que difieren del predeterminado. Las dos acciones nuevas reciben su atajo de fábrica en un archivo existente.

**Domain:**

- `ToolKind.Select` pasa a llamarse **`ToolKind.Move`**. No se guarda en ningún archivo, así que el cambio de nombre no afecta a nadie.
- `ShapeBuilder.Build`: nuevo caso `ToolKind.Ellipse`. Devuelve una polilínea cerrada que aproxima la elipse inscrita en el recuadro `inicio`–`fin`. El número de puntos crece con el tamaño (un punto cada ~4 DIP de perímetro, mínimo 24 y máximo 180), para que se vea curva sin cargar el trazo.
- `ShapeConstraint.Square(StrokePoint start, StrokePoint end)` (nuevo): ajusta `end` para que el recuadro sea cuadrado, con el lado más largo y conservando la dirección del arrastre. Lo usa Elipse con Mayús.
- `ToolRing.Order`: añade `Ellipse` después de `Rectangle`. `Move` no entra.

**Application:**

- `HotkeyAction`: añade `SelectMoveTool`, `SelectEllipseTool` y `DeleteSelection`.
- `HotkeyCatalog.All` (23 → 25), en el orden de la barra:
  - `Drawing(HotkeyAction.SelectMoveTool, HotkeyKey.M)` antes de Lápiz.
  - `Drawing(HotkeyAction.SelectEllipseTool, HotkeyKey.O)` después de Rectángulo.
- `FixedHotkeys`: nuevo `SelectionDelete` (`Supr` → `DeleteSelection`), en un grupo propio `Selection`.
- `ActiveHotkeys.Select` gana un tercer dato, `moveToolActive`. El grupo `Selection` solo entra cuando la capa está en modo dibujo **y** la herramienta es Mover.
- `HotkeyCoordinator` recibe `IDrawingSettings` y se refresca también cuando cambia la herramienta (hoy solo escucha el estado de la capa).
- `OverlayBroadcaster.DeleteSelection()`: lo envía a la pane del monitor del cursor, como Deshacer.
- `IOverlayCanvas` gana `DeleteSelection()`.
- Solo si hace falta (ver paso 7 del plan): puerto `IModifierKeyState` en `Abstractions/` con `bool IsCtrlDown`, para saber si Ctrl está pulsada aunque la capa no tenga el foco.

**Presentation:**

- **Historial por acciones.** `AnnotationHistory` pasa de «lista de anotaciones» a «lista de acciones deshacibles». `IAnnotation` pasa a llamarse `IHistoryEntry`, con un único `Undo()`, y tiene cinco tipos:

  | Entrada | Se registra cuando | `Undo()` |
  |---------|--------------------|----------|
  | `StrokeAdded` | Un trazo llega a la colección. | Lo quita. |
  | `TextAdded` | Se fija un texto. | Lo quita del lienzo. |
  | `StrokeErased` | El Borrador quita un trazo. | Lo vuelve a insertar en su índice original. |
  | `AnnotationsMoved` | Termina un arrastre (con la mano o con Ctrl). | Aplica el desplazamiento inverso a sus trazos y textos. |
  | `SelectionDeleted` | `Supr` borra la selección. | Devuelve sus trazos (en su índice) y sus textos. |

  - Las bajas que llegan de la colección solo cuentan como borrado mientras la herramienta es **Borrador**. Con cualquier otra herramienta siguen siendo el «olvido» silencioso de hoy: así el trazo de previsualización de las formas sigue sin dejar rastro.
  - Para no pasar de 400 líneas, cada entrada va en su archivo y la lógica de sincronizar con la colección queda en `AnnotationHistory`.
- **`StyledStroke`:** sobrescribe `Transform(Matrix, bool)` para mover también la **punta de la flecha** y vaciar la caché del pincel de degradado. Sin esto, al mover una flecha la punta se quedaría en su sitio y el degradado no seguiría al trazo. `Head` deja de ser inmutable (setter privado).
- **`AnnotationDrag`** (nuevo, sin WPF de ventana): mueve un conjunto de trazos y textos por un desplazamiento. Los trazos se mueven con `Stroke.Transform` y los textos con `InkCanvas.SetLeft`/`SetTop`. Al terminar, entrega la entrada `AnnotationsMoved`. Lo usan la mano y Ctrl, para no duplicar código.
- **`MoveToolBehavior`** (attached, sin code-behind):
  - **Con la herramienta Mover:** el lienzo está en `InkCanvasEditingMode.Select` con `ResizeEnabled="False"` y `UseCustomCursor="True"`. Al apretar sobre un dibujo no seleccionado, el behavior lo selecciona y empieza el arrastre con `AnnotationDrag` en el mismo gesto. Sobre un sitio vacío deja que el lienzo dibuje su lazo. El arrastre de una selección existente (`SelectionMoving`/`SelectionMoved` del lienzo) también se registra como `AnnotationsMoved`.
  - **Con Ctrl, desde otra herramienta:** en `PreviewMouseDown` con Ctrl pulsada, busca el dibujo bajo el cursor (`Strokes.HitTest` y los textos del lienzo). Si lo hay, marca el evento como manejado (el Lápiz, las formas, el láser y el texto no lo ven) y arrastra con `AnnotationDrag`. Si no lo hay, marca el evento como manejado y no hace nada más.
  - Quita al lienzo los comandos Cortar, Copiar y Pegar, que el `InkCanvas` trae de serie en modo selección y que el historial no controla.
  - Quita la selección cuando el ViewModel lo pide.
- **Cursores:** `hand` (mano abierta) y `grab` (mano cerrada) como cursores propios, construidos igual que el cursor de tinta (`CursorFile`). Blancos con borde oscuro, para verse sobre cualquier fondo, y a la escala de cada monitor.
- **`ToolCursorBehavior`:** Mover usa la mano abierta. Con Ctrl pulsada, cualquier herramienta muestra la mano abierta, y la mano cerrada mientras se arrastra. Elipse usa el cursor de tinta.
- **`OverlayViewModel`:**
  - `ToEditingMode`: `ToolKind.Move` → `InkCanvasEditingMode.Select`.
  - Avisa al historial cuándo la herramienta es Borrador.
  - Nuevo comando `DeleteSelection`.
  - Al cambiar de herramienta quita la selección.
- **`ShapeDrawingBehavior`:** `IsShapeTool` incluye `Ellipse`. Con Mayús pulsada aplica `ShapeConstraint.Square` antes de construir la forma.
- **Iconos** (`Resources/Icons.xaml`):
  - La clave actual `Icon.Select` (`mouse-pointer-2`, del Puntero) pasa a llamarse `Icon.Pointer`.
  - Nuevos `Icon.Move` (`hand`) e `Icon.Ellipse` (`circle`).
- **Barra:**
  - `ToolbarViewModel.BuildTools`: «Mover» primero y «Elipse» después de «Rectángulo», con su atajo en el tooltip.
  - El asa pasa a «Mover barra».
- **`HotkeyActionRouter`:** las tres acciones nuevas.

**Infrastructure:**

- Sin cambios para Elipse y el botón Mover: `Supr` ya está en la lista blanca de teclas (`HotkeyKey.Delete`).
- Solo si hace falta para Ctrl: `GetAsyncKeyState` en `NativeMethods` y un adaptador `Win32ModifierKeyState`.
- Solo si la prueba de vidrio se adopta:
  - `DwmExtendFrameIntoClientArea` y la estructura `Margins` (un archivo).
  - `DWMWA_WINDOW_CORNER_PREFERENCE` en `NativeMethods`.
  - Un puerto en Application que diga si el sistema soporta el acrílico, con su adaptador en `Win32ToolbarWindowStyler`.

---

## Plan de implementación

1. **Backlog.** HU-17 descartada; HU-14 y HU-16 al día. Solo docs.
2. **Elipse en Domain.** Caso `Ellipse` en `ShapeBuilder`, `ShapeConstraint.Square` y `ToolRing`.
   - Tests: la polilínea cierra; todos los puntos caen en la elipse; el número de puntos respeta el mínimo y el máximo; un arrastre hacia arriba a la izquierda da la misma elipse; `Square` conserva la dirección y toma el lado largo; el ciclo pasa de Rectángulo a Elipse y de ahí a Texto.
3. **Elipse en la app.** Behavior de formas, cursor, icono, botón, atajo `Ctrl+O` y enrutado.
   - Tests: el catálogo tiene la acción con `Ctrl+O`; la barra lista Elipse después de Rectángulo; `CursorTip` aplica a Elipse.
   - **Manual:** elipse y círculo con Mayús; degradado neón y de tinta; Deshacer, Borrador y Limpiar todo; en los dos monitores.
4. **Historial por acciones (sin Mover todavía).** `IHistoryEntry`, las entradas `StrokeAdded`, `TextAdded` y `StrokeErased`, y el aviso de herramienta Borrador.
   - Tests: dibujar, borrar y Deshacer devuelve el trazo en su índice; la previsualización de una forma sigue dejando una sola entrada; texto y trazos se deshacen en orden; Limpiar todo vacía el historial.
   - **Manual:** borrar con el Borrador y recuperar con `Ctrl+Z`.
5. **Mover una flecha sin perder la punta.** `Transform` en `StyledStroke` y `AnnotationDrag`.
   - Tests: la punta se desplaza con el cuerpo; `Clone` conserva la punta movida; el pincel de degradado sigue al trazo; `AnnotationDrag` mueve trazos y textos y su entrada los devuelve.
6. **Botón Mover.** `ToolKind.Move`, modo `Select`, icono `hand` (y cambio de nombre a `Icon.Pointer`), botón, «Mover barra», atajo `Ctrl+M`, cursores de mano y `MoveToolBehavior` para la herramienta.
   - Tests: la barra lista Mover primero; el catálogo tiene `Ctrl+M`; Mover no entra en el ciclo; mover y Deshacer devuelve trazos y textos a su sitio; cambiar de herramienta quita la selección.
   - **Manual:** agarrar y arrastrar sin seleccionar antes; lazo sobre varios; flechas con degradado y textos; cursor de mano abierta y cerrada; `Ctrl+Z`.
7. **Mantener Ctrl desde cualquier herramienta.** Primero se prueba si WPF ve Ctrl pulsada con la capa sin foco (la barra nunca activa la capa). Si no la ve, se añade el puerto `IModifierKeyState` con su adaptador. Después, la parte de Ctrl de `MoveToolBehavior` y del cursor.
   - Tests: con Ctrl, un clic sobre un dibujo no crea trazo y lo mueve; con Ctrl, un clic en vacío no crea nada; sin Ctrl, todo dibuja como hoy; tras soltar, la herramienta no cambió.
   - **Manual:** con Lápiz, Flecha, Elipse, Láser, Borrador y Texto, Ctrl+arrastrar mueve sin dibujar ni borrar; el cursor pasa a mano y vuelve al soltar Ctrl; los atajos `Ctrl+tecla` siguen funcionando.
8. **Borrar con `Supr`.** Atajo fijo en el grupo `Selection`, el coordinador atento a la herramienta, `DeleteSelection` de punta a punta y `SelectionDeleted`.
   - Tests: el grupo `Selection` solo se activa en modo dibujo con Mover; `Supr` no aparece en Ajustes; borrar y Deshacer devuelve todo en su capa; borrar sin selección no hace nada.
   - **Manual:** `Supr` borra solo lo seleccionado; con Texto activo, `Supr` borra letras en el cuadro de texto; en modo puntero, `Supr` funciona en la app de abajo.
9. **Repaso multi-monitor y fugas.** Mover y Deshacer en cada monitor; activar y desactivar la capa 20 veces con selección abierta sin que suba la memoria.
10. **Prueba de vidrio (máximo 1 día, rama aparte).** Comprobar los seis criterios de adopción en Windows 11.
    - Si pasan: el usuario decide si acepta el radio de Windows. Si lo acepta, se integra con el modo elegido al arrancar y el respaldo para Windows 10.
    - Si no pasan: se borra la rama y HU-18 queda descartada con el motivo.
11. **Design system y cierre.** Primero el artefacto «Acetato» y después la skill local. Backlog con el resultado de HU-18. Spec a **Implementado**. Solo docs.

---

## Criterios de aceptación

> **Avance (2026-11-01).** Pasos 1 a 9 y 11 implementados en la rama `spec-11-elipse-mover-y-vidrio`; build `-warnaserror` y tests en verde. La prueba del vidrio (paso 10) quedó lista en la rama aparte `spike-18-vidrio-real`, sin integrar. **Probado por el usuario en la app real (2026-11-02):** Elipse, Mover, Deshacer y General, conforme. La prueba del vidrio (2026-11-02) falló el criterio 1: con la barra inactiva Windows pinta un gris oscuro opaco, no acrílico. HU-18 queda descartada y la rama `spike-18-vidrio-real` no se integra.
>
> **Cubierto por tests automáticos:** la elipse (puntos, cierre, mínimo y máximo, dirección, círculo con Mayús, ciclo, atajo y botón); Deshacer de dibujo, Borrador (a su capa), movimientos y `Supr`; la punta y el degradado de una flecha movida; qué hace cada clic con la mano y con Ctrl (arrastra, pasa al lazo o se queda el clic); `Supr` solo con Mover en modo dibujo y fuera de Ajustes; la selección se suelta al cambiar de herramienta; atajos `Ctrl+M` y `Ctrl+O` y su cesión si ya están usados.
>
> **Diferencias con lo escrito arriba:**
> - La mano cerrada de Lucide se llama `grab`, no `hand-grab` (ya corregido en el texto).
> - El paso 7 no hizo la prueba previa de si WPF ve Ctrl sin foco, porque pedía lanzar la app: se fue directo al puerto `IModifierKeyState` (lee la tecla del sistema), que funciona en ambos casos.
> - La selección se suelta desde la pane (al pasar a Puntero y al ocultarse) y desde el propio behavior (al cambiar de herramienta), no con un aviso del ViewModel. Borrar con `Supr` va de la pane al historial: `MoveToolBehavior.TakeSelection` y `OverlayViewModel.DeleteSelection`.
> - La entrada del Borrador se llama `StrokesErased` (en plural), porque un gesto del Borrador puede quitar varios trazos a la vez.
> - `MoveToolBehavior` escucha en la ventana y no en el lienzo, para recibir el clic con Ctrl antes que el Lápiz, las formas, el láser y el Borrador.
> - Añadido en el repaso del paso 9: si un arrastre se corta a mitad (Alt+Tab, Esc, capa oculta), se cierra como al soltar; antes la mano cerrada quedaba puesta.
> - En el artefacto «Acetato» también se corrigieron restos anteriores a este spec: el ojo ya no figura como pendiente (HU-25) y Seleccionar pasó a Puntero.

**Elipse**

- [x] La barra muestra «Elipse» después de Rectángulo, con el icono de círculo y su atajo en el tooltip.
- [x] Arrastrar dibuja una elipse dentro del recuadro del arrastre, en cualquier dirección.
- [x] Con Mayús pulsada sale un círculo.
- [x] La previsualización sigue al cursor mientras se arrastra.
- [x] La elipse usa la tinta, el grosor y el degradado activos; el cursor es el círculo de tinta.
- [x] Un solo Deshacer quita la elipse entera; el Borrador también.
- [x] `Ctrl+O` elige Elipse en modo dibujo y se puede cambiar en Ajustes.
- [x] `Ctrl+Espacio` pasa de Rectángulo a Elipse y de Elipse a Texto.

**Mover con el botón**

- [x] La barra muestra «Mover» con el icono de mano, primera entre las herramientas. El Puntero sigue con su flecha y el asa dice «Mover barra».
- [x] Apretar sobre un trazo, una forma o un texto y arrastrar lo mueve en el acto; al soltar queda seleccionado.
- [x] Arrastrar desde un sitio vacío dibuja un lazo; lo que queda dentro se mueve junto.
- [x] Lo que no se movió no cambia.
- [x] Una flecha movida conserva su punta y su degradado.
- [x] La selección no tiene tiradores de tamaño.
- [x] El cursor es una mano abierta, y cerrada mientras se arrastra.
- [x] `Supr` borra solo lo seleccionado.
- [x] Con otra herramienta activa, o en modo puntero, `Supr` no hace nada en Acetato y llega a la app de abajo.
- [x] Mientras se escribe un texto, `Supr` borra letras.
- [x] Cambiar de herramienta, pulsar `Esc`, ocultar la capa o pulsar Limpiar todo quita la selección.
- [x] `Ctrl+M` elige Mover en modo dibujo y se puede cambiar en Ajustes; `Supr` no aparece en Ajustes.
- [x] Funciona igual en cada monitor, cada uno con su selección.

**Mover con Ctrl**

- [x] Con cualquier herramienta de dibujo, mantener Ctrl y arrastrar un trazo, una forma o un texto lo mueve sin dibujar ni borrar nada.
- [x] Al soltar, la herramienta sigue siendo la misma y no queda nada seleccionado.
- [x] Con Ctrl, apretar en un sitio vacío no dibuja nada.
- [x] Con Ctrl pulsada, el cursor es una mano; al soltar Ctrl vuelve el de la herramienta.
- [x] Los atajos `Ctrl+tecla` (colores, Deshacer, herramientas) siguen funcionando igual.

**Deshacer**

- [x] Tras borrar un trazo con el Borrador, `Ctrl+Z` lo devuelve en su sitio y en su misma capa.
- [x] Tras mover algo (con la mano o con Ctrl), `Ctrl+Z` lo devuelve a donde estaba.
- [x] Tras borrar una selección con `Supr`, `Ctrl+Z` la devuelve entera.
- [x] Dibujar, mover y borrar mezclados se deshacen en orden inverso, uno por `Ctrl+Z`.
- [x] Tras Limpiar todo, `Ctrl+Z` no hace nada.
- [x] Deshacer sin nada que deshacer no da error.

**Vidrio real**

- [x] La prueba se hizo y su resultado (adoptado o descartado, con el motivo) está en `BACKLOG.md`.
- [x] ~~Si se adoptó: en Windows 11 la barra muestra acrílico estando inactiva, no roba el foco, los popovers, el plegado y el arrastre funcionan, sigue fuera de los recortes y encima del dibujo; en Windows 10 queda el vidrio actual.~~ No aplica: se descartó.

**General**

- [x] `dotnet build -warnaserror` y `dotnet test` en verde.
- [x] Un `settings.json` existente conserva sus atajos y recibe `Ctrl+M` y `Ctrl+O` si están libres.
- [x] Sin subida de memoria tras activar y desactivar la capa 20 veces con selección.
- [x] `BACKLOG.md` marca HU-17 como descartada.
- [x] Las dos copias del design system describen Elipse, Mover y el nuevo Deshacer, sin Rehacer pendiente.

---

## Decisiones tomadas

1. **Rehacer queda fuera** (elección del usuario). HU-17 se marca descartada, no se aplaza.
2. **Vidrio real como prueba de 1 día con condiciones de adopción** (elección del usuario). Hay dudas serias de que Windows pinte el acrílico en una ventana que nunca se activa; una prueba corta lo resuelve sin arriesgar lo demás. Se descartó cerrarla sin probar y se descartó hacerla a cualquier costo.
3. **Modelo de ScreenBrush para mover: botón con mano y tecla mantenida** (elección del usuario, tras investigar siete apps del mismo tipo). Se descartó un botón «Seleccionar» con lazo (Zoom, Teams, ppInk): la mano dice «agarrar y llevar», no deja dos flechas en la barra y la tecla mantenida ahorra ir a la barra en mitad de una explicación. Se descartó la barra espaciadora: ninguna app de este tipo la usa y chocaría con escribir texto y con `Ctrl+Espacio`.
4. **Ctrl como tecla mantenida.** Es la equivalente en Windows de Cmd, que usan ScreenBrush y Presentify. Mantener Ctrl sola no dispara ningún atajo de Acetato, porque todos piden además una tecla.
5. **Con la mano se agarra y se arrastra en el mismo gesto**, sin seleccionar antes, como en ScreenBrush. El lazo queda para elegir varios.
6. **Con Ctrl solo se agarra un dibujo, sin lazo.** Es un atajo rápido; para elegir varios está el botón.
7. **El botón se llama «Mover» y el asa, «Mover barra».** «Mover» describe lo que hace la mano; el asa necesitaba otro nombre para no repetirlo. Se descartó llamar «Mano» al botón: dice el dibujo, no la acción.
8. **`ToolKind.Select` pasa a `ToolKind.Move` y `Icon.Select` a `Icon.Pointer`**, para que cada nombre diga lo que es. Ninguno de los dos se guarda en `settings.json`.
9. **Deshacer revierte cualquier acción, también el Borrador** (elección del usuario). Si mover y `Supr` se deshacen pero el Borrador no, el comportamiento sería incoherente.
10. **Limpiar todo sigue sin deshacerse.** Es lo que hace hoy y lo que dice HU-04. Hacerlo deshacible sería otra decisión de producto.
11. **Mover incluye los textos** (elección del usuario). El lienzo de WPF ya los trata como elementos que se pueden mover.
12. **Solo mover y borrar, sin cambiar el tamaño.** Es lo que pide HU-14. Estirar una flecha deformaría la punta y el grosor.
13. **`Supr` como atajo fijo condicionado**, igual que `Esc`. No depende del foco (la barra nunca activa la capa) y no roba la tecla al escribir texto ni a otras apps.
14. **Atajos `Ctrl+M` (Mover) y `Ctrl+O` (Elipse).** *Supuesto:* son teclas libres en el catálogo y fáciles de recordar («mover», «óvalo»). Se descartaron `Ctrl+A` y `Ctrl+V`, que en modo dibujo le quitarían «seleccionar todo» y «pegar» al cuadro de texto. Se pueden cambiar en Ajustes.
15. **Mover va primera en la barra y fuera del ciclo.** El UI kit del design system ya reservaba ese sitio para la herramienta de selección. El ciclo es para cambiar rápido entre herramientas de dibujo.
16. **Elipse como polilínea cerrada** dentro de la tubería de formas que ya existe. Así Deshacer, Borrador, degradado y recorte funcionan sin cambios.
17. **Mayús para el círculo solo en Elipse.** La historia pide círculos; el cuadrado en Rectángulo queda para otro spec.
18. **Las bajas solo cuentan como borrado con la herramienta Borrador.** Así no se rompe la previsualización de las formas, que hoy quita y pone su trazo a cada movimiento del mouse.
19. **Se quitan Cortar, Copiar y Pegar del lienzo.** El modo selección de WPF los trae de serie y dejarían el historial desfasado.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| WPF no ve Ctrl pulsada cuando la capa no tiene el foco. | Es lo primero que se prueba en el paso 7. Si falla, se lee el estado real del teclado desde Infrastructure con el puerto `IModifierKeyState`. |
| El cursor no cambia a mano al pulsar Ctrl hasta que se mueve el mouse. | Se acepta y está escrito así en el alcance. Lo normal es pulsar Ctrl y luego ir hacia el dibujo. |
| El lienzo de WPF, en modo selección, no deja agarrar y arrastrar en el mismo gesto un dibujo sin seleccionar. | `MoveToolBehavior` hace ese arrastre con `AnnotationDrag` en vez de esperar al lienzo; el lienzo solo pone el lazo. |
| Windows no pinta el acrílico en una ventana inactiva. | Es el criterio 1 de la prueba: si falla, HU-18 se descarta sin tocar la barra actual. |
| El radio de esquina de Windows (unos 8 px) no coincide con los 16 px de marca. | Lo decide el usuario en el paso 10, antes de integrar nada. |
| El recuadro y el lazo de selección de WPF no siguen la marca. | Se acepta para este spec; un estilo propio queda fuera de alcance. |
| Mover una flecha deja la punta atrás, o el degradado no sigue al trazo. | Paso 5, con tests, antes de habilitar Mover. |
| El historial por acciones rompe la previsualización de las formas o el Deshacer del texto. | Paso 4 aislado, con los tests actuales más tests nuevos, antes de Mover. |
| Un `settings.json` existente ya usa `Ctrl+M` o `Ctrl+O` para otra acción. | Se comprueba en los pasos 3 y 6; si choca, la acción nueva queda sin atajo y se puede asignar en Ajustes. |
| Registrar y quitar `Supr` en cada cambio de herramienta deja la tecla tomada si algo falla. | El coordinador ya reaplica el conjunto completo en cada cambio; test de que se libera al salir de Mover y al pasar a modo puntero. |
| `AnnotationHistory`, `OverlayViewModel` o `MoveToolBehavior` se pasan de los límites de tamaño. | Cada entrada del historial va en su archivo; el arrastre vive en `AnnotationDrag`, aparte del behavior. |

---

## Lo que **no** entra en este spec

- Rehacer.
- Cambiar el tamaño, girar, copiar, pegar o duplicar.
- Mover con el teclado o entre monitores.
- Lazo con Ctrl desde otras herramientas.
- Cambiar el color o el grosor de algo ya dibujado.
- Deshacer Limpiar todo.
- Estilo de marca para la selección.

Cada una de esas, si llega, va en su propio spec.
