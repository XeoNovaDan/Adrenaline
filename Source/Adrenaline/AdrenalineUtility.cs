using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using System.Reflection.Emit;

namespace Adrenaline
{

    public static class AdrenalineUtility
    {

        private const float BasePerceivedThreatDistance = 50;

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

        public static IEnumerable<Thing> GetPerceivedThreatsFor(Pawn pawn)
        {
            if (pawn.Map == null)
                yield break;

            foreach (var threat in pawn.Map.GetComponent<MapComponent_AdrenalineTracker>().allPotentialHostileThings.Where(t => t.IsPerceivedThreatBy(pawn)))
                yield return threat;
        }

        public static bool IsPerceivedThreatBy(this Thing t, Pawn pawn)
        {
            // Not spawned, fogged, too far away from the pawn in question or cannot see them
            if (!t.Spawned || t.Position.Fogged(t.Map) || pawn.Position.DistanceTo(t.Position) > BasePerceivedThreatDistance * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) || !AttackTargetFinder.CanSee(pawn, t))
                return false;

            // Pawn
            if (t is Pawn p)
            {
                return !p.Downed && (p.HostileTo(pawn) || pawn.InCombatWith(p));
            }

            // Turret (if pawn is not an animal)
            if (!pawn.RaceProps.Animal)
            {
                if (t is Building_Turret turret)
                {
                    // Has no power
                    var powerComp = turret.GetComp<CompPowerTrader>();
                    if (powerComp != null && !powerComp.PowerOn)
                        return false;

                    // Unmanned
                    var mannableComp = turret.GetComp<CompMannable>();
                    if (mannableComp != null && !mannableComp.MannedNow)
                        return false;

                    return turret.CurrentEffectiveVerb.Available() && turret.HostileTo(pawn);
                }
            }

            return false;
        }

        public static bool InCombatWith(this Pawn pawn, Pawn p)
        {
            // pawn is actively targeting p
            if (pawn.IsFighting() && pawn.CurJob.AnyTargetIs(p))
                return true;

            // p is actively targeting pawn and has made attacks
            var battle = pawn.records.BattleActive;
            if (p.IsFighting() && p.CurJob.AnyTargetIs(pawn) && battle != null && battle.Concerns(p))
                return true;

            return false;
        }

        public static bool IsPotentialPerceivableThreat(this Thing t)
        {
            return t is Building_Turret;
        }

        public static float PerceivedThreatSignificanceFor(this Thing t, Pawn pawn)
        {
            // If the adrenaline gainee is an animal, only factor in the other thing's body size relative to the animal's body size
            if (pawn.RaceProps.Animal)
            {
                if (t is Pawn p)
                    return (p.BodySize * p.health.summaryHealth.SummaryHealthPercent) / (pawn.BodySize * pawn.health.summaryHealth.SummaryHealthPercent);
                throw new NotImplementedException();
            }

            // Otherwise factor in 'effective combat power'
            else
                return t.EffectiveCombatPower() / pawn.EffectiveCombatPower();
        }

        public static float EffectiveCombatPower(this Thing t)
        {
            // Pawn
            if (t is Pawn p)
            {
                float combatPower;

                // If the pawn is a colonist, return the maximum of the kindDef's combatPower rating or the points per colonist based on the wealth of the player's wealthiest settlement
                if (p.IsColonist)
                {
                    var pawnIncidentTarget = Current.Game.World.worldObjects.Settlements.Where(s => s.HasMap && s.Map.IsPlayerHome).MaxBy(s => s.Map.PlayerWealthForStoryteller).Map;
                    combatPower =  Mathf.Max(PointsPerColonistByWealthCurve.Evaluate(pawnIncidentTarget.PlayerWealthForStoryteller), p.kindDef.combatPower);
                }

                else
                    combatPower = p.kindDef.combatPower;

                return combatPower * p.health.summaryHealth.SummaryHealthPercent * p.ageTracker.CurLifeStage.bodySizeFactor;
            }

            // Turret
            if (t is Building_Turret turret)
            {
                // Return 1/6th of its base market value
                return turret.def.GetStatValueAbstract(StatDefOf.MarketValue, null) / 6;
            }

            throw new NotImplementedException($"Unaccounted effective combat power calculation for {t} (Type={t.GetType().Name})");

        }

        public static bool IsHunting(this Pawn pawn, Pawn prey)
        {
            if (pawn.CurJob == null)
                return false;

            // Humanlike hunting
            var jobDriver_Hunt = pawn.jobs.curDriver as JobDriver_Hunt;
            if (jobDriver_Hunt != null)
                return jobDriver_Hunt.Victim == prey;

            // Predator hunting
            var jobDriver_PredatorHunt = pawn.jobs.curDriver as JobDriver_PredatorHunt;
            return jobDriver_PredatorHunt != null && jobDriver_PredatorHunt.Prey == prey;
        }

        public static bool CanGetAdrenaline(this Pawn p) => p.def.CanGetAdrenaline();

        public static bool CanGetAdrenaline(this ThingDef tDef)
        {
            var extraRaceProps = tDef.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;
            return tDef.race != null && extraRaceProps.HasAdrenaline && (tDef.race.hediffGiverSets?.Any(h => h.hediffGivers.Any(g => g.GetType() == typeof(HediffGiver_Adrenaline))) ?? false);
        }

        public static float TotalPainFactor(this Pawn pawn)
        {
            float factor = 1;
            foreach (var hediff in pawn.health.hediffSet.hediffs)
                factor *= hediff.PainFactor;
            return factor;
        }

    }

}
