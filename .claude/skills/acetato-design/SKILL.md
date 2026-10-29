---
name: acetato-design
description: Use this skill to generate well-branded interfaces and assets for Acetato, either for production or throwaway prototypes/mocks/etc. Contains essential design guidelines, colors, type, fonts, assets, and UI kit components for prototyping.
user-invocable: true
---

Read the README.md file within this skill, and explore the other available files.
If creating visual artifacts (slides, mocks, throwaway prototypes, etc), copy assets out and create static HTML files for the user to view. If working on production code, you can copy assets and read the rules here to become an expert in designing with this brand.
If the user invokes this skill without any other guidance, ask them what they want to build or design, ask some questions, and act as an expert designer who outputs HTML artifacts _or_ production code, depending on the need.

Key facts to load first:
- Acetato is a transparent, click-through annotation overlay for **Windows (C#/.NET 9, WPF)**. Designs here are visual reference for the WPF app — not web production code.
- **The code is the source of truth:** `src/Acetato.Presentation/Resources/Tokens.xaml` (tokens) and `Icons.xaml` (icons). `colors_and_type.css` mirrors them; if they disagree, the XAML wins and this skill gets updated.
- Guiding principle: **ZERO distraction.** The only visible chrome over the screen is one minimal, translucent floating toolbar (no blur yet, HU-18). Settings is the only other window.
- Two separate palettes: **Chrome** (dark glass neutrals + one teal accent, `--chrome-*`, `--accent`, `--fg-*`) and **Tintas** (the saturated drawing inks `--ink-red/blue/yellow/green/white/black` plus the neon `--ink-grad-*` pairs of Gradient mode). Never mix them. The accent marks only what is selected — no glow.
- Tokens: `colors_and_type.css`. Components & states: `preview/` and `ui_kits/toolbar/`.
- Icons: Lucide line icons, 2px stroke / 20px (see README → ICONOGRAPHY for the tool→name map).
- Copy: Spanish, one-or-two words, sentence case, no emoji, no voice. Shortcuts in Spanish key names (`Ctrl+Retroceso`, `Ctrl+RePág`).
