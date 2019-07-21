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

        private const float BaseSeverityGainPerDay = 6;
        private const float BaseSeverityLossPerDay = 2.4f;
        private const float GainableSeverityPerRushSeverityPerHour = 0.6f;
        private const int TicksAtPeakSeverityBeforeSeverityLoss = 6 * GenDate.TicksPerHour;

        private Hediff AdrenalineRush => pawn.health.hediffSet.GetFirstHediffOfDef(ExtraRaceProps.adrenalineRushHediff);

        protected override bool CanGainSeverity => base.CanGainSeverity && (AdrenalineRush == null || AdrenalineRush.ageTicks > GenDate.TicksPerHour * 2);

        protected override bool CanLoseSeverity => !CanGainSeverity && ticksSinceLastSeverityGain >= TicksAtPeakSeverityBeforeSeverityLoss;

        public override bool ShouldRemove => base.ShouldRemove && GainableSeverity == 0;

        protected override void UpdateSeverity()
        {
            // Update gainable severity
            if (AdrenalineRush != null)
            {
                GainableSeverity += GainableSeverityPerRushSeverityPerHour / GenDate.TicksPerHour * SeverityUpdateIntervalTicks * AdrenalineRush.Severity;
            }

            if (CanGainSeverity)
            {
                float severityToGain =
                    BaseSeverityGainPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * // Baseline
                    Mathf.Sqrt(cachedGainableSeverity); // From cached gainable severity

                GainSeverityFromTick(severityToGain);
            }

            else if (CanLoseSeverity)
            {
                Severity -= BaseSeverityLossPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks;
            }

            base.UpdateSeverity();
        }

    }

}
