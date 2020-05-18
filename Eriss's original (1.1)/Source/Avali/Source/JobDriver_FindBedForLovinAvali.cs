using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_FindBedForLovinAvali : JobDriver
	{
		private const int ticksBeforeMote = 200;
		
		private TargetIndex PartnerInd = TargetIndex.A;
		
		private Pawn partner
		{
			get
			{
				return (Pawn)((Thing)job.GetTarget(PartnerInd));
			}
		}
		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(partner, job, 1, -1, null);
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			//this.FailOnDespawnedOrNull(BedInd);
			this.FailOnDespawnedOrNull(PartnerInd);
			this.FailOn(() => !partner.health.capacities.CanBeAwake);
			
			Building partnerCellBuilding = partner.Position.GetFirstBuilding(Map);
			if (partnerCellBuilding == null || partnerCellBuilding.GetStatValue(StatDefOf.Comfort) < 0.5f)
			{
				Room room = pawn.GetRoom(RegionType.Set_Passable);
				if (room == null) yield break;
				AvaliUtility.FindAllThingsOnMapAtRange(pawn, null, typeof(Pawn), room.ContainedAndAdjacentThings, 20, 9999, true, true);
				
				/*
				Predicate<Thing> validator = delegate(Thing t)
				{
					Thing building2 = t as Building;
					
					return (building2.def.thingClass == typeof(Building_Bed) || building2.def.building.isSittable) && pawn.CanReserve(building2, 1, -1, null, false);
				};
				*/
				//Thing thing = (Thing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(thingDef), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				
				
				
				//List<Thing> buildingsInRoom = AvaliUtility.FindAllThingsOnMapAtRange(pawn, null, typeof(Building), room.ContainedAndAdjacentThings, 20, 9999, true, true);
				
				
				
				/*if (room.PsychologicallyOutdoors)
				{
					
				}*/
			}
			
			yield break;
		}
	}
}
