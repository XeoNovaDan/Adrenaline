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

    public class CompAdrenalineTracker : ThingComp
    {

        public int lastAdrenalineGainTick;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref lastAdrenalineGainTick, "lastAdrenalineGainTick");
            base.PostExposeData();
        }


    }

}
