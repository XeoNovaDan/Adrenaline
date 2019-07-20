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
                // If an attacker exists and has the Adrenaline hediff, multiply based on the stage
                if (attacker != null)
                {
                    var adrenalineHediff = attacker.health.hediffSet.GetFirstHediffOfDef(A_HediffDefOf.Adrenaline);
                    if (adrenalineHediff != null)
                    {
                        switch (adrenalineHediff.CurStageIndex)
                        {
                            case 0: // Slight
                                __result *= 1.05f;
                                return;

                            case 1: // Moderate
                                __result *= 1.1f;
                                return;

                            case 2: // Intense
                                __result *= 1.15f;
                                return;

                            case 3: // Extreme
                                __result *= 1.2f;
                                return;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }

        }

    }

}
