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

    public class CompProperties_AdrenalineTracker : CompProperties
    {

        public CompProperties_AdrenalineTracker()
        {
            compClass = typeof(CompAdrenalineTracker);
        }

        public float adrenalineProductionCapacity = 10000;
        public float adrenalineProductionRecoveryPerDay = 6400;

    }

}
