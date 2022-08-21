using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy{
	public class AutoExtractinatorItem : MachineItem<AutoExtractinator>{
		public override string ItemName => "Auto-Extractinator";
		public override string ItemTooltip => "Automatically extracts items from blocks that you can put in the Extractinator";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("empty")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("block"), frameY: tick % 45 / 5, rowCount: 9, buffer: 2),
				"Automatically extracts items from blocks that you can put" +
				"\nin the Extractinator",
				consumeTFLine: "Per game tick, " + GetMachineFluxUsageString(perGameTick: true),
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 32;
			Item.height = 36;
			Item.scale = 0.55f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 8, copper: 65);
		}
	}
}
