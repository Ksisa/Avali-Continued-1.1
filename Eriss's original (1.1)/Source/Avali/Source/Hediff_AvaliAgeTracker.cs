using System;
using System.Linq;
using Verse;
using RimWorld;

namespace Avali
{
	public class Hediff_AvaliAgeTracker : HediffWithComps
	{
		public const int pawnStateCheckInterval = 1000;
		
		public const float chance = 0.01f;
		
		public const int opinionNeeded = 25;
		
		private float pawnAge = 0;
		
		private bool hasCaretaker = false;
		
		public override void Tick()
		{
			base.Tick();
			if (pawn.IsHashIntervalTick(pawnStateCheckInterval))
			{
				if (hasCaretaker) // remove relation if caretaker is dead or null
				{
					foreach (Pawn pawn2 in pawn.relations.RelatedPawns)
					{
						if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Caretaker, pawn2))
						{
							if (pawn2.Dead || pawn2 == null)
							{
								pawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.Caretaker, pawn2);
								hasCaretaker = false;
							}
						}
					}
				}
				else
				{
					foreach (Pawn pawn2 in pawn.Map.mapPawns.FreeHumanlikesOfFaction(pawn.Faction))
					{
						if (pawn2.ageTracker.CurLifeStage.reproductive && pawn2 != pawn)
						{
							if (pawn.relations.OpinionOf(pawn2) >= opinionNeeded &&
						   	pawn2.relations.OpinionOf(pawn2) >= opinionNeeded)
							{
								pawn.relations.AddDirectRelation(PawnRelationDefOf.Caretaker, pawn2);
								pawn2.relations.AddDirectRelation(PawnRelationDefOf.Kit, pawn);
								pawn.relations.GetDirectRelation(PawnRelationDefOf.Kit, pawn).def.label += pawn;
								hasCaretaker = true;
							}
						}
					}
				}
				
				float pawnBioYears = pawn.ageTracker.AgeBiologicalYearsFloat;
				if (pawnBioYears >= 0 && pawnBioYears < 0.5f) // BabyStage (can't talk)
				{
					pawn.workSettings.DisableAll();
					pawn.health.AddHediff(HediffDefOf.CantTalk);
					pawn.story.traits.allTraits.Clear();
					
					GenerateTraits();
				}
				else if (pawnBioYears >= 0.5f && pawnBioYears < 1) // BabyStage (can talk)
				{
					GiveTraits();
					
					if (pawnAge < pawnBioYears)
					{
						Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == HediffDefOf.CantTalk);
						if (hediff != null) pawn.health.RemoveHediff(hediff);
						
						pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
					}
				}
				else if (pawnBioYears >= 1 && pawnBioYears < 3) // ChildStage
				{
					GiveTraits();
					
					/*
					if (pawn.skills.GetSkill(SkillDefOf.Animals).Level > 6)
					{
						pawn.skills.GetSkill(SkillDefOf.Animals).Level = 6;
					}
					*/
					
					if (pawnAge < pawnBioYears)
					{
						//pawn.story.WorkTagIsDisabled(WorkTags.None);
						
						string passion = pawn.skills.GetSkill(SkillDefOf.Animals).passion.ToString();
						
						pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
					}
				}
				else if (pawnBioYears >= 3 && pawnBioYears < 5) // TeenStage
				{
					GiveTraits();
					
					if (pawnAge < pawnBioYears)
					{
						pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
					}
				}
				else if (pawnBioYears >= 5) // AdultStage
				{
					GiveTraits();
					
					if (pawnAge < pawnBioYears)
					{
						pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
						pawn.health.RemoveHediff(this);
					}
				}
			}
		}
		
		public void GiveTraits()
		{
			if (pawn.story.traits.allTraits.Count >= 3) return;
			
			
			float timesInMentalState = pawn.records.GetValue(RecordDefOf.TimesInMentalState);
			
			if (timesInMentalState >= 3) ChangeTraitDegreeIfConditionReached(timesInMentalState);
			else if (timesInMentalState >= 6) ChangeTraitDegreeIfConditionReached(timesInMentalState);
			else if (timesInMentalState >= 9) ChangeTraitDegreeIfConditionReached(timesInMentalState);
			else if (timesInMentalState >= 12) ChangeTraitDegreeIfConditionReached(timesInMentalState);
			
			if (pawn.records.GetValue(RecordDefOf.TimesInMentalState) > 6)
			{
				
			}
			
			//GiveTraitIfConditionReached("Brawler", RecordDefOf., 1000, 0);
			GiveTraitIfConditionReached("Kind", RecordDefOf.PrisonersChatted, 1000, 0);
			GiveTraitIfConditionReached("Kind", RecordDefOf.PrisonersRecruited, 10, 0);
			GiveTraitIfConditionReached("Masochist", RecordDefOf.DamageTaken, 500, 0);
			GiveTraitIfConditionReached("TooSmart", RecordDefOf.ResearchPointsResearched, 4000, 0);
			GiveTraitIfConditionReached("FearsFire", RecordDefOf.TimesOnFire, 3, 0);
			GiveTraitIfConditionReached("Nerves", RecordDefOf.TimesInMentalState, 3, -1);
			GiveTraitIfConditionReached("Bloodlust", RecordDefOf.AnimalsSlaughtered, 40, 0);
			GiveTraitIfConditionReached("Bloodlust", RecordDefOf.KillsHumanlikes, 40, 0);
			GiveTraitIfConditionReached("ShootingAccuracy", RecordDefOf.Headshots, 10, 1); // careful shooter
			GiveTraitIfConditionReached("ShootingAccuracy", RecordDefOf.ShotsFired, 1000, -1); // trigger happy
			GiveTraitIfConditionReached("GreenThumb", RecordDefOf.PlantsSown, 100, 0);
		}
		
		public void ChangeTraitDegreeIfConditionReached(float timesInMentalState)
		{
			if (pawn.story.traits.DegreeOfTrait(TraitDef.Named("Nerves")) == -1)
			{
				pawn.story.traits.allTraits.Remove(new Trait(TraitDefOf.Nerves, -1, false));
				pawn.story.traits.GainTrait(new Trait(TraitDefOf.Nerves, -2, false));
			}
		}
		
		public void GiveTraitIfConditionReached(string traitName, RecordDef record, float recordsCount, int degree)
		{
			if (pawn.story.traits.allTraits.Count >= 3) return;
			if (!pawn.story.traits.HasTrait(TraitDef.Named(traitName)))
			{
				if (pawn.records.GetValue(record) > recordsCount)
				{
					pawn.story.traits.GainTrait(new Trait(TraitDef.Named(traitName), degree, false));
				}
			}
		}
		
		public void GenerateTraits()
		{	
			if (Rand.Chance(0.95f)) pawn.story.traits.GainTrait(new Trait(TraitDef.Named("Abrasive")));
			if (Rand.Chance(0.4f)) pawn.story.traits.GainTrait(new Trait(TraitDef.Named("FearsFire")));
			
			GiveTraitWithChance("Brawler");
			GiveTraitWithChance("Bloodlust");
			GiveTraitWithChance("CreepyBreathing");
			GiveTraitWithChance("FastLearner");
			GiveTraitWithChance("Ascetic");
			GiveTraitWithChance("Gay");
			GiveTraitWithChance("GreenThumb");
			GiveTraitWithChance("NaturalMood");
			GiveTraitWithChance("Nudist");
			GiveTraitWithChance("PsychicSensitivity");
			GiveTraitWithChance("Psychopath");
			GiveTraitWithChance("TooSmart");
			GiveTraitWithChance("Nimble");
			GiveTraitWithChance("Pyromaniac");
			GiveTraitWithChance("SuperImmune");
			GiveTraitWithChance("FastLearner");
			GiveTraitWithChance("Masochist");
			GiveTraitWithChance("NightOwl");
			GiveTraitWithChance("Wimp");
			
			GiveTraitWithChanceInRange("SpeedOffset", -1, 2);
			GiveTraitWithChanceInRange("DrugDesire", -1, 2);
			GiveTraitWithChanceInRange("NaturalMood", -2, 2);
			GiveTraitWithChanceInRange("Nerves", -2, 2);
			GiveTraitWithChanceInRange("Neurotic", -2, 2);
			GiveTraitWithChanceInRange("Industriousness", -2, 2);
			GiveTraitWithChanceInRange("PsychicSensitivity", -2, 2);
			GiveTraitWithChanceInRange("Beauty", 1, 2);
			//GiveTraitWithChanceInRange("ShootingAccuracy", -1, 1);
			
			if (pawn.story.traits.HasTrait(TraitDefOf.Abrasive))
			{
				if (Rand.Chance(0.25f)) pawn.story.traits.GainTrait(new Trait(TraitDefOf.Kind, 0, false));
				if (pawn.story.traits.allTraits.Count >= 3) return;
			}
			
			GiveTraitWithChanceIfNull("DislikesWomen", "DislikesMen");
			GiveTraitWithChanceIfNull("DislikesMen", "DislikesWomen");
		}
		
		public void GiveTraitWithChance(string traitName)
		{
			if (pawn.story.traits.allTraits.Count >= 3) return;
			if (Rand.Chance(chance)) pawn.story.traits.GainTrait(new Trait(TraitDef.Named(traitName)));
		}
		
		public void GiveTraitWithChanceInRange(string traitName, int min, int max)
		{
			if (pawn.story.traits.allTraits.Count >= 3) return;
			if (Rand.Chance(chance))
			{
				int degree = Rand.Range(min, max);
				if (degree == 0)
				{
					if (Rand.Chance(0.5f)) degree = -1;
					else degree = 1;
				}
				pawn.story.traits.GainTrait(new Trait(TraitDef.Named(traitName), degree, false));
			}
		}
		
		public void GiveTraitWithChanceIfNull(string nullTrait, string traitToGive)
		{
			if (pawn.story.traits.allTraits.Count >= 3) return;
			if (!pawn.story.traits.HasTrait(TraitDef.Named(nullTrait)))
			{
				if (Rand.Chance(chance)) pawn.story.traits.GainTrait(new Trait(TraitDef.Named(traitToGive)));
			}
		}
	}
}
