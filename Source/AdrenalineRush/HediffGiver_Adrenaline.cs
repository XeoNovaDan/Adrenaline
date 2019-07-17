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

        //private List<Thing> cachedHostileThings = new List<Thing>();

        private const int MinTicksSinceDamageTakenForAdrenalineFall = 600;

        private const float BaseSeverityGainPerDamageTaken = 0.01f;

        private const float BaseSeverityGainPerHour = 0.3f;

        private const float BaseSeverityLossPerHour = 0.5f;

        private float HostileThingTotalRelativeEffectiveCombatPower(IEnumerable<Thing> hostileThings, Pawn pawn) => hostileThings.Sum(t => pawn.EffectiveCombatPower() / t.EffectiveCombatPower());

        private float HostileThingTotalRelativeBodySize(IEnumerable<Thing> hostileThings, Pawn pawn) => hostileThings.Sum(t =>
        {
            if (t is Pawn p)
            {
                return pawn.BodySize / p.BodySize;
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
            // Update cached hostile attack searchers
            var map = pawn.Map;
            if (map != null)
            {
                var perceivedThreats = map.GetComponent<MapComponent_AdrenalineCache>().allPotentialHostileThings?.Where(t => t.IsPerceivedThreatBy(pawn));

                // Apply adrenaline if there are any hostile things
                if (perceivedThreats != null && perceivedThreats.Any())
                {
                    float severityMultiplier = pawn.RaceProps.Humanlike ?
                        TotalRelativeScoreToAdrenalineGainFactorCurve.Evaluate(HostileThingTotalRelativeEffectiveCombatPower(perceivedThreats, pawn)) :
                        TotalRelativeScoreToAdrenalineGainFactorCurve.Evaluate(HostileThingTotalRelativeBodySize(perceivedThreats, pawn));

                    float severityToAdd = BaseSeverityGainPerHour / GenDate.TicksPerHour * severityMultiplier * GenTicks.TicksPerRealSecond;
                    HealthUtility.AdjustSeverity(pawn, hediff, severityToAdd);
                }
            }

            // Otherwise reduce severity if it has been at least 600 ticks since the pawn was last harmed
            else if (Find.TickManager.TicksGame >= pawn.mindState.lastHarmTick + MinTicksSinceDamageTakenForAdrenalineFall)
            {
                float severityToRemove = BaseSeverityLossPerHour / GenDate.TicksPerHour * GenTicks.TicksPerRealSecond;
                HealthUtility.AdjustSeverity(pawn, hediff, -severityToRemove);
            }
        }

        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            // Hediff isn't an injury or the pawn is dead
            var injury = hediff as Hediff_Injury;
            if (injury == null || pawn.Dead)
                return false;

            float severityToAdd = BaseSeverityGainPerDamageTaken * injury.Severity / pawn.HealthScale;
            HealthUtility.AdjustSeverity(pawn, this.hediff, severityToAdd);

            return true;
        }

    }

}
