# SPEC 12 — Redimensionar lo seleccionado y recordar la última sesión (HU-28, HU-29)

> **Estado:** Implementado · **Depende de:** SPEC 11 (HU-14) — Mover, SPEC 10 (HU-27) — bloque Dibujo de Ajustes, SPEC 09 (HU-25) — barra plegable · **Fecha:** 2026-11-02
> **Objetivo:** Cambiar el tamaño de lo seleccionado con Mover, de forma pareja o libre según Ajustes, sin tocar nunca el grosor; y que Acetato arranque como se cerró: misma tinta, grosor, herramienta y barra.

---

## Por qué existe este spec

- **Redimensionar:** hoy, si un rectángulo quedó pequeño, hay que borrarlo y dibujarlo de nuevo. El SPEC 11 dejó la selección sin tiradores porque estirar una flecha deformaba la punta y el grosor. Este spec resuelve eso: el grosor no cambia y la punta se rehace.
- **Recordar:** cada arranque vuelve a Lápiz rojo de 3 px, con la barra arriba al centro. Quien siempre usa la flecha azul gruesa la elige cada vez.

---

## Alcance

### Redimensionar (HU-28)

- Con **Mover**, la selección muestra **8 tiradores** (4 esquinas y 4 lados), los del lienzo de Windows, igual que el recuadro y el lazo del SPEC 11.
- Al pasar sobre un tirador, el cursor es la flecha doble de Windows en esa dirección.
- Jalar un tirador cambia el tamaño **en vivo**, mientras se arrastra.
- **Dos modos**, según el interruptor nuevo **«Transformación libre»** del bloque *Dibujo* de Ajustes:

  | Modo | Interruptor | Qué hace |
  |------|-------------|----------|
  | **Parejo** (predeterminado) | Apagado | Jale del tirador que jale, ancho y alto crecen en la misma proporción. Un círculo sigue siendo círculo. El lado opuesto al tirador queda fijo (desde un lado, el centro de ese lado opuesto). |
  | **Libre** | Encendido | Crece según el tirador: un lado estira solo ese eje y una esquina estira los dos por separado. Un círculo puede volverse óvalo. |

- **El grosor nunca cambia**, en ningún modo: es el que se eligió al dibujar.
- **Flecha:** el cuerpo se estira y la punta se **rehace** al final con su tamaño original (depende del grosor), apuntando en la nueva dirección. No se deforma.
- **Texto:** la letra crece o se encoge para no deformarse. En modo parejo, en la misma proporción. En modo libre, según el área (*supuesto*: un texto estirado solo a lo ancho crece algo, no nada).
- **Tamaño mínimo:** un lado no baja de 8 DIP (si ya era más corto, como el alto de una línea recta, no se encoge más). La selección no se voltea al cruzar el lado opuesto.
- Un **`Ctrl+Z`** devuelve tamaño y sitio de una vez.
- Con **Ctrl** desde otra herramienta no hay tiradores: Ctrl solo mueve (SPEC 11).
- Cada monitor con su selección, como en el SPEC 11.
- El interruptor se guarda en `settings.json` y se aplica al momento.

### Recordar la última sesión (HU-29)

Al cerrar y volver a abrir Acetato, se conservan:

| Qué | Detalle |
|-----|---------|
| Tinta | La última tinta sólida. |
| Degradado | Si el modo Degradado (neón) estaba activo, sigue activo. |
| Grosor | El último paso de la escala. |
| Herramienta | La última, sea cual sea (también Borrador, Láser o Mover). |
| Barra plegada | Si quedó plegada con el ojo, arranca plegada. |
| Posición de la barra | Donde el usuario la dejó al arrastrarla. |

- **Posición:** solo se recuerda si el usuario arrastró la barra. Mientras no la mueva, sale como hoy (arriba al centro del monitor del cursor). Una vez movida, vuelve a su sitio cada vez que se muestra, también dentro de la misma sesión.
- Si el monitor donde quedó la barra **ya no está** (o cambió de resolución), sale como hoy.
- Lo de dibujo se guarda **al cambiar**. La posición de la barra se guarda **al salir** (para no escribir el archivo a cada píxel del arrastre). Si Acetato se cierra a la fuerza, se pierde solo la última posición.
- «Restaurar todo» de Ajustes sigue tocando solo los atajos.
- Un `settings.json` sin esta sección arranca como hoy.

**Fuera de alcance:** rotar; tiradores con estilo de marca; redimensionar con Ctrl o con el teclado; escalar el grosor (decisión del usuario); recordar los dibujos entre sesiones; recordar el monitor del overlay o el modo puntero; un botón para olvidar la sesión.

---

## Modelo de datos y cambios por capa

**`settings.json`** — dos cambios, ambos opcionales al leer:

```json
{
  "drawing": { "inkPairs": false, "freeResize": true },
  "session": {
    "ink": "Blue", "gradient": false, "thickness": 3, "tool": "Arrow",
    "toolbar": { "collapsed": false,
                 "placement": { "monitor": { "x": 0, "y": 0, "width": 2560, "height": 1440 },
                                "x": 812, "y": 64 } }
  }
}
```

Tinta y herramienta se guardan por nombre. Un nombre desconocido, un grosor fuera de rango o una sección dañada valen lo de fábrica, campo por campo.

**Domain:**

- `ResizeHandle` (enum): las 4 esquinas y los 4 lados.
- `SelectionBox` (record struct `X, Y, Width, Height`).
- `SelectionResize.Resize(SelectionBox start, ResizeHandle handle, double dx, double dy, bool free)` → `SelectionBox`. Puro y testeable: modo parejo y libre, lado fijo, mínimo y sin volteo.
- `ToolbarPlacement` (record struct `MonitorInfo Monitor, int X, int Y`, píxeles físicos) con `FindHome(monitores)`: el monitor guardado si sigue igual, o nada.

**Application:**

- `DrawingPreferences` gana `FreeResize` (apagado por defecto).
- `IDrawingSettings`: `FreeResize` y `SetFreeResize`.
- `IToolbarLayout`: `Placement`, `Remember(ToolbarPlacement)` (sin evento: se llama a cada paso del arrastre) y un constructor con el plegado inicial.
- `LastSession` (record) + puerto `ILastSessionStore` (`Load`/`Save`, nunca lanza).
- `IToolbarWindowPlacer` (puerto): leer el recuadro de la barra y moverla en píxeles físicos.
- `SessionMemory` (caso de uso): al arrancar aplica lo guardado al estado de dibujo y a la barra; después guarda en cada cambio de dibujo o de plegado; `Save()` al salir.

**Infrastructure:**

- `DrawingPreferencesJson`: clave `freeResize`.
- `LastSessionJson` + `JsonLastSessionStore`: sección `session`, sin tocar las demás.
- `Win32ToolbarWindowPlacer`: `GetWindowRect` y `SetWindowPlacement` (2 parámetros cada uno, S107). Corrige la diferencia entre coordenadas de pantalla y de área de trabajo con `GetWindowPlacement`, porque al primer `Show` la barra aún no tiene el estilo de ventana de herramientas.

**Presentation:**

- `OverlayWindow.xaml`: `ResizeEnabled="True"` y el modo libre enlazado al behavior.
- `SelectionResizeGesture` (nuevo, lo llama `MoveToolBehavior` para no pasar de 400 líneas): detecta el tirador con `InkCanvas.HitTestSelection`, pone el cursor de flecha doble, hace el arrastre y lo registra.
- `AnnotationResize` (como `AnnotationDrag`): lleva los trazos de un recuadro a otro con una matriz y escala la letra de los textos; al terminar entrega `AnnotationsResized` para el historial.
- `StyledStroke.Transform`: si la matriz no es un simple desplazamiento, rehace la punta con `ArrowHeadBuilder` sobre los puntos nuevos (mismo grosor → mismo tamaño de punta).
- Ajustes: interruptor «Transformación libre» con su pista, debajo de «Tintas degradadas».
- `ToolbarWindowSurface`: al mostrarse usa la posición recordada si su monitor sigue; recuerda la posición solo en arrastres del usuario.
- `App`: arranca `SessionMemory` antes de los atajos y llama `Save()` al salir.

---

## Plan de implementación

1. **Backlog y spec.** HU-28 y HU-29. Solo docs.
2. **Domain.** `SelectionBox`, `ResizeHandle`, `SelectionResize`, `ToolbarPlacement`, con tests.
3. **Punta de flecha al estirar.** `StyledStroke.Transform` rehace la punta; tests.
4. **Redimensionar.** `AnnotationResize`, `AnnotationsResized`, `SelectionResizeGesture`, tiradores en el lienzo; tests de escala parejo/libre, grosor intacto, texto y Deshacer.
5. **Interruptor.** `FreeResize` de punta a punta (preferencias, JSON, Ajustes, overlay); tests.
6. **Recordar la sesión.** `LastSession`, JSON, `SessionMemory`, plegado inicial; tests.
7. **Posición de la barra.** Puerto, adaptador Win32 y `ToolbarWindowSurface`.
8. **Prueba manual** (con permiso del usuario): tiradores, ambos modos, flecha, texto, Deshacer; cerrar y abrir con tinta, grosor, herramienta, plegado y barra movida; en los dos monitores.
9. **Design system** (artefacto «Acetato» y luego la skill local) y cierre del spec.

---

## Criterios de aceptación

> **Cierre (2026-11-02).** Los nueve pasos hechos en la rama `spec-12-redimensionar-y-recordar`. El usuario probó Redimensionar y Recordar en la app real y dio el visto bueno.
>
> **Arreglo aparte en la misma prueba:** a medio centímetro de la barra el cursor del lápiz pasaba a flecha. La sombra de la barra tiene algo de alfa y Windows la contaba como parte de la ventana. `HitAreaBehavior` hace que solo la barra reciba el mouse; en la sombra, el puntero pasa al lienzo. Venía de antes de este spec.
>
> **Cubierto por tests automáticos:** el cálculo del recuadro en modo parejo y libre (lado fijo, mínimo, sin volteo); trazos al recuadro nuevo con el grosor intacto; punta de flecha rehecha con su tamaño; letra del texto (pareja y por área); un Deshacer lo devuelve; el interruptor se guarda junto a *Tintas degradadas*; la sesión completa ida y vuelta en `settings.json`, campo por campo ante datos malos; la memoria aplica la sesión al arrancar y solo escribe si algo cambió; el sitio de la barra se guarda al salir; el monitor guardado se reconoce o se descarta.
>
> **Diferencias con lo escrito arriba:**
> - El gesto se llama `SelectionResizeBehavior` (no `SelectionResizeGesture`): lleva también la propiedad enlazable del modo libre.
> - `ToolbarWindowSurface` recibe la memoria y el posicionador juntos (`ToolbarPlacementServices`) para no pasar de cinco parámetros (S107).
> - `IToolbarLayout` no ganó constructor con el plegado inicial: `SessionMemory` lo pliega al arrancar si hace falta.

**Redimensionar**

- [x] Con Mover, lo seleccionado muestra 8 tiradores y el cursor cambia a flecha doble sobre ellos.
- [x] Interruptor apagado: desde cualquier tirador, crece y se encoge sin deformarse.
- [x] Interruptor encendido: un lado estira solo su eje; una esquina, los dos por separado.
- [x] En los dos modos, el grosor de líneas, formas y flechas no cambia.
- [x] Una flecha estirada conserva el tamaño de su punta, que apunta bien.
- [x] Un texto cambia el tamaño de su letra sin deformarse.
- [x] El cambio se ve en vivo y la selección no se voltea ni desaparece.
- [x] `Ctrl+Z` devuelve tamaño y sitio de una vez.
- [x] El interruptor se recuerda al reiniciar y «Restaurar todo» no lo cambia.

**Recordar**

- [x] Tras cerrar y abrir: misma tinta, Degradado, grosor y herramienta.
- [x] La barra arranca plegada si quedó plegada.
- [x] La barra movida vuelve a su sitio al reiniciar y al ocultar y mostrar la capa.
- [x] Si su monitor ya no está, la barra sale arriba al centro del monitor del cursor.
- [x] Sin la sección `session` en el archivo, arranca como hoy.

**General**

- [x] `dotnet build -warnaserror` y `dotnet test` en verde.
- [x] Las dos copias del design system describen los tiradores, el interruptor y lo que se recuerda.

---

## Decisiones tomadas

1. **El grosor nunca escala** (decisión del usuario). El tamaño de un dibujo y el grosor de su línea son cosas distintas; el grosor se eligió al dibujar.
2. **Dos modos en Ajustes, parejo por defecto** (decisión del usuario). Parejo es lo seguro: no deforma. Se descartó «libre y Mayús para parejo»: el usuario prefiere elegirlo una vez.
3. **Un interruptor, no un selector de dos opciones.** El bloque *Dibujo* ya usa interruptores; uno más no necesita un componente nuevo en el design system.
4. **Arrastre propio sobre los tiradores de Windows.** El lienzo de Windows solo dibuja un rectángulo de guía mientras se jala, no deja forzar la proporción en vivo y escalaría la letra de los textos de forma rara. Se usan sus tiradores (mismo aspecto que el recuadro del SPEC 11) y el arrastre lo hace Acetato.
5. **La punta de la flecha se rehace, no se estira.** Así no se deforma en modo libre y mantiene el tamaño ligado al grosor.
6. **Recordar todo lo que el usuario eligió**, también la herramienta (decisión del usuario), aunque sea Borrador o Láser.
7. **La posición de la barra se guarda en píxeles físicos con su monitor.** En DIP, con monitores de distinta escala, la barra caería en otro sitio (lección del SPEC de multi-monitor).
8. **La posición solo se recuerda si el usuario arrastró la barra.** Si no, sigue apareciendo en el monitor del cursor, que es lo que se espera de alguien que nunca la movió.
9. **La posición se guarda al salir, no en cada movimiento.** El arrastre dispara decenas de eventos por segundo.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Los tiradores de Windows no siguen a la selección mientras se jala. | Al terminar se vuelve a seleccionar (como el arrastre del SPEC 11); se revisa en la prueba manual. |
| Con el cursor propio de Mover, el lienzo no muestra la flecha doble sobre los tiradores. | El gesto pone el cursor él mismo al pasar sobre un tirador. |
| Las coordenadas de `SetWindowPlacement` son de área de trabajo si la barra aún no es ventana de herramientas. | Se calcula la diferencia con `GetWindowPlacement` y `GetWindowRect` antes de mover. |
| `MoveToolBehavior` pasa de 400 líneas. | El redimensionado vive en `SelectionResizeGesture`. |
| Restaurar la herramienta antes de los atajos deja el coordinador desfasado. | `SessionMemory` se aplica antes de arrancar el coordinador. |
