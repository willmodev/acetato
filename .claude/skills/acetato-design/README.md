# Acetato — Design System

**Acetato** is a native **Windows desktop app (C#/.NET 9, WPF)** that lays a
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
   black**) and six **neon gradient pairs** for Gradient mode. Saturated and
   legible over any backdrop, light or dark.

Full token values live in [`colors_and_type.css`](./colors_and_type.css), each
with its XAML key.

---

## CONTENT FUNDAMENTALS

There is almost no copy in Acetato — that is the point. Text appears only in
**tooltips and the Settings window.** Rules:

- **Language: Spanish (es-ES).** Toolbar labels, in bar order: *Mover,
  Ocultar barra (Mostrar barra when collapsed), Puntero, Lápiz, Láser, Borrador, Línea, Flecha, Rectángulo, Texto, Color,
  Grosor, Deshacer, Limpiar todo, Recortar, Ajustes, Cerrar.*
- **Voice: none.** The product does not "speak". No first or second person, no
  marketing voice, no encouragement. Labels *name* a thing; they never narrate.
- **One or two words, max.** A tooltip is a noun (*Lápiz*) or a short verb
  phrase (*Limpiar todo*). Never a sentence inside the toolbar.
- **Casing: Sentence case** (*Limpiar todo*, not *Limpiar Todo*). Settings block
  headings too (*Siempre activos*, *Solo al dibujar*).
- **No punctuation** ending labels. No exclamation marks, ever.
- **No emoji.** Not in UI, not in docs-facing strings. Icons carry meaning.
- **Keyboard hints** are monospace key caps next to the label, written in
  Spanish and joined with `+`: `Lápiz  Ctrl+P`, `Limpiar todo  Ctrl+Retroceso`,
  `Cerrar  Ctrl+Alt+D`, `Puntero  Esc`. Key names: `Ctrl`, `Alt`, `Mayús`, `Win`,
  `Espacio`, `Retroceso`, `Supr`, `RePág`, `AvPág`, `Inicio`, `Fin`, `Esc`. Actions without a
  shortcut (Mover, Ocultar barra, Mostrar barra, Color, Grosor, Ajustes) show
  the name only.
- **Settings messages** are short and concrete: *Ya la usa Lápiz*, *En uso por
  otra aplicación*, *Tecla no admitida*, *Usa Ctrl, Alt o Win*, *Mayús necesita
  otro modificador*, *Reservado por Windows*, *No disponible*, *Sin atajo*,
  *Pulsa la combinación…*
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
blur** (`AllowsTransparency` windows). Real Acrylic/Mica blur behind the bar is
backlog **HU-18**; when it lands it goes under the same `--chrome-glass`. Previews
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
is a mode, not an ink.

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
its lead edge, always on top, never in the taskbar, never stealing focus.

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

**Collapsed bar (eye button — designed, pending in the app).** Right after the
grip and its divider sits an eye button that collapses the bar:
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
- No shortcut and no memory between sessions: the bar always starts expanded.
- No animation beyond the instant resize.
Popovers open directly below their trigger; tooltips too. Settings is the only
other window (480 × 600, own draggable title, *Cerrar* button, *Restaurar todo*
footer with inline confirmation — never a system dialog).

---

## DRAWING (Tintas in use)

- **Solid inks** with `Ctrl+1…6` (red, blue, yellow, green, white, black).
  **Gradient mode** with `Ctrl+7` or the round 7th swatch: each stroke takes a
  random neon pair (never the previous one) and keeps it; the gradient runs from
  the first to the last point (closed shapes: bounding-box diagonal). Applies to
  pencil, line, arrow and rectangle; picking a solid ink leaves the mode.
- **Thickness** steps `--thick-1…5` (3, 5, 8, 12, 18px), starting at 3.
  `Ctrl+RePág` / `Ctrl+AvPág` move one step and stop at the ends.
- **Pencil / Arrow:** smoothed freehand. Arrow adds an open two-wing head on
  release, aimed by the final stretch, 3 × thickness long.
- **Rectangle:** drag; rounded corners at 2 × thickness (max half the short side).
- **Text:** click to place; multiline (Enter = new line), Esc or click outside
  confirms. Size from `--annot-*`, weight 600, last solid ink; empty placeholder
  *Texto* in `--fg-3`, no border.
- **Laser:** dot with a 22px halo and a trail that fades in 2 s; last solid ink;
  leaves nothing behind.
- **Eraser** removes whole strokes. **Undo** (`Ctrl+Z`) and **Clear**
  (`Ctrl+Retroceso`) work on whole strokes too. There is no Redo yet (HU-17).
- `--ink-halo-*` are defined but the app does not paint halos yet.

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
  | Mover | `Icon.Grip` | `grip-vertical` (filled) |
  | Ocultar barra (bar expanded) | `Icon.Eye` *(pending)* | `eye` |
  | Mostrar barra (bar collapsed) | `Icon.EyeOff` *(pending)* | `eye-off` |
  | Puntero | `Icon.Select` | `mouse-pointer-2` |
  | Lápiz | `Icon.Pencil` | `pencil` |
  | Láser | `Icon.Laser` | `locate` |
  | Borrador | `Icon.Eraser` | `eraser` |
  | Línea | `Icon.Line` | `minus` |
  | Flecha | `Icon.Arrow` | `arrow-up-right` |
  | Rectángulo | `Icon.Rectangle` | `square` |
  | Texto | `Icon.Text` | `type` |
  | Deshacer | `Icon.Undo` | `undo-2` |
  | Limpiar todo | `Icon.Clear` | `trash-2` |
  | Recortar | `Icon.Capture` | `camera` |
  | Ajustes | `Icon.Settings` | `settings-2` |
  | Cerrar, Quitar | `Icon.Close` | `x` |
  | Restaurar | `Icon.Reset` | `rotate-ccw` |

- **`Icon.Eye` / `Icon.EyeOff` for `Icons.xaml`** (circle turned into arcs, as
  the other icons):
  - `Icon.Eye`: `M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0 M9 12 A3 3 0 1 0 15 12 A3 3 0 1 0 9 12 Z`
  - `Icon.EyeOff`: `M10.733 5.076a10.744 10.744 0 0 1 11.205 6.575 1 1 0 0 1 0 .696 10.747 10.747 0 0 1-1.444 2.49 M14.084 14.158a3 3 0 0 1-4.242-4.242 M17.479 17.499a10.75 10.75 0 0 1-15.417-5.151 1 1 0 0 1 0-.696 10.75 10.75 0 0 1 4.446-5.143 M2 2 l20 20`
- *Seleccionar* (HU-14, select and move strokes) will need its own icon: the
  `mouse-pointer-2` glyph now belongs to *Puntero*.
- **Reserved** (SVGs ready, no feature yet — keep them off the bar): `circle`
  (Elipse, HU-16), `redo-2` (Rehacer, HU-17), `highlighter`, `palette`, `pin`,
  `pipette`.
- **Color** and **Grosor** are not icons: Color shows the active ink (or the
  gradient sample) as an 18px circle with a `--highlight` ring; Grosor is a
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
