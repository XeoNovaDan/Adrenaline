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

        private float cumulativeAdrenalineRushSeverity;

        private Pawn Pawn => (Pawn)parent;

        private ExtendedRaceProperties ExtraRaceProps => parent.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

        private CompProperties_AdrenalineTracker Props => (CompProperties_AdrenalineTracker)props;

        public float CumulativeAdrenalineRushSeverity
        {
            get => cumulativeAdrenalineRushSeverity;
            set
            {
                cumulativeAdrenalineRushSeverity = Mathf.Max(value, 0);
            }
        }

        public float AdrenalineProductionFactor => Mathf.Max(ExtraRaceProps.adrenalineGainFactorNatural * (1 - CumulativeAdrenalineRushSeverity / Props.maxCumulativeAdrenalineRushSeverity), 0);

        public bool CanProduceAdrenaline
        {
            get
            {
                // Can't naturally produce adrenaline
                if (ExtraRaceProps.adrenalineGainFactorNatural == 0)
                    return false;

                // Cool-headed
                if (Pawn.story != null && Pawn.story.traits.HasTrait(A_TraitDefOf.CoolHeaded))
                    return false;

                return AdrenalineProductionFactor > 0;
            }
        }

        public override void CompTick()
        {
            // If the pawn doesn't have an adrenaline rush, reduce the cumulative adrenaline rush severity
            if (parent.IsHashIntervalTick(UpdateIntervalTicks) && !Pawn.health.hediffSet.HasHediff(ExtraRaceProps.adrenalineRushHediff))
                CumulativeAdrenalineRushSeverity -= Props.cumulativeAdrenalineRushSeverityFallPerDay / GenDate.TicksPerDay * UpdateIntervalTicks;

            base.CompTick();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref cumulativeAdrenalineRushSeverity, "cumulativeAdrenalineRushSeverity");

            base.PostExposeData();
        }


    }

}
