# SPEC 07 — Recorte de pantalla con la herramienta de Windows (HU-23)

> **Estado:** Aprobado · **Depende de:** HU-12 (captura actual) y SPEC 05 (HU-21) — atajos configurables · **Fecha:** 2026-10-11
> **Objetivo:** Que el atajo de captura deje elegir el área a guardar, en vez de copiar todos los monitores juntos, usando el recorte nativo de Windows.

---

## Por qué existe este spec

- Hoy `Ctrl+S` guarda **todo el escritorio virtual**: con dos monitores sale una sola imagen con las dos pantallas pegadas. No se puede elegir un monitor ni un área.
- Hacer un recorte propio (pantalla congelada, rectángulo, `Esc`) cuesta 2–3 días y es delicado con monitores de distinta escala. Windows ya lo resuelve con su herramienta Recortes (`Win+Shift+S`), que además copia al portapapeles y permite editar.
- La herramienta de Windows se puede abrir desde otra app con el protocolo `ms-screenclip:` (registrado en este equipo, Recortes 11.2607). Acetato solo tiene que abrirla: los trazos ya están en pantalla y la barra ya se excluye de las capturas (`WDA_EXCLUDEFROMCAPTURE`).

### Alternativas descartadas

| Opción | Por qué no |
|--------|-----------|
| Recorte propio en Acetato | 2–3 días y más código que mantener para algo que Windows ya hace. |
| Capturar solo el monitor del cursor | Arregla «las dos pantallas juntas», pero sigue sin dejar elegir el área. |
| Quitar la captura | Se pierde el atajo dentro de Acetato, que es lo único que aporta frente a `Win+Shift+S`. |

---

## Alcance

**Dentro:**

- La acción `Capture` (`Ctrl+S`, grupo Solo al dibujar) **abre el recorte de Windows** en lugar de guardar el escritorio completo.
  - El nombre en Ajustes pasa de «Captura» a **«Recortar»**. El valor interno `Capture` no cambia, para que los `settings.json` existentes sigan valiendo.
- **Qué hace Windows desde ahí** (no es código de Acetato): seleccionar área, ventana, pantalla completa o forma libre; `Esc` cancela; la imagen va al portapapeles y Windows muestra su propio aviso para abrirla y editarla. Si el usuario tiene activado el guardado automático de Recortes, también queda en su carpeta de capturas de pantalla.
- **Se elimina el código de la captura propia**: el guardado en `Imágenes\Acetato`, el aviso propio de «Captura guardada» y la copia del escritorio virtual.
- Si Windows no puede abrir el recorte (protocolo ausente o bloqueado por política), la falla se traga sin romper la app, igual que hoy con las fallas de E/S.
- Alta de **HU-23** en `BACKLOG.md` y nota en HU-12 de que la reemplaza.

**Fuera de alcance:**

- Recorte propio dentro de Acetato (se reabre solo si la verificación del paso 1 falla sin arreglo).
- Guardar automáticamente en una carpeta de Acetato o elegir la carpeta.
- Botón de recorte en la barra o ítem en el menú de la bandeja.
- Grabación de video.

---

## Modelo de datos y cambios por capa

- **Domain:** se borra `VirtualDesktopBounds` (y sus tests) si tras el cambio no lo usa nadie más. Hoy solo lo usa la captura.
- **Application:**
  - Nuevo puerto `IScreenClipLauncher` en `Abstractions/`, con un método `Launch()`.
  - `OverlayBroadcaster.CaptureAsync` pasa a `LaunchScreenClip()` (síncrono: abrir la herramienta es instantáneo) y traga la falla de apertura.
  - Se borran `CaptureService`, `ICaptureService`, `CaptureResult`, `IScreenCaptureService`, `ICaptureDirectoryProvider` e `ICaptureNotifier`, con sus tests.
- **Infrastructure:**
  - Nuevo `WindowsScreenClipLauncher`: `Process.Start` de `ms-screenclip:` con `UseShellExecute = true`.
  - Se borran `GdiScreenCaptureService`, `PicturesCaptureDirectoryProvider` y `ToastCaptureNotifier`.
  - El paquete de toasts (`CommunityToolkit.WinUI.Notifications`) **se queda**: lo usa el aviso de atajos no disponibles del SPEC 05.
- **Presentation:**
  - `HotkeyActionRouter`: `Capture` llama a `broadcaster.LaunchScreenClip()`.
  - `HotkeyActionLabels`: «Recortar».
  - `App`: registrar el lanzador y quitar los registros de la captura propia.
- **Persistencia:** sin cambios.

---

## Plan de implementación

1. **Verificación manual (antes de tocar código).** Con Acetato en modo dibujo y trazos en los dos monitores, abrir el recorte con `Win+R` → `ms-screenclip:`. Comprobar:
   - a. La selección de Windows aparece **por encima** del lienzo y se puede arrastrar en cualquier monitor.
   - b. Los **trazos salen** en la imagen y la **barra no**.
   - c. Al cancelar con `Esc` o al terminar, Acetato sigue en modo dibujo y con los trazos.

   Si falla **a** → probar a pasar el overlay a click-through justo antes de abrir el recorte (los trazos siguen visibles) y repetir. Si falla **b** con la barra → ocultarla durante el recorte. Si nada funciona → parar y replantear (recorte propio).
2. **Backlog.** HU-23 en `BACKLOG.md` y nota en HU-12. Solo docs.
3. **Lanzador.** Puerto, adaptador y cambio del broadcaster y el router. Tests: el atajo llama al lanzador; una falla al abrir no se propaga. **Manual:** `Ctrl+S` en modo dibujo abre la selección de Windows.
4. **Limpieza.** Borrar la captura propia, sus registros y tests; renombrar la etiqueta. `dotnet build -warnaserror` y `dotnet test` verdes.

---

## Criterios de aceptación

- [ ] En modo dibujo, `Ctrl+S` abre la selección de área de Windows, y se puede elegir un área de **un solo monitor**.
- [ ] La imagen recortada incluye los trazos visibles y **no** incluye la barra de Acetato.
- [ ] Cancelar con `Esc` no guarda nada y deja Acetato como estaba (modo dibujo y trazos).
- [ ] Con el overlay oculto o en click-through, `Ctrl+S` sigue guardando en las demás apps (igual que hoy).
- [ ] En Ajustes la acción se llama «Recortar» y se puede cambiar, quitar y restaurar; un `settings.json` existente sigue funcionando.
- [ ] Ya no se crea nada en `Imágenes\Acetato` ni aparece el aviso «Captura guardada».
- [ ] `dotnet build -warnaserror` y `dotnet test` verdes; `BACKLOG.md` incluye HU-23.

---

## Riesgos

- **Aviso de Windows con el overlay a pantalla completa.** Windows puede mandar el aviso de Recortes al centro de notificaciones en vez de mostrarlo en pantalla, como ya pasa con los avisos propios de Acetato. La imagen igual queda en el portapapeles. Se comprueba en el paso 1.
- **Equipos sin Recortes** (Windows recortado o política corporativa). El atajo no hace nada; aceptable para una app personal.
- **Cambios de Microsoft** en el protocolo `ms-screenclip:`. Existe desde la llegada de Recorte y anotación a Windows 10, pero no lo controlamos.
