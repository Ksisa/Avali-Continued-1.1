using System;
using System.Linq;
using Verse;

namespace Avali
{
	public class PlaceWorker_NearTallBuildingOnly : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			Thing building = map.thingGrid.ThingAt(center, ThingCategory.Building);
			if (building != null)
			{
				if (building.def == def || building.def.holdsRoof || building.def.blockLight)
				{
					return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
				}
			}
			
			int rotation = rot.AsInt;
			// front
			if (rotation == 2)
			{
				IntVec3 AdjacentCell = new IntVec3(center.x + 1, center.y, center.z);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x + 1, center.y, center.z - 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x - 1, center.y, center.z);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x - 1, center.y, center.z - 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x, center.y, center.z + 1);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x, center.y, center.z - 2);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
			}
			// back
			else if (rotation == 0)
			{
				IntVec3 AdjacentCell = new IntVec3(center.x + 1, center.y, center.z);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x + 1, center.y, center.z + 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x - 1, center.y, center.z);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x - 1, center.y, center.z + 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x, center.y, center.z - 1);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x, center.y, center.z + 2);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
			}
			// side
			else if (rotation == 1)
			{
				IntVec3 AdjacentCell = new IntVec3(center.x, center.y, center.z + 1);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x + 1, center.y, center.z + 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x, center.y, center.z - 1);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x + 1, center.y, center.z - 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x + 2, center.y, center.z);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x - 1, center.y, center.z);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
			}
			// side
			else if (rotation == 3) 
			{
				IntVec3 AdjacentCell = new IntVec3(center.x, center.y, center.z + 1);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x - 1, center.y, center.z + 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x, center.y, center.z - 1);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x - 1, center.y, center.z - 1);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
				
				AdjacentCell = new IntVec3(center.x - 2, center.y, center.z);
				if (!AdjacentCell.Walkable(map))
				{
					AdjacentCell = new IntVec3(center.x + 1, center.y, center.z);
					if (!AdjacentCell.Walkable(map))
					{
						return true;
					}
				}
			}
			
			return "MustPlaceNearTallBuildingOnly".Translate();
		}
	}
}
