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
        public float recentPainFelt;
        public int severityLossDelayTicks;
        #endregion

        #region Properties
        public AdrenalineRushProperties Props => def.GetModExtension<HediffDefExtension>().adrenalineRush;
        protected virtual float TargetSeverity
        {
            get
            {
                float modifiedThreatSignificance = totalThreatSignificance < 1 ? totalThreatSignificance : Mathf.Sqrt(totalThreatSignificance);
                return ((modifiedThreatSignificance * Props.targetSeverityPerTotalThreatSignificance) + (recentPainFelt * Props.targetSeverityPerRecentPainFelt)) * AdrenalineTracker.AdrenalineProductionFactor;
            }
        }

        protected virtual float SeverityGainFactor => Mathf.Sqrt(totalThreatSignificance + recentPainFelt) * AdrenalineTracker.AdrenalineProductionFactor;

        protected virtual float SeverityLossFactor => ((1 / Mathf.Max(1, Mathf.Sqrt(totalThreatSignificance + recentPainFelt))) + Mathf.Max(0, 1 - AdrenalineTracker.AdrenalineProductionFactor)) * ExtraRaceProps.adrenalineLossFactor;
        #endregion

        protected override void UpdateSeverity()
        {

            // Gain severity if it is less than target severity
            if (Severity < TargetSeverity)
                Severity += Mathf.Min(TargetSeverity - Severity, Props.baseSeverityGainPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * SeverityGainFactor);

            // Otherwise drop severity if appropriate
            else if (severityLossDelayTicks <= 0)
                Severity -= Mathf.Min(Severity - TargetSeverity, Props.baseSeverityLossPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * SeverityLossFactor);

            recentPainFelt -= Mathf.Min(recentPainFelt, 1f / GenDate.TicksPerHour * SeverityUpdateIntervalTicks);
            severityLossDelayTicks -= Mathf.Min(severityLossDelayTicks, SeverityUpdateIntervalTicks);

            // Update adrenaline tracker
            AdrenalineTracker.CumulativeAdrenalineRushSeverity += Severity * SeverityUpdateIntervalTicks;
            
        }

        protected virtual void UpdateTotalThreatSignificance()
        {
            totalThreatSignificance = AdrenalineUtility.GetPerceivedThreatsFor(pawn).Sum(t => t.PerceivedThreatSignificanceFor(pawn));
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
            debugBuilder.AppendLine($"target severity: {TargetSeverity}".Indented());
            debugBuilder.AppendLine($"total threat significance: {totalThreatSignificance}".Indented().Indented());
            debugBuilder.AppendLine($"recent pain felt: {recentPainFelt}".Indented().Indented());
            debugBuilder.AppendLine($"severity gain factor: {SeverityGainFactor}".Indented());
            debugBuilder.AppendLine($"severity loss factor: {SeverityLossFactor}".Indented());
            debugBuilder.AppendLine(base.DebugString());
            return debugBuilder.ToString();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref totalThreatSignificance, "totalThreatSignificance");
            Scribe_Values.Look(ref recentPainFelt, "recentPainFelt");
            Scribe_Values.Look(ref severityLossDelayTicks, "severityLossDelayTicks");

            base.ExposeData();
        }

    }

}
