using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class JobDriver_SingsAlone : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				initAction = delegate
				{
					faceDir = ((!job.def.faceDir.IsValid) ? Rot4.Random : job.def.faceDir);
				},
				tickAction = delegate
				{
					pawn.rotationTracker.FaceCell(pawn.Position + faceDir.FacingCell);
					pawn.GainComfortFromCellIfPossible();
					JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f);
					
					if (pawn.IsHashIntervalTick(100))
					{
						MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Note);
					}
				},
				handlingFacing = true,
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = job.def.joyDuration
			};
			yield break;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<Rot4>(ref faceDir, "faceDir", default(Rot4), false);
		}

		private Rot4 faceDir;
	}
}
