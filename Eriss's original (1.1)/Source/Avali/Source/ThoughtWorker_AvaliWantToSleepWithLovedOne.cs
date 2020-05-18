using System;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Avali
{
	public class ThoughtWorker_AvaliWantToSleepWithLovedOne : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
			if (directPawnRelation == null)
			{
				return false;
			}
			if (!directPawnRelation.otherPawn.IsColonist || directPawnRelation.otherPawn.IsWorldPawn() || !directPawnRelation.otherPawn.relations.everSeenByPlayer)
			{
				return false;
			}
			if (p.relations.OpinionOf(directPawnRelation.otherPawn) <= 0)
			{
				return false;
			}
			
			Building_Bed bed = p.ownership.OwnedBed;
			if (bed != null)
			{
				Room bedRoom = bed.GetRoom(RegionType.Set_Passable);
				if (bedRoom != null)
				{
					if (bedRoom.Role == RoomRoleDefOf.Barracks || bedRoom.Role == RoomRoleDefOf.Bedroom)
					{
						List<Thing> containedAndAdjacentThings = bedRoom.ContainedAndAdjacentThings;
						for (int i = 0; i < containedAndAdjacentThings.Count; i++)
						{
							Thing thing = containedAndAdjacentThings[i];
							Building_Bed otherBed = thing as Building_Bed;
							if (otherBed != null)
							{
								for (int j = 0; j < otherBed.OwnersForReading.Count; j++)
								{
									Pawn owner = otherBed.OwnersForReading[j];
									if (owner == directPawnRelation.otherPawn) return false;
								}
							}
						}
					}
				}
			}
			
			return true;
		}
	}
}
