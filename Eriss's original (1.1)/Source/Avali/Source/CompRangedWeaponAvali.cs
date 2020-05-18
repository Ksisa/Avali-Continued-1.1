using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace Avali
{
	public class CompRangedWeaponAvali : ThingComp
	{
		public enum bindMode : byte
		{
			None,
			AnyPawnInFaction,
			OwnerPawnOnly
		}
		
		public bool debug = false;
		
		public Type shootBinded;
		
		public string currentBindMode = CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString();
		
		public int workLeft = 0;
		
		public CompEquippable compEquippable;
		
		public Pawn ownerPawn;
		
		//private int oldWorkLeft = 0;
		
		public CompProperties_WeaponAvali Props
		{
			get
			{
				return (CompProperties_WeaponAvali)props;
			}
		}
		
		protected virtual string FloatMenuOptionLabel
		{
			get
			{
				return string.Format(Props.useLabel, parent.Label);
			}
		}
		
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (shootBinded == null)
			{
				List<VerbProperties> thingVerbs = parent.def.Verbs;
				for (int i = 0; i < thingVerbs.Count(); i++)
				{
					var verbProps = thingVerbs[i];
					if (verbProps != null && verbProps.verbClass == typeof(Verb_ShootBinded))
					{
						shootBinded = verbProps.verbClass;
						break;
					}
				}
			}
			
			if (shootBinded != null && workLeft <= 0)
			{
				if (currentBindMode == CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString() ||
				    currentBindMode == CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString())
				{
					if (workLeft <= 0 || workLeft > Props.workLeft) workLeft = Props.workLeft;
				}
			}
		}
		
		public override void PostDeSpawn(Map map)
		{
			if (shootBinded == null)
			{
				List<VerbProperties> thingVerbs = parent.def.Verbs;
				for (int i = 0; i < thingVerbs.Count(); i++)
				{
					var verbProps = thingVerbs[i];
					if (verbProps != null && verbProps.verbClass == typeof(Verb_ShootBinded))
					{
						shootBinded = verbProps.verbClass;
						break;
					}
				}
			}
			
			if (shootBinded != null)
			{
				compEquippable = parent.GetComp<CompEquippable>();
				if (compEquippable == null)
				{
					Log.Error(parent + " not have CompEquippable which is requred.");
					return;
				}
				
				if (currentBindMode == CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString() ||
				    currentBindMode == CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString())
				{
					//if (oldWorkLeft == 0) oldWorkLeft = Props.workLeft;
					
					if (workLeft <= 0 || workLeft > Props.workLeft) workLeft = Props.workLeft;
				}
				
				if (debug) Log.Message(parent + " currentBindMode = " + currentBindMode + "; ownerPawn = " + ownerPawn);
			}
		}
		
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<string>(ref currentBindMode, "currentBindMode", CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString(), false);
			Scribe_Values.Look<int>(ref workLeft, "workLeft", 0, false);
			//Scribe_Values.Look<int>(ref oldWorkLeft, "oldWorkLeft", 0, false);
			Scribe_References.Look<Pawn>(ref ownerPawn, "ownerPawn", true);
		}
		
		public override void PostSplitOff(Thing piece)
		{
			CompRangedWeaponAvali comp = ((ThingWithComps)piece).GetComp<CompRangedWeaponAvali>();
			comp.currentBindMode = currentBindMode;
			comp.workLeft = workLeft;
			comp.ownerPawn = ownerPawn;
		}
		
		public override string GetDescriptionPart()
		{
			StringBuilder stringBuilder = new StringBuilder();
			
			if (ownerPawn != null)
			{
				stringBuilder.Append("WeaponOwnerPawn".Translate() + ownerPawn.Name);
			}
			else stringBuilder.Append("WeaponOwnerPawn".Translate() + "None".Translate());
			
			if (currentBindMode != CompRangedWeaponAvali.bindMode.None.ToString())
			{
				if (Props.workLeft > 0 && workLeft > 0)
				{
					float hackPercent = Props.workLeft - workLeft;
					if (hackPercent > 0)
					{
						hackPercent = 100 / (Props.workLeft / hackPercent);
					}
					
					if (hackPercent >= 0.1f) stringBuilder.Append("\n\n" + "HackProgress".Translate() + hackPercent + "%");
				}
			}
			
			return stringBuilder.ToString();
		}
		
		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			
			if (ownerPawn != null)
			{
				stringBuilder.Append("WeaponOwnerPawn".Translate() + ownerPawn.Name);
			}
			else stringBuilder.Append("WeaponOwnerPawn".Translate() + "None".Translate());
			
			if (currentBindMode != CompRangedWeaponAvali.bindMode.None.ToString())
			{
				if (Props.workLeft > 0 && workLeft > 0)
				{
					float hackPercent = Props.workLeft - workLeft;
					if (hackPercent > 0)
					{
						hackPercent = 100 / (Props.workLeft / hackPercent);
					}
					
					if (hackPercent >= 0.1f) stringBuilder.Append("\n" + "HackProgress".Translate() + hackPercent + "%");
				}
			}
			
			return stringBuilder.ToString();
		}
		
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (debug) Log.Message(parent + " workLeft = " + workLeft);
			
			if (workLeft == 0 || selPawn == null || ownerPawn == null || selPawn.Drafted || !selPawn.RaceProps.ToolUser) yield break;
			
			if (currentBindMode == CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString())
			{
				if (ownerPawn == selPawn) yield break;
			}
			else if (currentBindMode == CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString())
			{
				if (ownerPawn.Faction == selPawn.Faction) yield break;
			}
			
			if (selPawn.CanReserve(parent, 1, -1, null, false))
			{
				//if (parent.IsForbidden(selPawn)) yield break;
				
				if (Props.hackWorkSkill != null && selPawn.skills.GetSkill(Props.hackWorkSkill).Level < Props.hackMinSkillLevel)
				{
					yield return new FloatMenuOption("CantHackBindedThing".Translate() + Props.hackWorkSkill + "SkillLevelToSmall".Translate() + Props.hackMinSkillLevel + ".", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield break;
				}
			
				if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					yield return new FloatMenuOption(selPawn + "CantReach".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
					yield break;
				}
				
				Predicate<Thing> validator = delegate(Thing t)
				{
					Thing building2 = t as Thing;
					
					return	building2.def == Props.workTable &&
							building2.IsPowered() &&
							selPawn.CanReserve(building2, 1, -1, null, false);
							//building2.LinkedToRequredFacilities(Props.requredFacilities);
				};
				Thing workTable = (Thing)GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map, ThingRequest.ForDef(Props.workTable), PathEndMode.Touch, TraverseParms.For(selPawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				
				if (workTable == null)
				{
					string facilities = "";
					for (int i = 0; i < Props.requredFacilities.Count; i++)
					{
						if (facilities == "") facilities = Props.requredFacilities[i].LabelCap;
						else facilities += ", " + Props.requredFacilities[i].LabelCap;
					}
					
					if (facilities == "")
					{
						yield return new FloatMenuOption(string.Format("RequiredUnoccupiedWorkTable".Translate(), Props.workTable.LabelCap) + ".", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else
					{
						yield return new FloatMenuOption(string.Format("RequiredUnoccupiedWorkTable".Translate(), Props.workTable.LabelCap) + string.Format("WithFacilities".Translate(), facilities) + ".", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					
					yield break;
				}
				
				if (!workTable.def.hasInteractionCell)
				{
					Log.Error(workTable + " not have interaction cell.");
					yield break;
				}
				
				FloatMenuOption useopt = new FloatMenuOption(FloatMenuOptionLabel, delegate
				{
					if (selPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
					{
						foreach (CompUseEffect compUseEffect in parent.GetComps<CompUseEffect>())
						{
							if (compUseEffect.SelectedUseOption(selPawn))
							{
								return;
							}
						}
						TryStartUseJob(selPawn, workTable);
					}
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				yield return useopt;
			}
			else
			{
				yield return new FloatMenuOption(FloatMenuOptionLabel + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			
			yield break;
		}
		
		public void TryStartUseJob(Pawn selPawn, Thing workTable)
		{
			Job job = new Job(Props.useJob, parent, workTable, workTable.OccupiedRect().ClosestCellTo(workTable.InteractionCell));
			job.count = 1;
			selPawn.jobs.TryTakeOrderedJob(job, JobTag.MiscWork);
		}
		
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo c in base.CompGetGizmosExtra())
			{
				yield return c;
			}
			
			if (shootBinded == null) yield break;
			
			/*yield return new Command_Action
			{
				action = delegate
				{
					ChangeBindMode();
				},
				disabled = true,
				disabledReason = "ShouldBeEquiped".Translate(),
				//defaultLabel = ("BindMode_" + currentBindMode).Translate(),
				defaultDesc = ("BindMode_" + currentBindMode + "Desc").Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/" + "BindMode_" + currentBindMode, true),
				hotKey = KeyBindingDefOf.Misc4
			};*/
			yield return new Command_Action
			{
				action = delegate
				{
					EraseOwnerPawnInfo();
				},
				disabled = true,
				disabledReason = "ShouldBeEquiped".Translate(),
				//defaultLabel = "EraseOwnerPawnInfo".Translate(),
				defaultDesc = "EraseOwnerPawnInfoDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/EraseOwnerPawnInfo", true),
				hotKey = KeyBindingDefOf.Misc5
			};
			yield break;
		}
		
		public void ChangeBindMode()
		{
			if (currentBindMode == CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString())
			{
				currentBindMode = CompRangedWeaponAvali.bindMode.None.ToString();
				workLeft = 0;
			}
			else if (currentBindMode == CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString())
			{
				currentBindMode = CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString();
				workLeft = Props.workLeft;
			}
			else // if None
			{
				currentBindMode = CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString();
				workLeft = Props.workLeft;
			}
		}
		
		public void EraseOwnerPawnInfo()
		{
			ownerPawn = null;
		}
	}
}
