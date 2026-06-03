// Acetato — the transparent drawing surface (HTML canvas) above the app.
const { useRef, useEffect, useState, useCallback } = React;

const DRAW_TOOLS = ['pencil', 'eraser', 'rect', 'arrow', 'line'];

function DrawSurface({ tool, color, width, strokes, setStrokes }) {
  const canvasRef = useRef(null);
  const dprRef = useRef(1);
  const drawingRef = useRef(null);   // in-progress stroke
  const [draft, setDraft] = useState(null); // { x, y, value } text being typed

  const resize = useCallback(() => {
    const c = canvasRef.current;
    if (!c) return;
    const dpr = window.devicePixelRatio || 1;
    dprRef.current = dpr;
    const r = c.getBoundingClientRect();
    c.width = Math.round(r.width * dpr);
    c.height = Math.round(r.height * dpr);
    render();
  }, [strokes]);

  useEffect(() => {
    resize();
    window.addEventListener('resize', resize);
    return () => window.removeEventListener('resize', resize);
  }, [resize]);

  useEffect(() => { render(); }, [strokes]);

  const fontFor = (w) => Math.round(13 + w * 1.8);

  function drawStroke(ctx, s) {
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.globalCompositeOperation = s.tool === 'eraser' ? 'destination-out' : 'source-over';
    ctx.strokeStyle = s.color;
    ctx.fillStyle = s.color;
    ctx.lineWidth = s.width;

    if (s.tool === 'text') {
      ctx.font = `600 ${s.size}px "Segoe UI Variable","Public Sans",system-ui,sans-serif`;
      ctx.textBaseline = 'top';
      ctx.fillText(s.text, s.x, s.y);
      return;
    }

    const pts = s.points || [];
    if (!pts.length) return;

    if (s.tool === 'pencil' || s.tool === 'eraser') {
      ctx.beginPath();
      ctx.moveTo(pts[0].x, pts[0].y);
      for (let i = 1; i < pts.length; i++) ctx.lineTo(pts[i].x, pts[i].y);
      ctx.stroke();
    } else if (s.tool === 'line') {
      const a = pts[0], b = pts[pts.length - 1];
      ctx.beginPath(); ctx.moveTo(a.x, a.y); ctx.lineTo(b.x, b.y); ctx.stroke();
    } else if (s.tool === 'rect') {
      const a = pts[0], b = pts[pts.length - 1];
      ctx.strokeRect(Math.min(a.x, b.x), Math.min(a.y, b.y), Math.abs(b.x - a.x), Math.abs(b.y - a.y));
    } else if (s.tool === 'arrow') {
      const a = pts[0], b = pts[pts.length - 1];
      ctx.beginPath(); ctx.moveTo(a.x, a.y); ctx.lineTo(b.x, b.y); ctx.stroke();
      const ang = Math.atan2(b.y - a.y, b.x - a.x);
      const head = Math.max(12, s.width * 3);
      ctx.beginPath();
      ctx.moveTo(b.x, b.y);
      ctx.lineTo(b.x - head * Math.cos(ang - Math.PI / 6), b.y - head * Math.sin(ang - Math.PI / 6));
      ctx.moveTo(b.x, b.y);
      ctx.lineTo(b.x - head * Math.cos(ang + Math.PI / 6), b.y - head * Math.sin(ang + Math.PI / 6));
      ctx.stroke();
    }
  }

  function render() {
    const c = canvasRef.current;
    if (!c) return;
    const ctx = c.getContext('2d');
    const dpr = dprRef.current;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    ctx.clearRect(0, 0, c.width, c.height);
    for (const s of strokes) drawStroke(ctx, s);
    if (drawingRef.current) drawStroke(ctx, drawingRef.current);
  }

  const isDraw = DRAW_TOOLS.includes(tool);
  const interactive = isDraw || tool === 'text';

  function pos(e) {
    const r = canvasRef.current.getBoundingClientRect();
    return { x: e.clientX - r.left, y: e.clientY - r.top };
  }

  function commitDraft() {
    setDraft(d => {
      if (d && d.value.trim()) {
        setStrokes(prev => [...prev, {
          tool: 'text', color, x: d.x, y: d.y, text: d.value, size: fontFor(width), width,
        }]);
      }
      return null;
    });
  }

  function onPointerDown(e) {
    if (tool === 'text') {
      // commit any open draft, then open a new one
      const p = pos(e);
      if (draft && draft.value.trim()) {
        setStrokes(prev => [...prev, {
          tool: 'text', color, x: draft.x, y: draft.y, text: draft.value, size: fontFor(width), width,
        }]);
      }
      setDraft({ x: p.x, y: p.y, value: '' });
      return;
    }
    if (!isDraw) return;
    e.currentTarget.setPointerCapture(e.pointerId);
    drawingRef.current = {
      tool, color, width: tool === 'eraser' ? Math.max(width * 2.4, 16) : width,
      points: [pos(e)],
    };
    render();
  }
  function onPointerMove(e) {
    if (!drawingRef.current) return;
    drawingRef.current.points.push(pos(e));
    render();
  }
  function onPointerUp() {
    if (!drawingRef.current) return;
    const s = drawingRef.current;
    drawingRef.current = null;
    if (['rect', 'arrow', 'line'].includes(s.tool) && s.points.length < 2) { render(); return; }
    setStrokes(prev => [...prev, s]);
  }

  const cursor = !interactive ? 'cursor-select'
    : tool === 'eraser' ? 'cursor-erase'
    : tool === 'text' ? 'cursor-text' : 'cursor-draw';

  return (
    <div className={'draw-layer ' + cursor} style={{ pointerEvents: interactive ? 'auto' : 'none' }}>
      <canvas
        ref={canvasRef}
        onPointerDown={onPointerDown}
        onPointerMove={onPointerMove}
        onPointerUp={onPointerUp}
      />
      {draft && (
        <input
          className="text-draft"
          autoFocus
          value={draft.value}
          style={{
            left: draft.x, top: draft.y, color,
            fontSize: fontFor(width), caretColor: color,
          }}
          placeholder="Escribe…"
          onChange={(e) => setDraft({ ...draft, value: e.target.value })}
          onKeyDown={(e) => { if (e.key === 'Enter') commitDraft(); if (e.key === 'Escape') setDraft(null); }}
          onBlur={commitDraft}
        />
      )}
    </div>
  );
}

window.DrawSurface = DrawSurface;
