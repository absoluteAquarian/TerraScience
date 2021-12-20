using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class ComposterItem : MachineItem<Composter>{
		public override string ItemName => "Composter";

		public override string ItemTooltip => "Crushes plants into Dirt";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(tick => MachineTile.GetExampleTexturePath("up_plant"),
				tick => MachineTile.GetExampleTexturePath("down"),
				ItemTooltip);

		public override void SafeSetDefaults(){
			item.width = 32;
			item.height = 30;
			item.scale = 0.9f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 1);
		}
	}
}
