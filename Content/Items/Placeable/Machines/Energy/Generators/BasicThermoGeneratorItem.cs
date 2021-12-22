using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Generators{
	public class BasicThermoGeneratorItem : MachineItem<BasicThermoGenerator>{
		public override string ItemName => "Basic Thermal Generator";

		public override string ItemTooltip => "Converts certain foods and wooden items into Terra Flux (TF)";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(tick => MachineTile.GetExampleTexturePath("off"),
				tick => MachineTile.GetExampleTexturePath("on" + tick % 20 / 10),
				"Burns food and wood into Terra Flux (TF)",
				consumeTFLine: null,
				produceTFLine: "Per game tick, amount production variable");

		public override void SafeSetDefaults(){
			item.width = 30;
			item.height = 30;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 4, copper: 20);
		}
	}
}
