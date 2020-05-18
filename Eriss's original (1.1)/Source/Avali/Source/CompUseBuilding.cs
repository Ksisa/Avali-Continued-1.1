using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Avali
{
	public class CompUseBuilding : CompMannable
	{
		public CompProperties_UseBuilding Props
		{
			get
			{
				return (CompProperties_UseBuilding)props;
			}
		}
		
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			if (pawn.Drafted || !pawn.RaceProps.ToolUser)
			{
				yield break;
			}
			if (!pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly, 1, -1, null, false))
			{
				yield break;
			}
			if (Props.workType != WorkTags.None && pawn.story != null && pawn.story.DisabledWorkTagsBackstoryAndTraits == Props.workType)
			{
				yield break;
			}
			
			FloatMenuOption opt = new FloatMenuOption(Props.floatMenuText.Translate(parent.LabelShort), delegate
			{
				Job job = new Job(Props.useJob, parent, parent.InteractionCell, parent);
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.Default, null, null, 0f, null, null);
			yield return opt;
			
			yield break;
		}
	}
}
