# UI Kit — Barra flotante de Acetato

High-fidelity, interactive recreation of Acetato's single visible surface: the
floating glass toolbar, shown overlaying a presentation slide (the product's
core use case — annotating live during a talk or tutorial).

> Reference for re-implementation in **WinForms/WPF**. The HTML/JS here exists to
> pin down visuals and interaction; it is not the production code.

## Run
Open `index.html`. Everything is fake but live:
- **Pick a tool** — Seleccionar, Lápiz, Línea recta, Flecha, Rectángulo, Texto, Borrador.
- **Color** — click the ink swatch to open the *Tinta* popover (6 inks).
- **Grosor** — click the thickness glyph for the 5-step stepper.
- **Draw** — freehand, straight lines, arrows and rectangles render on a transparent `<canvas>`.
- **Texto** — pick Texto, click on the slide, and type an annotation in place.
- **Captura** — flashes the screen and shows a "copiada" toast (fake capture).
- **Ajustes** — minimal settings popover: bar size (Compacta/Cómoda), smoothing,
  snap-to-edges, show-shortcuts toggles.
- **Deshacer / Rehacer / Limpiar** — full stroke history.
- **Drag** — grab the grip at the bar's lead edge to move it.
- **Keyboard** — `V P L A R T E` select tools, `Ctrl+P` capture,
  `Ctrl+Z`/`Ctrl+Y` undo/redo, `Del` clear, `Esc` returns to Select.

## Files
| File | Role |
|------|------|
| `index.html` | Mounts React + Babel + Lucide; loads the scripts below. |
| `kit.css` | Local layout/cosmetics; extends the root `colors_and_type.css`. |
| `tools.jsx` | Tool/ink/width catalogs **+ the React-safe `Icon` component**. |
| `DrawSurface.jsx` | The transparent canvas: pointer drawing, shapes, eraser, re-render. |
| `Toolbar.jsx` | The bar: `ToolButton`, `InkPopover`, `ThicknessPopover`, tooltip, drag. |
| `App.jsx` | State, undo/redo stacks, keyboard shortcuts, the backdrop slide. |

## Notes for native devs
- **Glass** = `--chrome-glass` fill + a blur-behind. On Windows use Acrylic/Mica
  (DWM) under the bar; fall back to `--chrome-solid` if composition is off.
- **Icons** are [Lucide](https://lucide.dev) line icons at 20px / 2px stroke.
  Ship the equivalent SVGs as resources (names listed in the root README's
  ICONOGRAPHY section).
- **Active tool** is the only element that uses the accent (fill + glow + ring).
  Everything else stays neutral so the bar reads as "off" at a glance.
- The toolbar is the *only* visible chrome. There is no window frame, no title
  bar, no background panel — just the bar floating over a click-through overlay.
