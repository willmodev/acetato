// Acetato — the Settings window (SettingsWindow.xaml + SettingsStyles.xaml):
// the editable shortcut list with capture, conflicts, restore and remove.
const { useState: useSt, useEffect: useEf } = React;

// Rules from KeyChordRules / HotkeyMessages.
function chordError(chord) {
  const parts = chord.split('+');
  const mods = parts.slice(0, -1);
  if (!mods.length) return 'Usa Ctrl, Alt o Win';
  if (mods.every(m => m === 'Mayús')) return 'Mayús necesita otro modificador';
  if (chord === 'Alt+F4' || chord === 'Alt+Espacio') return 'Reservado por Windows';
  return null;
}

function HotkeyRow({ def, chord, isCapturing, message, canReplace, onCapture, onReset, onRemove, onReplace, onCancel }) {
  const Icon = window.Icon;
  return (
    <div className="row">
      <span className="row-label">{def.label}</span>
      <button className={'keycap' + (isCapturing ? ' keycap--capturing' : '')} onClick={onCapture}>
        {isCapturing ? 'Pulsa la combinación…' : (chord || 'Sin atajo')}
      </button>
      {chord !== def.chord
        ? <button className="tool tool--row" title="Restaurar" aria-label="Restaurar" onClick={onReset}><Icon name="rotate-ccw" size={14} /></button>
        : <span></span>}
      {chord
        ? <button className="tool tool--row" title="Quitar" aria-label="Quitar" onClick={onRemove}><Icon name="x" size={14} /></button>
        : <span></span>}
      {message && (
        <div className="row-msg">
          <span>{message}</span>
          {canReplace && (
            <span>
              <button className="textbtn" onClick={onReplace}>Reemplazar</button>
              <button className="textbtn" onClick={onCancel}>Cancelar</button>
            </span>
          )}
        </div>
      )}
    </div>
  );
}

function SettingsWindow({ chords, setChords, onClose }) {
  const Icon = window.Icon;
  const defs = window.ACETATO_HOTKEYS;
  const [capturing, setCapturing] = useSt(null);  // action being captured
  const [pending, setPending] = useSt(null);      // { action, chord, owner, message }
  const [confirmAll, setConfirmAll] = useSt(false);

  useEf(() => {
    if (!capturing) return undefined;
    const onKey = (e) => {
      e.preventDefault();
      e.stopPropagation();
      if (e.key === 'Escape') { setCapturing(null); setPending(null); return; }
      const chord = window.chordFromEvent(e);
      if (!chord) return;
      const error = chordError(chord);
      if (error) { setPending({ action: capturing, message: error }); return; }
      const owner = Object.keys(chords).find(a => a !== capturing && chords[a] === chord);
      if (owner) {
        const label = defs.find(d => d.action === owner).label;
        setPending({ action: capturing, chord, owner, message: 'Ya la usa ' + label });
        return;
      }
      setChords({ ...chords, [capturing]: chord });
      setCapturing(null);
      setPending(null);
    };
    window.addEventListener('keydown', onKey, true);
    return () => window.removeEventListener('keydown', onKey, true);
  }, [capturing, chords]);

  const set = (action, chord) => { setChords({ ...chords, [action]: chord }); setPending(null); setCapturing(null); };
  const replace = () => {
    setChords({ ...chords, [pending.owner]: '', [pending.action]: pending.chord });
    setPending(null); setCapturing(null);
  };
  const resetAll = () => { setChords(Object.fromEntries(defs.map(d => [d.action, d.chord]))); setConfirmAll(false); };

  const renderRow = (def) => (
    <HotkeyRow key={def.action} def={def} chord={chords[def.action]}
               isCapturing={capturing === def.action}
               message={pending && pending.action === def.action ? pending.message : null}
               canReplace={pending && pending.action === def.action && !!pending.owner}
               onCapture={() => { setCapturing(def.action); setPending(null); }}
               onReset={() => set(def.action, def.chord)}
               onRemove={() => set(def.action, '')}
               onReplace={replace}
               onCancel={() => { setPending(null); setCapturing(null); }} />
  );

  return (
    <div className="settings" role="dialog" aria-label="Ajustes">
      <div className="settings-head">
        <span>Ajustes</span>
        <button className="tool" aria-label="Cerrar" onClick={onClose}><Icon name="x" /></button>
      </div>
      <div className="hr"></div>
      <div className="settings-body">
        <div className="settings-cap">Siempre activos</div>
        {defs.filter(d => d.scope === 'always').map(renderRow)}
        <div className="settings-cap">Solo al dibujar</div>
        {defs.filter(d => d.scope === 'drawing').map(renderRow)}
      </div>
      <div className="hr"></div>
      <div className="settings-foot">
        {confirmAll ? (
          <>
            <span className="row-label">Restaurar todos los atajos</span>
            <span>
              <button className="textbtn" onClick={resetAll}>Restaurar</button>
              <button className="textbtn" onClick={() => setConfirmAll(false)}>Cancelar</button>
            </span>
          </>
        ) : (
          <button className="textbtn" style={{ marginLeft: 'auto' }} onClick={() => setConfirmAll(true)}>Restaurar todo</button>
        )}
      </div>
    </div>
  );
}

window.SettingsWindow = SettingsWindow;
