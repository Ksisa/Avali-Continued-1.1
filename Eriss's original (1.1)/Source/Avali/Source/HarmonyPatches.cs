using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace Avali
{
	[StaticConstructorOnStartup]
  public static class HarmonyPatches
	{
    private static readonly Type patchType = typeof(HarmonyPatches);
    
		static HarmonyPatches()
		{
			Harmony harmony = new Harmony("rimworld.erisss.avali");
			
			harmony.Patch(AccessTools.Method(typeof(VerbTracker), "GetVerbsCommands", null, null), null, new HarmonyMethod(HarmonyPatches.patchType, "GetVerbsCommandsPostfix", null), null);
			harmony.Patch(AccessTools.Method(typeof(Pawn), "GetInspectString", null, null), null, new HarmonyMethod(HarmonyPatches.patchType, "GetInspectStringPostfix", null), null);
			
			harmony.Patch(AccessTools.Method(typeof(NegativeInteractionUtility), "NegativeInteractionChanceFactor", null, null), null, new HarmonyMethod(HarmonyPatches.patchType, "NegativeInteractionChanceFactorPostfix", null), null);
		}
		
		public static void GetVerbsCommandsPostfix(ref VerbTracker __instance, ref IEnumerable<Command> __result)
		{
			CompEquippable compEquippable = __instance.directOwner as CompEquippable;
			if (compEquippable == null) return;
			
			Thing thing = compEquippable.parent;
			
			CompRangedWeaponAvali compWeaponAvali = null;
			//Log.Message("thing: " + thing);
			
			if (thing != null) compWeaponAvali = thing.TryGetComp<CompRangedWeaponAvali>();
			if (compWeaponAvali == null) return;
			
			string currentBindMode = compWeaponAvali.currentBindMode;
			//Log.Message("currentBindMode: " + currentBindMode);
			bool isDisabled = false;
			string isDisabledReason = "CannotOrderNonControlled".Translate();
			
			Pawn holderPawn = compEquippable.PrimaryVerb.CasterPawn;
			//Log.Message("ownerPawn: " + ownerPawn);
			if (holderPawn == null || holderPawn.IsColonist == false) return;
			
			if (!holderPawn.Awake())
			{
				isDisabled = true;
				isDisabledReason = string.Format("NotAwake".Translate(), holderPawn.LabelShort.CapitalizeFirst());
			}
			else if (compWeaponAvali.ownerPawn != null && holderPawn != compWeaponAvali.ownerPawn)
			{
				if (currentBindMode == CompRangedWeaponAvali.bindMode.OwnerPawnOnly.ToString() ||
				    currentBindMode == CompRangedWeaponAvali.bindMode.AnyPawnInFaction.ToString())
				{
					isDisabled = true;
					isDisabledReason = "OwnerNotCasterPawn".Translate();
				}
			}
			else if (holderPawn.Drafted)
			{
				isDisabled = true;
				isDisabledReason = string.Format("ShouldBeUndrafted".Translate(), holderPawn.LabelShort.CapitalizeFirst());
			}
			//Log.Message("isDisabled: " + isDisabled);
			
			List<Command> result = __result.ToList();
			/*result.Add(new Command_Action
			{
				action = delegate
				{
					compWeaponAvali.ChangeBindMode();
				},
				disabled = isDisabled,
				disabledReason = isDisabledReason,
				//defaultLabel = ("BindMode_" + currentBindMode).Translate(),
				defaultDesc = ("BindMode_" + currentBindMode + "Desc").Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/" + "BindMode_" + currentBindMode, true),
				hotKey = KeyBindingDefOf.Misc4
			});*/
			result.Add(new Command_Action
			{
				action = delegate
				{
					compWeaponAvali.EraseOwnerPawnInfo();
				},
				disabled = isDisabled,
				disabledReason = isDisabledReason,
				//defaultLabel = "EraseOwnerPawnInfo".Translate(),
				defaultDesc = "EraseOwnerPawnInfoDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/EraseOwnerPawnInfo", true),
				hotKey = KeyBindingDefOf.Misc5
			});
			
			List<Verb> verbs = __instance.AllVerbs;
			for (int i = 0; i < verbs.Count; i++)
			{
				Verb verb = verbs[i];
				if (verb.verbProps.hasStandardCommand)
				{
					__result = result;
				}
			}
		}
		
		public static void GetInspectStringPostfix(ref Pawn __instance, ref string __result)
		{
			if (__instance.IsColonist == false) return;
			
			ThingWithComps equipedWeapon = __instance.equipment.Primary;
			if (equipedWeapon != null)
			{
				CompRangedWeaponAvali compWeaponAvali = equipedWeapon.GetComp<CompRangedWeaponAvali>();
				if (compWeaponAvali == null) return;
				
				if (compWeaponAvali.ownerPawn != null)
				{
					__result = __result + "\n" + "EquipedWeaponOwnerPawn".Translate() + compWeaponAvali.ownerPawn.Name;
				}
				else
				{
					__result = __result + "\n" + "EquipedWeaponOwnerPawn".Translate() + "None".Translate();
				}
			}
		}
  	
		#region NegativeInteractionChanceFactorPostfix
  	public static void NegativeInteractionChanceFactorPostfix(ref Pawn initiator, ref Pawn recipient, ref float __result)
  	{
  		if (initiator.def != ThingDefOf.Avali) return;
  		if (initiator.story.traits.HasTrait(TraitDefOf.Kind)) return;
  		
			if (initiator.HavePackRelation(recipient))
			{
				__result *= 1.15f;
			}
			else if (recipient.def == ThingDefOf.Avali)
			{
				__result *= 1.725f;
			}
			else
			{
				__result *= 2.3f;
			}
  	}
  	
  	private static readonly SimpleCurve CompatibilityFactorCurve = new SimpleCurve
		{
			{
				new CurvePoint(-2.5f, 4f),
				true
			},
			{
				new CurvePoint(-1.5f, 3f),
				true
			},
			{
				new CurvePoint(-0.5f, 2f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(1f, 0.75f),
				true
			},
			{
				new CurvePoint(2f, 0.5f),
				true
			},
			{
				new CurvePoint(3f, 0.4f),
				true
			}
		};
  	
		private static readonly SimpleCurve OpinionFactorCurve = new SimpleCurve
		{
			{
				new CurvePoint(-100f, 6f),
				true
			},
			{
				new CurvePoint(-50f, 4f),
				true
			},
			{
				new CurvePoint(-25f, 2f),
				true
			},
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(50f, 0.1f),
				true
			},
			{
				new CurvePoint(100f, 0f),
				true
			}
		};
		#endregion NegativeInteractionChanceFactorPostfix
  }
}
