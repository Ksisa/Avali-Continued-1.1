using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class JoyGiver_SingKaraoke : JoyGiver_InteractBuilding
	{
		protected override Job TryGivePlayJob(Pawn pawn, Thing thing)
		{
			CompPowerTrader compPowerTrader = (CompPowerTrader)thing.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null)
			{
				if (compPowerTrader.PowerNet.CurrentStoredEnergy() <= 0 || thing.IsBrokenDown() || !FlickUtility.WantsToBeOn(thing))
				{
					return null;
				}
			}
			
			Room room = pawn.GetRoom(RegionType.Set_Passable);
			if (room != null)
			{
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
								return null;
							}
						}
					}
				}
			}
			
			List<IntVec3> thingWatchCells = WatchBuildingUtility.CalculateWatchCells(thing.def, thing.Position, thing.Rotation, thing.Map).ToList();
			for (int i = 0; i < thingWatchCells.Count(); i++)
			{
				int cellRand = Rand.RangeInclusive(0, thingWatchCells.Count());
				IntVec3 cell = thingWatchCells[cellRand];
				if (cell.Standable(pawn.Map))
				{
					//Log.Message(pawn + " watch cell = " + cellRand + " of " + thingWatchCells.Count());
					return new Job(def.jobDef, thing, cell);
				}
			}
			
			return null;
		}
	}
}
