using System;
using Verse;

// DISABLED
// WORK IN PROGRESS

namespace Avali
{
	public class CompProperties_WormholePlatform : CompProperties
	{
		public CompProperties_WormholePlatform()
		{
			this.compClass = typeof(CompWormholePlatform);
		}
		
		public int maxGeneratorsPerConsole = 3;
		
		public int maxGeneratorsAIOperator = 3;
		
		public int wormholeGenPowerConsumption = 2000;
		
		public int AIoperatorSkillLevel = 20;
		
		// 0.0002 * 500000 = 100%
		// 500000 / 60(mins) = 8333.3(hours)
		// 24(hours in day) * 60(days in year) = 1440(hours in year)
		// 1440 * 6 = 8640(hours)
		// 5.5 years to get 100% of progress on 1 generator
		public float progressPerGenPerSec = 0.0002f;
		
		public float progressUnusedPerSec = -0.0001f;
	}
}
