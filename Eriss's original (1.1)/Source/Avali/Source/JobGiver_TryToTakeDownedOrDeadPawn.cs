using System;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class JobGiver_TryToTakeDownedOrDeadPawn : ThinkNode_JobGiver
	{
		public float maxSearchDistance = 15;
		
		protected override Job TryGiveJob(Pawn pawn)
		{
			//if (pawn.def != ThingDefOf.Avali) return null;
			
			if (pawn.IsColonist || pawn.MentalState == null) return null;
			
			if (pawn.MentalState.def == null || pawn.MentalState.def != MentalStateDefOf.PanicFlee) return null;
			
			IntVec3 exitCell;
			if (!RCellFinder.TryFindBestExitSpot(pawn, out exitCell, TraverseMode.ByPawn))
			{
				return null;
			}
			
			Hediff_AvaliBiology avaliBiologyHediff = pawn.health.hediffSet.GetHediffs<Hediff_AvaliBiology>().First();
			
			Predicate<Thing> validator = null;
			Pawn pawn2 = null;
			
			if (avaliBiologyHediff != null)
			{
				// First priority: find any downed humanlike what is related to this pawn
				validator = delegate(Thing t)
				{
					Pawn pawn3 = t as Pawn;
					
					return  pawn3.RaceProps.Humanlike &&
									pawn3.PawnListed(avaliBiologyHediff.packPawns) &&
									pawn3.Downed &&
									pawn.CanReserve(pawn3);
				};
				pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (pawn2 != null)
				{
					return new Job(JobDefOf.TakeDownedOrDeadPawn)
					{
						targetA = pawn2,
						targetB = exitCell,
						count = 1
					};
				}
			}
			
			// Second priority: find any downed humanlike what is consist in pawn faction
			validator = delegate(Thing t)
			{
				Pawn pawn3 = t as Pawn;
				
				return  pawn3.RaceProps.Humanlike &&
						pawn3.Faction == pawn.Faction &&
						pawn3.Downed &&
						pawn.CanReserve(pawn3);
			};
			pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxSearchDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			if (pawn2 != null)
			{
				return new Job(JobDefOf.TakeDownedOrDeadPawn)
				{
					targetA = pawn2,
					targetB = exitCell,
					count = 1
				};
			}
			
			// Third priority: find any dead humanlike what was related to this pawn
			validator = delegate(Thing t)
			{
				Corpse corpse = t as Corpse;
				
				return  corpse != null && corpse.InnerPawn != null &&
								corpse.InnerPawn.RaceProps.Humanlike &&
								corpse.InnerPawn.PawnListed(avaliBiologyHediff.packPawns) &&
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
			}
			
			return null;
		}
	}
}
