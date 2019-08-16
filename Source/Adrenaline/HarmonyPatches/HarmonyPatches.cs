using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace Adrenaline
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            Adrenaline.HarmonyInstance.PatchAll();

            // PawnInventoryGenerator.GiveCombatEnhancingDrugs source predicate
            Patch_PawnInventoryGenerator.ManualPatch_GiveCombatEnhancingDrugs_source_predicate.predicateType = typeof(PawnInventoryGenerator).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.Name.Contains("GiveCombatEnhancingDrugs"));
            Adrenaline.HarmonyInstance.Patch(Patch_PawnInventoryGenerator.ManualPatch_GiveCombatEnhancingDrugs_source_predicate.predicateType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(m => m.ReturnType == typeof(bool)),
                transpiler: new HarmonyMethod(typeof(Patch_PawnInventoryGenerator.ManualPatch_GiveCombatEnhancingDrugs_source_predicate), "Transpiler"));
        }

    }

}
