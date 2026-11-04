# Acetato — Design System

**Acetato** is a native **Windows desktop app (C#/.NET 10, WPF)** that lays a
transparent, click-through layer over *any* application so the user can draw and
annotate on top of it — live, during presentations and tutorials. The name means
*acetate* (the clear film you used to write on with overhead projectors); the
product is exactly that, in software.

The **only** visible element is a **floating, minimal toolbar**. Everything else
is the user's own screen showing through. That single constraint drives the
whole system.

> **Guiding principle #1 — ZERO distraction.** The UI must be discreet, nearly
> invisible when idle, and never steal attention from whatever is underneath it.

This design system is **visual reference for the WPF app.** The HTML/CSS/JS here
locks down look-and-feel and interaction — it is not production code.

## Sources
The **code is the source of truth.** This system was first drafted from the
written spec (June 2026) and reconciled against the app on 2026-10-29:

| What | Where in the repo |
|------|-------------------|
| Tokens (colors, radii, sizes, type) | `src/Acetato.Presentation/Resources/Tokens.xaml` |
| Icons (Lucide paths as `StreamGeometry`) | `src/Acetato.Presentation/Resources/Icons.xaml` |
| Toolbar, tooltip, popover styles | `Resources/ToolbarStyles.xaml`, `Views/ToolbarWindow.xaml` |
| Settings window | `Resources/SettingsStyles.xaml`, `Views/SettingsWindow.xaml` |
| Drawing scales (thickness, text size, laser) | `src/Acetato.Domain/` (`ThicknessScale`, `FontScale`, `LaserFade`) |
| Labels and shortcuts | `ToolbarViewModel`, `HotkeyCatalog`, `HotkeyActionLabels`, `KeyChordText` |

When the app changes, update `Tokens.xaml` first and mirror it in
[`colors_and_type.css`](./colors_and_type.css). Tokens marked **unused** exist in
`Tokens.xaml` but the app does not paint them today. A browsable copy of this
system is published as the private Claude artifact *Acetato Design System*.

---

## The two palettes
Acetato deliberately keeps **two separate color worlds** so the UI never gets
confused with the user's drawing:

1. **Chrome** — the tool's own UI. Dark translucent neutrals **+ one accent**
   (teal). The accent marks *only* what is selected.
2. **Tintas** — the drawing inks: six solids (**red, blue, yellow, green, white,
   black**), six **neon gradient pairs** for Gradient mode and one fixed
   **neighbor per solid** (`--ink-*-end`) for *Tintas degradadas*.
   Saturated and legible over any backdrop, light or dark.

Full token values live in [`colors_and_type.css`](./colors_and_type.css), each
with its XAML key.

---

## CONTENT FUNDAMENTALS

There is almost no copy in Acetato — that is the point. Text appears only in
**tooltips and the Settings window.** Rules:

- **Language: Spanish (es-ES).** Toolbar labels, in bar order: *Mover barra,
  Ocultar barra (Mostrar barra when collapsed), Puntero, Mover, Lápiz, Láser, Borrador, Línea, Flecha, Rectángulo, Elipse,
  Texto, Color, Grosor, Deshacer, Limpiar todo, Recortar, Ajustes, Cerrar.*
- **Voice: none.** The product does not "speak". No first or second person, no
  marketing voice, no encouragement. Labels *name* a thing; they never narrate.
- **One or two words, max.** A tooltip is a noun (*Lápiz*) or a short verb
  phrase (*Limpiar todo*). Never a sentence inside the toolbar.
- **Casing: Sentence case** (*Limpiar todo*, not *Limpiar Todo*). Settings block
  headings too (*Dibujo*, *Siempre activos*, *Solo al dibujar*, *Acerca de*).
- **Settings hints** (one line under an option, `--fg-3`, 11px) may be a short
  phrase with no final period: *Cada tinta pasa a un tono vecino*. Only in
  Settings, never in the toolbar.
- **No punctuation** ending labels. No exclamation marks, ever.
- **No emoji.** Not in UI, not in docs-facing strings. Icons carry meaning.
- **Keyboard hints** are monospace key caps next to the label, written in
  Spanish and joined with `+`: `Lápiz  Ctrl+P`, `Limpiar todo  Ctrl+Retroceso`,
  `Cerrar  Ctrl+Alt+D`, `Puntero  Esc`, `Ocultar barra  Ctrl+Alt+H`. Key names: `Ctrl`, `Alt`, `Mayús`, `Win`,
  `Espacio`, `Retroceso`, `Supr`, `RePág`, `AvPág`, `Inicio`, `Fin`, `Esc`. Actions without a
  shortcut (Mover barra, Color, Grosor, Ajustes) show the name only.
- **Settings messages** are short and concrete: *Ya la usa Lápiz*, *En uso por
  otra aplicación*, *Tecla no admitida*, *Usa Ctrl, Alt o Win*, *Mayús necesita
  otro modificador*, *Reservado por Windows*, *No disponible*, *Sin atajo*,
  *Pulsa la combinación…*
- **Two full sentences, and only two:** the error notice and the closing
  message (see *Errors and About* below). They are the only exceptions to "one
  or two words", because the user has to understand what happened.
- **Vibe:** calm, instrumental, invisible. When in doubt, remove the word.

Examples (good): `Flecha`, `Grosor`, `Limpiar todo`, `Restaurar todo`.
Examples (avoid): `¡Dibuja ahora!`, `Tu lápiz favorito`, `Haz clic para borrar todo 😀`.

---

## VISUAL FOUNDATIONS

**Overall mood.** Dark, translucent, weightless. The bar should feel like a
sliver of acetate hovering over the screen — present enough to use, quiet enough
to forget.

**Color & contrast.** The bar is a near-black translucent fill
(`--chrome-glass`, 72 %) so the user's screen tints through it. Foreground uses a
**white alpha scale** (`--fg-1/2/3` at 96/62/38 %) rather than fixed greys, so it
holds up over light and dark content. `--fg-3` is for disabled icons and hints
only — never text that must be read. The **teal accent** (`#2FD9C4`) is the only
chromatic moment in the UI: the active tool, the chosen thickness and ink, and
the shortcut being captured in Settings. `--danger` appears only in Settings
messages. Inks never appear as UI chrome.

**Transparency & blur.** Today the bar and popovers are **translucent without
blur** (`AllowsTransparency` windows). That is final: real Windows 11 Acrylic
behind the bar (**HU-18**) was tested and dropped, because with the bar inactive
(it never activates, so it never steals focus) Windows paints an opaque dark
grey instead of glass. Previews
here render without blur to match the app. Settings is a normal opaque window on
`--chrome-solid`.

**Type.** Minimal and small. Canonical face **Segoe UI Variable** (Windows 11
system font); web previews substitute **Public Sans**. UI sizes go from 12px
(tooltips, Settings rows and title) down to 11px (Settings headings, text
buttons, messages). Monospace (**Cascadia Code**) only for shortcuts. The text the
user types on screen is a separate scale (`--annot-1…5`, 16–60px, weight 600)
tied to the thickness step.

**Spacing.** 4px base scale (`--s-1…6`). The bar is tight: 6px inner padding,
buttons touching (no gap), 36px square hit targets, 20px icons, dividers with 4px
each side. Popovers pad 12px; Settings pads 20px at the sides.

**Corners.** Soft but not pill-round: toolbar 16px, popovers 14px, buttons 10px,
swatch ring / tooltip / Settings keycap 8px, tooltip kbd and inner ink sample
5px. A full round (`--r-pill`) is used only for the Gradient swatch, because it
is a mode, not an ink, and for the Settings switch.

**Borders & edges.** Every translucent surface gets a **1px hairline**
`--stroke-1`. Dividers between tool groups and Settings rows are a single
`--stroke-2` line. The app paints **no** top inner highlight (`--shadow-inset` is
kept for reference only).

**Elevation / shadows.** The overlay floats high above everything, so shadows are
deep and soft: `--shadow-bar` (bar), `--shadow-pop` (popovers), `--shadow-tip`
(tooltips). The toolbar window keeps a transparent margin so the shadow fits;
clicks on that margin pass through to the app below.

**Backgrounds.** There is **no app background** — Acetato is transparent. No
gradients, images, textures, or patterns in the chrome. Demo surfaces (the split
light/dark stage in the cards) exist only to prove legibility.

**Hover / press / active states.**
- *Hover:* button bg → `--hover`, icon → `--fg-1`.
- *Press:* bg → `--press`. No scale transform.
- *Active (selected tool, thickness):* `--accent-fill` background + `--accent`
  glyph. **No glow and no ring** (`--accent-glow` / `--shadow-active` unused).
  In pointer mode the lit button is *Puntero* and no drawing tool is lit.
- *Selected ink:* 2px `--accent-ring` around the swatch.
- *Disabled:* icon drops to `--fg-3`, no hover (no button uses it today).

**Motion.** Restrained. Popovers use the system's short fade. No bounces, no
infinite loops, no decorative animation. The only custom motion is the laser
trail fading out over `--laser-life` (2 s).

**Layout rules.** The bar is a single floating element, draggable by the grip at
its lead edge, always on top, never in the taskbar, never stealing focus. Once
dragged, it comes back to that spot when shown and after a restart, while its
monitor is still connected (HU-29); otherwise it opens top-center on the
cursor's monitor.

**Drawing mode and pointer mode (HU-24).** The app opens with the layer and the
bar visible, in drawing mode with *Lápiz*; the tray icon stays.
- *Puntero* (first button after the grip, `mouse-pointer-2`) leaves drawing
  mode: clicks reach the windows below while the bar and the strokes stay on
  screen. It closes any open popover. Clicking it again does nothing.
- `Esc` does the same, but only in drawing mode; in pointer mode `Esc` reaches
  the app in focus. While typing a *Texto*, the first `Esc` confirms the text.
  `Esc` is fixed: it is not listed in Settings.
- Back to drawing: click any drawing tool (it becomes the active one) or press
  `Ctrl+Alt+E`. Picking an ink or a thickness keeps pointer mode.
- *Recortar* switches to pointer mode before opening Windows' snipping, so one
  `Esc` cancels it.

**Collapsed bar (eye button, HU-25).** Right after the grip and its divider sits
an eye button that collapses the bar:
- *Expanded:* `eye` icon, tooltip **Ocultar barra**. Click → the bar shrinks to
  **grip · divider · eye** (one 36px button), keeping the same surface
  (`--chrome-glass`, `--stroke-1`, `--r-toolbar`, `--bar-pad`, `--shadow-bar`).
- *Collapsed:* `eye-off` icon, tooltip **Mostrar barra**. Click → the full bar
  comes back. The icon shows the bar's state; the tooltip names the action.
- The grip and the eye **do not move**: the bar's left edge stays put and the
  rest folds toward them. The collapsed bar still drags from the grip.
- Collapsing closes any open popover. It hides only the bar: drawing mode, the
  active tool, ink, thickness, strokes and every shortcut keep working.
- The eye is a regular ToolButton (`--fg-2` resting, `--hover` on hover). It
  **never** uses the accent: collapsed is not a "selected" state.
- Shortcut `Ctrl+Alt+H` (*Plegar barra* in Settings, group *Siempre activos*):
  works in drawing and pointer mode, does nothing while the layer is hidden. If
  the user already gave `Ctrl+Alt+H` to another action, that one keeps it.
- Remembered between sessions (HU-29): if it was collapsed, it starts collapsed.
- No animation beyond the instant resize.
Popovers open directly below their trigger; tooltips too. Settings is the only
other window (480 × 600, own draggable title, *Cerrar* button, *Restaurar todo*
footer with inline confirmation — never a system dialog). *Restaurar todo*
resets shortcuts only, not the *Dibujo* options.

**Acerca de** (HU-33) is Settings' last block, after the shortcuts: a row with
*Acetato 0.9.0* (the screen version, without the commit code) and the text
button *Copiar versión* on the right (copies `0.9.0`). Below it, the text button
*Reportar problema*, left-aligned with the row labels, and its hint (`--fg-3`,
11px): *Crea un .zip en el escritorio para enviarlo a soporte*. While the .zip is
being built the button is disabled (`--fg-3`, no hover, no hand cursor). When it
ends the hint reads *Reporte guardado en el escritorio* and Explorer opens with
the file selected; if it fails, *No se pudo crear el reporte*, still `--fg-3`
(it is not a shortcut error, so no `--danger`). No new colors, no accent.

**Errors and About (HU-32).** Acetato logs every error locally and asks nothing
of the user while it can stay open.

- **Error notice.** After the first interface error of the session Acetato
  stays open and a **native Windows notification** appears, with no buttons and
  no click action. Title *Algo falló, pero Acetato sigue abierto*; text *Si se
  repite, usa Ajustes › Reportar problema*. Once per session; later errors are
  only logged. With Windows notifications off, nothing else happens. Windows
  draws it: no Acetato tokens.
- **Closing message.** For an unrecoverable error (or three interface errors in
  under 10 s), a standard Windows message box with the error icon and OK:
  *Acetato tuvo un problema y se va a cerrar. Se guardó un registro.* Then
  Acetato closes. It is the only system dialog Acetato shows.
- Neither the log nor the report keeps what was drawn, annotation text or
  what is on screen.

**Settings switch** (`SettingsSwitch`, HU-27). For on/off options in the
*Dibujo* block: a 32 × 18 pill (`--switch-w/h`; in the app `Size.Switch.*` and
`Radius.Switch`, half its height) with a 12px knob, right-aligned on
its row. Off: `--chrome-solid-2` track, `--stroke-1` border, `--fg-2` knob on
the left (hover: `--highlight` border). On: `--accent-fill` track,
`--accent-ring` border, `--accent` knob on the right — the same recipe as a
capturing keycap. No animation beyond the instant change. It applies and
saves at once; there is no Save button.

---

## DRAWING (Tintas in use)

- **Solid inks** with `Ctrl+1…6` (red, blue, yellow, green, white, black).
  **Gradient mode** with `Ctrl+7` or the round 7th swatch: each stroke takes a
  random neon pair (never the previous one) and keeps it; the gradient runs from
  the first to the last point (closed shapes: bounding-box diagonal). Applies to
  pencil, line, arrow, rectangle and ellipse; picking a solid ink leaves the mode.
- **Tintas degradadas (HU-27)** — switch in *Ajustes → Dibujo*, off by
  default, remembered between sessions. When on, every solid ink paints a
  gradient from itself to its fixed neighbor on the color wheel (`--ink-*-end`):
  red → pink, blue → indigo, yellow → orange, green → lime; white and black,
  which have no hue, fade to gray. The pair is always the same for each ink (no
  randomness), with the same axis and tools as Gradient mode. The ink swatches
  and the Color button show the pair (135°, start top-left), and so does the ink
  cursor. Text and Láser keep the flat ink. Gradient mode (the round swatch,
  `Ctrl+7`) still picks random neon pairs and ignores the switch; while it is
  on, it wins. Strokes already drawn keep their colors when the switch changes.
  No shortcut. Saved in the `drawing` section of `settings.json`.
  Neighbor hex values are a first pass, to be tuned by eye in the app.
- **Transformación libre (HU-28)** — second switch in *Ajustes → Dibujo*, hint
  *Estira según el tirador. Apagada, crece parejo*. Off by default; decides how
  Mover's handles resize (see Move below). Saved next to *Tintas degradadas*.
- **Remembered between sessions (HU-29):** last ink, Gradient mode, thickness,
  tool (whichever it was), collapsed bar and the bar's dragged spot. Saved in
  the `session` section of `settings.json`; *Restaurar todo* leaves it alone.
  Drawings are not remembered.
- **Thickness** steps `--thick-1…5` (3, 5, 8, 12, 18px), starting at 3.
  `Ctrl+RePág` / `Ctrl+AvPág` move one step and stop at the ends.
- **Pencil / Arrow:** smoothed freehand. Arrow adds an open two-wing head on
  release, aimed by the final stretch, 3 × thickness long.
- **Rectangle:** drag; rounded corners at 2 × thickness (max half the short side).
- **Ellipse (HU-16):** drag; inscribed in the drag box; with `Mayús`, a circle.
  One stroke, with ink, thickness, gradient (diagonal, like the rectangle) and
  the ink cursor. `Ctrl+O`.
- **Text:** click to place; multiline (Enter = new line), Esc or click outside
  confirms. Size from `--annot-*`, weight 600, last solid ink; empty placeholder
  *Texto* in `--fg-3`, no border.
- **Laser:** dot with a 22px halo and a trail that fades in 2 s; last solid ink;
  leaves nothing behind.
- **Eraser** removes whole strokes. **Clear** (`Ctrl+Retroceso`) too, and it
  cannot be undone.
- **Move (HU-14):** first tool of the bar, hand icon (`hand`), `Ctrl+M`. Press
  on a stroke, shape or text and drag: it moves in the same gesture and stays
  selected. From empty space, a lasso picks several. `Supr` deletes the
  selection. Cursor: open hand, closed (`grab`) while
  dragging, light stroke with a dark edge. The selection box is Windows' own
  (no brand style); it drops on tool change, pointer mode and hiding the layer.
- **Resize (HU-28):** the selection shows Windows' 8 handles (4 corners, 4
  sides) with Windows' double arrow over each. Dragging one resizes live. With
  *Transformación libre* off (default) it grows evenly from any handle, never
  distorted; on, it stretches by the handle (a side moves only its axis).
  **Thickness never changes.** An arrow rebuilds its head at its usual size;
  a text scales its letter without distortion. A side never goes below 8 DIP.
  No handles with Ctrl from another tool.
- **Hold Ctrl** and drag moves a drawing from any tool, without drawing or
  selecting; the cursor turns into the hand. Same model as ScreenBrush.
- **Undo** (`Ctrl+Z`) reverts the last action: drawing, erasing with the Eraser
  (the stroke returns to its layer), moving, resizing, or deleting with `Supr`. **No
  Redo**: it was dropped (HU-17).
- `--ink-halo-*` are defined but the app does not paint halos yet.
- **Ink cursor (HU-26).** With Lápiz, Línea, Flecha or Rectángulo the pointer is
  a filled circle: the active ink (in gradient mode, the pair of the next stroke;
  with *Tintas degradadas*, the ink and its neighbor; diagonal start → end), as wide as the thickness with a 6 DIP minimum, and the
  stroke starts at its center. Two 1px rings keep it visible on any background:
  dark inside (`Cursor.Ring.Dark.Color`, black 60 %), light outside
  (`Cursor.Ring.Light.Color`, white 80 %). It is a real Windows cursor, rebuilt
  at once when ink or thickness change and at each monitor's scale. Texto keeps
  the I-beam, Láser hides the pointer, Borrador and pointer mode use the normal
  one.

---

## ICONOGRAPHY

- **Style:** simple line icons — uniform **2px stroke**, round caps/joins, 24px
  grid, displayed at **20px** in the bar (14px in Settings rows).
- **Set: [Lucide](https://lucide.dev)** (ISC). A project choice — Acetato has no
  bespoke set. In the app the paths live in `Icons.xaml` as `StreamGeometry`,
  drawn by a stroked `Path` that inherits the button color. Only the grip is
  filled.
- SVG sources: [`assets/icons/`](./assets/icons/).

  | Action | XAML key | Lucide |
  |--------|----------|--------|
  | Mover barra | `Icon.Grip` | `grip-vertical` (filled) |
  | Ocultar barra (bar expanded) | `Icon.Eye` | `eye` |
  | Mostrar barra (bar collapsed) | `Icon.EyeOff` | `eye-off` |
  | Puntero | `Icon.Pointer` | `mouse-pointer-2` |
  | Mover | `Icon.Move` | `hand` |
  | Mover, cursor while dragging | `Icon.Grab` | `grab` |
  | Lápiz | `Icon.Pencil` | `pencil` |
  | Láser | `Icon.Laser` | `locate` |
  | Borrador | `Icon.Eraser` | `eraser` |
  | Línea | `Icon.Line` | `minus` |
  | Flecha | `Icon.Arrow` | `arrow-up-right` |
  | Rectángulo | `Icon.Rectangle` | `square` |
  | Elipse | `Icon.Ellipse` | `circle` |
  | Texto | `Icon.Text` | `type` |
  | Deshacer | `Icon.Undo` | `undo-2` |
  | Limpiar todo | `Icon.Clear` | `trash-2` |
  | Recortar | `Icon.Capture` | `camera` |
  | Ajustes | `Icon.Settings` | `settings-2` |
  | Cerrar, Quitar | `Icon.Close` | `x` |
  | Restaurar | `Icon.Reset` | `rotate-ccw` |

- **Reserved** (SVGs ready, no feature yet — keep them off the bar): `highlighter`,
  `palette`, `pin`, `pipette`. `circle` became Elipse (HU-16); `redo-2` was
  removed, Redo was dropped (HU-17).
- **Color** and **Grosor** are not icons: Color shows the active ink (its pair
  with *Tintas degradadas*, or the gradient sample) as an 18px circle with a `--highlight` ring; Grosor is a
  custom **3-stacked-lines glyph** (1.5 / 2.5 / 4px).
- **Emoji / unicode as icons: never.**

---

## Index / manifest

Root files:
- [`README.md`](./README.md) — this file.
- [`colors_and_type.css`](./colors_and_type.css) — all design tokens (Chrome + Tintas, type, spacing, shape, elevation, drawing scales), mirrored from `Tokens.xaml`.
- [`SKILL.md`](./SKILL.md) — entry point when used as an Agent Skill.
- `preview/` — the Design System cards (colors, type, spacing, components, brand) + shared `preview.css` / `toolbar.css`.
- `assets/` — logo, isotype and the Lucide SVGs.

UI kits:
- [`ui_kits/toolbar/`](./ui_kits/toolbar/) — **the floating toolbar**, an interactive recreation overlaying a slide. See its [README](./ui_kits/toolbar/README.md).

There is no slide template in this system, so no `slides/` folder.
