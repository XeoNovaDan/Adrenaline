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

    public class ThingDefExtension : DefModExtension
    {

        public static readonly ThingDefExtension defaultValues = new ThingDefExtension();

        public bool ingestibleWhenDowned;
        public string downedIngestGizmoLabel;
        public string downedIngestGizmoDescription;
        public string downedIngestGizmoTexPath;
        public string downedIngestGizmoNoneNearby;

    }

}
