using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class LiquidDuplicatorItem : MachineItem<LiquidDuplicator>{
		public override string ItemName => "Liquid Duplicator";

		public override string ItemTooltip => "Consumes Terra Flux (TF) to duplicate certain liquids";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("full")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("empty")),
				ItemTooltip,
				consumeTFLine: "Per operation, amount consumed variable",
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 32;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 8, copper: 20);
		}
	}
}
