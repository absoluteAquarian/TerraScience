using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class PulverizerItem : MachineItem<Pulverizer>{
		public override string ItemName => "Pulverizer";

		public override string ItemTooltip => "Crushes certain blocks into powder and other useful materials";

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 32;
			Item.scale = 0.8f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 6, copper: 40);
		}
	}
}
