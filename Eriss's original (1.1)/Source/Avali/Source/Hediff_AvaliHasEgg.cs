using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Avali
{
	public class Hediff_AvaliHasEgg : HediffWithComps
	{
		public Pawn father;

		private const int PawnStateCheckInterval = 1000;

		private const float MTBMiscarryStarvingDays = 0.5f;

		private const float MTBMiscarryWoundedDays = 0.5f;
		
		public float GestationProgress
		{
			get
			{
				return Severity;
			}
			private set
			{
				Severity = value;
			}
		}

		private bool IsSeverelyWounded
		{
			get
			{
				float num = 0f;
				List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					if (hediffs[i] is Hediff_Injury && !hediffs[i].IsPermanent())
					{
						num += hediffs[i].Severity;
					}
				}
				List<Hediff_MissingPart> missingPartsCommonAncestors = this.pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int j = 0; j < missingPartsCommonAncestors.Count; j++)
				{
					if (missingPartsCommonAncestors[j].IsFreshNonSolidExtremity)
					{
						num += missingPartsCommonAncestors[j].Part.def.GetMaxHealth(this.pawn);
					}
				}
				return num > 38f * this.pawn.RaceProps.baseHealthScale;
			}
		}

		public override void Tick()
		{
			ageTicks++;
			if (pawn.IsHashIntervalTick(PawnStateCheckInterval))
			{
				//Log.Message("Father = " + father);
				if (pawn.needs.food != null && pawn.needs.food.CurCategory == HungerCategory.Starving && Rand.MTBEventOccurs(0.5f, 60000f, 1000f))
				{
					if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
					{
						Messages.Message("MessageMiscarriedStarvation".Translate(pawn.LabelIndefinite()).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeHealthEvent);
					}
					Miscarry();
					return;
				}
				
				if (IsSeverelyWounded && Rand.MTBEventOccurs(0.5f, 60000f, 1000f))
				{
					if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
					{
						Messages.Message("MessageMiscarriedPoorHealth".Translate(pawn.LabelIndefinite()).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeHealthEvent);
					}
					Miscarry();
					return;
				}
				
				if (GestationProgress >= 1f)
				{
					/*
					if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
					{
						Messages.Message("MessageAvaliLayedAnEgg".Translate(new object[]
						{
							pawn.LabelIndefinite()
						}).CapitalizeFirst(), pawn, MessageTypeDefOf.PositiveEvent);
					}
					*/
					
					/*
					if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
					{
						
						Messages.Message("MessageLayedEgg".Translate(new object[]
						{
							this.pawn.LabelIndefinite()
						}).CapitalizeFirst(), this.pawn, MessageTypeDefOf.PositiveEvent);
					}
					*/
					
					//Log.Message(pawn + " try to start job GotoLayAvaliEgg");
					//Log.Message(pawn + " egg.father = " + father);
					//Log.Message(pawn + " bed = " + RestUtility.FindBedFor(pawn));
					
					Job newJob = null;
					Building_Bed bed = RestUtility.FindBedFor(pawn);
					if (bed != null)
					{
						newJob = new Job(JobDefOf.GotoLayAvaliEgg, father, bed);
					}
					else
					{
						IntVec3 cell = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 5f, null, Danger.Some);
						newJob = new Job(JobDefOf.GotoLayAvaliEgg, father, cell);
					}
					
					pawn.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false);
					
					//Hediff_AvaliHasEgg.LayEgg(this.pawn, this.father);
					//this.pawn.health.RemoveHediff(this);
				}
			}
			if (GestationProgress < 1f) GestationProgress += 1f / (pawn.RaceProps.gestationPeriodDays * 60000f);
		}

		private void Miscarry()
		{
			pawn.health.RemoveHediff(this);
		}

		/*
		public static void LayEgg(Pawn mother, Pawn father)
		{
			
			int num = (mother.RaceProps.litterSizeCurve == null) ? 1 : Mathf.RoundToInt(Rand.ByCurve(mother.RaceProps.litterSizeCurve, 300));
			if (num < 1)
			{
				num = 1;
			}
			
			
			
			PawnGenerationRequest request = new PawnGenerationRequest(mother.kindDef, mother.Faction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null);
			Pawn pawn = null;
			for (int i = 0; i < num; i++)
			{
				pawn = PawnGenerator.GeneratePawn(request);
				if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, mother))
				{
					if (pawn.playerSettings != null && mother.playerSettings != null)
					{
						pawn.playerSettings.AreaRestriction = mother.playerSettings.AreaRestriction;
					}
					if (pawn.RaceProps.IsFlesh)
					{
						pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
						if (father != null)
						{
							pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
						}
					}
				}
				else
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
				TaleRecorder.RecordTale(TaleDefOf.GaveBirth, new object[]
				{
					mother,
					pawn
				});
			}
			
			
			
			if (mother.Spawned)
			{
				FilthMaker.MakeFilth(mother.Position, mother.Map, ThingDefOf.FilthAmnioticFluid, mother.LabelIndefinite(), 5);
				if (mother.caller != null)
				{
					mother.caller.DoCall();
				}
				if (pawn.caller != null)
				{
					pawn.caller.DoCall();
				}
			}
			
		}
		*/

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref father, "father", false);
		}

		public override string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.DebugString());
			stringBuilder.AppendLine("Gestation progress: " + GestationProgress.ToStringPercent());
			stringBuilder.AppendLine("Time left: " + ((int)((1f - GestationProgress) * pawn.RaceProps.gestationPeriodDays * 60000f)).ToStringTicksToPeriod());
			return stringBuilder.ToString();
		}
	}
}
