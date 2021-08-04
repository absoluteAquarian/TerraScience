using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class ItemCacheItem : MachineItem<ItemCache>{
		public override string ItemName => "Item Cache";

		public override string ItemTooltip => "Stores large quantities of a singular item type" +
			"\nCan be \"locked\" to only store one item type, even when running out of actual items";

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 32;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 10, copper: 20);
		}
	}
}
