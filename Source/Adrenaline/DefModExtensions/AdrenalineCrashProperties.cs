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

    public class AdrenalineCrashProperties
    {

        public float targetSeverityGainPerAdrenalineRushHediffSeverityPerHour;

        public float baseSeverityGainPerDay;

        public int baseTicksAtPeakSeverityBeforeSeverityLoss;

        public float baseSeverityLossPerDay;

    }

}
