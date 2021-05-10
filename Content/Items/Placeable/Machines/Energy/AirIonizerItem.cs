using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class AirIonizerItem : MachineItem<AirIonizer>{
		public override string ItemName => "Air Ionizer";
		public override string ItemTooltip => "TODO";

		public override void SafeSetDefaults(){
			item.width = 40;
			item.height = 46;
			item.scale = 0.62f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 10, copper: 5);
		}
	}
}