using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class AutoExtractinatorItem : MachineItem<AutoExtractinator>{
		public override string ItemName => "Auto-Extractinator";
		public override string ItemTooltip => "Automatically extracts items from blocks that you can put in the Extractinator";

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 36;
			Item.scale = 0.55f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 8, copper: 65);
		}
	}
}
