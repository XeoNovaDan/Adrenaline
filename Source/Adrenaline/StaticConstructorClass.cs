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

    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {

        static StaticConstructorClass()
        {

            
            foreach (var tDef in DefDatabase<ThingDef>.AllDefs)
            {
                // Add CompAdrenalineTracker to each eligible pawn def that doesn't already have one
                if (tDef.CanGetAdrenaline())
                {
                    if (tDef.comps == null)
                        tDef.comps = new List<CompProperties>();

                    if (!tDef.comps.Any(c => c.GetType() == typeof(CompProperties_AdrenalineTracker)))
                        tDef.comps.Add(new CompProperties_AdrenalineTracker());
                }

                // Populate adrenaline gizmo icons
                else if (tDef.GetModExtension<ThingDefExtension>() is ThingDefExtension thingDefExtension && !thingDefExtension.downedIngestGizmoTexPath.NullOrEmpty())
                    AdrenalineUtility.adrenalineGizmoIcons.Add(tDef, ContentFinder<Texture2D>.Get(thingDefExtension.downedIngestGizmoTexPath));
            }
                

        }

    }

}
