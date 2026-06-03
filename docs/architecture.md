# Arquitectura — Acetato

App de escritorio **Windows (C#/.NET 9, WPF)**: overlay transparente y *click-through* para
anotar sobre cualquier aplicación. Arquitectura **por capas** (estilo Clean/Hexagonal
pragmático), con dependencias apuntando **hacia adentro**.

## Diagrama de capas

```
┌─────────────────────────────────────────────────────────────┐
│  Acetato.Presentation  (WPF)                                 │
│  App (composition root + DI) · Views · ViewModels · Resources│
│        │ usa                       ▲ registra implementaciones│
│        ▼                           │                          │
│  Acetato.Application               │  Acetato.Infrastructure  │
│  Casos de uso + Puertos (I*)       │  Adaptadores (Win32,     │
│        │                           │  captura, bandeja, JSON) │
│        ▼                           │        │                 │
│  Acetato.Domain  ◄─────────────────┴────────┘                 │
│  Entidades / value objects puros (sin WPF, sin Win32)         │
└─────────────────────────────────────────────────────────────┘
```

**Regla de oro:** las flechas de dependencia solo apuntan hacia `Domain`. `Presentation`
conoce a `Infrastructure` **únicamente** en el composition root (`App.BuildServiceProvider`)
para enlazar cada puerto con su adaptador; el resto del código de Presentation depende solo
de las **interfaces** de `Application`.

## Responsabilidades

### Acetato.Domain
Modelo del dominio, inmutable y sin dependencias externas.
- `ToolKind`, `TintaColor`, `StrokePoint`, y (al implementar HUs) `Stroke`, `DrawingDocument`.

### Acetato.Application
Orquestación agnóstica de plataforma y **puertos** (`Abstractions/`):
- `IGlobalHotkeyService` (HU-01) · `IOverlayController` (HU-01/02) · y a futuro
  `IDrawingSession` (HU-03/04/07), `IScreenCaptureService` (HU-12), `ITrayService` (HU-08),
  `ISettingsStore`.

### Acetato.Infrastructure
Implementaciones concretas. **Único** sitio con P/Invoke:
- Atajos globales (`RegisterHotKey`).
- Interop del overlay: `WS_EX_LAYERED` / `WS_EX_TRANSPARENT` (click-through) / `WS_EX_TOOLWINDOW`.
- Captura de pantalla a PNG, icono de bandeja, persistencia de ajustes (JSON).

### Acetato.Presentation
WPF. `OverlayWindow` (transparente, sin chrome, `Topmost`, maximizada), el toolbar flotante
y sus popovers, ViewModels (MVVM con CommunityToolkit.Mvvm) y los `Resources/` (tokens +
iconos del design system).

## Mapa HU → capa (dónde tocar)
| HU | Foco | Capa principal |
|----|------|----------------|
| HU-01 activar/desactivar + hotkey | hotkey global, mostrar capa | Infrastructure + Application + Presentation |
| HU-02 click-through | alternar `WS_EX_TRANSPARENT` | Infrastructure |
| HU-03 trazo a mano alzada | render del trazo | Presentation (canvas) + Domain |
| HU-04 limpiar | vaciar documento | Application + Domain |
| HU-05/06 color y grosor | estado de herramienta | Domain + Presentation |
| HU-07 deshacer | pila de historial | Application + Domain |
| HU-08 bandeja | NotifyIcon | Infrastructure + Presentation |
| HU-09 multi-monitor / DPI | Per-Monitor V2 (`app.manifest`) | Presentation + Infrastructure |
| HU-10 toolbar flotante | barra + popovers | Presentation |
| HU-11 formas (línea/flecha/rect) | render de formas | Presentation + Domain |
| HU-12 guardar PNG | captura compuesta | Infrastructure |

## Decisiones
- **WPF** sobre WinForms: transparencia real, Acrylic/Mica, DPI Per-Monitor V2, render
  acelerado e `InkCanvas` para trazos; los tokens del design system mapean ≈ 1:1 a
  `ResourceDictionary`.
- **Render del dibujo:** empezar con primitivas WPF / `InkCanvas`; si la latencia objetivo
  (< 50 ms, HU-03) no se alcanza, evaluar Direct2D (Vortice.Windows).
- **DI:** `Microsoft.Extensions.DependencyInjection` con composition root en `App`.
