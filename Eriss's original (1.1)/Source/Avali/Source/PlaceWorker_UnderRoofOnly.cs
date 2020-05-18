using Verse;

namespace Avali
{
	public class PlaceWorker_UnderRoofOnly : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (loc.GetRoof(map) != null)
			{
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
