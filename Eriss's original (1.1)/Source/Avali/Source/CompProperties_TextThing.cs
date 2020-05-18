using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace Avali
{
	public class CompProperties_TextThing : CompProperties
	{
		public CompProperties_TextThing()
		{
			this.compClass = typeof(CompTextThing);
		}
		
		public string translatedTexPath;
		
		public JobDef useJob;

		public string useLabel;
		
		public SkillDef workSkill;
		
		public int minSkillLevel = 0;
		
		public ThingDef workTable;
		
		public int workLeft = -1;
		
		public bool showAuthor = true;
		
		public bool showTranslator = true;
		
		public bool showWorkLeft = true;
		
		public float translatedMarketValue;
		
		public float defaultMarketValue;
		
		public string labelTranslated = "";
		
		public string descriptionNotTranslated = "";
		
		public string descriptionTranslated = "";
		
		public string translatedText = "";
		
		public string author;
		
		public TaleDef taleWhenTranslated;
		
		public ThoughtDef thoughtWhenTranslated;
		
		public Vector2 translationTabWinSize = Vector2.zero;
	}
}
