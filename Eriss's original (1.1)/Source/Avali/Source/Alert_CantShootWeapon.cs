using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Avali
{
	public class Alert_CantShootWeapon : Alert
	{
		public Alert_CantShootWeapon()
		{
			defaultPriority = AlertPriority.High;
		}
		
		public override string GetLabel()
		{
			int pawnsCantShootWeaponCount = PawnsCantShootWeapon().Count();
			if (pawnsCantShootWeaponCount == 1)
			{
				return "PawnCantShootWeapon".Translate();
			}
			return string.Format("PawnsCantShootWeapons".Translate(), pawnsCantShootWeaponCount+1);
		}
		
		private IEnumerable<Thing> PawnsCantShootWeapon()
		{
			List<Pawn> allMaps_FreeColonistsSpawned = PawnsFinder.AllMaps_FreeColonistsSpawned.ToList();
			for (int i = 0; i < allMaps_FreeColonistsSpawned.Count; i++)
			{
				Pawn pawn = allMaps_FreeColonistsSpawned[i];
				if (pawn.equipment.Primary != null)
				{
					CompRangedWeaponAvali compWeaponAvali = pawn.equipment.Primary.GetComp<CompRangedWeaponAvali>();
					if (compWeaponAvali == null) yield break;
					if (compWeaponAvali.ownerPawn == null) yield break;
					
					if (compWeaponAvali.currentBindMode == CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString() && pawn != compWeaponAvali.ownerPawn)
					{
						yield return pawn;
					}
					else if (compWeaponAvali.currentBindMode == CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString() && pawn.Faction != compWeaponAvali.ownerPawn.Faction)
					{
						yield return pawn;
					}
				}
			}
			yield break;
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<Thing> pawnsCantShootWeapon = PawnsCantShootWeapon().ToList();
			for (int i = 0; i < pawnsCantShootWeapon.Count; i++)
			{
				Thing pawn = pawnsCantShootWeapon[i];
				stringBuilder.AppendLine("    " + pawn.LabelShort.CapitalizeFirst());
			}
			return string.Format("PawnsCantShootWeaponsDesc".Translate(), stringBuilder);
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(PawnsCantShootWeapon().ToList());
		}
	}
}
