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

    public static class Patch_Pawn
    {

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch(nameof(Pawn.GetGizmos))]
        public static class Patch_GetGizmos
        {

            public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
            {
                // If the pawn is downed, is a colonist and can take adrenaline, add a 'take adrenaline' gizmo
                if (__instance.Downed && __instance.IsColonistPlayerControlled && __instance.CanGetAdrenaline())
                {
                    var adrenalineCells = __instance.CellsAdjacent8WayAndInside().Where(c => c.GetFirstThing(__instance.Map, A_ThingDefOf.Adrenaline) != null);
                    var adrenalineGizmo = new Command_Action()
                    {
                        defaultLabel = "Adrenaline.Command_TakeAdrenaline".Translate(),
                        defaultDesc = "Adrenaline.Command_TakeAdrenaline_Description".Translate(),
                        icon = A_TexCommand.Adrenaline,
                        action = () => adrenalineCells.First().GetFirstThing(__instance.Map, A_ThingDefOf.Adrenaline).Ingested(__instance, 0)
                    };

                    // No adrenaline nearby
                    if (!adrenalineCells.Any())
                        adrenalineGizmo.Disable("Adrenaline.Command_TakeAdrenaline_FailNoAdrenaline".Translate());

                    // Can't do manipulation
                    else if (!__instance.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                        adrenalineGizmo.Disable("Adrenaline.Command_TakeAdrenaline_FailNoManipulation".Translate(__instance.LabelShort));

                    __result = __result.Add(adrenalineGizmo);
                }
            }

        }

    }

}
