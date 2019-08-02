using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace Adrenaline
{

    public static class Patch_Pawn_MindState
    {

        [HarmonyPatch(typeof(Pawn_MindState), nameof(Pawn_MindState.MindStateTick))]
        public static class Patch_MindStateTick
        {

            public static void Postfix(Pawn_MindState __instance)
            {
                var pawn = __instance.pawn;
                // Try to inject nearby adrenaline items if the pawn is downed, not a player pawn, is at least a tooluser, is capable of manipulation and moving, and can gain an adrelaine hediff
                if (pawn.IsHashIntervalTick(60) && pawn.Downed && pawn.Faction != Faction.OfPlayer && pawn.RaceProps.intelligence >= Intelligence.ToolUser &&
                    pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                {
                    var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;
                    if (extraRaceProps.adrenalineRushHediff != null)
                    {
                        var adrenalineHediff = pawn.health.hediffSet.GetFirstHediffOfDef(extraRaceProps.adrenalineRushHediff);
                        if ((adrenalineHediff == null || adrenalineHediff.CurStageIndex < adrenalineHediff.def.stages.Count - 1) &&
                            AdrenalineUtility.AnyNearbyAdrenaline(pawn, extraRaceProps.RelevantConsumablesDowned, out List<Thing> adrenalineThings))
                        {
                            adrenalineThings.First().Ingested(pawn, 0);
                        }
                    }
                }
            }

        }

    }

}
