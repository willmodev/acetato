# Acetato — Design System

**Acetato** is a native **Windows desktop app (C#/.NET)** that lays a transparent,
click-through layer over *any* application so the user can draw and annotate on
top of it — live, during presentations and tutorials. The name means *acetate*
(the clear film you used to write on with overhead projectors); the product is
exactly that, in software.

The **only** visible element is a **floating, minimal toolbar**. Everything else
is the user's own screen showing through. That single constraint drives the
whole system.

> **Guiding principle #1 — ZERO distraction.** The UI must be discreet, nearly
> invisible when idle, and never steal attention from whatever is underneath it.

This design system is **visual reference to be re-implemented natively in
WinForms/WPF.** The HTML/CSS/JS here exists to lock down look-and-feel and
interaction — it is not production code.

## Sources
This system was built **from a written product specification only** — no
codebase, Figma file, screenshots, or existing assets were provided. All values
(palettes, type, spacing, the toolbar layout, the icon mapping) are original,
derived from the brief. If a canonical source (repo / Figma / brand kit) exists,
attach it and this system should be reconciled against it.

---

## The two palettes
Acetato deliberately keeps **two separate color worlds** so the UI never gets
confused with the user's drawing:

1. **Chrome** — the tool's own UI. Dark glass neutrals **+ one accent** (teal).
   The accent marks *only* the active tool.
2. **Tintas** — the drawing inks: **red, blue, yellow, green, white, black.**
   Saturated and legible over any backdrop, light or dark.

Full token values live in [`colors_and_type.css`](./colors_and_type.css).

---

## CONTENT FUNDAMENTALS

There is almost no copy in Acetato — that is the point. Text appears only in
**tooltips, popover captions, keyboard hints, and settings.** Rules:

- **Language: Spanish (es-ES).** UI labels are Spanish: *Lápiz, Borrador,
  Flecha, Rectángulo, Grosor, Tinta, Deshacer, Rehacer, Limpiar todo, Cerrar.*
- **Voice: none.** The product does not "speak". No first or second person, no
  marketing voice, no encouragement. Labels *name* a thing; they never narrate.
- **One or two words, max.** A tooltip is a noun (*Lápiz*) or a short verb
  phrase (*Limpiar todo*). Never a sentence inside the toolbar.
- **Casing: Sentence case** for labels (*Limpiar todo*, not *Limpiar Todo*).
  **UPPERCASE** only for the tiny popover section captions (*TINTA*, *GROSOR*),
  used as a typographic device, never for emphasis in prose.
- **No punctuation** ending labels. No exclamation marks, ever.
- **No emoji.** Not in UI, not in docs-facing strings. Icons carry meaning.
- **Keyboard hints** are shown as monospace key caps next to the label
  (`Lápiz  P`, `Deshacer  Ctrl+Z`).
- **Numbers**: thickness in `px` (`8 px`), no other units surfaced in the bar.
- **Vibe:** calm, instrumental, invisible. The copy equivalent of a good tool —
  you barely notice it's there. When in doubt, remove the word.

Examples (good): `Flecha`, `Grosor`, `Limpiar todo`, `Color`, `Cerrar`.
Examples (avoid): `¡Dibuja ahora!`, `Tu lápiz favorito`, `Haz clic para borrar todo 😀`.

---

## VISUAL FOUNDATIONS

**Overall mood.** Dark, glassy, weightless. The bar should feel like a sliver of
frosted acetate hovering over the screen — present enough to use, quiet enough
to forget. Think Windows 11 Fluent *Acrylic*, dialed toward minimal.

**Color & contrast.** Chrome is a near-black glass (`--chrome-glass`,
`rgba(22,24,28,.72)`) so the user's screen tints through it. Foreground icons use
a **white alpha scale** (`--fg-1/2/3` at 96/62/38%) rather than fixed greys, so
they hold up over both light and dark content beneath. The **teal accent**
(`#2FD9C4`) is the system's only chromatic moment in the UI and is reserved for
the **active tool**. Drawing inks (tintas) are fully saturated and never appear
as UI chrome.

**Transparency & blur — the signature.** The toolbar and all popovers are
**semi-transparent + backdrop-blur** (`blur(20px) saturate(150%)`). This is the
core brand motif: legibility on *any* background comes from blur + a white
hairline, not from an opaque panel. Natively this maps to **Acrylic/Mica**;
provide an opaque `--chrome-solid` fallback when DWM composition is unavailable.

**Type.** Minimal and small. Canonical face is **Segoe UI Variable** (Windows 11
system font); web previews substitute **Public Sans**. Sizes top out at 12px
(tooltips) and bottom out at 10px (uppercased captions). Weights 400/500/600.
Monospace (**Cascadia Code**) only for keyboard hints. Type never carries the
brand — it's purely functional.

**Spacing.** 4px base scale (`--s-1…6`). The bar is tight: 6px inner padding,
2px gaps between buttons, 36px square hit targets (44px in touch/pen builds),
20px icons.

**Corners.** Soft but not pill-round: toolbar 16px, popovers 14px, buttons 10px,
ink swatches 8px. A full pill (999px) is available but used sparingly.

**Borders & edges.** Every glass surface gets a **1px hairline** at
`rgba(255,255,255,.10)` plus a **top inner highlight** (`inset 0 1px 0
rgba(255,255,255,.14)`) — the "lit top edge" that sells the glass. Dividers
between tool groups are a single `rgba(255,255,255,.07)` line.

**Elevation / shadows.** The overlay floats high above everything, so shadows
are deep and soft: `--shadow-bar` (bar), `--shadow-pop` (popovers). The active
tool adds a tinted glow + ring (`--shadow-active`). No hard/short shadows.

**Backgrounds.** There is **no app background** — Acetato is transparent. There
are no gradients, images, textures, or patterns in the chrome itself. Demo
surfaces (slides, the split light/dark stage in the cards) exist only to prove
legibility, never as brand decoration.

**Hover / press / active states.**
- *Hover:* button bg → `rgba(255,255,255,.08)`, icon → `--fg-1`. Subtle.
- *Press:* `transform: scale(0.94)` + a fainter fill. Quick, tactile.
- *Active (selected tool):* accent fill `rgba(47,217,196,.16)`, accent-colored
  glyph, glow + ring. This is the one place color enters the UI.
- *Disabled:* icon drops to `--fg-3`, no hover.

**Motion.** Restrained and fast. Transitions are 80–120ms ease on background,
color, transform. Popovers fade/scale in quickly. **No bounces, no infinite
loops, no decorative animation** — motion only confirms an action. Respect
`prefers-reduced-motion`.

**Layout rules.** The bar is a single floating element, draggable by a grip at
its lead edge, and snaps to screen edges in the native build. It is the only
fixed chrome. Popovers anchor directly below their trigger button. Nothing else
is ever persistently on screen.

---

## ICONOGRAPHY

- **Style:** simple, recognizable **line icons** — uniform **2px stroke**, round
  caps/joins, drawn on a 24px grid, displayed at **20px** in the bar.
- **Set used: [Lucide](https://lucide.dev)** (ISC-licensed, open source). It is
  loaded from CDN in the previews/UI kit. **This is a substitution** — Acetato
  had no icon set of its own — chosen because its weight and geometry exactly fit
  the "discreet line icon" brief. *If you have a bespoke set, swap it in and keep
  the 2px/20px spec.*
- In the React UI kit, icons are rendered as **inline SVG** via a small
  `Icon` component that reads Lucide's icon-node data (no DOM mutation), so they
  reconcile cleanly with React.
- **Tool → Lucide name** mapping (ship these SVGs as native resources):

  | Tool | Lucide | Tool | Lucide |
  |------|--------|------|--------|
  | Seleccionar | `mouse-pointer-2` | Color | `palette` |
  | Lápiz | `pencil` | Cuentagotas | `pipette` |
  | Marcador | `highlighter` | Grosor | `spline` (or the 3-line glyph) |
  | Borrador | `eraser` | Deshacer | `undo-2` |
  | Flecha | `arrow-up-right` | Rehacer | `redo-2` |
  | Línea | `minus` | Limpiar | `trash-2` |
  | Rectángulo | `square` | Captura | `camera` |
  | Elipse | `circle` | Ajustes | `settings-2` |
  | Texto | `type` | Mover | `grip-vertical` |
  | Fijar | `pin` | Cerrar | `x` |

- **Emoji / unicode as icons: never.** All iconography is vector line icons.
- The **thickness** control uses a custom **3-stacked-lines glyph** (not a Lucide
  icon) because it doubles as a live indicator of relative stroke weight.

---

## Index / manifest

Root files:
- [`README.md`](./README.md) — this file.
- [`colors_and_type.css`](./colors_and_type.css) — all design tokens (Chrome + Tintas, type, spacing, shape, elevation).
- [`SKILL.md`](./SKILL.md) — entry point when used as an Agent Skill.
- `preview/` — the Design System cards (colors, type, spacing, components, brand) + shared `preview.css` / `toolbar.css`.
- `assets/` — copied/derived visual assets. (Icons are loaded from the Lucide CDN; see ICONOGRAPHY.)

UI kits:
- [`ui_kits/toolbar/`](./ui_kits/toolbar/) — **the floating toolbar**, an interactive recreation overlaying a slide. See its [README](./ui_kits/toolbar/README.md).

There is no slide template in this system (none was provided), so no `slides/`
folder was created.
