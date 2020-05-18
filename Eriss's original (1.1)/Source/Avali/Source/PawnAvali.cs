using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Avali
{
	public class PawnAvali : Pawn
	{
		public override void TickRare()
		{
			base.TickRare();
			if (!ThingOwnerUtility.ContentsFrozen(base.ParentHolder))
			{
				if (this.apparel != null)
				{
					this.apparel.ApparelTrackerTickRare();
				}
				this.inventory.InventoryTrackerTickRare();
			}
			
			/*
			if (base.Spawned && this.RaceProps.IsFlesh)
			{
				GenTemperature.PushHeat(this, 0.3f * this.BodySize * 4.16666651f);
			}
			*/
		}
	}
}
