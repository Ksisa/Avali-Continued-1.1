using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Avali
{
	public class MentalState_ReturnsToPackmates : MentalState_RaceDependant
	{
		public override void PreStart()
		{
			base.PreStart();
			List<Pawn> relatedPawns = pawn.relations.RelatedPawns.ToList();
			for (int i = 0; i < relatedPawns.Count(); i++)
			{
				Pawn pawn2 = relatedPawns[i];
				if (pawn2 != null)
				{
					if (pawn.HaveLoveRelation(pawn2))
					{
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
					}
					else if (pawn.HavePackRelation(pawn2))
					{
						if (pawn2.Faction != pawn.Faction)
						{
							break;
						}
					}
				}
			}
		}
		
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
