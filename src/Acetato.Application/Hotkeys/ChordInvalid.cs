using Acetato.Domain;

namespace Acetato.Application.Hotkeys;

/// <summary>La combinación no cumple las reglas (HU-21); nada cambia.</summary>
public sealed record ChordInvalid(KeyChordError Error) : AssignResult;
