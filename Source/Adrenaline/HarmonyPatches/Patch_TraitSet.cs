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

    public static class Patch_TraitSet
    {

        [HarmonyPatch(typeof(TraitSet))]
        [HarmonyPatch(nameof(TraitSet.GainTrait))]
        public static class Patch_GainTrait
        {

            public static bool Prefix(TraitSet __instance, Pawn ___pawn, Trait trait)
            {
                // If the trait in question is Adrenaline junkie and the none of the pawn's hediff givers include adrenaline, reject it
                if (trait.def == A_TraitDefOf.AdrenalineJunkie && !___pawn.RaceProps.hediffGiverSets.Any(h => h.hediffGivers.Any(g => g.GetType() == typeof(HediffGiver_Adrenaline))))
                    return false;

                return true;
            }

        }

    }

}
