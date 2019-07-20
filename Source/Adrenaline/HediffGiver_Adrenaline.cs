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

        public const float BaseSeverityGainPerThreatNeutralised = 0.05f;

        private const float BaseSeverityGainPerDamageTaken = 0.004f;

        private const float BaseSeverityGainPerHour = 0.1f;

        private const float SeverityGainOverTimeFactorDowned = 0.5f;

        private static readonly SimpleCurve TotalPerceivedThreatSignificanceToAdrenalineGainFactorCurve = new SimpleCurve()
        {
            new CurvePoint(0, 0),
            new CurvePoint(1, 1),
            new CurvePoint(4, 2)
        };

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            var extraRaceProps = pawn.def.GetModExtension<ExtraRaceProperties>() ?? ExtraRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                var map = pawn.Map;

                // If the pawn isn't a world or caravan pawn...
                if (map != null)
                {
                    // Get all pawns and things (e.g. turrets) that are perceived as threats by the pawn
                    var perceivedThreats = map.GetComponent<MapComponent_AdrenalineTracker>().allPotentialHostileThings?.Where(t => t.IsPerceivedThreatBy(pawn));

                    // Apply adrenaline if there are any perceived threats
                    if (perceivedThreats != null && perceivedThreats.Any())
                    {
                        float totalPerceivedThreatSignificance = 0;
                        foreach (var threat in perceivedThreats)
                            totalPerceivedThreatSignificance += threat.PerceivedThreatSignificanceFor(pawn);

                        float severityGain = BaseSeverityGainPerHour / GenDate.TicksPerHour * // Base
                            extraRaceProps.adrenalineGainFactorNatural * // From DefModExtension
                            TotalPerceivedThreatSignificanceToAdrenalineGainFactorCurve.Evaluate(totalPerceivedThreatSignificance) * // From perceived threats
                            (pawn.Downed ? SeverityGainOverTimeFactorDowned : 1) * // From downed state
                            HealthTuning.HediffGiverUpdateInterval;

                        HealthUtility.AdjustSeverity(pawn, extraRaceProps.adrenalineHediff, severityGain);
                    }
                }
            }
        }

        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            var extraRaceProps = pawn.def.GetModExtension<ExtraRaceProperties>() ?? ExtraRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                // Hediff isn't an injury, is a scar or the pawn is dead
                var injury = hediff as Hediff_Injury;
                if (injury == null || injury.IsPermanent() || pawn.Dead)
                    return false;

                float severityToAdd = BaseSeverityGainPerDamageTaken * extraRaceProps.adrenalineGainFactorNatural * injury.Severity / pawn.HealthScale;
                HealthUtility.AdjustSeverity(pawn, extraRaceProps.adrenalineHediff, severityToAdd);
                return true;
            }

            return false;
        }

    }

}
