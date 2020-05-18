using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace Avali
{
	public class PlaceWorker_WormholePod : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			Thing avaliWormholePlatform = AvaliUtility.SpecifiedThingAtCellWithDefName(map.thingGrid.ThingsListAtFast(loc), "AvaliWormholePlatform");
			if (avaliWormholePlatform != null && loc == avaliWormholePlatform.InteractionCell) return true;
			
			Thing avaliWormholePod = AvaliUtility.SpecifiedThingAtCellWithDefName(map.thingGrid.ThingsListAtFast(new IntVec3(loc.x, loc.y, loc.z+1)), "AvaliWormholePod");
			if (DirectionCorrect(avaliWormholePod, loc)) return true;
			
			avaliWormholePod = AvaliUtility.SpecifiedThingAtCellWithDefName(map.thingGrid.ThingsListAtFast(new IntVec3(loc.x, loc.y, loc.z-1)), "AvaliWormholePod");
			if (DirectionCorrect(avaliWormholePod, loc)) return true;
			
			avaliWormholePod = AvaliUtility.SpecifiedThingAtCellWithDefName(map.thingGrid.ThingsListAtFast(new IntVec3(loc.x+1, loc.y, loc.z)), "AvaliWormholePod");
			if (DirectionCorrect(avaliWormholePod, loc)) return true;
			
			avaliWormholePod = AvaliUtility.SpecifiedThingAtCellWithDefName(map.thingGrid.ThingsListAtFast(new IntVec3(loc.x-1, loc.y, loc.z)), "AvaliWormholePod");
			if (DirectionCorrect(avaliWormholePod, loc)) return true;
			
			return new AcceptanceReport("MustPlaceOnWormholePlatformIntCell".Translate());
		}
		
		/*
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
		{
			
		}
		
		
		public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
			
		}
		*/
		
		public static bool DirectionCorrect(Thing avaliWormholePod, IntVec3 parentPos)
		{
			if (avaliWormholePod != null)
			{
				if (parentPos.x == avaliWormholePod.Position.x)
				{
					if (avaliWormholePod.Rotation == Rot4.East || avaliWormholePod.Rotation == Rot4.West) return true;
				}
				else if (parentPos.z == avaliWormholePod.Position.z)
				{
					if (avaliWormholePod.Rotation == Rot4.North || avaliWormholePod.Rotation == Rot4.South) return true;
				}
			}
			
			return false;
		}
	}
}
