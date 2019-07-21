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
                // If the trait in question is adrenaline-related and the pawn can't gain adrenaline, reject it
                if ((trait.def == A_TraitDefOf.AdrenalineJunkie || trait.def == A_TraitDefOf.CoolHeaded) && !___pawn.CanGetAdrenaline())
                    return false;

                return true;
            }

        }

    }

}
