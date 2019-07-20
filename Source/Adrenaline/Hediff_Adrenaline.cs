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

    public class Hediff_Adrenaline : HediffWithComps
    {

        private const float BaseSeverityLossPerHour = 0.3f;

        private const int MinTicksSinceSeverityGainForSeverityLoss = 600;

        public int lastSeverityGainTick;

        public override float Severity
        {
            get => base.Severity;
            set
            {
                // Compare the old severity to new severity and if the new severity is higher, update field that stores when the severity last increased
                float prevSeverity = base.Severity;
                base.Severity = value;
                if (base.Severity > prevSeverity)
                    lastSeverityGainTick = Find.TickManager.TicksGame;
            }
        }

        public override void Tick()
        {
            base.Tick();

            // Update every 60 ticks
            if (pawn.IsHashIntervalTick(HealthTuning.HediffGiverUpdateInterval))
            {
                // Reduce severity over time if it's been more than 600 ticks since the last time severity was increased
                if (Find.TickManager.TicksGame > lastSeverityGainTick + MinTicksSinceSeverityGainForSeverityLoss)
                {
                    var extraRaceProps = pawn.def.GetModExtension<ExtraRaceProperties>() ?? ExtraRaceProperties.defaultValues;
                    Severity -= BaseSeverityLossPerHour / GenDate.TicksPerHour * extraRaceProps.adrenalineLossFactor * HealthTuning.HediffGiverUpdateInterval;
                }
                    
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastSeverityGainTick, "lastSeverityGainTick");
        }

    }

}
