// Acetato — the transparent drawing surface (canvas) above the "desktop".
// Mirrors the app: smoothed freehand, arrow head on release, rounded rectangles,
// Gradient mode, whole-stroke eraser, multiline text and the fading laser.
const { useRef, useEffect, useState, useCallback } = React;

const DRAW_TOOLS = ['pencil', 'line', 'arrow', 'rect'];
const LASER_LIFE = 2000; // ms, linear fade (LaserFade)
const LASER_HALO = 22;   // px

function dist(a, b) { return Math.hypot(a.x - b.x, a.y - b.y); }

// Gradient axis: first → last point; closed figures run along the bbox diagonal.
function strokePaint(ctx, s) {
  if (!s.grad) return s.color;
  const pts = s.points;
  let a = pts[0], b = pts[pts.length - 1];
  if (s.tool === 'rect' || dist(a, b) < 2 * s.width) {
    const xs = pts.map(p => p.x), ys = pts.map(p => p.y);
    a = { x: Math.min(...xs), y: Math.min(...ys) };
    b = { x: Math.max(...xs), y: Math.max(...ys) };
  }
  if (dist(a, b) < 1) return s.grad[0];
  const g = ctx.createLinearGradient(a.x, a.y, b.x, b.y);
  g.addColorStop(0, s.grad[0]);
  g.addColorStop(1, s.grad[1]);
  return g;
}

function smoothPath(ctx, pts) {
  ctx.moveTo(pts[0].x, pts[0].y);
  for (let i = 1; i < pts.length - 1; i++) {
    const mx = (pts[i].x + pts[i + 1].x) / 2, my = (pts[i].y + pts[i + 1].y) / 2;
    ctx.quadraticCurveTo(pts[i].x, pts[i].y, mx, my);
  }
  const last = pts[pts.length - 1];
  ctx.lineTo(last.x, last.y);
}

// Open two-wing head aimed by the final stretch (not the last point), 3 × width.
function arrowHead(ctx, pts, width) {
  const tip = pts[pts.length - 1];
  const len = Math.max(12, width * 3);
  let back = pts[0];
  for (let i = pts.length - 2; i >= 0; i--) { if (dist(pts[i], tip) >= len) { back = pts[i]; break; } }
  const ang = Math.atan2(tip.y - back.y, tip.x - back.x);
  ctx.moveTo(tip.x - len * Math.cos(ang - Math.PI / 6), tip.y - len * Math.sin(ang - Math.PI / 6));
  ctx.lineTo(tip.x, tip.y);
  ctx.lineTo(tip.x - len * Math.cos(ang + Math.PI / 6), tip.y - len * Math.sin(ang + Math.PI / 6));
}

function drawStroke(ctx, s, live) {
  ctx.lineCap = 'round';
  ctx.lineJoin = 'round';
  if (s.tool === 'text') {
    ctx.fillStyle = s.color;
    ctx.font = `600 ${s.size}px "Segoe UI Variable","Public Sans",system-ui,sans-serif`;
    ctx.textBaseline = 'top';
    s.text.split('\n').forEach((line, i) => ctx.fillText(line, s.x, s.y + i * s.size * 1.2));
    return;
  }
  const pts = s.points;
  if (!pts || pts.length < 2) return;
  // Pencil and arrow show the pair's first color while drawing; gradient on release.
  const freehand = s.tool === 'pencil' || s.tool === 'arrow';
  ctx.strokeStyle = live && freehand && s.grad ? s.grad[0] : strokePaint(ctx, s);
  ctx.lineWidth = s.width;
  ctx.beginPath();
  const a = pts[0], b = pts[pts.length - 1];
  if (freehand) {
    smoothPath(ctx, pts);
    if (s.tool === 'arrow' && !live) arrowHead(ctx, pts, s.width);
  } else if (s.tool === 'line') {
    ctx.moveTo(a.x, a.y); ctx.lineTo(b.x, b.y);
  } else if (s.tool === 'rect') {
    const w = Math.abs(b.x - a.x), h = Math.abs(b.y - a.y);
    ctx.roundRect(Math.min(a.x, b.x), Math.min(a.y, b.y), w, h, Math.min(2 * s.width, Math.min(w, h) / 2));
  }
  ctx.stroke();
}

function hitStroke(s, p) {
  if (s.tool === 'text') {
    const lines = s.text.split('\n');
    const w = Math.max(...lines.map(l => l.length)) * s.size * 0.55;
    return p.x >= s.x && p.x <= s.x + w && p.y >= s.y && p.y <= s.y + lines.length * s.size * 1.2;
  }
  return s.points.some(q => dist(q, p) <= s.width / 2 + 8);
}

function DrawSurface({ tool, enabled, paint, widthIndex, strokes, onAdd, onErase }) {
  const canvasRef = useRef(null);
  const laserRef = useRef(null);
  const drawingRef = useRef(null);
  const trailRef = useRef([]);       // laser points { x, y, t, starts }
  const haloRef = useRef(null);
  const [draft, setDraftState] = useState(null); // { x, y, value }
  const draftRef = useRef(null);                  // latest draft, read by commit/blur
  const setDraft = (d) => { draftRef.current = d; setDraftState(d); };
  const draftKey = (d) => d ? d.x + ',' + d.y : '';
  const width = window.ACETATO_WIDTHS[widthIndex];
  const fontSize = window.ACETATO_FONT_SIZES[widthIndex];

  const fit = (c) => {
    const dpr = window.devicePixelRatio || 1;
    const r = c.getBoundingClientRect();
    c.width = Math.round(r.width * dpr); c.height = Math.round(r.height * dpr);
    const ctx = c.getContext('2d'); ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    return ctx;
  };

  const render = useCallback(() => {
    const c = canvasRef.current;
    if (!c) return;
    const ctx = c.getContext('2d');
    ctx.clearRect(0, 0, c.width, c.height);
    for (const s of strokes) drawStroke(ctx, s, false);
    if (drawingRef.current) drawStroke(ctx, drawingRef.current, true);
  }, [strokes]);

  useEffect(() => {
    const onResize = () => { fit(canvasRef.current); fit(laserRef.current); render(); };
    onResize();
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, [render]);

  // Laser: ephemeral trail redrawn every frame, never stored as a stroke.
  useEffect(() => {
    let raf;
    const tick = () => {
      const c = laserRef.current;
      const ctx = c.getContext('2d');
      ctx.clearRect(0, 0, c.width, c.height);
      const now = performance.now();
      trailRef.current = trailRef.current.filter(p => now - p.t < LASER_LIFE);
      const pts = trailRef.current;
      ctx.lineCap = 'round'; ctx.lineWidth = 3; ctx.strokeStyle = paint.solid;
      for (let i = 1; i < pts.length; i++) {
        if (pts[i].starts) continue;
        ctx.globalAlpha = Math.max(0, 1 - (now - pts[i].t) / LASER_LIFE);
        ctx.beginPath(); ctx.moveTo(pts[i - 1].x, pts[i - 1].y); ctx.lineTo(pts[i].x, pts[i].y); ctx.stroke();
      }
      ctx.globalAlpha = 1;
      if (tool === 'laser' && haloRef.current) {
        const h = haloRef.current, g = ctx.createRadialGradient(h.x, h.y, 0, h.x, h.y, LASER_HALO / 2);
        g.addColorStop(0, paint.solid); g.addColorStop(1, 'transparent');
        ctx.fillStyle = g; ctx.beginPath(); ctx.arc(h.x, h.y, LASER_HALO / 2, 0, Math.PI * 2); ctx.fill();
      }
      raf = requestAnimationFrame(tick);
    };
    raf = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(raf);
  }, [tool, paint.solid]);

  const pos = (e) => {
    const r = canvasRef.current.getBoundingClientRect();
    return { x: e.clientX - r.left, y: e.clientY - r.top };
  };

  const commitDraft = () => {
    const d = draftRef.current;
    if (d && d.value.trim()) {
      onAdd({ tool: 'text', color: paint.solid, x: d.x, y: d.y, text: d.value, size: fontSize });
    }
    setDraft(null);
  };

  function onPointerDown(e) {
    const p = pos(e);
    if (tool === 'text') {
      // Avoid the focus change (and its blur commit): commit here, then open a new box.
      e.preventDefault();
      commitDraft();
      setDraft({ x: p.x, y: p.y, value: '' });
      return;
    }
    e.currentTarget.setPointerCapture(e.pointerId);
    if (tool === 'eraser') { onErase(p); return; }
    if (tool === 'laser') { trailRef.current.push({ ...p, t: performance.now(), starts: true }); return; }
    drawingRef.current = { tool, width, color: paint.solid, grad: paint.nextGradient(), points: [p] };
  }
  function onPointerMove(e) {
    const p = pos(e);
    haloRef.current = p;
    if (!(e.buttons & 1)) return;
    if (tool === 'eraser') { onErase(p); return; }
    if (tool === 'laser') { trailRef.current.push({ ...p, t: performance.now(), starts: false }); return; }
    if (drawingRef.current) { drawingRef.current.points.push(p); render(); }
  }
  function onPointerUp() {
    const s = drawingRef.current;
    drawingRef.current = null;
    if (!s) return;
    const len = dist(s.points[0], s.points[s.points.length - 1]);
    // An arrow shorter than its head is discarded, like in the app.
    if (s.points.length < 2 || (s.tool === 'arrow' && len < Math.max(12, s.width * 3))) { render(); return; }
    onAdd(s);
  }

  const cursor = tool === 'eraser' ? 'cursor-erase' : tool === 'text' ? 'cursor-text'
    : tool === 'laser' ? 'cursor-laser' : 'cursor-draw';

  return (
    <div className={'draw-layer ' + cursor} style={{ pointerEvents: enabled ? 'auto' : 'none' }}
         onPointerLeave={() => { haloRef.current = null; }}>
      <canvas ref={canvasRef} onPointerDown={onPointerDown} onPointerMove={onPointerMove} onPointerUp={onPointerUp} />
      <canvas ref={laserRef} className="laser-layer" />
      {draft && (
        <textarea
          key={draftKey(draft)} data-k={draftKey(draft)}
          className="text-draft" autoFocus rows={Math.max(1, draft.value.split('\n').length)}
          value={draft.value} placeholder="Texto"
          style={{ left: draft.x, top: draft.y, color: paint.solid, fontSize, caretColor: paint.solid }}
          onChange={(e) => setDraft({ ...draft, value: e.target.value })}
          onKeyDown={(e) => { if (e.key === 'Escape') commitDraft(); }}
          onBlur={(e) => { if (e.target.dataset.k === draftKey(draftRef.current)) commitDraft(); }}
        />
      )}
    </div>
  );
}

window.DrawSurface = DrawSurface;
window.ACETATO_HIT = hitStroke;
window.ACETATO_DRAW_TOOLS = DRAW_TOOLS;
