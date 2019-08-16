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

    public class AdrenalineSettings : ModSettings
    {

        public static bool affectDownedPawns = false;

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            var defaultColor = GUI.color;
            options.Begin(wrect);
            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            options.Gap();
            options.CheckboxLabeled("Adrenaline.AffectDownedPawns".Translate(), ref affectDownedPawns, "Adrenaline.AffectDownedPawns_ToolTip".Translate());

            options.End();
            Mod.GetSettings<AdrenalineSettings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref affectDownedPawns, "affectDownedPawns", false);
        }

    }

}
