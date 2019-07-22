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
            // List isn't defined so return default
            if (stages.NullOrEmpty())
                return ExtraHediffStageProperties.defaultValues;

            // Look for an index match in the list and return the first match
            foreach (var stageProps in stages)
                if (stageProps.index == index)
                    return stageProps;

            // If there wasn't a match, return default
            return ExtraHediffStageProperties.defaultValues;
        }

        private List<ExtraHediffStageProperties> stages;

    }

}
