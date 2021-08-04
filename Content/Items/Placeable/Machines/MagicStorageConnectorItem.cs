using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class MagicStorageConnectorItem : MachineItem<MagicStorageConnector>{
		public override string ItemName => "Connector";

		public override string ItemTooltip => "Connects item pipes to a Magic Storage storage system" +
			"\nThis multitile can be connected to a Magic Storage storage system like any of the other Magic Storage tiles" +
			"\nHowever, it cannot be the bridge between two system nets";

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 32;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(silver: 50, copper: 10);
		}
	}
}
