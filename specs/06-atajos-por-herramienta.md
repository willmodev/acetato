# SPEC 06 — Atajos por herramienta (HU-22)

> **Estado:** Listo para implementar · **Depende de:** SPEC 05 (HU-21) — atajos configurables · **Fecha:** 2026-10-11
> **Objetivo:** Que cada herramienta de dibujo y el grosor tengan su propio atajo configurable, y que la barra muestre el atajo de cada botón para descubrirlos sin abrir Ajustes.

---

## Por qué existe este spec

- Hoy solo Texto (`Ctrl+T`) y Láser (`Ctrl+L`) se eligen directamente. Para llegar al rectángulo hay que pulsar `Ctrl+Espacio` hasta cuatro veces o ir a la barra.
- El grosor solo se cambia con la rueda del mouse o con el popover; no hay teclado.
- Los atajos solo se ven en Ajustes. Si hay más atajos y no se ven en la barra, nadie los descubre.
- El SPEC 05 dejó las tres cosas fuera de alcance a propósito. La infraestructura (catálogo, coordinador, Ajustes y persistencia) ya existe: este spec solo suma acciones y su presentación.

---

## Alcance

**Dentro:**

- **Cinco acciones nuevas para elegir herramienta**, en el grupo **Solo al dibujar**: Lápiz, Borrador, Línea, Flecha y Rectángulo.
- **Dos acciones nuevas para el grosor**, en el grupo **Solo al dibujar**: Más grueso y Más fino. Usan el paso siguiente o el anterior de la escala actual (con tope en los extremos, igual que la rueda).
- Las siete acciones aparecen en **Ajustes → Solo al dibujar** y se pueden cambiar, quitar y restaurar como las demás (sin código nuevo en Ajustes, solo filas nuevas).
- **Tooltips de la barra con el atajo**: `Lápiz  Ctrl+P`, con el atajo en monoespaciada (`text-kbd`), según el design system. Se actualizan al instante cuando el atajo cambia en Ajustes. Una acción sin atajo muestra solo el nombre. Solo en los botones de la barra; los popovers de tinta y grosor no cambian.
- Alta de **HU-22** en `BACKLOG.md`.

**Fuera de alcance (specs futuros):**

- Atajo para **Seleccionar** (la herramienta aún no existe) y para **Elipse** (HU-16 sin implementar).
- Atajos de **una sola tecla** sin modificador, estilo EpicPen (`P`, `E`…).
- Mostrar atajos en otro sitio que no sea la barra (popovers de tinta y grosor, anillo, ayuda, cheat-sheet).
- Cambios en la **captura de pantalla**: van en el [SPEC 07](./07-recorte-de-pantalla.md).
- Atajos para **Rehacer** (HU-17) o para abrir **Ajustes**.
- Cambiar el **grupo** de una acción.

---

## Modelo de datos

### Atajos por defecto nuevos (propuesta)

| Acción (`HotkeyAction`) | Atajo propuesto | Grupo | Nota |
|-------------------------|-----------------|-------|------|
| `SelectPencilTool` — Lápiz | Ctrl+P | Al dibujar | P de *pencil*; L ya es Láser |
| `SelectEraserTool` — Borrador | Ctrl+B | Al dibujar | B de *borrador* |
| `SelectLineTool` — Línea | Ctrl+I | Al dibujar | `Ctrl+L` ya es Láser; `Ctrl+N` es «nuevo» en casi todas las apps |
| `SelectArrowTool` — Flecha | Ctrl+F | Al dibujar | F de *flecha* |
| `SelectRectangleTool` — Rectángulo | Ctrl+R | Al dibujar | R de *rectángulo* |
| `ThicknessUp` — Más grueso | Ctrl+RePág | Al dibujar | No choca con mover el cursor al escribir un Texto (`Ctrl+↑/↓` sí) |
| `ThicknessDown` — Más fino | Ctrl+AvPág | Al dibujar | En algunos portátiles exige `Fn` |

- Ninguna choca con los predeterminados actuales (`Ctrl+1…7`, `Z`, `Retroceso`, `Espacio`, `T`, `L`, `S`). Un test lo garantiza.
- Como son del grupo Al dibujar, `Ctrl+P`, `Ctrl+F` o `Ctrl+R` solo se toman **en modo dibujo**. Fuera de él (overlay oculto o en click-through), imprimir, buscar y recargar siguen funcionando en las demás apps. Es el mismo trato que ya tienen `Ctrl+S` y `Ctrl+Z`.

### Cambios por capa

- **Application:** 7 valores nuevos en `HotkeyAction` y sus definiciones en `HotkeyCatalog`, en el orden de la barra.
- **Presentation:**
  - `HotkeyActionRouter`: mapear cada acción a `broadcaster.SelectTool(...)` y, para el grosor, a `IncreaseThickness`/`DecreaseThickness` del estado de dibujo (hoy ya existen y los usa la rueda).
  - `HotkeyActionLabels`: los 7 nombres en español.
  - Tooltips: `ToolItem` y los botones fijos de la barra reciben su `HotkeyAction` y un texto de atajo que sale del `HotkeyCoordinator` (`Changed` lo refresca). Plantilla de tooltip con el nombre y la tecla en `Font.Mono`.
- **Infrastructure:** sin cambios (el registro ya es dinámico).
- **Persistencia:** sin cambios de formato. Quien ya tenga `settings.json` recibe los atajos nuevos por defecto, porque el archivo solo guarda diferencias.

---

## Plan de implementación

1. **Backlog.** HU-22 en `BACKLOG.md`. Solo docs.
2. **Acciones y catálogo.** 7 valores nuevos en `HotkeyAction`, definiciones en `HotkeyCatalog` y etiquetas. Tests: unicidad de predeterminados, reglas y la tabla nueva.
3. **Enrutado.** `HotkeyActionRouter` mapea las 7 acciones. **Manual:** en modo dibujo, `Ctrl+R` elige rectángulo y `Ctrl+RePág` engrosa el trazo.
4. **Tooltips con atajo.** Tooltip de la barra con nombre y tecla, actualizado al cambiar el atajo en Ajustes. **Manual:** cambiar el atajo de Deshacer en Ajustes y ver el tooltip nuevo sin reiniciar.

---

## Criterios de aceptación

- [ ] En modo dibujo, `Ctrl+P`, `Ctrl+B`, `Ctrl+I`, `Ctrl+F` y `Ctrl+R` eligen Lápiz, Borrador, Línea, Flecha y Rectángulo, y el botón activo de la barra lo refleja.
- [ ] En modo dibujo, `Ctrl+RePág` y `Ctrl+AvPág` cambian el grosor un paso, con tope en los extremos.
- [ ] Mientras se escribe un Texto, `Ctrl+↑/↓` siguen moviendo el cursor (no se tocan).
- [ ] Con el overlay oculto o en click-through, `Ctrl+P`, `Ctrl+F` y `Ctrl+R` funcionan con normalidad en otras apps.
- [ ] Las 7 acciones aparecen en Ajustes → Solo al dibujar y se pueden cambiar, quitar y restaurar.
- [ ] Un `settings.json` existente del SPEC 05 sigue funcionando y recibe los atajos nuevos.
- [ ] Cada botón de la barra con atajo lo muestra en su tooltip en monoespaciada; sin atajo, solo el nombre.
- [ ] Cambiar un atajo en Ajustes actualiza su tooltip sin reiniciar.
- [ ] `dotnet build -warnaserror` y `dotnet test` verdes; `BACKLOG.md` incluye HU-22.

---

## Decisiones tomadas

1. **Atajo de Línea:** `Ctrl+I`. Libre y sin un uso fuerte en otras apps; `Ctrl+N` («nuevo») y `Ctrl+G` se descartaron.
2. **Grosor:** `Ctrl+RePág` / `Ctrl+AvPág`. `Ctrl+↑/↓` se descartó porque, al escribir un Texto, dejaría de mover el cursor por párrafos.
3. **Tooltips:** solo en la barra. Los popovers quedan para un spec futuro si hacen falta.
4. **Alcance:** herramientas, grosor y tooltips van juntos en este spec (todo reutiliza la infraestructura del SPEC 05). La captura de pantalla va aparte, en el SPEC 07.
