using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>Atajo a registrar (HU-21): la acción y la combinación que la dispara.</summary>
public sealed record HotkeyAssignment(HotkeyAction Action, KeyChord Chord);
