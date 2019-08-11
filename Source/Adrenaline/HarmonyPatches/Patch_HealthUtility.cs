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

    public static class Patch_HealthUtility
    {

        [HarmonyPatch(typeof(HealthUtility), nameof(HealthUtility.DamageUntilDowned))]
        public static class DamageUntilDowned
        {

            public static void Prefix(Pawn p)
            {
                // Add adrenaline hediff with 1250 tick delay if applicable
                if (p.CanGetAdrenaline())
                {
                    var extendedRaceProps = p.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;
                    var adrenalineHediff = (Hediff_AdrenalineRush)p.health.AddHediff(extendedRaceProps.adrenalineRushHediff);
                    adrenalineHediff.Severity = extendedRaceProps.adrenalineRushHediff.maxSeverity;
                    adrenalineHediff.severityLossDelayTicks = GenDate.TicksPerHour / 2;
                }
            }

        }

        [HarmonyPatch(typeof(HealthUtility), nameof(HealthUtility.DamageLegsUntilIncapableOfMoving))]
        public static class DamageLegsUntilIncapableOfMoving
        {

            public static void Prefix(Pawn p)
            {
                DamageUntilDowned.Prefix(p);
            }

        }

    }

}
