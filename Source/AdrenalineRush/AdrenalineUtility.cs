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
            // Not spawned, fogged or too far away to be perceived as a threat
            if (!t.Spawned || t.Position.Fogged(t.Map) || t.Position.DistanceTo(pawn.Position) > MaxPerceivedThreatDistance)
                return false;

            // Pawn
            if (t is Pawn p)
            {
                return !p.Downed && p.HostileTo(pawn);
            }

            // Turret (if pawn's humanlike)
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

    }

}
