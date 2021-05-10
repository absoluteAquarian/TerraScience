using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class GreenhouseItem : MachineItem<Greenhouse>{
		public override string ItemName => "Greenhouse";
		public override string ItemTooltip => "Automatically grows and harvests saplings, cacti, mushroom grass and herbs" +
			"\nDoes not require TF to function";

		public override void SafeSetDefaults(){
			item.width = 20;
			item.height = 38;
			item.scale = 0.78f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 2, copper: 10);
		}
	}
}
