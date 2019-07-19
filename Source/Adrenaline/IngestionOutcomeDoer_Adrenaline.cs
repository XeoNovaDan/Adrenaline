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
            Hediff hediff = HediffMaker.MakeHediff(this.hediffDef, pawn, null);
            float num;
            if (this.severity > 0f)
            {
                num = this.severity;
            }
            else
            {
                num = this.hediffDef.initialSeverity;
            }

            // Multiply by the pawn's adrenaline resistance
            var extraRaceProps = pawn.def.GetModExtension<ExtraRaceProperties>() ?? ExtraRaceProperties.defaultValues;
            num *= extraRaceProps.adrenalineGainFactorArtificial;

            hediff.Severity = num;
            pawn.health.AddHediff(hediff, null, null, null);
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

        public float severity = -1f;

    }
    // Okay, you got me... :(

}
