# UI Kit — Barra flotante de Acetato

High-fidelity, interactive recreation of Acetato's visible surfaces — the
floating toolbar and the Settings window — over a presentation slide (the
product's core use case). Reconciled with the WPF app on 2026-10-29.

> Reference for the **WPF** app. The HTML/JS here pins down visuals and
> interaction; it is not the production code.

## Run
Serve the skill folder over HTTP (Babel loads the `.jsx` files with XHR, so
`file://` does not work), e.g. `python -m http.server --directory .claude/skills/acetato-design`,
then open `ui_kits/toolbar/index.html`. Everything is fake but live:
- **Tools** in bar order: Seleccionar (disabled, as in the app), Lápiz, Láser,
  Borrador, Línea, Flecha, Rectángulo, Texto. `Ctrl+Espacio` cycles them.
- **Color** — 6 inks and the round **Degradado** swatch. In Gradient mode each
  stroke takes a random neon pair (never the previous one).
- **Grosor** — 5 steps (3, 5, 8, 12, 18 px); text size follows the step.
- **Draw** — smoothed freehand, freehand arrow with head on release, straight
  line, rounded rectangle, multiline text (Esc or click outside confirms).
- **Láser** — fading trail (2 s) with a halo; leaves nothing behind.
- **Borrador** removes whole strokes. **Deshacer / Limpiar todo** as in the app.
- **Recortar** — only shows a hint: the app opens Windows' Snipping Tool.
- **Ajustes** — the shortcut list: click a key to capture, conflicts offer
  *Reemplazar / Cancelar*, per-row *Restaurar* / *Quitar*, *Restaurar todo* with
  inline confirmation.
- **Cerrar** (`Ctrl+Alt+D`) hides everything; `Ctrl+Alt+E` toggles drawing mode
  (clicks pass through to the slide).
- **Ocultar barra** (eye, right after the grip) folds the bar to grip + eye;
  the eye turns into `eye-off` (*Mostrar barra*) and brings it back. Drawing and
  shortcuts keep working while collapsed. Designed here; pending in the app.
- **Drag** the grip to move the bar (also when collapsed).
- Browsers keep some combos for themselves (`Ctrl+1…7` switch tabs in most), so
  a few default shortcuts may not reach the page; remap them in Ajustes.

## Files
| File | Role |
|------|------|
| `index.html` | Mounts React + Babel + Lucide; loads the scripts below. |
| `kit.css` | Local layout/cosmetics; extends the root `colors_and_type.css`. |
| `tools.jsx` | Catalogs mirrored from the app (tools, hotkeys, inks, gradient pairs, widths, font sizes) **+ the React-safe `Icon` component**. |
| `DrawSurface.jsx` | The transparent canvas: strokes, gradient, eraser, text, laser. |
| `Toolbar.jsx` | The bar: tool buttons, ink and thickness popovers, tooltip with shortcut, drag. |
| `SettingsWindow.jsx` | The Settings window: shortcut capture, conflicts, restore. |
| `App.jsx` | State, configurable shortcuts, visibility and drawing mode, the backdrop slide. |

## Notes for native devs
- **Glass** = `--chrome-glass` fill, **no blur yet** (HU-18 will add
  Acrylic/Mica behind it). Settings is opaque on `--chrome-solid`.
- **Icons** are [Lucide](https://lucide.dev) line icons at 20px / 2px stroke; the
  app ships them as `StreamGeometry` in `Icons.xaml` (see the root README).
- **Accent** marks only what is selected: active tool, chosen thickness, chosen
  ink (ring) and the shortcut being captured. No glow, no ring on the active tool.
- The toolbar is the *only* visible chrome over the overlay: no window frame, no
  title bar, no background panel.
