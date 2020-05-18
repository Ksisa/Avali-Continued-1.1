using System;
using UnityEngine;
using Verse;

// DISABLED
// WORK IN PROGRESS

namespace Avali
{
	public class Settings : ModSettings
	{
		public static float AvaliEggChance = 10;
		
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref Settings.AvaliEggChance, "AvaliEggChance", 10, false);
		}

		public static void DoSettingsWindowContents(Rect rect)
		{
			Listing_Standard listing_Standard = new Listing_Standard(GameFont.Small);
			listing_Standard.ColumnWidth = rect.width;
			listing_Standard.Begin(rect);
			listing_Standard.Label("AvaliEggChance".Translate() + (int)Settings.AvaliEggChance + "%");
			Settings.AvaliEggChance = listing_Standard.Slider(Settings.AvaliEggChance, 0, 100);
			listing_Standard.End();
		}
	}
}
