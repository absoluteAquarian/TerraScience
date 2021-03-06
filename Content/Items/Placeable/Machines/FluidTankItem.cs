﻿using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class FluidTankItem : MachineItem<FluidTank>{
		public override string ItemName => "Fluid Tank";

		public override string ItemTooltip => "Stores liquids and gases";

		public override void SafeSetDefaults(){
			item.width = 26;
			item.height = 40;
			item.scale = 0.7f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 5, copper: 20);
		}
	}
}
