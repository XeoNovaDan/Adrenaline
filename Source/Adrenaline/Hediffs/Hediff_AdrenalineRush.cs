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

        #region Fields
        protected float totalThreatSignificance; // Determines severity gain/loss rate
        protected float peakTotalThreatSignificance; // Determines attainable severity
        protected float totalSeverityGained;
        public int ticksUntilSeverityLoss;
        #endregion

        #region Properties
        public AdrenalineRushProperties Props => def.GetModExtension<HediffDefExtension>().adrenalineRush;

        protected virtual float TotalAttainableSeverity
        {
            get
            {
                float baseAttainableSeverity = peakTotalThreatSignificance * Props.baseAttainableSeverityGainPerPeakTotalThreatSignificance;
                if (baseAttainableSeverity <= 1)
                    return baseAttainableSeverity;

                // If the base attainable severity is more than 1, return the square root to prevent extreme values
                return Mathf.Sqrt(baseAttainableSeverity);
            }
        }
        protected virtual float AttainableSeverity => TotalAttainableSeverity - totalSeverityGained;

        protected virtual float SeverityGainFactor =>
            Mathf.Sqrt(totalThreatSignificance) * // From threat
            AdrenalineTracker.AdrenalineRushSeverityGainFactor * // From adrenaline tracker
            ExtraRaceProps.adrenalineGainFactorNatural; // From extra race properties

        protected virtual float SeverityLossFactor =>
            1 / Mathf.Max(1, Mathf.Sqrt(totalThreatSignificance)) * // From threat
            ExtraRaceProps.adrenalineLossFactor; // From extra race properties
        #endregion

        public override void Reset()
        {
            peakTotalThreatSignificance = 0;
            totalSeverityGained = Severity;
        }

        public override void PostMake()
        {
            ticksUntilSeverityLoss = Props.baseSeverityLossDelayTicks;
            base.PostMake();
        }

        protected override void UpdateSeverity()
        {
            // Gain severity if there are any threats, any more severity can be gained and the pawn can currently produce adrenaline
            if (totalThreatSignificance > 0 && totalSeverityGained < TotalAttainableSeverity && AdrenalineTracker.CanProduceAdrenaline)
            {
                if (ticksUntilSeverityLoss < Props.baseSeverityLossDelayTicks)
                    ticksUntilSeverityLoss = Props.baseSeverityLossDelayTicks;

                float severityToGain = Mathf.Min(AttainableSeverity, Props.baseSeverityGainPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * SeverityGainFactor);

                Severity += severityToGain;
                totalSeverityGained += severityToGain;
            }

            // Otherwise drop severity if appropriate
            else
            {
                if (ticksUntilSeverityLoss <= 0)
                    Severity -= Props.baseSeverityLossPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * SeverityLossFactor;

                else
                    ticksUntilSeverityLoss -= SeverityUpdateIntervalTicks;
            }

            // Update adrenaline tracker
            AdrenalineTracker.CumulativeAdrenalineRushSeverity += Severity * SeverityUpdateIntervalTicks;
            
        }

        protected virtual void UpdateTotalThreatSignificance()
        {
            float previousTotalThreatSignificance = totalThreatSignificance;
            totalThreatSignificance = AdrenalineUtility.GetPerceivedThreatsFor(pawn).Sum(t => t.PerceivedThreatSignificanceFor(pawn));

            // If severity was dropping, threat significance had hit 0 and there are new threats, reset peakTotalThreatSignificance and set totalSeverityGained to current severity
            if (ticksUntilSeverityLoss == 0 && previousTotalThreatSignificance == 0 && totalThreatSignificance > previousTotalThreatSignificance)
                Reset();

            if (totalThreatSignificance > peakTotalThreatSignificance)
                peakTotalThreatSignificance = totalThreatSignificance;
        }

        public override void Tick()
        {
            // Update peak total threat significance
            if (ageTicks % HealthTuning.HediffGiverUpdateInterval == 0)
                UpdateTotalThreatSignificance();

            base.Tick();
        }

        public override string DebugString()
        {
            var debugBuilder = new StringBuilder();
            debugBuilder.AppendLine($"total severity gained: {totalSeverityGained}".Indented("    "));
            debugBuilder.AppendLine($"attainable severity: {AttainableSeverity}".Indented("    "));
            debugBuilder.AppendLine($"severity gain factor: {SeverityGainFactor}".Indented("    "));
            debugBuilder.AppendLine($"severity loss factor: {SeverityLossFactor}".Indented("    "));
            debugBuilder.AppendLine(base.DebugString());
            return debugBuilder.ToString();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref totalThreatSignificance, "totalThreatSignificance");
            Scribe_Values.Look(ref peakTotalThreatSignificance, "peakTotalThreatSignificance");
            Scribe_Values.Look(ref totalSeverityGained, "totalSeverityGained");
            Scribe_Values.Look(ref ticksUntilSeverityLoss, "ticksUntilSeverityLoss");

            base.ExposeData();
        }

    }

}
