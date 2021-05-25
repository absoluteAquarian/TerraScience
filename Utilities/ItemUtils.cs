using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using TerraScience.Systems;

namespace TerraScience.Utilities{
	public static class ItemUtils{
		public static bool IsOre(Item item){
			// TODO: mod ore compatability
			int type = item.type;
			return type == ItemID.CopperOre || type == ItemID.IronOre || type == ItemID.SilverOre || type == ItemID.GoldOre || type == ItemID.TinOre || type == ItemID.LeadOre || type == ItemID.TungstenOre || type == ItemID.PlatinumOre;
		}
	}
}
