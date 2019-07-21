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

        #region Constants
        private const float AttainableSeverityPerAdrenalineRushHediffSeverityPerHour = 0.5f;
        private const float BaseSeverityGainPerDay = 3; // 8 hours baseline for full severity to kick in

        private const int BaseTicksAtPeakSeverityBeforeSeverityLoss = GenDate.TicksPerHour * 12; // Used in determining the delay between last severity gain and severity fall
        private const float BaseSeverityLossPerDay = 0.7f; // Just over 16 hours for effects to fully subside
        #endregion

        #region Fields
        private int severityAdjustDelay;
        private float totalSeverityGained;
        private float totalAttainableSeverity;
        private int ticksAtPeakSeverity;
        #endregion

        #region Properties
        private Hediff AdrenalineRushHediff => pawn.health.hediffSet.GetFirstHediffOfDef(ExtraRaceProps.adrenalineRushHediff);
        private float SeverityGainFactor => totalAttainableSeverity < 1 ? Mathf.Sqrt(totalAttainableSeverity) : totalAttainableSeverity;
        private float AttainableSeverity => totalAttainableSeverity - totalSeverityGained;

        public override bool ShouldRemove => base.ShouldRemove && AttainableSeverity <= 0;
        #endregion

        public override void PostMake()
        {
            severityAdjustDelay = GenMath.RoundRandom(GenDate.TicksPerHour * Rand.Range(0.8f, 1.2f));
            base.PostMake();
        }

        protected override void UpdateSeverity()
        {
            // Increase the attainable severity based on the adrenaline rush's severity
            if (AdrenalineRushHediff != null)
            {
                totalAttainableSeverity += AdrenalineRushHediff.Severity * AttainableSeverityPerAdrenalineRushHediffSeverityPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks; // Factor in time 
            }

            // Only start changing the severity after the hediff is at least one in-game hour old +/- 20%
            if (ageTicks > severityAdjustDelay)
            {
                // Increase severity if total severity gained is less than the attainable severity
                if (totalSeverityGained < totalAttainableSeverity)
                {
                    float severityToGain = Mathf.Min(AttainableSeverity,
                        BaseSeverityGainPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * // Baseline
                        SeverityGainFactor); // From cumulative adrenaline severity

                    Severity += severityToGain;
                    totalSeverityGained += severityToGain;
                }

                // Otherwise if it's been a certain amount of time since peak severity was attained (depending on peak severity), drop severity
                else
                {
                    if (ticksAtPeakSeverity >= (int)(BaseTicksAtPeakSeverityBeforeSeverityLoss * Severity))
                        Severity -= BaseSeverityLossPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks;

                    else
                        ticksAtPeakSeverity += SeverityUpdateIntervalTicks;
                }
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
            Scribe_Values.Look(ref severityAdjustDelay, "severityAdjustDelay");
            Scribe_Values.Look(ref totalSeverityGained, "totalSeverityGained");
            Scribe_Values.Look(ref totalAttainableSeverity, "totalAttainableSeverity");
            Scribe_Values.Look(ref ticksAtPeakSeverity, "ticksAtPeakSeverity");

            base.ExposeData();
        }

    }

}
