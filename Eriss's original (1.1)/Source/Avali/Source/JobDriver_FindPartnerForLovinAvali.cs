using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace Avali
{
	public class JobDriver_FindPartnerForLovinAvali : JobDriver
	{	
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Room pawnRoom = pawn.GetRoom();
			List<Pawn> pawnsForLovin = pawn.FindPawnsForLovinInRoom(pawnRoom, false);
			if (pawnsForLovin == null) yield break;
			
			List<Pawn> avalablePawns = null;
			for (int i = 0; i < pawnsForLovin.Count; i++) // вначале создаем список свободных или занятых работой типа Joy пешек
			{
				Pawn pawnForLovin = pawnsForLovin[i];
				if (!pawnForLovin.CurJobDef.isIdle || pawnForLovin.CurJobDef.joyGainRate > 0)
				{
					avalablePawns.Add(pawnForLovin);
				}
			}
			
			if (avalablePawns == null)  // если таких пешек не найдено создаем список из уже занятых пешек
			{
				for (int i = 0; i < pawnsForLovin.Count; i++)
				{
					Pawn pawnForLovin = pawnsForLovin[i];
					if (pawnForLovin.CurJobDef == JobDefOf.LovinAvali || pawnForLovin.CurJobDef == JobDefOf.LovinAvaliPartner)
					{
						avalablePawns.Add(pawnForLovin);
					}
				}
			}
			
			if (avalablePawns != null)
			{
				Pawn partner = GetBestPartnerForLovin(avalablePawns); // выбираем лучшую пешку
				if (partner != null)
				{
					Building bed = RestUtility.FindBedFor(partner);
					if (bed == null || !pawn.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false))
					{
						bed = RestUtility.FindBedFor(pawn);
						if (bed == null || !pawn.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false))
						{
							bed = null;
							List<Thing> thingsInRoom = pawnRoom.ContainedAndAdjacentThings;
							float comfort = 0;
							for (int i = 0; i < thingsInRoom.Count; i++)
							{
								Building sittable = thingsInRoom[i] as Building;
								if (sittable != null)
								{
									float sittableComfort = sittable.GetStatValue(StatDefOf.Comfort);
									if (sittableComfort > comfort && pawn.CanReserveAndReach(sittable, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false))
									{
										bed = sittable;
										comfort = sittableComfort;
									}
								}
							}
						}
					}
					
					Job newJob = new Job(JobDefOf.LovinAvali, partner, bed);
					pawn.jobs.StartJob(newJob, JobCondition.Succeeded, null, false, true, null, null, false);
				}
			}
			
			yield break;
		}
		
		public Pawn GetBestPartnerForLovin(List<Pawn> candidates)
		{
			if (candidates.Count == 0) return null;
			
			Pawn partner = null;
			
			// get pawn from list with love relation
			for (int i = 0; i < candidates.Count; i++)
			{
				Pawn pawnForLovin = candidates[i];
				if (!pawn.HaveLoveRelation(pawnForLovin)) continue;
				
				partner = pawnForLovin;
				break;
			}
			
			if (partner == null)
			{
				// get pawn from list with highest opinion
				int opinionPawn = 0;
				int opinionPartner = 0;
				for (int i = 0; i < candidates.Count; i++)
				{
					Pawn candidate = candidates[i];
					
					int opinionPawnNew = pawn.relations.OpinionOf(candidate);
					int opinionPartnerNew = candidate.relations.OpinionOf(pawn);
					if (opinionPawnNew > opinionPawn && opinionPartnerNew > opinionPartner)
					{
						opinionPawn = opinionPawnNew;
						opinionPartner = opinionPartnerNew;
						partner = candidate;
					}
				}
			}
			
			return partner;
		}
	}
}
