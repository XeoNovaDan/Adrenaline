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

    public class Hediff_AdrenalineRush : Hediff_Adrenaline
    {

        private const float BaseSeverityGainPerHour = 0.2f;
        private const float BaseSeverityLossPerHour = 0.5f;
        private const float GainableSeverityPerThreatSignificance = 0.3f;
        private const float SeverityGainFactorPerCachedGainableSeverity = 5;
        private const int ThreatSignificanceUpdateIntervalTicks = HealthTuning.HediffGiverUpdateInterval;
        private const int MaxTicksWithoutPerceivedThreatsBeforeForcedSeverityLoss = 300;
        private const int MinTicksSinceLastSeverityGainForSeverityLoss = 300;

        private float threatSignificanceProcessedIntoSeverity;
        private float peakTotalThreatSignificance;
        private float peakTotalThreatSignificanceDeltaFromPrevious;
        
        private int ticksWithoutPerceivedThreats;

        private float TotalThreatSignificance => pawn.Map.GetComponent<MapComponent_AdrenalineTracker>().allPotentialHostileThings.Where(t => t.IsPerceivedThreatBy(pawn)).Sum(t => t.PerceivedThreatSignificanceFor(pawn));

        private bool DropAdrenalineFromLackOfThreats => ticksWithoutPerceivedThreats > MaxTicksWithoutPerceivedThreatsBeforeForcedSeverityLoss;

        protected override bool CanGainSeverity => base.CanGainSeverity && !DropAdrenalineFromLackOfThreats;

        protected override bool CanLoseSeverity => DropAdrenalineFromLackOfThreats || ticksSinceLastSeverityGain > MinTicksSinceLastSeverityGainForSeverityLoss;

        protected override void UpdateSeverity()
        {
            if (CanGainSeverity)
            {
                float severityToGain = Mathf.Min(gainableSeverity,
                    BaseSeverityGainPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks * // Baseline
                    ExtraRaceProps.adrenalineGainFactorNatural * // From extra race props
                    Mathf.Sqrt(cachedGainableSeverity) // From cached gainable severity
                    );

                GainSeverityFromTick(severityToGain);
            }
            else if(CanLoseSeverity)
            {
                Severity -= BaseSeverityLossPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks * // Baseline
                    ExtraRaceProps.adrenalineLossFactor; // From extra race props
            }
            base.UpdateSeverity();
        }

        public override void Tick()
        {
            // Update threat significance every 60 ticks since it's computationally expensive
            if (pawn.Map != null && ageTicks % ThreatSignificanceUpdateIntervalTicks == 0)
            {
                // Update peak total threat significance
                float totalThreatSignificance = TotalThreatSignificance; // A bid to reduce performance costs
                if (totalThreatSignificance > peakTotalThreatSignificance)
                {
                    ticksWithoutPerceivedThreats = 0;
                    peakTotalThreatSignificanceDeltaFromPrevious = totalThreatSignificance - peakTotalThreatSignificance;
                    peakTotalThreatSignificance = totalThreatSignificance;

                    GainableSeverity += peakTotalThreatSignificanceDeltaFromPrevious * GainableSeverityPerThreatSignificance;
                }
                else if (totalThreatSignificance == 0)
                {
                    ticksWithoutPerceivedThreats += ThreatSignificanceUpdateIntervalTicks;
                }
            }

            base.Tick();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref threatSignificanceProcessedIntoSeverity, "threatSignificanceProcessedIntoSeverity");
            Scribe_Values.Look(ref peakTotalThreatSignificanceDeltaFromPrevious, "peakTotalThreatSignificanceDeltaFromPrevious");
            Scribe_Values.Look(ref peakTotalThreatSignificance, "peakTotalThreatSignificance");
            Scribe_Values.Look(ref ticksWithoutPerceivedThreats, "ticksWithoutPerceivedThreats");

            base.ExposeData();
        }

    }

}
