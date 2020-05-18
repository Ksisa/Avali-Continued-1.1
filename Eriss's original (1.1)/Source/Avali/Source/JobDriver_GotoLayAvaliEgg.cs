using System.Collections.Generic;
using System;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_GotoLayAvaliEgg : JobDriver
	{	
		//private TargetIndex fatherInd = TargetIndex.A;
		
		private TargetIndex bedInd = TargetIndex.B;
		
		private Job layEgg;
		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (TargetB.IsValid)
			{
				yield return Toils_Goto.GotoThing(bedInd, PathEndMode.OnCell);
				yield return Toils_General.Do(delegate
				{
				    layEgg = new Job(JobDefOf.LayAvaliEgg, TargetA, TargetB);
				});
			}
			else
			{
				layEgg = new Job(JobDefOf.LayAvaliEgg, TargetA);
			}
			
			yield return Toils_General.Do(delegate
			{
			    //Log.Message(pawn + " try to start job LayAvaliEgg");
				//Log.Message(pawn + " egg.father = " + TargetA);
				//Log.Message(pawn + " bed = " + TargetB);
			    pawn.jobs.StartJob(layEgg, JobCondition.InterruptForced, null, false, true, null, null, false);
			});
			
			yield break;
		}
	}
}
