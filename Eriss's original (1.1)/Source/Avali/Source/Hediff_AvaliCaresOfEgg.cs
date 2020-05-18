using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Avali
{
	public class Hediff_AvaliCaresOfEgg : HediffWithComps
	{
		private const int pawnStateCheckInterval = 200000;
		
		public override void Tick()
		{
			base.Tick();
			
			if (!pawn.IsColonistPlayerControlled)
			{
				return;
			}
			
			if (pawn.IsHashIntervalTick(pawnStateCheckInterval))
			{
				Predicate<Thing> validator = delegate(Thing t)
				{
					Thing egg = t as Thing;
					return egg.def.defName == ThingDefOf.AvaliEgg.ToString() &&
					(egg.TryGetComp<CompHatcher>().hatcheeParent == pawn ||
					 egg.TryGetComp<CompHatcher>().otherParent == pawn);
				};
				Thing egg2 = (Thing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(pawn.def), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (egg2 != null)
				{
					Pawn mother = egg2.TryGetComp<CompHatcher>().hatcheeParent;
					if (mother == null)
					{
						mother = egg2.TryGetComp<CompHatcher>().otherParent;
						if (mother == null)
						{
							pawn.health.RemoveHediff(this);
							return;
						}
					}
					if (mother.CurrentBed() == null) return;
					
					//Log.Message("Giving CheckAvaliEgg job to " + pawn);
					Job newJob = new Job(JobDefOf.CheckAvaliEgg, egg2, mother.CurrentBed(), mother.CurrentBed().OccupiedRect().CenterCell);
					pawn.jobs.StartJob(newJob, JobCondition.InterruptForced, null, true, false, null, null, false);
				}
			}
		}
	}
}
