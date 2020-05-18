using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace Avali
{
	public class ThoughtWorker_AvaliSmell : ThoughtWorker
	{
		private const int rangeInCells = 3;
		private const int maxAvaliPawns = 6;
		
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.def == ThingDefOf.Avali) return ThoughtState.Inactive;
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff hediff = hediffs[i];
				if (hediff != null && hediff.Part != null && hediff.Part.Label == "nose")
				{
					if (hediff.def == RimWorld.HediffDefOf.MissingBodyPart) return ThoughtState.Inactive;
				}
			}
			
			List<Thing> avaliPawns = new List<Thing>();
			Room room = p.GetRoom(RegionType.Set_Passable);
			if (room.PsychologicallyOutdoors) return ThoughtState.Inactive;
			List<Pawn> freeColonistsAndPrisoners = p.GetRoom(RegionType.Set_Passable).Map.mapPawns.FreeColonistsAndPrisoners.ToList();
			for (int i = 0; i < freeColonistsAndPrisoners.Count; i++)
			{
				Thing freeColonistOrPrisoner = freeColonistsAndPrisoners[i] as Thing;
				if (freeColonistOrPrisoner != null) avaliPawns.Add(freeColonistOrPrisoner);
			}
			
			if (avaliPawns.Count == 0) return ThoughtState.Inactive;
			
			int avaliPawnsInRange = AvaliUtility.FindAllThingsOnMapAtRange(p, ThingDefOf.Avali, null, avaliPawns, rangeInCells, maxAvaliPawns, false, false).Count;
			if (avaliPawnsInRange > 0)
			{
				avaliPawnsInRange--;
				if (avaliPawnsInRange > 5) avaliPawnsInRange = 5;
				return ThoughtState.ActiveAtStage(avaliPawnsInRange);
			}
			
			return ThoughtState.Inactive;
		}
	}
}
