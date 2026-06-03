---
description: "Genera commits en español con Conventional Commits + emojis"
allowed-tools: ["Bash(git add:*)", "Bash(git status:*)", "Bash(git commit:*)", "Bash(git diff:*)"]
---

Actúa como un Senior Developer. Analiza el diff actual con `git diff HEAD` y `git status`, luego genera un commit en español.

## Formato requerido: Conventional Commits

`<emoji> <type>(<scope>): <subject>`

### Tipos y emojis (usar EXACTAMENTE estos):
- feat: ✨
- fix: 🐛
- docs: 📚
- style: 🎨
- refactor: ♻️
- test: ✅
- chore: 🛠️

### Reglas:
1. El subject debe estar en modo imperativo y ser conciso (máx 70 chars).
2. Usa los emojis exactamente como están listados arriba.
3. Después del título, agrega una lista con los cambios principales realizados.
4. NO agregues el footer de co-autoría de Claude.

### Ejemplo de salida esperada:
```
✨ feat(reports): actualizar dashboard de caracterización con nuevas métricas

- Se actualiza la vista SQL y el modelo para incluir datos de país, nivel de ventas, antigüedad y tamaño de empresa.
- Se eliminan los cálculos y campos relacionados con los niveles de madurez.
- Se implementan nuevos filtros por nivel de ventas y tamaño de empresa en el controlador.
- Se agrega el cálculo de distribución por antigüedad de la empresa.
```

Ejecuta el commit con el mensaje generado usando `git add -A && git commit -m "..."`.
