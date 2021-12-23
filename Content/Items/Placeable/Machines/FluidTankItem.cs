using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class FluidTankItem : MachineItem<FluidTank>{
		public override string ItemName => "Fluid Tank";

		public override string ItemTooltip => "Stores liquids and gases";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("empty")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("full")),
				ItemTooltip,
				consumeTFLine: null,
				produceTFLine: null);

		public override void SafeSetDefaults(){
			item.width = 26;
			item.height = 40;
			item.scale = 0.7f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 5, copper: 20);
		}
	}
}
