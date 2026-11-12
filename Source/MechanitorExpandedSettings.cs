using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class MechanitorExpandedSettings : ModSettings
    {
        // Pestaña y apariencia
        public float tabWidth = 490f;
        public float tabHeight = 480f;
        public bool showCommandRange = true;
        public bool showControlGroups = true;

        // Secciones activas / visibles
        public bool showImplantsSection = true;
        public bool showEquipmentSection = true;

        // Estado inicial de colapso de los acordeones
        public bool defaultExpandGeneral = true;
        public bool defaultExpandStats = true;
        public bool defaultExpandImplants = true;
        public bool defaultExpandEquipment = true;

        // Organización de fabricación: mover recetas de implantes de mecanizador al Subcore Encoder
        public bool moveRecipesToSubcoreEncoder = false;

        // Fase 2: Neurofibra y Sueros Bandnode
        public bool enableNeurofibre = true;
        public bool enableBandnodeSera = true;
        public int maxBandnodeSeraDoses = 36;
        public float neurofibreMultiplier = 1.0f;
        public float serumMultiplier = 1.0f;

        // Fase 3: Resurrección y hackeo de mecanoides enemigos
        public bool enableResurrectEnemyMechs = true;
        public bool suppressFriendlyApocritonHateAura = true;
        public bool enableApocritonRecharge = true;
        public bool enableApocritonGestation = true;

        // Fase 4: Ajustes generales del mecanizador y mecanoides
        public float baseCommandRange = 24.9f;
        public float mechWorkSpeedMultiplier = 1.0f;
        public float mechGestationSpeedMultiplier = 1.0f;
        public bool instantMechGestation = false;
        public bool infiniteMechEnergy = false;
        public bool disableMechWastepacks = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tabWidth, "tabWidth", 490f);
            Scribe_Values.Look(ref tabHeight, "tabHeight", 480f);
            Scribe_Values.Look(ref showCommandRange, "showCommandRange", true);
            Scribe_Values.Look(ref showControlGroups, "showControlGroups", true);

            Scribe_Values.Look(ref showImplantsSection, "showImplantsSection", true);
            Scribe_Values.Look(ref showEquipmentSection, "showEquipmentSection", true);

            Scribe_Values.Look(ref defaultExpandGeneral, "defaultExpandGeneral", true);
            Scribe_Values.Look(ref defaultExpandStats, "defaultExpandStats", true);
            Scribe_Values.Look(ref defaultExpandImplants, "defaultExpandImplants", true);
            Scribe_Values.Look(ref defaultExpandEquipment, "defaultExpandEquipment", true);

            Scribe_Values.Look(ref moveRecipesToSubcoreEncoder, "moveRecipesToSubcoreEncoder", false);

            Scribe_Values.Look(ref enableNeurofibre, "enableNeurofibre", true);
            Scribe_Values.Look(ref enableBandnodeSera, "enableBandnodeSera", true);
            Scribe_Values.Look(ref maxBandnodeSeraDoses, "maxBandnodeSeraDoses", 36);
            Scribe_Values.Look(ref neurofibreMultiplier, "neurofibreMultiplier", 1.0f);
            Scribe_Values.Look(ref serumMultiplier, "serumMultiplier", 1.0f);

            Scribe_Values.Look(ref enableResurrectEnemyMechs, "enableResurrectEnemyMechs", true);
            Scribe_Values.Look(ref suppressFriendlyApocritonHateAura, "suppressFriendlyApocritonHateAura", true);
            Scribe_Values.Look(ref enableApocritonRecharge, "enableApocritonRecharge", true);
            Scribe_Values.Look(ref enableApocritonGestation, "enableApocritonGestation", true);

            Scribe_Values.Look(ref baseCommandRange, "baseCommandRange", 24.9f);
            Scribe_Values.Look(ref mechWorkSpeedMultiplier, "mechWorkSpeedMultiplier", 1.0f);
            Scribe_Values.Look(ref mechGestationSpeedMultiplier, "mechGestationSpeedMultiplier", 1.0f);
            Scribe_Values.Look(ref instantMechGestation, "instantMechGestation", false);
            Scribe_Values.Look(ref infiniteMechEnergy, "infiniteMechEnergy", false);
            Scribe_Values.Look(ref disableMechWastepacks, "disableMechWastepacks", false);
        }

        public void ResetToDefaults()
        {
            tabWidth = 490f;
            tabHeight = 480f;
            showCommandRange = true;
            showControlGroups = true;
            showImplantsSection = true;
            showEquipmentSection = true;
            defaultExpandGeneral = true;
            defaultExpandStats = true;
            defaultExpandImplants = true;
            defaultExpandEquipment = true;
            moveRecipesToSubcoreEncoder = false;
            enableNeurofibre = true;
            enableBandnodeSera = true;
            maxBandnodeSeraDoses = 36;
            neurofibreMultiplier = 1.0f;
            serumMultiplier = 1.0f;
            enableResurrectEnemyMechs = true;
            suppressFriendlyApocritonHateAura = true;
            enableApocritonRecharge = true;
            enableApocritonGestation = true;
            baseCommandRange = 24.9f;
            mechWorkSpeedMultiplier = 1.0f;
            mechGestationSpeedMultiplier = 1.0f;
            instantMechGestation = false;
            infiniteMechEnergy = false;
            disableMechWastepacks = false;
        }
    }
}
