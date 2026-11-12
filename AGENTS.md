# AGENTS.md — TOLGRIM: Mechanitor Expanded

Guía operativa, estándares de ingeniería y contexto de desarrollo para agentes de IA que trabajen en este repositorio.

---

## 1. Contexto del Proyecto

- **Nombre del Mod:** TOLGRIM: Mechanitor Expanded
- **PackageId:** `Tolgrim.MechanitorExpanded`
- **Juego Base:** RimWorld (Versión objetivo: `1.6.4871+`)
- **DLC Obligatorio:** Biotech (`Ludeon.RimWorld.Biotech`)
- **Target Framework:** `.NET Framework 4.8` (`net48`), C# `latest` / nullable enable.
- **Ruta del repositorio:** `c:\Users\jelys\3D Objects\antigravity-projects\rimworld-modding\TOLGRIM-mechanitor-expanded`
- **Ruta de instalación en RimWorld:** `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\TOLGRIM - Mechanitor Expanded`

---

## 2. Reglas y Estándares de Ingeniería

1. **Principio de Mínimo Código (KISS, DRY, SOLID):**
   - No crear abstracciones superfluas, menús flotantes innecesarios ni duplicar sistemas de RimWorld o Biotech.
   - Reutilizar clases nativas (`ITab`, `StatWorker`, `StatPart`, `CompUseEffect`, `Recipe_InstallImplant`, etc.).
2. **Sin Modificaciones Destructivas:**
   - Evitar reemplazar `Defs` o métodos originales si no es estrictamente indispensable.
   - Parchear con `PatchOperation` de forma condicional (`PatchOperationFindMod` verificando `Biotech`).
   - Evitar Harmony si la arquitectura nativa permite resolver la funcionalidad con `StatPart`, `Comp` o `ITab`.
3. **Persistencia y Compatibilidad de Guardados:**
   - Guardar datos personalizados en componentes asociados a cosas reales (`ThingComp` / `CompProperties`).
   - Nunca dejar referencias rotas si un módulo o sub-sistema es desactivado por el usuario en `ModSettings`.
4. **Reutilización Gráfica:**
   - No crear ni copiar archivos `.png` sueltos para items que pueden reutilizar texturas existentes de Core o Biotech. Resolver mediante rutas nativas (`Things/Pawn/Humanlike/Apparel/Mechlink`, `Things/Item/Special/MechSerumHealer`).
5. **Localización Obligatoria:**
   - Cualquier texto visible en la UI debe tener clave en `Keyed/` con traducciones en:
     - `1.6/Languages/English/Keyed/`
     - `1.6/Languages/Spanish/Keyed/`
     - `1.6/Languages/SpanishLatin/Keyed/`

---

## 3. Estructura del Repositorio

```
├── .gitignore
├── AGENTS.md                                   # Este archivo
├── About/
│   └── About.xml                               # Metadatos del mod para RimWorld
├── 1.6/
│   ├── Assemblies/
│   │   ├── Tolgrim_MechanitorExpanded.dll      # Ensamblado distribuido
│   │   └── Tolgrim_MechanitorExpanded.pdb
│   ├── Defs/
│   │   ├── HediffDefs/
│   │   │   └── Hediffs_Neurofibre.xml          # Hediffs, items y cirugías de Neurofibra I-IV
│   │   └── ThingDefs_Items/
│   │       └── Items_BandnodeSerum.xml         # Suero Bandnode y receta médica
│   ├── Languages/
│   │   ├── English/Keyed/                      # Textos en inglés
│   │   ├── Spanish/Keyed/                      # Textos en español
│   │   └── SpanishLatin/Keyed/                 # Textos en español latino
│   └── Patches/
│       ├── MechanitorTab_Patch.xml             # Inyección de ITab_Pawn_Mechanitor en BasePawn
│       └── MechanitorPhase2_Patch.xml          # Inyección de CompMechanitorBandwidth y StatPart
└── Source/
    ├── Tolgrim_MechanitorExpanded.csproj       # Proyecto MSBuild para dotnet
    ├── BandwidthBonusSystem.cs                 # Registry, IBandwidthBonusProvider, StatPart y Comp
    ├── CompUseEffect_BandnodeSerum.cs          # Lógica del consumible Suero Bandnode
    ├── ITab_Pawn_Mechanitor.cs                 # Pestaña nativa de inspección del mecanizador
    ├── MechanitorExpandedMod.cs                # Clase Mod y render de Mod Settings
    ├── MechanitorExpandedSettings.cs           # Serialización de opciones del mod
    ├── NeurofibreRecipes.cs                    # Lógica de reemplazo quirúrgico de neurofibra
    └── SubcoreEncoderRecipeManager.cs          # Reubicación dinámica de recetas al Subcore Encoder
```

---

## 4. Comandos de Compilación y Despliegue

### Compilar el proyecto C#
```powershell
& "C:\Users\jelys\scoop\apps\dotnet-sdk\current\dotnet.exe" build "Source\Tolgrim_MechanitorExpanded.csproj" -c Release
```

### Sincronizar con el directorio del juego
```powershell
Copy-Item -Path "Source\1.6\Assemblies\*" -Destination "1.6\Assemblies" -Force
$target = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\TOLGRIM - Mechanitor Expanded"
Copy-Item -Path "About" -Destination "$target\About" -Recurse -Force
Copy-Item -Path "1.6" -Destination "$target\1.6" -Recurse -Force
```

---

## 5. Fases Implementadas

- **Fase 1:**
  - Pestaña nativa «Mecanizador» (`ITab_Pawn_Mechanitor`) en el panel de inspección de colonos (visible solo con `Mechlink`).
  - Acordeones colapsables para Información General, Capacidades, Implantes y Equipamiento.
  - Opciones de Mod Settings para dimensiones y visibilidad.
  - Reubicación opcional en caliente de recetas de implantes de mecanizador al *Codificador de subnúcleos* (`Subcore Encoder`).
- **Fase 2:**
  - Sistema modular de bonificaciones permanentes de ancho de banda (`BandwidthBonusRegistry`).
  - Inyección en `StatDefOf.MechBandwidth` mediante `StatPart_MechanitorExpandedBandwidth`.
  - 4 niveles de implantes *Mech Neurofibre* (I: +3, II: +6, III: +9, IV: +12) mutuamente excluyentes y escalonados en tecnología.
  - Sueros exóticos *Bandnode Sera* (+1 de ancho de banda por dosis, configurable hasta 36 dosis).
  - Persistencia de datos por mecanizador (`CompMechanitorBandwidth`).
  - Tooltip explicativo dinámico y detallado en la pestaña «Mecanizador».
