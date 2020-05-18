using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace Avali
{
	[StaticConstructorOnStartup]
	public class CompRunningGenerator : CompPowerTrader
	{
		private static readonly string Folder_south = "Things/Building/Joy/RunningTrack/RunningTrack_south";
		private static readonly string Folder_north = "Things/Building/Joy/RunningTrack/RunningTrack_north";
		private static readonly string Folder_east = "Things/Building/Joy/RunningTrack/RunningTrack_east";
		
		private static readonly Material RunningTrack_south0 = MaterialPool.MatFrom(Folder_south+"/0");
		private static readonly Material RunningTrack_south1 = MaterialPool.MatFrom(Folder_south+"/1");
		private static readonly Material RunningTrack_south2 = MaterialPool.MatFrom(Folder_south+"/2");
		private static readonly Material RunningTrack_south3 = MaterialPool.MatFrom(Folder_south+"/3");
		private static readonly Material RunningTrack_south4 = MaterialPool.MatFrom(Folder_south+"/4");
		private static readonly Material RunningTrack_south5 = MaterialPool.MatFrom(Folder_south+"/5");
		private static readonly Material RunningTrack_south6 = MaterialPool.MatFrom(Folder_south+"/6");
		private static readonly Material RunningTrack_south7 = MaterialPool.MatFrom(Folder_south+"/7");
		private static readonly Material RunningTrack_south8 = MaterialPool.MatFrom(Folder_south+"/8");
		
		private static readonly Material RunningTrack_north0 = MaterialPool.MatFrom(Folder_north+"/0");
		private static readonly Material RunningTrack_north1 = MaterialPool.MatFrom(Folder_north+"/1");
		private static readonly Material RunningTrack_north2 = MaterialPool.MatFrom(Folder_north+"/2");
		private static readonly Material RunningTrack_north3 = MaterialPool.MatFrom(Folder_north+"/3");
		private static readonly Material RunningTrack_north4 = MaterialPool.MatFrom(Folder_north+"/4");
		private static readonly Material RunningTrack_north5 = MaterialPool.MatFrom(Folder_north+"/5");
		private static readonly Material RunningTrack_north6 = MaterialPool.MatFrom(Folder_north+"/6");
		private static readonly Material RunningTrack_north7 = MaterialPool.MatFrom(Folder_north+"/7");
		private static readonly Material RunningTrack_north8 = MaterialPool.MatFrom(Folder_north+"/8");
		
		private static readonly Material RunningTrack_east0 = MaterialPool.MatFrom(Folder_east+"/0");
		private static readonly Material RunningTrack_east1 = MaterialPool.MatFrom(Folder_east+"/1");
		private static readonly Material RunningTrack_east2 = MaterialPool.MatFrom(Folder_east+"/2");
		private static readonly Material RunningTrack_east3 = MaterialPool.MatFrom(Folder_east+"/3");
		private static readonly Material RunningTrack_east4 = MaterialPool.MatFrom(Folder_east+"/4");
		private static readonly Material RunningTrack_east5 = MaterialPool.MatFrom(Folder_east+"/5");
		private static readonly Material RunningTrack_east6 = MaterialPool.MatFrom(Folder_east+"/6");
		private static readonly Material RunningTrack_east7 = MaterialPool.MatFrom(Folder_east+"/7");
		private static readonly Material RunningTrack_east8 = MaterialPool.MatFrom(Folder_east+"/8");
		
		private static readonly List<Material> RunningTrack_southSheet = new List<Material>();
		private static readonly List<Material> RunningTrack_northSheet = new List<Material>();
		private static readonly List<Material> RunningTrack_eastSheet = new List<Material>();
		
		private CompMannable compMannable;
		
		private int currFrame = 0;
		
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			
			RunningTrack_southSheet.Add(RunningTrack_south0);
			RunningTrack_southSheet.Add(RunningTrack_south1);
			RunningTrack_southSheet.Add(RunningTrack_south2);
			RunningTrack_southSheet.Add(RunningTrack_south3);
			RunningTrack_southSheet.Add(RunningTrack_south4);
			RunningTrack_southSheet.Add(RunningTrack_south5);
			RunningTrack_southSheet.Add(RunningTrack_south6);
			RunningTrack_southSheet.Add(RunningTrack_south7);
			RunningTrack_southSheet.Add(RunningTrack_south8);
			
			RunningTrack_northSheet.Add(RunningTrack_north0);
			RunningTrack_northSheet.Add(RunningTrack_north1);
			RunningTrack_northSheet.Add(RunningTrack_north2);
			RunningTrack_northSheet.Add(RunningTrack_north3);
			RunningTrack_northSheet.Add(RunningTrack_north4);
			RunningTrack_northSheet.Add(RunningTrack_north5);
			RunningTrack_northSheet.Add(RunningTrack_north6);
			RunningTrack_northSheet.Add(RunningTrack_north7);
			RunningTrack_northSheet.Add(RunningTrack_north8);
			
			RunningTrack_eastSheet.Add(RunningTrack_east0);
			RunningTrack_eastSheet.Add(RunningTrack_east1);
			RunningTrack_eastSheet.Add(RunningTrack_east2);
			RunningTrack_eastSheet.Add(RunningTrack_east3);
			RunningTrack_eastSheet.Add(RunningTrack_east4);
			RunningTrack_eastSheet.Add(RunningTrack_east5);
			RunningTrack_eastSheet.Add(RunningTrack_east6);
			RunningTrack_eastSheet.Add(RunningTrack_east7);
			RunningTrack_eastSheet.Add(RunningTrack_east8);
			
			compMannable = parent.TryGetComp<CompMannable>();
			if (base.Props.basePowerConsumption <= 0f && !parent.IsBrokenDown() && FlickUtility.WantsToBeOn(parent))
			{
				base.PowerOn = true;
			}
			
			//Log.Message("RunningTrack_southSheet frames count = " + RunningTrack_southSheet.Count);
		}
		
		public override void PostDraw()
		{
			base.PostDraw();
			DrawCurrentFrame();
		}
		
		public override void CompTick()
		{
			base.CompTick();
			UpdateDesiredPowerOutput();
			
			if (compMannable == null) return;
			if (compMannable.MannedNow)
			{
				if (parent.IsHashIntervalTick(1))
				{
					if (currFrame >= 8) currFrame = -1;
					currFrame++;
				}
			}
		}

		public void UpdateDesiredPowerOutput()
		{
			if (parent.IsHashIntervalTick(60))
			{
				if (!parent.IsBrokenDown() && FlickUtility.WantsToBeOn(parent))
				{
			    	if (compMannable != null)
					{
						if (compMannable.MannedNow)
						{
							PowerOutput = (compMannable.ManningPawn.GetStatValue(StatDefOf.MoveSpeed) * 100) + Rand.RangeInclusive(-100, 100);
						}
						else PowerOutput = 0;
					}
			    	else Log.Error(parent + "not have CompProperties_UseBuilding.");
				}
				else PowerOutput = 0;
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
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(1f, 1f, 2f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, RunningTrack_southSheet[currFrame], 0);
			}
			// north
			else if (rotation == 0)
			{
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(1f, 1f, 2f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, RunningTrack_northSheet[currFrame], 0);
			}
			// east
			else if (rotation == 1)
			{
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(2f, 1f, 1f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, RunningTrack_eastSheet[currFrame], 0);
			}
			// east
			else if (rotation == 3)
			{
				matrix.SetTRS(vector, Quaternion.identity, new Vector3(2f, 1f, 1f));
				Graphics.DrawMesh(MeshPool.plane10Flip, matrix, RunningTrack_eastSheet[currFrame], 0);
			}
		}
	}
}
