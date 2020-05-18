using UnityEngine;
using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_LovinAvali : JobDriver
	{
		public const int TicksBetweenHeartMotes = 100;
		
		public const float joyPerTick = 1;
		
		private int ticksLeft = 9999999;
		
		private TargetIndex partnerInd = TargetIndex.A;
		
		private TargetIndex bedInd = TargetIndex.B;
		
		private static readonly SimpleCurve LovinIntervalHoursFromAgeCurve = new SimpleCurve
		{
			{
				new CurvePoint(13f, 1.5f),
				true
			},
			{
				new CurvePoint(22f, 1.5f),
				true
			},
			{
				new CurvePoint(30f, 4f),
				true
			},
			{
				new CurvePoint(50f, 12f),
				true
			},
			{
				new CurvePoint(75f, 36f),
				true
			},
			{
				new CurvePoint(90f, 108f),
				true
			},
			{
				new CurvePoint(115f, 324f),
				true
			}
		};
		
		private Pawn partner
		{
			get
			{
				//Log.Message("Partner: " + (Pawn)((Thing)job.GetTarget(PartnerInd)));
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref ticksLeft, "ticksLeft", 0, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (bed != null) pawn.Reserve(bed, job, 1, -1, null);
			return pawn.Reserve(partner, job, 1, -1, null);
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.ReportStringProcessed("Moving".Translate());
			
			this.FailOnDespawnedOrNull(partnerInd);
			this.FailOn(() => !partner.health.capacities.CanBeAwake);
			
			yield return Toils_Goto.GotoThing(partnerInd, PathEndMode.ClosestTouch);
			
			Room room = pawn.GetRoom();
			if (pawn.FindPawnsForLovinInRoom(room, false) == null) yield break;
			
			MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_HeartSpeech);
			Toil wait = Toils_General.WaitWith(partnerInd, 200, false, true);
			wait.socialMode = RandomSocialMode.Off;
			yield return wait;
			MoteMaker.ThrowMetaIcon(partner.Position, partner.Map, ThingDefOf.Mote_HeartSpeech);
			
			if (pawn.FindPawnsForLovinInRoom(room, false) == null) yield break;
			
			if (bed != null && pawn.Position != bed.Position && partner.Position != bed.Position)
			{
				if (pawn.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Deadly) && partner.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Deadly))
			    {
					Job newJob = new Job(RimWorld.JobDefOf.Goto, bed);
					partner.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false);
					yield return Toils_Goto.GotoThing(bedInd, PathEndMode.ClosestTouch);
					
			    	MoteMaker.ThrowMetaIcon(pawn.Position, partner.Map, ThingDefOf.Mote_HeartSpeech);
					MoteMaker.ThrowMetaIcon(partner.Position, partner.Map, ThingDefOf.Mote_HeartSpeech);
			    }
			}
			
			yield return new Toil
			{
				initAction = delegate
				{
					//Log.Message(partner + " current bed " + Bed);
					Job newJob = new Job(JobDefOf.LovinAvaliPartner, partner);
					partner.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false);
					
					ticksLeft = (int)(2500f * Mathf.Clamp(Rand.Range(0.1f, 1.1f), 0.1f, 2f));
					//Log.Message("Ticks left = " + ticksLeft);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			
			Toil doLovinAvali = Toils_General.Wait(ticksLeft);
			doLovinAvali.tickAction = delegate
			{
				pawn.GainComfortFromCellIfPossible();
				partner.GainComfortFromCellIfPossible();
				
				partner.rotationTracker.Face(pawn.DrawPos);
				
				if (partner.health.Dead || partner.CurJob.def != JobDefOf.LovinAvaliPartner)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				
				ticksLeft--;
				if (ticksLeft <= 0)
				{
					//Log.Message("Ready for next toil. Ticks left = " + ticksLeft);
					this.ReadyForNextToil();
					JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None, 1);
					//JoyUtility.JoyTickCheckEnd(partner, JoyTickFullJoyAction.None, 1);
				}
				else
				{
					JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1);
					//JoyUtility.JoyTickCheckEnd(partner, JoyTickFullJoyAction.EndJob, 1);
					
					if (pawn.IsHashIntervalTick(TicksBetweenHeartMotes))
					{
						if (pawn.FindPawnsForLovinInRoom(room, true) == null)
						{
							partner.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
							this.EndJobWith(JobCondition.InterruptForced);
						}
						MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, RimWorld.ThingDefOf.Mote_Heart);
					}
					if (partner.IsHashIntervalTick(TicksBetweenHeartMotes))
					{
						MoteMaker.ThrowMetaIcon(partner.Position, pawn.Map, RimWorld.ThingDefOf.Mote_Heart);
					}
				}
			};
			doLovinAvali.AddFinishAction(delegate
			{
			    partner.jobs.EndCurrentJob(JobCondition.InterruptForced);
				Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(RimWorld.ThoughtDefOf.GotSomeLovin);
				pawn.needs.mood.thoughts.memories.TryGainMemory(newThought, partner);
				partner.needs.mood.thoughts.memories.TryGainMemory(newThought, pawn);
				pawn.mindState.canLovinTick = Find.TickManager.TicksGame + GenerateRandomMinTicksToNextLovin(pawn);
				partner.mindState.canLovinTick = Find.TickManager.TicksGame + GenerateRandomMinTicksToNextLovin(partner);
				
				
				
				if (pawn.def.defName != partner.def.defName ||
				   	!pawn.ageTracker.CurLifeStage.reproductive || 
				   	!partner.ageTracker.CurLifeStage.reproductive ||
				   	pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.AvaliHasEgg, true) != null)
				{
					return;
				}
				
				/*
				float chance = (int)Settings.AvaliEggChance;
				if (Rand.Chance(chance / 100))
				{
					if (pawn.gender == Gender.Male && partner.gender == Gender.Female)
					{
						AddHasEggHediff(pawn, partner);
					}
					else if (pawn.gender == Gender.Female && partner.gender == Gender.Male)
					{
						AddHasEggHediff(partner, pawn);
					}
				}
				*/
			});
			doLovinAvali.socialMode = RandomSocialMode.Off;
			
			this.ReportStringProcessed("Lovin".Translate());
			yield return doLovinAvali;
			
			yield break;
		}
		
		private void AddHasEggHediff(Pawn male, Pawn female)
		{
			Hediff_AvaliHasEgg hediff_HasEgg = (Hediff_AvaliHasEgg)HediffMaker.MakeHediff(HediffDefOf.AvaliHasEgg, female, null);
			hediff_HasEgg.father = male;
			female.health.AddHediff(hediff_HasEgg, null, null);
			//Log.Message(female + " begin making egg.");
		}
		
		private int GenerateRandomMinTicksToNextLovin(Pawn pawn)
		{
			if (DebugSettings.alwaysDoLovin)
			{
				return 100;
			}
			float num = JobDriver_LovinAvali.LovinIntervalHoursFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
			num = Rand.Gaussian(num, 0.3f);
			if (num < 0.5f)
			{
				num = 0.5f;
			}
			return (int)(num * 10000f); // 2500
		}
	}
}
