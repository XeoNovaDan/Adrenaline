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

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
        public static class Patch_GetGizmos
        {

            public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
            {
                // If the pawn is downed, is a colonist and can take adrenaline, add a 'take adrenaline' gizmo
                if (__instance.Downed && __instance.IsColonistPlayerControlled && __instance.CanGetAdrenaline())
                {
                    var extraRaceProps = __instance.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;
                    foreach (var tDef in extraRaceProps.RelevantConsumablesDowned)
                    {
                        var thingDefExtension = tDef.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;

                        bool anyNearbyAdrenaline = AdrenalineUtility.AnyNearbyAdrenaline(__instance, tDef, out List<Thing> adrenalineThings);
                        var adrenalineGizmo = new Command_Action()
                        {
                            defaultLabel = thingDefExtension.downedIngestGizmoLabel,
                            defaultDesc = thingDefExtension.downedIngestGizmoDescription,
                            action = () => adrenalineThings.First().Ingested(__instance, 0)
                        };
                        if (AdrenalineUtility.adrenalineGizmoIcons.TryGetValue(tDef, out Texture2D icon))
                            adrenalineGizmo.icon = icon;

                        // No adrenaline nearby
                        if (!anyNearbyAdrenaline)
                            adrenalineGizmo.Disable(thingDefExtension.downedIngestGizmoNoneNearby);

                        // Can't do manipulation
                        else if (!__instance.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                            adrenalineGizmo.Disable("Adrenaline.Command_TakeAdrenaline_FailNoManipulation".Translate(__instance.LabelShort));

                        __result = __result.Add(adrenalineGizmo);

                    }
                }
            }

        }

    }

}
