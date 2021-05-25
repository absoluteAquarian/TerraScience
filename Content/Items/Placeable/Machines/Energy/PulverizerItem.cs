using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class PulverizerItem : MachineItem<Pulverizer>{
		public override string ItemName => "Pulverizer";

		public override string ItemTooltip => "Crushes certain blocks into powder and other useful materials";

		public override void SafeSetDefaults(){
			item.width = 32;
			item.height = 32;
			item.scale = 0.8f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 6, copper: 40);
		}
	}
}
