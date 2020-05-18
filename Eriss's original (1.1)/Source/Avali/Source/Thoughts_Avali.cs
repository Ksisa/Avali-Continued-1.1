using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace Avali
{
	public static class Thoughts_Avali
	{	
		public static int TryAddPackLossThought(this Pawn pawn, List<Pawn> relatedPawns, int packLossStage)
		{
			if (pawn.ageTracker.CurLifeStage.reproductive) // adult pawns only
			{
				Thought thought = pawn.needs.mood.thoughts.memories.OldestMemoryOfDef(ThoughtDefOf.PackLoss);
				if (thought != null) 
				{
					if (packLossStage == -1) return thought.CurStageIndex;
				}
				else
				{
					//if (relatedPawns == null) relatedPawns = pawn.relations.RelatedPawns.ToList();
					for (int i = 0; i < relatedPawns.Count(); i++)
					{
						Pawn pawn2 = relatedPawns[i];
						
						if (pawn2.Map != null)
						{
							if (pawn.HavePackRelation(pawn2) || pawn.HaveLoveRelation(pawn2))
							{
								return -1;
							}
						}
					}
					
					if (packLossStage < 3) packLossStage++;
					else if (packLossStage > 3) packLossStage = 3;
					GiveThoughtWithStage(pawn, ThoughtDefOf.PackLoss, packLossStage);
				}
			}
			
			return packLossStage;
		}
		
		public static int TryRemovePackLossThought(this Pawn pawn, List<Pawn> relatedPawns, int packLossStage)
		{
			//if (relatedPawns == null) relatedPawns = pawn.relations.RelatedPawns.ToList();
			for (int i = 0; i < relatedPawns.Count(); i++)
			{
				Pawn pawn2 = relatedPawns[i];
				
				if (pawn2.Map != null)
				{
					if (pawn.HavePackRelation(pawn2) || pawn.HaveLoveRelation(pawn2))
					{
						pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.PackLoss);
						return -1;
					}
				}
			}
			
			return packLossStage;
		}
		
		public static void Thought_AvaliSleepingRoomImpressiveness(this Pawn pawn)
		{
			if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return;
			
			if (pawn.InBed())
			{
				Room room = pawn.GetRoom(RegionType.Set_Passable);
				if (room != null)
				{
					int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
					//Log.Message(pawn + "Sleeping room impressiveness = " + scoreStageIndex);
					
					GiveThoughtWithStage(pawn, ThoughtDefOf.AvaliSleepingRoomImpressiveness, scoreStageIndex);
					
					/*
					else if (room.Role == RoomRoleDefOf.PrisonCell || room.Role == RoomRoleDefOf.PrisonBarracks)
					{
						GiveThoughtWithStage(pawn, ThoughtDefOf.AvaliSleepingRoomImpressiveness, scoreStageIndex, false);
					}
					else if (room.Role == RoomRoleDefOf.PrisonBarracks)
					{
						//GiveThoughtWithStage(pawn, ThoughtDefOf.AvaliSleepingRoomImpressiveness, scoreStageIndex, false);
					}
					*/
				}
			}
		}
		
		public static void Thought_AvaliPackSleepingRoomRelations(this Pawn pawn, int maxPawnsInPack = 5)
		{
			if (!pawn.Awake() && !pawn.Downed)
			{
				Room room = pawn.GetRoom(RegionType.Set_Passable);
				if (room != null)
				{
					if (room.Role == RoomRoleDefOf.Barracks)
					{
						int stage = -1;
						
						List<Building_Bed> containedBeds = room.ContainedBeds.ToList();
						for (int i = 0; i < containedBeds.Count(); i++)
						{
							Building_Bed bed = containedBeds[i];
							if (bed.OwnersForReading != null)
							{
								for (int j = 0; j < bed.OwnersForReading.Count; j++)
								{
									Pawn bedOwner = bed.OwnersForReading[j];
									if (bedOwner.CurrentBed() == bed)
									{
										//Log.Message("bedOwner = " + bedOwner);
										if (pawn.HavePackRelation(bedOwner))
										{
											stage++;
											//Log.Message(pawn + " have direct relation with " + bedOwner);
											//Log.Message("stage = " + stage);
										}
									}
								}
							}
						}
						
						int maxStage = maxPawnsInPack - 1;
						if (stage > maxStage) stage = maxStage;
						//Log.Message("cur stage = " + stage);
						if (stage > -1)
						{
							GiveThoughtWithStage(pawn, ThoughtDefOf.AvaliPackSleepingRoomRelations, stage);
						}
					}
				}
			}
		}
		
		public static void Thought_SleepDisturbedAvali(this Pawn pawn,  int opinionNeeded = 25)
		{
			if (!pawn.Awake() && !pawn.Downed && pawn.IsColonist)
			{
				Room room = pawn.GetRoom(RegionType.Set_Passable);
				if (room != null)
				{
					List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
					for (int i = 0; i < containedAndAdjacentThings.Count; i++)
					{
						Thing thing = containedAndAdjacentThings[i];
						Pawn pawn2 = thing as Pawn;
						if (pawn2 != null)
						{
							if (pawn2.RaceProps.Humanlike && pawn != pawn2)
							{
								if (pawn.relations.OpinionOf(pawn2) < opinionNeeded && !pawn.HavePackRelation(pawn2))
								{
									pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AvaliSleepDisturbed);
								}
							}
						}
					}
				}
			}
		}
		
		public static void GiveThoughtWithStage(this Pawn pawn, ThoughtDef thought, int stage, bool overrideCurStage = true)
		{
			Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(thought);
			pawn.needs.mood.thoughts.memories.TryGainMemory(newThought);
			if (newThought.CurStage == null)
			{
				pawn.needs.mood.thoughts.memories.OldestMemoryOfDef(thought).SetForcedStage(stage);
				//pawn.needs.mood.thoughts.memories.ExposeData();
			}
			else if (overrideCurStage)
			{
				pawn.needs.mood.thoughts.memories.OldestMemoryOfDef(thought).SetForcedStage(stage);
				//pawn.needs.mood.thoughts.memories.ExposeData();
			}
		}
		
		public static void AddMemoryOfDef(this Pawn pawn, ThoughtDef thought)
		{
			Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(thought);
			pawn.needs.mood.thoughts.memories.TryGainMemory(newThought);
		}
		
		public static void TryAddNewThoughtToList(ThoughtDef thoughtDef, List<Thought> thoughtsList)
		{
			Thought newThought = (Thought)ThoughtMaker.MakeThought(thoughtDef);
			for (int i = 0; i < thoughtsList.Count; i++)
			{
				Thought thought = thoughtsList[i];
				if (thought == newThought) return;
			}
			
			thoughtsList.Add(newThought);
		}
	}
}
