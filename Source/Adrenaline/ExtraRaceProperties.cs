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

    public class ExtraRaceProperties : DefModExtension
    {

        public static readonly ExtraRaceProperties defaultValues = new ExtraRaceProperties();

        public float adrenalineGainFactorNatural = 1;

        public float adrenalineGainFactorArtificial = 1;

        public float adrenalineLossFactor = 1;

        public bool HasAdrenaline => adrenalineGainFactorNatural > 0 || adrenalineGainFactorArtificial > 0;


    }

}
