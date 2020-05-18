using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_TranslateText : JobDriver
	{
		public float ticksPerWorkPoint = 60; // 1 sec
		
		private TargetIndex thingInd = TargetIndex.A;
		private TargetIndex buildingInd = TargetIndex.B;
		private TargetIndex buildingCellInd = TargetIndex.C;
		
		private Thing thing
		{
			get
			{
				return (Thing)((Thing)job.GetTarget(thingInd));
			}
		}
		
		private Thing building
		{
			get
			{
				return (Thing)((Thing)job.GetTarget(buildingInd));
			}
		}
		
		private IntVec3 buildingCell
		{
			get
			{
				return (IntVec3)((IntVec3)job.GetTarget(buildingCellInd));
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (building != null)
			{
				return pawn.Reserve(thing, job, 1, -1, null) && pawn.Reserve(building, job, 1, -1, null);
			}
			else
			{
				return pawn.Reserve(thing, job, 1, -1, null);
			}
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (thing == null) pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			
			bool err = false;
			Toil error = Toils_General.Do(delegate
			{
				Log.Error("Error in Toils_Haul.PlaceHauledThingInCell. Breaking job.");
				Log.Error("thing = " + thing);
				Log.Error("building = " + building);
				Log.Error("buildingCell = " + buildingCell);
				
				err = true;
			});
			
			CompProperties_TextThing textThing = thing.TryGetComp<CompTextThing>().Props;
			if (textThing != null)
			{
				float num = 0;
				//int workLeft = thing.TryGetComp<CompTextThing>().workLeft;
				CompTextThing compTextThing = thing.TryGetComp<CompTextThing>();
				if (building != null && building.GetStatValue(StatDefOf.ResearchSpeedFactor, true) > 0)
				{
					num = 1.1f * pawn.GetStatValue(StatDefOf.ResearchSpeed, true); // 1.1 * 0.58 = 0.638
					num *= building.GetStatValue(StatDefOf.ResearchSpeedFactor, true); // 0.638 * 1 = 0.638
					ticksPerWorkPoint = ticksPerWorkPoint / num; // 60 / 0.638 = 94
				}
				else if (textThing.workSkill != null)
				{
					num = (pawn.skills.GetSkill(textThing.workSkill).Level - textThing.minSkillLevel) * 2;
					
					if (ticksPerWorkPoint - num > 0)
					{
						ticksPerWorkPoint = ticksPerWorkPoint - num;
					}
					else ticksPerWorkPoint = 1;
				}
				
				this.FailOnForbidden(thingInd);
				
				if (building != null)
        {
            this.FailOnDespawnedOrNull(buildingInd);
            this.FailOnForbidden(buildingInd);
        }
				
				//Log.Message("num = " + num);
				//Log.Message("ticksPerWorkPoint = " + ticksPerWorkPoint);
				//Log.Message("workLeft = " + workLeft);
				
				yield return Toils_Goto.GotoThing(thingInd, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(thingInd).FailOnSomeonePhysicallyInteracting(thingInd);
				yield return Toils_Misc.SetForbidden(thingInd, false);
				yield return Toils_Haul.StartCarryThing(thingInd, false, false, false);
				
				if (building != null)
				{
					if (buildingCell != new IntVec3(-1000, -1000, -1000))
					{
						yield return Toils_Goto.GotoThing(buildingInd, PathEndMode.InteractionCell).FailOnDespawnedOrNull(buildingInd);
					}
					else
					{
						yield return Toils_Goto.GotoThing(buildingInd, PathEndMode.OnCell).FailOnDespawnedOrNull(buildingInd);
					}
				}
				
				if (buildingCell != new IntVec3(-1000, -1000, -1000))
				{
					//Log.Message("buildingCell = " + buildingCell);
					yield return Toils_Haul.PlaceHauledThingInCell(buildingCellInd, Toils_Jump.Jump(error), false);
				}
				
				//IntVec3 thingPosition = thing.PositionHeld;
				//IntVec3 buildingPosition = building.PositionHeld;
				
				float workLeftInTicks = compTextThing.workLeft * (ticksPerWorkPoint * 1.1f);
				Toil translate = Toils_General.Wait((int)workLeftInTicks).FailOnDespawnedNullOrForbidden(thingInd).FailOnDespawnedNullOrForbidden(buildingInd);;
				
				translate.tickAction = delegate
				{
					if (textThing.workSkill != null)
					{
						pawn.skills.Learn(textThing.workSkill, 0.11f, false);
					}
					
					pawn.GainComfortFromCellIfPossible();
					
					if (pawn.IsHashIntervalTick((int)ticksPerWorkPoint))
					{
						//Log.Message("workLeft = " + compTextThing.workLeft);
						compTextThing.workLeft--;
						
						if (textThing.showTranslator)
						{
							if (compTextThing.translator == "")
							{
								compTextThing.translator += pawn.Name;
							}
							else
							{
								if (!compTextThing.translator.Contains(pawn.ToString()))
								{
								compTextThing.translator += pawn.Name.ToString();
									compTextThing.translator += ", " + pawn.Name;
								}
							}
						}
					}
					
					if (compTextThing.workLeft <= 0)
					{
						compTextThing.workLeft = 0;
						thing.def.BaseMarketValue = textThing.translatedMarketValue;
						
						if (textThing.taleWhenTranslated != null)
						{
							TaleRecorder.RecordTale(textThing.taleWhenTranslated, new object[]
							{
							    pawn,
								thing.def
							});
						}
						
						if (textThing.thoughtWhenTranslated != null)
						{
							Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(textThing.thoughtWhenTranslated);
							pawn.needs.mood.thoughts.memories.TryGainMemory(newThought, null);
						}
						
						pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				};
				
				//Log.Message("Translate");
				yield return translate;
			}
			
			if (err) yield return error;
			yield break;
		}
	}
}
