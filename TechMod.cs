using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;
using TerraScience.API.CrossMod;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.API.Networking;
using TerraScience.Content.ID;
using TerraScience.Content.Items;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.Items.Weapons;
using TerraScience.Content.Projectiles;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles;
using TerraScience.Content.UI;
using TerraScience.Systems;
using TerraScience.Systems.Energy;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience {
	public partial class TechMod : Mod {
		public static class ScienceRecipeGroups{
			public const string Sand = "TerraScience: Sand Blocks";
			public const string EvilBars = "TerraScience: Evil Bars";
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

			ComposterEntity.ingredients = new List<(int input, float resultStack)>();

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

			RegisterRecipeGroup(ScienceRecipeGroups.EvilBars, ItemID.DemoniteBar, new int[]{ ItemID.DemoniteBar, ItemID.CrimtaneBar });
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

			ComposterEntity.ingredients = null;

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

		public override void UpdateUI(GameTime gameTime) {
			machineLoader.UpdateUI(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			machineLoader.ModifyInterfaceLayers(layers);
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI){
			NetHandler.HandlePacket(reader, whoAmI);
		}

		// -- Types --
		// Call("Register Blast Furnace Recipe", ingredient type, ingredient stack, result type, result stack)
		//  - registers a recipe for the Blast Furnace machine
		// Call("Register Matter Energizer Recipe", ingredient type, ingredient stack, result type, result stack)
		//  - registers a recipe for the Blast Furnace machine
		// Call("Register as Wood", item type)
		//  - registers the item type as a valid input for the Reinforced Furnace
		// Call("Register as Plant", item type, input stack, result stack)
		//  - registers the item type as a valid input for the Composter
		public override object Call(params object[] args){
			//People who don't use the exact call name are dumb.  We shouldn't have to make sure they typed the name correctly

			if(args[0] is string method){
				switch(method){
					case "Register Blast Furnace Recipe":
						if(args.Length != 5)
							throw new Exception($"Invalid arguments list for Mod.Call(\"{method}\")");

						if(args[1] is int ingredientType && args[2] is int ingredientStack && args[3] is int resultType && args[4] is int resultStack){
							BlastFurnaceEntity.ingredientToResult.Add(ingredientType, (ingredientStack, resultType, resultStack));
							return true;
						}else
							throw new Exception($"Invalid data passed to Mod.Call(\"{method}\", int, int, int)");
					case "Register Matter Energizer Recipe":
						if(args.Length != 5)
							throw new Exception($"Invalid arguments list for Mod.Call(\"{method}\")");

						if(args[1] is int ingredientType2 && args[2] is int ingredientStack2 && args[3] is int resultType2 && args[4] is int resultStack2){
							BlastFurnaceEntity.ingredientToResult.Add(ingredientType2, (ingredientStack2, resultType2, resultStack2));
							return true;
						}else
							throw new Exception($"Invalid data passed to Mod.Call(\"{method}\", int, int, int)");
					case "Register as Wood":
						if(args.Length != 2)
							throw new Exception($"Invalid arguments list for Mod.Call(\"{method}\")");

						if(args[1] is int woodType){
							ReinforcedFurnaceEntity.woodTypes.Add(woodType);
							return true;
						}else
							throw new Exception($"Invalid data passed to Mod.Call(\"{method}\", int)");
					case "Register as Plant":
						if(args.Length != 4)
							throw new Exception($"Invalid arguments list for Mod.Call(\"{method}\")");

						if(args[1] is int plantType && args[2] is int plantInputCount && args[3] is int dirtOutputCount){
							ComposterEntity.ingredients.Add((plantType, (float)dirtOutputCount / plantInputCount));
							return true;
						}else
							throw new Exception($"Invalid data passed to Mod.Call(\"{method}\", int, int, int)");
					default:
						throw new Exception($"Unknown call requested: \"{method}\"");
				}
			}else
				throw new Exception("Expected a string argument for the call request");
		}
	}
}