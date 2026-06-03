# Acetato

App de escritorio **nativa para Windows (C#/.NET 9, WPF)** que superpone una capa
transparente y *click-through* sobre cualquier aplicación para **dibujar y anotar en vivo**
durante presentaciones y tutoriales. El único elemento visible es una **barra de
herramientas flotante**, minimalista y discreta.

> Principio rector: **CERO distracción.**

## Requisitos
- **Windows 11** (compatible Windows 10).
- **.NET 9 SDK** — instalar con: `winget install Microsoft.DotNet.SDK.9`

## Construir y ejecutar
```pwsh
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Acetato.Presentation
```

> **Nota:** el andamiaje se creó sin un SDK instalado en la máquina. El primer
> `dotnet restore`/`build` puede requerir: (1) instalar el .NET 9 SDK, (2) ajustar alguna
> versión de paquete en `Directory.Packages.props` si NuGet reporta una versión inexistente,
> y (3) una pasada corta para resolver hallazgos de los analizadores estrictos.

## Estructura del repositorio
```
acetato/
├─ Acetato.sln
├─ Directory.Build.props / Directory.Packages.props / .editorconfig / SonarLint.xml / global.json
├─ CLAUDE.md                 # contrato de trabajo (arquitectura + estándares)
├─ BACKLOG.md                # épicas, historias de usuario (HU) y DoD
├─ docs/architecture.md      # diagrama de capas y mapa HU → capa
├─ src/
│  ├─ Acetato.Domain/         # value objects puros
│  ├─ Acetato.Application/    # casos de uso + puertos (Abstractions/)
│  ├─ Acetato.Infrastructure/ # adaptadores Win32 / captura / bandeja
│  └─ Acetato.Presentation/   # WPF: App, Views, ViewModels, Resources (tokens)
├─ tests/
│  ├─ Acetato.Domain.Tests/
│  └─ Acetato.Application.Tests/
└─ .claude/skills/acetato-design/   # design system (invocable: /acetato-design)
```

## Arquitectura (resumen)
Capas con dependencias hacia adentro: `Presentation`/`Infrastructure` → `Application` →
`Domain`. El interop Win32 vive **solo** en `Infrastructure`. Detalle en
[`docs/architecture.md`](./docs/architecture.md) y reglas en [`CLAUDE.md`](./CLAUDE.md).

## Diseño
El sistema de diseño (tokens, iconos, UI kit del toolbar, guías de marca) está en
[`.claude/skills/acetato-design/`](./.claude/skills/acetato-design/). Los tokens ya están
portados a `src/Acetato.Presentation/Resources/Tokens.xaml`.

## Convenciones
- **Commits:** Conventional Commits.
- **UI/copy:** español (es-ES), sentence case, sin emoji.
- **Código:** SOLID/DRY, file-scoped namespaces, límites de tamaño/complejidad forzados por
  analizadores (ver `CLAUDE.md`).
