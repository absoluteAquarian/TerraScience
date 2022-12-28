using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class GreenhouseItem : MachineItem<Greenhouse>{
		public override string ItemName => "Greenhouse";
		public override string ItemTooltip => "Automatically grows and harvests saplings, cacti, mushroom grass and herbs" +
			"\nDoes not require TF to function";

		internal override ScienceWorkbenchItemRegistry GetRegistry(){
			int frame = Main.rand.Next(30);

			return new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("empty")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("plants"), frameX: frame % 8, frameY: frame / 8, columnCount: 8, rowCount: 4, buffer: 2),
				"Automatically grows and harvests plants.  Works faster when powered",
				consumeTFLine: "Per game tick, " + GetMachineFluxUsageString(perGameTick: true),
				produceTFLine: null);
		}

		public override void SafeSetDefaults(){
			Item.width = 20;
			Item.height = 38;
			Item.scale = 0.78f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 2, copper: 10);
		}
	}
}
