# Changelog — TOLGRIM: Mechanitor Expanded

Todos los cambios notables, hitos y fases implementadas de este proyecto se documentan en este archivo.

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/) y siguiendo [Semantic Versioning](https://semver.org/).

---

## [0.2.0] — 2026-11-12

### Fase 2: Neurofibra, Sueros Bandnode y Sistema de Bonificaciones

#### Añadido
- **Sistema Genérico de Bonificaciones de Ancho de Banda (`BandwidthBonusRegistry`):**
  - Infraestructura modular y extensible basada en proveedores (`IBandwidthBonusProvider`).
  - Soporte para fuentes de bonificación acumulativas y no acumulativas.
  - Integración nativa mediante `StatPart_MechanitorExpandedBandwidth` en `StatDefOf.MechBandwidth` para que todos los sistemas del juego (Gizmos, InfoCard, StatWorker y la pestaña de inspección) lean el valor consolidado sin duplicar cálculos.
- **Componente de Persistencia por Mecanizador (`CompMechanitorBandwidth`):**
  - Inyectado mediante parche condicional en `Human` para almacenar de forma segura las dosis consumidas de sueros (`Scribe_Values`).
- **Implantes Mech Neurofibre (Niveles I a IV):**
  - Cuatro implantes progresivos de ancho de banda:
    - **Mech Neurofibre I:** +3 Bandwidth.
    - **Mech Neurofibre II:** +6 Bandwidth.
    - **Mech Neurofibre III:** +9 Bandwidth.
    - **Mech Neurofibre IV:** +12 Bandwidth.
  - Comportamiento quirúrgico mutuamente excluyente (`Recipe_InstallNeurofibre`): instalar un nivel superior remueve de forma automática y limpia el nivel anterior.
  - Recetas de fabricación escalonadas entre `StandardMechtech` y `HighMechtech` utilizando Mechlinks sobrantes, componentes y subnúcleos.
  - Reutilización gráfica nativa del sprite original de `Mechlink` (`Things/Pawn/Humanlike/Apparel/Mechlink`) sin duplicar archivos PNG.
- **Sueros Neuronales Bandnode (`Tolgrim_BandnodeSerum`):**
  - Consumible exótico de rareza ultra-tech (`tradeTags: ExoticMisc`, recompensa de misiones de baja frecuencia).
  - Otorga +1 de ancho de banda permanente por dosis, con un límite configurable (por defecto 36 dosis).
  - Bloqueo preventivo de administración si el colono no es mecanizador o si ya alcanzó el tope permitido.
  - Reutilización gráfica nativa del sprite de `MechSerumHealer` (`Things/Item/Special/MechSerumHealer`).
- **Desglose en la Pestaña «Mecanizador» y Tooltips:**
  - Nuevas filas reactivas que desglosan la bonificación de Neurofibra, Sueros Bandnode (`dosis / máximo`) y bonificaciones de otras fuentes.
  - Tooltip nativo completo en la estadística de Ancho de Banda mediante `StatWorker.GetExplanationFull`, detallando el desglose real (base, equipo, implantes, sueros y mods externos).
- **Ampliación de Mod Settings:**
  - Opciones para habilitar/deshabilitar de forma segura Neurofibra y Sueros Bandnode (sin corromper partidas guardadas).
  - Configuración del límite máximo de dosis de suero por mecanizador (1 a 100).
  - Sliders para multiplicadores de bonificación de Neurofibra y Sueros (0.5x a 3.0x).
- **Localización Completa:**
  - Claves agregadas en Inglés (`English`), Español (`Spanish`) y Español Latino (`SpanishLatin`).

---

## [0.1.0] — 2026-11-12

### Fase 1: Implementación de la Pestaña «Mecanizador»

#### Añadido
- **Pestaña Nativa en el Panel de Inspección (`ITab_Pawn_Mechanitor`):**
  - Integrada al mismo nivel que las pestañas nativas del juego (`Personaje`, `Necesidades`, `Salud`, `Equipo`).
  - Condición de visibilidad estricta: visible únicamente en colonos con `Mechlink` activo y reconocido como mecanizador. Oculta para colonos normales.
  - Acordeones colapsables interactivos con iconos nativos (`Collapse` / `Reveal`).
  - Scroll vertical fluido con márgenes dedicados para evitar solapamientos con el botón de cierre `[X]`.
- **Contenido del Panel:**
  - **Información General:** Estado del mecanizador (`Activo` / `Incapacitado`), Ancho de banda total/usado/libre, Radio de comando en combate (24.9 casillas), mecas controlados y grupos de control.
  - **Capacidades:** Estadísticas en tiempo real de Biotech (`MechBandwidth`, `MechControlGroups`, `MechRemoteRepairDistance`, `MechRemoteShieldDistance`, `MechRemoteShieldEnergy`, `MechFormingSpeed`, `MechRepairSpeed`, `MechWorkSpeed`).
  - **Implantes de Mecanizador:** Detección dinámica de implantes instalados y resumen compacto de sus modificadores.
  - **Equipamiento de Mecanizador:** Filtrado e inspección de prendas, armaduras y armas que otorguen modificadores de mecanizador, con iconos y tooltips.
- **Sistema de Mod Settings (`MechanitorExpandedMod` & `MechanitorExpandedSettings`):**
  - Sliders de configuración para ajustar el ancho (430px - 650px) y alto (400px - 700px) de la pestaña.
  - Filtros de visibilidad para filas y secciones.
  - Configuración del estado inicial desplegado/colapsado de los acordeones.
  - **Reubicación de Fabricación al Codificador de Subnúcleos:** Opción configurable en caliente para mover todas las recetas de implantes de mecanizador desde la Mesa de Mecanizado al Codificador de Subnúcleos (`Subcore Encoder`), despejando la mesa de trabajo.
- **Parches y Compatibilidad:**
  - Inyección no destructiva mediante `PatchOperationFindMod` verificando el DLC `Biotech`.
  - Soporte de idiomas en Español (`Mecanit.`), Español Latino e Inglés (`Mechanitor`).
