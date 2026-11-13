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

        // Fase 3: Resurrección y hackeo de mecanoides enemigos / Resurrección selectiva
        public bool enableResurrectEnemyMechs = true;
        public bool suppressFriendlyApocritonHateAura = true;
        public bool enableApocritonRecharge = true;
        public bool enableApocritonGestation = true;
        public bool enableForgetfulMechResurrection = true;

        // Fase 4: Ajustes generales del mecanizador y mecanoides
        public float baseCommandRange = 24.9f;
        public float mechWorkSpeedMultiplier = 1.0f;
        public float mechGestationSpeedMultiplier = 1.0f;
        public bool instantMechGestation = false;
        public bool infiniteMechEnergy = false;
        public bool disableMechWastepacks = false;

        // Fase 5: Fabricación en el Subcore Encoder (Subnúcleos, Chips y Mechlink)
        public bool craftSubcoreRegular = true;
        public int costSubcoreRegularSteel = 50;
        public int costSubcoreRegularComponents = 4;

        public bool craftSubcoreHigh = true;
        public int costSubcoreHighSteel = 50;
        public int costSubcoreHighComponents = 4;

        public bool craftSignalChip = true;
        public int costSignalChipPlasteel = 25;
        public int costSignalChipUranium = 5;
        public int costSignalChipComponents = 2;

        public bool craftPowerfocusChip = true;
        public int costPowerfocusChipPlasteel = 25;
        public int costPowerfocusChipUranium = 5;
        public int costPowerfocusChipComponents = 3;

        public bool craftNanostructuringChip = true;
        public int costNanostructuringChipPlasteel = 50;
        public int costNanostructuringChipUranium = 15;
        public int costNanostructuringChipSpacerComponents = 2;

        public bool craftMechlink = true;
        public int costMechlinkNanostructuringChips = 2;
        public int costMechlinkSpacerComponents = 10;

        // Parche integrado: Chips mecanoides indestructibles (Unbreakable Mech Chips)
        public bool unbreakableMechChips = false;

        // Fase 6: Reparación en cargadores mecanoides (Mech Chargers Repair Mechs)
        public bool enableChargerRepair = true;
        public bool restoreMissingPartsInCharger = true;
        public float chargerRepairSpeedMultiplier = 1.0f;

        // Fase 7: Transporte de mecanoides al cargador (Mechs Can Haul To Charger)
        public bool enableMechsHaulToCharger = true;
        public bool haulInjuredMechsToCharger = true;

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
            Scribe_Values.Look(ref enableForgetfulMechResurrection, "enableForgetfulMechResurrection", true);

            Scribe_Values.Look(ref baseCommandRange, "baseCommandRange", 24.9f);
            Scribe_Values.Look(ref mechWorkSpeedMultiplier, "mechWorkSpeedMultiplier", 1.0f);
            Scribe_Values.Look(ref mechGestationSpeedMultiplier, "mechGestationSpeedMultiplier", 1.0f);
            Scribe_Values.Look(ref instantMechGestation, "instantMechGestation", false);
            Scribe_Values.Look(ref infiniteMechEnergy, "infiniteMechEnergy", false);
            Scribe_Values.Look(ref disableMechWastepacks, "disableMechWastepacks", false);

            Scribe_Values.Look(ref craftSubcoreRegular, "craftSubcoreRegular", true);
            Scribe_Values.Look(ref costSubcoreRegularSteel, "costSubcoreRegularSteel", 50);
            Scribe_Values.Look(ref costSubcoreRegularComponents, "costSubcoreRegularComponents", 4);

            Scribe_Values.Look(ref craftSubcoreHigh, "craftSubcoreHigh", true);
            Scribe_Values.Look(ref costSubcoreHighSteel, "costSubcoreHighSteel", 50);
            Scribe_Values.Look(ref costSubcoreHighComponents, "costSubcoreHighComponents", 4);

            Scribe_Values.Look(ref craftSignalChip, "craftSignalChip", true);
            Scribe_Values.Look(ref costSignalChipPlasteel, "costSignalChipPlasteel", 25);
            Scribe_Values.Look(ref costSignalChipUranium, "costSignalChipUranium", 5);
            Scribe_Values.Look(ref costSignalChipComponents, "costSignalChipComponents", 2);

            Scribe_Values.Look(ref craftPowerfocusChip, "craftPowerfocusChip", true);
            Scribe_Values.Look(ref costPowerfocusChipPlasteel, "costPowerfocusChipPlasteel", 25);
            Scribe_Values.Look(ref costPowerfocusChipUranium, "costPowerfocusChipUranium", 5);
            Scribe_Values.Look(ref costPowerfocusChipComponents, "costPowerfocusChipComponents", 3);

            Scribe_Values.Look(ref craftNanostructuringChip, "craftNanostructuringChip", true);
            Scribe_Values.Look(ref costNanostructuringChipPlasteel, "costNanostructuringChipPlasteel", 50);
            Scribe_Values.Look(ref costNanostructuringChipUranium, "costNanostructuringChipUranium", 15);
            Scribe_Values.Look(ref costNanostructuringChipSpacerComponents, "costNanostructuringChipSpacerComponents", 2);

            Scribe_Values.Look(ref craftMechlink, "craftMechlink", true);
            Scribe_Values.Look(ref costMechlinkNanostructuringChips, "costMechlinkNanostructuringChips", 2);
            Scribe_Values.Look(ref costMechlinkSpacerComponents, "costMechlinkSpacerComponents", 10);
            Scribe_Values.Look(ref unbreakableMechChips, "unbreakableMechChips", false);

            Scribe_Values.Look(ref enableChargerRepair, "enableChargerRepair", true);
            Scribe_Values.Look(ref restoreMissingPartsInCharger, "restoreMissingPartsInCharger", true);
            Scribe_Values.Look(ref chargerRepairSpeedMultiplier, "chargerRepairSpeedMultiplier", 1.0f);

            Scribe_Values.Look(ref enableMechsHaulToCharger, "enableMechsHaulToCharger", true);
            Scribe_Values.Look(ref haulInjuredMechsToCharger, "haulInjuredMechsToCharger", true);
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
            enableForgetfulMechResurrection = true;
            baseCommandRange = 24.9f;
            mechWorkSpeedMultiplier = 1.0f;
            mechGestationSpeedMultiplier = 1.0f;
            instantMechGestation = false;
            infiniteMechEnergy = false;
            disableMechWastepacks = false;

            craftSubcoreRegular = true;
            costSubcoreRegularSteel = 50;
            costSubcoreRegularComponents = 4;

            craftSubcoreHigh = true;
            costSubcoreHighSteel = 50;
            costSubcoreHighComponents = 4;

            craftSignalChip = true;
            costSignalChipPlasteel = 25;
            costSignalChipUranium = 5;
            costSignalChipComponents = 2;

            craftPowerfocusChip = true;
            costPowerfocusChipPlasteel = 25;
            costPowerfocusChipUranium = 5;
            costPowerfocusChipComponents = 3;

            craftNanostructuringChip = true;
            costNanostructuringChipPlasteel = 50;
            costNanostructuringChipUranium = 15;
            costNanostructuringChipSpacerComponents = 2;

            craftMechlink = true;
            costMechlinkNanostructuringChips = 2;
            costMechlinkSpacerComponents = 10;
            unbreakableMechChips = false;

            enableChargerRepair = true;
            restoreMissingPartsInCharger = true;
            chargerRepairSpeedMultiplier = 1.0f;
            enableMechsHaulToCharger = true;
            haulInjuredMechsToCharger = true;
        }
    }
}
