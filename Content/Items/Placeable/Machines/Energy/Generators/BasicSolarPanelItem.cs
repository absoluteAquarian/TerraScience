using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Generators{
	public class BasicSolarPanelItem : MachineItem<BasicSolarPanel>{
		public override string Texture => "TerraScience/Content/Items/Placeable/Machines/TemporaryMachineSprite";

		public override string ItemName => "Basic Solar Panel";
		public override string ItemTooltip => "Generates Terra Flux (TF) based on the time of day and weather";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile"), frameY: tick % 60 / 20, rowCount: 3),
				tick => null,
				ItemTooltip,
				consumeTFLine: null,
				produceTFLine: "Per game tick, amount produced variable");

		public override void SafeSetDefaults(){
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(silver: 6, copper: 50);
		}
	}
}
