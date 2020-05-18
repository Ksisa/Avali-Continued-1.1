using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using AlienRace;

namespace Avali
{
	public class CompCanBePutOn : ThingComp
	{
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (selPawn.CanReserveAndReach(parent, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false))
			{
				if (parent.GetType() != typeof(Apparel)) yield break;
				
				if (!selPawn.IsColonist || !selPawn.Awake()) yield break;
				
				List<Thing> avalablePawns = AvaliUtility.FindAllThingsOnMapAtRange(parent, null, typeof(Pawn), null, float.MaxValue, int.MaxValue, true, true);
				if (avalablePawns.Count == 0) yield break;
				
				const string putOnLabel = "PutOnLabel";
				
				for (int i = 0; i < avalablePawns.Count; i++)
				{
					var p = avalablePawns[i] as Pawn;
					//Log.Message("p = " + p + "; i = " + i);
					
					if (p != null && p != selPawn)
					{
						if (!p.IsColonist && !p.IsPrisonerOfColony)
						{
							if (!p.Downed) continue;
							if (p.RaceProps.IsMechanoid || !p.RaceProps.ToolUser) continue;
						}
					}
					else continue;
					
					var alienDef = p.def as ThingDef_AlienRace;
					if (alienDef != null)
					{
						List<string> apparelList = alienDef.alienRace.raceRestriction.apparelList;
						if (apparelList.Count > 0)
						{
							apparelList.AddRange(alienDef.alienRace.raceRestriction.whiteApparelList);
							bool isCompatibleApparel = false;
							
							for (int j = 0; j < apparelList.Count; j++)
							{
								string apparel = apparelList[j];
								
								if (parent.def.defName == apparel)
								{
									isCompatibleApparel = true;
									break;
								}
							}
							
							if (!isCompatibleApparel) continue;
						}
						
					}
					
					yield return new FloatMenuOption(putOnLabel.Translate(p.Label), delegate
					{
					  Job job = new Job(JobDefOf.PutOn, parent, p, selPawn.Position);
					  job.count = 1;
						selPawn.jobs.TryTakeOrderedJob(job, JobTag.MiscWork);
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
				}
			}
			
			yield break;
		}
	}
}
