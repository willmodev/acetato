# SPEC 09 — Barra plegable con el ojo y cursor de tinta (HU-25, HU-26)

> **Estado:** Implementado · **Depende de:** HU-06 (grosor), HU-10 (barra flotante), SPEC 03 (HU-19) — tintas y degradado, SPEC 05 (HU-21) — atajos configurables, SPEC 08 (HU-24) — modo puntero · **Fecha:** 2026-10-30
> **Objetivo:** Que el botón del ojo (o `Ctrl+Alt+H`) pliegue y despliegue la barra, y que al dibujar el cursor sea un círculo relleno del color y el grosor elegidos en lugar de la cruz.

---

## Por qué existe este spec

- La barra completa tapa parte de lo que se presenta aunque no se use. El botón del ojo ya está diseñado en el design system («designed, pending in the app»), pero no existe en la app.
- Con Lápiz, Línea, Flecha y Rectángulo el cursor es una cruz fina (`ToolCursorBehavior`). No dice con qué color ni con qué grosor se va a pintar: uno se entera al soltar el primer trazo. ScreenBrush o Epic Pen muestran un círculo con la tinta y el tamaño reales.
- La app ya elige el próximo par del degradado antes de dibujar (`IDrawingSettings.NextGradient`) y avisa cuando cambia, así que mostrarlo en el cursor no obliga a tocar el sorteo.

---

## Alcance

**Dentro — HU-25, barra plegable:**

- **Botón del ojo**, justo después del asa de arrastre y su separador, tal como lo describe el design system:
  - *Desplegada:* icono `eye`, tooltip `Ocultar barra  Ctrl+Alt+H`. Un clic pliega la barra y quedan solo **asa · separador · ojo**, con la misma superficie glass.
  - *Plegada:* icono `eye-off`, tooltip `Mostrar barra  Ctrl+Alt+H`. Un clic vuelve a mostrar la barra completa.
  - El asa y el ojo **no se mueven**: el borde izquierdo de la barra se queda fijo y el resto se recoge hacia ellos. La barra plegada se sigue arrastrando por el asa.
  - El ojo es un botón normal (color de reposo y hover). **Nunca** lleva el acento teal.
  - Al plegar se cierran los popovers de tinta y grosor.
  - Plegar oculta **solo** la barra. El modo (dibujo o puntero), la herramienta, la tinta, el grosor, los trazos y todos los atajos (incluido `Esc`) siguen funcionando igual.
  - El cambio de tamaño es instantáneo, sin animación.
- **Atajo nuevo «Plegar barra»**, `Ctrl+Alt+H`, en el grupo «Siempre» de Ajustes, configurable como los demás:
  - Funciona en modo dibujo y en modo puntero.
  - Con la capa oculta (Cerrar o `Ctrl+Alt+D`) no hace nada.
  - Si el usuario ya tenía `Ctrl+Alt+H` asignado a otra acción en Ajustes, gana su asignación y «Plegar barra» queda sin atajo hasta que lo reasigne.
- **Memoria solo durante la sesión:**
  - Si se oculta la capa con la barra plegada y se vuelve a mostrar, sigue plegada.
  - Al reiniciar la app, la barra siempre arranca desplegada. No se guarda nada en `settings.json`.

**Dentro — HU-26, cursor de tinta:**

- Con **Lápiz, Línea, Flecha y Rectángulo** en modo dibujo, el cursor pasa a ser un **círculo relleno**:
  - **Relleno:** la tinta activa (roja, azul, amarilla, verde, blanca o negra).
  - **En modo degradado:** el relleno es el degradado del par que va a salir en el **próximo** trazo; tras cada trazo el círculo cambia al par siguiente.
  - **Diámetro:** el del grosor activo (3, 5, 8, 12 o 18 DIP), con un mínimo visible de **6 DIP**. Con grosor 3 y 5 el círculo mide 6; con 8, 12 y 18 coincide con el trazo.
  - **Aro de contraste:** un aro exterior claro (blanco al 80 %) y uno interior oscuro (negro al 60 %), de 1 px cada uno, para que el círculo se vea sobre cualquier fondo.
  - **Punto activo en el centro** del círculo: el trazo empieza justo ahí.
- El círculo **sigue visible mientras se dibuja** (botón del mouse apretado).
- **Se actualiza al instante**, sin mover el mouse, al cambiar tinta, grosor o par del degradado, venga el cambio de un atajo, de la rueda del mouse o de la barra.
- **Mismo tamaño que el trazo en cada monitor:** con escalas distintas (p. ej. 100 % y 150 %), cada pantalla genera su círculo a su escala.
- **Es un cursor real de Windows** creado en memoria, no un dibujo que persigue al mouse; no se queda atrás al moverlo rápido.
- **Sin cambios:** Texto (barra de escribir), Láser (sin cursor, solo su brillo), Borrador (cursor normal) y modo puntero (cursor normal de Windows).

**Documentación:**

- Alta de **HU-25** y **HU-26** en `BACKLOG.md` (épica EP-02).
- En HU-24 se quita «botón de ojo» de la nota de fuera de alcance.
- Design system:
  - `README.md` del skill: el ojo deja de figurar como *pending*; `Icon.Eye`/`Icon.EyeOff` como portados; atajo `Ctrl+Alt+H`; sección corta del cursor de tinta.
  - `ui_kits/toolbar/README.md` actualizado.

**Fuera de alcance (specs futuros):**

- Recordar entre sesiones si la barra quedó plegada.
- Animar el pliegue de la barra.
- Cursor en círculo para el Borrador, o cambios en los cursores de Texto y Láser.
- Previsualizar la forma (línea, flecha, rectángulo) antes de hacer clic.
- Cambios en el botón Cerrar, la bandeja o la herramienta Seleccionar (HU-14).
- Actualizar la maqueta interactiva del design system (`ui_kits/toolbar/*.jsx`), que ya tiene el ojo.

---

## Modelo de datos y cambios por capa

No se guarda nada nuevo. El único valor que puede aparecer en `settings.json` es el atajo, si el usuario lo cambia, como cualquier otro.

- **Domain:**
  - `HotkeyKey.H` (nuevo valor, si no existe ya).
  - `CursorTipSize` (nuevo, estático): diámetro del círculo en DIP a partir del grosor (`Math.Max(6, grosor)`). Lógica pura, probada sin WPF.
- **Application:**
  - `HotkeyAction.ToggleToolbarCollapsed` (nuevo valor) y su entrada en `HotkeyCatalog`: `Always(HotkeyAction.ToggleToolbarCollapsed, HotkeyKey.H)`. El catálogo pasa de 22 a 23 definiciones.
  - `IToolbarLayout` en `Abstractions/` (nuevo puerto) y `ToolbarLayout` (implementación, instancia única):

    ```csharp
    public interface IToolbarLayout
    {
        bool IsCollapsed { get; }
        void ToggleCollapsed();
        event EventHandler? Changed;
    }
    ```

    Vive en Application para que el atajo y la barra compartan el estado sin conocerse (hoy el router de atajos no conoce el ViewModel de la barra).
- **Infrastructure:**
  - `HotkeyKeyMap`: `HotkeyKey.H` → `VK_H`, si falta.
- **Presentation:**
  - `Resources/Icons.xaml`: `Icon.Eye` e `Icon.EyeOff` con los trazados del `README.md` del design system.
  - `ToolbarViewModel`:
    - Recibe `IToolbarLayout`.
    - Expone `IsCollapsed`, `CollapseHint` (tooltip según el estado), `EyeGlyph` y `ToggleCollapsedCommand`.
    - Al plegar, cierra los popovers.
  - `ToolbarWindow.xaml`:
    - Botón del ojo tras el primer separador.
    - Todo lo que va después del ojo (Puntero, herramientas, tinta, grosor, acciones) se agrupa en un panel que se oculta con `IsCollapsed`.
    - La ventana ya usa `SizeToContent` y conserva su posición izquierda, así que el borde izquierdo no se mueve.
  - `HotkeyActionRouter`: `ToggleToolbarCollapsed` → `toolbarLayout.ToggleCollapsed()`, solo si `controller.IsVisible`.
  - `HotkeyActionLabels`: «Plegar barra».
  - `ToolCursorBehavior`:
    - Además de `Tool`, recibe por enlace la tinta (`SolidInkColor`), el degradado (`ActiveGradient`) y el grosor activo, que ya expone `OverlayViewModel`.
    - Para Lápiz, Línea, Flecha y Rectángulo pide el cursor a `InkCursorFactory`; las demás herramientas no cambian.
  - `InkCursorFactory` (nueva, en `Behaviors/`):
    - Dibuja el círculo (relleno sólido o degradado, más el aro de contraste) a la escala del monitor (`VisualTreeHelper.GetDpi`).
    - Lo empaqueta como `.cur` de 32 bits con alfa en memoria, con el punto activo en el centro, y crea un `Cursor` de WPF.
    - Cachea cada cursor por (tinta o par, grosor, escala) para no repetir el trabajo; como mucho 12 × 5 × número de escalas.
    - Sin P/Invoke: basta el constructor `Cursor(Stream)` de WPF.
  - `Tokens.xaml`: dos tokens de Chrome para el aro, `Cursor.Ring.Light` (`#CCFFFFFF`) y `Cursor.Ring.Dark` (`#99000000`), para no fijar colores en código.
  - `App.BuildServiceProvider`: registra `IToolbarLayout` → `ToolbarLayout` como singleton y lo pasa al router.

---

## Plan de implementación

1. **Backlog.** HU-25 y HU-26 en `BACKLOG.md`; ajuste de la nota de HU-24. Solo docs.
2. **Estado de la barra plegada.** `IToolbarLayout` y `ToolbarLayout`. Tests: arranca desplegada; `ToggleCollapsed` alterna y dispara `Changed`.
3. **Botón del ojo.** Iconos, propiedades del ViewModel, botón y panel plegable en `ToolbarWindow.xaml`.
   - Tests del ViewModel: tooltip e icono cambian con el estado; plegar cierra los popovers; plegar no cambia modo, herramienta, tinta ni grosor.
   - **Manual:** plegar y desplegar con el ojo; asa y ojo no se mueven; la barra plegada se arrastra; se sigue dibujando.
4. **Atajo `Ctrl+Alt+H`.** `HotkeyKey.H` y su tecla virtual, la acción, la entrada en el catálogo, la etiqueta y el enrutado.
   - Tests: la acción está en el catálogo en «Siempre»; un `settings.json` sin la acción toma `Ctrl+Alt+H`; si el usuario ya tenía `Ctrl+Alt+H` en otra acción, la suya gana y «Plegar barra» queda sin atajo; con la capa oculta el atajo no cambia el estado.
   - **Manual:** `Ctrl+Alt+H` en modo dibujo y en modo puntero; ocultar la capa con la barra plegada y volver a mostrarla: sigue plegada.
5. **Tamaño del círculo.** `CursorTipSize` en Domain. Tests: grosores 3 y 5 → 6; grosores 8, 12 y 18 → su mismo valor.
6. **Cursor de tinta sólida.** `InkCursorFactory` (relleno sólido y aro), tokens del aro y `ToolCursorBehavior` enlazado a tinta y grosor.
   - Tests: la fábrica devuelve el mismo cursor para la misma combinación; Texto, Láser y Borrador mantienen su cursor.
   - **Manual:** círculo rojo de grosor 8 del mismo ancho que el trazo; `Ctrl+2` o la rueda del mouse lo actualizan sin moverlo.
7. **Cursor en modo degradado.** Relleno con el par de `ActiveGradient` y refresco al cambiar de par.
   - **Manual:** con `Ctrl+7`, el color del círculo coincide con el inicio del trazo siguiente y cambia tras cada trazo.
8. **Multi-monitor.** Cada pantalla genera el círculo a su escala.
   - **Manual:** con monitores al 100 % y 150 %, círculo y trazo coinciden en los dos.
9. **Design system.** `README.md` del skill (ojo portado, atajo, cursor de tinta) y `ui_kits/toolbar/README.md`. Solo docs.

---

## Criterios de aceptación

> **Cierre (2026-10-30):** verificados con tests automáticos: estado plegado compartido entre el ojo y `Ctrl+Alt+H`, tooltip e icono según el estado, cierre de popovers al plegar, ancho de la barra plegada, atajo en el grupo «Siempre» y cesión del predeterminado si el usuario ya lo usa, tamaño del círculo, aros de contraste, archivo de cursor, par del degradado y cursor por herramienta. Build `-warnaserror` y tests verdes. En la app real el usuario probó todos los criterios, incluidos los dos monitores, y todo funcionó.

**HU-25 — Barra plegable**

- [x] Hay un botón con icono de ojo justo después del asa y su separador; su tooltip es «Ocultar barra» con `Ctrl+Alt+H` en monoespaciada.
- [x] Clic en el ojo: la barra queda en asa · separador · ojo tachado, con la misma superficie glass; el tooltip pasa a «Mostrar barra».
- [x] Clic en el ojo tachado: vuelve la barra completa.
- [x] Al plegar y desplegar, el asa y el ojo se quedan en el mismo sitio de la pantalla.
- [x] La barra plegada se arrastra por el asa, también a otro monitor.
- [x] Con la barra plegada se sigue dibujando con la misma herramienta, tinta y grosor, y los atajos (incluido `Esc`) funcionan.
- [x] Con un popover de tinta o grosor abierto, plegar lo cierra.
- [x] El ojo nunca lleva el acento teal, plegada o desplegada.
- [x] `Ctrl+Alt+H` pliega y despliega la barra en modo dibujo y en modo puntero.
- [x] Con la capa oculta, `Ctrl+Alt+H` no hace nada; al mostrar la capa, la barra está como estaba.
- [x] Ocultar la capa con la barra plegada y volver a mostrarla: sigue plegada.
- [x] Al reiniciar la app, la barra arranca desplegada.
- [x] «Plegar barra» aparece en Ajustes, grupo «Siempre», con `Ctrl+Alt+H`, y se puede reasignar.
- [x] Un `settings.json` existente sigue funcionando; si ya tenía `Ctrl+Alt+H` en otra acción, esa asignación se mantiene y «Plegar barra» queda sin atajo.

**HU-26 — Cursor de tinta**

- [x] Con Lápiz, Línea, Flecha o Rectángulo en modo dibujo, el cursor es un círculo relleno de la tinta activa con aro de contraste; no aparece la cruz.
- [x] Con grosor 8 el círculo y el trazo tienen el mismo ancho; con grosor 3 el círculo mide 6 DIP.
- [x] El trazo empieza en el centro del círculo.
- [x] Un círculo de tinta negra se ve sobre fondo negro y uno de tinta blanca sobre fondo blanco.
- [x] Al cambiar tinta o grosor con atajo, rueda del mouse o barra, el círculo cambia sin mover el mouse.
- [x] En modo degradado el círculo muestra el par del próximo trazo y cambia después de cada trazo.
- [x] El círculo se ve mientras se arrastra para dibujar.
- [x] Con monitores a escalas distintas, el círculo coincide con el ancho del trazo en cada uno.
- [x] Texto, Láser y Borrador mantienen su cursor actual; en modo puntero el cursor es el normal de Windows.

**General**

- [x] `dotnet build -warnaserror` y `dotnet test` verdes; `BACKLOG.md` incluye HU-25 y HU-26; el design system describe el ojo y el cursor de tinta.

---

## Decisiones tomadas

1. **Un spec con dos historias.** Las dos son pequeñas y se revisan juntas; cada una va en sus propios commits.
2. **El ojo sigue el design system al pie de la letra** (asa y ojo fijos, sin acento, cierra popovers, no toca el dibujo). Lo único nuevo es el atajo.
3. **Atajo `Ctrl+Alt+H` en «Siempre».** La barra estorba también en modo puntero. Se descartó `Ctrl+H` en «Solo al dibujar»: no funciona en puntero y en muchas apps abre «Reemplazar».
4. **Plegada durante la sesión, desplegada al reiniciar.** Si se plegó fue a propósito; no guardarlo evita «perder» la barra para siempre. Se descartó guardarlo en `settings.json`.
5. **Estado en un puerto de Application (`IToolbarLayout`).** El atajo y la barra lo comparten sin conocerse. Se descartó que el router llame al ViewModel de la barra: ya hay un ciclo de dependencias que se rompe a mano en `App`.
6. **Etiqueta «Plegar barra» en Ajustes.** Evita confundirla con «Mostrar/ocultar», que oculta todo Acetato. Los tooltips mantienen «Ocultar barra» / «Mostrar barra» del design system.
7. **Choque con una asignación previa de `Ctrl+Alt+H`: gana la del usuario.** «Plegar barra» queda sin atajo hasta reasignarlo; el botón del ojo sigue disponible.
8. **Círculo solo donde hoy hay cruz.** El Borrador con círculo hueco se descartó para no ampliar el alcance.
9. **Aro doble de contraste (claro fuera, oscuro dentro).** Sin él, el cursor desaparece con tinta del color del fondo. Se descartaron el relleno solo y un único aro.
10. **Mínimo de 6 DIP.** Con grosor 3 un punto de 3 px cuesta encontrarlo. Se descartó el tamaño exacto siempre.
11. **Degradado: el par del próximo trazo.** Muestra lo que va a salir y aprovecha que la app ya lo elige de antemano. Se descartaron los colores fijos del botón de degradado y un círculo blanco neutro.
12. **Cursor real de Windows, no un dibujo en la capa.** No se queda atrás al mover el mouse rápido. Se descartó pintarlo en una capa como el Láser, por ese retraso.
13. **Sin P/Invoke para el cursor.** `Cursor(Stream)` con un `.cur` en memoria basta; Infrastructure no se toca para esto.
14. **Definición rápida.** El usuario pidió el spec completo de una vez, sin confirmar sección por sección; las dudas surgidas al redactarlo se resolvieron con las propuestas por defecto.

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Otra app ya usa `Ctrl+Alt+H` y el registro falla. | Aviso de atajos no disponibles del SPEC 05 y reasignación en Ajustes. El botón del ojo sigue funcionando. |
| Windows limita o reescala el tamaño de los cursores (accesibilidad: «tamaño del puntero»). Con 18 DIP al 250 % el círculo mide unos 49 px físicos. | Se comprueba en el paso 6. Si Windows lo escala, se ajusta el tamaño pedido; si no es posible, se acepta que no coincida exacto en esos casos y se anota. |
| `Cursor(Stream)` escribe un archivo temporal en cada creación y causa tirones al girar la rueda rápido. | Caché por combinación: cada cursor se crea una sola vez por sesión. |
| El cursor sale sin transparencia en algún equipo y se ve un cuadrado alrededor. | `.cur` de 32 bits con alfa, soportado por Windows desde XP. Comprobación manual en el paso 6. |
| Fugas al activar y desactivar la capa muchas veces (DoD). | La caché tiene entradas acotadas (tintas o pares × grosores × escalas) y vive lo que dura la app. |

---

## Lo que **no** entra en este spec

- Recordar entre sesiones si la barra quedó plegada, o animar el pliegue.
- Cursor en círculo para el Borrador, o cambios en los cursores de Texto y Láser.
- Previsualizar la forma antes de hacer clic.
- Cambios en Cerrar, la bandeja, la herramienta Seleccionar o la maqueta interactiva del design system.
