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

    public static class Patch_VerbProperties
    {

        [HarmonyPatch(typeof(VerbProperties))]
        [HarmonyPatch(nameof(VerbProperties.GetDamageFactorFor))]
        [HarmonyPatch(new Type[] { typeof(Tool), typeof(Pawn), typeof(HediffComp_VerbGiver) })]
        public static class Patch_GetDamageFactorFor
        {

            public static void Postfix(Pawn attacker, ref float __result)
            {
                // If an attacker exists and has an adrenaline rush hediff, multiply based on the stage
                if (attacker != null)
                {
                    var extraRaceProps = attacker.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;
                    var adrenalineHediff = attacker.health.hediffSet.GetFirstHediffOfDef(extraRaceProps.adrenalineRushHediff);
                    if (adrenalineHediff != null)
                    {
                        var hediffDefExtension = adrenalineHediff.def.GetModExtension<HediffDefExtension>() ?? HediffDefExtension.defaultValues;
                        var extraHediffStageProps = hediffDefExtension.GetExtraHediffStagePropertiesAt(adrenalineHediff.CurStageIndex);
                        Log.Message(extraHediffStageProps.meleeDamageFactor.ToString());
                        __result *= extraHediffStageProps.meleeDamageFactor;
                    }
                }
            }

        }

    }

}
