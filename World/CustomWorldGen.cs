using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.World.Generation;

namespace TerraScience.World{
	public static class CustomWorldGen{
		public static void MoreShinies(List<GenPass> tasks){
			int shinies = tasks.FindIndex(genpass => genpass.Name == "Shinies");
			if(shinies > 0){
				//Add the other prehm ore variants depending on what WorldGen decided on

				// WorldGen.CopperTierOre is the TileID for either copper (7) or tin (166) ore
				tasks.Insert(++shinies, new PassLegacy("Other Copper Tier", PlaceOtherCopperTier));
				// WorldGen.IronTierOre is the TileID for either iron (6) or lead (167) ore
				tasks.Insert(++shinies, new PassLegacy("Other Iron Tier", PlaceOtherIronTier));
				// WorldGen.SilverTierOre is the TileID for either silver (9) or tungsten (168) ore
				tasks.Insert(++shinies, new PassLegacy("Other Silver Tier", PlaceOtherSilverTier));
				// WorldGen.GoldTierOre is the TileID for either gold (8) or platinum (169) ore
				tasks.Insert(++shinies, new PassLegacy("Other Gold Tier", PlaceOtherGoldTier));
			}
		}

		private static void PlaceOtherCopperTier(GenerationProgress progress){
			progress.Message = "Other Copper Tier";

			int oreTier = WorldGen.CopperTierOre == TileID.Copper ? TileID.Tin : TileID.Copper;

			for (int k = 0; k < Main.maxTilesX * Main.maxTilesY * 6E-05; k++){
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, (int)WorldGen.worldSurfaceHigh), WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), oreTier);
			}
			for (int l = 0; l < Main.maxTilesX * Main.maxTilesY * 8E-05; l++){
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceHigh, (int)WorldGen.rockLayerHigh), WorldGen.genRand.Next(3, 7), WorldGen.genRand.Next(3, 7), oreTier);
			}
			for (int m = 0; m < Main.maxTilesX * Main.maxTilesY * 0.0002; m++){
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.rockLayerLow, Main.maxTilesY), WorldGen.genRand.Next(4, 9), WorldGen.genRand.Next(4, 8), oreTier);
			}
		}

		private static void PlaceOtherIronTier(GenerationProgress progress){
			progress.Message = "Other Iron Tier";

			int oreTier = WorldGen.IronTierOre == TileID.Iron ? TileID.Lead : TileID.Iron;

			for (int n = 0; n < Main.maxTilesX * Main.maxTilesY * 3E-05; n++){
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, (int)WorldGen.worldSurfaceHigh), WorldGen.genRand.Next(3, 7), WorldGen.genRand.Next(2, 5), oreTier);
			}
			for (int num = 0; num < Main.maxTilesX * Main.maxTilesY * 8E-05; num++){
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceHigh, (int)WorldGen.rockLayerHigh), WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(3, 6), oreTier);
			}
			for (int num2 = 0; num2 < Main.maxTilesX * Main.maxTilesY * 0.0002; num2++){
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.rockLayerLow, Main.maxTilesY), WorldGen.genRand.Next(4, 9), WorldGen.genRand.Next(4, 8), oreTier);
			}
		}

		private static void PlaceOtherSilverTier(GenerationProgress progress){
			progress.Message = "Other Silver Tier";

			int oreTier = WorldGen.SilverTierOre == TileID.Silver ? TileID.Tungsten : TileID.Silver;

			for (int num3 = 0; num3 < Main.maxTilesX * Main.maxTilesY * 2.6E-05; num3++)
			{
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceHigh, (int)WorldGen.rockLayerHigh), WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(3, 6), oreTier);
			}
			for (int num4 = 0; num4 < Main.maxTilesX * Main.maxTilesY * 0.00015; num4++)
			{
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.rockLayerLow, Main.maxTilesY), WorldGen.genRand.Next(4, 9), WorldGen.genRand.Next(4, 8), oreTier);
			}
			for (int num5 = 0; num5 < Main.maxTilesX * Main.maxTilesY * 0.00017; num5++)
			{
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next(0, (int)WorldGen.worldSurfaceLow), WorldGen.genRand.Next(4, 9), WorldGen.genRand.Next(4, 8), oreTier);
			}
		}

		private static void PlaceOtherGoldTier(GenerationProgress progress){
			progress.Message = "Other Gold Tier";

			int oreTier = WorldGen.GoldTierOre == TileID.Gold ? TileID.Platinum : TileID.Gold;

			for (int num6 = 0; num6 < Main.maxTilesX * Main.maxTilesY * 0.00012; num6++)
			{
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.rockLayerLow, Main.maxTilesY), WorldGen.genRand.Next(4, 8), WorldGen.genRand.Next(4, 8), oreTier);
			}
			for (int num7 = 0; num7 < Main.maxTilesX * Main.maxTilesY * 0.00012; num7++)
			{
				WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next(0, (int)WorldGen.worldSurfaceLow - 20), WorldGen.genRand.Next(4, 8), WorldGen.genRand.Next(4, 8), oreTier);
			}
		}
	}
}
