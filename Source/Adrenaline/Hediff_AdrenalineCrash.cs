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

        private const float BaseSeverityGainPerDay = 4;
        private const float BaseSeverityLossPerDay = 2.4f;
        private const float SeverityGainFactorPerCachedGainableSeverity = 1;

        private Hediff AdrenalineRush => pawn.health.hediffSet.GetFirstHediffOfDef(ExtraRaceProps.adrenalineRushHediff);

        protected override bool CanGainSeverity => base.CanGainSeverity;

        protected override void UpdateSeverity()
        {
            Log.Message($"Adrenaline crash gainable severity for {pawn} = {cachedGainableSeverity}", true);

            // Update gainable severity
            if (AdrenalineRush != null)
            {
                GainableSeverity += AdrenalineRush.Severity / 100;
            }

            if (CanGainSeverity)
            {
                float severityToGain =
                    BaseSeverityGainPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks * // Basline
                    (cachedGainableSeverity * SeverityGainFactorPerCachedGainableSeverity); // From cached gainable severity

                GainSeverityFromTick(severityToGain);
            }

            else
            {
                Severity -= BaseSeverityLossPerDay / GenDate.TicksPerDay * SeverityUpdateIntervalTicks;
            }

            base.UpdateSeverity();
        }

        public override void ExposeData()
        {


            base.ExposeData();
        }

    }

}
