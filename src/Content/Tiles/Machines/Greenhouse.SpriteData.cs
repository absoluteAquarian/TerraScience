using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SerousEnergyLib.API;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.API;

namespace TerraScience.Content.Tiles.Machines {
	partial class Greenhouse {
		private static void SetSpriteData() {
			if (Main.dedServ)
				return;

			// Soil sprites
			Vector2 soilSpriteOffset = new Vector2(4, 24);
			string soilSpritePath = TechMod.GetEffectPath<Greenhouse>("soil");
			Texture2D soilSheet = ModContent.Request<Texture2D>(soilSpritePath, AssetRequestMode.ImmediateLoad).Value;

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.DirtBlock, ItemID.None,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 10), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.DirtBlock, ItemID.GrassSeeds,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 0), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.DirtBlock, ItemID.CorruptSeeds,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 1), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.DirtBlock, ItemID.CrimsonSeeds,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 2), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.DirtBlock, ItemID.HallowedSeeds,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 5), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.StoneBlock, ItemID.None,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 11), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.MudBlock, ItemID.JungleGrassSeeds,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 3), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.MudBlock, ItemID.MushroomGrassSeeds,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 4), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.SandBlock, ItemID.None,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 7), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.SnowBlock, ItemID.None,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 8), true));

			TechMod.Sets.Greenhouse.AddSoilEffect(ItemID.AshBlock, ItemID.None,
				new MachineSpriteEffectInformation(soilSpritePath, soilSpriteOffset, soilSheet.Frame(1, 12, 0, 9), true));

			// Plant sprites
			string immatureHerbPath = "Terraria/Images/Tiles_" + TileID.ImmatureHerbs;
			Texture2D immatureHerbSheet = ModContent.Request<Texture2D>(immatureHerbPath, AssetRequestMode.ImmediateLoad).Value;
			string matureHerbPath = "Terraria/Images/Tiles_" + TileID.MatureHerbs;
			Texture2D matureHerbSheet = ModContent.Request<Texture2D>(matureHerbPath, AssetRequestMode.ImmediateLoad).Value;
			string bloomingHerbPath = "Terraria/Images/Tiles_" + TileID.BloomingHerbs;
			Texture2D bloomingHerbSheet = ModContent.Request<Texture2D>(bloomingHerbPath, AssetRequestMode.ImmediateLoad).Value;
			
			string herbGlowmaskPath = TechMod.GetEffectPath<Greenhouse>("plant_glow");
			Texture2D herbGlowmaskSheet = ModContent.Request<Texture2D>(herbGlowmaskPath, AssetRequestMode.ImmediateLoad).Value;

			string saplingPath = "Terraria/Images/Tiles_" + TileID.Saplings;
			Texture2D saplingSheet = ModContent.Request<Texture2D>(saplingPath, AssetRequestMode.ImmediateLoad).Value;

			string pumpkinPath = TechMod.GetEffectPath<Greenhouse>("pumpkin");
			Texture2D pumpkinSheet = ModContent.Request<Texture2D>(pumpkinPath, AssetRequestMode.ImmediateLoad).Value;

			string cactusPath = "Terraria/Images/Tiles_" + TileID.Cactus;
			Texture2D cactusSheet = ModContent.Request<Texture2D>(cactusPath, AssetRequestMode.ImmediateLoad).Value;

			string bambooPath = TechMod.GetEffectPath<Greenhouse>("bamboo");

			string mushroomPath = "Terraria/Images/Tiles_" + TileID.MushroomPlants;
			Texture2D mushroomSheet = ModContent.Request<Texture2D>(mushroomPath, AssetRequestMode.ImmediateLoad).Value;

			string mushroomGlowmaskPath = TechMod.GetEffectPath<Greenhouse>("mushroom_glow");
			Texture2D mushroomGlowmaskSheet = ModContent.Request<Texture2D>(mushroomGlowmaskPath, AssetRequestMode.ImmediateLoad).Value;

			string gemSaplingPath = "Terraria/Images/Tiles_" + TileID.GemSaplings;
			Texture2D gemSaplingSheet = ModContent.Request<Texture2D>(gemSaplingPath, AssetRequestMode.ImmediateLoad).Value;
			
			Vector2 herbOffset = new Vector2(8, 6);

			Vector2 blinkrootOffset = herbOffset + new Vector2(0, -2);

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.None, ItemID.BlinkrootSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, blinkrootOffset, immatureHerbSheet.Frame(7, 1, 2, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   blinkrootOffset,   matureHerbSheet.Frame(7, 1, 2, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, blinkrootOffset, bloomingHerbSheet.Frame(7, 1, 2, 0), affectedByLight: true),
				glowmaskSprite: new MachineSpriteEffectInformation(herbGlowmaskPath, herbOffset, herbGlowmaskSheet.Frame(7, 1, 2, 0), affectedByLight: false)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Acorn,
				new MachineSpriteEffectInformation(saplingPath, new Vector2(8, 2), saplingSheet.Frame(30, 1, 0, 0), affectedByLight: true)));

			Vector2 pumpkinOffset = new Vector2(4, 6);

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.PumpkinSeed,
				new MachineSpriteEffectInformation(pumpkinPath, pumpkinOffset, pumpkinSheet.Frame(1, 3, 0, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(pumpkinPath, pumpkinOffset, pumpkinSheet.Frame(1, 3, 0, 1), affectedByLight: true),
				new MachineSpriteEffectInformation(pumpkinPath, pumpkinOffset, pumpkinSheet.Frame(1, 3, 0, 2), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.DaybloomSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, herbOffset, immatureHerbSheet.Frame(7, 1, 0, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   herbOffset,   matureHerbSheet.Frame(7, 1, 0, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, herbOffset, bloomingHerbSheet.Frame(7, 1, 0, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.Acorn,
				new MachineSpriteEffectInformation(saplingPath, new Vector2(8, 2), saplingSheet.Frame(30, 1, 11, 0), affectedByLight: true)));

			Vector2 deathweedOffset = herbOffset + new Vector2(2, 2);

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.DeathweedSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, deathweedOffset, immatureHerbSheet.Frame(7, 1, 3, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   deathweedOffset,   matureHerbSheet.Frame(7, 1, 3, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, deathweedOffset, bloomingHerbSheet.Frame(7, 1, 3, 0), affectedByLight: true),
				glowmaskSprite: new MachineSpriteEffectInformation(herbGlowmaskPath, herbOffset, herbGlowmaskSheet.Frame(7, 1, 3, 0), affectedByLight: false),
				// Color derived from TileDrawing.EmitAlchemyHerbParticles() and Dust.UpdateDust()
				emitLight: new Color(new Vector3(0.6f, 0.2f, 1f))));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.Acorn,
				new MachineSpriteEffectInformation(saplingPath, new Vector2(8, 0), saplingSheet.Frame(30, 1, 13, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.DeathweedSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, deathweedOffset, immatureHerbSheet.Frame(7, 1, 3, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   deathweedOffset,   matureHerbSheet.Frame(7, 1, 3, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, deathweedOffset, bloomingHerbSheet.Frame(7, 1, 3, 0), affectedByLight: true),
				glowmaskSprite: new MachineSpriteEffectInformation(herbGlowmaskPath, herbOffset, herbGlowmaskSheet.Frame(7, 1, 3, 0), affectedByLight: false),
				// Color derived from TileDrawing.EmitAlchemyHerbParticles() and Dust.UpdateDust()
				emitLight: new Color(new Vector3(0.6f, 0.2f, 1f))));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.DirtBlock, ItemID.HallowedSeeds, ItemID.Acorn,
				new MachineSpriteEffectInformation(saplingPath, new Vector2(8, 2), saplingSheet.Frame(30, 1, 16, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.SandBlock, ItemID.None, ItemID.Acorn,
				new MachineSpriteEffectInformation(saplingPath, new Vector2(8, -4), saplingSheet.Frame(30, 1, 18, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.SandBlock, ItemID.None, ItemID.Cactus,
				new MachineSpriteEffectInformation(cactusPath, new Vector2(8, 6), new Rectangle(0, 0, 16, 28), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.SandBlock, ItemID.None, ItemID.WaterleafSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, herbOffset, immatureHerbSheet.Frame(7, 1, 4, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   herbOffset,   matureHerbSheet.Frame(7, 1, 4, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, herbOffset, bloomingHerbSheet.Frame(7, 1, 4, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.Acorn,
				new MachineSpriteEffectInformation(saplingPath, new Vector2(8, -8), saplingSheet.Frame(30, 1, 6, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.BambooBlock,
				new MachineSpriteEffectInformation(bambooPath, new Vector2(8, 2), null, affectedByLight: true)));

			Vector2 moonglowOffset = herbOffset + new Vector2(2, 0);

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.MoonglowSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, moonglowOffset, immatureHerbSheet.Frame(7, 1, 1, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   moonglowOffset,   matureHerbSheet.Frame(7, 1, 1, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, moonglowOffset, bloomingHerbSheet.Frame(7, 1, 1, 0), affectedByLight: true),
				glowmaskSprite: new MachineSpriteEffectInformation(herbGlowmaskPath, herbOffset, herbGlowmaskSheet.Frame(7, 1, 1, 0), affectedByLight: false),
				// Color derived from TileDrawing.EmitAlchemyHerbParticles() and Dust.UpdateDust()
				emitLight: new Color(new Vector3(0.4f, 0.9f, 1f))));

			Vector2 mushroomOffset = new Vector2(8, 6);

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.MudBlock, ItemID.MushroomGrassSeeds, ItemID.GlowingMushroom,
				new MachineSpriteEffectInformation(mushroomPath, mushroomOffset, mushroomSheet.Frame(5, 1, 4, 0), affectedByLight: true),
				glowmaskSprite: new MachineSpriteEffectInformation(mushroomGlowmaskPath, mushroomOffset, null, affectedByLight: false),
				// Color derived from TileDrawing.DrawTiles_EmitParticles() and Dust.UpdateDust()
				emitLight: new Color(new Vector3(0.4f, 0.9f, 1f))));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.SnowBlock, ItemID.None, ItemID.Acorn,
				new MachineSpriteEffectInformation(saplingPath, new Vector2(8, -4), saplingSheet.Frame(30, 1, 3, 0), affectedByLight: true)));

			Vector2 shiverthornOffset = herbOffset + new Vector2(0, -2);

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.SnowBlock, ItemID.None, ItemID.ShiverthornSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, herbOffset, immatureHerbSheet.Frame(7, 1, 6, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   herbOffset,   matureHerbSheet.Frame(7, 1, 6, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, herbOffset, bloomingHerbSheet.Frame(7, 1, 6, 0), affectedByLight: true),
				glowmaskSprite: new MachineSpriteEffectInformation(herbGlowmaskPath, herbOffset, herbGlowmaskSheet.Frame(7, 1, 6, 0), affectedByLight: false),
				// Color derived from TileDrawing.EmitAlchemyHerbParticles() and Dust.UpdateDust()
				emitLight: new Color(50, 255, 255)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.AshBlock, ItemID.None, ItemID.FireblossomSeeds,
				new MachineSpriteEffectInformation(immatureHerbPath, herbOffset, immatureHerbSheet.Frame(7, 1, 5, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(matureHerbPath,   herbOffset,   matureHerbSheet.Frame(7, 1, 5, 0), affectedByLight: true),
				new MachineSpriteEffectInformation(bloomingHerbPath, herbOffset, bloomingHerbSheet.Frame(7, 1, 5, 0), affectedByLight: true),
				glowmaskSprite: new MachineSpriteEffectInformation(herbGlowmaskPath, herbOffset, herbGlowmaskSheet.Frame(7, 1, 5, 0), affectedByLight: false),
				// Color derived from TileDrawing.EmitAlchemyHerbParticles() and Dust.UpdateDust()
				emitLight: new Color(new Vector3(1f, 0.65f, 0.4f))));

			Vector2 gemSaplingOffset = new Vector2(8, 4);

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeTopazSeed,
				new MachineSpriteEffectInformation(gemSaplingPath, gemSaplingOffset, gemSaplingSheet.Frame(24, 1, 1, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeAmethystSeed,
				new MachineSpriteEffectInformation(gemSaplingPath, gemSaplingOffset, gemSaplingSheet.Frame(24, 1, 4, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeSapphireSeed,
				new MachineSpriteEffectInformation(gemSaplingPath, gemSaplingOffset, gemSaplingSheet.Frame(24, 1, 7, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeEmeraldSeed,
				new MachineSpriteEffectInformation(gemSaplingPath, gemSaplingOffset, gemSaplingSheet.Frame(24, 1, 10, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeRubySeed,
				new MachineSpriteEffectInformation(gemSaplingPath, gemSaplingOffset, gemSaplingSheet.Frame(24, 1, 13, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeDiamondSeed,
				new MachineSpriteEffectInformation(gemSaplingPath, gemSaplingOffset, gemSaplingSheet.Frame(24, 1, 16, 0), affectedByLight: true)));

			TechMod.Sets.Greenhouse.AddPlantEffect(new GreenhousePlantSpriteInformation(ItemID.StoneBlock, ItemID.None, ItemID.GemTreeAmberSeed,
				new MachineSpriteEffectInformation(gemSaplingPath, gemSaplingOffset, gemSaplingSheet.Frame(24, 1, 19, 0), affectedByLight: true)));
		}
	}
}
