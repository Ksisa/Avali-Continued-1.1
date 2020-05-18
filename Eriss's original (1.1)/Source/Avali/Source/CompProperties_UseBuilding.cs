using System;
using Verse;

namespace Avali
{
	public class CompProperties_UseBuilding : CompProperties
	{
		public CompProperties_UseBuilding()
		{
			this.compClass = typeof(CompUseBuilding);
		}
		
		public WorkTags workType;
		
		public JobDef useJob;
		
		public string floatMenuText;
	}
}
