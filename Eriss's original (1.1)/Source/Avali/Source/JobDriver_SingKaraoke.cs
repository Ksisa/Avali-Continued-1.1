using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class JobDriver_SingKaraoke : JobDriver
	{
		TargetIndex thingInd = TargetIndex.A;
		
		private Thing thing
		{
			get
			{
				return (Thing)((Thing)job.GetTarget(thingInd));
			}
		}
		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null) && 
				   pawn.Reserve(job.targetB, job, 1, -1, null);
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(thingInd, JobCondition.Incompletable); // thing
			this.FailOnForbidden(thingInd);
			
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell); // cell
			
			CompPowerTrader compPowerTrader = (CompPowerTrader)thing.TryGetComp<CompPowerTrader>();
			float statValue = TargetThingA.GetStatValue(StatDefOf.JoyGainFactor, true);
			Toil sing = new Toil();
			sing.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(TargetA.Cell);
				
				if (compPowerTrader != null)
				{
					if (compPowerTrader.PowerNet.CurrentStoredEnergy() <= 0 || thing.IsBrokenDown() || !FlickUtility.WantsToBeOn(thing))
					{
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
				
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, statValue);
				
				if (pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Note);
					
					// add thought to everyone in room
					
				    Room room = pawn.GetRoom(RegionType.Set_Passable);
					if (room != null)
					{
						int stage = 0;
						SkillRecord artSkill = pawn.skills.GetSkill(SkillDefOf.Artistic);
						if (!artSkill.TotallyDisabled)
						{
							float stageFloat = GetSingThoughtState(artSkill.Level) * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking) * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);
							stage = (int)stageFloat;
							if (stage > 4) stage = 4;
						}
						
						List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
						for (int i = 0; i < containedAndAdjacentThings.Count; i++)
						{
							Pawn pawn2 = containedAndAdjacentThings[i] as Pawn;
							if (pawn2 != null)
							{
								if (pawn2 != pawn && pawn2.RaceProps.Humanlike)
								{
									//Log.Message("Singer: " + pawn + ". Stage: " + stage);
									//Log.Message("Listener: " + pawn2);
									if (pawn.health.hediffSet.HasHediff(HediffDefOf.AvaliBiology)) // Avali singer
									{
										if (pawn2.health.hediffSet.HasHediff(HediffDefOf.AvaliBiology)) // Avali listener
										{
											if (stage < 2) stage = 2;
											stage = CheckSingerTraits(stage);
											pawn2.GiveThoughtWithStage(ThoughtDefOf.ListenerAvali, stage);
										}
										else // non Avali listener
										{
											if (stage < 1) stage = 1;
											stage = CheckSingerTraits(stage);
											pawn2.GiveThoughtWithStage(ThoughtDefOf.ListenerAny, stage);
										}
									}
									else // non Avali singer
									{
										stage = CheckSingerTraits(stage);
										if (pawn2.health.hediffSet.HasHediff(HediffDefOf.AvaliBiology)) // Avali listener
										{
											pawn2.GiveThoughtWithStage(ThoughtDefOf.ListenerAvali, stage);
										}
										else // non Avali listener
										{
											pawn2.GiveThoughtWithStage(ThoughtDefOf.ListenerAny, stage);
										}
									}
								}
							}
						}
					}
				}
			};
			sing.handlingFacing = true;
			sing.socialMode = RandomSocialMode.Off;
			sing.defaultCompleteMode = ToilCompleteMode.Delay;
			sing.defaultDuration = job.def.joyDuration;
			sing.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(pawn);
			});
			
			yield return sing;
			yield break;
		}
		
		public override object[] TaleParameters()
		{
			return new object[]
			{
				pawn,
				base.TargetA.Thing.def
			};
		}
		
		public int CheckSingerTraits(int stage)
		{
			if (pawn.story.traits.HasTrait(TraitDefOf.AnnoyingVoice)) return 0;
			if (pawn.story.traits.HasTrait(TraitDefOf.CreepyBreathing)) return 0;
			
			return stage;
		}
		
		public int GetSingThoughtState(int artSkillLevel)
		{
			if (artSkillLevel <= 3)
			{
				return 0;
			}
			else if (artSkillLevel <= 5)
			{
				return 1;
			}
			else if (artSkillLevel <= 10)
			{
				return 2;
			}
			else if (artSkillLevel <= 15)
			{
				return 3;
			}
			else // <= 20
			{
				return 4;
			}
		}
	}
}
