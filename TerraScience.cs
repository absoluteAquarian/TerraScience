using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;
using TerraScience.API.Classes.ModLiquid;
using TerraScience.API.StructureData;
using TerraScience.Content.Items;
using TerraScience.Content.Items.Icons;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable;
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

			//The machine icon items
			IconTemplate.allRecipes = new Dictionary<string, Action<ScienceRecipe, IconTemplate>>();

			AddIcon("SaltExtractor",
				new int[]{ ItemID.Glass, ItemID.CopperPlating, ItemID.GrayBrick },
				new int[]{ 3, 2, 4 });
			AddIcon("ScienceWorkbench",
				new int[]{ ModContent.ItemType<MachineSupportItem>(), ItemID.Wood, ItemID.CopperPlating, ItemID.GrayBrick, ItemID.Glass },
				new int[]{ 1, 3, 2, 2, 1 });

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
			Logger.DebugFormat("Unloading dictionaries...");

			IconTemplate.allRecipes = null;

			TileUtils.tileToEntity = null;
			TileUtils.tileToStructure = null;
			TileUtils.tileToStructureName = null;

			StructureExtractor.Unload();

			DebugHotkey = null;

			Main.OnTick -= OnUpdate;

			Logger.DebugFormat("Unloading machines and UI...");

			TileUtils.Structures.Unload();
			machineLoader?.Unload();

			AirIonizerEntity.ResultTypes = null;
			AirIonizerEntity.ResultWeights = null;

			BlastFurnaceEntity.ingredientToResult = null;

			NetworkCollection.Unload();
		}

		public override void PostSetupContent() {
			Logger.DebugFormat("Loading tile data and machine structures...");

			TileUtils.tileToStructureName = new Dictionary<int, string>(){
				[ModContent.TileType<SaltExtractor>()] = nameof(TileUtils.Structures.SaltExtractor),
				[ModContent.TileType<ScienceWorkbench>()] = nameof(TileUtils.Structures.ScienceWorkbench),
				[ModContent.TileType<ReinforcedFurnace>()] = nameof(TileUtils.Structures.ReinforcedFurncace),
				[ModContent.TileType<AirIonizer>()] = nameof(TileUtils.Structures.AirIonizer),
				[ModContent.TileType<Electrolyzer>()] = nameof(TileUtils.Structures.Electrolyzer),
				[ModContent.TileType<BasicWindTurbine>()] = nameof(TileUtils.Structures.BasicWindTurbine),
				[ModContent.TileType<BasicBattery>()] = nameof(TileUtils.Structures.BasicBattery),
				[ModContent.TileType<BlastFurnace>()] = nameof(TileUtils.Structures.BlastFurnace)
			};

			StructureExtractor.Load();

			TileUtils.tileToEntity = new Dictionary<int, MachineEntity>(){
				[ModContent.TileType<SaltExtractor>()] = ModContent.GetInstance<SaltExtractorEntity>(),
				[ModContent.TileType<ScienceWorkbench>()] = ModContent.GetInstance<ScienceWorkbenchEntity>(),
				[ModContent.TileType<ReinforcedFurnace>()] = ModContent.GetInstance<ReinforcedFurnaceEntity>(),
				[ModContent.TileType<AirIonizer>()] = ModContent.GetInstance<AirIonizerEntity>(),
				[ModContent.TileType<Electrolyzer>()] = ModContent.GetInstance<ElectrolyzerEntity>(),
				[ModContent.TileType<BasicWindTurbine>()] = ModContent.GetInstance<BasicWindTurbineEntity>(),
				[ModContent.TileType<BasicBattery>()] = ModContent.GetInstance<BasicBatteryEntity>(),
				[ModContent.TileType<BlastFurnace>()] = ModContent.GetInstance<BlastFurnaceEntity>()
			};

			TileUtils.tileToStructure = new Dictionary<int, Tile[,]>(){
				[ModContent.TileType<SaltExtractor>()] = TileUtils.Structures.SaltExtractor,
				[ModContent.TileType<ScienceWorkbench>()] = TileUtils.Structures.ScienceWorkbench,
				[ModContent.TileType<ReinforcedFurnace>()] = TileUtils.Structures.ReinforcedFurncace,
				[ModContent.TileType<AirIonizer>()] = TileUtils.Structures.AirIonizer,
				[ModContent.TileType<Electrolyzer>()] = TileUtils.Structures.Electrolyzer,
				[ModContent.TileType<BasicWindTurbine>()] = TileUtils.Structures.BasicWindTurbine,
				[ModContent.TileType<BasicBattery>()] = TileUtils.Structures.BasicBattery,
				[ModContent.TileType<BlastFurnace>()] = TileUtils.Structures.BlastFurnace
			};

			//Loading merge data here instead of TFWireTile.SetDefaults
			int wireType = ModContent.TileType<TFWireTile>();
			foreach(var pair in TileUtils.tileToEntity){
				if(pair.Value is PoweredMachineEntity)
					Main.tileMerge[wireType][pair.Key] = true;
			}
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

		private void AddIcon(string internalName, int[] buildMaterials, int[] buildStacks){
			AddItem($"{internalName}Icon", new IconTemplate(internalName, buildMaterials, buildStacks));
		}
	}
}