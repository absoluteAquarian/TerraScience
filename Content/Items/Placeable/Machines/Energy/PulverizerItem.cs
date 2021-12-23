using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class PulverizerItem : MachineItem<Pulverizer>{
		public override string ItemName => "Pulverizer";

		public override string ItemTooltip => "Crushes certain blocks into powder and other useful materials";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("anim"), frameY: tick % 48 / 4, rowCount: 12),
				ItemTooltip,
				consumeTFLine: "Per game tick, " + GetMachineFluxUsageString(perGameTick: true),
				produceTFLine: null);

		public override void SafeSetDefaults(){
			item.width = 32;
			item.height = 32;
			item.scale = 0.8f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 6, copper: 40);
		}
	}
}
