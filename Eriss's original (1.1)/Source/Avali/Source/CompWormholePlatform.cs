using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

// DISABLED
// WORK IN PROGRESS

namespace Avali
{
	public class CompWormholePlatform : ThingComp
	{
		public float wormholeProgress = 0;
		public Thing wormhole = null;
		public string targetPlanet = "Avalon";
		
		private string planetIconPath = null;
		
		private CompPowerTrader compPower;
		private List<CompPowerTrader> powerComps;
		
		private float wormholeProgressPerSec = 0;
		
		private bool haveStateStabilizer = false;
		private List<ThingWithComps> wormholeConsoles = new List<ThingWithComps>();
		private List<ThingWithComps> AIOperators = new List<ThingWithComps>();
		private List<ThingWithComps> wormholeGenerators = new List<ThingWithComps>();
		
		private int supportedWormholeGenerators = 0;
		private int avalableWormholeGenerators = 0;
		
		public CompProperties_WormholePlatform Props
		{
			get
			{
				return (CompProperties_WormholePlatform)props;
			}
		}
		
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			compPower = parent.GetComp<CompPowerTrader>();
			if (compPower == null) Log.Error(parent + " not have CompPowerTrader which is required.");
		}
		
		public override void PostExposeData()
		{
			Scribe_Values.Look<float>(ref wormholeProgress, "wormholeProgress", 0, false);
			Scribe_References.Look<Thing>(ref wormhole, "wormhole", false);
		}
		
		public override void CompTick()
		{
			if (compPower == null) return;
			
			if (parent.IsHashIntervalTick(60))
			{
				if (wormholeProgress > 0)
				{
					if (wormhole == null) wormhole = GenSpawn.Spawn(ThingDef.Named("AvaliWormhole"), parent.Position, parent.Map);
					
					compPower.PowerOutput = -wormholeProgress * 10;
					if (!compPower.PowerOn || parent.IsBrokenDown())
					{
						parent.HitPoints -= (int)(0.1f * wormholeProgress);
						wormholeProgress -= 0.0100f;
						return;
					}
				}
				
				powerComps = compPower.PowerNet.powerComps;
				
				wormholeProgressPerSec = 0;
				
				haveStateStabilizer = false;
				wormholeConsoles = new List<ThingWithComps>();
				AIOperators = new List<ThingWithComps>();
				wormholeGenerators = new List<ThingWithComps>();
				
				supportedWormholeGenerators = 0;
				avalableWormholeGenerators = 0;
				
				for (int i = 0; i < powerComps.Count; i++)
				{
					CompPowerTrader compPowerTrader = powerComps[i];
					if (compPowerTrader.parent.def.defName == "AvaliWormholeStateStabilizer") haveStateStabilizer = true;
					else if (compPowerTrader.parent.def.defName == "AvaliWormholeConsole")
					{
						if (compPowerTrader.PowerOn == true && compPowerTrader.parent.IsOnAndNotBrokenDown() && compPowerTrader.parent.GetUserPawn() != null)
						{
							wormholeConsoles.Add(compPowerTrader.parent);
						}
					}
					else if (compPowerTrader.parent.def.defName == "AvaliWormholeGenerator")
					{
						if (compPowerTrader.PowerOn == true && compPowerTrader.parent.IsOnAndNotBrokenDown())
						{
							wormholeGenerators.Add(compPowerTrader.parent);
						}
					}
					else if (compPowerTrader.parent.def.defName == "AvaliWormholeAIOperator")
					{
						if (compPowerTrader.PowerOn == true && compPowerTrader.parent.IsOnAndNotBrokenDown())
						{
							AIOperators.Add(compPowerTrader.parent);
						}
					}
				}
				
				if (!haveStateStabilizer)
				{
					if (wormholeProgress >= Math.Abs(Props.progressUnusedPerSec)) wormholeProgress += Props.progressUnusedPerSec;
					else wormholeProgress = 0;
				}
				
				if (wormholeGenerators.Count == 0) return;
				
				if (wormholeProgress >= 100.1f)
				{
					if (AIOperators.Count > 0) SwitchAIOperators(AIOperators, false);
					
					SwitchGenerators(wormholeGenerators, false);
					return;
				}
				
				if (AIOperators.Count > 0) SwitchAIOperators(AIOperators, true);
				
				supportedWormholeGenerators = (wormholeConsoles.Count * Props.maxGeneratorsPerConsole) + (AIOperators.Count * Props.maxGeneratorsAIOperator);
				
				if (supportedWormholeGenerators == 0)
				{
					SwitchGenerators(wormholeGenerators, false);
					return;
				}
				
				avalableWormholeGenerators = wormholeGenerators.Count;
				int unavalableGenerators = supportedWormholeGenerators - wormholeGenerators.Count;
				if (unavalableGenerators < 0)
				{
					for (int i = 0; i > unavalableGenerators; i--)
					{
						wormholeGenerators.RemoveLast();
					}
				}
				
				SwitchGenerators(wormholeGenerators, true);
			}
		}
		
		public override string CompInspectStringExtra()
		{
			string compInspectString = "";
			
			if (wormholeProgress < 100)
			{
				compInspectString = string.Format("WormholeProgress".Translate(), wormholeProgress, wormholeProgressPerSec, wormholeProgressPerSec*60);
			}
			else
			{
				compInspectString = string.Format("WormholeProgress".Translate(), "100", wormholeProgressPerSec, wormholeProgressPerSec*60);
			}
			
			compInspectString += "WorkingSupportedAvalableWormholeGenerators".Translate();
			compInspectString += "\n" + string.Format("WSAWormholeGenerators".Translate(), wormholeGenerators.Count, supportedWormholeGenerators, avalableWormholeGenerators);
			
			if (haveStateStabilizer) compInspectString += "\n" + "HaveStateStabilizer".Translate();
			else compInspectString += "\n" + "NotHaveStateStabilizer".Translate();
			
			return compInspectString;
		}
		
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo c in base.CompGetGizmosExtra())
			{
				yield return c;
			}
			
			bool disabled = false;
			//string curTargetPlanet = "Avalon";
			
			if (wormholeProgress > 0) disabled = true;
			if (planetIconPath == null) planetIconPath = "UI/Commands/" + targetPlanet;
			
			yield return new Command_Action
			{
				action = delegate
				{
					//NextWormholeTarget();
				},
				disabled = false,
				disabledReason = "WormholeTargetDisabled".Translate(),
				defaultDesc = "CommandNextWormholeTarget".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/IncreaseDelay", true),
				hotKey = KeyBindingDefOf.Misc5
			};
			
			yield return new Command_Action
			{
				action = delegate
				{
					//PrevWormholeTarget();
				},
				disabled = false,
				disabledReason = "WormholeTargetDisabled".Translate(),
				defaultDesc = "CommandPrevWormholeTarget".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/DecreaseDelay", true),
				hotKey = KeyBindingDefOf.Misc5
			};
			
			yield return new Command_Action
			{
				action = delegate{},
				defaultDesc = (targetPlanet + "Desc").Translate() + " " + "WormholeTargetDisabled".Translate(),
				icon = ContentFinder<Texture2D>.Get(planetIconPath, true)
			};
			
			yield break;
		}
		
		public static void SwitchAIOperators(List<ThingWithComps> AIOperators, bool isOn)
		{
			for (int i = 0; i < AIOperators.Count; i++)
			{
				CompPowerTrader AIOperatorPowerComp = AIOperators[i].GetComp<CompPowerTrader>();
				if (isOn)
				{
					AIOperatorPowerComp.PowerOutput = AIOperatorPowerComp.Props.basePowerConsumption;
				}
				else AIOperatorPowerComp.PowerOutput = 0;
			}
		}
		
		public void SwitchGenerators(List<ThingWithComps> wormholeGenerators, bool isOn)
		{
			int wormholeGeneratorsCount = 0;
			int AIOperatorsCount = AIOperators.Count;
			int wormholeConsolesCount = wormholeConsoles.Count;
			
			for (int i = 0; i < wormholeGenerators.Count; i++)
			{
				CompPowerTrader wormholeGeneratorPowerComp = wormholeGenerators[i].GetComp<CompPowerTrader>();
				if (isOn)
				{
					wormholeGeneratorPowerComp.PowerOutput = Props.wormholeGenPowerConsumption;
					wormholeGeneratorsCount++;
					
					float progressPerGenPerSecMod = 0;
					if (AIOperatorsCount > 0)
					{
						if (wormholeGeneratorsCount > Props.maxGeneratorsAIOperator)
						{
							AIOperatorsCount--;
							wormholeGeneratorsCount = 0;
						}
						
						progressPerGenPerSecMod = Props.AIoperatorSkillLevel / 200000;
					}
					else if (wormholeConsoles.Count > 0)
					{
						if (wormholeGeneratorsCount > Props.maxGeneratorsPerConsole)
						{
							wormholeConsolesCount--;
							wormholeGeneratorsCount = 0;
						}
						
						SkillRecord operatorIntelSkill = wormholeConsoles[wormholeConsolesCount].GetUserPawn().skills.GetSkill(SkillDefOf.Intellectual);
						if (!operatorIntelSkill.TotallyDisabled) progressPerGenPerSecMod = operatorIntelSkill.Level / 200000;
					}
					
					wormholeProgressPerSec += Props.progressPerGenPerSec + progressPerGenPerSecMod;
				}
				else wormholeGeneratorPowerComp.PowerOutput = 0;
			}
			
			wormholeProgress += wormholeProgressPerSec;
		}
	}
}
