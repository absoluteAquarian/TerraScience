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
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("anim"), frameX: tick % 5, frameY: tick % 30 / 5, columnCount: 5, rowCount: 6, buffer: 2),
				ItemTooltip,
				consumeTFLine: "Per game tick, " + GetMachineFluxUsageString(perGameTick: true),
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 30;
			Item.scale = 0.9f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 1);
		}
	}
}
