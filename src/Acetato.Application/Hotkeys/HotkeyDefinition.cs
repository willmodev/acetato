using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Hotkeys;

/// <summary>Definición fija de una acción (HU-21): su grupo y su atajo por defecto.</summary>
public sealed record HotkeyDefinition(HotkeyAction Action, HotkeyScope Scope, KeyChord Default);
