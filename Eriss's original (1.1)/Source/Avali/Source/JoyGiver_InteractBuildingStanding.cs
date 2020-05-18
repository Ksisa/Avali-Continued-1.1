using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class JoyGiver_InteractBuildingStanding : JoyGiver_InteractBuilding
	{
		protected override Job TryGivePlayJob(Pawn pawn, Thing thing)
		{
			if (!thing.InteractionCell.Impassable(thing.Map) && !thing.IsForbidden(pawn) && !thing.InteractionCell.IsForbidden(pawn) && !pawn.Map.pawnDestinationReservationManager.IsReserved(thing.InteractionCell))
			{
				return new Job(def.jobDef, thing, thing.InteractionCell);
			}
			return null;
		}
	}
}
