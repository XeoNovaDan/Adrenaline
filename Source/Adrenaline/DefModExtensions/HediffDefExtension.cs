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

    public class HediffDefExtension : DefModExtension
    {

        public static readonly HediffDefExtension defaultValues = new HediffDefExtension();

        public ExtraHediffStageProperties GetExtraHediffStagePropertiesAt(int index)
        {
            return stages.NullOrEmpty() ? ExtraHediffStageProperties.defaultValues : stages[index];
        }

        private List<ExtraHediffStageProperties> stages;

        public AdrenalineRushProperties adrenalineRush = new AdrenalineRushProperties();

        public AdrenalineCrashProperties adrenalineCrash = new AdrenalineCrashProperties();

    }

}
