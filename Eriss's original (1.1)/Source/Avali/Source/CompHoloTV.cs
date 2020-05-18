using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace Avali
{
	[StaticConstructorOnStartup]
	public class CompHoloTV : CompGlower
	{
		private static readonly string Folder_south = "Things/Building/Joy/TV/Hologram_south";
		private static readonly string Folder_north = "Things/Building/Joy/TV/Hologram_north";
		
		private static readonly Material Hologram_east = MaterialPool.MatFrom("Things/Building/Joy/TV/Hologram_east");
		
		private static readonly Material Hologram_front1 = MaterialPool.MatFrom(Folder_south+"/1");
		private static readonly Material Hologram_front2 = MaterialPool.MatFrom(Folder_south+"/2");
		private static readonly Material Hologram_front3 = MaterialPool.MatFrom(Folder_south+"/3");
		private static readonly Material Hologram_front4 = MaterialPool.MatFrom(Folder_south+"/4");
		private static readonly Material Hologram_front5 = MaterialPool.MatFrom(Folder_south+"/5");
		private static readonly Material Hologram_front6 = MaterialPool.MatFrom(Folder_south+"/6");
		private static readonly Material Hologram_front7 = MaterialPool.MatFrom(Folder_south+"/7");
		private static readonly Material Hologram_front8 = MaterialPool.MatFrom(Folder_south+"/8");
		private static readonly Material Hologram_front9 = MaterialPool.MatFrom(Folder_south+"/9");
		private static readonly Material Hologram_front10 = MaterialPool.MatFrom(Folder_south+"/10");
		private static readonly Material Hologram_front11 = MaterialPool.MatFrom(Folder_south+"/11");
		private static readonly Material Hologram_front12 = MaterialPool.MatFrom(Folder_south+"/12");
		private static readonly Material Hologram_front13 = MaterialPool.MatFrom(Folder_south+"/13");
		private static readonly Material Hologram_front14 = MaterialPool.MatFrom(Folder_south+"/14");
		private static readonly Material Hologram_front15 = MaterialPool.MatFrom(Folder_south+"/15");
		private static readonly Material Hologram_front16 = MaterialPool.MatFrom(Folder_south+"/16");
		private static readonly Material Hologram_front17 = MaterialPool.MatFrom(Folder_south+"/17");
		private static readonly Material Hologram_front18 = MaterialPool.MatFrom(Folder_south+"/18");
		private static readonly Material Hologram_front19 = MaterialPool.MatFrom(Folder_south+"/19");
		private static readonly Material Hologram_front20 = MaterialPool.MatFrom(Folder_south+"/20");
		
		private static readonly Material Hologram_back1 = MaterialPool.MatFrom(Folder_north+"/1");
		private static readonly Material Hologram_back2 = MaterialPool.MatFrom(Folder_north+"/2");
		private static readonly Material Hologram_back3 = MaterialPool.MatFrom(Folder_north+"/3");
		private static readonly Material Hologram_back4 = MaterialPool.MatFrom(Folder_north+"/4");
		private static readonly Material Hologram_back5 = MaterialPool.MatFrom(Folder_north+"/5");
		private static readonly Material Hologram_back6 = MaterialPool.MatFrom(Folder_north+"/6");
		private static readonly Material Hologram_back7 = MaterialPool.MatFrom(Folder_north+"/7");
		private static readonly Material Hologram_back8 = MaterialPool.MatFrom(Folder_north+"/8");
		private static readonly Material Hologram_back9 = MaterialPool.MatFrom(Folder_north+"/9");
		private static readonly Material Hologram_back10 = MaterialPool.MatFrom(Folder_north+"/10");
		private static readonly Material Hologram_back11 = MaterialPool.MatFrom(Folder_north+"/11");
		private static readonly Material Hologram_back12 = MaterialPool.MatFrom(Folder_north+"/12");
		private static readonly Material Hologram_back13 = MaterialPool.MatFrom(Folder_north+"/13");
		private static readonly Material Hologram_back14 = MaterialPool.MatFrom(Folder_north+"/14");
		private static readonly Material Hologram_back15 = MaterialPool.MatFrom(Folder_north+"/15");
		private static readonly Material Hologram_back16 = MaterialPool.MatFrom(Folder_north+"/16");
		private static readonly Material Hologram_back17 = MaterialPool.MatFrom(Folder_north+"/17");
		private static readonly Material Hologram_back18 = MaterialPool.MatFrom(Folder_north+"/18");
		private static readonly Material Hologram_back19 = MaterialPool.MatFrom(Folder_north+"/19");
		private static readonly Material Hologram_back20 = MaterialPool.MatFrom(Folder_north+"/20");
		
		private static readonly List<Material> Hologram_southSheet = new List<Material>();
		private static readonly List<Material> Hologram_northSheet = new List<Material>();
		
		private int currFrame = 1;
		
		private bool powered = false;
		
		private CompPowerTrader compPowerTrader;
		
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			
			Hologram_southSheet.Add(Hologram_front1);
			Hologram_southSheet.Add(Hologram_front2);
			Hologram_southSheet.Add(Hologram_front3);
			Hologram_southSheet.Add(Hologram_front4);
			Hologram_southSheet.Add(Hologram_front5);
			Hologram_southSheet.Add(Hologram_front6);
			Hologram_southSheet.Add(Hologram_front7);
			Hologram_southSheet.Add(Hologram_front8);
			Hologram_southSheet.Add(Hologram_front9);
			Hologram_southSheet.Add(Hologram_front10);
			Hologram_southSheet.Add(Hologram_front11);
			Hologram_southSheet.Add(Hologram_front12);
			Hologram_southSheet.Add(Hologram_front13);
			Hologram_southSheet.Add(Hologram_front14);
			Hologram_southSheet.Add(Hologram_front15);
			Hologram_southSheet.Add(Hologram_front16);
			Hologram_southSheet.Add(Hologram_front17);
			Hologram_southSheet.Add(Hologram_front18);
			Hologram_southSheet.Add(Hologram_front19);
			Hologram_southSheet.Add(Hologram_front20);
			
			Hologram_northSheet.Add(Hologram_back1);
			Hologram_northSheet.Add(Hologram_back2);
			Hologram_northSheet.Add(Hologram_back3);
			Hologram_northSheet.Add(Hologram_back4);
			Hologram_northSheet.Add(Hologram_back5);
			Hologram_northSheet.Add(Hologram_back6);
			Hologram_northSheet.Add(Hologram_back7);
			Hologram_northSheet.Add(Hologram_back8);
			Hologram_northSheet.Add(Hologram_back9);
			Hologram_northSheet.Add(Hologram_back10);
			Hologram_northSheet.Add(Hologram_back11);
			Hologram_northSheet.Add(Hologram_back12);
			Hologram_northSheet.Add(Hologram_back13);
			Hologram_northSheet.Add(Hologram_back14);
			Hologram_northSheet.Add(Hologram_back15);
			Hologram_northSheet.Add(Hologram_back16);
			Hologram_northSheet.Add(Hologram_back17);
			Hologram_northSheet.Add(Hologram_back18);
			Hologram_northSheet.Add(Hologram_back19);
			Hologram_northSheet.Add(Hologram_back20);
			
			compPowerTrader = (CompPowerTrader)parent.TryGetComp<CompPowerTrader>();
			//Log.Message("compPowerTrader = " + compPowerTrader);
		}
		
		public override void PostDraw()
		{
			base.PostDraw();
			if (powered)
			{
				DrawCurrentFrame();
			}
		}
		
		public override void CompTick()
		{
			base.CompTick();
			
			if (parent.IsHashIntervalTick(8))
			{
				if (compPowerTrader != null)
				{
					if (compPowerTrader.PowerNet.CurrentStoredEnergy() <= 0 || parent.IsBrokenDown() || !FlickUtility.WantsToBeOn(parent))
					{
						powered = false;
						return;
					}
				}
				
				powered = true;
				//Log.Message("currFrame = " + currFrame);
				if (currFrame >= 19) currFrame = -1;
				currFrame++;
			}
		}
		
		private void DrawCurrentFrame()
		{
			Matrix4x4 matrix = default(Matrix4x4);
			int rotation = parent.Rotation.AsInt;
			Vector3 vector = parent.TrueCenter();
			//Log.Message(parent + " vector = " + vector);
			vector.y = 5.3f;
			
			// south
			if (rotation == 2)
			{
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(1f, 1f, 1f));
				Graphics.DrawMesh(MeshPool.plane20, matrix, Hologram_southSheet[currFrame], 0);
			}
			// north
			else if (rotation == 0)
			{
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(1f, 1f, 1f));
				Graphics.DrawMesh(MeshPool.plane20, matrix, Hologram_northSheet[currFrame], 0);
			}
			// east
			else if (rotation == 1)
			{
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(1f, 1f, 1f));
				Graphics.DrawMesh(MeshPool.plane20, matrix, Hologram_east, 0);
				return;
			}
			// east
			else if (rotation == 3)
			{
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(1f, 1f, 1f));
				Graphics.DrawMesh(MeshPool.plane20, matrix, Hologram_east, 0);
				return;
			}
		}
	}
}
