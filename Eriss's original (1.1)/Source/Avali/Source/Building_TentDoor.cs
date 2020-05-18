using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace Avali
{
	public class Building_TentDoor : Building_Door
	{
		public override void Draw()
		{
			base.Rotation = Building_Door.DoorRotationAt(base.Position, base.Map);
			float num = Mathf.Clamp01((float)base.ticksSinceOpen / (float)base.TicksToOpenNow);
			float d = 0.45f * num;
			for (int i = 0; i < 2; i++)
			{
				Vector3 vector = default(Vector3);
				Mesh mesh;
				if (i == 0)
				{
					Log.Message("i == 0 " + def.defName + " open.");
					vector = new Vector3(0f, 0f, -1f);
					mesh = MeshPool.plane10;
				}
				else
				{
					Log.Message("i != 0 " + def.defName + " close.");
					vector = new Vector3(0f, 0f, 1f);
					mesh = MeshPool.plane10Flip;
				}
				Rot4 rotation = base.Rotation;
				rotation.Rotate(RotationDirection.Clockwise);
				vector = rotation.AsQuat * vector;
				Vector3 vector2 = this.DrawPos;
				vector2.y = AltitudeLayer.DoorMoveable.AltitudeFor();
				vector2 += vector * d;
				Graphics.DrawMesh(mesh, vector2, base.Rotation.AsQuat, this.Graphic.MatAt(base.Rotation, null), 0);
			}
			base.Comps_PostDraw();
		}
	}
}