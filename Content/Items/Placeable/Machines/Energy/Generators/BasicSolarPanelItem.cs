﻿using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;

namespace TerraScience.Content.Items.Placeable.Machines.Energy.Generators{
	public class BasicSolarPanelItem : MachineItem<BasicSolarPanel>{
		public override string Texture => "TerraScience/Content/Items/Placeable/Machines/TemporaryMachineSprite";

		public override string ItemName => "Basic Solar Panel";
		public override string ItemTooltip => "Generates electricity based on the time of day and weather";

		public override void SafeSetDefaults(){
			item.width = 24;
			item.height = 24;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Orange;
			item.value = Item.buyPrice(silver: 6, copper: 50);
		}
	}
}
