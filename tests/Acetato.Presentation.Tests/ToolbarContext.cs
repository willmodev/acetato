using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Tests;

/// <summary>Piezas reales de una barra de prueba: estado de dibujo, controlador, estado plegado, pizarra y ViewModel.</summary>
internal sealed record ToolbarContext(
    DrawingSettings Settings,
    OverlayController Controller,
    ToolbarLayout Layout,
    ToolbarViewModel ViewModel,
    WhiteboardController Whiteboard,
    IOverlayCanvas Canvas);
