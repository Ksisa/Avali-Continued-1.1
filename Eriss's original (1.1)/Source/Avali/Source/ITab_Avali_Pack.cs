using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Avali
{
	public class ITab_Avali_Pack : ITab
	{
		public Vector2 WinSize = new Vector2(630f, 510f);
		public const float startPosY = 20;
		public const float labelSizeY = 24;
		
		private Hediff packHediff;
		private Color oldColor;
		
		public ITab_Avali_Pack()
		{
			this.size = WinSize;
			this.labelKey = "TabAvaliPack";
			oldColor = GUI.color;
		}
		
		public override bool IsVisible
		{
			get
			{
				return SelPawn.def == ThingDefOf.Avali;
			}
		}
		
		protected override void FillTab()
		{
			//Log.Message("ITab_Avali_Pack");
			
			Rect rect1 = new Rect(0, 0, WinSize.x, WinSize.y).ContractedBy(10f);
			Text.Font = GameFont.Small;
			oldColor = GUI.color;
			
			Hediff avaliBiology = SelPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.AvaliBiology);
			if (avaliBiology == null)
			{
				NotInPack(rect1);
				return;
			}
			
			HediffDef packHediffDef = (HediffDef)Traverse.Create(avaliBiology).Field("packHediffDef").GetValue();
			List<Pawn> packPawns = (List<Pawn>)Traverse.Create(avaliBiology).Field("packPawns").GetValue();
			Pawn packLeader = null;
			int maxPawnsInPack = (int)Traverse.Create(avaliBiology).Field("maxPawnsInPack").GetValue();
			
			//Log.Message("packHediffDef = " + packHediffDef + "; maxPawnsInPack = " + maxPawnsInPack + "; relatedPawns = " + packPawns);
			if (packHediffDef == null || maxPawnsInPack < 2 || packPawns == null)
			{
				NotInPack(rect1);
				return;
			}
			
			for (int i = 0; i < packPawns.Count; i++)
			{
				Pawn relatedPawn = packPawns[i];
				if (SelPawn.relations.DirectRelationExists(PawnRelationDefOf.PackLeader, relatedPawn))
				{
					packLeader = relatedPawn;
					break;
				}
			}
			if (packLeader == null) packLeader = SelPawn;
			
			packHediff = SelPawn.health.hediffSet.GetFirstHediffOfDef(packHediffDef);
			if (packHediff == null)
			{
				NotInPack(rect1);
				return;
			}
			
			/* --------------------------------------------------------------------------------------------- //
			
			Specialization: packHediff.LabelCap
			Pack Leader: packLeader
			Pack members: 5/5
			
			Pack activity effects:
			...
			
			// --------------------------------------------------------------------------------------------- */
			
			Rect rect2 = new Rect(0, startPosY, WinSize.x, 42).ContractedBy(10f);
			NewRect(rect2, "PackSpecialization".Translate(), packHediff.LabelCap, rect2, "PackSpecializationDesc".Translate());
			
			DrawLineHorizontalWithColor(10, startPosY + labelSizeY * 1 + 8, WinSize.x-20, Color.gray);
			//GUI.color = oldColor;
			
			Rect rect3 = new Rect(0, startPosY + labelSizeY * 1, WinSize.x, 42).ContractedBy(10f);
			NewRect(rect3, "PackLeader".Translate(), packLeader.Name.ToString(), rect3, "PackLeaderDesc".Translate());
			
			Rect rect4 = new Rect(0, startPosY + labelSizeY * 2, WinSize.x, 42).ContractedBy(10f);
			string packMembers = SelPawn.Name.ToString();
			int packMembersCount = 1;
			for (int i = 0; i < packPawns.Count; i++)
			{
				Pawn relatedPawn = packPawns[i];
				if (relatedPawn == SelPawn) continue;
				packMembers += "\n" + relatedPawn.Name;
				packMembersCount++;
			}
			NewRect(rect4, "PackMembers".Translate(), packMembersCount + "/" + maxPawnsInPack, rect4, packMembers);
			
			if (packHediff.CurStage == null || packHediff.CurStage.statOffsets == null || packHediff.CurStage.statOffsets.Count == 0)
			{
				NotInPack(rect1);
				return;
			}
			
			if (SelPawn.IsColonist || Prefs.DevMode)
			{
				DrawLineHorizontalWithColor(10, startPosY + labelSizeY * 5 + 8, WinSize.x-20, Color.gray);
				//GUI.color = oldColor;
				
				Rect rect5 = new Rect(0, startPosY + labelSizeY * 4, WinSize.x, 42).ContractedBy(10f);
				float hearingRange = 30 * SelPawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);
				NewRect(rect5, "PackEffects".Translate(), "", rect5, string.Format("PackEffectsDesc".Translate(), (int)hearingRange));
				
				float yPosition = startPosY + labelSizeY * 5;
				List<StatModifier> statOffsets = packHediff.CurStage.statOffsets;
				if (statOffsets.Count > 1)
				{
					int secondColumnStart = statOffsets.Count / 2;
					
					for (int i = secondColumnStart; i < statOffsets.Count; i++)
					{
						StatModifier packStatMod = statOffsets[i];
						if (packStatMod == null) continue;
						
						Rect rect = new Rect(WinSize.x / 2 - 10, yPosition, WinSize.x / 2 + 10, 42).ContractedBy(10f);
						Rect labelRect = new Rect(WinSize.x - 75, yPosition + 10, 1000, 42);
						NewRect(rect, packStatMod.stat.LabelCap, packStatMod.ValueToStringAsOffset, labelRect, packStatMod.stat.description);
						yPosition += labelSizeY;
					}
				}
				
				yPosition = startPosY + labelSizeY * 5;
				for (int i = 0; i < statOffsets.Count/2; i++)
				{
	
					StatModifier packStatMod = statOffsets[i];
					if (packStatMod == null) continue;
					
					Rect rect = new Rect(0, yPosition, WinSize.x / 2 + 10, 42).ContractedBy(10f);
					Rect labelRect = new Rect(WinSize.x / 2 - 75, yPosition + 10, 1000, 42);
					NewRect(rect, packStatMod.stat.LabelCap, packStatMod.ValueToStringAsOffset, labelRect, packStatMod.stat.description);
					yPosition += labelSizeY;
				}
			}
		}
		
		public void DrawLineHorizontalWithColor(float x, float y, float length, Color color)
		{
			GUI.color = color;
			Widgets.DrawLineHorizontal(x, y, length);
			GUI.color = oldColor;
		}
		
		public static void NewRect(Rect rect, string label, string rectLabel, Rect rectLabelRect, string tooltip = null)
		{
			Text.Font = GameFont.Small;
			Widgets.Label(rect, label);
			
			if (rectLabel != null)
			{
				if (rect == rectLabelRect) rectLabelRect = new Rect(rect.center.x, rect.y, rect.width, rect.height);
				Widgets.Label(rectLabelRect, rectLabel);
			}
			
			if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);
			
			if (tooltip != null) TooltipHandler.TipRegion(rect, new TipSignal(tooltip, rect.GetHashCode()));
		}
		
		public virtual string GetPackStatOffsets()
		{
			List<StatModifier> packStatOffsets = packHediff.CurStage.statOffsets;
			string packStatOffsetsStr = "";
			for (int i = 0; i < packStatOffsets.Count; i++)
			{
				StatModifier packStatOffset = packStatOffsets[i];
				packStatOffsetsStr += packStatOffset + "\n";
			}
			
			return packStatOffsetsStr;
		}
		
		public void NotInPack(Rect rect)
		{
			string label = string.Format("NotInPack".Translate(), SelPawn);
			Text.Font = GameFont.Medium;
			GUI.color = Color.gray;
			rect = new Rect(rect.x + WinSize.x / 4 - label.Length, rect.y + WinSize.y / 2 - label.Length, rect.width, rect.height);
			Widgets.Label(rect, label);
		}
	}
}
