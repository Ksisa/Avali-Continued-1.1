using UnityEngine;
using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_LovinAvaliSingle : JobDriver
	{
		public int TicksBetweenHeartMotes = 100;
		
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

		private Building_Bed bed
		{
			get
			{
				//Log.Message("Bed: " + (Building_Bed)((Thing)job.GetTarget(BedInd)));
				return (Building_Bed)((Thing)job.GetTarget(bedInd));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref ticksLeft, "ticksLeft", 0, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			//Log.Message("Toil reservations copmplete.");
			return pawn.Reserve(partner, job, 1, -1, null);
		}

		/*
		public override bool CanBeginNowWhileLyingDown()
		{
			Log.Message("Lay down.");
			new Job(JobDefOf.Lovin, partnerInMyBed, pawn.CurrentBed());
			return RestUtility.(this.pawn, this.job.GetTarget(this.BedInd));
		}
		*/

		protected override IEnumerable<Toil> MakeNewToils()
		{
			//this.FailOnDespawnedOrNull(bedInd);
			this.FailOnDespawnedOrNull(partnerInd);
			this.FailOn(() => !partner.health.capacities.CanBeAwake);
			//this.KeepLyingDown(this.BedInd);
			//yield return Toils_Bed.ClaimBedIfNonMedical(this.BedInd, TargetIndex.None);
			//Log.Message("Go to " + partner);
			
			if (pawn.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Deadly))
			{
				Job newJob = new Job(RimWorld.JobDefOf.Goto, bed);
				partner.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false);
				yield return Toils_Goto.GotoThing(bedInd, PathEndMode.ClosestTouch);
			}
			else
			{
				yield return Toils_Goto.GotoThing(partnerInd, PathEndMode.ClosestTouch);
			}
			
			//this.ReportStringProcessed("lovin'");
			yield return new Toil
			{
				initAction = delegate
				{
					/*
					if (this.partner.CurJob == null || this.partner.CurJob.def != JobDefOf.LovinAvali)
					{
						Job newJob = new Job(JobDefOf.LovinAvali, this.pawn, this.Bed);
						this.partner.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false);
						this.ticksLeft = (int)(2500f * Mathf.Clamp(Rand.Range(0.1f, 1.1f), 0.1f, 2f));
					}
					else
					{
						this.ticksLeft = 9999999;
					}
					*/
					
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
				}
				else
				{
					if (pawn.IsHashIntervalTick(TicksBetweenHeartMotes))
					{
						MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, RimWorld.ThingDefOf.Mote_Heart);
					}
					if (partner.IsHashIntervalTick(TicksBetweenHeartMotes))
					{
						MoteMaker.ThrowMetaIcon(partner.Position, pawn.Map, RimWorld.ThingDefOf.Mote_Heart);
					}
				}
			};
			
			/*
			Toil doLovin = Toils_LayDown.LayDown(BedInd, false, false, false, false);
			//doLovin.FailOn(() => partner.CurJob == null || partner.CurJob.def != JobDefOf.LovinAvali);
			doLovin.AddPreTickAction(delegate
			{
			    Log.Message("Do Lovin. Ticks left = " + ticksLeft);
				ticksLeft--;
				if (ticksLeft <= 0)
				{
					Log.Message("Ready for next toil. Ticks left = " + ticksLeft);
					this.ReadyForNextToil();
				}
				else if (pawn.IsHashIntervalTick(100))
				{
					Log.Message("Make mote.");
					MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				}
			});
			*/
			
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
			
			//Log.Message("Done");
			doLovinAvali.socialMode = RandomSocialMode.Off;
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
			float num = JobDriver_LovinAvaliSingle.LovinIntervalHoursFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
			num = Rand.Gaussian(num, 0.3f);
			if (num < 0.5f)
			{
				num = 0.5f;
			}
			return (int)(num * 10000f); // 2500
		}
	}
}