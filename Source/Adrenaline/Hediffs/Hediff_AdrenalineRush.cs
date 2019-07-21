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
        private const int BaseSeverityLossDelayTicks = 300;
        private const float BaseAttainableSeverityPerPeakTotalThreatSignificance = 0.65f;
        private const float BaseSeverityGainPerHour = 1.2f;
        private const float BaseSeverityLossPerHour = 1.7f;
        #endregion

        #region Fields
        private float totalThreatSignificance; // Determines severity gain/loss rate
        private float peakTotalThreatSignificance; // Determines attainable severity
        private float totalSeverityGained;
        public int ticksUntilSeverityLoss;
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

        private float SeverityGainFactor => Mathf.Max(0.2f, Mathf.Sqrt(totalThreatSignificance));

        private float SeverityLossFactor => 1 / Mathf.Max(1, Mathf.Sqrt(totalThreatSignificance));
        #endregion

        private void Reset()
        {
            peakTotalThreatSignificance = 0;
            totalSeverityGained = Severity;
        }

        public override void PostMake()
        {
            ticksUntilSeverityLoss = BaseSeverityLossDelayTicks;
            base.PostMake();
        }

        protected override void UpdateSeverity()
        {
            // Gain severity if the pawn can naturally gain adrenaline, total severity gained is less than the attainable severity and any threats are present
            if (ExtraRaceProps.adrenalineGainFactorNatural > 0 && totalThreatSignificance > 0 && totalSeverityGained < TotalAttainableSeverity)
            {
                if (ticksUntilSeverityLoss < BaseSeverityLossDelayTicks)
                    ticksUntilSeverityLoss = BaseSeverityLossDelayTicks;

                float severityToGain = Mathf.Min(AttainableSeverity, 
                    BaseSeverityGainPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks * // Baseline
                    ExtraRaceProps.adrenalineGainFactorNatural * // From extra race properties
                    SeverityGainFactor); // From threats

                Severity += severityToGain;
                totalSeverityGained += severityToGain;
            }

            // Otherwise drop severity if appropriate
            else
            {
                if (ticksUntilSeverityLoss <= 0)
                {
                    float severityToLose = BaseSeverityLossPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks * // Baseline
                        ExtraRaceProps.adrenalineLossFactor * // From extra race properties
                        SeverityLossFactor; // From threats

                    Severity -= severityToLose;
                }

                else
                    ticksUntilSeverityLoss -= SeverityUpdateIntervalTicks;
            }
            
        }

        public override void Tick()
        {
            // Update peak total threat significance
            if (ageTicks % HealthTuning.HediffGiverUpdateInterval == 0)
            {
                float previousTotalThreatSignificance = totalThreatSignificance;
                totalThreatSignificance = AdrenalineUtility.GetPerceivedThreatsFor(pawn).Sum(t => t.PerceivedThreatSignificanceFor(pawn));

                // If severity was dropping, threat significance had hit 0 and there are new threats, reset peakTotalThreatSignificance and set totalSeverityGained to current severity
                if (ticksUntilSeverityLoss == 0 && previousTotalThreatSignificance == 0 && totalThreatSignificance > previousTotalThreatSignificance)
                    Reset();

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
