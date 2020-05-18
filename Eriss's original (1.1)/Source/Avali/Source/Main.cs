using System;
using UnityEngine;
using Verse;

// DISABLED
// WORK IN PROGRESS

namespace Avali
{
	public class Main : Mod
	{
		public Main(ModContentPack content) : base(content)
		{
			base.GetSettings<Settings>();
		}
		
		public override string SettingsCategory()
		{
			return "Avali".Translate();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoSettingsWindowContents(inRect);
		}
	}
}
