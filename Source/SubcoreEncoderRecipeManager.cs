using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    [StaticConstructorOnStartup]
    public static class SubcoreEncoderRecipeManager
    {
        private static readonly FieldInfo? AllRecipesCachedField = typeof(ThingDef).GetField("allRecipesCached", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly List<RecipeDef> TrackedMovedRecipes = new List<RecipeDef>();
        private static bool isApplied = false;

        static SubcoreEncoderRecipeManager()
        {
            // Aplica la configuración guardada al arrancar el juego
            ApplyRecipeRouting(MechanitorExpandedMod.Settings.moveRecipesToSubcoreEncoder);
        }

        public static void ApplyRecipeRouting(bool moveToEncoder)
        {
            if (isApplied == moveToEncoder) return;

            ThingDef? encoderDef = DefDatabase<ThingDef>.GetNamedSilentFail("SubcoreEncoder");
            ThingDef? machiningDef = DefDatabase<ThingDef>.GetNamedSilentFail("TableMachining");

            if (encoderDef == null || machiningDef == null) return;

            if (moveToEncoder)
            {
                TrackedMovedRecipes.Clear();
                List<RecipeDef> allRecipes = DefDatabase<RecipeDef>.AllDefsListForReading;

                for (int i = 0; i < allRecipes.Count; i++)
                {
                    RecipeDef recipe = allRecipes[i];
                    if (IsMechanitorImplantRecipe(recipe))
                    {
                        if (recipe.recipeUsers == null)
                        {
                            recipe.recipeUsers = new List<ThingDef>();
                        }

                        // Quitamos de la mesa de mecanizado
                        recipe.recipeUsers.Remove(machiningDef);

                        // Agregamos al codificador de subnúcleos
                        if (!recipe.recipeUsers.Contains(encoderDef))
                        {
                            recipe.recipeUsers.Add(encoderDef);
                        }

                        TrackedMovedRecipes.Add(recipe);
                    }
                }
            }
            else
            {
                // Restauramos a su estado original en la mesa de mecanizado
                for (int i = 0; i < TrackedMovedRecipes.Count; i++)
                {
                    RecipeDef recipe = TrackedMovedRecipes[i];
                    if (recipe.recipeUsers != null)
                    {
                        recipe.recipeUsers.Remove(encoderDef);
                        if (!recipe.recipeUsers.Contains(machiningDef))
                        {
                            recipe.recipeUsers.Add(machiningDef);
                        }
                    }
                }
                TrackedMovedRecipes.Clear();
            }

            // Limpiamos los cachés de AllRecipes de las mesas afectadas
            ClearRecipeCache(encoderDef);
            ClearRecipeCache(machiningDef);

            isApplied = moveToEncoder;
        }

        private static bool IsMechanitorImplantRecipe(RecipeDef recipe)
        {
            ThingDef? produced = recipe.ProducedThingDef;
            if (produced == null) return false;

            // 1. Tag o Categoría Mechtech (nativo y estándar en mods)
            if (produced.thingCategories != null && produced.thingCategories.Any(c => c.defName == "BodyPartsMechtech"))
            {
                return true;
            }

            // 2. Comps de implante que requieren Mechlink para ser instalados
            if (produced.comps != null)
            {
                for (int i = 0; i < produced.comps.Count; i++)
                {
                    if (produced.comps[i] is CompProperties_Usable usable && usable.userMustHaveHediff != null)
                    {
                        if (usable.userMustHaveHediff.defName == "MechlinkImplant")
                        {
                            return true;
                        }
                    }
                    if (produced.comps[i] is CompProperties_UseEffectInstallImplant installImplant)
                    {
                        string hName = installImplant.hediffDef?.defName ?? "";
                        if (hName.Contains("Sublink") || hName.Contains("RemoteRepairer") || 
                            hName.Contains("RemoteShielder") || hName.Contains("RepairProbe") ||
                            hName.Contains("MechFormfeeder"))
                        {
                            return true;
                        }
                    }
                }
            }

            // 3. Modificadores de estadísticas de mecanizador en el producto
            if (produced.equippedStatOffsets != null)
            {
                for (int i = 0; i < produced.equippedStatOffsets.Count; i++)
                {
                    StatDef stat = produced.equippedStatOffsets[i].stat;
                    if (stat != null && (stat.category?.defName == "Mechanitor" || stat == StatDefOf.MechBandwidth || stat == StatDefOf.MechControlGroups))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void ClearRecipeCache(ThingDef def)
        {
            if (AllRecipesCachedField != null)
            {
                AllRecipesCachedField.SetValue(def, null);
            }
        }
    }
}
