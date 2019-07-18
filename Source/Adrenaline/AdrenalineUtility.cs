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

        private const float MaxPerceivedThreatDistance = 50;

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

        public static bool IsPerceivedThreatBy(this Thing t, Pawn pawn)
        {
            // Not spawned or no line of sight
            //if (!t.Spawned || t.Position.Fogged(t.Map) || t.Position.DistanceTo(pawn.Position) > MaxPerceivedThreatDistance)
            if (!t.Spawned || !GenSight.LineOfSight(pawn.Position, t.Position, t.Map, validator: (c) => pawn.Position.DistanceTo(c) <= MaxPerceivedThreatDistance))
                return false;

            // Pawn
            if (t is Pawn p)
            {
                return !p.Downed && (p.HostileTo(pawn) || pawn.InCombatWith(p) || p.InCombatWith(pawn));
            }

            // Turret (if pawn is humanlike)
            if (pawn.RaceProps.Humanlike)
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

        public static bool InCombatWith(this Pawn pawn, Pawn p) => pawn.IsFighting() && pawn.CurJob.AnyTargetIs(p);

        public static bool IsPotentialPerceivableThreat(this Thing t)
        {
            return t is Building_Turret;
        }

        public static float EffectiveCombatPower(this Thing t)
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

            throw new NotImplementedException($"Unaccounted effective combat power calculation for {t} (Type={t.GetType().Name})");

        }

        public static bool CanGetAdrenaline(this Pawn p) => p.def.CanGetAdrenaline();

        public static bool CanGetAdrenaline(this ThingDef tDef)
        {
            var extraRaceProps = tDef.GetModExtension<ExtraRaceProperties>() ?? ExtraRaceProperties.defaultValues;
            return tDef.race != null && extraRaceProps.HasAdrenaline && (tDef.race.hediffGiverSets?.Any(h => h.hediffGivers.Any(g => g.GetType() == typeof(HediffGiver_Adrenaline))) ?? false);
        } 

    }

}
