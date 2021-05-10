using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Utilities{
	public static class VanillaMethods{
		/// <summary>
		/// A modified version of the Player.ExtractinatorUse to feed the item directly into the Auto-Extractinator
		/// </summary>
		/// <param name="extractType">The item type used for extracting</param>
		/// <param name="type">The item type extracted.  A type of 0 means nothing was extracted</param>
		/// <param name="stack">The stack of the item type extracted</param>
		public static void Player_ExtractinatorUse(int extractType, out int type, out int stack){
			int amberMosquitoChance = 5000;
			int gemsNotAmberChance = 25;
			int gemsAmberChance = 50;
			int fossilOreChance = -1;
			if(extractType == ItemID.DesertFossil){
				amberMosquitoChance /= 3;
				gemsNotAmberChance *= 2;
				gemsAmberChance /= 2;
				fossilOreChance = 10;
			}

			stack = 1;
			if(fossilOreChance != -1 && Main.rand.Next(fossilOreChance) == 0){
				type = ItemID.FossilOre;
				if(Main.rand.Next(5) == 0)
					stack += Main.rand.Next(2);

				if(Main.rand.Next(10) == 0)
					stack += Main.rand.Next(3);

				if(Main.rand.Next(15) == 0)
					stack += Main.rand.Next(4);
			}else if(Main.rand.Next(2) == 0){
				if(Main.rand.Next(12000) == 0){
					type = ItemID.PlatinumCoin;
					if(Main.rand.Next(14) == 0)
						stack += Main.rand.Next(0, 2);

					if(Main.rand.Next(14) == 0)
						stack += Main.rand.Next(0, 2);

					if(Main.rand.Next(14) == 0)
						stack += Main.rand.Next(0, 2);
				}else if(Main.rand.Next(800) == 0){
					type = ItemID.GoldCoin;
					if(Main.rand.Next(6) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(6) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(6) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(6) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(6) == 0)
						stack += Main.rand.Next(1, 20);
				}else if(Main.rand.Next(60) == 0){
					type = ItemID.SilverCoin;
					if(Main.rand.Next(4) == 0)
						stack += Main.rand.Next(5, 26);

					if(Main.rand.Next(4) == 0)
						stack += Main.rand.Next(5, 26);

					if(Main.rand.Next(4) == 0)
						stack += Main.rand.Next(5, 26);

					if(Main.rand.Next(4) == 0)
						stack += Main.rand.Next(5, 25);
				}else{
					type = ItemID.CopperCoin;
					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(10, 26);

					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(10, 26);

					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(10, 26);

					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(10, 25);
				}
			}else if(amberMosquitoChance != -1 && Main.rand.Next(amberMosquitoChance) == 0){
				type = ItemID.AmberMosquito;
			}else if(gemsNotAmberChance != -1 && Main.rand.Next(gemsNotAmberChance) == 0){
				switch (Main.rand.Next(6)){
					case 0:
						type = ItemID.Amethyst;
						break;
					case 1:
						type = ItemID.Topaz;
						break;
					case 2:
						type = ItemID.Sapphire;
						break;
					case 3:
						type = ItemID.Emerald;
						break;
					case 4:
						type = ItemID.Ruby;
						break;
					default:
						type = ItemID.Diamond;
						break;
				}

				if(Main.rand.Next(20) == 0)
					stack += Main.rand.Next(0, 2);

				if(Main.rand.Next(30) == 0)
					stack += Main.rand.Next(0, 3);

				if(Main.rand.Next(40) == 0)
					stack += Main.rand.Next(0, 4);

				if(Main.rand.Next(50) == 0)
					stack += Main.rand.Next(0, 5);

				if(Main.rand.Next(60) == 0)
					stack += Main.rand.Next(0, 6);
			}else if(gemsAmberChance != -1 && Main.rand.Next(gemsAmberChance) == 0){
				type = ItemID.Amber;
				if(Main.rand.Next(20) == 0)
					stack += Main.rand.Next(0, 2);

				if(Main.rand.Next(30) == 0)
					stack += Main.rand.Next(0, 3);

				if(Main.rand.Next(40) == 0)
					stack += Main.rand.Next(0, 4);

				if(Main.rand.Next(50) == 0)
					stack += Main.rand.Next(0, 5);

				if(Main.rand.Next(60) == 0)
					stack += Main.rand.Next(0, 6);
			}else if(Main.rand.Next(3) == 0){
				if(Main.rand.Next(5000) == 0){
					type = ItemID.PlatinumCoin;
					if(Main.rand.Next(10) == 0)
						stack += Main.rand.Next(0, 3);

					if(Main.rand.Next(10) == 0)
						stack += Main.rand.Next(0, 3);

					if(Main.rand.Next(10) == 0)
						stack += Main.rand.Next(0, 3);

					if(Main.rand.Next(10) == 0)
						stack += Main.rand.Next(0, 3);

					if(Main.rand.Next(10) == 0)
						stack += Main.rand.Next(0, 3);
				}else if(Main.rand.Next(400) == 0){
					type = ItemID.GoldCoin;
					if(Main.rand.Next(5) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(5) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(5) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(5) == 0)
						stack += Main.rand.Next(1, 21);

					if(Main.rand.Next(5) == 0)
						stack += Main.rand.Next(1, 20);
				}else if(Main.rand.Next(30) == 0){
					type = ItemID.SilverCoin;
					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(5, 26);

					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(5, 26);

					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(5, 26);

					if(Main.rand.Next(3) == 0)
						stack += Main.rand.Next(5, 25);
				}else{
					type = ItemID.CopperCoin;
					if(Main.rand.Next(2) == 0)
						stack += Main.rand.Next(10, 26);

					if(Main.rand.Next(2) == 0)
						stack += Main.rand.Next(10, 26);

					if(Main.rand.Next(2) == 0)
						stack += Main.rand.Next(10, 26);

					if(Main.rand.Next(2) == 0)
						stack += Main.rand.Next(10, 25);
				}
			}else{
				switch(Main.rand.Next(8)){
					case 0:
						type = ItemID.CopperOre;
						break;
					case 1:
						type = ItemID.IronOre;
						break;
					case 2:
						type = ItemID.SilverOre;
						break;
					case 3:
						type = ItemID.GoldOre;
						break;
					case 4:
						type = ItemID.TinOre;
						break;
					case 5:
						type = ItemID.LeadOre;
						break;
					case 6:
						type = ItemID.TungstenOre;
						break;
					default:
						type = ItemID.PlatinumOre;
						break;
				}

				if(Main.rand.Next(20) == 0)
					stack += Main.rand.Next(0, 2);

				if(Main.rand.Next(30) == 0)
					stack += Main.rand.Next(0, 3);

				if(Main.rand.Next(40) == 0)
					stack += Main.rand.Next(0, 4);

				if(Main.rand.Next(50) == 0)
					stack += Main.rand.Next(0, 5);

				if(Main.rand.Next(60) == 0)
					stack += Main.rand.Next(0, 6);
			}

			ItemLoader.ExtractinatorUse(ref type, ref stack, extractType);
		}
	}
}
