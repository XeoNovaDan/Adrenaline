using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;

namespace Adrenaline
{

    public class HediffGiver_Adrenaline : HediffGiver
    {

        private const float BaseSeverityGainPerDamageTaken = 0.0025f;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                var map = pawn.Map;
                bool hasRush = pawn.health.hediffSet.HasHediff(extraRaceProps.adrenalineRushHediff);

                // If the pawn isn't a world or caravan pawn and doesn't already have an adrenaline rush...
                if (map != null && !pawn.health.hediffSet.HasHediff(extraRaceProps.adrenalineRushHediff))
                {
                    // Get all pawns and things (e.g. turrets) that are perceived as threats by the pawn
                    var perceivedThreats = map.GetComponent<MapComponent_AdrenalineTracker>().allPotentialHostileThings?.Where(t => t.IsPerceivedThreatBy(pawn));

                    // Apply adrenaline if there are any perceived threats
                    if (perceivedThreats != null && perceivedThreats.Any())
                    {
                        pawn.health.AddHediff(extraRaceProps.adrenalineRushHediff);
                    }
                }

                // Otherwise if they have an adrenaline rush and don't have an adrenaline crash hediff, add an adrenaline crash hediff
                else if (hasRush && extraRaceProps.adrenalineCrashHediff != null && !pawn.health.hediffSet.HasHediff(extraRaceProps.adrenalineCrashHediff))
                {
                    pawn.health.AddHediff(extraRaceProps.adrenalineCrashHediff);
                }

            }
        }

        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                // Hediff isn't an injury, is a scar or the pawn is dead
                var injury = hediff as Hediff_Injury;
                if (injury == null || injury.IsPermanent() || pawn.Dead)
                    return false;

                float severityToAdd = BaseSeverityGainPerDamageTaken * extraRaceProps.adrenalineGainFactorNatural * injury.Severity / pawn.HealthScale;
                HealthUtility.AdjustSeverity(pawn, extraRaceProps.adrenalineRushHediff, severityToAdd);
                return true;
            }

            return false;
        }

    }

}
