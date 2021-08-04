using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class LiquidDuplicatorItem : MachineItem<LiquidDuplicator>{
		public override string ItemName => "Liquid Duplicator";

		public override string ItemTooltip => "Consumes Terra Flux to duplicate certain liquids";

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 32;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 8, copper: 20);
		}
	}
}
