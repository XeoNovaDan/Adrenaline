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
        protected float _targetSeverityUnclamped;
        protected int ticksAtTargetSeverity;
        #endregion

        #region Properties
        public AdrenalineCrashProperties Props => def.GetModExtension<HediffDefExtension>().adrenalineCrash;
        protected Hediff AdrenalineRushHediff => pawn.health.hediffSet.GetFirstHediffOfDef(ExtraRaceProps.adrenalineRushHediff);
        protected virtual float TargetSeverityUnclamped
        {
            get => Mathf.Max(_targetSeverityUnclamped, 0);
            set => _targetSeverityUnclamped = value;
        }
        protected virtual float SeverityGainFactor => TargetSeverityUnclamped < 1 ? Mathf.Sqrt(TargetSeverityUnclamped) : TargetSeverityUnclamped;
        protected virtual float TargetSeverity => Mathf.Min(TargetSeverityUnclamped, def.maxSeverity);

        public override bool ShouldRemove => base.ShouldRemove && TargetSeverity == 0;
        #endregion

        protected override void UpdateSeverity()
        {
            // Increase the target severity based on the adrenaline rush's severity
            if (AdrenalineRushHediff != null)
                TargetSeverityUnclamped += AdrenalineRushHediff.Severity * Props.attainableSeverityPerAdrenalineRushHediffSeverityPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks; // Factor in time 

            // Increase severity if total severity gained is below target severity
            if (Severity < TargetSeverity)
                Severity += Mathf.Min(TargetSeverity - Severity, Props.baseSeverityGainPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * SeverityGainFactor);

            // Otherwise if it's been a certain amount of time since target severity was hit, drop severity
            else
            {
                if (ticksAtTargetSeverity >= (int)(Props.baseTicksAtPeakSeverityBeforeSeverityLoss * Severity))
                {
                    ticksAtTargetSeverity -= Mathf.Min(ticksAtTargetSeverity, SeverityUpdateIntervalTicks);
                    Severity -= Props.baseSeverityLossPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks;
                    TargetSeverityUnclamped = Severity;
                }

                else
                    ticksAtTargetSeverity += SeverityUpdateIntervalTicks;
            }
        }

        public override string DebugString()
        {
            var debugBuilder = new StringBuilder();
            debugBuilder.AppendLine($"unclamped target severity: {TargetSeverityUnclamped}".Indented());
            debugBuilder.AppendLine($"ticks at target severity: {ticksAtTargetSeverity}".Indented());
            debugBuilder.AppendLine(base.DebugString());
            return debugBuilder.ToString();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _targetSeverityUnclamped, "targetSeverityUnclamped");
            Scribe_Values.Look(ref ticksAtTargetSeverity, "ticksAtTargetSeverity");

            base.ExposeData();
        }

    }

}
