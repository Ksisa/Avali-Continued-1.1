using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public static class AvaliUtility
	{
		public static bool BothPawnsReproductiveOrNotReproductive(Pawn pawn, Pawn pawn2) // kits form packs with kits and adults form packs with adults
		{
			if ((pawn.ageTracker.CurLifeStage.reproductive && pawn2.ageTracker.CurLifeStage.reproductive) || 
				(!pawn.ageTracker.CurLifeStage.reproductive && !pawn2.ageTracker.CurLifeStage.reproductive))
			{
				return true;
			}
			
			return false;
		}
		
		public static bool HavePackRelation(this Pawn pawn, Pawn pawn2)
		{
			if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Packmate, pawn2) || 
				pawn.relations.DirectRelationExists(PawnRelationDefOf.PackLeader, pawn2))
			{
				return true;
			}
			
			return false;
		}
		
		public static bool HaveLoveRelation(this Pawn pawn, Pawn pawn2)
		{
			if (pawn.relations.DirectRelationExists(RimWorld.PawnRelationDefOf.Lover, pawn2) ||
				pawn.relations.DirectRelationExists(RimWorld.PawnRelationDefOf.Spouse, pawn2) ||
				pawn.relations.DirectRelationExists(RimWorld.PawnRelationDefOf.Fiance, pawn2))
			{
				return true;
			}
			
			return false;
		}
		
		public static bool PawnListed(this Pawn pawn, List<Pawn> relatedPawns)
		{
			for (int i = 0; i < relatedPawns.Count; i++)
			{
				Pawn pawn2 = relatedPawns[i];
				if (pawn2 != null && pawn2 == pawn)
				{
					return true;
				}
			}
			
			return false;
		}
		
		public static int GetSkillLevel(this Pawn pawn, SkillDef skillDef)
		{
			if (pawn.skills == null) return 0;
			SkillRecord skill = pawn.skills.GetSkill(skillDef);
			
			if (skill != null && !skill.TotallyDisabled && skill.Level > 0)
			{
				return skill.Level;
			}
			
			return 0;
		}
		
		public static int GetTotalSkillLevel(this Pawn pawn)
		{
			if (pawn.skills == null) return 0;
			
			int totalSkillCount = 0;
			List<SkillRecord> pawnSkills = new List<SkillRecord>();
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Animals));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Artistic));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Construction));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Cooking));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Crafting));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Plants));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Intellectual));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Medicine));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Melee));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Mining));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Shooting));
			pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Social));
			
			for (int i = 0; i < pawnSkills.Count; i++)
			{
				SkillRecord skill = pawnSkills[i];
				if (skill != null && !skill.TotallyDisabled && skill.Level > 0) totalSkillCount += skill.Level;
			}
			
			return totalSkillCount;
		}
		
		public static SkillRecord GetHighestPackSkill(this Pawn thisPawn, List<Pawn> pack)
		{
			pack.Add(thisPawn);
			SkillRecord highestSkill = null;
			int highestSkillLevel = 0;
			for (int i = 0; i < pack.Count; i++)
			{
				Pawn pawn = pack[i];
				if (pawn.skills == null || (thisPawn != pawn && !thisPawn.HavePackRelation(pawn))) continue;
				
				List<SkillRecord> pawnSkills = new List<SkillRecord>();
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Animals));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Artistic));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Construction));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Cooking));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Crafting));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Plants));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Intellectual));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Medicine));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Melee));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Mining));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Shooting));
				pawnSkills.Add(pawn.skills.GetSkill(SkillDefOf.Social));
				
				for (int j = 0; j < pawnSkills.Count; j++)
				{
					SkillRecord skill = pawnSkills[j];
					if (skill != null && !skill.TotallyDisabled && skill.Level > highestSkillLevel)
					{
						highestSkill = skill;
						highestSkillLevel = skill.Level;
					}
				}
			}
			
			return highestSkill;
		}
		
		public static HediffDef TryRemovePackHediffsAndAddPackHediff(this Pawn pawn, HediffDef newPackHediff)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def == newPackHediff) return newPackHediff;
			}
			
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff hediff = hediffs[i];
				if (hediff == null) continue;
				
				if (hediff.def == HediffDefOf.AvaliPackExploration || 
				    hediff.def == HediffDefOf.AvaliPackMilitary || 
				   	hediff.def == HediffDefOf.AvaliPackHunting ||
				 	hediff.def == HediffDefOf.AvaliPackScientific ||
					hediff.def == HediffDefOf.AvaliPackIndustrial ||
					hediff.def == HediffDefOf.AvaliPackArtistical)
				{
					pawn.health.RemoveHediff(hediff);
				}
			}
			
			pawn.health.AddHediff(newPackHediff);
			return newPackHediff;
		}
		
		public static void TryAddDirectRelation(this Pawn pawn1, Pawn pawn2, PawnRelationDef relation)
		{
			if (!pawn1.relations.DirectRelationExists(relation, pawn2))
			{
				pawn1.relations.AddDirectRelation(relation, pawn2);
			}
		}
		
		public static void TryMakeImmune(this Pawn pawn, float minResist, float reqResist, HediffDef immunityHediff)
		{
			Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == immunityHediff);
			if (minResist >= reqResist && hediff == null) 
			{
				pawn.health.AddHediff(immunityHediff);
			}
			else if (minResist < reqResist && hediff != null) 
			{
				pawn.health.RemoveHediff(hediff);
			}
		}
		
		public static List<Thing> FindAllThingsOnMapAtRange(Thing thing, ThingDef thingDef = null, Type thingType = null, List<Thing> thingsList = null, float range = float.MaxValue, int maxOutputListThings = int.MaxValue, bool shouldBeReachable = false, bool shouldBeReservable = false)
		{
			List<Thing> outputThingsList = new List<Thing>();
			IntVec3 thingPosition = thing.Position;
			
			if (range != float.MaxValue) range = range / 100;
			if (thingsList == null) thingsList = thing.Map.spawnedThings.ToList();
			
			//Log.Message (thing + " range = " + range);
			//Log.Message (thing + ": things in list (pre): " + thingsList.Count);
			
			for (int i = 0; i < thingsList.Count; i++)
			{
				Thing thingInList = thingsList[i];
				if (thingInList.def == null) continue;
				
				if (thingType == null)
				{
					if (thingDef == null) break;
					if (thingInList.def != thingDef) continue;
				}
				else
				{
					if (thingInList.def.thingClass != thingType) continue;
				}
				
				if (shouldBeReachable || shouldBeReservable)
				{
					Pawn pawn = thing as Pawn;
					if (pawn != null)
					{
						if (shouldBeReachable)
						{
							if (!pawn.CanReach(thingInList, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn)) continue;
						}
						if (shouldBeReservable)
						{
							if (!pawn.CanReserve(thingInList, 1, -1, null, false)) continue;
						}
					}
				}
				
				IntVec3 thingInListPosition = thingInList.Position;
				
				if (range != float.MaxValue)
				{
					double distToThing = Math.Sqrt(Math.Pow(thingInListPosition.x - thingPosition.x, 2) + Math.Pow(thingInListPosition.z - thingPosition.z, 2));
					//Log.Message (thing + ": distance to " + thingInList + " = " + distToThing);
					if (distToThing <= range)
					{
						outputThingsList.Add(thingInList);
					}
				}
				else
				{
					outputThingsList.Add(thingInList);
				}
				
				if (outputThingsList.Count > maxOutputListThings) break;
			}
			
			List<Thing> outputThingsListSorted = new List<Thing>();
			while (outputThingsList.Count > 0)
			{
				double distToClosestThing = double.MaxValue;
				Thing closestThing = outputThingsList.First();
				for (int i = 0; i < outputThingsList.Count; i++)
				{
					Thing outputThing = outputThingsList[i];
					IntVec3 outputThingPosition = outputThing.Position;
					double distToThing = Math.Sqrt(Math.Pow(outputThingPosition.x - thingPosition.x, 2) + Math.Pow(outputThingPosition.z - thingPosition.z, 2));
					if (distToThing < distToClosestThing)
					{
						distToClosestThing = distToThing;
						closestThing = outputThing;
					}
				}
				outputThingsListSorted.Add(closestThing);
				outputThingsList.Remove(closestThing);
			}
			
			//Log.Message (thing + ": things in list (post): " + thingsList.Count);
			return outputThingsListSorted;
		}
		
		public static bool IsSlave(this Pawn pawn)
		{
			if (!pawn.IsColonist && pawn.kindDef == PawnKindDef.Named("AvaliSlave"))
			{
				return true;
			}
			
			return false;
		}
		
		public static bool IsPowered(this Thing building)
		{
			CompPowerTrader compPowerTrader = building.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null)
			{
				if (!compPowerTrader.PowerOn) return false;
			}
			
			return true;
		}
		
		public static bool IsOnAndNotBrokenDown(this ThingWithComps thing)
		{
			if (FlickUtility.WantsToBeOn(thing) && !thing.IsBrokenDown()) return true;
			return false;
		}
		
		public static Pawn GetUserPawn(this ThingWithComps thing)
		{
			List<Thing> thingList = thing.InteractionCell.GetThingList(thing.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null)
				{
					//Log.Message(parent + " pawn = " + pawn);
					Job pawnJob = pawn.CurJob;
					//Log.Message(parent + " pawnJob = " + pawnJob);
					if (pawnJob != null)
					{
						LocalTargetInfo CurJobTargetA = pawnJob.targetA.Thing;
						LocalTargetInfo CurJobTargetB = pawnJob.targetB.Thing;
						LocalTargetInfo CurJobTargetC = pawnJob.targetC.Thing;
						/*Log.Message(pawn +
							          " CurJobTargetA: " + CurJobTargetA +
							          ", CurJobTargetB: " + CurJobTargetB +
							          ", CurJobTargetC: " + CurJobTargetC +
							          ". WorkTable: " + parent);*/
						if (CurJobTargetA == thing || CurJobTargetB == thing || CurJobTargetC == thing)
						{
							return pawn;
						}
					}
				}
			}
			
			return null;
		}
		
		public static Thing FindClosestSittableUnoccupiedThing(Pawn pawn, ThingDef thingDef, float maxSearchDistance = 9999)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				Thing building2 = t as Building;
				
				return building2.def.building.isSittable && pawn.CanReserve(building2, 1, -1, null, false);
			};
			Thing thing = (Thing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(thingDef), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			return thing;
		}
		
		public static Thing FindClosestUnoccupiedThing(Pawn pawn, ThingDef thingDef, float maxSearchDistance = 9999, bool shouldBePowered = false)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				Thing building2 = t as Building;
				if (shouldBePowered) shouldBePowered = building2.IsPowered();
				else shouldBePowered = true;
				
				return pawn.CanReserve(building2, 1, -1, null, false) && shouldBePowered;
			};
			Thing thing = (Thing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(thingDef), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			return thing;
		}
		
		public static Thing SpecifiedThingAtCellWithDefName(List<Thing> thingsAtCell, string thingDefName)
		{
			for (int i = 0; i < thingsAtCell.Count; i++)
			{
				Thing thing = thingsAtCell[i];
				if (thing.def != null && thing.def.defName == thingDefName) return thing;
			}
			
			return null;
		}
		
		public static bool LinkedToRequredFacilities(this Thing building, List<ThingDef> requredFacilities) // returns linkedFacilities.Count = 0 even when have linked facilities. Need fix
		{
			if (requredFacilities.Count == 0) return true;
			//Log.Message(parent + " Props.requredFacilities = " + Props.requredFacilities);
			
			CompAffectedByFacilities compAffectedByFacilities = building.TryGetComp<CompAffectedByFacilities>();
			//Log.Message(parent + " compAffectedByFacilities = " + compAffectedByFacilities);
			if (compAffectedByFacilities == null) return false;
			
			List<Thing> linkedFacilities = compAffectedByFacilities.LinkedFacilitiesListForReading;
			//Log.Message(parent + " pre linkedFacilities.Count = " + linkedFacilities.Count);
			if (linkedFacilities.Count == 0) return false;
			
			for (int i = 0; i < requredFacilities.Count; i++)
			{
				ThingDef requredFacility = requredFacilities[i];
				for (int j = 0; j < linkedFacilities.Count; j++)
				{
					Thing linkedFacility = linkedFacilities[j];
					if (requredFacility == linkedFacility.def)
					{
						linkedFacilities.Remove(linkedFacility);
					}
				}
			}
			
			//Log.Message(parent + " post linkedFacilities.Count = " + linkedFacilities.Count);
			if (linkedFacilities.Count == 0) return true;
			return false;
		}
		
		public static Building BuildingInPosition(Map map, IntVec3 position, BuildableDef buildableDef = null)
		{
			List<Thing> things = map.thingGrid.ThingsAt(position).ToList();
			for (int i = 0; i < things.Count(); i++)
			{
				Building building = things[i] as Building;
				if (building != null)
				{
					if (buildableDef != null && building.def.altitudeLayer == buildableDef.altitudeLayer) return building;
					if (building.def.holdsRoof ||
					    building.def.blockLight)
					{
						return building;
					}
				}
			}
			return null;
		}
		
		public static List<Pawn> FindPawnsForLovinInRoom(this Pawn pawn, Room room, bool giveThought = false)
		{
			const int minOpinion = 30;
			
			if (room.PsychologicallyOutdoors) return null;
			List<Pawn> pawnsInRoom = room.Map.mapPawns.AllPawnsSpawned;
			bool pawnHasGayTrait = pawn.story.traits.HasTrait(RimWorld.TraitDefOf.Gay);
			for (int i = 0; i < pawnsInRoom.Count; i++)
			{
				Pawn pawnInRoom = pawnsInRoom[i];
				if (pawnInRoom.Downed || pawnInRoom.Dead || pawnInRoom.health.HasHediffsNeedingTend()) return null;
				
				if (pawnInRoom.RaceProps.Humanlike && 
				    (!pawn.HavePackRelation(pawnInRoom) && !pawn.HaveLoveRelation(pawnInRoom)) ||
				    (pawn.relations.OpinionOf(pawnInRoom) < minOpinion && pawnInRoom.relations.OpinionOf(pawn) < minOpinion))
				{
					if (giveThought) pawn.AddMemoryOfDef(ThoughtDefOf.AvaliNeedPrivacy);
					return null;
				}
				
				if (pawnInRoom.Drafted)
				{
					pawnsInRoom.Remove(pawnInRoom);
					continue;
				}
				
				// traits
				bool pawnInRoomHasGayTrait = pawnInRoom.story.traits.HasTrait(RimWorld.TraitDefOf.Gay);
				if (pawn.gender == pawnInRoom.gender)
    		{
					if (!pawnHasGayTrait || !pawnInRoomHasGayTrait) pawnsInRoom.Remove(pawnInRoom);
    		}
			}
			
			return pawnsInRoom;
		}
	}
}
