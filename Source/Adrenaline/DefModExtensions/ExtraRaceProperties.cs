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

    public class ExtendedRaceProperties : DefModExtension
    {

        public static readonly ExtendedRaceProperties defaultValues = new ExtendedRaceProperties();
        public HediffDef adrenalineRushHediff = A_HediffDefOf.Adrenaline;
        public HediffDef adrenalineCrashHediff = A_HediffDefOf.AdrenalineCrash;
        public float adrenalineGainFactorNatural = 1;
        public float adrenalineGainFactorArtificial = 1;
        public float adrenalineLossFactor = 1;

        [Unsaved]
        private List<ThingDef> _relevantConsumables;
        [Unsaved]
        private List<ThingDef> _relevantConsumablesDowned;

        public bool HasAdrenaline => adrenalineRushHediff != null && (adrenalineGainFactorNatural > 0 || adrenalineGainFactorArtificial > 0);

        public List<ThingDef> RelevantConsumables
        {
            get
            {
                if (_relevantConsumables == null)
                    _relevantConsumables = DefDatabase<ThingDef>.AllDefs.Where(t => t.IsDrug && t.ingestible.outcomeDoers.Any(o => o is IngestionOutcomeDoer_Adrenaline adrenalineOutcome && adrenalineOutcome.hediffDef == adrenalineRushHediff)).ToList();
                return _relevantConsumables;
            }
        }

        public List<ThingDef> RelevantConsumablesDowned
        {
            get
            {
                if (_relevantConsumablesDowned == null)
                    _relevantConsumablesDowned = RelevantConsumables.Where(t => (t.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues).ingestibleWhenDowned).ToList();
                return _relevantConsumablesDowned;
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            // Has no adrenaline rush hediff but has adrenaline crash hediff
            if (adrenalineRushHediff == null && adrenalineCrashHediff != null)
                yield return $"Has null adrenalineRushHediff but has {adrenalineCrashHediff} adrenalineCrashHediff";
        }


    }

}
