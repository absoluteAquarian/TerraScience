using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class GreenhouseItem : MachineItem<Greenhouse>{
		public override string ItemName => "Greenhouse";
		public override string ItemTooltip => "Automatically grows and harvests saplings, cacti, mushroom grass and herbs" +
			"\nDoes not require TF to function";

		public override void SafeSetDefaults(){
			Item.width = 20;
			Item.height = 38;
			Item.scale = 0.78f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 2, copper: 10);
		}
	}
}
