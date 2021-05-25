using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class ElectrolyzerItem : MachineItem<Electrolyzer>{
		public override string ItemName => "Electrolyzer";
		public override string ItemTooltip => "TODO";

		public override void SafeSetDefaults(){
			item.width = 24;
			item.height = 24;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}