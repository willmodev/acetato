// Acetato — app shell: backdrop slide + draw surface + floating toolbar + shortcuts.
const { useState: useS, useEffect: useE, useCallback: useC } = React;
const Icon = window.Icon;

function App() {
  const [tool, setTool] = useS('pencil');
  const [color, setColor] = useS('#FF3B30');
  const [width, setWidth] = useS(5);
  const [strokes, setStrokes] = useS([]);
  const [redo, setRedo] = useS([]);
  const [pos, setPos] = useS({ x: 0, y: 28 });
  const [settings, setSettings] = useS({ size: 'compact', smooth: true, snap: true, hints: true });
  const [flash, setFlash] = useS(false);
  const [toast, setToast] = useS(false);

  // center the bar horizontally on first paint
  useE(() => {
    const w = 560; // approx bar width
    setPos(p => ({ ...p, x: Math.max(16, (window.innerWidth - w) / 2) }));
  }, []);

  const capture = useC(() => {
    setFlash(true);
    setTimeout(() => setFlash(false), 340);
    setToast(true);
    setTimeout(() => setToast(false), 1700);
  }, []);

  const commit = useC((updater) => {
    setStrokes(prev => {
      const next = typeof updater === 'function' ? updater(prev) : updater;
      return next;
    });
    setRedo([]);
  }, []);

  const undo = useC(() => {
    setStrokes(prev => {
      if (!prev.length) return prev;
      setRedo(r => [...r, prev[prev.length - 1]]);
      return prev.slice(0, -1);
    });
  }, []);
  const redoFn = useC(() => {
    setRedo(prev => {
      if (!prev.length) return prev;
      const s = prev[prev.length - 1];
      setStrokes(st => [...st, s]);
      return prev.slice(0, -1);
    });
  }, []);
  const clear = useC(() => { setStrokes([]); setRedo([]); }, []);

  useE(() => {
    const onKey = (e) => {
      if (e.target.tagName === 'INPUT' || e.target.isContentEditable) return;
      const mod = e.ctrlKey || e.metaKey;
      if (mod && e.key.toLowerCase() === 'p') { e.preventDefault(); capture(); return; }
      if (mod && e.key.toLowerCase() === 'z') { e.preventDefault(); e.shiftKey ? redoFn() : undo(); return; }
      if (mod && e.key.toLowerCase() === 'y') { e.preventDefault(); redoFn(); return; }
      if (e.key === 'Delete' || e.key === 'Backspace') { clear(); return; }
      if (e.key === 'Escape') { setTool('select'); return; }
      const t = window.ACETATO_TOOLS.find(x => x.key.toLowerCase() === e.key.toLowerCase());
      if (t) setTool(t.id);
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [undo, redoFn, clear, capture]);

  return (
    <div className="app">
      <div className="slide">
        <span className="eyebrow">Resultados Q3</span>
        <h1>Crecimiento por trimestre</h1>
        <div className="chart">
          <div className="col" data-v="18" style={{ height: '34%' }}></div>
          <div className="col" data-v="27" style={{ height: '52%' }}></div>
          <div className="col" data-v="41" style={{ height: '74%' }}></div>
          <div className="col hl" data-v="63" style={{ height: '100%' }}></div>
        </div>
      </div>

      <DrawSurface tool={tool} color={color} width={width} strokes={strokes} setStrokes={commit} />

      <Toolbar
        pos={pos} setPos={setPos}
        tool={tool} setTool={setTool}
        color={color} setColor={setColor}
        width={width} setWidth={setWidth}
        onUndo={undo} onRedo={redoFn} onClear={clear} onCapture={capture}
        canUndo={strokes.length > 0} canRedo={redo.length > 0} hasInk={strokes.length > 0}
        settings={settings} setSettings={setSettings}
      />

      {flash && <div className="flash"></div>}
      {toast && (
        <div className="toast">
          <Icon name="camera" size={16} />
          Captura copiada al portapapeles
        </div>
      )}

      <div className="hint">Elige una herramienta y dibuja sobre la diapositiva · P lápiz · L línea · A flecha · R rectángulo · T texto · Ctrl+Z deshacer</div>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App />);
