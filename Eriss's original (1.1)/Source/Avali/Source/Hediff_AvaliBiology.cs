using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class Hediff_AvaliBiology : HediffWithComps
	{
		public bool debug = false;
		
		public int pawnOpinionNeeded = 30;
		
		public int opinionToRecruit = 25;
		
		public int maxPawnsInPack = 5;
		
		public bool makeStartingPack = true;
		
		//public float chanceToFilthPerHour = 0.042f;
		
		public float minEnvResist = 0.15f;
		
		public List<Pawn> packPawns = new List<Pawn>();
		
		public List<Pawn> deadPackmates = new List<Pawn>();
		
		public HediffDef packHediffDef = null;
		
		public Pawn leader;
		
		public bool immuneToPackLoss = true;
		
		private int pawnPosInQueue = -1;
		
		private int pawnsInPack = 1;
		
		private int pawnTotalSkillCount;
		
		private int packLossStage = -1;
		
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref makeStartingPack, "makeStartingPack", true, false);
			Scribe_Values.Look<bool>(ref immuneToPackLoss, "immuneToPackLoss", true, false);
			Scribe_Defs.Look<HediffDef>(ref packHediffDef, "packHediffDef");
			Scribe_References.Look<Pawn>(ref leader, "leader", false);
			//Scribe_Collections.Look(ref deadPackmates, "deadPackmates", LookMode.Def);
		}
		
		public bool TryMakeStartingPack()
		{
			if (makeStartingPack == false) return false;
			
			if (debug) Log.Message("DaysPassed = " + GenDate.DaysPassedFloat);
			if (pawn.IsPrisoner) return false;
			if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath)) return false;
			if (pawn.IsColonist)
			{
				if (GenDate.DaysPassedFloat > 0.01f) return false;
			}
			
			for (int i = 0; i < packPawns.Count; i++)
			{
				Pawn pawn2 = packPawns[i];
				if (pawn.HavePackRelation(pawn2))
				{
					return false;
				}
			}
			
			List<Pawn> freeHumanlikesOfFaction = pawn.Map.mapPawns.FreeHumanlikesOfFaction(pawn.Faction).ToList();
			//Log.Message("FreeColonists count = " + freeHumanlikesOfFaction.Count);
			
			pawnPosInQueue = 0;
			for (int i = 0; i < freeHumanlikesOfFaction.Count; i++)
			{
				Pawn pawn2 = freeHumanlikesOfFaction[i];
				
				if (pawn2 == pawn || 
				    pawn2.def != ThingDefOf.Avali || 
				    pawn2.IsSlave() || 
				    pawn2.story.traits.HasTrait(TraitDefOf.Psychopath) ||
				    pawn2.HavePackRelation(pawn))
				{
					continue;
				}
				
				if (AvaliUtility.BothPawnsReproductiveOrNotReproductive(pawn, pawn2))
				{
					if (pawn.thingIDNumber > pawn2.thingIDNumber)
					{
						pawnPosInQueue+=1;
					}
				}
			}
			
			if (debug) Log.Message(pawn + " ID: " + pawn.thingIDNumber + "; pawnPosInQueue = " + pawnPosInQueue);
			return false;
		}
		
		public void CheckPawnsInPack(List<Pawn> relatedPawns)
		{
			pawnsInPack = 1;
			
			List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
			
			// get pawns in pack
			for (int i = 0; i < relatedPawns.Count; i++)
			{
				Pawn pawn2 = relatedPawns[i];
				
				// remove relation if child is dead or null
				if (pawn2.DestroyedOrNull() || pawn2.Dead)
				{
					if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Kit, pawn2))
					{
						for (int j = 0; j < memories.Count; j++)
						{
							Thought_Memory memory = memories[j];
							if (memory.def == RimWorld.ThoughtDefOf.PawnWithGoodOpinionDied)
							{
								if (memory.otherPawn == pawn2)
								{
									pawn.needs.mood.thoughts.memories.RemoveMemory(memory);
								}
							}
							
						}
						
					  pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Kit, pawn2);
					}
				}
				
				// remove relation if packmate is dead or null
				if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Packmate, pawn2))
				{
					if (pawn2.DestroyedOrNull() || pawn2.Dead)
					{
						for (int j = 0; j < memories.Count; j++)
						{
							Thought_Memory memory = memories[j];
							if (memory.def == RimWorld.ThoughtDefOf.PawnWithGoodOpinionDied)
							{
								if (memory.otherPawn == pawn2)
								{
									pawn.needs.mood.thoughts.memories.RemoveMemory(memory);
								}
							}
							
						}
						
						pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Packmate, pawn2);
						
						if (pawnsInPack > 1) pawnsInPack--;
					}
					else
					{
						if (pawn.relations.DirectRelationExists(PawnRelationDefOf.PackLeader, pawn2))
						{
							if (debug) Log.Message(pawn + " removed PackLeader relation with " + pawn2);
							pawn.relations.RemoveDirectRelation(PawnRelationDefOf.PackLeader, pawn2);
						}
						
						pawnsInPack++;
					}
				}
				
				// remove relation if leader is dead or null
				if (pawn.relations.DirectRelationExists(PawnRelationDefOf.PackLeader, pawn2))
				{
					if (pawn2.DestroyedOrNull() || pawn2.Dead)
					{
						for (int j = 0; j < memories.Count; j++)
						{
							Thought_Memory memory = memories[j];
							if (memory.def == RimWorld.ThoughtDefOf.PawnWithGoodOpinionDied)
							{
								if (memory.otherPawn == pawn2)
								{
									pawn.needs.mood.thoughts.memories.RemoveMemory(memory);
								}
							}
							
						}
						
						pawn.relations.RemoveDirectRelation(PawnRelationDefOf.PackLeader, pawn2);
						leader = null;
						
						if (pawnsInPack > 1) pawnsInPack--;
					}
					else if (!pawn2.DestroyedOrNull() && !pawn2.Dead)
					{
						leader = pawn2;
						pawnsInPack++;
					}
				}
			}
			
			// update leader
			Pawn newLeader = null;
			for (int i = 0; i < relatedPawns.Count; i++)
			{
				Pawn pawn2 = relatedPawns[i];
				if (pawn.DestroyedOrNull() || pawn2.Dead) continue;
				if (pawn.HavePackRelation(pawn2))
				{
					int pawn2TotalSkillLevel = pawn2.GetTotalSkillLevel();
					if (pawn2TotalSkillLevel > pawnTotalSkillCount)
					{
						pawnTotalSkillCount = pawn2TotalSkillLevel;
						newLeader = pawn2;
					}
				}
			}
			
			if (newLeader != null && newLeader != leader)
			{
				if (leader != null)
				{
					// remove PackLeader relation
					pawn.TryAddDirectRelation(leader, PawnRelationDefOf.Packmate);
					pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.PackLeader, leader);
				}
				
				// add PackLeader relation
				pawn.TryAddDirectRelation(newLeader, PawnRelationDefOf.PackLeader);
				pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Packmate, newLeader);
				
				leader = newLeader;
			}
		}
		
		public void UpdatePackHediff(List<Pawn> relatedPawns)
		{
			if (relatedPawns.Count == 0) return;
			
			SkillRecord highestPackSkill = pawn.GetHighestPackSkill(relatedPawns);
			if (debug)
			{
				Log.Message(pawn + "'s pack highestPackSkill = " + highestPackSkill);
				Log.Message(pawn + "'s pack size = " + relatedPawns.Count);
			}
			
			if (highestPackSkill == null) return;
			
			if (highestPackSkill.Level < 10)
			{
				// Exploration pack
				packHediffDef = pawn.TryRemovePackHediffsAndAddPackHediff(HediffDefOf.AvaliPackExploration);
			}
			else if (highestPackSkill.def == SkillDefOf.Melee || highestPackSkill.def == SkillDefOf.Shooting)
			{
				// Military pack
				packHediffDef = pawn.TryRemovePackHediffsAndAddPackHediff(HediffDefOf.AvaliPackMilitary);
			}
			else if (highestPackSkill.def == SkillDefOf.Intellectual || highestPackSkill.def == SkillDefOf.Medicine)
			{
				// Scientific pack
				packHediffDef = pawn.TryRemovePackHediffsAndAddPackHediff(HediffDefOf.AvaliPackScientific);
			}
			else if (highestPackSkill.def == SkillDefOf.Crafting || 
			         highestPackSkill.def == SkillDefOf.Mining || 
			         highestPackSkill.def == SkillDefOf.Plants)
			{
				// Industrial pack
				packHediffDef = pawn.TryRemovePackHediffsAndAddPackHediff(HediffDefOf.AvaliPackIndustrial);
			}
			else if (highestPackSkill.def == SkillDefOf.Artistic || highestPackSkill.def == SkillDefOf.Cooking)
			{
				// Artistical pack
				packHediffDef = pawn.TryRemovePackHediffsAndAddPackHediff(HediffDefOf.AvaliPackArtistical);
			}
			else
			{
				// Exploration pack
				packHediffDef = pawn.TryRemovePackHediffsAndAddPackHediff(HediffDefOf.AvaliPackExploration);
			}
			
			if (debug) Log.Message(pawn + " packHediffDef = " + packHediffDef);
		}
		
		public void UpdatePackHediffState(HediffDef packHediffDef, List<Pawn> packPawns)
		{
			if (packHediffDef == null || packPawns.Count == 0) return;
			
			int stage = 0;
			float hearingRange = 30 * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);
			Room currentRoom = pawn.GetRoom(RegionType.Set_Passable);
			//Log.Message(pawn + "currentRoom = " + currentRoom);
			if (currentRoom == null || hearingRange < 1) return;
			
			for (int i = 0; i < packPawns.Count; i++)
			{
				Pawn relatedPawn = packPawns[i];
				if (relatedPawn.def == ThingDefOf.Avali && pawn.HavePackRelation(relatedPawn))
				{
					float currHearingRange = hearingRange;
					if (currentRoom != relatedPawn.GetRoom(RegionType.Set_Passable)) currHearingRange = currHearingRange / 2;
					
					IntVec3 pawnPosition = pawn.Position;
					IntVec3 relatedPawnPosition = relatedPawn.Position;
					double distToRelatedPawn = Math.Sqrt(Math.Pow(relatedPawnPosition.x - pawnPosition.x, 2) + Math.Pow(relatedPawnPosition.y - pawnPosition.y, 2));
					//Log.Message("Distance from " + pawn + " to " + relatedPawn + " = " + distToRelatedPawn);
					if (distToRelatedPawn <= currHearingRange) stage++;
					if (stage == maxPawnsInPack) break;
				}
			}
			
			if (stage == 0) return;
			
			Hediff currentPackHediff = pawn.health.hediffSet.GetFirstHediffOfDef(packHediffDef);
			if (currentPackHediff != null && currentPackHediff.Severity != stage)
			{
				currentPackHediff.Severity = stage;
			}
		}
		
		public bool TryRecruitAvaliPrisoner()
		{
			if (!pawn.IsColonist && pawn.IsPrisoner && pawn.Awake())
			{
				if (pawn.CurJob.def == RimWorld.JobDefOf.PrisonerAttemptRecruit)
				{
					List<Pawn> playerFactionPawns = pawn.Map.mapPawns.FreeHumanlikesOfFaction(Faction.OfPlayer).ToList();
					for (int i = 0; i < playerFactionPawns.Count; i++)
					{
						Pawn playerFactionPawn = playerFactionPawns[i];
						
						if(pawn.relations.OpinionOf(playerFactionPawn) >= opinionToRecruit)
						{
							pawn.SetFaction(playerFactionPawn.Faction, playerFactionPawn);
							Messages.Message("LetterLabelMessageRecruitSuccess".Translate().CapitalizeFirst(), pawn, MessageTypeDefOf.PositiveEvent);
							pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
							
							return true;
						}
					}
				}
			}
			
			return false;
		}
		
		public bool TryMakeImmuneToPackLoss()
		{
			if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath) ||
					pawn.health.hediffSet.HasHediff(HediffDef.Named("AvaliPackLossAugment")) ||
			  	pawn.health.hediffSet.HasHediff(HediffDef.Named("CyberneticAvaliHead")))
			{
				packLossStage = -1;
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.PackLoss);
				return true;
			}
			
			packLossStage = pawn.TryRemovePackLossThought(packPawns, packLossStage);
			return false;
		}
		
		public override void Tick()
		{
			if (pawn.Map == null || pawn.InContainerEnclosed) return;
			
			if (packPawns.Count == 0) packPawns = pawn.relations.RelatedPawns.ToList();
			
			if (pawn.IsHashIntervalTick(60)) // update per second
			{
				makeStartingPack = TryMakeStartingPack();
				
				TryUpdatePackLossStage();
			  UpdateResists();
			  CheckBodyparts();
			  UpdatePackHediffState(packHediffDef, packPawns);
				TryToTakeDownedOrDeadPawn(packPawns);
			}
			
			if (pawn.IsHashIntervalTick(2500) || pawnPosInQueue > -1) // update per game hour
			{
				if (pawnPosInQueue > 0)
				{
					pawnPosInQueue--;
					return;
				}
				
				if (debug) Log.Message(pawn + " pawnPosInQueue = " + pawnPosInQueue);
				
				TryToTakeDownedOrDeadPawn(packPawns);
				
				//Log.Message(pawn + " packmates count = " + relatedPawns.Count);
				packPawns = pawn.relations.RelatedPawns.ToList();
				//Log.Message(pawn + " packmates count = " + relatedPawns.Count);
				
				//if (Rand.Chance(chanceToFilthPerHour)) FilthMaker.MakeFilth(pawn.Position, pawn.Map, RimWorld.ThingDefOf.Filth_Dirt, pawn.LabelIndefinite(), 1);
				
				UpdateRaceSpecificThoughts();
				immuneToPackLoss = TryMakeImmuneToPackLoss();
				
				if (!pawn.Awake() || pawn.IsSlave())
				{
					if (pawnPosInQueue == 0) pawnPosInQueue = -1;
					return;
				}
				
				if (TryRecruitAvaliPrisoner()) return;
				
				pawnTotalSkillCount = pawn.GetTotalSkillLevel();
				
				CheckPawnsInPack(packPawns);
				
				//if (debug) Log.Message(pawn + " packSize = " + pawnsInPack);
				//Log.Message("FreeColonists count = " + pawn.Map.mapPawns.FreeColonists.Count());
				
				//if (debug) Log.Message(pawn + " packSize = " + pawnsInPack);
				
				int currentPawnsInPack = pawnsInPack;
				TryMakePack();
				if (currentPawnsInPack < pawnsInPack)
				{
					MakePawnKnowHisPackmates();
					CheckPawnsInPack(packPawns);
				}
				
				UpdatePackHediff(packPawns);
				
				if (pawnPosInQueue == 0) pawnPosInQueue = -1;
			}
		}
		
		public void TryMakePack()
		{
			//TryMakePackUsingJoinPack();
			
			if (pawnsInPack < maxPawnsInPack)
			{
				List<Pawn> freeHumanlikesOfFaction = pawn.Map.mapPawns.FreeHumanlikesOfFaction(pawn.Faction).ToList();
				//Log.Message("FreeColonists count = " + freeHumanlikesOfFaction.Count());
				for (int i = 0; i < freeHumanlikesOfFaction.Count; i++)
				{
					Pawn pawn2 = freeHumanlikesOfFaction[i];
					
					if (pawn2 == pawn || !pawn2.Awake() || pawn2.IsSlave() || pawn.HavePackRelation(pawn2))
					{
						continue;
					}
					
					if (AvaliUtility.BothPawnsReproductiveOrNotReproductive(pawn, pawn2)) // kits form packs with kits and adults form packs with adults
					{
						if (pawnPosInQueue == 0 || (pawn.relations.OpinionOf(pawn2) >= pawnOpinionNeeded && pawn2.relations.OpinionOf(pawn) >= pawnOpinionNeeded))
						{
							if (TotalPawnsInPack(pawn2) < maxPawnsInPack) // check if pawn2 has atleast 1 vacant slot in pack
							{
								if (debug) Log.Message(pawn + " try make pack relation with " + pawn2);
								pawn.TryAddDirectRelation(pawn2, PawnRelationDefOf.Packmate);
								if (pawn2.def == ThingDefOf.Avali)
								{
									pawn2.TryAddDirectRelation(pawn, PawnRelationDefOf.Packmate);
								}
								
								pawnsInPack++;
								
								if (debug) Log.Message(pawn + " packSize = " + pawnsInPack);
								if (pawnsInPack >= maxPawnsInPack) return;
							}
							/*
							if (TotalPawnsInPack(pawn2) < maxPawnsInPack) // check if pawn2 has atleast 1 valant slot in pack
							{
								if (debug) Log.Message(pawn + " try make pack relation with " + pawn2);
								GetPawnSkillCount(pawn2); // compare pawn and pawn2 total skill count
							}
							*/
						}
					}
				}
			}
		}
		
		public void TryToTakeDownedOrDeadPawn(List<Pawn> relatedPawns)
		{
			if (pawn.CurJobDef == JobDefOf.TakeDownedOrDeadPawn || 
			    pawn.IsColonist || pawn.MentalState == null || pawn.IsSlave())
			{
				return;
			}
			
			if (pawn.MentalStateDef == MentalStateDefOf.PanicFlee)
			{
				const float maxSearchDistance = 20;
				
				IntVec3 exitCell;
				if (!RCellFinder.TryFindBestExitSpot(pawn, out exitCell, TraverseMode.ByPawn))
				{
					return;
				}
				
				// First priority: find any downed humanlike what is related to this pawn
				Predicate<Thing> validator = delegate(Thing t)
				{
					Pawn pawn3 = t as Pawn;
					
					return  pawn3 != null && pawn3.RaceProps.Humanlike &&
									pawn3.PawnListed(relatedPawns) &&
									pawn3.Downed &&
									pawn.CanReserve(pawn3);
				};
				Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (pawn2 != null)
				{
					//Log.Message(pawn + " try start job TakeDownedOrDeadPawn to " + pawn2);
					
					Job takeDownedOrDeadPawn = new Job(JobDefOf.TakeDownedOrDeadPawn)
					{
						targetA = pawn2,
						targetB = exitCell,
						count = 1
					};
					
					pawn.jobs.StartJob(takeDownedOrDeadPawn, JobCondition.InterruptForced, null, true);
					return;
				}
				
				// Second priority: find any downed humanlike what is consists in pawn's faction
				validator = delegate(Thing t)
				{
					Pawn pawn3 = t as Pawn;
					
					return  pawn3 != null && pawn3.RaceProps.Humanlike &&
									pawn3.Faction == pawn.Faction &&
									pawn3.Downed &&
									pawn.CanReserve(pawn3);
				};
				pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (pawn2 != null)
				{
					//Log.Message(pawn + " try start job TakeDownedOrDeadPawn to " + pawn2);
					
					Job takeDownedOrDeadPawn = new Job(JobDefOf.TakeDownedOrDeadPawn)
					{
						targetA = pawn2,
						targetB = exitCell,
						count = 1
					};
					
					pawn.jobs.StartJob(takeDownedOrDeadPawn, JobCondition.InterruptForced, null, true);
					return;
				}
				
				// Third priority: find any dead humanlike what was related to this pawn
				validator = delegate(Thing t)
				{
					Corpse corpse = t as Corpse;
					
					return  corpse != null && corpse.InnerPawn != null &&
									corpse.InnerPawn.RaceProps.Humanlike &&
									corpse.InnerPawn.PawnListed(relatedPawns) &&
									pawn.CanReserve(corpse);
				};
				Corpse corpse2 = (Corpse)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (corpse2 != null)
				{
					//Log.Message(pawn + " try start job TakeDownedOrDeadPawn to " + corpse2);
					
					Job takeDownedOrDeadPawn = new Job(JobDefOf.TakeDownedOrDeadPawn)
					{
						targetA = corpse2,
						targetB = exitCell,
						count = 1
					};
					
					pawn.jobs.StartJob(takeDownedOrDeadPawn, JobCondition.InterruptForced, null, true);
					return;
				}
				
				return;
			}
			
			
			
//			if (pawn.health.HasHediffsNeedingTend())
//			{
//				float maxSearchDistance = 30 * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);
//				List<Thing> allPawnsOfFactionOnMap = (List<Thing>)pawn.Map.mapPawns.FreeHumanlikesOfFaction(pawn.Faction);
//				List<Thing> allPawnsOfFactionInRange = AvaliUtility.FindAllThingsOnMapAtRange(pawn as Thing, null, typeof(Pawn), allPawnsOfFactionOnMap, maxSearchDistance, 9999, true, true);
//				
//				if (allPawnsOfFactionInRange.Count == 0)
//				{
//					if (!pawn.Downed)
//					{
//						
//					}
//					else return;
//				}
//				
//				Pawn pawn1 = null;
//				
//				for (int i = 0; i < allPawnsOfFactionInRange.Count; i++)
//				{
//					Pawn pawn2 = allPawnsOfFactionInRange[i] as Pawn;
//					if (pawn2 != null && pawn2 != pawn && pawn2.inventory.Contains(Thing))
//					{
//						
//					}
//				}
//				
//				
//				
//				
////					if (pawn.inventory.Contains(ThingMaker.MakeThing(RimWorld.ThingDefOf.MedicineIndustrial)))
////					{
////						
////					}
//				
//				#region Выполнется только в случае если у pawn1 в инвентаре есть медикоменты и pawnMedicineSkillLevel/2 < pawn1MedicineSkillLevel
//				Predicate<Thing> validator = delegate(Thing t)
//				{
//					Pawn pawn1 = t as Pawn;
//					
//					if (pawn1 != null && pawn1 != pawn && 
//					    pawn1.RaceProps.Humanlike && pawn1.Faction == pawn.Faction && 
//					    pawn1.inventory.Contains())
//					{
//						int pawnMedicineSkillLevel = pawn.GetSkillLevel(SkillDefOf.Medicine);
//						int pawn1MedicineSkillLevel = pawn1.GetSkillLevel(SkillDefOf.Medicine);
//						
//						if (pawnMedicineSkillLevel/2 < pawn1MedicineSkillLevel)
//						{
//							return pawn.CanReserve(pawn1) && pawn1.CanReserve(pawn);
//						}
//					}
//					
//					return false;
//				};
//				Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
//				if (pawn2 != null)
//				{
//					Job tendDownedPawn = new Job(RimWorld.JobDefOf.TendPatient)
//					{
//						targetA = pawn,
//						count = 1
//					};
//					
//					pawn2.jobs.StartJob(tendDownedPawn, JobCondition.InterruptForced, null, true);
//					return;
//				}
//				#endregion
//				
//				#region Выполнется только в случае если у pawn1 в инвентаре есть медикоменты
//				validator = delegate(Thing t)
//				{
//					Pawn pawn1 = t as Pawn;
//					
//					if (pawn1 != null && pawn1.RaceProps.Humanlike && pawn1.Faction == pawn.Faction && pawn1.inventory.Contains())
//					{
//						int pawnMedicineSkillLevel = pawn.GetSkillLevel(SkillDefOf.Medicine);
//						int pawn1MedicineSkillLevel = pawn1.GetSkillLevel(SkillDefOf.Medicine);
//						
//						if (pawnMedicineSkillLevel/2 < pawn1MedicineSkillLevel)
//						{
//							return pawn.CanReserve(pawn1) && pawn1.CanReserve(pawn);
//						}
//					}
//					
//					return false;
//				};
//				pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
//				if (pawn2 != null)
//				{
//					Job tendDownedPawn = new Job(RimWorld.JobDefOf.TendPatient)
//					{
//						targetA = pawn,
//						count = 1
//					};
//					
//					pawn2.jobs.StartJob(tendDownedPawn, JobCondition.InterruptForced, null, true);
//					return;
//				}
//				#endregion
//				
//				
//				
//				
//				
//				validator = delegate(Thing t)
//				{
//					Pawn pawn1 = t as Pawn;
//					
//					if (pawn1 != null && pawn1.RaceProps.Humanlike && pawn1.Faction == pawn.Faction)
//					{
//						int pawnMedicineSkillLevel = pawn.GetSkillLevel(SkillDefOf.Medicine);
//						int pawn1MedicineSkillLevel = pawn1.GetSkillLevel(SkillDefOf.Medicine);
//						
//						if (pawnMedicineSkillLevel < pawn1MedicineSkillLevel)
//						{
//							return pawn.CanReserve(pawn1) && pawn1.CanReserve(pawn);
//						}
//					}
//					
//					return false;
//				};
//				pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
//				if (pawn2 != null)
//				{
//					Job tendDownedPawn = new Job(RimWorld.JobDefOf.TendPatient)
//					{
//						targetA = pawn,
//						count = 1
//					};
//					
//					pawn2.jobs.StartJob(tendDownedPawn, JobCondition.InterruptForced, null, true);
//					return;
//				}
//				
//				
//				
//				
//				
//				if (pawn.Downed)
//				{
//					
//				}
//				else
//				{
//					
//				}
//			}
		}
		
		public void MakePawnKnowHisPackmates()
		{
			packPawns = pawn.relations.RelatedPawns.ToList();
			if (debug)
			{
				Log.Message(pawn + " try to introduce his packmates eachother.");
				
				string pawns = "";
				for (int i = 0; i < packPawns.Count; i++)
				{
					if (pawns == "") pawns = pawns + packPawns[i];
					else pawns = pawns + ", " + packPawns[i];
				}
				Log.Message(pawn + " relatedPawns = " + pawns);
			}
			
			for (int i = 0; i < packPawns.Count; i++)
			{
				Pawn pawn2 = packPawns[i];
				
				if (pawn2.DestroyedOrNull() || pawn2.Dead) continue;
				
				if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Packmate, pawn2) && 
				    !pawn.relations.DirectRelationExists(PawnRelationDefOf.PackLeader, pawn2))
				{
					for (int j = 0; j < packPawns.Count; j++)
					{
						Pawn pawn3 = packPawns[j];
						
						if (pawn3.DestroyedOrNull() || pawn3.Dead || pawn3 == pawn2) continue;
						
						if (!pawn2.relations.DirectRelationExists(PawnRelationDefOf.Packmate, pawn3) && 
				   			!pawn2.relations.DirectRelationExists(PawnRelationDefOf.PackLeader, pawn3))
						{
							if (pawn2.def == ThingDefOf.Avali)
							{
								pawn2.TryAddDirectRelation(pawn3, PawnRelationDefOf.Packmate);
							}
							
							if (pawn3.def == ThingDefOf.Avali)
							{
								pawn3.TryAddDirectRelation(pawn2, PawnRelationDefOf.Packmate);
							}
						}
					}
				}
			}
		}
		
		#region Disabled
//		public void TryMakePackUsingJoinPack()
//		{
//			relatedPawns = pawn.relations.RelatedPawns.ToList();
//			for (int i = 0; i < relatedPawns.Count; i++)
//			{
//				Pawn relatedPawn = relatedPawns[i];
//				
//				if (!relatedPawn.DestroyedOrNull() && !relatedPawn.Dead && 
//				    relatedPawn.def == ThingDefOf.Avali &&
//				    pawn.HavePackRelation(relatedPawn) && !relatedPawn.HavePackRelation(pawn))
//        {
//					// Join two packs together by ensuring all members of pack 1 are pack members with all members of pack 2.
//					
//					if (debug) Log.Message(pawn + " restoring pack relation with " + relatedPawn);
//					
//					JoinPacks(pawn, relatedPawn);
//				}
//			}
//			
//			if (pawnsInPack < maxPawnsInPack)
//      {
//				List<Pawn> freeHumanlikesOfPawnFaction = pawn.Map.mapPawns.FreeHumanlikesOfFaction(pawn.Faction).ToList();
//				
//				for (int i = 0; i < freeHumanlikesOfPawnFaction.Count; i++)
//				{
//					Pawn factionPawn = freeHumanlikesOfPawnFaction[i];
//					
//					if (factionPawn != pawn && factionPawn.Awake() && 
//        	    !factionPawn.IsSlave() && !pawn.HavePackRelation(factionPawn) && 
//        	    AvaliUtility.BothPawnsReproductiveOrNotReproductive(pawn, factionPawn) && 
//        	    (pawnPosInQueue == 0 || (pawn.relations.OpinionOf(factionPawn) >= pawnOpinionNeeded && factionPawn.relations.OpinionOf(pawn) >= pawnOpinionNeeded)) && 
//        	    Hediff_AvaliBiology.TotalPawnsInPack(factionPawn) < maxPawnsInPack)
//          {
//            if (debug) Log.Message(pawn + " try to join pack with " + factionPawn);
//            
//            pawn.TryAddDirectRelation(factionPawn, PawnRelationDefOf.Packmate);
//            JoinPacks(pawn, factionPawn);
//            pawnsInPack++;
//            CheckPawnsInPack(relatedPawns);
//            
//            if (pawnsInPack >= maxPawnsInPack) return;
//          }
//				}
//      }
//		}
//		
//		
//		// Join two packs together by ensuring all members of pack 1 are pack members with all members of pack 2.
//    public static void JoinPacks(Pawn pawn1, Pawn pawn2)
//    {
//    	List<Pawn> pack1 = GetPawnsInPack(pawn1);
//      List<Pawn> pack2 = GetPawnsInPack(pawn2);
//
//      TryAddSinglePackRelationshipReciprocal(pawn1, pawn2);
//      
//      for (int j = 0; j < pack2.Count; j++)
//      {
//      	Pawn pack2Pawn = pack2[j];
//      	TryAddSinglePackRelationshipReciprocal(pawn1, pack2Pawn);
//      }
//      
//      for (int j = 0; j < pack1.Count; j++)
//      {
//      	Pawn pack1Pawn = pack1[j];
//      	TryAddSinglePackRelationshipReciprocal(pawn2, pack1Pawn);
//      	
//      	for (int k = 0; k < pack2.Count; k++)
//      	{
//      		Pawn pack2Pawn = pack2[k];
//      		TryAddSinglePackRelationshipReciprocal(pack1Pawn, pack2Pawn);
//      	}
//      }
//    }
//		
//		// Get all pawns in the pawn's pack other than themselves.
//    public static List<Pawn> GetPawnsInPack(Pawn pawn)
//    {
//    	return pawn.relations.RelatedPawns.Where(relatedPawn => pawn.HavePackRelation(relatedPawn)).ToList();
//    }
//
//    // Make pawn 2 a pack member according to pawn 1 if alive.
//    public static void TryAddSinglePackRelationship(Pawn pawn1, Pawn pawn2)
//    {
//      if (!pawn2.DestroyedOrNull() && !pawn2.Dead && pawn1 != pawn2)
//      {
//        if (pawn1.def == ThingDefOf.Avali && !pawn1.HavePackRelation(pawn2))
//        {
//            pawn1.TryAddDirectRelation(pawn2, PawnRelationDefOf.Packmate);
//        }
//      }
//    }
//
//    // Make pawn 1 and pawn 2 pack members of each other if alive.
//    public static void TryAddSinglePackRelationshipReciprocal(Pawn pawn1, Pawn pawn2)
//    {
//      TryAddSinglePackRelationship(pawn1, pawn2);
//      TryAddSinglePackRelationship(pawn2, pawn1);
//    }
		#endregion Disabled
		
		public static int TotalPawnsInPack(Pawn pawn2)
		{
			int totalPawnsInPack = 1;
			
			// check count of related to pawn2 pawns
			List<Pawn> pawn2RelatedPawns = pawn2.relations.RelatedPawns.ToList();
			for (int i = 0; i < pawn2RelatedPawns.Count; i++)
			{
				Pawn pawn3 = pawn2RelatedPawns[i];
				
				if (pawn2.HavePackRelation(pawn3) || pawn3.HavePackRelation(pawn2))
				{
					totalPawnsInPack++;
				}
			}
			
			//if (debug) Log.Message(pawn2 + " pawns in pack = " + totalPawnsInPack);
			return totalPawnsInPack;
		}
		
		public void TryUpdatePackLossStage()
		{
			if (!immuneToPackLoss)
			{
				packLossStage = pawn.TryAddPackLossThought(packPawns, packLossStage);
			}
		}
		
		public void UpdateResists()
		{
			float envResist = 0;
			List<Apparel> wornApparels = pawn.apparel.WornApparel;
			envResist += pawn.GetStatValue(StatDef.Named("ArmorRating_Enviromental"));
			for (int i = 0; i < wornApparels.Count; i++)
			{
				Apparel wornApparel = wornApparels[i];
				envResist += wornApparel.GetStatValue(StatDef.Named("ArmorRating_Enviromental"));
			}
			//Log.Message(pawn + " ArmorRating_Enviromental = " + envResist);
			
			pawn.TryMakeImmune(envResist, 1, HediffDefOf.ToxicImmunity);
			
			Hediff hediff_OxygenAtmosphere = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == HediffDefOf.OxygenAtmosphere);
			if (envResist < minEnvResist)
			{
				if (pawn.IsColonist || pawn.IsPrisoner)
				{
					if (hediff_OxygenAtmosphere != null) hediff_OxygenAtmosphere.Severity += 0.001f;
					else
					{
						pawn.health.AddHediff(HediffDefOf.OxygenAtmosphere);
						//Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == HediffDefOf.OxygenAtmosphere);
						//float severity = Rand.Range(10, 100);
						//hediff.Severity = severity / 1000;
					}
				}
			}
			else
			{
				if (hediff_OxygenAtmosphere != null)
				{
					hediff_OxygenAtmosphere.Severity -= 0.001f;
					if (hediff_OxygenAtmosphere.Severity <= 0.001f) pawn.health.RemoveHediff(hediff_OxygenAtmosphere);
				}
			}
		}
		
		public void CheckBodyparts()
		{
			int eyes = 2;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff hediff = hediffs[i];
				if (hediff != null && hediff.Part != null)
				{
					if (hediff.Part.def != null && hediff.Part.def == BodyPartDefOf.Eye)
					{
						if (hediff.def == RimWorld.HediffDefOf.MissingBodyPart) eyes--;
						else if (hediff.CurStage != null && hediff.CurStage.capMods != null)
						{
							List<PawnCapacityModifier> capMods = hediff.CurStage.capMods;
							for (int j = 0; j < capMods.Count; j++)
							{
								PawnCapacityModifier capMod = capMods[j];
								if (capMod != null && capMod.offset <= -0.5f)
								{
									eyes--;
								}
							}
						}
					}
				}
				
				if (eyes <= 0) break;
			}
			
			if (eyes == 0) this.Severity = 0.001f; // 0
			else if (eyes == 1) this.Severity = 0.25f; // -0.25
			else this.Severity = 0.5f; // -0.5
		}
		
		public void UpdateRaceSpecificThoughts()
		{
			for (int i = 0; i < pawn.needs.mood.thoughts.memories.Memories.Count; i++)
			{
				Thought_Memory thought = pawn.needs.mood.thoughts.memories.Memories[i];
				if (thought.CurStage == null && !thought.ShouldDiscard) pawn.needs.mood.thoughts.memories.RemoveMemory(thought);
			}
			
			pawn.Thought_AvaliSleepingRoomImpressiveness();
			pawn.Thought_AvaliPackSleepingRoomRelations(maxPawnsInPack);
			pawn.Thought_SleepDisturbedAvali(25);
			
			pawn.needs.mood.thoughts.memories.ExposeData();
		}
	}
}
