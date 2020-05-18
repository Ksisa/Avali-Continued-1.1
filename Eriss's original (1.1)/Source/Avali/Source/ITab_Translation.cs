using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace Avali
{
	public class ITab_Translation : ITab
	{
		public Vector2 WinSize = new Vector2(800f, 510f);
		private Vector2 scrollPosition = new Vector2(0f, 0f);
		
		public ITab_Translation()
		{
			this.size = WinSize;
			this.labelKey = "TabText";
		}
		
		private Thing thing
		{
			get
			{
				Thing thing = Find.Selector.SingleSelectedThing;
				MinifiedThing minifiedThing = thing as MinifiedThing;
				if (minifiedThing != null)
				{
					thing = minifiedThing.InnerThing;
				}
				if (thing == null)
				{
					return null;
				}
				return thing;
			}
		}
		
		private CompTextThing SelectedCompTextThing
		{
			get
			{
				return thing.TryGetComp<CompTextThing>();
			}
		}
		
		private CompProperties_TextThing Props
		{
			get
			{
				return SelectedCompTextThing.Props;
			}
		}
		
		protected override void FillTab()
		{
			Rect rect = new Rect(0, 0, WinSize.x, WinSize.y).ContractedBy(10f);
			
			Text.Font = GameFont.Medium;
			string label = "";
			label += thing.def.label;
			
			if (SelectedCompTextThing.workLeft <= 0 && Props.labelTranslated != "")
			{
				label += " (" + Props.labelTranslated + ")";
			}
			
			Widgets.Label(rect, label);
			
			rect.yMin += 35f;
			
			Text.Font = GameFont.Small;
			string text = "";
			if (SelectedCompTextThing.workLeft <= 0) text += Props.translatedText;
			else text += Props.descriptionNotTranslated;
			
			if (SelectedCompTextThing.workLeft <= 0)
			{
				text += "\n";
				if (Props.showAuthor) text += "\n" + "Author".Translate() + ": " + Props.author;
				if (Props.showTranslator)
				{
					text += "\n" + "Translator".Translate() + ": " + SelectedCompTextThing.translator;
				}
			}
			
			Widgets.LabelScrollable(rect, text, ref scrollPosition, false, true);
		}
	}
}
