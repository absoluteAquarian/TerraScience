using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class ElectrolyzerItem : MachineItem<Electrolyzer>{
		public override string ItemName => "Electrolyzer";
		public override string ItemTooltip => "Consumes Terra Flux and certain liquids to produce gases";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("empty")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("full")),
				ItemTooltip,
				consumeTFLine: "Per operation, " + GetMachineFluxUsageString(perGameTick: false),
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}