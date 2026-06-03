# CLAUDE.md — Acetato

Contrato de trabajo para este repositorio. Estas reglas **anulan** comportamientos por
defecto y deben seguirse al pie de la letra.

## Producto
**Acetato** es una app de escritorio **nativa para Windows (C#/.NET 9, WPF)** que superpone
una capa transparente y *click-through* sobre cualquier aplicación para dibujar y anotar en
vivo (presentaciones, tutoriales). El **único** elemento visible es una **barra de
herramientas flotante**, minimalista y discreta.

> **Principio rector #1 — CERO distracción.** La UI debe ser discreta, casi invisible
> cuando no se usa, y nunca robarle protagonismo a lo que hay debajo en pantalla.

- **Backlog / HUs:** [`BACKLOG.md`](./BACKLOG.md) (épicas EP-01…04, historias HU-01…12, DoD).
- **Design system:** [`.claude/skills/acetato-design/`](./.claude/skills/acetato-design/)
  (invocable como `/acetato-design`). Es la **referencia visual de marca**.

## Stack y tooling
- **.NET 9**, **C# `latest`**, **WPF** (Presentation). Requiere el **.NET 9 SDK** instalado
  (`winget install Microsoft.DotNet.SDK.9`).
- `Nullable` enable, `ImplicitUsings`, **file-scoped namespaces**.
- Versiones de paquetes centralizadas en `Directory.Packages.props` (Central Package Management).
- Propiedades comunes en `Directory.Build.props`; estilo/naming/umbrales en `.editorconfig`
  + `SonarLint.xml`.

## Arquitectura — capas y regla de dependencias
Solución multi-proyecto. **Las dependencias apuntan hacia adentro:**

```
Presentation ─┐
              ├─► Application ─► Domain
Infrastructure┘                  ▲
   └──────────────────────────────┘   (Infrastructure → Application, Domain)
Presentation ─► Infrastructure  (SOLO en el composition root, para registrar en DI)
```

| Proyecto | Rol | Puede depender de |
|----------|-----|-------------------|
| `Acetato.Domain` | Entidades / value objects puros (`Stroke`, `ToolKind`, `TintaColor`, `StrokePoint`). | (nada) |
| `Acetato.Application` | Casos de uso + **puertos** (interfaces) en `Abstractions/`. Agnóstico de plataforma. | Domain |
| `Acetato.Infrastructure` | **Adaptadores**: interop Win32 (P/Invoke), captura, bandeja, persistencia. | Application, Domain |
| `Acetato.Presentation` | WPF: `App` (composition root), `Views/`, `ViewModels/`, `Resources/`. | Application, Domain, Infrastructure |

**Reglas de capa (no negociables):**
- **El interop Win32 (P/Invoke) vive EXCLUSIVAMENTE en Infrastructure.** Nunca en Domain,
  Application ni Presentation.
- **Domain no referencia WPF ni Win32.** Solo tipos del lenguaje.
- **MVVM estricto:** sin lógica en el code-behind de WPF; todo en ViewModels. El code-behind
  solo llama a `InitializeComponent()`.
- Inyección **por constructor**; las implementaciones se registran en `App.BuildServiceProvider`.

## Estándares de código
- **SOLID** y **DRY**. Una responsabilidad por tipo; **un tipo público por archivo** (nombre
  de archivo = nombre del tipo).
- `record` para value objects inmutables; preferir inmutabilidad.
- Métodos `async` con sufijo `Async`.
- Nombres expresivos. **Naming:** `PascalCase` (tipos/miembros), `IPascal` (interfaces),
  `_camelCase` (campos privados), `camelCase` (locales/parámetros).

### Límites duros (el build FALLA si se superan — no suprimir la regla, refactorizar)
| Límite | Valor | Regla |
|--------|-------|-------|
| Líneas por método | **≤ 40** | S138 |
| Líneas por archivo | **≤ 400** | S104 |
| Complejidad ciclomática | **≤ 10** | S1541 |
| Parámetros por método | **≤ 5** | S107 |
| Anidamiento de control de flujo | **≤ 3** | S134 |

Si algo excede el límite → **extraer** (método/clase/archivo). Los umbrales están en
`SonarLint.xml`; la severidad (`error`) en `.editorconfig`.

## Idioma y copy
- **UI en español (es-ES).** Labels: *Lápiz, Borrador, Flecha, Rectángulo, Grosor, Tinta,
  Deshacer, Rehacer, Limpiar todo, Cerrar.* Sentence case, 1–2 palabras, **sin emoji**, sin voz.
- **Comentarios de código en español.**
- **Commits: Conventional Commits** (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`)
  — exigido por el DoD del backlog.

## Design system — reglas de marca (al construir UI)
- Tokens ya portados en `src/Acetato.Presentation/Resources/Tokens.xaml`. Úsalos por
  `StaticResource`; **no** hardcodear colores.
- **Dos paletas SEPARADAS:** *Chrome* (`Chrome.*`, `Fg.*`, `Accent.*`) para la UI; *Tintas*
  (`Ink.*`) para los trazos del usuario. **Nunca mezclar.**
- El **acento teal** marca **solo** la herramienta activa.
- Iconos: línea Lucide (2px / 20px). Origen SVG en
  `.claude/skills/acetato-design/assets/icons/`; portar a `Resources/Icons.xaml` (Geometry).
- Glass = `Chrome.Glass` + blur (Acrylic/Mica vía DWM); fallback opaco `Chrome.Solid` si no
  hay composición.

## Comandos
```pwsh
dotnet restore
dotnet build -warnaserror      # build verde = límites y estilo cumplidos
dotnet test                    # xUnit
dotnet format                  # aplica estilo de .editorconfig
dotnet run --project src/Acetato.Presentation
```

## Definition of Done (resumen — ver BACKLOG.md)
Cumple todos sus criterios de aceptación · compila **sin warnings nuevos** · probada en
**Windows 11** (y multi-monitor cuando aplique) · atajos sin chocar con el SO · sin fugas de
memoria al activar/desactivar repetidamente · commit en Conventional Commits.
