using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Storage{
	public class BasicBatteryItem : MachineItem<BasicBattery>{
		public override string ItemName => "Basic Battery";
		public override string ItemTooltip => "Stores Terra Flux (TF)";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile")),
				tick => null,
				ItemTooltip,
				consumeTFLine: null,
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}