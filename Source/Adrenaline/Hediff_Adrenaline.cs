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

        protected float gainableSeverity;
        protected float cachedGainableSeverity; // Exists to determine the rate at which severity increases
        protected int ticksSinceLastSeverityGain;

        protected float GainableSeverity
        {
            get => gainableSeverity;
            set
            {
                gainableSeverity = value;
                cachedGainableSeverity = value;
            }
        }

        protected ExtendedRaceProperties ExtraRaceProps => pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

        protected virtual bool CanGainSeverity => GainableSeverity > 0;

        protected virtual bool CanLoseSeverity => true;

        protected void GainSeverityFromTick(float sevOffset)
        {
            Severity += sevOffset;
            GainableSeverity -= sevOffset;
        }

        protected virtual void UpdateSeverity()
        {
            if (CanGainSeverity)
                ticksSinceLastSeverityGain = 0;
            else
                ticksSinceLastSeverityGain += SeverityUpdateIntervalTicks;
        }

        public override void Tick()
        {
            if (ageTicks % SeverityUpdateIntervalTicks == 0)
                UpdateSeverity();

            base.Tick();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref gainableSeverity, "gainableSeverity");
            Scribe_Values.Look(ref cachedGainableSeverity, "cachedGainableSeverity");
            Scribe_Values.Look(ref ticksSinceLastSeverityGain, "ticksSinceLastSeverityGain");

            base.ExposeData();
        }

    }

}
