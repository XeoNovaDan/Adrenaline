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

        private float HostileThingTotalEffectiveCombatPower(IEnumerable<Thing> hostileThings, Pawn pawn) => hostileThings.Sum(t =>
        {
            // Pawn
            if (t is Pawn p)
            {
                // If the pawn is a colonist, return the maximum of the kindDef's combatPower rating or the points per colonist based on the wealth of the player's wealthiest settlement
                if (p.IsColonist)
                {
                    var pawnIncidentTarget = Current.Game.World.worldObjects.Settlements.Where(s => s.HasMap && s.Map.IsPlayerHome).MaxBy(s => s.Map.PlayerWealthForStoryteller).Map;
                    return Mathf.Max(PointsPerColonistByWealthCurve.Evaluate(pawnIncidentTarget.PlayerWealthForStoryteller), p.kindDef.combatPower);
                }

                return p.kindDef.combatPower;
            }

            // Turret
            if (t is Building_Turret turret)
            {
                // Return 1/10th of its base market value
                return turret.def.GetStatValueAbstract(StatDefOf.MarketValue, null) / 10;
            }

            throw new NotImplementedException();
        });

        private static readonly SimpleCurve PointsPerColonistByWealthCurve = new SimpleCurve // Copy-pasted from StorytellerUtility
        {
            {
                new CurvePoint(0f, 15f),
                true
            },
            {
                new CurvePoint(10000f, 15f),
                true
            },
            {
                new CurvePoint(400000f, 140f),
                true
            },
            {
                new CurvePoint(1000000f, 200f),
                true
            }
        };

        private float HostileThingTotalRelativeBodySize(IEnumerable<Thing> hostileThings, Pawn pawn) => hostileThings.Sum(t =>
        {
            if (t is Pawn p)
            {
                return pawn.BodySize / p.BodySize;
            }
            throw new NotImplementedException();
        });

        private static readonly SimpleCurve TotalCombatPowerToAdrenalineGainFactor = new SimpleCurve()
        {
            new CurvePoint(0, 0),
            new CurvePoint(25, 0.35f),
            new CurvePoint(150, 1),
        };

        private static readonly SimpleCurve TotalRelativeBodySizeToAdrenalineGainFactor = new SimpleCurve()
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
                        TotalCombatPowerToAdrenalineGainFactor.Evaluate(HostileThingTotalEffectiveCombatPower(perceivedThreats, pawn)) :
                        TotalRelativeBodySizeToAdrenalineGainFactor.Evaluate(HostileThingTotalRelativeBodySize(perceivedThreats, pawn));

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
