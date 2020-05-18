using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

// DISABLED
// WORK IN PROGRESS

namespace Avali
{
	public class JobDriver_LayAvaliEgg : JobDriver
	{
		//private TargetIndex fatherInd = TargetIndex.A;
		
		//private TargetIndex bedInd = TargetIndex.B;
		
		private Pawn father
		{
			get
			{
				return (Pawn)((Thing)job.GetTarget(TargetIndex.A));
			}
		}
		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			base.ExposeData();
			if (TargetB.IsValid) return pawn.Reserve(TargetB, job, 1, -1, null);
			
			return true;
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{	
			IntVec3 cell = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 5f, null, Danger.Deadly);
			pawn.rotationTracker.Face(cell.ToVector3());
			
			yield return Toils_General.ClearTarget(TargetIndex.A);
			yield return Toils_General.Wait(500);
			
			yield return Toils_General.Do(delegate
			{
			    //this.ReportStringProcessed("laying egg.");
			    
				Thing egg = GenSpawn.Spawn(ThingDefOf.AvaliEgg, pawn.Position, Map);
			    //FilthMaker.MakeFilth(pawn.Position, pawn.Map, ThingDefOf.FilthAmnioticFluid, pawn.LabelIndefinite(), 1);
			    egg.SetForbidden(true, true);
			    
				CompHatcherAvali compHatcher = egg.TryGetComp<CompHatcherAvali>();
				if (compHatcher != null)
				{
					//Log.Message("mother = " + pawn);
					Hediff hediff_HasEgg = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == HediffDefOf.AvaliHasEgg);
					if (hediff_HasEgg != null)
					{
						pawn.health.RemoveHediff(hediff_HasEgg);
					}
					//Log.Message(pawn + " removed hediff: " + hediff_HasEgg);
					
					Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.AvaliCaresOfEgg);
					
					compHatcher.hatcheeFaction = pawn.Faction;
					compHatcher.hatcheeParent = pawn;
					pawn.health.AddHediff(HediffDefOf.AvaliCaresOfEgg, null, null);
					pawn.needs.mood.thoughts.memories.TryGainMemory(newThought, pawn);
					
					if (father != null)
					{
						compHatcher.otherParent = father;
						if (!father.Dead)
						{
							father.health.AddHediff(HediffDefOf.AvaliCaresOfEgg, null, null);
							father.needs.mood.thoughts.memories.TryGainMemory(newThought, father);
						}
					}
					
					//Log.Message(pawn + " egg.hatcheeFaction = " + compHatcher.hatcheeFaction);
					//Log.Message(pawn + " egg.hatcheeMother = " + pawn);
					//Log.Message(pawn + " egg.hatcheeFather = " + father);
				}
				else Log.Error(egg + " not have CompHatcherAvali.");
			});
			
			yield break;
		}
	}
}
