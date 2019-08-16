using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace Adrenaline
{

    public class Adrenaline : Mod
    {

        public Adrenaline(ModContentPack content) : base(content)
        {
            GetSettings<AdrenalineSettings>();
            HarmonyInstance = HarmonyInstance.Create("XeoNovaDan.Adrenaline");
        }

        public static HarmonyInstance HarmonyInstance;

        public override string SettingsCategory() => "Adrenaline.SettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<AdrenalineSettings>().DoWindowContents(inRect);
        }

    }

}
