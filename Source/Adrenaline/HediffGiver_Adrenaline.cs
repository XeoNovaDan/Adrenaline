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

        private const float BaseSeverityGainPerDamageTaken = 0.005f;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                var storyTracker = pawn.story;
                bool hasRush = pawn.health.hediffSet.HasHediff(extraRaceProps.adrenalineRushHediff);

                // If the pawn isn't cool-headed and doesn't already have an adrenaline rush, add adrenaline rush
                if ((storyTracker == null || !storyTracker.traits.HasTrait(A_TraitDefOf.CoolHeaded)) && !hasRush && AdrenalineUtility.GetPerceivedThreatsFor(pawn).Any())
                    pawn.health.AddHediff(extraRaceProps.adrenalineRushHediff);

                // Otherwise if they have an adrenaline rush and don't have an adrenaline crash hediff, add an adrenaline crash hediff
                else if (hasRush && extraRaceProps.adrenalineCrashHediff != null && !pawn.health.hediffSet.HasHediff(extraRaceProps.adrenalineCrashHediff))
                    pawn.health.AddHediff(extraRaceProps.adrenalineCrashHediff);

            }
        }

        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                var storyTracker = pawn.story;

                // Hediff isn't an injury, is a scar, the pawn is dead or the pawn has the 'cool-headed' trait
                var injury = hediff as Hediff_Injury;
                if (injury == null || injury.IsPermanent() || pawn.Dead || (storyTracker != null && storyTracker.traits.HasTrait(A_TraitDefOf.CoolHeaded)))
                    return false;

                float severityToAdd = BaseSeverityGainPerDamageTaken * extraRaceProps.adrenalineGainFactorNatural * injury.Severity / pawn.HealthScale;
                HealthUtility.AdjustSeverity(pawn, extraRaceProps.adrenalineRushHediff, severityToAdd);
                return true;
            }

            return false;
        }

    }

}
