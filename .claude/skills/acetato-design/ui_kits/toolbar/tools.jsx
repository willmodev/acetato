// Acetato — catalogs mirrored from the app (ToolbarViewModel, HotkeyCatalog,
// HotkeyActionLabels, ThicknessScale, FontScale, Tokens.xaml) + the Icon component.

// Toolbar tools, in bar order. `action` links each tool to its shortcut.
const ACETATO_TOOLS = [
  { id: 'select', icon: 'mouse-pointer-2', label: 'Seleccionar', enabled: false },
  { id: 'pencil', icon: 'pencil',          label: 'Lápiz',       enabled: true, action: 'pencil' },
  { id: 'laser',  icon: 'locate',          label: 'Láser',       enabled: true, action: 'laser' },
  { id: 'eraser', icon: 'eraser',          label: 'Borrador',    enabled: true, action: 'eraser' },
  { id: 'line',   icon: 'minus',           label: 'Línea',       enabled: true, action: 'line' },
  { id: 'arrow',  icon: 'arrow-up-right',  label: 'Flecha',      enabled: true, action: 'arrow' },
  { id: 'rect',   icon: 'square',          label: 'Rectángulo',  enabled: true, action: 'rect' },
  { id: 'text',   icon: 'type',            label: 'Texto',       enabled: true, action: 'text' },
];

// Ctrl+Espacio cycles: lápiz → láser → línea → flecha → rectángulo → texto → borrador.
const ACETATO_TOOL_RING = ['pencil', 'laser', 'line', 'arrow', 'rect', 'text', 'eraser'];

// Hotkey catalog (HotkeyCatalog): scope 'always' = Ctrl+Alt, 'drawing' = Ctrl.
const ACETATO_HOTKEYS = [
  { action: 'toggle',    label: 'Mostrar/ocultar',       scope: 'always',  chord: 'Ctrl+Alt+D' },
  { action: 'drawMode',  label: 'Modo dibujo',           scope: 'always',  chord: 'Ctrl+Alt+E' },
  { action: 'inkRed',    label: 'Tinta roja',            scope: 'drawing', chord: 'Ctrl+1' },
  { action: 'inkBlue',   label: 'Tinta azul',            scope: 'drawing', chord: 'Ctrl+2' },
  { action: 'inkYellow', label: 'Tinta amarilla',        scope: 'drawing', chord: 'Ctrl+3' },
  { action: 'inkGreen',  label: 'Tinta verde',           scope: 'drawing', chord: 'Ctrl+4' },
  { action: 'inkWhite',  label: 'Tinta blanca',          scope: 'drawing', chord: 'Ctrl+5' },
  { action: 'inkBlack',  label: 'Tinta negra',           scope: 'drawing', chord: 'Ctrl+6' },
  { action: 'gradient',  label: 'Degradado',             scope: 'drawing', chord: 'Ctrl+7' },
  { action: 'undo',      label: 'Deshacer',              scope: 'drawing', chord: 'Ctrl+Z' },
  { action: 'clear',     label: 'Limpiar todo',          scope: 'drawing', chord: 'Ctrl+Retroceso' },
  { action: 'cycle',     label: 'Siguiente herramienta', scope: 'drawing', chord: 'Ctrl+Espacio' },
  { action: 'pencil',    label: 'Lápiz',                 scope: 'drawing', chord: 'Ctrl+P' },
  { action: 'laser',     label: 'Láser',                 scope: 'drawing', chord: 'Ctrl+L' },
  { action: 'eraser',    label: 'Borrador',              scope: 'drawing', chord: 'Ctrl+B' },
  { action: 'line',      label: 'Línea',                 scope: 'drawing', chord: 'Ctrl+I' },
  { action: 'arrow',     label: 'Flecha',                scope: 'drawing', chord: 'Ctrl+F' },
  { action: 'rect',      label: 'Rectángulo',            scope: 'drawing', chord: 'Ctrl+R' },
  { action: 'text',      label: 'Texto',                 scope: 'drawing', chord: 'Ctrl+T' },
  { action: 'thicker',   label: 'Más grueso',            scope: 'drawing', chord: 'Ctrl+RePág' },
  { action: 'thinner',   label: 'Más fino',              scope: 'drawing', chord: 'Ctrl+AvPág' },
  { action: 'capture',   label: 'Recortar',              scope: 'drawing', chord: 'Ctrl+S' },
];

// Solid inks (Ink.*), in swatch and Ctrl+1…6 order.
const ACETATO_INKS = [
  { id: 'red',    value: '#FF3B30', label: 'Tinta roja' },
  { id: 'blue',   value: '#0A84FF', label: 'Tinta azul' },
  { id: 'yellow', value: '#FFD60A', label: 'Tinta amarilla' },
  { id: 'green',  value: '#30D158', label: 'Tinta verde' },
  { id: 'white',  value: '#FFFFFF', label: 'Tinta blanca' },
  { id: 'black',  value: '#0A0A0A', label: 'Tinta negra' },
];

// Gradient mode neon pairs (Ink.Gradient.*), start → end.
const ACETATO_GRADIENTS = [
  ['#2D6BFF', '#00E5FF'], ['#FFE600', '#B6FF00'], ['#FF2DAA', '#FF2D2D'],
  ['#9B3DFF', '#FF3DCB'], ['#22FF7A', '#00E5FF'], ['#FF8A00', '#FF3D9A'],
];

const ACETATO_WIDTHS = [3, 5, 8, 12, 18];        // ThicknessScale (px)
const ACETATO_FONT_SIZES = [16, 22, 30, 42, 60]; // FontScale, per thickness step

// Key names as the app writes them (KeyChordText).
const ACETATO_KEY_NAMES = {
  ' ': 'Espacio', Backspace: 'Retroceso', Delete: 'Supr', Home: 'Inicio', End: 'Fin',
  PageUp: 'RePág', PageDown: 'AvPág', ArrowLeft: '←', ArrowUp: '↑', ArrowRight: '→', ArrowDown: '↓',
};

// Turns a keydown into "Ctrl+Alt+D" style text, or null for a bare modifier.
function chordFromEvent(e) {
  if (['Control', 'Alt', 'Shift', 'Meta'].includes(e.key)) return null;
  const parts = [];
  if (e.ctrlKey) parts.push('Ctrl');
  if (e.altKey) parts.push('Alt');
  if (e.shiftKey) parts.push('Mayús');
  if (e.metaKey) parts.push('Win');
  const named = ACETATO_KEY_NAMES[e.key];
  const key = named || (/^Digit\d$/.test(e.code) ? e.code.slice(5) : e.key.length === 1 ? e.key.toUpperCase() : e.key);
  parts.push(key);
  return parts.join('+');
}

window.ACETATO_TOOLS = ACETATO_TOOLS;
window.ACETATO_TOOL_RING = ACETATO_TOOL_RING;
window.ACETATO_HOTKEYS = ACETATO_HOTKEYS;
window.ACETATO_INKS = ACETATO_INKS;
window.ACETATO_GRADIENTS = ACETATO_GRADIENTS;
window.ACETATO_WIDTHS = ACETATO_WIDTHS;
window.ACETATO_FONT_SIZES = ACETATO_FONT_SIZES;
window.chordFromEvent = chordFromEvent;

// React-safe Lucide icon: reads the icon-node array from the UMD global and
// renders an inline <svg> (no DOM mutation, so React reconciliation is happy).
function pascal(name) {
  return name.split('-').map(s => s.charAt(0).toUpperCase() + s.slice(1)).join('');
}
function Icon({ name, size = 20, stroke = 2 }) {
  const node = window.lucide && window.lucide[pascal(name)];
  if (!node) return null;
  return React.createElement(
    'svg',
    {
      width: size, height: size, viewBox: '0 0 24 24', fill: 'none',
      stroke: 'currentColor', strokeWidth: stroke, strokeLinecap: 'round', strokeLinejoin: 'round',
    },
    node.map((child, i) => React.createElement(child[0], Object.assign({ key: i }, child[1])))
  );
}
window.Icon = Icon;
