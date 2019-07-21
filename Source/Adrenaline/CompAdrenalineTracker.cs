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

        private const int UpdateIntervalTicks = 20;
        private const int MaxCumulativeAdrenalineRushSeverity = GenDate.TicksPerHour * 2;
        private const float MinAdrenalineRushSeverityGainFactor = 0.25f;

        private float cumulativeAdrenalineRushSeverity;

        private Pawn Pawn => (Pawn)parent;

        private ExtendedRaceProperties ExtraRaceProps => parent.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

        public float CumulativeAdrenalineRushSeverity
        {
            get => cumulativeAdrenalineRushSeverity;
            set
            {
                cumulativeAdrenalineRushSeverity = Mathf.Clamp(value, 0, MaxCumulativeAdrenalineRushSeverity);
            }
        }

        public float AdrenalineRushSeverityGainFactor => Mathf.Lerp(MinAdrenalineRushSeverityGainFactor, 1, CumulativeAdrenalineRushSeverity / MaxCumulativeAdrenalineRushSeverity);

        public bool CanGainAdrenaline => CumulativeAdrenalineRushSeverity < MaxCumulativeAdrenalineRushSeverity;

        public override void CompTick()
        {
            // If the pawn doesn't have an adrenaline rush, reduce the cumulative adrenaline rush severity
            if (parent.IsHashIntervalTick(UpdateIntervalTicks) && !Pawn.health.hediffSet.HasHediff(ExtraRaceProps.adrenalineRushHediff))
                CumulativeAdrenalineRushSeverity -= UpdateIntervalTicks;

            base.CompTick();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref cumulativeAdrenalineRushSeverity, "cumulativeAdrenalineRushSeverity");

            base.PostExposeData();
        }


    }

}
