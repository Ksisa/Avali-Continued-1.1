using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace Avali
{
	public class Verb_ShootBinded : Verb_Shoot
	{
		CompRangedWeaponAvali compWeaponAvali;
		
		public string currentBindMode = CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString();
		
		public Pawn ownerPawn;
		
		public override bool Available() // updates every tick if pawn drafted
		{
			//Log.Message(base.caster + " test.");
			
			if (base.CasterPawn == null) return base.Available();
			
			compWeaponAvali = this.EquipmentSource.TryGetComp<CompRangedWeaponAvali>();
			if (compWeaponAvali == null)
			{
				Log.Error(ownerPawn + ": CompWeaponAvali is required for Verb_ShootBinded");
				return false;
			}
			
			/*
			if (compWeaponAvali.ownerPawn == null && base.CasterPawn.IsColonist && base.CasterPawn.Drafted)
			{
				return base.Available();
			}
			*/
			
			if (ownerPawn == null)
			{
				ownerPawn = compWeaponAvali.ownerPawn;
				if (ownerPawn == null)
				{
					ownerPawn = base.CasterPawn;
					compWeaponAvali.ownerPawn = base.CasterPawn;
				}
			}
			else if (compWeaponAvali.ownerPawn == null)
			{
				ownerPawn = base.CasterPawn;
				compWeaponAvali.ownerPawn = base.CasterPawn;
			}
			//Log.Message("Verb_ShootBinded ownerPawn: " + ownerPawn);
			
			if (ownerPawn != null)
			{
				currentBindMode = compWeaponAvali.currentBindMode;
				if (currentBindMode == CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString())
				{
					if (ownerPawn.Faction != base.CasterPawn.Faction) return false;
				}
				else if (currentBindMode == CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString())
				{
					if (ownerPawn != base.CasterPawn) return false;
				}
			}
			
			return base.Available();
		}
		
		/*public override void Notify_EquipmentLost()
		{
			base.Notify_EquipmentLost();
			
		}*/
		
		protected override bool TryCastShot()
		{
			bool flag = base.TryCastShot();
			if (flag && base.CasterIsPawn)
			{
				base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
			}
			return flag;
		}
	}
}
