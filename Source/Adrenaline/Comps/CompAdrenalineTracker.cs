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
        private const float ProductionFactorThreshold = 0.2f;

        private float adrenalineProduced;

        private Pawn Pawn => (Pawn)parent;

        private ExtendedRaceProperties ExtraRaceProps => parent.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

        private CompProperties_AdrenalineTracker Props => (CompProperties_AdrenalineTracker)props;

        public float AdrenalineProduced
        {
            get => adrenalineProduced;
            set
            {
                adrenalineProduced = Mathf.Max(value, 0);
            }
        }

        public float AdrenalineProductionFactor
        {
            get
            {
                if (Pawn.Downed && !AdrenalineSettings.affectDownedPawns)
                    return 0;
                return Mathf.Max(1 - AdrenalineProduced / Props.adrenalineProductionCapacity, 0);
            }
        }

        public bool CanProduceAdrenaline => parent.GetStatValue(A_StatDefOf.AdrenalineProduction) > ProductionFactorThreshold;

        public override void CompTick()
        {
            // If the pawn doesn't have an adrenaline rush, reduce the cumulative adrenaline rush severity
            if (parent.IsHashIntervalTick(UpdateIntervalTicks) && !Pawn.health.hediffSet.HasHediff(ExtraRaceProps.adrenalineRushHediff))
                AdrenalineProduced -= Props.adrenalineProductionRecoveryPerDay / GenDate.TicksPerDay * UpdateIntervalTicks;

            base.CompTick();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref adrenalineProduced, "adrenalineProduced");

            base.PostExposeData();
        }


    }

}
