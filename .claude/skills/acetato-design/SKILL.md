---
name: acetato-design
description: Use this skill to generate well-branded interfaces and assets for Acetato, either for production or throwaway prototypes/mocks/etc. Contains essential design guidelines, colors, type, fonts, assets, and UI kit components for prototyping.
user-invocable: true
---

Read the README.md file within this skill, and explore the other available files.
If creating visual artifacts (slides, mocks, throwaway prototypes, etc), copy assets out and create static HTML files for the user to view. If working on production code, you can copy assets and read the rules here to become an expert in designing with this brand.
If the user invokes this skill without any other guidance, ask them what they want to build or design, ask some questions, and act as an expert designer who outputs HTML artifacts _or_ production code, depending on the need.

Key facts to load first:
- Acetato is a transparent, click-through annotation overlay for **Windows (C#/.NET)**. Designs here are visual reference to be re-implemented in WinForms/WPF — not web production code.
- Guiding principle: **ZERO distraction.** The only visible chrome is one minimal, dark-glass floating toolbar.
- Two separate palettes: **Chrome** (dark glass neutrals + one teal accent, `--chrome-*`, `--accent`, `--fg-*`) and **Tintas** (the saturated drawing inks `--ink-red/blue/yellow/green/white/black`). Never mix them.
- Tokens: `colors_and_type.css`. Components & states: `preview/` and `ui_kits/toolbar/`.
- Icons: Lucide line icons, 2px stroke / 20px (see README → ICONOGRAPHY for the tool→name map).
- Copy: Spanish, one-or-two words, sentence case, no emoji, no voice.
