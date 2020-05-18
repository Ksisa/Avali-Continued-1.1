using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Avali
{
	public class CompProperties_WeaponAvali : CompProperties
	{
		public CompProperties_WeaponAvali()
		{
			this.compClass = typeof(CompRangedWeaponAvali);
		}
		
		public JobDef useJob;

		public string useLabel;
		
		public ThingDef workTable;
		
		public SkillDef hackWorkSkill;
		
		public int hackMinSkillLevel = 0;
		
		public int workLeft = 0;
		
		public List<ThingDef> requredFacilities = new List<ThingDef>();
	}
}