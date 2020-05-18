using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_HackBindedThing : JobDriver
	{
		public int ticksPerWorkPoint = 60; // 1 sec
		
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
			return pawn.Reserve(thing, job, 1, -1, null) && pawn.Reserve(building, job, 1, -1, null);
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (thing == null || building == null) pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			
			bool err = false;
			Toil error = Toils_General.Do(delegate
			{
				Log.Error("Error in Toils_Haul.PlaceHauledThingInCell. Breaking job.");
				Log.Error("thingInd = " + thingInd);
				Log.Error("buildingInd = " + buildingInd);
				Log.Error("buildingCellInd = " + buildingCellInd);
				
				err = false;
			});
			
			CompProperties_WeaponAvali weaponAvali = thing.TryGetComp<CompRangedWeaponAvali>().Props;
			if (weaponAvali != null)
			{
				float num = 0;
				CompRangedWeaponAvali compWeaponAvali = thing.TryGetComp<CompRangedWeaponAvali>();
				if (building != null && building.GetStatValue(StatDefOf.ResearchSpeedFactor, true) > 0)
				{
					num = 1.1f * pawn.GetStatValue(StatDefOf.ResearchSpeed, true); // 1.1 * 0.58 = 0.638
					num *= building.GetStatValue(StatDefOf.ResearchSpeedFactor, true); // 0.638 * 1 = 0.638
					ticksPerWorkPoint = (int)(ticksPerWorkPoint / num); // 60 / 0.638 = 94
					if (ticksPerWorkPoint > 1 && pawn.def == ThingDefOf.Avali) ticksPerWorkPoint = ticksPerWorkPoint / 2;
				}
				else if (weaponAvali.hackWorkSkill != null)
				{
					num = (pawn.skills.GetSkill(weaponAvali.hackWorkSkill).Level - weaponAvali.hackMinSkillLevel) * 2;
					
					if (ticksPerWorkPoint - num > 0)
					{
						ticksPerWorkPoint = (int)(ticksPerWorkPoint - num);
					}
					else ticksPerWorkPoint = 1;
				}
				
				//Log.Message("num = " + num);
				//Log.Message("ticksPerWorkPoint = " + ticksPerWorkPoint);
				//Log.Message("workLeft = " + compWeaponAvali.workLeft);
				
				this.FailOnDestroyedOrNull(thingInd);
        this.FailOnDespawnedOrNull(buildingInd);
				
				yield return Toils_Goto.GotoThing(thingInd, PathEndMode.Touch).FailOnDestroyedOrNull(thingInd).FailOnSomeonePhysicallyInteracting(thingInd);
				yield return Toils_Misc.SetForbidden(thingInd, false);
				yield return Toils_Haul.StartCarryThing(thingInd, false, false, false);
				yield return Toils_Goto.GotoThing(buildingInd, PathEndMode.InteractionCell).FailOnDespawnedOrNull(buildingInd);
				yield return Toils_Haul.PlaceHauledThingInCell(buildingCellInd, Toils_Jump.Jump(error), false);
				
				//IntVec3 thingPosition = thing.PositionHeld;
				//IntVec3 buildingPosition = building.PositionHeld;
				
				int workLeftInTicks = (int)(compWeaponAvali.workLeft * (ticksPerWorkPoint * 1.1f));
				Toil hack = Toils_General.Wait(workLeftInTicks, buildingInd).FailOnDespawnedNullOrForbidden(thingInd).FailOnDespawnedNullOrForbidden(buildingInd);
				hack.tickAction = delegate
				{
					//else if (thing.PositionHeld != thingPosition || building.PositionHeld != buildingPosition) pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					//else if (thing.IsForbidden(pawn)) pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					
					pawn.skills.Learn(weaponAvali.hackWorkSkill, 0.11f, false);
					pawn.GainComfortFromCellIfPossible();
					
					if (pawn.IsHashIntervalTick(ticksPerWorkPoint))
					{
						if (!building.IsPowered()) pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
						
						//Log.Message("workLeft = " + compWeaponAvali.workLeft);
						compWeaponAvali.workLeft--;
						
						if (compWeaponAvali.workLeft <= 0)
						{
							compWeaponAvali.workLeft = 0;
							compWeaponAvali.EraseOwnerPawnInfo();
							pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
						}
					}
				};
				
				//Log.Message("Hack");
				yield return hack;
			}
			else
      {
      	Log.Warning(weaponAvali + " is not a Avali Weapon.");
      }
			
			if (err) yield return error;
			yield break;
		}
	}
}
