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

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(tick => MachineTile.GetExampleTexturePath("empty"),
				tick => MachineTile.GetExampleTexturePath("grass_sapling"),
				"Automatically grows and harvests plants");

		public override void SafeSetDefaults(){
			item.width = 20;
			item.height = 38;
			item.scale = 0.78f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 2, copper: 10);
		}
	}
}
