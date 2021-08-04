using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class ElectrolyzerItem : MachineItem<Electrolyzer>{
		public override string ItemName => "Electrolyzer";
		public override string ItemTooltip => "Consumes Terra Flux and certain liquids to produce gases";

		public override void SafeSetDefaults(){
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}