using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class AutoExtractinatorItem : MachineItem<AutoExtractinator>{
		public override string ItemName => "Auto-Extractinator";
		public override string ItemTooltip => "Automatically extracts items from blocks that you can put in the Extractinator";

		public override void SafeSetDefaults(){
			item.width = 32;
			item.height = 36;
			item.scale = 0.55f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 8, copper: 65);
		}
	}
}
