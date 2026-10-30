// Acetato — the floating toolbar (ToolbarWindow.xaml). Icon buttons, ink and
// thickness popovers, tooltips with shortcut, drag from the grip.
const { useState, useCallback: useCb } = React;
const Icon = window.Icon;

function InkPopover({ ink, gradient, onPick, onGradient }) {
  return (
    <div className="popover">
      {window.ACETATO_INKS.map(i => (
        <button key={i.id} className={'ink' + (!gradient && ink === i.id ? ' ink--sel' : '')}
                title={i.label} aria-label={i.label} onClick={() => onPick(i.id)}>
          <i style={{ background: i.value }}></i>
        </button>
      ))}
      {/* Round: Gradient is a mode, not an ink. */}
      <button className={'ink ink--round' + (gradient ? ' ink--sel' : '')} title="Degradado"
              aria-label="Degradado" onClick={onGradient}>
        <i className="grad-sample"></i>
      </button>
    </div>
  );
}

function ThicknessPopover({ index, onPick }) {
  return (
    <div className="popover">
      {window.ACETATO_WIDTHS.map((w, i) => {
        const d = 6 + i * 4;
        return (
          <button key={w} className={'cell' + (i === index ? ' cell--sel' : '')}
                  aria-label={'Grosor ' + w + ' px'} onClick={() => onPick(i)}>
            <i style={{ width: d, height: d }}></i>
          </button>
        );
      })}
    </div>
  );
}

function Toolbar(props) {
  const { pos, setPos, tool, setTool, ink, gradient, setInk, setGradient, widthIndex, setWidthIndex,
          chords, onUndo, onClear, onCapture, onSettings, onClose } = props;
  const [open, setOpen] = useState(null); // 'ink' | 'thick' | null
  const [hover, setHover] = useState(null);
  const [collapsed, setCollapsed] = useState(false); // eye button: grip + eye only

  const startDrag = useCb((e) => {
    const start = { x: e.clientX, y: e.clientY, x0: pos.x, y0: pos.y };
    setOpen(null);
    const move = (ev) => setPos({ x: start.x0 + (ev.clientX - start.x), y: start.y0 + (ev.clientY - start.y) });
    const up = () => { window.removeEventListener('pointermove', move); window.removeEventListener('pointerup', up); };
    window.addEventListener('pointermove', move);
    window.addEventListener('pointerup', up);
  }, [pos]);

  // Tooltip below the button: name + shortcut (if the action has one).
  const tip = (label, action) => ({
    onMouseEnter: (e) => {
      const r = e.currentTarget.getBoundingClientRect();
      const bar = e.currentTarget.closest('.bar').getBoundingClientRect();
      setHover({ label, chord: action ? chords[action] : '', cx: r.left - bar.left + r.width / 2 });
    },
    onMouseLeave: () => setHover(null),
  });
  const toggle = (name) => setOpen(open === name ? null : name);
  const solid = window.ACETATO_INKS.find(i => i.id === ink).value;

  return (
    <div className="bar" style={{ left: pos.x, top: pos.y }} role="toolbar" aria-label="Acetato">
      <div className="grip" onPointerDown={startDrag} {...tip('Mover')}><Icon name="grip-vertical" size={18} /></div>
      <div className="divider"></div>
      {/* Icon shows the bar's state; tooltip names the action. Never accent. */}
      <button className="tool" aria-label={collapsed ? 'Mostrar barra' : 'Ocultar barra'} aria-expanded={!collapsed}
              onClick={() => { setOpen(null); setHover(null); setCollapsed(!collapsed); }}
              {...tip(collapsed ? 'Mostrar barra' : 'Ocultar barra')}>
        <Icon name={collapsed ? 'eye-off' : 'eye'} />
      </button>

      {!collapsed && (<>

      {window.ACETATO_TOOLS.map(t => (
        <button key={t.id} className={'tool' + (tool === t.id ? ' tool--active' : '')} disabled={!t.enabled}
                aria-label={t.label} aria-pressed={tool === t.id}
                onClick={() => { setTool(t.id); setOpen(null); }} {...tip(t.label, t.action)}>
          <Icon name={t.icon} />
        </button>
      ))}

      <div className="divider"></div>
      <span className="anchor">
        <button className="tool" aria-label="Color" onClick={() => toggle('ink')} {...tip('Color')}>
          <span className={'swatch' + (gradient ? ' grad-sample' : '')} style={gradient ? null : { background: solid }}></span>
        </button>
        {open === 'ink' && (
          <InkPopover ink={ink} gradient={gradient}
                      onPick={(id) => { setInk(id); setGradient(false); setOpen(null); }}
                      onGradient={() => { setGradient(true); setOpen(null); }} />
        )}
      </span>
      <span className="anchor">
        <button className="tool" aria-label="Grosor" onClick={() => toggle('thick')} {...tip('Grosor')}>
          <span className="thick-glyph"><span></span><span></span><span></span></span>
        </button>
        {open === 'thick' && <ThicknessPopover index={widthIndex} onPick={(i) => { setWidthIndex(i); setOpen(null); }} />}
      </span>

      <div className="divider"></div>
      <button className="tool" aria-label="Deshacer" onClick={onUndo} {...tip('Deshacer', 'undo')}><Icon name="undo-2" /></button>
      <button className="tool" aria-label="Limpiar todo" onClick={onClear} {...tip('Limpiar todo', 'clear')}><Icon name="trash-2" /></button>

      <div className="divider"></div>
      <button className="tool" aria-label="Recortar" onClick={onCapture} {...tip('Recortar', 'capture')}><Icon name="camera" /></button>
      <button className="tool" aria-label="Ajustes" onClick={() => { setOpen(null); onSettings(); }} {...tip('Ajustes')}><Icon name="settings-2" /></button>

      <div className="divider"></div>
      <button className="tool" aria-label="Cerrar" onClick={onClose} {...tip('Cerrar', 'toggle')}><Icon name="x" /></button>
      </>)}

      {hover && !open && (
        <div className="tooltip" style={{ left: hover.cx }}>
          {hover.label}
          {hover.chord && <span className="kbd">{hover.chord}</span>}
        </div>
      )}
    </div>
  );
}

window.Toolbar = Toolbar;
