using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace Avali
{
	public class CompTextThing : ThingComp
	{
		public int workLeft = -1;
		
		public string translator = "";
		
		public bool debug = false;
		
		public CompProperties_TextThing Props
		{
			get
			{
				return (CompProperties_TextThing)props;
			}
		}
		
		protected virtual string FloatMenuOptionLabel
		{
			get
			{
				return Props.useLabel;
			}
		}
		
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (Props.author == "") Props.author = "Unknown".Translate();
			
			if (Props.defaultMarketValue == 0) Props.defaultMarketValue = parent.def.BaseMarketValue;
			else
			{
				if (workLeft != 0) parent.def.BaseMarketValue = Props.defaultMarketValue;
			}
			
			if (workLeft > Props.workLeft) workLeft = Props.workLeft;
			
			if (workLeft == -1)
			{
				workLeft = Props.workLeft;
			}
			else if (workLeft > 0)
			{
				if(CheckTale()) workLeft = 0;
			}
			else if (workLeft == 0)
			{
				if (translator == "") translator = "UnknownLower".Translate();
				//if (Props.translatedTexPath != "") parent.Graphic.path = Props.translatedTexPath;
				
				parent.def.BaseMarketValue = Props.translatedMarketValue;
			}
			
			if (Props.translationTabWinSize != Vector2.zero)
			{
				List<InspectTabBase> inspectTabs = (List<InspectTabBase>)parent.GetInspectTabs();
				for (int i = 0; i < inspectTabs.Count(); i++)
				{
					ITab_Translation translationTab = inspectTabs[i] as ITab_Translation;
					if (translationTab != null)
					{
						translationTab.WinSize = Props.translationTabWinSize;
						break;
					}
				}
			}
		}
		
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref workLeft, "workLeft", -1, false);
			Scribe_Values.Look<string>(ref translator, "translator", "", false);
		}
		
		public override void PostSplitOff(Thing piece)
		{
			CompTextThing comp = ((ThingWithComps)piece).GetComp<CompTextThing>();
			comp.workLeft = workLeft;
			comp.translator = translator;
		}
		
		public override string TransformLabel(string label)
		{
			if (workLeft <= 0 && Props.labelTranslated != "")
			{
				return label += " (" + Props.labelTranslated + ")";
			}
			
			return label;
		}
		
		public override string GetDescriptionPart()
		{
			StringBuilder stringBuilder = new StringBuilder();
			
			if (Props.workLeft > 0 && workLeft > 0)
			{
				if (Props.descriptionNotTranslated != "") stringBuilder.Append(Props.descriptionNotTranslated);
				else if (parent.def.description != "") stringBuilder.Append(parent.def.description);
				
				if (Props.showWorkLeft)
				{
					float translationPercent = Math.Abs(workLeft - Props.workLeft);
					if (translationPercent > 0)
					{
						translationPercent = 100 / (Props.workLeft / translationPercent);
					}
					
					stringBuilder.Append("\n\n" + "TranslationProgress".Translate() + translationPercent + "%");
				}
				
				if (Props.workSkill != null)
				{
					stringBuilder.Append("\n" + "RequredSkill".Translate() + " " + Props.workSkill + "(" + Props.minSkillLevel + ")");
				}
				
				return stringBuilder.ToString();
			}
			
			if (Props.descriptionTranslated != "") stringBuilder.Append(Props.descriptionTranslated);
			else if (Props.descriptionNotTranslated != "") stringBuilder.Append(Props.descriptionNotTranslated);
			else if (parent.def.description != "") stringBuilder.Append(parent.def.description);
			
			return stringBuilder.ToString();
		}
		
		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Props.workLeft > 0 && workLeft > 0)
			{
				if(CheckTale())
				{
					workLeft = 0;
					if (translator == "") translator = "UnknownLower".Translate();
					//if (Props.translatedTexPath != "") parent.Graphic.path = Props.translatedTexPath;
					
					parent.def.BaseMarketValue = Props.translatedMarketValue;
					return null;
				}
				
				if (Props.descriptionNotTranslated != "") stringBuilder.Append(Props.descriptionNotTranslated);
				else stringBuilder.Append(parent.def.description);
				
				stringBuilder.Append("\n ");
				
				if (Props.showWorkLeft)
				{
					if (Props.workLeft > 0 && workLeft > 0)
					{
						float translationPercent = Math.Abs(workLeft - Props.workLeft);
						if (translationPercent > 0)
						{
							translationPercent = 100 / (Props.workLeft / translationPercent);
						}
						
						stringBuilder.Append("\n" + "TranslationProgress".Translate() + translationPercent + "%");
					}
				}
				
				if (Props.workSkill != null)
				{
					stringBuilder.Append("\n" + "RequredSkill".Translate() + " " + Props.workSkill + "(" + Props.minSkillLevel + ")");
				}
			}
			else
			{
				if (Props.descriptionTranslated != "") stringBuilder.Append(Props.descriptionTranslated);
				else if (Props.descriptionNotTranslated != "") stringBuilder.Append(Props.descriptionNotTranslated);
				else stringBuilder.Append(parent.def.description);
			}
			return stringBuilder.ToString();
		}
		
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (debug) Log.Message(parent + " workLeft: " + workLeft);
			if (workLeft == 0) yield break;
			
			if (debug) Log.Message(parent + " Tale check.");
			if(CheckTale())
			{
				workLeft = 0;
				if (translator == "") translator = "UnknownLower".Translate();
				//if (Props.translatedTexPath != "") parent.Graphic.path = Props.translatedTexPath;
				
				parent.def.BaseMarketValue = Props.translatedMarketValue;
				yield break;
			}
			
			if (debug) Log.Message(parent + " Drafted and ToolUser check");
			if (selPawn.Drafted || !selPawn.RaceProps.ToolUser)
			{
				yield break;
			}
			
			if (debug) Log.Message(parent + " CanReserve check.");
			if (!selPawn.CanReserve(parent, 1, -1, null, false))
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			else
			{
				//if (parent.IsForbidden(selPawn)) yield break;
				
				if (Props.workSkill != null && selPawn.skills.GetSkill(Props.workSkill).Level < Props.minSkillLevel)
				{
					yield return new FloatMenuOption("CantTranslate".Translate() + Props.workSkill + "SkillLevelToSmall".Translate() + Props.minSkillLevel + ".", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield break;
				}
			
				if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					yield return new FloatMenuOption(selPawn + "CantReach".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield break;
				}
				
				FloatMenuOption useopt = new FloatMenuOption(FloatMenuOptionLabel, delegate
				{
					foreach (CompUseEffect compUseEffect in parent.GetComps<CompUseEffect>())
					{
						if (compUseEffect.SelectedUseOption(selPawn))
						{
							return;
						}
					}
					TryStartUseJob(selPawn);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				yield return useopt;
			}
			yield break;
		}
		
		public void TryStartUseJob(Pawn selPawn)
		{
			if (debug) Log.Message("TryStartUseJob");
			
			Job job = null;
			if (Props.workTable == null)
			{
				job = new Job(Props.useJob, parent, null, null);
				
				Thing sittableThing = AvaliUtility.FindAllThingsOnMapAtRange(selPawn, null, typeof(Building), null, 15, 1, true, true).First();
				if (sittableThing != null) job = new Job(Props.useJob, parent, sittableThing, null);
				
				if (debug) Log.Message(selPawn + " job = " + Props.useJob + ", " + parent + ", " + sittableThing);
			}
			else
			{
				Thing workTable = null;
				
				if (Props.workTable == ThingDef.Named("SimpleResearchBench"))
				{
					workTable = AvaliUtility.FindClosestUnoccupiedThing(selPawn, ThingDef.Named("HiTechResearchBench"), 9999, true);
					if (debug) Log.Message("Closest unoccupied hi-tech research bench = " + workTable);
				}
				
				if (workTable == null)
				{
					workTable = AvaliUtility.FindClosestUnoccupiedThing(selPawn, Props.workTable, 9999, false);
					if (debug) Log.Message("Closest unoccupied work table = " + workTable);
				}
				
				if (workTable == null)
				{
					//if (Props.workTable.defName == "HiTechResearchBench") new AcceptanceReport("NoHiTechResearchBench".Translate());
					//else if (Props.workTable.defName == "SimpleResearchBench") new AcceptanceReport("NoResearchBench".Translate());
					//else new AcceptanceReport("NoAppropriateWorkBench".Translate());
					
					if (debug) Log.Message("workTable = null");
					return;
				}
				
				if (!workTable.def.hasInteractionCell)
				{
					Log.Error(workTable + " not have interaction cell.");
					return;
				}
				
				job = new Job(Props.useJob, parent, workTable, workTable.OccupiedRect().ClosestCellTo(workTable.InteractionCell));
			}
			
			job.count = 1;
			selPawn.jobs.TryTakeOrderedJob(job, JobTag.MiscWork);
		}
		
		public bool CheckTale()
		{
			if (Props.taleWhenTranslated != null)
			{
				List<Tale> allTalesList = Find.TaleManager.AllTalesListForReading;
				for (int i = 0; i < allTalesList.Count(); i++)
				{
					Tale tale = allTalesList[i];
					if (tale.def == Props.taleWhenTranslated)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
