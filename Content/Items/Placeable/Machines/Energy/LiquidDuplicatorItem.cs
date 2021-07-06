using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class LiquidDuplicatorItem : MachineItem<LiquidDuplicator>{
		public override string ItemName => "Liquid Duplicator";

		public override string ItemTooltip => "Consumes Terra Flux to duplicate certain liquids";

		public override void SafeSetDefaults(){
			item.width = 32;
			item.height = 32;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 8, copper: 20);
		}
	}
}
