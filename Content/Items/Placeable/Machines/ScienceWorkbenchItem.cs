using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class ScienceWorkbenchItem : MachineItem<ScienceWorkbench>{
		public override string ItemName => "Science Workbench";
		public override string ItemTooltip => "Displays information about the machines in Terraria Tech Mod";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile")),
				tick => null,
				ItemTooltip,
				consumeTFLine: null,
				produceTFLine: null);

		public override void SafeSetDefaults(){
			item.width = 24;
			item.height = 24;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 4, copper: 30);
		}
	}
}
