using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using Verse.Sound;

namespace Avali
{
	public class JobDriver_UseRunningTrack : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null) && pawn.Reserve(job.targetB, job, 1, -1, null);
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			this.EndOnDespawnedOrNull(TargetIndex.B, JobCondition.Incompletable);
			this.FailOnForbidden(TargetIndex.A);
			
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			
			float statValue = TargetThingA.GetStatValue(StatDefOf.JoyGainFactor, true);
			Building building = (Building)pawn.CurJob.targetA.Thing;
			//CompRunningGenerator comp = (CompRunningGenerator)building.GetComp<CompRunningGenerator>();
			//int storedEnergy = 0;
			//int skipTick = 0;
			//int sameValueTimes = 0;
			
			Toil use = new Toil();
			use.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(TargetA.Cell);
				pawn.GainComfortFromCellIfPossible();
				
				building.GetComp<CompMannable>().ManForATick(pawn);
				
				if (TargetC.IsValid)
				{
					JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None, statValue);
					
					//Log.Message(building + " current stored energy = " + (int)comp.PowerNet.CurrentStoredEnergy());
					/*
					if (skipTick < 60)
					{
						//Log.Message(building + " stored energy = " + storedEnergy);
						if ((int)comp.PowerNet.CurrentStoredEnergy() == storedEnergy)
						{
							if (sameValueTimes < 60)
							{
								sameValueTimes++;
							}
							else pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
						}
						else sameValueTimes = 0;
						
						skipTick++;
					}
					else 
					{
						storedEnergy = (int)comp.PowerNet.CurrentStoredEnergy();
						skipTick = 0;
					}
					*/
				}
				else JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, statValue);
			};
			
			if (!TargetC.IsValid)
			{
				use.defaultCompleteMode = ToilCompleteMode.Delay;
				use.defaultDuration = job.def.joyDuration;
			}
			else use.defaultCompleteMode = ToilCompleteMode.Never;
			
			use.handlingFacing = true;
			use.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			
			yield return use;
			yield break;
		}
		
		public override object[] TaleParameters()
		{
			return new object[]
			{
				pawn,
				TargetA.Thing.def
			};
		}
	}
}
