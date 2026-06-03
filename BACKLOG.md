# Backlog — Overlay de Anotación en Pantalla (Windows)

> Aplicación de escritorio para Windows que muestra una capa transparente sobre toda la pantalla
> para dibujar y anotar encima de cualquier aplicación. Stack objetivo: **C# / .NET 9**.
>
> _Renombra el proyecto a tu gusto en este encabezado._

---

## Convenciones

### Formato de la historia (narrativa)
```
Como <rol>, quiero <acción>, para <beneficio>.
```
Toda historia debe responder a los tres elementos: **quién** (rol), **qué** (acción) y **para qué** (valor).

### Criterios de aceptación (Gherkin / BDD)
```
Escenario: <nombre del escenario>
  Dado <contexto inicial>
  Cuando <acción que ocurre>
  Entonces <resultado verificable>
  [Y <condición adicional>]
```
Los criterios deben ser **observables y medibles** (verdadero/falso), no descripciones vagas.

### Criterios INVEST
Cada historia busca ser: **I**ndependiente · **N**egociable · **V**aliosa · **E**stimable · **P**equeña (Small) · **C**omprobable (Testable).

### Prioridad (MoSCoW)
- **Must** — imprescindible para el MVP.
- **Should** — importante, pero el MVP funciona sin ello.
- **Could** — deseable; va después.
- **Won't (por ahora)** — fuera de alcance de esta versión.

### Estimación (story points, escala Fibonacci)
`1` · `2` · `3` · `5` · `8` · `13` — esfuerzo relativo, no horas.

---

## Épicas

| ID | Épica | Descripción |
|----|-------|-------------|
| EP-01 | Núcleo del overlay | Capa transparente, click-through y ciclo de dibujo básico. |
| EP-02 | Herramientas de dibujo | Colores, grosor, deshacer, formas. |
| EP-03 | Integración con el sistema | Bandeja, atajos globales, multi-monitor. |
| EP-04 | Salida y captura | Guardar la pantalla anotada. |

---

## Historias de usuario

### MVP esencial (Must) — el "caminar mínimo"

---

#### HU-01 — Activar/desactivar el overlay con atajo global
- **Épica:** EP-01 · **Prioridad:** Must · **Estimación:** 5
- **Narrativa:**
  Como usuario, quiero activar y desactivar una capa de dibujo a pantalla completa con un atajo global, para poder anotar sobre cualquier aplicación sin cambiar de ventana ni perder el foco.

**Criterios de aceptación**
```
Escenario: Activar la capa
  Dado que la aplicación está corriendo en segundo plano
  Cuando presiono el atajo global de activación (ej. Ctrl+Alt+D)
  Entonces aparece una capa transparente que cubre toda la pantalla
  Y la capa queda siempre por encima de las demás ventanas.

Escenario: Desactivar la capa
  Dado que la capa de dibujo está activa
  Cuando presiono nuevamente el atajo global
  Entonces la capa desaparece
  Y vuelvo a la aplicación que estaba usando.

Escenario: El atajo funciona sin importar la app en foco
  Dado que tengo cualquier aplicación en primer plano
  Cuando presiono el atajo global
  Entonces la capa se activa sin necesidad de hacer foco previo en la app.
```
**Notas técnicas:** ventana sin bordes `TopMost`, estilos extendidos `WS_EX_LAYERED` + `WS_EX_TOOLWINDOW` (fuera del Alt-Tab). Atajo global con `RegisterHotKey`.

---

#### HU-02 — Click-through cuando no se está dibujando
- **Épica:** EP-01 · **Prioridad:** Must · **Estimación:** 3
- **Narrativa:**
  Como usuario, quiero que cuando no estoy dibujando los clics atraviesen la capa hacia la aplicación de abajo, para seguir trabajando con normalidad sin cerrar la herramienta.

**Criterios de aceptación**
```
Escenario: El modo normal deja pasar los clics
  Dado que la capa está visible pero en modo "normal" (sin dibujar)
  Cuando hago clic en un elemento de la app que está debajo
  Entonces el clic llega a esa app
  Y la capa no lo intercepta.

Escenario: El modo dibujo captura los clics
  Dado que activé el modo dibujo
  Cuando hago clic y arrastro sobre la capa
  Entonces el clic es capturado por la capa para dibujar
  Y no pasa a la app de abajo.
```
**Notas técnicas:** alternar el flag `WS_EX_TRANSPARENT` según el modo (normal = activo, dibujo = inactivo).

---

#### HU-03 — Dibujar trazos a mano alzada
- **Épica:** EP-01 · **Prioridad:** Must · **Estimación:** 5
- **Narrativa:**
  Como usuario, quiero trazar líneas a mano alzada manteniendo presionado el botón del mouse, para resaltar o señalar elementos en pantalla.

**Criterios de aceptación**
```
Escenario: Dibujar un trazo
  Dado que el modo dibujo está activo
  Cuando mantengo presionado el botón izquierdo y muevo el mouse
  Entonces se renderiza una línea continua que sigue el recorrido del cursor.

Escenario: Finalizar el trazo
  Dado que estoy dibujando una línea
  Cuando suelto el botón del mouse
  Entonces el trazo queda fijo en pantalla.

Escenario: Fluidez del trazo
  Dado que dibujo moviendo el mouse rápidamente
  Cuando se renderiza la línea
  Entonces no se perciben saltos ni retardo notorio (latencia objetivo < 50 ms).
```
**Notas técnicas:** prototipar con GDI+; si la fluidez no alcanza, migrar el render a Direct2D (Vortice.Windows / Silk.NET).

---

#### HU-04 — Limpiar toda la pantalla
- **Épica:** EP-01 · **Prioridad:** Must · **Estimación:** 2
- **Narrativa:**
  Como usuario, quiero borrar todos los trazos con una sola tecla, para limpiar la pantalla rápidamente.

**Criterios de aceptación**
```
Escenario: Borrar trazos existentes
  Dado que hay uno o más trazos en pantalla
  Cuando presiono la tecla de limpiar (ej. Esc)
  Entonces se eliminan todos los trazos
  Y la capa queda vacía.

Escenario: Limpiar una capa ya vacía
  Dado que la capa no tiene trazos
  Cuando presiono la tecla de limpiar
  Entonces no ocurre ningún error
  Y la capa permanece vacía.
```

---

### MVP+ (Should) — lo que la vuelve útil de verdad

---

#### HU-05 — Cambiar el color del trazo con atajos
- **Épica:** EP-02 · **Prioridad:** Should · **Estimación:** 3
- **Narrativa:**
  Como usuario, quiero cambiar el color del trazo con atajos de teclado, para diferenciar mis anotaciones.

**Criterios de aceptación**
```
Escenario: Seleccionar un color
  Dado que el modo dibujo está activo
  Cuando presiono el atajo de un color (ej. Ctrl+1 … Ctrl+4)
  Entonces el siguiente trazo que dibuje usará ese color.

Escenario: Los trazos previos conservan su color
  Dado que cambié de color
  Cuando dibujo un nuevo trazo
  Entonces los trazos anteriores mantienen su color original.
```

---

#### HU-06 — Ajustar el grosor del trazo
- **Épica:** EP-02 · **Prioridad:** Should · **Estimación:** 2
- **Narrativa:**
  Como usuario, quiero ajustar el grosor del trazo, para dar más o menos énfasis a lo que resalto.

**Criterios de aceptación**
```
Escenario: Cambiar el grosor
  Dado que el modo dibujo está activo
  Cuando aumento o disminuyo el grosor (atajo o rueda del mouse)
  Entonces el siguiente trazo refleja el nuevo grosor.

Escenario: Límites de grosor
  Dado que el grosor está en su valor mínimo o máximo
  Cuando intento sobrepasarlo
  Entonces el valor se mantiene dentro del rango definido.
```

---

#### HU-07 — Deshacer el último trazo
- **Épica:** EP-02 · **Prioridad:** Should · **Estimación:** 3
- **Narrativa:**
  Como usuario, quiero deshacer el último trazo, para corregir un error sin borrar todo.

**Criterios de aceptación**
```
Escenario: Deshacer con trazos presentes
  Dado que hay varios trazos en pantalla
  Cuando presiono Ctrl+Z
  Entonces se elimina únicamente el último trazo dibujado
  Y los demás permanecen intactos.

Escenario: Deshacer sin trazos
  Dado que no hay trazos en pantalla
  Cuando presiono Ctrl+Z
  Entonces no ocurre ningún error.
```

---

#### HU-08 — Vivir en la bandeja del sistema
- **Épica:** EP-03 · **Prioridad:** Should · **Estimación:** 3
- **Narrativa:**
  Como usuario, quiero que la app viva en la bandeja del sistema, para tenerla disponible sin ocupar espacio en la barra de tareas.

**Criterios de aceptación**
```
Escenario: Inicio en segundo plano
  Dado que inicio la aplicación
  Cuando termina de cargar
  Entonces aparece un ícono en la bandeja del sistema
  Y no se muestra ninguna ventana en la barra de tareas.

Escenario: Menú contextual
  Dado que la app está en la bandeja
  Cuando hago clic derecho en el ícono
  Entonces se muestra un menú con opciones (activar/desactivar dibujo, salir).

Escenario: Salir de la aplicación
  Dado que el menú de la bandeja está abierto
  Cuando elijo "Salir"
  Entonces la app se cierra
  Y libera los atajos globales registrados.
```

---

### Después (Could) — no bloquea el MVP

---

#### HU-09 — Soporte multi-monitor
- **Épica:** EP-03 · **Prioridad:** Could · **Estimación:** 8
- **Narrativa:**
  Como usuario con varios monitores, quiero dibujar en cualquiera de mis pantallas, para anotar donde esté trabajando.

**Criterios de aceptación**
```
Escenario: Cobertura de todas las pantallas
  Dado un setup con varios monitores
  Cuando activo la capa
  Entonces la capa cubre todas las pantallas conectadas.

Escenario: DPI mixto sin desfase
  Dado monitores con distinto escalado DPI
  Cuando dibujo sobre cualquiera de ellos
  Entonces el trazo aparece exactamente bajo el cursor, sin desfase de posición.
```
**Notas técnicas:** declarar la app como Per-Monitor V2 DPI-aware. Es el punto históricamente más problemático en herramientas similares (ej. gInk).

---

#### HU-10 — Barra de herramientas flotante
- **Épica:** EP-02 · **Prioridad:** Could · **Estimación:** 5
- **Narrativa:**
  Como usuario, quiero una barra flotante para elegir herramienta y color con el mouse, para no depender solo de los atajos.

**Criterios de aceptación**
```
Escenario: Mostrar la barra
  Dado que el modo dibujo está activo
  Cuando aparece la capa
  Entonces se muestra una barra con herramientas (color, grosor, borrar, deshacer).

Escenario: Seleccionar una herramienta
  Dado que la barra está visible
  Cuando selecciono una herramienta
  Entonces se aplica al siguiente trazo.

Escenario: Mover la barra
  Dado que la barra estorba el área de dibujo
  Cuando la arrastro a otra posición
  Entonces la barra se reubica y no interfiere con el dibujo.
```

---

#### HU-11 — Dibujar formas (línea, flecha, rectángulo)
- **Épica:** EP-02 · **Prioridad:** Could · **Estimación:** 8
- **Narrativa:**
  Como usuario, quiero dibujar líneas rectas, flechas y rectángulos, para hacer anotaciones más limpias que las de mano alzada.

**Criterios de aceptación**
```
Escenario: Dibujar una flecha
  Dado que seleccioné la herramienta "flecha"
  Cuando hago clic en un punto inicial y arrastro hasta un punto final
  Entonces se dibuja una flecha del punto inicial al final.

Escenario: Dibujar un rectángulo
  Dado que seleccioné la herramienta "rectángulo"
  Cuando hago clic y arrastro
  Entonces se dibuja un rectángulo definido por el área arrastrada.
```

---

#### HU-12 — Guardar la pantalla anotada como imagen
- **Épica:** EP-04 · **Prioridad:** Could · **Estimación:** 5
- **Narrativa:**
  Como usuario, quiero guardar la pantalla con mis anotaciones como imagen, para compartirla después.

**Criterios de aceptación**
```
Escenario: Capturar pantalla anotada
  Dado que tengo trazos sobre la pantalla
  Cuando presiono el atajo de captura
  Entonces se guarda un archivo PNG que contiene el contenido de la pantalla más los trazos.
```

---

## Definition of Done (global)

Una historia se considera **terminada** cuando:

1. Cumple **todos** sus criterios de aceptación.
2. El código compila sin warnings nuevos.
3. Fue probada manualmente en **Windows 11** (y en multi-monitor cuando aplique).
4. Los atajos no chocan con atajos comunes del sistema operativo.
5. No hay fugas de memoria evidentes tras activar/desactivar la capa repetidas veces.
6. El commit sigue **Conventional Commits**.

---

## Roadmap sugerido

1. **Sprint 0 / Spike:** HU-01 → HU-02 → HU-03 → HU-04 (núcleo funcional usable).
2. **Iteración 1:** HU-05 → HU-06 → HU-07 (herramientas básicas de dibujo).
3. **Iteración 2:** HU-08 (integración con el sistema).
4. **Backlog futuro:** HU-09, HU-10, HU-11, HU-12.