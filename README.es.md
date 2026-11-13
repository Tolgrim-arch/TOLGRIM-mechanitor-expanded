# TOLGRIM: Mechanitor Expanded

**RimWorld 1.6+ | Requiere DLC Biotech**

Una expansión y colección de mejoras de calidad de vida (QoL) estilo *Vanilla Expanded* para Mecanizadores y Mecanoides. Diseñada con integración nativa, compatibilidad total con partidas guardadas y opciones configurables en Ajustes de Mods.

> [Read in English](README.md)

---

## Características

### 1. Pestaña Nativa de Mecanizador
- Añade la pestaña **Mecanizador** (`ITab_Pawn_Mechanitor`) en colonos con Mechlink.
- Secciones colapsables: **Información General**, **Capacidades**, **Implantes** y **Equipamiento**.
- Dimensiones configurables, visibilidad de secciones y opción para mover recetas de implantes al **Codificador de Subnúcleos**.

### 2. Refuerzo de Ancho de Banda
- **Neurofibra Meca I–IV:** Implantes excluyentes entre sí y desbloqueables por investigación que otorgan `+3`, `+6`, `+9` y `+12` de ancho de banda.
- **Suero de Nodo de Banda:** Suero consumible que otorga `+1` de ancho de banda por dosis (límite configurable, `36` por defecto).
- Integración nativa con `StatDefOf.MechBandwidth` y desglose detallado en la pestaña del Mecanizador.

### 3. Resurrección de Mecanoides Hostiles y Soporte para Apocriton
- Recetas de reprogramación en gestadores para resucitar cadáveres de mecanoides enemigos usando subnúcleos y acero.
- **Soporte de Apocriton aliados:**
  - Receta balanceada de gestación (`Tolgrim_GestateApocriton`) en `LargeMechGestator` (`UltraMechtech`, Ancho de banda: `6`).
  - Suprime el aura de odio, efectos visuales hostiles y el pulso PEM (y su texto de advertencia) al morir en Apocriton del jugador.
  - Habilita los gizmos de recargador para Apocriton aliados.

### 4. Mantenimiento, Reparación y Transporte a Cargadores
- **Reparación Pasiva en Cargadores:** Los mecanoides que se recargan en un cargador con energía curan heridas y (opcionalmente) regeneran partes perdidas, escalando con las investigaciones Biotech (`BasicMechtech` → `UltraMechtech`).
- **Transporte de Mecanoides al Cargador:** Permite que mecanoides trabajadores (ej. Lifters) y colonos lleven mecanoides derribados, descargados o heridos a cargadores disponibles.
- **Mantener Mecanoides de Pie en Autoapagado:** Opción para evitar que se acuesten durante `SelfShutdown` y permitir transportar mecanoides con `0%` de energía al cargador.

### 5. Grupos de Control, Colores y Órdenes Manuales
- **Renombrado y Colores de Grupos:** Clic derecho en el gizmo de grupo para renombrar o personalizar colores de los mecanoides.
- **Órdenes Manuales Forzadas:** Opción para habilitar órdenes directas con clic derecho en mecanoides, incluyendo manejo QoL de armas (conserva el arma nativa `destroyOnDrop` en el inventario al equipar un arma externa, la restaura al soltarla y suelta armas externas al morir).

---

## Configuración

Todas las funciones principales pueden activarse, desactivarse o ajustarse en **Opciones → Ajustes de mods → TOLGRIM: Mechanitor Expanded**.

---

## Créditos y Requisitos

- **Autor:** TOLGRIM
- **PackageId:** `Tolgrim.MechanitorExpanded`
- **Dependencias:** RimWorld Biotech, Harmony (`0Harmony.dll`)
