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

    public class AdrenalineRushProperties
    {

        public float targetSeverityPerTotalThreatSignificance;

        public float targetSeverityPerRecentPainFelt;

        public float baseSeverityGainPerDay;

        public float severityGainFactorOffsetPerRecentPainFelt;

        public float baseSeverityLossPerDay;


    }

}
