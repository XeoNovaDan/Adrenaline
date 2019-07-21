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

        #region Constants
        private const int SeverityLossDelayTicks = 300;
        private const float BaseAttainableSeverityPerPeakTotalThreatSignificance = 0.75f;
        private const float BaseSeverityGainPerHour = 0.8f;
        private const float BaseSeverityLossPerHour = 1.6f;
        #endregion

        #region Fields
        private float totalThreatSignificance; // Determines severity gain/loss rate
        private float peakTotalThreatSignificance; // Determines attainable severity
        private float totalSeverityGained;
        private int ticksSinceLastSeverityGain;
        #endregion

        #region Properties
        private float TotalAttainableSeverity
        {
            get
            {
                float baseAttainableSeverity = peakTotalThreatSignificance * BaseAttainableSeverityPerPeakTotalThreatSignificance;
                if (baseAttainableSeverity <= 1)
                    return baseAttainableSeverity;

                // If the base attainable severity is more than 1, return the square root to prevent extreme values
                return Mathf.Sqrt(baseAttainableSeverity);
            }
        }
        private float AttainableSeverity => TotalAttainableSeverity - totalSeverityGained;

        private float SeverityAdjustCoefficient => Mathf.Max(0.2f, Mathf.Sqrt(totalThreatSignificance));
        #endregion

        protected override void UpdateSeverity()
        {
            // Gain severity if total severity gained is less than the attainable severity and any threats are present
            if (totalSeverityGained < AttainableSeverity && totalThreatSignificance > 0)
            {
                float severityToGain = Mathf.Min(AttainableSeverity, 
                    BaseSeverityGainPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks * // Baseline
                    ExtraRaceProps.adrenalineGainFactorNatural * // From extra race properties
                    SeverityAdjustCoefficient); // From threats

                Severity += severityToGain;
                totalSeverityGained += severityToGain;
                ticksSinceLastSeverityGain = 0;
            }

            // Otherwise drop severity if it has been at least 300 ticks since the last gain
            else
            {
                if (ticksSinceLastSeverityGain > SeverityLossDelayTicks)
                {
                    float severityToLose = BaseSeverityLossPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks * // Baseline
                        ExtraRaceProps.adrenalineLossFactor / // From extra race properties
                        SeverityAdjustCoefficient; // From threats

                    Severity -= severityToLose;
                }

                else
                    ticksSinceLastSeverityGain += SeverityUpdateIntervalTicks;
            }
            
        }

        public override void Tick()
        {
            // Update peak total threat significance
            if (ageTicks % HealthTuning.HediffGiverUpdateInterval == 0)
            {
                totalThreatSignificance = AdrenalineUtility.GetPerceivedThreatsFor(pawn).Sum(t => t.PerceivedThreatSignificanceFor(pawn));
                if (totalThreatSignificance > peakTotalThreatSignificance)
                    peakTotalThreatSignificance = totalThreatSignificance;
            }

            base.Tick();
        }

        public override string DebugString()
        {
            var debugBuilder = new StringBuilder();
            debugBuilder.AppendLine($"total severity gained: {totalSeverityGained}".Indented("    "));
            debugBuilder.AppendLine($"attainable severity: {AttainableSeverity}".Indented("    "));
            debugBuilder.AppendLine($"severity adjust coefficient: {SeverityAdjustCoefficient}".Indented("    "));
            debugBuilder.AppendLine(base.DebugString());
            return debugBuilder.ToString();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref totalThreatSignificance, "totalThreatSignificance");
            Scribe_Values.Look(ref peakTotalThreatSignificance, "peakTotalThreatSignificance");
            Scribe_Values.Look(ref totalSeverityGained, "totalSeverityGained");
            Scribe_Values.Look(ref ticksSinceLastSeverityGain, "ticksSinceLastSeverityGain");

            base.ExposeData();
        }

    }

}
