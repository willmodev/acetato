// Acetato — app shell: backdrop slide + draw surface + floating toolbar +
// Settings window + configurable shortcuts (HotkeyCatalog).
const { useState: useS, useEffect: useE, useCallback: useC, useRef: useR, useMemo: useM } = React;

const DEFAULT_CHORDS = Object.fromEntries(window.ACETATO_HOTKEYS.map(d => [d.action, d.chord]));
const INK_ACTIONS = { inkRed: 'red', inkBlue: 'blue', inkYellow: 'yellow', inkGreen: 'green', inkWhite: 'white', inkBlack: 'black' };

function App() {
  const [tool, setTool] = useS('pencil');
  const [ink, setInk] = useS('red');
  const [gradient, setGradient] = useS(false);
  const [widthIndex, setWidthIndex] = useS(0); // starts at 3 px
  const [strokes, setStrokes] = useS([]);
  const [chords, setChords] = useS(DEFAULT_CHORDS);
  const [visible, setVisible] = useS(true);
  const [drawMode, setDrawMode] = useS(true);
  const [settingsOpen, setSettingsOpen] = useS(false);
  const [hint, setHint] = useS(null);
  const [pos, setPos] = useS({ x: 0, y: 28 });
  const lastPair = useR(-1);

  useE(() => { setPos(p => ({ ...p, x: Math.max(16, (window.innerWidth - 640) / 2) })); }, []);

  const flashHint = useC((text) => { setHint(text); setTimeout(() => setHint(null), 2200); }, []);

  // Each gradient stroke takes a random neon pair, never the previous one.
  const paint = useM(() => ({
    solid: window.ACETATO_INKS.find(i => i.id === ink).value,
    nextGradient: () => {
      if (!gradient) return null;
      const pairs = window.ACETATO_GRADIENTS;
      let i;
      do { i = Math.floor(Math.random() * pairs.length); } while (i === lastPair.current);
      lastPair.current = i;
      return pairs[i];
    },
  }), [ink, gradient]);

  const add = useC((s) => setStrokes(prev => [...prev, s]), []);
  const erase = useC((p) => setStrokes(prev => prev.filter(s => !window.ACETATO_HIT(s, p))), []);
  const undo = useC(() => setStrokes(prev => prev.slice(0, -1)), []);
  const clear = useC(() => setStrokes([]), []);
  const capture = useC(() => flashHint('Recortar abre la herramienta Recortes de Windows'), [flashHint]);
  const cycle = useC(() => setTool(t => {
    const ring = window.ACETATO_TOOL_RING;
    return ring[(ring.indexOf(t) + 1) % ring.length];
  }), []);

  const run = useC((action) => {
    if (INK_ACTIONS[action]) { setInk(INK_ACTIONS[action]); setGradient(false); return; }
    const table = {
      toggle: () => setVisible(v => !v),
      drawMode: () => setDrawMode(d => !d),
      gradient: () => setGradient(true),
      undo, clear, capture, cycle,
      thicker: () => setWidthIndex(i => Math.min(i + 1, 4)),
      thinner: () => setWidthIndex(i => Math.max(i - 1, 0)),
    };
    if (table[action]) table[action]();
    else if (window.ACETATO_TOOLS.some(t => t.action === action)) setTool(action);
  }, [undo, clear, capture, cycle]);

  useE(() => {
    const onKey = (e) => {
      if (settingsOpen || e.target.tagName === 'TEXTAREA') return;
      const chord = window.chordFromEvent(e);
      const def = chord && window.ACETATO_HOTKEYS.find(d => chords[d.action] === chord);
      if (!def) return;
      // "Solo al dibujar" shortcuts only work while the overlay is visible in drawing mode.
      if (def.scope === 'drawing' && !(visible && drawMode)) return;
      e.preventDefault();
      run(def.action);
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [chords, settingsOpen, visible, drawMode, run]);

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

      {visible && (
        <DrawSurface tool={tool} enabled={drawMode} paint={paint} widthIndex={widthIndex}
                     strokes={strokes} onAdd={add} onErase={erase} />
      )}

      {visible && (
        <Toolbar
          pos={pos} setPos={setPos}
          tool={tool} setTool={setTool}
          ink={ink} gradient={gradient} setInk={setInk} setGradient={setGradient}
          widthIndex={widthIndex} setWidthIndex={setWidthIndex}
          chords={chords}
          onUndo={undo} onClear={clear} onCapture={capture}
          onSettings={() => setSettingsOpen(true)} onClose={() => setVisible(false)}
        />
      )}

      {settingsOpen && (
        <div className="settings-backdrop">
          <SettingsWindow chords={chords} setChords={setChords} onClose={() => setSettingsOpen(false)} />
        </div>
      )}

      <div className="hint">
        {hint || (!visible
          ? `Acetato oculto · ${chords.toggle || 'sin atajo'} lo muestra`
          : !drawMode
            ? `Modo dibujo apagado: los clics pasan a la diapositiva · ${chords.drawMode || 'sin atajo'}`
            : [['pencil', 'lápiz'], ['laser', 'láser'], ['arrow', 'flecha'], ['gradient', 'degradado'], ['undo', 'deshacer']]
                .filter(([a]) => chords[a]).map(([a, n]) => `${chords[a]} ${n}`).join(' · '))}
      </div>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App />);
