﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class Vial_Saltwater : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Vial of Saltwater");
			Tooltip.SetDefault("Seems like the Salt Extractor will be able to process this" +
				"\nmore efficiently than regular water...");
		}

		public override void SetDefaults(){
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 1, copper: 15);
			TechMod.VialDefaults(Item);
		}
	}
}
