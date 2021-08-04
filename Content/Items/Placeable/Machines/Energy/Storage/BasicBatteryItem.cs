using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Storage{
	public class BasicBatteryItem : MachineItem<BasicBattery>{
		public override string Texture => "TerraScience/Content/Items/Placeable/Machines/TemporaryMachineSprite";

		public override string ItemName => "Basic Battery";
		public override string ItemTooltip => "Stores Terra Flux (TF)";

		public override void SafeSetDefaults(){
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}