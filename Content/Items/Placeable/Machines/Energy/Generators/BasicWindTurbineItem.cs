using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Generators{
	public class BasicWindTurbineItem : MachineItem<BasicWindTurbine>{
		public override string Texture => "TerraScience/Content/Items/Placeable/Machines/TemporaryMachineSprite";

		public override string ItemName => "Basic Wind Turbine";
		public override string ItemTooltip => "Generates Terra Flux based on the weather and how fast the air is moving";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile")),
				tick => null,
				"Generates Terra Flux based on the weather",
				consumeTFLine: null,
				produceTFLine: "Per game tick, amount produced variable");

		public override void SafeSetDefaults(){
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}