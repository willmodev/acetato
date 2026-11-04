# Acetato — Qué falta para lanzar y cómo cobrar

> Investigación del **2026-11-02**. Precios, comisiones y políticas cambian: los datos llevan su
> fuente al final. Marcas usadas: **[NV]** = no verificado en fuente oficial · **[Supuesto]** =
> inferencia o recomendación nuestra, no un hecho.
>
> Nota: en la petición aparece "7 semanas" y "7 días"; este documento asume **7 días** de prueba.

---

## 1. Veredicto corto

- **Las herramientas de dibujo ya están maduras.** Acetato trae gratis casi todo lo que Epic Pen
  (el rival directo en Windows) cobra en su plan Pro: formas, flecha, texto, láser, mover y
  redimensionar, multi-monitor con DPI mixto, atajos configurables.
- **Lo que NO está listo es "el producto alrededor"**: no hay prueba gratuita ni cobro, solo habla
  español, no hay instalador ni actualizaciones, no hay manejo de errores, y el runtime **.NET 9
  pierde soporte el 10-11-2026** (en 8 días).
- **Faltan 3 funciones que casi todos los competidores tienen**: resaltador semitransparente,
  pizarra blanca/negra y soporte de lápiz con presión / pantalla táctil (por verificar).
- **Ruta recomendada [Supuesto]:** publicar primero en **Microsoft Store** (registro gratis, firma
  gratis, sin avisos de "Windows protegió su PC", Microsoft cobra y paga a Colombia), con una
  **web sencilla** para lo legal y la promoción. Abrir venta directa por la web solo si hay tracción.

Orden sugerido: **(1) funcionalidad y robustez → (2) licencia/prueba → (3) empaquetado MSIX →
(4) web y legal → (5) cuenta de Partner Center y ficha → (6) lanzamiento.** El "ejecutable" y el
"hash" van al final y, en la Store, casi desaparecen como problema (ver §6.3).

---

## 2. Qué tiene Acetato frente a la competencia

### 2.1 Competidores

| Producto | Plataforma | Cobro | Precio (nov-2026) | Prueba | Canal |
|---|---|---|---|---|---|
| **Epic Pen** | Windows | Freemium + suscripción Pro | **US$3/mes o US$24/año** (leído de su API de precios; hay un pago único de US$80 no anunciado) | Pro 14 días (si pide tarjeta: [NV], probable) | Web propia, cobra con Paddle |
| ScreenBrush | macOS | Gratis + pago único "Plus" | US$6,99 (EE. UU.) | 7 días dentro de la app | Mac App Store |
| Presentify | macOS | Pago único o Setapp | US$14,99 | No en App Store; 7 días en Setapp | App Store, Setapp, web |
| ZoomIt (Microsoft) | Windows | **Gratis** | — | — | Sysinternals, PowerToys |
| ppInk / gInk | Windows | **Gratis** (código abierto) | — | — | GitHub |
| DemoHelper | Windows | **Gratis** (código abierto) | — | — | Web, GitHub |

Lectura: en Windows el único rival de pago serio es Epic Pen, y hay tres alternativas gratis.
**El precio de referencia del mercado es US$2–3/mes.** Ojo también: Microsoft está probando
"Live Annotation" en Recortes (dibujar antes de capturar) — amenaza sin fecha confirmada [NV].

### 2.2 Lo que ya tenemos y Epic Pen cobra
Formas (línea, flecha, rectángulo, elipse), texto, tinta que se desvanece (vía láser), más
colores (6 + degradados), y extras que Epic Pen no anuncia: mover/redimensionar anotaciones,
multi-monitor con DPI mixto, atajos configurables, recordar la sesión.

---

## 3. Pendientes de funcionalidad

### 3.1 Bloqueantes para vender (no son "dibujo", pero sin ellos no hay producto)

| # | Pendiente | Por qué es necesario | Esfuerzo [Supuesto] |
|---|---|---|---|
| B1 | **Migrar a .NET 10 (LTS)** | .NET 9 queda sin parches de seguridad el **10-11-2026**. .NET 10 tiene soporte hasta nov-2028. Vender sobre un runtime sin soporte es un riesgo y mala señal en la certificación. | 0,5–1 día |
| B2 | **Prueba de 7 días + suscripción (licencia)** | Es el modelo de negocio. Ver §5 y §6. | 4–6 días |
| B3 | **Idioma inglés** | Todos los competidores lo tienen; sin inglés el mercado se reduce a hispanohablantes. Hoy los textos están escritos dentro de las pantallas; hay que sacarlos a archivos de idioma y elegir idioma según Windows. | 3–5 días |
| B4 | **Manejo global de errores + registro (log)** | Hoy un error inesperado cierra la app sin dejar rastro. Con clientes que pagan hay que poder diagnosticar ("envíame el archivo de log"). | 1–2 días |
| B5 | **Una sola instancia** | Si se abre dos veces, la segunda pelea por los atajos (ya vimos ese problema con los procesos "zombi" en desarrollo). | 0,5 día |
| B6 | **Versión visible + "Acerca de"** | Soporte y actualizaciones necesitan saber qué versión tiene el usuario. Hoy el proyecto no declara versión. | 0,5 día |
| B7 | **Iniciar con Windows (opcional, apagado por defecto)** | Una app de bandeja que no arranca sola se olvida. La Store pide consentimiento para cambiar el arranque. | 1 día |
| B8 | **Primera ejecución** | Al abrir por primera vez, el usuario no sabe que todo funciona con atajos. Una tarjeta breve con los 3 atajos clave y el ícono de bandeja reduce desinstalaciones. | 1–2 días |
| B9 | **Pasada de verificación manual completa** | Varias HU quedaron con "verificación manual pendiente" en las notas. Probar todo en Windows 11 y **Windows 10** (el README dice compatible) y en un equipo limpio. | 2–3 días |

### 3.2 Funciones "casi obligatorias" (las tienen 4 o más competidores)

| # | Función | Dónde aparece | Esfuerzo [Supuesto] |
|---|---|---|---|
| F1 | **Resaltador semitransparente** | 6 de 8 (Epic Pen lo da **gratis**) | 1–2 días |
| F2 | **Pizarra blanca / negra** (fondo opaco para explicar sin la pantalla detrás) | 4 de 8 (Epic Pen la cobra) | 1–2 días |
| F3 | **Lápiz con presión y pantallas táctiles** | 5 y 4 de 8 | 1–3 días. **Verificar primero**: el trazo propio (estilo ScreenBrush) podría estar ignorando la presión del lápiz. Importante para docentes con tableta o Surface. |

> Lupa/zoom también aparece en 4 de 8, pero es la función central de **ZoomIt, gratis y de
> Microsoft**. [Supuesto] No vale la pena competir ahí en la v1.

### 3.3 Deseables (v1.1, después del lanzamiento)
- Resaltar el cursor o los clics (Epic Pen Pro, Presentify, DemoHelper).
- Trazos que se desvanecen solos (además del láser) — Epic Pen Pro, Presentify, ppInk.
- Foco / spotlight (oscurecer todo menos una zona).
- Rellenos de formas; más colores o selector propio.
- Mantener una tecla para usar la app de abajo sin salir del modo dibujo.

### 3.4 Diferenciales (más adelante, si hay demanda)
Mostrar teclas pulsadas, sellos numerados (1, 2, 3…), desenfocar zonas (privacidad), licencias por
volumen para colegios/empresas (Epic Pen lo vende como plan de equipos), control desde el móvil.

**Mínimo recomendado para lanzar [Supuesto]:** B1–B9 + F1 + F2 + verificar F3.
Esfuerzo total aproximado: **20–32 días-persona** (sin web, ficha ni trámites, ver §8).

---

## 4. Modelo de cobro

### 4.1 Prueba ¿con tarjeta o sin tarjeta?

Datos de First Page Sage (86 empresas SaaS, 2022–2025):

| Tipo de prueba | Visitantes que empiezan la prueba | Pruebas que terminan pagando |
|---|---|---|
| Sin tarjeta | 8,5 % | 18,2 % |
| Con tarjeta | 2,5 % | 48,8 % |

En su informe de 2026 **ambos modelos terminan en ~13 clientes de pago por cada 1.000
visitantes**. Es decir: con tarjeta entran menos pero pagan más; sin tarjeta entran más pero
pagan menos. **El resultado final es parecido.** (Son datos de SaaS mayormente B2B, no de
utilidades de escritorio; úsalos como orientación.)

Qué decide en la práctica:
- **En Microsoft Store la suscripción con prueba EXIGE tarjeta** (método de pago en la cuenta
  Microsoft). No hay opción sin tarjeta. Al terminar la prueba pasa sola a cobro.
- Sin tarjeta solo es posible **con cobro propio** (prueba gestionada por la app + servidor de
  licencias).

### 4.2 ¿Bloquear al terminar la prueba o dejar una versión gratis?
Hay tres alternativas gratis en Windows y Epic Pen tiene versión gratis. Si Acetato se bloquea por
completo al día 8, el usuario se va a ZoomIt o a Epic Pen gratis y deja una mala reseña.

**Recomendación [Supuesto]: modelo freemium como Epic Pen.**
- **Gratis para siempre:** lápiz, 3–4 tintas, borrador, limpiar, deshacer, un monitor.
- **Pro (suscripción):** todo lo demás (formas, texto, láser, mover/redimensionar, degradados,
  multi-monitor, pizarra, atajos configurables, recordar sesión).
- **Botón "Probar Pro 7 días"** dentro de la app. En la Store ese botón abre el cobro de Microsoft
  con prueba (pide tarjeta, no cobra durante 7 días).

Ventajas: la gente instala sin miedo, la app genera reseñas y boca a boca, y la prueba con tarjeta
la activa quien ya quiere más. Desventaja: hay que decidir qué va en cada plan (decisión de
producto, no técnica).

### 4.3 Precio
- Referencia: Epic Pen US$3/mes o US$24/año.
- **Las comisiones fijas castigan los precios bajos.** Ejemplo por cobro (sin impuestos ni
  retenciones):

| Precio | Microsoft Store (15 %) | Paddle (5 % + US$0,50) |
|---|---|---|
| US$3/mes | te quedan US$2,55 | te quedan US$2,35 (comisión 22 %) |
| US$24/año | te quedan US$20,40 | te quedan US$22,30 (comisión 7 %) |

- **Recomendación [Supuesto]:** mensual **US$3,99** y anual **US$29** (≈ US$2,42/mes), mostrando
  el anual como la opción destacada. Por debajo de Epic Pen no aporta; el diferencial es calidad,
  no precio.
- **Cuidado en la Store:** el precio de una suscripción **no se puede subir** después de
  publicada (solo bajar), y no admite códigos promocionales ni rebajas. Elegirlo bien desde el
  principio.
- Pago único: no hay datos sólidos; algunos desarrolladores independientes reportan que el público
  de utilidades rechaza la suscripción pura. Acetato no tiene costos de servidor por usuario, así
  que un "vitalicio" caro (p. ej. US$79) es viable como tercera opción **solo en venta directa**
  (la suscripción de la Store no lo combina fácilmente). Validar con usuarios.

---

## 5. ¿Tienda de aplicaciones o web propia?

### 5.1 Microsoft Store (lo verificado)

| Tema | Dato |
|---|---|
| Registro | **Gratis** para individuos y empresas (el cobro de US$99 a empresas se eliminó el 07-05-2026). |
| Tipo de cuenta | La política dice que si distribuyes "en relación con tu negocio u oficio" la cuenta debe ser **de Empresa**. [Supuesto] Cobrar suscripción encaja ahí. **Una cuenta individual no se convierte luego en empresa.** |
| Comisión con cobro de Microsoft | **15 %** para apps (12 % juegos). Recargos extra con tarjetas regalo o factura de operador. |
| Comisión con cobro propio | **0 %** para apps que no son juegos (cumpliendo la política 10.8.2). |
| Firma del instalador | **Gratis** si se empaqueta como MSIX (Microsoft lo firma). |
| Avisos de seguridad de Windows | Las apps MSIX de la Store **no** muestran el aviso de SmartScreen. |
| Actualizaciones | Automáticas para MSIX. |
| Prueba en suscripción | 1 semana o 1 mes; **exige tarjeta**; una prueba por cliente. |
| Pagos a Colombia | **Sí** (banco o PayPal), umbral US$50, mensual. |
| Impuestos | Con cobro de Microsoft, Microsoft recauda impuestos al consumidor en los países de su anexo. Hay que llenar formulario fiscal de EE. UU. (W-8BEN / W-8BEN-E). Colombia **no tiene tratado** de renta con EE. UU.: [Supuesto] podría retenerse **30 % de lo vendido a EE. UU.** — confirmar con contador. |
| Requisitos de ficha | URL de **política de privacidad** obligatoria, mínimo 1 captura (se recomiendan 4), descripción, divulgar compras dentro de la app y términos de la prueba. |
| Limitaciones | Precio de suscripción no se puede subir; sin cupones; para probar el cobro hay que publicar la app (puede ser oculta). |

### 5.2 Venta directa por web propia (lo verificado)

| Tema | Dato |
|---|---|
| **Stripe** | **No acepta cuentas de Colombia.** Stripe Atlas (empresa en EE. UU., US$500 + US$100/año + costos de mantener la empresa) no compensa para empezar. |
| **Paddle** (vendedor oficial: cobra, liquida impuestos de cada país y te paga) | Acepta Colombia. 5 % + US$0,50 por cobro. Prueba con tarjeta: sí; sin tarjeta: en acceso anticipado. **No genera licencias**: hay que montar Keygen o un servidor propio. Paga por banco o Payoneer, mínimo US$100. Epic Pen lo usa. |
| **Polar.sh** | Acepta Colombia. 5 % + US$0,50 (+1,5 % tarjeta no estadounidense). **Genera licencias** incluidas. Empresa más joven. |
| Lemon Squeezy | Acepta Colombia, pero su futuro es incierto tras la compra por Stripe. No recomendado. |
| Pasarelas locales (Wompi, PayU, Mercado Pago) | Cobran, pero **no liquidan IVA de otros países**: tendrías que hacerlo tú. No recomendado para venta global. |
| Firma del instalador | **Obligatoria en la práctica.** El servicio barato de Microsoft (Artifact Signing, US$9,99/mes) **no está disponible en Colombia**. Queda un certificado OV (~US$220–400/año según revendedor [NV]) en token o en la nube. Incluso firmado, Windows muestra el aviso hasta acumular reputación ("semanas y cientos de instalaciones"). El certificado EV **ya no** evita ese aviso. |
| Instalador y actualizaciones | **Velopack** (gratis, código abierto): instalador + actualización automática. |
| Web | Paddle exige públicamente: precios, términos, **política de reembolso de 14 a 90 días** ("sin devoluciones" no se acepta) y privacidad. |

### 5.3 Comparación

| Criterio | Store + cobro Microsoft | Store (MSIX) + cobro propio | Solo web propia |
|---|---|---|---|
| Tiempo hasta el primer cobro | **El más corto** | Medio | El más largo |
| Comisión | 15 % | ~5 % + US$0,50 | ~5 % + US$0,50 |
| Servidor de licencias | **No hace falta** | Hace falta | Hace falta |
| Certificado de firma | **No** | No | **Sí** (US$220–400/año) |
| Aviso "Windows protegió su PC" | **No** | No | Sí, al principio |
| Impuestos de otros países | Microsoft (países del anexo) | Paddle/Polar | Paddle/Polar |
| Prueba sin tarjeta | No | Sí | Sí |
| Control de precio, cupones, vitalicio | Limitado | Total | Total |
| Visibilidad | Búsqueda de la Store | Búsqueda de la Store | Solo marketing propio |

### 5.4 Recomendación [Supuesto]

**Fase 1 — Store con cobro de Microsoft.** Es lo más rápido y barato para validar si la gente
paga: sin certificado, sin servidor, sin liquidar impuestos de cada país, y Microsoft paga a
Colombia. La web existe igual (es obligatoria para la política de privacidad) pero solo enlaza a
la ficha de la Store.

**Fase 2 — si hay tracción**, abrir venta directa (Paddle o Polar + Velopack + certificado OV) para
ofrecer prueba sin tarjeta, plan vitalicio, cupones, licencias por volumen para colegios, y quedarse
con más margen en el plan anual. Disparadores para pasar a fase 2: que la retención del 30 % sobre
ventas a EE. UU. se confirme y pese mucho, o que lleguen pedidos de empresas/colegios.

**Clave técnica para que la fase 2 no sea reescribir:** la licencia se diseña como un **puerto en
Application** (p. ej. "¿este usuario tiene Pro? ¿cuántos días de prueba le quedan?") con un
**adaptador de la Store en Infrastructure**. Luego se añade un adaptador de Paddle/Polar sin tocar
el resto de la app. Encaja con la arquitectura actual del proyecto.

### 5.5 Qué NO recomendamos
- Stripe directo (no opera con Colombia) ni Stripe Atlas para empezar.
- Pagar un certificado EV "para evitar SmartScreen" (ya no lo evita).
- Lemon Squeezy (futuro incierto) y pasarelas locales para venta global.
- Distribuir un `.exe` sin firmar: Windows 11 con Control inteligente de aplicaciones lo bloquea.

---

## 6. Lo técnico de la publicación (resumen)

### 6.1 Empaquetado MSIX
- Un WPF empaquetado necesita la capacidad **runFullTrust**, que se justifica en Partner Center
  (puede alargar la primera certificación).
- El cobro de la Store (`StoreContext`) **solo funciona con la app empaquetada**; desde WPF hay que
  asociarlo a la ventana (`InitializeWithWindow`). No funciona si la app corre como administrador.
- Revisar en MSIX: notificaciones (con MSIX la app ya tiene identidad propia, lo que simplifica lo
  hecho en HU-12), ruta de `settings.json`, arranque con Windows (`StartupTask`), desinstalación
  limpia (política 10.2.7) y que todo funcione con las notificaciones apagadas (política 10.9).
- No encontramos ninguna política que prohíba capas superpuestas, atajos globales ni excluir la
  barra de capturas. Aplica la regla general: hacer lo que la ficha dice y pedir consentimiento
  antes de cambiar el comportamiento de Windows.

### 6.2 Licencia en la app
- Fase 1: consultar a la Store al arrancar si hay suscripción activa o prueba en curso; guardar el
  último resultado para funcionar **sin internet** unos días.
- Fase 2: token de licencia firmado (el servidor lo firma, la app lo comprueba sin internet) con
  vencimiento = fin del periodo pagado + 3–7 días de gracia. Para que reinstalar no reinicie la
  prueba sin tarjeta, registrar una huella del equipo; esto es dato personal y va declarado en la
  política de privacidad.

### 6.3 El "ejecutable" y el "hash"
- **En la Store no hay que preocuparse por ninguno de los dos:** se sube un paquete MSIX, Microsoft
  lo firma y lo entrega.
- **En venta directa**, lo que da confianza es la **firma de código** (identifica al editor y
  Windows la usa para decidir si avisa). El **hash SHA-256** publicado junto al botón de descarga
  solo sirve para comprobar que el archivo no se corrompió; Windows no lo usa y, si alguien
  compromete la web, cambia el instalador y el hash a la vez. Es un complemento barato, no un
  sustituto de la firma.

---

## 7. La web mínima

Necesaria incluso en la fase 1 (la Store exige la URL de privacidad):

1. **Inicio**: qué hace, video o GIF de 20 segundos, botón "Obtener en Microsoft Store".
2. **Precios**: gratis vs Pro, mensual/anual, términos de la prueba.
3. **Política de privacidad** (qué guarda la app: ajustes locales; en fase 2, licencia y huella del equipo).
4. **Términos de uso / licencia (EULA)** y **política de reembolso**.
5. **Soporte**: correo, preguntas frecuentes, lista de atajos, registro de cambios.
6. Fase 2: botón de descarga directa (con SHA-256 y requisitos), portal de cliente para cancelar o
   cambiar la tarjeta (lo dan Paddle y Polar), página "si Windows muestra un aviso al instalar".

Idiomas: español e inglés. Dominio propio y correo del dominio (también lo pide la Store para la
cuenta de Empresa).

---

## 8. Plan por etapas (orden recomendado)

| Etapa | Contenido | Esfuerzo [Supuesto] |
|---|---|---|
| 0. Decisiones | Persona natural o empresa (¿proyecto personal o de Sinapsis?), qué va en Gratis vs Pro, precio, verificar que el nombre "Acetato" esté libre en la Store y como dominio | 1–2 días |
| 1. Producto | B1–B9 + F1 + F2 + verificar F3 (§3) | 20–32 días-persona |
| 2. Licencia | Puerto de licencia + adaptador Store + pantallas "Probar Pro" / "Pro activo" + restricciones del plan gratis | incluido en B2 |
| 3. Empaquetado | Proyecto MSIX, runFullTrust, arranque con Windows, prueba de instalar/desinstalar en equipo limpio | 2–4 días |
| 4. Web y legal | Landing es/en, privacidad, términos, reembolsos, soporte, dominio y correo | 3–5 días |
| 5. Cuenta y ficha | Partner Center (verificación 2–5 días hábiles si es empresa sin D-U-N-S), formulario fiscal, banco, ficha es/en con 4+ capturas, publicación **oculta** para probar el cobro real, certificación | 1–2 semanas de calendario |
| 6. Lanzamiento | Publicación, recoger reseñas y métricas (instalaciones, pruebas iniciadas, conversión) | — |

**Total estimado: 6–9 semanas** de una persona a tiempo parcial-completo, desde hoy hasta tener
la app cobrando en la Store. Lo que más puede mover la cifra: el inglés (si aparecen textos en
más sitios de lo previsto), problemas con lápiz/táctil, y los tiempos de verificación de Microsoft.

---

## 9. Colombia: impuestos y trámites (panorama — **confirmar con un contador**)

- **RUT y facturación electrónica** según la figura (persona natural o empresa).
- **Exportación de servicios**: puede estar exenta de IVA si el servicio se usa exclusivamente en el
  exterior (Estatuto Tributario art. 481 lit. c). [NV] Si vender a Microsoft o a Paddle (que revenden
  al usuario final) cuenta como exportación de servicios o como licencia de software depende del
  contrato: debe revisarlo un tributarista.
- **Ventas a usuarios en Colombia**: los servicios digitales causan IVA del 19 %; [NV] confirmar
  cómo lo maneja la Store o el vendedor oficial elegido.
- **Renta**: los ingresos del exterior se declaran en la cédula general (persona natural).
- **Retención de EE. UU.**: ver §5.1 (posible 30 % sobre ventas a EE. UU. vía Store).

---

## 10. Decisiones que necesitamos de ti

1. **¿Persona natural o empresa?** Define el tipo de cuenta en la Store (no se puede cambiar
   después) y la parte fiscal.
2. **¿Freemium (gratis limitado + Pro) o solo prueba y bloqueo?** Recomendamos freemium (§4.2).
3. **Precio**: ¿US$3,99/mes y US$29/año? En la Store no se puede subir después.
4. **¿Arrancamos por la Store (fase 1) o directo por web propia?** Recomendamos Store.

---

## Fuentes

**Competencia**
- Epic Pen: https://epicpen.com/ · https://epicpen.com/pro · https://epicpen.com/faq · https://epicpen.com/features · API de precios `epicpen.com/api/pricing?countryCode=US`
- ScreenBrush: https://apps.apple.com/us/app/screenbrush/id1233965871?mt=12
- Presentify: https://presentifyapp.com/ · https://setapp.com/apps/presentify
- ZoomIt: https://learn.microsoft.com/en-us/sysinternals/downloads/zoomit
- ppInk: https://github.com/pubpub-zz/ppInk · gInk: https://github.com/geovens/gInk · DemoHelper: https://tools.stefankueng.com/DemoHelper.html
- Live Annotation en Recortes [NV]: https://winaero.com/snipping-tool-is-getting-live-annotation-that-lets-you-draw-on-screen-in-real-time

**.NET**
- Fin de soporte de .NET 8 y 9 (10-11-2026): https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/

**Microsoft Store**
- Cuenta de desarrollador: https://learn.microsoft.com/en-us/windows/apps/publish/partner-center/open-a-developer-account
- Registro gratis para empresas (07-05-2026): https://blogs.windows.com/windowsdeveloper/2026/05/07/publish-to-microsoft-store-as-a-company-now-with-free-registration-and-faster-onboarding/
- Registro gratis para individuos: https://blogs.windows.com/windowsdeveloper/2025/09/10/free-developer-registration-for-individual-developers-on-microsoft-store/
- Acuerdo de desarrollador (ADA): https://go.microsoft.com/fwlink/?linkid=528905
- Políticas de la Store: https://learn.microsoft.com/en-us/windows/apps/publish/store-policies
- Publicar apps Win32: https://learn.microsoft.com/en-us/windows/apps/distribute-through-store/how-to-distribute-your-win32-app-through-microsoft-store
- Suscripciones: https://learn.microsoft.com/en-us/windows/uwp/monetize/enable-subscription-add-ons-for-your-app
- Compras y pruebas en la app: https://learn.microsoft.com/en-us/windows/uwp/monetize/in-app-purchases-and-trials
- Pagos por país: https://learn.microsoft.com/en-us/partner-center/marketplace-offers/payment-thresholds-methods-timeframes
- Cuenta de pagos y formularios fiscales: https://learn.microsoft.com/en-us/partner-center/account-settings/set-up-your-payout-account
- Tratados de EE. UU.: https://www.irs.gov/businesses/international-businesses/united-states-income-tax-treaties-a-to-z
- Capacidades (runFullTrust): https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/app-capability-declarations
- StartupTask: https://learn.microsoft.com/en-us/uwp/api/windows.applicationmodel.startuptask

**Venta directa, licencias y firma**
- Paddle países: https://www.paddle.com/help/start/account-verification/which-countries-are-supported-by-paddle · precios: https://www.paddle.com/pricing · pruebas sin tarjeta: https://developer.paddle.com/build/trials/cardless-trials · pagos: https://www.paddle.com/help/manage/get-paid/when-and-how-do-i-get-paid · requisitos de web: https://developer.paddle.com/partners/embed-billing/seller-go-live/minimum-seller-terms
- Polar: https://polar.sh/docs/merchant-of-record/supported-countries · https://polar.sh/resources/pricing · https://polar.sh/docs/features/benefits/license-keys
- Lemon Squeezy: https://docs.lemonsqueezy.com/help/getting-started/fees
- Stripe países: https://stripe.com/global · Stripe Atlas: https://stripe.com/atlas
- Keygen: https://keygen.sh/pricing/
- Artifact Signing (países): https://learn.microsoft.com/en-us/azure/artifact-signing/quickstart
- SmartScreen y EV: https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation
- Opciones de firma: https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options
- Velopack: https://github.com/velopack/velopack
- Conversión de pruebas: https://firstpagesage.com/seo-blog/saas-free-trial-conversion-rate-benchmarks/ · https://firstpagesage.com/seo-blog/conversion-rate-benchmarks/

**Colombia** [panorama, no asesoría]
- Oficio DIAN 20609/2018: https://normograma.dian.gov.co/dian/compilacion/docs/oficio_dian_20609_2018.htm
