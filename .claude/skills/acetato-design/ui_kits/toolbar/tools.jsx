// Acetato — tool catalog. Shared by Toolbar and keyboard shortcuts.
const ACETATO_TOOLS = [
  { id: 'select', icon: 'mouse-pointer-2', label: 'Seleccionar', key: 'V', kind: 'mode' },
  { id: 'pencil', icon: 'pencil',          label: 'Lápiz',        key: 'P', kind: 'draw' },
  { id: 'line',   icon: 'minus',            label: 'Línea recta',  key: 'L', kind: 'draw' },
  { id: 'arrow',  icon: 'arrow-up-right',   label: 'Flecha',       key: 'A', kind: 'draw' },
  { id: 'rect',   icon: 'square',           label: 'Rectángulo',   key: 'R', kind: 'draw' },
  { id: 'text',   icon: 'type',             label: 'Texto',        key: 'T', kind: 'text' },
  { id: 'eraser', icon: 'eraser',           label: 'Borrador',     key: 'E', kind: 'draw' },
];

// Actions (not drawing modes) shown to the right of the tools.
const ACETATO_ACTIONS = {
  capture:  { icon: 'camera',     label: 'Captura de pantalla', key: 'Ctrl+P' },
  settings: { icon: 'settings-2', label: 'Ajustes',             key: '' },
};

const ACETATO_INKS = [
  { id: 'red',    value: '#FF3B30', label: 'Rojo' },
  { id: 'blue',   value: '#0A84FF', label: 'Azul' },
  { id: 'yellow', value: '#FFD60A', label: 'Amarillo' },
  { id: 'green',  value: '#30D158', label: 'Verde' },
  { id: 'white',  value: '#FFFFFF', label: 'Blanco' },
  { id: 'black',  value: '#0A0A0A', label: 'Negro' },
];

// Stroke widths (px) mapped to the 5-step thickness stepper.
const ACETATO_WIDTHS = [3, 5, 8, 12, 18];

window.ACETATO_TOOLS = ACETATO_TOOLS;
window.ACETATO_ACTIONS = ACETATO_ACTIONS;
window.ACETATO_INKS = ACETATO_INKS;
window.ACETATO_WIDTHS = ACETATO_WIDTHS;

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
