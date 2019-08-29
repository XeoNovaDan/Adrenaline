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

        public static bool allowNaturalGain = true;
        public static bool affectDownedPawns = false;
        public static bool adrenalineCrashes = true;
        public static bool npcUse = true;

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            var defaultColor = GUI.color;
            options.Begin(wrect);
            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            options.Gap();
            options.CheckboxLabeled("Adrenaline.AllowNaturalGain".Translate(), ref allowNaturalGain, "Adrenaline.AllowNaturalGain_ToolTip".Translate());

            options.Gap();
            options.CheckboxLabeled("Adrenaline.AffectDownedPawns".Translate(), ref affectDownedPawns, "Adrenaline.AffectDownedPawns_ToolTip".Translate());

            options.Gap();
            options.CheckboxLabeled("Adrenaline.AllowAdrenalineCrashes".Translate(), ref adrenalineCrashes, "Adrenaline.AllowAdrenalineCrashes_ToolTip".Translate());

            options.Gap();
            options.CheckboxLabeled("Adrenaline.AllowNPCUse".Translate(), ref npcUse, "Adrenaline.AllowNPCUse_ToolTip".Translate());

            options.End();
            Mod.GetSettings<AdrenalineSettings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref allowNaturalGain, "allowNaturalGain", true);
            Scribe_Values.Look(ref affectDownedPawns, "affectDownedPawns", false);
            Scribe_Values.Look(ref adrenalineCrashes, "adrenalineCrashes", true);
            Scribe_Values.Look(ref npcUse, "npcUse", true);
        }

    }

}
