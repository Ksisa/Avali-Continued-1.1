using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class JobDriver_SingsSocial : JobDriver
	{
		private Thing GatherSpotParent
		{
			get
			{
				return job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private bool HasChair
		{
			get
			{
				return job.GetTarget(TargetIndex.B).HasThing;
			}
		}

		private bool HasDrink
		{
			get
			{
				return job.GetTarget(TargetIndex.C).HasThing;
			}
		}

		private IntVec3 ClosestGatherSpotParentCell
		{
			get
			{
				return GatherSpotParent.OccupiedRect().ClosestCellTo(pawn.Position);
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null) && (!HasDrink || pawn.Reserve(job.GetTarget(TargetIndex.C), job, 1, -1, null));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			if (HasChair)
			{
				this.EndOnDespawnedOrNull(TargetIndex.B, JobCondition.Incompletable);
			}
			if (HasDrink)
			{
				this.FailOnDestroyedNullOrForbidden(TargetIndex.C);
				yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.C);
				yield return Toils_Haul.StartCarryThing(TargetIndex.C, false, false, false);
			}
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil chew = new Toil();
			chew.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(ClosestGatherSpotParentCell);
				pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.GoToNextToil, 1f);
				
				if (pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Note);
				}
			};
			chew.handlingFacing = true;
			chew.defaultCompleteMode = ToilCompleteMode.Delay;
			chew.defaultDuration = job.def.joyDuration;
			chew.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			chew.socialMode = RandomSocialMode.SuperActive;
			Toils_Ingest.AddIngestionEffects(chew, pawn, TargetIndex.C, TargetIndex.None);
			yield return chew;
			if (HasDrink)
			{
				yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.C);
			}
			yield break;
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 closestGatherSpotParentCell = ClosestGatherSpotParentCell;
			return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, closestGatherSpotParentCell, this.pawn);
		}

		private const TargetIndex GatherSpotParentInd = TargetIndex.A;

		private const TargetIndex ChairOrSpotInd = TargetIndex.B;

		private const TargetIndex OptionalIngestibleInd = TargetIndex.C;
	}
}
