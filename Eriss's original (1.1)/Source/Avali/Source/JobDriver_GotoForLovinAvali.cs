using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_GotoForLovinAvali : JobDriver
	{
		private const int ticksBeforeMote = 200;
		
		private TargetIndex partnerInd = TargetIndex.A;
		
		private TargetIndex bedInd = TargetIndex.B;
		
		private Pawn partner
		{
			get
			{
				return (Pawn)((Thing)job.GetTarget(partnerInd));
			}
		}
		
		private Building bed
		{
			get
			{
				return (Building)((Thing)job.GetTarget(bedInd));
			}
		}
		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(partnerInd);
			this.FailOn(() => !partner.health.capacities.CanBeAwake);
			
			yield return Toils_Goto.GotoThing(partnerInd, PathEndMode.Touch);
			
			Room pawnRoom = pawn.GetRoom();
			List<Pawn> pawnsForLovin = pawn.FindPawnsForLovinInRoom(pawnRoom, false);
			if (pawnsForLovin == null) yield break;
			
			const int waitTicks = 1;
			bool rand = Rand.Bool;
			Toil wait = Toils_General.Wait(waitTicks);
			wait.tickAction = delegate
			{
				pawn.GainComfortFromCellIfPossible();
				
				if (partner.CurJobDef != JobDefOf.LovinAvali && partner.CurJobDef != JobDefOf.LovinAvaliPartner)
				{
					this.ReadyForNextToil();
					if (rand) JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1);
				}
				else
				{
					if (rand) // watching
					{
						pawn.rotationTracker.Face(partner.DrawPos);
						JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None, 1);
					}
					else // waiting
					{
						if (pawn.IsHashIntervalTick(100))
						{
							pawn.rotationTracker.FaceCell(pawn.RandomAdjacentCell8Way());
						}
					}
				}
				
			};
			wait.socialMode = RandomSocialMode.SuperActive;
			
			if (partner.CurJobDef == JobDefOf.LovinAvali || partner.CurJobDef == JobDefOf.LovinAvaliPartner)
			{
				if (rand) this.ReportStringProcessed("Watching".Translate());
				else this.ReportStringProcessed("Waiting".Translate());
				
				yield return wait;
			}
			
			/*
			MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_HeartSpeech);
			
			Toil wait = Toils_General.WaitWith(PartnerInd, ticksBeforeMote, false, true);
			wait.socialMode = RandomSocialMode.Off;
			pawn.rotationTracker.Face(partner.DrawPos);
			yield return wait;
			MoteMaker.ThrowMetaIcon(partner.Position, partner.Map, ThingDefOf.Mote_HeartSpeech);
			*/
			
			Job newJob = new Job(JobDefOf.LovinAvali, partner, bed);
			pawn.jobs.StartJob(newJob, JobCondition.Succeeded, null, false, true, null, null, false);
			
			yield break;
		}
	}
}
