using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class FluidTankItem : MachineItem<FluidTank>{
		public override string ItemName => "Fluid Tank";

		public override string ItemTooltip => "Stores liquids and gases";

		public override void SafeSetDefaults(){
			Item.width = 26;
			Item.height = 40;
			Item.scale = 0.7f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 5, copper: 20);
		}
	}
}
