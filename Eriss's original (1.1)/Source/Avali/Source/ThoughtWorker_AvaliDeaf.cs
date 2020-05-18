using System;
using Verse;
using RimWorld;

namespace Avali
{
	public class ThoughtWorker_AvaliDeaf : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.def != ThingDefOf.Avali) return false;
			if (p.health.capacities.GetLevel(PawnCapacityDefOf.Hearing) > 0) return false;
			
			return true;
		}
	}
}
