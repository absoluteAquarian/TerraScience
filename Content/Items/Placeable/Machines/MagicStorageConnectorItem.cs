using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines {
	public class MagicStorageConnectorItem : MachineItem<MagicStorageConnector>{
		public override string ItemName => "Connector";

		public override string ItemTooltip => "Connects item pipes to a Magic Storage storage system" +
			"\nThis multitile can be connected to a Magic Storage storage system like any of the other Magic Storage tiles" +
			"\nHowever, it cannot be the bridge between two system nets";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(tick => MachineTile.GetExampleTexturePath("tile"),
				tick => null,
				"Allows extraction/insertion of items from/into Magic Storage storage" +
				"\nsystems via Item Pumps and Item Pipes");

		public override void SafeSetDefaults(){
			item.width = 32;
			item.height = 32;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Orange;
			item.value = Item.buyPrice(silver: 50, copper: 10);
		}
	}
}
