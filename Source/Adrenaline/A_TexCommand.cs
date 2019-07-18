using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using System.Reflection.Emit;

namespace Adrenaline
{

    [StaticConstructorOnStartup]
    public static class A_TexCommand
    {

        public static readonly Texture2D Adrenaline = ContentFinder<Texture2D>.Get("UI/Commands/Adrenaline");

    }

}
