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
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("anim"), frameY: tick % 48 / 4, rowCount: 12, buffer: 2),
				ItemTooltip,
				consumeTFLine: "Per game tick, " + GetMachineFluxUsageString(perGameTick: true),
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 32;
			Item.scale = 0.8f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 6, copper: 40);
		}
	}
}
