using Verse;

namespace Avali
{
	public class PlaceWorker_AvaliWindow : PlaceWorker
	{
		private IntVec3 c1;
		private IntVec3 c2;
		
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (rot == Rot4.West || rot == Rot4.East)
			{
				c1 = center + new IntVec3(1, 0, 0);
				c2 = center - new IntVec3(1, 0, 0);
			}
			else
			{
				c1 = center + new IntVec3(0, 0, 1);
				c2 = center - new IntVec3(0, 0, 1);
			}
			
			if (AvaliUtility.BuildingInPosition(map, c1, def) != null)
			{
				if (AvaliUtility.BuildingInPosition(map, c2, def) != null)
				{
					return true;
				}
			}
			
			return true;
		}
		
		/*
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
		{
			
		}
		*/
	}
}
