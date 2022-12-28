﻿using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Generators{
	public class BasicThermoGeneratorItem : MachineItem<BasicThermoGenerator>{
		public override string ItemName => "Basic Thermal Generator";

		public override string ItemTooltip => "Converts certain foods and wooden items into Terra Flux (TF)";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("off")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("on"), frameY: tick % 20 / 10, rowCount: 2, buffer: 2),
				"Burns food and wood into Terra Flux (TF)",
				consumeTFLine: null,
				produceTFLine: "Per game tick, amount production variable");

		public override void SafeSetDefaults(){
			Item.width = 30;
			Item.height = 30;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 4, copper: 20);
		}
	}
}
