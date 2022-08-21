using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class AirIonizerItem : MachineItem<AirIonizer>{
		public override string ItemName => "Matter Energizer";
		public override string ItemTooltip => "Consumes Terra Flux (TF) to convert certain items into other items";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("closed")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("open"), frameY: Main.rand.Next(2), rowCount: 2),
				"Consumes Terra Flux (TF) to transmute items into other items",
				consumeTFLine: "Per operation, consumption amount variable",
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 40;
			Item.height = 46;
			Item.scale = 0.62f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}