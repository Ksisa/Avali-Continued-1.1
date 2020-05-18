using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Avali
{
	public class HediffGiver_AddHediff : HediffGiver
	{
		public bool hasHediff = false;
		
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (pawn.IsHashIntervalTick(1))
			{
				if (!pawn.health.hediffSet.HasHediff(hediff))
				{
					//Log.Message("Try apply " + cause + " hediff to " + pawn);
					base.TryApply(pawn, null);
				}
			}
		}
	}
}
