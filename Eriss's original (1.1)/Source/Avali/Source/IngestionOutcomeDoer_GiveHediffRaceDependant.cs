using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Avali
{
	public class raceDependencies
	{
		public ThingDef race;
		
		public HediffDef hediffDef;
		
		public float severity = -1f;
		
		public ChemicalDef toleranceChemical;
		
		public bool divideByBodySize;
	}
	
	public class IngestionOutcomeDoer_GiveHediffRaceDependant : IngestionOutcomeDoer
	{
		public HediffDef hediffDef;

		public float severity = -1f;

		public ChemicalDef toleranceChemical;
		
		public bool divideByBodySize;
		
		public List<raceDependencies> raceDependencies;
		
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			if (raceDependencies != null)
			{
				for (int i = 0; i < raceDependencies.Count; i++)
				{
					raceDependencies raceDependency = raceDependencies[i];
					if (pawn.def == raceDependency.race)
					{
						Hediff hediffRace = HediffMaker.MakeHediff(raceDependency.hediffDef, pawn, null);
						float numRace;
						if (raceDependency.severity > 0f)
						{
							numRace = raceDependency.severity;
						}
						else
						{
							numRace = raceDependency.hediffDef.initialSeverity;
						}
						
						if (raceDependency.divideByBodySize)
						{
							numRace /= pawn.BodySize;
						}
						AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, raceDependency.toleranceChemical, ref numRace);
						hediffRace.Severity = numRace;
						pawn.health.AddHediff(hediffRace, null, null);
						
						return;
					}
				}
			}
			
			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, null);
			float num;
			if (severity > 0f)
			{
				num = severity;
			}
			else
			{
				num = hediffDef.initialSeverity;
			}
			
			if (divideByBodySize)
			{
				num /= pawn.BodySize;
			}
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num);
			hediff.Severity = num;
			pawn.health.AddHediff(hediff, null, null);
		}
		
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (parentDef.IsDrug && chance >= 1f)
			{
				foreach (StatDrawEntry s in hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
				{
					yield return s;
				}
			}
			yield break;
		}
	}
}
