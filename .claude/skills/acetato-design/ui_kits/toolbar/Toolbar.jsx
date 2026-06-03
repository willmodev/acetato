// Acetato — the floating toolbar. Icon buttons, ink + thickness popovers, drag.
const { useState, useRef, useCallback: useCb } = React;
const Icon = window.Icon;

function ToolButton({ t, active, onClick, onHover, onLeave, children }) {
  return (
    <button
      className={'tool' + (active ? ' tool--active' : '')}
      onClick={onClick}
      onMouseEnter={onHover}
      onMouseLeave={onLeave}
      aria-label={t.label}
      aria-pressed={active}
    >
      {children || <Icon name={t.icon} />}
    </button>
  );
}

function InkPopover({ color, onPick }) {
  return (
    <div className="popover" style={{ left: 0 }}>
      <div className="caption">Tinta</div>
      <div className="swatches">
        {window.ACETATO_INKS.map(ink => (
          <button
            key={ink.id}
            className={'ink' + (ink.id === 'white' ? ' ink--white' : ink.id === 'black' ? ' ink--black' : '')
              + (color === ink.value ? ' ink--sel' : '')}
            style={{ background: ink.value }}
            title={ink.label}
            onClick={() => onPick(ink.value)}
          />
        ))}
      </div>
    </div>
  );
}

function ThicknessPopover({ width, onPick }) {
  const sizes = window.ACETATO_WIDTHS;
  return (
    <div className="popover" style={{ left: 0, width: 188 }}>
      <div className="caption">Grosor</div>
      <div className="dots">
        {sizes.map(w => {
          const d = 6 + (w / 18) * 18;
          return (
            <button
              key={w}
              className={'dot' + (width === w ? ' dot--sel' : '')}
              style={{ width: d, height: d }}
              onClick={() => onPick(w)}
              title={w + ' px'}
            />
          );
        })}
      </div>
    </div>
  );
}

function Toggle({ on, onChange }) {
  return (
    <button className={'toggle' + (on ? ' toggle--on' : '')} onClick={() => onChange(!on)} role="switch" aria-checked={on}>
      <span className="toggle-knob"></span>
    </button>
  );
}

function SettingsPopover({ settings, setSettings, right }) {
  const set = (k, v) => setSettings(s => ({ ...s, [k]: v }));
  return (
    <div className="popover popover--settings" style={right ? { right: 0 } : { left: 0 }}>
      <div className="caption">Ajustes</div>
      <div className="set-row">
        <span className="set-label">Tamaño de la barra</span>
        <div className="seg">
          <button className={settings.size === 'compact' ? 'seg-on' : ''} onClick={() => set('size', 'compact')}>Compacta</button>
          <button className={settings.size === 'cozy' ? 'seg-on' : ''} onClick={() => set('size', 'cozy')}>Cómoda</button>
        </div>
      </div>
      <div className="set-row">
        <span className="set-label">Suavizar trazo</span>
        <Toggle on={settings.smooth} onChange={(v) => set('smooth', v)} />
      </div>
      <div className="set-row">
        <span className="set-label">Ajustar a bordes</span>
        <Toggle on={settings.snap} onChange={(v) => set('snap', v)} />
      </div>
      <div className="set-row">
        <span className="set-label">Mostrar atajos</span>
        <Toggle on={settings.hints} onChange={(v) => set('hints', v)} />
      </div>
    </div>
  );
}

function Toolbar({ pos, setPos, tool, setTool, color, setColor, width, setWidth, onUndo, onRedo, onClear, onCapture, canUndo, canRedo, hasInk, settings, setSettings }) {
  const [open, setOpen] = useState(null); // 'ink' | 'thick' | null
  const [hover, setHover] = useState(null); // tooltip target
  const dragRef = useRef(null);
  const barRef = useRef(null);

  const startDrag = useCb((e) => {
    const start = { x: e.clientX, y: e.clientY, ...pos };
    dragRef.current = start;
    setOpen(null);
    const move = (ev) => {
      setPos({ x: start.x0 + (ev.clientX - start.x), y: start.y0 + (ev.clientY - start.y) });
    };
    start.x0 = pos.x; start.y0 = pos.y;
    const up = () => { window.removeEventListener('pointermove', move); window.removeEventListener('pointerup', up); };
    window.addEventListener('pointermove', move);
    window.addEventListener('pointerup', up);
  }, [pos]);

  const pick = (t) => { setTool(t.id); setOpen(null); };
  const tip = (key, label) => ({
    onMouseEnter: (e) => {
      const el = e.currentTarget;
      const cx = el.offsetLeft + el.offsetWidth / 2;
      setHover({ key, label, cx });
    },
    onMouseLeave: () => setHover(null),
  });

  const TOOLS = window.ACETATO_TOOLS;
  const ACT = window.ACETATO_ACTIONS;
  const btnSize = settings.size === 'cozy' ? '44px' : '36px';

  return (
    <div className="bar" ref={barRef} style={{ left: pos.x, top: pos.y, '--btn': btnSize }}>
      <div className="grip" onPointerDown={startDrag} {...tip('', 'Arrastrar')}>
        <Icon name="grip-vertical" size={14} />
      </div>

      {TOOLS.map((t, i) => (
        <React.Fragment key={t.id}>
          {i === 1 && <div className="divider"></div>}
          <span {...tip(t.key, t.label)} style={{ display: 'inline-flex' }}>
            <ToolButton t={t} active={tool === t.id} onClick={() => pick(t)} />
          </span>
        </React.Fragment>
      ))}

      <div className="divider"></div>

      {/* current ink */}
      <span {...tip('C', 'Color')} style={{ position: 'relative', display: 'inline-flex' }}>
        <ToolButton t={{ label: 'Color' }} active={open === 'ink'} onClick={() => setOpen(open === 'ink' ? null : 'ink')}>
          <span className="swatch" style={{ background: color }}></span>
        </ToolButton>
        {open === 'ink' && <InkPopover color={color} onPick={(v) => { setColor(v); setOpen(null); }} />}
      </span>

      {/* thickness */}
      <span {...tip('G', 'Grosor')} style={{ position: 'relative', display: 'inline-flex' }}>
        <ToolButton t={{ label: 'Grosor' }} active={open === 'thick'} onClick={() => setOpen(open === 'thick' ? null : 'thick')}>
          <span className="thick-glyph"><span></span><span></span><span></span></span>
        </ToolButton>
        {open === 'thick' && <ThicknessPopover width={width} onPick={(w) => { setWidth(w); setOpen(null); }} />}
      </span>

      <div className="divider"></div>

      <span {...tip('Ctrl Z', 'Deshacer')} style={{ display: 'inline-flex' }}>
        <button className="tool" disabled={!canUndo} onClick={onUndo} aria-label="Deshacer"><Icon name="undo-2" /></button>
      </span>
      <span {...tip('Ctrl Y', 'Rehacer')} style={{ display: 'inline-flex' }}>
        <button className="tool" disabled={!canRedo} onClick={onRedo} aria-label="Rehacer"><Icon name="redo-2" /></button>
      </span>
      <span {...tip('Del', 'Limpiar todo')} style={{ display: 'inline-flex' }}>
        <button className="tool" disabled={!hasInk} onClick={onClear} aria-label="Limpiar todo"><Icon name="trash-2" /></button>
      </span>

      <div className="divider"></div>
      <span {...tip(ACT.capture.key, ACT.capture.label)} style={{ display: 'inline-flex' }}>
        <button className="tool" onClick={onCapture} aria-label={ACT.capture.label}><Icon name={ACT.capture.icon} /></button>
      </span>
      <span {...tip(ACT.settings.key, ACT.settings.label)} style={{ position: 'relative', display: 'inline-flex' }}>
        <ToolButton t={{ label: ACT.settings.label }} active={open === 'settings'} onClick={() => setOpen(open === 'settings' ? null : 'settings')}>
          <Icon name={ACT.settings.icon} />
        </ToolButton>
        {open === 'settings' && <SettingsPopover settings={settings} setSettings={setSettings} right />}
      </span>

      <div className="divider"></div>
      <span {...tip('Esc', 'Cerrar')} style={{ display: 'inline-flex' }}>
        <button className="tool" aria-label="Cerrar"><Icon name="x" /></button>
      </span>

      {hover && settings.hints && (
        <div className="tooltip" style={{ left: hover.cx, transform: 'translateX(-50%)' }}>
          {hover.label}
          {hover.key && <span className="kbd">{hover.key}</span>}
        </div>
      )}
    </div>
  );
}

window.Toolbar = Toolbar;
