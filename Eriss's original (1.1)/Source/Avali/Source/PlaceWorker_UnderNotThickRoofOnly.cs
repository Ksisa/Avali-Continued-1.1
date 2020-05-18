using Verse;

namespace Avali
{
	public class PlaceWorker_UnderNotThickRoofOnly : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			RoofDef roof = loc.GetRoof(map);
			if (roof != null)
			{
				if (roof.isThickRoof) return new AcceptanceReport("MustPlaceUnderNotThickRoof".Translate());
				
				if (AvaliUtility.BuildingInPosition(map, loc, checkingDef) != null)
				{
					return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
				}
				else
				{
					return true;
				}
			}
			return new AcceptanceReport("MustPlaceUnderRoof".Translate());
		}
	}
}
