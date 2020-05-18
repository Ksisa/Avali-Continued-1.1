using System;
using RimWorld;
using Verse;

namespace Avali
{
	public class RecipeWorkerAvaliEgg : RecipeWorker
	{
		public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
		{
			Pawn mother = ingredient.TryGetComp<CompHatcher>().hatcheeParent;
			Pawn father = ingredient.TryGetComp<CompHatcher>().otherParent;
			
			if (mother != null) GiveThought(mother);
			
			if (father != null) GiveThought(father);
			
			ingredient.Destroy(DestroyMode.Vanish);
		}
		
		public static void GiveThought(Pawn parent)
		{
			if (parent.jobs.curJob.bill.recipe.defName == "MakeAvaliEggOmlette")
			{
				parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ParentForcedToCookAvaliEgg);
			}
			else parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AvaliEggCooked);
				
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.AvaliCaresOfEgg, parent, null);
			if (parent.health.hediffSet.hediffs.Contains(hediff)) parent.health.RemoveHediff(hediff);
			parent.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.AvaliCaresOfEgg);
		}
	}
}