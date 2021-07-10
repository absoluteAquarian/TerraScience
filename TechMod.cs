﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;
using TerraScience.API.CrossMod;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.Content.ID;
using TerraScience.Content.Items;
using TerraScience.Content.Items.Energy;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.Items.Placeable.Machines.Energy.Generators;
using TerraScience.Content.Items.Placeable.Machines.Energy.Storage;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.Items.Weapons;
using TerraScience.Content.Projectiles;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Content.UI;
using TerraScience.Systems;
using TerraScience.Systems.Energy;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience {
	public class TechMod : Mod {
		public static class ScienceRecipeGroups{
			public const string Sand = "TerraScience: Sand Blocks";
		}

		/// <summary>
		/// For fast and easy access to this mod's instance when one doesn't exist already
		/// </summary>
		public static TechMod Instance => ModContent.GetInstance<TechMod>();

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

		public static ModHotKey DebugHotkey;

		public const string WarningWaterExplode = "[c/bb3300:WARNING:] exposure to water may cause spontaneous combustion!";

		public static bool debugging;

		internal static Type[] types;

		public override void Load(){
			Logger.DebugFormat("Loading Factories and System Loaders...");

			machineLoader = new MachineUILoader();

			Logger.DebugFormat("Initializing dictionaries...");

			DebugHotkey = RegisterHotKey("Debugging", "J");

			TileUtils.tileToEntity = new Dictionary<int, MachineEntity>();
			TileUtils.tileToStructureName = new Dictionary<int, string>();

			ElectrolyzerEntity.liquidToGas = new Dictionary<MachineLiquidID, (MachineGasID, MachineGasID)>(){
				[MachineLiquidID.Water] = (MachineGasID.Hydrogen, MachineGasID.Oxygen),
				[MachineLiquidID.Saltwater] = (MachineGasID.Hydrogen, MachineGasID.Chlorine)
			};

			DatalessMachineInfo.recipes = new Dictionary<int, Action<Mod>>();

			Capsule.containerTypes = new Dictionary<int, MachineGasID>();
			FakeCapsuleFluidItem.containerTypes = new Dictionary<int, (MachineGasID?, MachineLiquidID?)>();

			MachineMufflerTile.mufflers = new List<Point16>();

			PulverizerEntity.inputToOutputs = new Dictionary<int, WeightedRandom<(int type, int stack)>>();

			JunctionMergeable.InitMergeArray();

			NetworkCollection.networkTypeCtors = new Dictionary<Type, NetworkCollection.typeCtor>(){
				[typeof(TFWire)] = (location, network) => new TFWire(location, network),
				[typeof(ItemPipe)] = (location, network) => new ItemPipe(location, network),
				[typeof(FluidPipe)] = (location, network) => new FluidPipe(location, network)
			};

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

			MachineGasID[] gasIDs = (MachineGasID[])Enum.GetValues(typeof(MachineGasID));
			for(int i = 0; i < gasIDs.Length; i++){
				AddItem("Capsule_" + gasIDs[i].EnumName(), new Capsule());
				int id = ItemLoader.ItemCount - 1;
				Capsule.containerTypes.Add(id, gasIDs[i]);
			}

			for(int i = 1; i < gasIDs.Length; i++){
				AddItem("Fake" + gasIDs[i].EnumName() + "GasIngredient", new FakeCapsuleFluidItem());
				int id = ItemLoader.ItemCount - 1;
				FakeCapsuleFluidItem.containerTypes.Add(id, (gasIDs[i], null));
			}

			MachineLiquidID[] liquidIDs = (MachineLiquidID[])Enum.GetValues(typeof(MachineLiquidID));
			for(int i = 1; i < liquidIDs.Length; i++){
				AddItem("Fake" + liquidIDs[i].EnumName() + "LiquidIngredient", new FakeCapsuleFluidItem());
				int id = ItemLoader.ItemCount - 1;
				FakeCapsuleFluidItem.containerTypes.Add(id, (null, liquidIDs[i]));
			}

			//Register the dataless machines
			types = Code.GetTypes();
			var datalessTypeNoArgs = typeof(DatalessMachineItem<>);
			foreach(var type in types){
				//Ignore abstract types and the "DatalessMachineItem<T>" type
				if(type.IsAbstract || (type.IsGenericType && type.GetGenericTypeDefinition() == datalessTypeNoArgs))
					continue;

				if(typeof(MachineItem).IsAssignableFrom(type)){
					string name = type.Name;
			
					if(!name.EndsWith("Item"))
						throw new ArgumentException("Machine item type had an unexpected name: " + name);

					var datalessType = datalessTypeNoArgs.MakeGenericType(type);
					AddItem($"Dataless{name.Substring(0, name.LastIndexOf("Item"))}", (ModItem)Activator.CreateInstance(datalessType, new object[]{ false }));
				}
			}

			Logger.DebugFormat("Inializing machines and UI...");

			machineLoader.Load();

			AirIonizerEntity.recipes = new Dictionary<int, (int requireStack, int resultType, int resultStack, float energyUsage, float convertTimeSeconds)>(){
				[ItemID.CopperOre] =   (1, ItemID.TinOre,      1, 1000f, 8f),
				[ItemID.IronOre] =     (1, ItemID.LeadOre,     1, 1200f, 11.5f),
				[ItemID.SilverOre] =   (1, ItemID.TungstenOre, 1, 1500f, 15f),
				[ItemID.GoldOre] =     (1, ItemID.PlatinumOre, 1, 2000f, 20f),
				[ItemID.TinOre] =      (1, ItemID.CopperOre,   1, 1000f, 8f),
				[ItemID.LeadOre] =     (1, ItemID.IronOre,     1, 1200f, 11.5f),
				[ItemID.TungstenOre] = (1, ItemID.SilverOre,   1, 1500f, 15f),
				[ItemID.PlatinumOre] = (1, ItemID.GoldOre,     1, 2000f, 20f)
			};

			BlastFurnaceEntity.ingredientToResult = new Dictionary<int, (int requireStack, int resultType, int resultStack)>(){
				[ItemID.CopperOre] =   (3, ItemID.CopperBar,    2),
				[ItemID.IronOre] =     (3, ItemID.IronBar,      2),
				[ItemID.SilverOre] =   (4, ItemID.SilverBar,    2),
				[ItemID.GoldOre] =     (4, ItemID.GoldBar,      2),
				[ItemID.TinOre] =      (3, ItemID.TinBar,       2),
				[ItemID.LeadOre] =     (3, ItemID.LeadBar,      2),
				[ItemID.TungstenOre] = (4, ItemID.TungstenBar,  2),
				[ItemID.PlatinumOre] = (4, ItemID.PlatinumBar,  2),

				[ItemID.Meteorite] =   (3, ItemID.MeteoriteBar, 2),

				[ItemID.DemoniteOre] = (3, ItemID.DemoniteBar,  2),
				[ItemID.CrimtaneOre] = (3, ItemID.CrimtaneOre,  2)
			};

			ReinforcedFurnaceEntity.woodTypes = new List<int>(){
				ItemID.Wood,
				ItemID.BorealWood,
				ItemID.RichMahogany,
				ItemID.Shadewood,
				ItemID.Ebonwood,
				ItemID.Pearlwood,
				ItemID.PalmWood
			};

			//SendPowerToMachines needs to run after the generators have exported their power to the networks, but before the rest of the machines update
			TileEntity._UpdateStart += NetworkCollection.SendPowerToMachines;

			Logger.Debug("Executing MSIL Edits and Method Detours...");

			//Method Detours and IL Edits
			API.Edits.MSIL.ILHelper.InitMonoModDumps();

			API.Edits.MSIL.ILHelper.Load();
			API.Edits.MSIL.Vanilla.Load();

			API.Edits.MSIL.ILHelper.DeInitMonoModDumps();

			API.Edits.Detours.Vanilla.Load();

			Logger.Debug("Loading Cross-Mod Capabilities...");

			MagicStorageHandler.handler = new ModHandler();
			MagicStorageHandler.handler.Load("MagicStorage");
		}

		public static int GetCapsuleType(MachineGasID gas){
			int type = Instance.ItemType("Capsule_" + gas);
			if(type == 0)
				throw new ArgumentException("Gas ID requested was invalid: " + gas.ToString());

			return type;
		}

		public static int GetFakeIngredientType(MachineLiquidID id) => Instance.ItemType("Fake" + id.EnumName() + "LiquidIngredient");

		public static int GetFakeIngredientType(MachineGasID id) => Instance.ItemType("Fake" + id.EnumName() + "GasIngredient");

		public override void AddRecipeGroups(){
			RegisterRecipeGroup(ScienceRecipeGroups.Sand, ItemID.SandBlock, new int[]{ ItemID.SandBlock, ItemID.CrimsandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock });
		}

		private static void RegisterRecipeGroup(string groupName, int itemForAnyName, int[] validTypes)
			=> RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(itemForAnyName)}", validTypes));

		internal void SetNetworkTilesSolid(){
			Main.tileSolid[ModContent.TileType<TransportJunction>()] = true;

			foreach(var type in types){
				if(type.IsAbstract)
					continue;

				if(typeof(JunctionMergeable).IsAssignableFrom(type)){
					int tileType = GetTile(type.Name).Type;

					Main.tileSolid[tileType] = true;
				}
			}
		}

		internal void ResetNetworkTilesSolid(){
			Main.tileSolid[ModContent.TileType<TransportJunction>()] = false;

			foreach(var type in types){
				if(type.IsAbstract)
					continue;

				if(typeof(JunctionMergeable).IsAssignableFrom(type)){
					int tileType = GetTile(type.Name).Type;

					Main.tileSolid[tileType] = false;
				}
			}
		}

		public override void PreUpdateEntities(){
			//ModHooks.PreUpdateEntities() is called before WorldGen.UpdateWorld, which updates the tile entities
			//So this is a good place to have the tile stuff update

			//Sanity check
			ResetNetworkTilesSolid();

			//Reset the wire network export counts
			NetworkCollection.ResetNetworkInfo();
		}

		public override void MidUpdateProjectileItem(){
			NetworkCollection.UpdateItemNetworks();
			NetworkCollection.UpdateFluidNetworks();

			if(MagicStorageHandler.GUIRefreshPending && Main.GameUpdateCount % 120 == 0){
				//A return of "true" means the GUIs were refreshed
				MagicStorageHandler.GUIRefreshPending = !MagicStorageHandler.RefreshGUIs();
			}
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

			ElectrolyzerEntity.liquidToGas = null;

			DatalessMachineInfo.recipes = null;

			Capsule.containerTypes = null;
			FakeCapsuleFluidItem.containerTypes = null;

			MachineMufflerTile.mufflers = null;

			PulverizerEntity.inputToOutputs = null;

			JunctionMergeable.mergeTypes = null;

			NetworkCollection.networkTypeCtors = null;

			DebugHotkey = null;

			Logger.DebugFormat("Unloading machines and UI...");

			machineLoader?.Unload();

			AirIonizerEntity.recipes = null;

			BlastFurnaceEntity.ingredientToResult = null;

			ReinforcedFurnaceEntity.woodTypes = null;

			NetworkCollection.Unload();

			TileEntity._UpdateStart -= NetworkCollection.SendPowerToMachines;

			types = null;

			Logger.DebugFormat("Unloading Cross-Mod Capabilities...");

			MagicStorageHandler.handler?.Unload();
			MagicStorageHandler.handler = null;
		}

		public override void PostSetupContent() {
			Logger.DebugFormat("Loading tile data and machine structures...");

			TileUtils.RegisterAllEntities();

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

			DatalessMachineInfo.Register<BasicThermoGeneratorItem>(new[]{
				(ItemID.Glass, 3),    (ItemID.CopperBar, 2),                  (ItemID.Glass, 4),
				(ItemID.RedBrick, 5), (ItemID.Torch, 10),                     (ItemID.WaterBucket, 3),
				(ItemID.RedBrick, 5), (ModContent.ItemType<TFWireItem>(), 6), (ItemID.GrayBrick, 3)
			});

			DatalessMachineInfo.Register<PulverizerItem>(new[]{
				(ItemID.IronBar, 2),                  (ModContent.ItemType<IronPipe>(), 1),         (ItemID.IronBar, 2),
				(ModContent.ItemType<IronPipe>(), 1), (ModContent.ItemType<BasicMachineCore>(), 1), (ModContent.ItemType<IronPipe>(), 1),
				(ItemID.IronBar, 2),                  (ModContent.ItemType<TFWireItem>(), 6),       (ItemID.IronBar, 2)
			});

			DatalessMachineInfo.Register<FluidTankItem>(new[]{
				(ItemID.IronBar, 5), (ModContent.ItemType<FluidPump>(), 1), (ItemID.IronBar, 5),
				(ItemID.Glass, 10),  (ItemID.IronBar, 5),                   (ItemID.Glass, 10),
				(ItemID.IronBar, 5), (ModContent.ItemType<FluidPump>(), 1), (ItemID.IronBar, 5)
			});

			DatalessMachineInfo.Register<LiquidDuplicatorItem>(new[]{
				(ItemID.IronBar, 2),   (ItemID.OutletPump, 1),                       (ItemID.IronBar, 2),
				(ItemID.InletPump, 1), (ModContent.ItemType<BasicMachineCore>(), 1), (ItemID.InletPump, 1),
				(ItemID.IronBar, 2),   (ModContent.ItemType<TFWireItem>(), 6),       (ItemID.IronBar, 2)
			});

			DatalessMachineInfo.recipes.Add(ModContent.ItemType<MagicStorageConnectorItem>(), mod => {
				if(!MagicStorageHandler.handler.ModIsActive)
					return;

				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(MagicStorageHandler.ItemType("StorageAccess"));
				recipe.AddIngredient(ModContent.ItemType<Silicon>(), 30);
				recipe.AddIngredient(ModContent.ItemType<ItemPump>(), 2);
				recipe.AddTile(TileID.Anvils);
				recipe.SetResult(mod.ItemType("DatalessMagicStorageConnector"), 1);
				recipe.AddRecipe();
			});

			DatalessMachineInfo.Register<ItemCacheItem>(new[]{
				(ItemID.IronBar, 1),                  (ItemID.IronBar, 1), (ItemID.IronBar, 1),
				(ItemID.IronBar, 1),                  (ItemID.Chest, 1),   (ItemID.IronBar, 1),
				(ModContent.ItemType<ItemPump>(), 1), (ItemID.IronBar, 1), (ModContent.ItemType<ItemPump>(), 1)
			});

			//Loading merge data here instead of <tile>.SetDefaults()
			foreach(var type in types){
				if(type.IsAbstract)
					continue;

				if(typeof(JunctionMergeable).IsAssignableFrom(type)){
					int tileType = GetTile(type.Name).Type;

					foreach(var pair in TileUtils.tileToEntity){
						if(pair.Value is PoweredMachineEntity){
							Main.tileMerge[tileType][pair.Key] = true;
							Main.tileMerge[pair.Key][tileType] = true;
						}
					}
				}
			}
		}

		public override void AddRecipes(){
			//Electrolyzer recipes
			AddElectrolyzerRecipe(MachineLiquidID.Water, MachineGasID.Hydrogen);
			AddElectrolyzerRecipe(MachineLiquidID.Water, MachineGasID.Oxygen);
			AddElectrolyzerRecipe(MachineLiquidID.Saltwater, MachineGasID.Hydrogen);
			AddElectrolyzerRecipe(MachineLiquidID.Saltwater, MachineGasID.Chlorine);

			AddElectrolyzerRecipe(MachineGasID.Hydrogen);
			AddElectrolyzerRecipe(MachineGasID.Oxygen);
			AddElectrolyzerRecipe(MachineGasID.Chlorine);

			//Blast furnace recipes
			foreach(var entry in BlastFurnaceEntity.ingredientToResult){
				ScienceRecipe recipe = new ScienceRecipe(this);
				recipe.AddIngredient(entry.Key, entry.Value.requireStack);
				recipe.AddTile(ModContent.TileType<BlastFurnace>());
				recipe.SetResult(entry.Value.resultType, entry.Value.resultStack);
				recipe.AddRecipe();
			}

			//Matter Energizer recipes
			foreach(var entry in AirIonizerEntity.recipes){
				ScienceRecipe recipe = new ScienceRecipe(this);
				recipe.AddIngredient(entry.Key, entry.Value.requireStack);
				recipe.AddIngredient(ModContent.ItemType<TerraFluxIndicator>());
				recipe.AddTile(ModContent.TileType<AirIonizer>());
				recipe.SetResult(entry.Value.resultType, entry.Value.resultStack);
				recipe.AddRecipe();
			}

			//Additional Extractinator recipes
			AddExtractinatorRecipe(ItemID.SandBlock, ModContent.ItemType<Silicon>());

			AddExtractinatorRecipe(ItemID.EbonsandBlock, ModContent.ItemType<Silicon>());
			AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.CursedFlame);
			AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.RottenChunk);
			AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.VilePowder);
			AddExtractinatorRecipe(ItemID.EbonsandBlock, ItemID.SoulofNight);

			AddExtractinatorRecipe(ItemID.CrimsandBlock, ModContent.ItemType<Silicon>());
			AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.Ichor);
			AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.Vertebrae);
			AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.ViciousPowder);
			AddExtractinatorRecipe(ItemID.CrimsandBlock, ItemID.SoulofNight);

			AddExtractinatorRecipe(ItemID.PearlsandBlock, ModContent.ItemType<Silicon>());
			AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.CrystalShard);
			AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.PixieDust);
			AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.UnicornHorn);
			AddExtractinatorRecipe(ItemID.PearlsandBlock, ItemID.SoulofLight);

			//Greenhouse recipes
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

			AddGreenhouseRecipe(ItemID.PumpkinSeed, ItemID.DirtBlock, ItemID.GrassSeeds, ItemID.Pumpkin);

			//Pulverizer recipes
			AddPulverizerEntry(inputItem: ItemID.DirtBlock,
				(ItemID.SandBlock,             1, 0.01),
				(ItemID.SiltBlock,             1, 0.01),
				(ItemID.StoneBlock,            1, 0.01),
				(ItemID.ClayBlock,             1, 0.01),
				(ItemID.GrassSeeds,            1, 0.015),
				(ItemID.DaybloomSeeds,         1, 0.015),
				(ItemID.BlinkrootSeeds,        1, 0.015),
				(ItemID.Acorn,                 1, 0.05),
				(ItemID.Worm,                  1, 0.125),
				(ItemID.EnchantedNightcrawler, 1, 0.0075));

			AddPulverizerEntry(inputItem: ItemID.StoneBlock,
				(ItemID.SandBlock,   1, 0.05),
				(ItemID.SiltBlock,   1, 0.025),
				(ItemID.CopperOre,   1, 0.01),
				(ItemID.TinOre,      1, 0.01),
				(ItemID.IronOre,     1, 0.005),
				(ItemID.LeadOre,     1, 0.005),
				(ItemID.SilverOre,   1, 0.002),
				(ItemID.SilverOre,   1, 0.002),
				(ItemID.GoldOre,     1, 0.001),
				(ItemID.PlatinumOre, 1, 0.001),
				(ItemID.Amethyst,    1, 0.01),
				(ItemID.Topaz,       1, 0.008),
				(ItemID.Sapphire,    1, 0.006),
				(ItemID.Emerald,     1, 0.005),
				(ItemID.Amber,       1, 0.005),
				(ItemID.Ruby,        1, 0.0025),
				(ItemID.Diamond,     1, 0.001));

			//Liquid duplicator recipes
			for(int i = 0; i < ItemLoader.ItemCount; i++){
				MachineLiquidID id = MiscUtils.GetIDFromItem(i);

				if(id == MachineLiquidID.None)
					continue;

				int fakeItem = GetFakeIngredientType(id);

				ScienceRecipe recipe = new ScienceRecipe(this);
				recipe.AddIngredient(i, 1);
				recipe.AddIngredient(ModContent.ItemType<TerraFluxIndicator>(), 1);
				recipe.AddTile(ModContent.TileType<LiquidDuplicator>());
				recipe.SetResult(fakeItem, 1);
				recipe.AddRecipe();
			}
		}

		private void AddElectrolyzerRecipe(MachineLiquidID input, MachineGasID output){
			ScienceRecipe recipe = new ScienceRecipe(this);
			recipe.AddIngredient(GetFakeIngredientType(input), 1);
			recipe.AddIngredient(ModContent.ItemType<TerraFluxIndicator>());
			recipe.AddTile(ModContent.TileType<Electrolyzer>());
			recipe.SetResult(GetFakeIngredientType(output), 1);
			recipe.AddRecipe();
		}

		private void AddElectrolyzerRecipe(MachineGasID capsuleGasType){
			ScienceRecipe recipe = new ScienceRecipe(this);
			recipe.AddIngredient(ItemType("Capsule"), 1);
			recipe.AddIngredient(GetFakeIngredientType(capsuleGasType), 1);
			recipe.AddTile(ModContent.TileType<Electrolyzer>());
			recipe.SetResult(GetCapsuleType(capsuleGasType), 1);
			recipe.AddRecipe();
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

		private void AddExtractinatorRecipe(int inputType, int outputType){
			ScienceRecipe recipe = new ScienceRecipe(this);
			recipe.AddIngredient(inputType, 1);
			recipe.AddTile(TileID.Extractinator);
			recipe.SetResult(outputType, 1);
			recipe.AddRecipe();

			recipe = new ScienceRecipe(this);
			recipe.AddIngredient(inputType, 1);
			recipe.AddIngredient(ModContent.ItemType<TerraFluxIndicator>());
			recipe.AddTile(ModContent.TileType<AutoExtractinator>());
			recipe.SetResult(outputType, 1);
			recipe.AddRecipe();
		}

		private void AddPulverizerEntry(int inputItem, params (int type, int stack, double weight)[] outputs){
			var wRand = new WeightedRandom<(int type, int stack)>(Main.rand);

			double remaining = 1.0;
			foreach((int type, int stack, double weight) in outputs){
				wRand.Add((type, stack), weight);
				remaining -= weight;

				//Add a recipe for the thing
				ScienceRecipe recipe = new ScienceRecipe(this);
				recipe.AddIngredient(inputItem);
				recipe.AddIngredient(ModContent.ItemType<TerraFluxIndicator>());
				recipe.AddTile(ModContent.TileType<Pulverizer>());
				recipe.SetResult(type, stack);
				recipe.AddRecipe();
			}

			if(remaining > 0)
				wRand.Add((-1, 0), remaining);

			PulverizerEntity.inputToOutputs.Add(inputItem, wRand);
		}

		public override void UpdateUI(GameTime gameTime) {
			machineLoader.UpdateUI(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			machineLoader.ModifyInterfaceLayers(layers);
		}

		// -- Types --
		// Call("Register Blast Furnace Recipe", ingredient type, ingredient stack, result type, result stack)
		//  - registers a recipe for the Blast Furnace machine
		// Call("Register Matter Energizer Recipe", ingredient type, ingredient stack, result type, result stack)
		//  - registers a recipe for the Blast Furnace machine
		public override object Call(params object[] args){
			//People who don't use the exact call name are dumb.  We shouldn't have to make sure they typed the name correctly

			switch((string)args[0]){
				case "Register Blast Furnace Recipe":
					if(args[1] is int ingredientType && args[2] is int ingredientStack && args[3] is int resultType && args[4] is int resultStack){
						BlastFurnaceEntity.ingredientToResult.Add(ingredientType, (ingredientStack, resultType, resultStack));
						return true;
					}else
						throw new Exception("Invalid data passed to Mod.Call(\"Register Blast Furnace Recipe\")");
				case "Register Matter Energizer Recipe":
					if(args[1] is int ingredientType2 && args[2] is int ingredientStack2 && args[3] is int resultType2 && args[4] is int resultStack2){
						BlastFurnaceEntity.ingredientToResult.Add(ingredientType2, (ingredientStack2, resultType2, resultStack2));
						return true;
					}else
						throw new Exception("Invalid data passed to Mod.Call(\"Register Matter Energizer Recipe\")");
			}

			return base.Call(args);
		}
	}
}