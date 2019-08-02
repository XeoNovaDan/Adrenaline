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

    public class IngestionOutcomeDoer_Adrenaline : IngestionOutcomeDoer
    {

        // I definitely, definitely did not copy and paste a decompiled IngestionOutcomeDoer_GiveHediff and adapt it. Why would I ever do that?
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            // Check if the ingesting pawn can actually get adrenaline
            if (pawn.CanGetAdrenaline())
            {
                // Improperly configured properties
                if (hediffDef.hediffClass != typeof(Hediff_AdrenalineRush))
                {
                    Log.Error($"hediffDef for {ingested.def} does not have a hediffClass of Adrenaline.Hediff_AdrenalineRush");
                    return;
                }

                // Do nothing if the pawn can't benefit from the ingested thing
                var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;
                if (!extraRaceProps.RelevantConsumables.Contains(ingested.def))
                    return;

                // Determine severity gain
                float severityGain = (severity > 0) ? severity : hediffDef.initialSeverity;

                if (divideByBodySize)
                    severityGain /= pawn.BodySize;

                severityGain *= extraRaceProps.adrenalineGainFactorArtificial;

                // Add severity and increase the duration of the hediff
                HealthUtility.AdjustSeverity(pawn, hediffDef, severityGain);
                var adrenalineHediff = (Hediff_AdrenalineRush)pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                adrenalineHediff.severityLossDelayTicks += adrenalineHediffDurationOffset;
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (parentDef.IsDrug && this.chance >= 1f)
            {
                foreach (StatDrawEntry s in this.hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
                {
                    yield return s;
                }
            }
            yield break;
        }

        public HediffDef hediffDef;

        private float severity = -1;

        private bool divideByBodySize;

        private int adrenalineHediffDurationOffset;

    }
    // Okay, you got me... :(

}
