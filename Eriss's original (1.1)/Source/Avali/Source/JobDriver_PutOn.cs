using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_PutOn : JobDriver
	{
		private TargetIndex apparelInd = TargetIndex.A;
		private TargetIndex anotherPawnInd = TargetIndex.B;
		private TargetIndex pawnCellInd = TargetIndex.C;
		
		private Apparel apparel
		{
			get
			{
				return (Apparel)((Thing)job.GetTarget(apparelInd));
			}
		}
		
		private Pawn anotherPawn
		{
			get
			{
				return (Pawn)((Thing)job.GetTarget(anotherPawnInd));
			}
		}
		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(apparel, job, 1, -1, null) && pawn.Reserve(anotherPawn, job, 1, -1, null);
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(apparelInd);
			this.FailOnDespawnedOrNull(anotherPawnInd);
			this.FailOnBurningImmobile(anotherPawnInd);
			
			yield return Toils_Goto.GotoThing(apparelInd, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(apparelInd);
			yield return Toils_Misc.SetForbidden(apparelInd, false);
			yield return Toils_Haul.StartCarryThing(apparelInd, false, false, false);
			yield return Toils_Goto.GotoThing(anotherPawnInd, PathEndMode.OnCell).FailOnAggroMentalState(anotherPawnInd);
			
			Toil wait = Toils_General.Wait(60, anotherPawnInd).FailOnDespawnedOrNull(anotherPawnInd).WithProgressBarToilDelay(anotherPawnInd, true, -0.5f);
			wait.initAction = delegate
			{
				if (anotherPawn.Awake() && !anotherPawn.InBed())
				{
					Job newJob = new Job(RimWorld.JobDefOf.Wait);
					anotherPawn.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false);
				}
			};
			yield return wait;
			
			Toil place = Toils_Haul.PlaceHauledThingInCell(pawnCellInd, null, false);
			place.AddFinishAction(delegate
			{
				anotherPawn.apparel.Wear(apparel, true, false);
				anotherPawn.jobs.EndCurrentJob(JobCondition.Succeeded);
			});
			yield return place;
			
			yield break;
		}
	}
}
