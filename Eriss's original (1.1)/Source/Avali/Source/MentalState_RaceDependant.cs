using System;
using System.Linq;
using Verse.AI;

namespace Avali
{
	public class MentalState_RaceDependant : MentalState
	{
		public override void PreStart()
		{
			if (pawn.def != ThingDefOf.Avali)
			{
				SkipMentalState();
				return;
			}
		}
		
		public void SkipMentalState()
		{
			if (!pawn.Dead)
			{
				pawn.mindState.mentalStateHandler.Reset();
			}
			if (pawn.Spawned)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
			}
		}
	}
}
