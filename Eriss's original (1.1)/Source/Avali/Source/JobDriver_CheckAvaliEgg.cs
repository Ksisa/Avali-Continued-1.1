using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_CheckAvaliEgg : JobDriver
	{
		private TargetIndex eggInd = TargetIndex.A;
		
		private TargetIndex bedInd = TargetIndex.B;
		
		private TargetIndex bedCellInd = TargetIndex.C;
		
		private Thing egg
		{
			get
			{
				return (Thing)((Thing)job.GetTarget(eggInd));
			}
		}
		
		private Building bed
		{
			get
			{
				return (Building)((Building)job.GetTarget(bedInd));
			}
		}
		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(egg, job, 1, -1, null) && pawn.Reserve(bed, job, 1, -1, null);
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			bool err = false;
			Toil error = Toils_General.Do(delegate
			{
				Log.Error("Error in Toils_Haul.PlaceHauledThingInCell. Breaking job.");
				Log.Error("eggInd = " + eggInd);
				Log.Error("bedInd = " + bedInd);
				Log.Error("bedCellInd = " + bedCellInd);
				
				err = true;
			});
			
			this.FailOnDestroyedOrNull(eggInd);
      this.FailOnDespawnedOrNull(bedInd);
			
			yield return Toils_Goto.GotoThing(eggInd, PathEndMode.Touch).FailOnDestroyedOrNull(eggInd).FailOnSomeonePhysicallyInteracting(eggInd);
			
			if (!TargetA.Thing.IsForbidden(pawn) && TargetB.IsValid)
			{
				if (TargetA.Cell != TargetB.Cell)
				{
					yield return Toils_Goto.GotoThing(eggInd, PathEndMode.Touch).FailOnDestroyedOrNull(eggInd).FailOnSomeonePhysicallyInteracting(eggInd);
					yield return Toils_Haul.StartCarryThing(eggInd, false, false, false);
					yield return Toils_Goto.GotoThing(bedInd, PathEndMode.Touch).FailOnDespawnedOrNull(bedInd);
					yield return Toils_Haul.PlaceHauledThingInCell(bedCellInd, Toils_Jump.Jump(error), false);
					
					TargetA.Thing.SetForbidden(true, true);
				}
			}
			
			yield return Toils_General.Wait(1000);
			if (err) yield return error;
			yield break;
		}
	}
}
