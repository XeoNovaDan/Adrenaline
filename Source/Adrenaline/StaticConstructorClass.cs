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

            // Add CompAdrenalineTracker to all eligible defs
            foreach (var tDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (tDef.CanGetAdrenaline())
                {
                    if (tDef.comps == null)
                        tDef.comps = new List<CompProperties>();
                    tDef.comps.Add(new CompProperties(typeof(CompAdrenalineTracker)));
                }
            }

        }

    }

}
