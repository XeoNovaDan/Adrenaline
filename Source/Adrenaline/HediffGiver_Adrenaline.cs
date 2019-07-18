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

        private float baseSeverityGainPerDamageTaken;
        private float baseSeverityGainPerHour;
        private float baseSeverityLossPerHour;
        private float maxSeverityForGainOverTime = Int32.MaxValue;
        private int minTicksSinceSeverityGainForSeverityLoss;

        private float HostileThingTotalRelativeEffectiveCombatPower(IEnumerable<Thing> hostileThings, Pawn pawn) => hostileThings.Sum(t => t.EffectiveCombatPower() / pawn.EffectiveCombatPower());

        private float HostileThingTotalRelativeBodySize(IEnumerable<Thing> hostileThings, Pawn pawn) => hostileThings.Sum(t =>
        {
            if (t is Pawn p)
            {
                return p.BodySize / pawn.BodySize;
            }
            throw new NotImplementedException();
        });

        private static readonly SimpleCurve TotalRelativeScoreToAdrenalineGainFactorCurve = new SimpleCurve()
        {
            new CurvePoint(0, 0),
            new CurvePoint(1, 1)
        };

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            var adrenalineComp = pawn.GetComp<CompAdrenalineTracker>();
            var map = pawn.Map;
            var adrenalineHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);

            // If the pawn isn't a world or caravan pawn and either doesn't have the adrenaline hediff or it can still passively gain...
            if (map != null && (adrenalineHediff == null || adrenalineHediff.Severity < maxSeverityForGainOverTime))
            {
                // Get all pawns and things (e.g. turrets) that are perceived as threats by the pawn
                var perceivedThreats = map.GetComponent<MapComponent_AdrenalineTracker>().allPotentialHostileThings?.Where(t => t.IsPerceivedThreatBy(pawn));

                // Apply adrenaline if there are any hostile things
                if (perceivedThreats != null && perceivedThreats.Any())
                {
                    float severityMultiplier = pawn.RaceProps.Humanlike ?
                        TotalRelativeScoreToAdrenalineGainFactorCurve.Evaluate(HostileThingTotalRelativeEffectiveCombatPower(perceivedThreats, pawn)) :
                        TotalRelativeScoreToAdrenalineGainFactorCurve.Evaluate(HostileThingTotalRelativeBodySize(perceivedThreats, pawn));

                    float severityGain = baseSeverityGainPerHour / GenDate.TicksPerHour * severityMultiplier * GenTicks.TicksPerRealSecond;
                    HealthUtility.AdjustSeverity(pawn, hediff, severityGain);
                    adrenalineComp.lastAdrenalineGainTick = Find.TickManager.TicksGame;
                    return;
                }
            }

            // Otherwise reduce severity if it has been at least 600 ticks since the pawn was last harmed
            if (Find.TickManager.TicksGame >= adrenalineComp.lastAdrenalineGainTick + minTicksSinceSeverityGainForSeverityLoss)
            {
                float severityLoss = baseSeverityLossPerHour / GenDate.TicksPerHour * GenTicks.TicksPerRealSecond;
                HealthUtility.AdjustSeverity(pawn, hediff, -severityLoss);
            }
        }

        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            // Hediff isn't an injury, is a scar or the pawn is dead
            var injury = hediff as Hediff_Injury;
            if (injury == null || injury.IsPermanent() || pawn.Dead)
                return false;

            float severityToAdd = baseSeverityGainPerDamageTaken * injury.Severity / pawn.HealthScale;
            HealthUtility.AdjustSeverity(pawn, this.hediff, severityToAdd);
            pawn.GetComp<CompAdrenalineTracker>().lastAdrenalineGainTick = Find.TickManager.TicksGame;
            return true;
        }

    }

}
