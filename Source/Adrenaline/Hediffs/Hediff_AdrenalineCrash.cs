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

    public class Hediff_AdrenalineCrash : Hediff_Adrenaline
    {

        #region Fields
        protected bool hediffWasNull;
        protected float totalSeverityGained;
        protected float totalAttainableSeverity;
        protected int ticksAtPeakSeverity;
        #endregion

        #region Properties
        public AdrenalineCrashProperties Props => def.GetModExtension<HediffDefExtension>().adrenalineCrash;
        protected Hediff AdrenalineRushHediff => pawn.health.hediffSet.GetFirstHediffOfDef(ExtraRaceProps.adrenalineRushHediff);
        protected virtual float SeverityGainFactor => totalAttainableSeverity < 1 ? Mathf.Sqrt(totalAttainableSeverity) : totalAttainableSeverity;
        protected virtual float AttainableSeverity => totalAttainableSeverity - totalSeverityGained;

        public override bool ShouldRemove => base.ShouldRemove && AttainableSeverity <= 0;
        #endregion

        public override void Reset()
        {
            ticksAtPeakSeverity = 0;
        }

        protected override void UpdateSeverity()
        {
            // Increase the attainable severity based on the adrenaline rush's severity
            if (AdrenalineRushHediff != null)
            {
                if (hediffWasNull)
                {
                    Reset();
                    hediffWasNull = false;
                }
                totalAttainableSeverity += AdrenalineRushHediff.Severity * Props.attainableSeverityPerAdrenalineRushHediffSeverityPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks; // Factor in time 
            }
            else
                hediffWasNull = true;

            // Increase severity if total severity gained is less than the attainable severity
            if (totalSeverityGained < totalAttainableSeverity)
            {
                float severityToGain = Mathf.Min(AttainableSeverity,
                    Props.baseSeverityGainPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * // Baseline
                    SeverityGainFactor); // From cumulative adrenaline severity

                Severity += severityToGain;
                totalSeverityGained += severityToGain;
            }

            // Otherwise if it's been a certain amount of time since peak severity was attained (depending on peak severity), drop severity
            else
            {
                if (ticksAtPeakSeverity >= (int)(Props.baseTicksAtPeakSeverityBeforeSeverityLoss * Severity))
                    Severity -= Props.baseSeverityLossPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks;

                else
                    ticksAtPeakSeverity += SeverityUpdateIntervalTicks;
            }
        }

        public override string DebugString()
        {
            var debugBuilder = new StringBuilder();
            debugBuilder.AppendLine($"attainable severity: {AttainableSeverity}".Indented("    "));
            debugBuilder.AppendLine($"ticks at peak severity: {ticksAtPeakSeverity}".Indented("    "));
            debugBuilder.AppendLine(base.DebugString());
            return debugBuilder.ToString();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hediffWasNull, "hediffWasNull");
            Scribe_Values.Look(ref totalSeverityGained, "totalSeverityGained");
            Scribe_Values.Look(ref totalAttainableSeverity, "totalAttainableSeverity");
            Scribe_Values.Look(ref ticksAtPeakSeverity, "ticksAtPeakSeverity");

            base.ExposeData();
        }

    }

}
