﻿using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class ItemCacheItem : MachineItem<ItemCache>{
		public override string ItemName => "Item Cache";

		public override string ItemTooltip => "Stores large quantities of a singular item type" +
			"\nCan be \"locked\" to only store one item type, even when running out of actual items";

		public override void SafeSetDefaults(){
			item.width = 32;
			item.height = 32;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 10, copper: 20);
		}
	}
}
