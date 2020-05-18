using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

// базовый класс

namespace Avali
{
	public class JobGiver_GotoForLovinAvali : ThinkNode_JobGiver
	{	
		protected override Job TryGiveJob(Pawn pawn)
		{
			//Log.Message("Try give job GotoForLovinAvali");
			
			if (pawn.def != ThingDefOf.Avali)
			{
				//Log.Message("race = " + pawn.def.defName);
				return null;
			}
			
			
			if (Find.TickManager.TicksGame < pawn.mindState.canLovinTick)
			{
				/*
				string inventory = pawn.inventory.ToString();
				if (inventory.Contains("EggAvali"))
				{
					
				}
				*/
				
				//Log.Message("TickManager.TicksGame < pawn.mindState.canLovinTick");
				return null;
			}
			
			
			Predicate<Thing> validator = delegate(Thing t)
			{
				Pawn pawn3 = t as Pawn;
				
				return  !pawn3.Downed &&
						!pawn3.IsForbidden(pawn) &&
						!pawn3.health.HasHediffsNeedingTend() &&
						pawn3.InBed() &&
						//pawn3.CanCasuallyInteractNow(false) &&
						!pawn.health.HasHediffsNeedingTend() &&
						!pawn.Drafted &&
						//!pawn.InBed() &&
						LovePartnerRelationUtility.LovePartnerRelationExists(pawn, pawn3) &&
						Find.TickManager.TicksGame >= pawn3.mindState.canLovinTick &&
						//pawn.relations.OpinionOf(pawn3) >= 25 &&
						//pawn3.relations.OpinionOf(pawn) >= 25 &&
						pawn3.RaceProps.Humanlike == pawn.RaceProps.Humanlike;
			};
			Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(pawn.def), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			if (pawn2 == null)
			{
				//Log.Message("pawn2 = null");
				return null;
			}
			
			Building_Bed bed = RestUtility.FindBedFor(pawn2);
			//Log.Message(pawn + " bed = " + bed);
			if (bed != null)
			{
				//Log.Message(pawn + " current bed = " + pawn2.CurrentBed());
				if (pawn2.CurrentBed() != bed) return null;
				
				//Log.Message(pawn + " bed medical = " + bed.Medical);
				if (!bed.Medical)
				{
					//Log.Message("pawn2 = " + pawn2);
					return new Job(JobDefOf.GotoForLovinAvali, pawn2, bed); // give job
				}
			}
			
			return null;
		}
	}
}
