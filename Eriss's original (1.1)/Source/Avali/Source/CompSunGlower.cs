using System;
using RimWorld;
using Verse;

namespace Avali
{
	public class CompSunGlower : CompGlower
	{
		public int glow = -1;
		
		public CompProperties_Glower Props
		{
			get
			{
				return (CompProperties_Glower)props;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (parent.Spawned && glow == -1)
			{
				int curSkyGlow = (int)(255 * parent.Map.skyManager.CurSkyGlow);
				glow = curSkyGlow;
				Props.glowColor = new ColorInt(curSkyGlow, curSkyGlow, curSkyGlow);
				
				parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
				parent.Map.glowGrid.RegisterGlower(this);
			}
		}

		public override void CompTick()
		{
			if (parent.Spawned && parent.IsHashIntervalTick(60))
			{
				int curSkyGlow = (int)(255 * parent.Map.skyManager.CurSkyGlow);
				if (glow != curSkyGlow)
				{
					glow = curSkyGlow;
					Props.glowColor = new ColorInt(curSkyGlow, curSkyGlow, curSkyGlow);
					
					parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
					parent.Map.glowGrid.DeRegisterGlower(this);
					parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
					parent.Map.glowGrid.RegisterGlower(this);
				}
			}
		}
		
		public override void ReceiveCompSignal(string signal)
		{
			return;
		}

		public override void PostExposeData()
		{
			//Scribe_Values.Look<int>(ref skyGlow, "skyGlow", -1, false);
		}
		
		public override void PostDeSpawn(Map map)
		{
			if (glow != -1)
			{
				map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
				map.glowGrid.DeRegisterGlower(this);
			}
		}
	}
}
