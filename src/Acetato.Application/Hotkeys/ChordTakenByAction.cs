using Acetato.Application.Abstractions;

namespace Acetato.Application.Hotkeys;

/// <summary>La combinación la usa otra acción de Acetato (HU-21); se puede ofrecer Reemplazar.</summary>
public sealed record ChordTakenByAction(HotkeyAction Owner) : AssignResult;
