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

    public class HediffGiver_Adrenaline : HediffGiver
    {

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;

            if (extraRaceProps.HasAdrenaline)
            {
                var adrenalineTracker = pawn.GetComp<CompAdrenalineTracker>();
                bool hasRush = pawn.health.hediffSet.HasHediff(extraRaceProps.adrenalineRushHediff);

                // If the pawn can produce adrenaline and doesn't already have an adrenaline rush, add adrenaline rush
                if (AdrenalineSettings.allowNaturalGain && (AdrenalineSettings.affectAnimals || !pawn.RaceProps.Animal) && adrenalineTracker.CanProduceAdrenaline && !hasRush && AdrenalineUtility.GetPerceivedThreatsFor(pawn).Any())
                {
                    TryTeachAdrenalineConcept(pawn);
                    pawn.health.AddHediff(extraRaceProps.adrenalineRushHediff);
                }   

                // Otherwise if they have an adrenaline rush and don't have an adrenaline crash hediff, add an adrenaline crash hediff
                else if (AdrenalineSettings.adrenalineCrashes && hasRush && extraRaceProps.adrenalineCrashHediff != null && !pawn.health.hediffSet.HasHediff(extraRaceProps.adrenalineCrashHediff))
                {
                    var crashHediff = (Hediff_AdrenalineCrash)pawn.health.AddHediff(extraRaceProps.adrenalineCrashHediff);
                    crashHediff.ticksToSeverityGain = crashHediff.Props.severityGainDelay;
                }
                   

            }
        }

        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            if (AdrenalineSettings.allowNaturalGain)
            {
                var extraRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>() ?? ExtendedRaceProperties.defaultValues;
                if (extraRaceProps.HasAdrenaline)
                {
                    var adrenalineTracker = pawn.GetComp<CompAdrenalineTracker>();
                    if (adrenalineTracker.CanProduceAdrenaline)
                    {
                        // Hediff isn't an injury, is a scar or the pawn is dead
                        var injury = hediff as Hediff_Injury;
                        if (injury == null || injury.IsPermanent() || pawn.Dead)
                            return false;

                        // Try to add target severity based on the pain caused by the injury
                        float painFromInjury = injury.PainOffset / pawn.HealthScale * pawn.TotalPainFactor();
                        if (painFromInjury > 0)
                        {
                            TryTeachAdrenalineConcept(pawn);
                            var rushhediff = (Hediff_AdrenalineRush)(pawn.health.hediffSet.GetFirstHediffOfDef(extraRaceProps.adrenalineRushHediff) ?? pawn.health.AddHediff(extraRaceProps.adrenalineRushHediff));
                            rushhediff.recentPainFelt += painFromInjury;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void TryTeachAdrenalineConcept(Pawn pawn)
        {
            if (pawn.Faction == Faction.OfPlayer && !PlayerKnowledgeDatabase.IsComplete(A_ConceptDefOf.Adrenaline))
                LessonAutoActivator.TeachOpportunity(A_ConceptDefOf.Adrenaline, OpportunityType.Important);
        }

    }

}
