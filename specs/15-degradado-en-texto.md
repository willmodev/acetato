# SPEC 15 — Degradado en el texto (HU-36)

> **Estado:** Aprobado · **Depende de:** SPEC 03 (HU-19) — modo Degradado, SPEC 10 (HU-27) — Tintas degradadas, SPEC 01 (HU-13) — herramienta Texto · **Fecha:** 2026-11-13
> **Objetivo:** Que el texto tome el par del degradado, como los trazos, cuando el modo Degradado o las Tintas degradadas están activos.

---

## Por qué existe este spec

- Desde el SPEC 03 el texto usaba siempre la última tinta sólida, aunque el botón Color mostrara un degradado. El SPEC 03 lo dejó fuera de alcance («el texto con degradado requiere otro tipo de pintado»), no por ser inviable: el texto es un bloque de texto de WPF y su color puede ser un degradado.
- Se pidió durante la prueba de la pizarra (SPEC 14), donde la diferencia se notaba.
- Se hizo como spec aparte, en su propia rama, para no mezclarlo con el resaltador y la pizarra.

---

## Alcance

**Dentro:**

- Con el modo **Degradado** (par neón al azar) o con **Tintas degradadas** (tinta → su vecina), el texto se pinta con el par del próximo trazo.
- **Dirección:** en diagonal sobre el recuadro del texto, de arriba a la izquierda a abajo a la derecha, como el rectángulo (`DiagonalGradientBrush`). Se adapta solo cuando el texto crece al escribir o cambia de tamaño.
- **El par se fija al empezar a escribir** y el texto lo conserva para siempre, aunque luego cambie el modo. Mientras se escribe, el editor ya muestra el degradado.
- **Al fijar un texto con degradado** se sortea el par siguiente, igual que con un trazo. Un texto vacío se descarta y no consume el par.
- Sin degradado, el texto sigue en tinta sólida como hasta ahora.
- Mover, Redimensionar, Deshacer y la pizarra funcionan igual: el pincel es relativo al recuadro.

**Fuera de alcance (para otros specs):**

- Degradado en el láser (su estela se redibuja y se apaga en cada fotograma).
- Degradado a lo largo de las letras o por letra.
- Elegir el par del texto aparte del de los trazos.

---

## Modelo de datos

Este spec no introduce datos nuevos. Reutiliza `GradientPair` y el par que ya entrega `InkGradient.Pair`. Cambios en Presentation:

- `DiagonalGradientBrush` (nuevo, `Resources/`): pincel congelado en diagonal para un par. Lo usan también el cursor de tinta (`InkCursorFactory.FillOf`) y el texto.
- `TextAnnotationBehavior`: el editor toma el par de `InkGradient.GetPair(canvas)`; al fijar un texto con degradado avisa con `InkGradient.NotifyUsed`.

---

## Plan de implementación

1. **Design system.** Texto con degradado en el artefacto «Acetato» y luego en la skill local.
2. **Pincel y texto.** `DiagonalGradientBrush`, el editor y la etiqueta con el par, y el aviso al fijar. Tests: el pincel, el editor y el texto con par, con Tintas degradadas, par consumido una sola vez, texto vacío sin consumir y texto sin degradado en tinta sólida.
3. **Prueba manual** (con permiso del usuario).

---

## Criterios de aceptación

- [x] Con el modo Degradado, el texto fijado se pinta con un par neón.
- [x] Con Tintas degradadas, el texto toma el par de su tinta.
- [x] Mientras se escribe, el editor ya muestra el degradado.
- [x] Al fijar un texto con degradado se sortea el par siguiente; un texto vacío no lo consume.
- [x] Sin degradado, el texto sigue en tinta sólida.
- [ ] Un texto con degradado conserva sus colores al moverlo, cambiar su tamaño y deshacer.
- [ ] En la pizarra blanca y en la negra, el texto con degradado se ve y respeta el par.
- [x] `dotnet build -warnaserror` y `dotnet test` en verde.
- [x] Las dos copias del design system describen el texto con degradado y se actualizaron antes del código.

---

## Decisiones tomadas

1. **Solo el texto, no el láser** (decisión del usuario: «degradado de texto»). El láser es efímero y habría que recalcular el degradado en cada fotograma.
2. **Con las dos fuentes de degradado.** Si el trazo de lápiz toma un par con Degradado o con Tintas degradadas, el texto también: una sola regla para todo lo que deja marca.
3. **Diagonal sobre el recuadro**, como el rectángulo. Un eje «del primer al último punto» no existe en un texto. El pincel relativo al recuadro se adapta solo al crecer o redimensionar.
4. **El par se fija al empezar a escribir.** Así el editor y el texto fijado se ven igual. Se descartó sortearlo al fijar: el color cambiaría de repente.
5. **Spec aparte y rama propia (`spec-15-degradado-en-texto`)**, pedido a mitad del SPEC 14. Se parte de la última confirmación de esa rama, así que el PR del SPEC 14 debe unirse primero.

---

## Lo que **no** está en este spec

- Degradado en el láser.
- Degradado por letra o a lo largo del texto.
- Elegir el par del texto aparte.

Cada uno, si llega, va en su propio spec.
