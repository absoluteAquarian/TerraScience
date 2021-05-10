using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;
using TerraScience.API.Classes.ModLiquid;
using TerraScience.Content.Items.Energy;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.Items.Placeable.Machines.Energy.Generators;
using TerraScience.Content.Items.Placeable.Machines.Energy.Storage;
using TerraScience.Content.Items.Weapons;
using TerraScience.Content.Projectiles;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Content.Tiles.Energy;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;
using TerraScience.Content.UI;
using TerraScience.Systems.Energy;
using TerraScience.Systems.TemperatureSystem;
using TerraScience.Utilities;

namespace TerraScience {
	public class TerraScience : Mod {
		public static class ScienceRecipeGroups{
			public const string Sand = "TerrasScience: Sand Blocks";
		}

		/// <summary>
		/// For fast and easy access to this mod's instance when one doesn't exist already
		/// </summary>
		public static TerraScience Instance => ModContent.GetInstance<TerraScience>();

		/// <summary>
		/// A <seealso cref="WeightedRandom{(int, int)}"/> used by <seealso cref="AirIonizerEntity"/>.
		/// Initialized during <seealso cref="Load"/>
		/// </summary>
		public static WeightedRandom<(int, int)> wRand;

		public static readonly Action<ModRecipe> NoRecipe = r => { };
		public static readonly Action<ModRecipe> OnlyWorkBench = r => { r.AddTile(TileID.WorkBenches); };

		// TODO: Move this into a MaterialLoader?
		public static readonly Action<Item> VialDefaults = i => {
			i.maxStack = 99;
			i.width = 26;
			i.height = 26;
			i.useStyle = ItemUseStyleID.SwingThrow;
			i.useTime = 15;
			i.useAnimation = 10;
			i.autoReuse = true;
			i.useTurn = true;
			i.noMelee = true;
		};

		internal MachineUILoader machineLoader;

		internal ModLiquidLoader LiquidLoader { get; private set; }

		internal ModLiquidFactory LiquidFactory { get; private set; }

		internal TemperatureSystem temperatureSystem;

		public static ModHotKey DebugHotkey;

		public const string WarningWaterExplode = "[c/bb3300:WARNING:] exposure to water may cause spontaneous combustion!";

		public override void Load() {
			//Consistent randomness with Main
			wRand = new WeightedRandom<(int, int)>(Main.rand);

			Logger.DebugFormat("Loading Factories and system Loaders...");

			LiquidLoader = new ModLiquidLoader();
			LiquidFactory = new ModLiquidFactory();
			machineLoader = new MachineUILoader();
			temperatureSystem = new TemperatureSystem();

			Logger.DebugFormat("Initializing dictionaries...");

			DebugHotkey = RegisterHotKey("Debuging", "J");

			TileUtils.tileToEntity = new Dictionary<int, MachineEntity>();
			TileUtils.tileToStructureName = new Dictionary<int, string>();

			DatalessMachineInfo.recipes = new Dictionary<int, Action<Mod>>();

			Logger.DebugFormat("Adding other content...");

			AddProjectile("PepperDust", new ShakerDust("Pepper Dust", new Color(){ PackedValue = 0xff2a2a2a }));
			AddProjectile("SaltDust", new ShakerDust("Salt Dust", new Color(){ PackedValue = 0xffd5d5d5 }));

			AddItem("Shaker_Pepper", new Shaker("Pepper Shaker",
				"\"Time to spice up the competition a bit!\"",
				() => ModContent.ItemType<Capsaicin>(),
				item => {
					item.damage = 28;
					item.knockBack = 5.735f;
					item.rare = ItemRarityID.Blue;
					item.value = Item.sellPrice(silver: 4, copper: 20);
					item.shoot = ProjectileType("PepperDust");
				}));
			AddItem("Shaker_Salt", new Shaker("Salt Shaker",
				"\"Enemy #1 in the Slug Kingdom\"",
				() => ModContent.ItemType<Salt>(),
				item => {
					item.damage = 11;
					item.knockBack = 5.735f;
					item.rare = ItemRarityID.Blue;
					item.value = Item.sellPrice(silver: 4, copper: 20);
					item.shoot = ProjectileType("SaltDust");
				}));

			AddDatalessMachineItem<SaltExtractorItem>();
			AddDatalessMachineItem<ScienceWorkbenchItem>();
			AddDatalessMachineItem<ReinforcedFurnaceItem>();
			AddDatalessMachineItem<AirIonizerItem>();
			AddDatalessMachineItem<ElectrolyzerItem>();
			AddDatalessMachineItem<BlastFurnaceItem>();
			AddDatalessMachineItem<BasicWindTurbineItem>();
			AddDatalessMachineItem<BasicBatteryItem>();
			AddDatalessMachineItem<AutoExtractinatorItem>();
			AddDatalessMachineItem<BasicSolarPanelItem>();
			AddDatalessMachineItem<GreenhouseItem>();

			Main.OnTick += OnUpdate;

			Logger.DebugFormat("Inializing machines and UI...");

			LiquidLoader.LoadLiquids(LiquidFactory);
			machineLoader.Load();

			// TODO: finish this
			//Set the Air Ionizer stuff
			/*
			AirIonizerEntity.ResultTypes = new List<int>(){
				ItemType(ElementUtils.ElementName(Element.Nitrogen)),
				ItemType(ElementUtils.ElementName(Element.Oxygen)),
				ItemType(ElementUtils.ElementName(Element.Argon)),
				ItemType(CompoundUtils.CompoundName(Compound.CarbonDioxide)),
				ItemType(ElementUtils.ElementName(Element.Neon)),
				ItemType(ElementUtils.ElementName(Element.Helium)),
				ItemType(CompoundUtils.CompoundName(Compound.Methane)),
				ItemType(ElementUtils.ElementName(Element.Krypton)),
				ItemType(ElementUtils.ElementName(Element.Hydrogen))
			};

			AirIonizerEntity.ResultWeights = new List<double>(){
				78.084, 20.946, 0.9340, 0.0407, 0.001818, 0.000524, 0.00018, 0.000114, 0.000055
			};

			AirIonizerEntity.ResultStacks = new List<int>(){
				2, 2, 2, 1, 2, 2, 1, 2, 2
			};
			*/

			BlastFurnaceEntity.ingredientToResult = new Dictionary<int, (int, int)>(){
				[ItemID.CopperOre] = (ItemID.CopperBar, 2),
				[ItemID.IronOre] = (ItemID.IronBar, 2),
				[ItemID.SilverOre] = (ItemID.SilverBar, 2),
				[ItemID.GoldOre] = (ItemID.GoldBar, 2),
				[ItemID.TinOre] = (ItemID.TinBar, 2),
				[ItemID.LeadOre] = (ItemID.LeadBar, 2),
				[ItemID.TungstenOre] = (ItemID.TungstenBar, 2),
				[ItemID.PlatinumOre] = (ItemID.PlatinumBar, 2)
			};
		}

		private void AddDatalessMachineItem<T>() where T : MachineItem{
			string name = typeof(T).Name;
			
			if(!name.EndsWith("Item"))
				throw new ArgumentException("Machine item type had an unexpected name: " + name);

			AddItem($"Dataless{name.Substring(0, name.LastIndexOf("Item"))}", new DatalessMachineItem<T>());
		}

		public override void AddRecipeGroups(){
			RegisterRecipeGroup(ScienceRecipeGroups.Sand, ItemID.SandBlock, new int[]{ ItemID.SandBlock, ItemID.CrimsandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock });
		}

		private static void RegisterRecipeGroup(string groupName, int itemForAnyName, int[] validTypes)
			=> RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(itemForAnyName)}", validTypes));

		private void OnUpdate() {
			if (LiquidFactory.Liquids != null) {
				foreach (var liquid in LiquidFactory.Liquids) {
					liquid.Value.Update();
				}
			}
		}

		public override void PreUpdateEntities(){
			Main.tileSolid[ModContent.TileType<TFWireTile>()] = false;
		}

		public override void Unload() {
			//Revert the sand blocks to their original extractinator state
			ItemID.Sets.ExtractinatorMode[ItemID.SandBlock] = -1;
			ItemID.Sets.ExtractinatorMode[ItemID.EbonsandBlock] = -1;
			ItemID.Sets.ExtractinatorMode[ItemID.CrimsandBlock] = -1;
			ItemID.Sets.ExtractinatorMode[ItemID.PearlsandBlock] = -1;

			Logger.DebugFormat("Unloading dictionaries...");

			TileUtils.tileToEntity = null;
			TileUtils.tileToStructureName = null;

			DatalessMachineInfo.recipes = null;

			DebugHotkey = null;

			Main.OnTick -= OnUpdate;

			Logger.DebugFormat("Unloading machines and UI...");

			machineLoader?.Unload();

			AirIonizerEntity.ResultTypes = null;
			AirIonizerEntity.ResultWeights = null;

			BlastFurnaceEntity.ingredientToResult = null;

			NetworkCollection.Unload();
		}

		public override void PostSetupContent() {
			Logger.DebugFormat("Loading tile data and machine structures...");

			TileUtils.Register<SaltExtractor,     SaltExtractorEntity>();
			TileUtils.Register<ScienceWorkbench,  ScienceWorkbenchEntity>();
			TileUtils.Register<ReinforcedFurnace, ReinforcedFurnaceEntity>();
			TileUtils.Register<AirIonizer,        AirIonizerEntity>();
			TileUtils.Register<Electrolyzer,      ElectrolyzerEntity>();
			TileUtils.Register<BlastFurnace,      BlastFurnaceEntity>();
			TileUtils.Register<BasicWindTurbine,  BasicWindTurbineEntity>();
			TileUtils.Register<BasicBattery,      BasicBatteryEntity>();
			TileUtils.Register<AutoExtractinator, AutoExtractinatorEntity>();
			TileUtils.Register<BasicSolarPanel,   BasicSolarPanelEntity>();
			TileUtils.Register<Greenhouse,        GreenhouseEntity>();

			DatalessMachineInfo.Register<SaltExtractorItem>(new[]{
				(ItemID.Glass, 10),                    (ModContent.ItemType<MachineSupportItem>(), 5), (ItemID.Glass, 10),
				(ModContent.ItemType<EmptyVial>(), 3), (ItemID.Torch, 30),                             (ModContent.ItemType<EmptyVial>(), 3),
				(ItemID.IronBar, 2),                   (ModContent.ItemType<TFWireItem>(), 5),         (ItemID.IronBar, 2)
			});

			DatalessMachineInfo.recipes.Add(ModContent.ItemType<ScienceWorkbenchItem>(), mod => {
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddRecipeGroup(RecipeGroupID.Wood, 20);
				recipe.AddIngredient(ItemID.CopperBar, 5);
				recipe.AddIngredient(ItemID.Glass, 8);
				recipe.AddIngredient(ItemID.GrayBrick, 30);
				recipe.AddTile(TileID.WorkBenches);
				recipe.SetResult(ModContent.ItemType<ScienceWorkbenchItem>(), 1);
				recipe.AddRecipe();
			});

			DatalessMachineInfo.Register<ReinforcedFurnaceItem>(new[]{
				(ItemID.GrayBrick, 5), (ItemID.RedBrick, 5), (ItemID.GrayBrick, 5),
				(ItemID.RedBrick, 5),  (ItemID.TinBar, 8),   (ItemID.RedBrick, 5),
				(ItemID.GrayBrick, 5), (ItemID.RedBrick, 5), (ItemID.GrayBrick, 5)
			});
			DatalessMachineInfo.Register<AirIonizerItem>(new[]{
				(ItemID.TinBar, 3),    (ItemID.TinBar, 3),                           (ItemID.TinBar, 3),
				(ItemID.TinBar, 3),    (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.TinBar, 3),
				(ItemID.GrayBrick, 8), (ItemID.GrayBrick, 8),                        (ItemID.GrayBrick, 8)
			});
			DatalessMachineInfo.Register<ElectrolyzerItem>(new[]{
				(ItemID.IronBar, 5), (ModContent.ItemType<IronPipe>(), 2),         (ItemID.IronBar, 5),
				(ItemID.Glass, 20),  (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.Glass, 20),
				(ItemID.IronBar, 5), (ItemID.IronBar, 5),                          (ItemID.IronBar, 5)
			});
			DatalessMachineInfo.Register<BlastFurnaceItem>(new[]{
				(ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5),
				(ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5),
				(ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5), (ModContent.ItemType<BlastBrick>(), 5)
			});
			DatalessMachineInfo.Register<BasicWindTurbineItem>(new[]{
				(ModContent.ItemType<MachineSupportItem>(), 5), (ModContent.ItemType<WindmillSail>(), 4),     (ModContent.ItemType<MachineSupportItem>(), 5),
				(ItemID.IronBar, 6),                            (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.IronBar, 6),
				(ItemID.IronBar, 6),                            (ModContent.ItemType<TFWireItem>(), 10),      (ItemID.IronBar, 6)
			});
			DatalessMachineInfo.Register<BasicBatteryItem>(new[]{
				(ItemID.IronBar, 5),   (ItemID.CopperBar, 4),                        (ItemID.IronBar, 5),
				(ItemID.TinBar, 4),    (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.TinBar, 4),
				(ItemID.IronBar, 5),   (ItemID.CopperBar, 4),                        (ItemID.IronBar, 5)
			});
			DatalessMachineInfo.Register<AutoExtractinatorItem>(new[]{
				(ModContent.ItemType<Silicon>(), 10), (ItemID.IronBar, 8),                          (ModContent.ItemType<Silicon>(), 10),
				(ItemID.WaterBucket, 2),              (ItemID.Extractinator, 1),                    (ItemID.WaterBucket, 2),
				(ItemID.IronBar, 8),                  (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.IronBar, 8)
			});
			DatalessMachineInfo.Register<BasicSolarPanelItem>(new[]{
				(ItemID.Glass, 5),                    (ItemID.Glass, 8),                            (ItemID.Glass, 5),
				(ModContent.ItemType<Silicon>(), 5),  (ModContent.ItemType<BasicMachineCore>(), 1), (ModContent.ItemType<Silicon>(), 5),
				(ItemID.IronBar, 4),                  (ModContent.ItemType<TFWireItem>(), 10),      (ItemID.IronBar, 4)
			});
			DatalessMachineInfo.recipes.Add(ModContent.ItemType<GreenhouseItem>(), mod => {
				ScienceRecipe recipe = new ScienceRecipe(mod);
				recipe.AddIngredient(ItemID.Glass, 4);
				recipe.AddIngredient(ItemID.Glass, 4);
				recipe.AddIngredient(ItemID.Glass, 4);

				recipe.AddIngredient(ItemID.Glass, 4);
				recipe.AddIngredient(ModContent.ItemType<BasicMachineCore>(), 1);
				recipe.AddIngredient(ItemID.Glass, 4);
					
				recipe.AddRecipeGroup(RecipeGroupID.Wood, 30);
				recipe.AddIngredient(ModContent.ItemType<TFWireItem>(), 8);
				recipe.AddRecipeGroup(RecipeGroupID.Wood, 30);

				recipe.AddTile(ModContent.TileType<ScienceWorkbench>());
				recipe.SetResult(mod.ItemType("DatalessGreenhouse"), 1);
				recipe.AddRecipe();
			});

			//Loading merge data here instead of TFWireTile.SetDefaults
			int wireType = ModContent.TileType<TFWireTile>();
			foreach(var pair in TileUtils.tileToEntity){
				if(pair.Value is PoweredMachineEntity){
					Main.tileMerge[wireType][pair.Key] = true;
					Main.tileMerge[pair.Key][wireType] = true;
				}
			}
		}

		public override void AddRecipes(){
			AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Wood);
			AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.Ebonwood);
			AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.Shadewood);
			AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ItemID.HallowedSeeds, ItemID.Pearlwood);
			AddGreenhouseRecipe(ItemID.Acorn, ItemID.DirtBlock, ModContent.ItemType<Vial_Saltwater>(), ItemID.PalmWood);
			AddGreenhouseRecipe(ItemID.Acorn, ItemID.SnowBlock, null, ItemID.BorealWood);
			AddGreenhouseRecipe(ItemID.Acorn, ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.RichMahogany);

			AddGreenhouseRecipe(ItemID.DaybloomSeeds, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Daybloom);
			AddGreenhouseRecipe(ItemID.BlinkrootSeeds, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Blinkroot);
			AddGreenhouseRecipe(ItemID.ShiverthornSeeds, ItemID.SnowBlock, null, ItemID.Shiverthorn);
			AddGreenhouseRecipe(ItemID.WaterleafSeeds, ItemID.SandBlock, null, ItemID.Waterleaf);
			AddGreenhouseRecipe(ItemID.MoonglowSeeds, ItemID.MudBlock, ItemID.JungleGrassSeeds, ItemID.Moonglow);
			AddGreenhouseRecipe(ItemID.DeathweedSeeds, ItemID.DirtBlock, ItemID.CorruptSeeds, ItemID.Deathweed);
			AddGreenhouseRecipe(ItemID.DeathweedSeeds, ItemID.DirtBlock, ItemID.CrimsonSeeds, ItemID.Deathweed);
			AddGreenhouseRecipe(ItemID.Fireblossom, ItemID.AshBlock, null, ItemID.Fireblossom);

			AddGreenhouseRecipe(ItemID.MushroomGrassSeeds, ItemID.MudBlock, null, ItemID.GlowingMushroom);

			AddGreenhouseRecipe(ItemID.Cactus, ItemID.SandBlock, null, ItemID.Cactus);
		}

		private void AddGreenhouseRecipe(int inputType, int blockType, int? modifierType, int resultType){
			ScienceRecipe recipe = new ScienceRecipe(this);
			recipe.AddIngredient(inputType, 1);
			recipe.AddIngredient(blockType, 1);
			if(modifierType is int modifier)
				recipe.AddIngredient(modifier, 1);
			recipe.AddTile(ModContent.TileType<Greenhouse>());
			recipe.SetResult(resultType, 1);
			recipe.AddRecipe();
		}

		public override void UpdateUI(GameTime gameTime) {
			machineLoader.UpdateUI(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			machineLoader.ModifyInterfaceLayers(layers);
		}

		// -- Types --
		// Call("Get Machine Entity", new Point16(x, y))
		//	- gets the MachineEntity at the tile position, if one exists there
		// Call("Add Compound")
		//  - todo, does nothing
		public override object Call(params object[] args) {
			//People who don't use the exact call name are dumb.  We shouldn't have to make sure they typed the name correctly

			switch((string)args[0]){
				case "Get Machine Entity":
					MiscUtils.TryGetTileEntity((Point16)args[1], out MachineEntity entity);
					return entity;
			}

			return base.Call(args);
		}
	}
}