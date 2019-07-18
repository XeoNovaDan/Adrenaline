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

        private const float BaseSeverityGainPerDamageTaken = 0.02f;

        private const float BaseSeverityGainPerHour = 0.6f;

        private const float BaseSeverityLossPerHour = 1;

        private const float MaxSeverityForGainOverTime = 0.8f;

        private const int MinTicksSinceSeverityGainForSeverityLoss = 600;

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
            var extraRaceProps = pawn.def.GetModExtension<ExtraRaceProperties>() ?? ExtraRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                var adrenalineComp = pawn.GetComp<CompAdrenalineTracker>();
                var map = pawn.Map;
                var adrenalineHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);

                // If the pawn isn't a world or caravan pawn and either doesn't have the adrenaline hediff or it can still passively gain...
                if (map != null && (adrenalineHediff == null || adrenalineHediff.Severity < MaxSeverityForGainOverTime))
                {
                    // Get all pawns and things (e.g. turrets) that are perceived as threats by the pawn
                    var perceivedThreats = map.GetComponent<MapComponent_AdrenalineTracker>().allPotentialHostileThings?.Where(t => t.IsPerceivedThreatBy(pawn));

                    // Apply adrenaline if there are any hostile things
                    if (perceivedThreats != null && perceivedThreats.Any())
                    {
                        float relativeScore = pawn.RaceProps.Humanlike ?
                            HostileThingTotalRelativeEffectiveCombatPower(perceivedThreats, pawn) :
                            HostileThingTotalRelativeBodySize(perceivedThreats, pawn);

                        float severityGain = BaseSeverityGainPerHour / GenDate.TicksPerHour *
                            extraRaceProps.adrenalineGainFactor *
                            TotalRelativeScoreToAdrenalineGainFactorCurve.Evaluate(relativeScore) *
                            GenTicks.TicksPerRealSecond;

                        HealthUtility.AdjustSeverity(pawn, hediff, severityGain);
                        adrenalineComp.lastAdrenalineGainTick = Find.TickManager.TicksGame;
                        return;
                    }
                }

                // Otherwise reduce severity if it has been at least 600 ticks since the pawn was last harmed
                if (Find.TickManager.TicksGame >= adrenalineComp.lastAdrenalineGainTick + MinTicksSinceSeverityGainForSeverityLoss)
                {
                    float severityLoss = BaseSeverityLossPerHour / GenDate.TicksPerHour * extraRaceProps.adrenalineLossFactor * GenTicks.TicksPerRealSecond;
                    HealthUtility.AdjustSeverity(pawn, hediff, -severityLoss);
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

                float severityToAdd = BaseSeverityGainPerDamageTaken * extraRaceProps.adrenalineGainFactor * injury.Severity / pawn.HealthScale;
                HealthUtility.AdjustSeverity(pawn, this.hediff, severityToAdd);
                pawn.GetComp<CompAdrenalineTracker>().lastAdrenalineGainTick = Find.TickManager.TicksGame;
                return true;
            }

            return false;
        }

    }

}
