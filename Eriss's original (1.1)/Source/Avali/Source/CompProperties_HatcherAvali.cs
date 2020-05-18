using System;
using Verse;

// DISABLED
// WORK IN PROGRESS

namespace Avali
{
	public class CompProperties_HatcherAvali : CompProperties
	{
		public CompProperties_HatcherAvali()
		{
			this.compClass = typeof(CompHatcherAvali);
		}

		public float hatcherDaystoHatch = 1f;

		public PawnKindDef hatcherPawn;
	}
}
