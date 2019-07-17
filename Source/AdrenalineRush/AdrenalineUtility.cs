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

    }

}
