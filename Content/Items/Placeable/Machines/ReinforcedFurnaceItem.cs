using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class ReinforcedFurnaceItem : MachineItem<ReinforcedFurnace>{
		public override string ItemName => "Reinforced Furnace";
		public override string ItemTooltip => "Burns wood into Coal";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("closed")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("openactive")),
				ItemTooltip,
				consumeTFLine: null,
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 40;
			Item.height = 46;
			Item.scale = 0.62f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}
}
