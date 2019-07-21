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

    public abstract class Hediff_Adrenaline : HediffWithComps
    {

        protected const int SeverityUpdateIntervalTicks = 20;

        protected ExtendedRaceProperties ExtraRaceProps => pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

        protected abstract void UpdateSeverity();

        public override void Tick()
        {
            if (ageTicks % SeverityUpdateIntervalTicks == 0)
                UpdateSeverity();

            base.Tick();
        }

    }

}
