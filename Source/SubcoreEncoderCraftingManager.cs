using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    [StaticConstructorOnStartup]
    public static class SubcoreEncoderCraftingManager
    {
        public static readonly string SubcoreRegularDefName = "Tolgrim_CraftSubcoreRegular";
        public static readonly string SubcoreHighDefName = "Tolgrim_CraftSubcoreHigh";
        public static readonly string SignalChipDefName = "Tolgrim_CraftSignalChip";
        public static readonly string PowerfocusChipDefName = "Tolgrim_CraftPowerfocusChip";
        public static readonly string NanostructuringChipDefName = "Tolgrim_CraftNanostructuringChip";
        public static readonly string MechlinkDefName = "Tolgrim_CraftMechlink";

        static SubcoreEncoderCraftingManager()
        {
            ApplyAllIngredientCosts();
        }

        public static void ApplyAllIngredientCosts()
        {
            MechanitorExpandedSettings s = MechanitorExpandedMod.Settings;
            if (s == null) return;

            // 1. Subcore Regular (SubcoreBasic: 1, Steel, ComponentIndustrial)
            ApplyRecipeCosts(SubcoreRegularDefName, new Dictionary<string, int>
            {
                { "SubcoreBasic", 1 },
                { "Steel", s.costSubcoreRegularSteel },
                { "ComponentIndustrial", s.costSubcoreRegularComponents }
            });

            // 2. Subcore High (SubcoreRegular: 1, Steel, ComponentIndustrial)
            ApplyRecipeCosts(SubcoreHighDefName, new Dictionary<string, int>
            {
                { "SubcoreRegular", 1 },
                { "Steel", s.costSubcoreHighSteel },
                { "ComponentIndustrial", s.costSubcoreHighComponents }
            });

            // 3. Signal Chip (SubcoreBasic: 1, Plasteel, Uranium, ComponentIndustrial)
            ApplyRecipeCosts(SignalChipDefName, new Dictionary<string, int>
            {
                { "SubcoreBasic", 1 },
                { "Plasteel", s.costSignalChipPlasteel },
                { "Uranium", s.costSignalChipUranium },
                { "ComponentIndustrial", s.costSignalChipComponents }
            });

            // 4. Powerfocus Chip (SignalChip: 1, Plasteel, Uranium, ComponentIndustrial)
            ApplyRecipeCosts(PowerfocusChipDefName, new Dictionary<string, int>
            {
                { "SignalChip", 1 },
                { "Plasteel", s.costPowerfocusChipPlasteel },
                { "Uranium", s.costPowerfocusChipUranium },
                { "ComponentIndustrial", s.costPowerfocusChipComponents }
            });

            // 5. Nanostructuring Chip (PowerfocusChip: 1, Plasteel, Uranium, ComponentSpacer)
            ApplyRecipeCosts(NanostructuringChipDefName, new Dictionary<string, int>
            {
                { "PowerfocusChip", 1 },
                { "Plasteel", s.costNanostructuringChipPlasteel },
                { "Uranium", s.costNanostructuringChipUranium },
                { "ComponentSpacer", s.costNanostructuringChipSpacerComponents }
            });

            // 6. Mechlink (NanostructuringChip, ComponentSpacer)
            ApplyRecipeCosts(MechlinkDefName, new Dictionary<string, int>
            {
                { "NanostructuringChip", s.costMechlinkNanostructuringChips },
                { "ComponentSpacer", s.costMechlinkSpacerComponents }
            });
        }

        private static void ApplyRecipeCosts(string recipeDefName, Dictionary<string, int> costs)
        {
            RecipeDef? recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(recipeDefName);
            if (recipe == null || recipe.ingredients == null) return;

            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                IngredientCount ing = recipe.ingredients[i];
                ThingDef? fixedDef = ing.filter?.AnyAllowedDef;
                if (fixedDef != null && costs.TryGetValue(fixedDef.defName, out int count))
                {
                    ing.SetBaseCount(Mathf.Max(1, count));
                }
            }
        }

        public static bool IsRecipeAvailable(RecipeDef recipe)
        {
            MechanitorExpandedSettings s = MechanitorExpandedMod.Settings;
            if (s == null) return true;

            if (recipe.defName == SubcoreRegularDefName) return s.craftSubcoreRegular;
            if (recipe.defName == SubcoreHighDefName) return s.craftSubcoreHigh;
            if (recipe.defName == SignalChipDefName) return s.craftSignalChip;
            if (recipe.defName == PowerfocusChipDefName) return s.craftPowerfocusChip;
            if (recipe.defName == NanostructuringChipDefName) return s.craftNanostructuringChip;
            if (recipe.defName == MechlinkDefName) return s.craftMechlink;

            return true;
        }
    }
}
