using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

// DISABLED
// WORK IN PROGRESS

namespace Avali
{
	public class CompHatcherAvali : ThingComp
	{
		private float gestateProgress;

		public Pawn hatcheeParent;

		public Pawn otherParent;

		public Faction hatcheeFaction;
		
		public CompProperties_HatcherAvali Props
		{
			get
			{
				return (CompProperties_HatcherAvali)props;
			}
		}

		private CompTemperatureRuinable FreezerComp
		{
			get
			{
				return this.parent.GetComp<CompTemperatureRuinable>();
			}
		}

		public bool TemperatureDamaged
		{
			get
			{
				CompTemperatureRuinable freezerComp = this.FreezerComp;
				return freezerComp != null && this.FreezerComp.Ruined;
			}
		}
		
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref this.gestateProgress, "gestateProgress", 0f, false);
			Scribe_References.Look<Pawn>(ref this.hatcheeParent, "hatcheeParent", false);
			Scribe_References.Look<Pawn>(ref this.otherParent, "otherParent", false);
			Scribe_References.Look<Faction>(ref this.hatcheeFaction, "hatcheeFaction", false);
		}
		
		public override void CompTick()
		{			
			if (!TemperatureDamaged)
			{
				float num = 1f / (Props.hatcherDaystoHatch * 60000f);
				gestateProgress += num;
				if (gestateProgress > 1f)
				{
					RemoveHediffAndThought();
					HatchAvali();
				}
			}
		}
		
		public void RemoveHediffAndThought()
		{
			Pawn mother = parent.TryGetComp<CompHatcherAvali>().hatcheeParent;
			Pawn father = parent.TryGetComp<CompHatcherAvali>().otherParent;
			
			if (mother != null)
			{
				Hediff hediff = mother.health.hediffSet.hediffs.Find((Hediff x) => x.def == HediffDefOf.AvaliCaresOfEgg);
				if (hediff != null)
				{
					mother.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.AvaliCaresOfEgg);
					mother.health.RemoveHediff(hediff);
				}
			}
			
			if (father != null)
			{
				Hediff hediff = father.health.hediffSet.hediffs.Find((Hediff x) => x.def == HediffDefOf.AvaliCaresOfEgg);
				if (hediff != null)
				{
					father.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.AvaliCaresOfEgg);
					father.health.RemoveHediff(hediff);
				}
			}
		}
		
		public void HatchAvali()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(Props.hatcherPawn, hatcheeFaction, PawnGenerationContext.PlayerStarter, -1, false, true, false, false, true, false, 0, false, true, true, false, false, false, false, null, null, null, null, null, null, null);
			for (int i = 0; i < parent.stackCount; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent))
				{
					if (pawn != null)
					{
						pawn.story.childhood = BackstoryDatabase.allBackstories["RimworldChildAvali"];
						pawn.story.adulthood = BackstoryDatabase.allBackstories["RimworldAdultAvali"];
						pawn.health.AddHediff(HediffDefOf.AvaliAgeTracker);
						
						if (hatcheeParent != null)
						{
							if (pawn.playerSettings != null && hatcheeParent.playerSettings != null && hatcheeParent.Faction == hatcheeFaction)
							{
								pawn.playerSettings.AreaRestriction = hatcheeParent.playerSettings.AreaRestriction;
							}
							if (pawn.RaceProps.IsFlesh)
							{
								pawn.relations.AddDirectRelation(RimWorld.PawnRelationDefOf.Parent, hatcheeParent);
							}
						}
						if (otherParent != null && (hatcheeParent == null || hatcheeParent.gender != otherParent.gender) && pawn.RaceProps.IsFlesh)
						{
							pawn.relations.AddDirectRelation(RimWorld.PawnRelationDefOf.Parent, otherParent);
						}
						
						Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.AvaliHatched);
						pawn.needs.mood.thoughts.memories.TryGainMemory(newThought);
					}
					if (parent.Spawned)
					{
						FilthMaker.MakeFilth(parent.Position, parent.Map, RimWorld.ThingDefOf.Filth_AmnioticFluid, 5);
					}
				}
				else
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
			}
			parent.Destroy(DestroyMode.Vanish);
		}
		
		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(parent.stackCount + count);
			CompHatcherAvali comp = ((ThingWithComps)otherStack).GetComp<CompHatcherAvali>();
			float b = comp.gestateProgress;
			gestateProgress = Mathf.Lerp(gestateProgress, b, t);
		}
		
		public override void PostSplitOff(Thing piece)
		{
			CompHatcherAvali comp = ((ThingWithComps)piece).GetComp<CompHatcherAvali>();
			comp.gestateProgress = gestateProgress;
			comp.hatcheeParent = hatcheeParent;
			comp.otherParent = otherParent;
			comp.hatcheeFaction = hatcheeFaction;
		}
		
		public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PrePreTraded(action, playerNegotiator, trader);
			if (action == TradeAction.PlayerBuys)
			{
				hatcheeFaction = Faction.OfPlayer;
			}
			else if (action == TradeAction.PlayerSells)
			{
				hatcheeFaction = trader.Faction;
			}
		}

		public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
		{
			base.PostPostGeneratedForTrader(trader, forTile, forFaction);
			hatcheeFaction = forFaction;
		}

		public override string CompInspectStringExtra()
		{
			if (!TemperatureDamaged)
			{
				return "EggProgress".Translate() + ": " + gestateProgress.ToStringPercent();
			}
			return null;
		}
	}
}
