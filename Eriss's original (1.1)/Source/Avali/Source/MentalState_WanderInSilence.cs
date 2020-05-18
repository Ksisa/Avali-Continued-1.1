using System;
using RimWorld;
using Verse.AI;

namespace Avali
{
	public class MentalState_WanderInSilence : MentalState_RaceDependant
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
