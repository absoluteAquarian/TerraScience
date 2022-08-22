using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TerraScience.API.UI;
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


    // TODO: Move this into a MaterialLoader?
    public partial class TechMod : Mod {

        public static readonly Action<Item> VialDefaults = i => {
            i.maxStack = 99;
            i.width = 26;
            i.height = 26;
            i.useStyle = ItemUseStyleID.Swing;
            i.useTime = 15;
            i.useAnimation = 10;
            i.autoReuse = true;
            i.useTurn = true;
            i.noMelee = true;
        };

        public static class ScienceRecipeGroups{
			public const string Sand = "TerraScience: Sand Blocks";
			public const string EvilBars = "TerraScience: Evil Bars";

			public const string PreHmBarsTier1 = "TerraScience: Copper/Tin Bars";
			public const string PreHmBarsTier3 = "TerraScience: Silver/Tungsten Bars";
			public const string PreHmBarsTier4 = "TerraScience: Gold/Platinum Bars";

			public const string Chest = "TerraScience: Chests";
		}

		/// <summary>
		/// For fast and easy access to this mod's instance when one doesn't exist already
		/// </summary>
		public static TechMod Instance => ModContent.GetInstance<TechMod>();

		internal MachineUILoader machineLoader;

        public static ModKeybind DebugHotkey;

		public static bool debugging;


		internal static Type[] types;

		public const bool Release = false;

		public override void Load(){
			Logger.Debug("Loading localization...");

			/*ModTranslation text = CreateTranslation("DefaultPromptText");
			text.SetDefault("<enter a string>");
			AddTranslation(text);

			text = CreateTranslation("EnterTessseractNetworkName");
			text.SetDefault("Network name...");
			AddTranslation(text);

			text = CreateTranslation("EnterTessseractNetworkPassword");
			text.SetDefault("Network password...");
			AddTranslation(text);

			text = CreateTranslation("InputPassword");
			text.SetDefault("Input network password...");
			AddTranslation(text);

			text = CreateTranslation("EnterNewNetworkPassword");
			text.SetDefault("New network password...");
			AddTranslation(text);*/

			Logger.DebugFormat("Loading Factories and System Loaders...");

			machineLoader = new MachineUILoader();

            Logger.DebugFormat("Initializing dictionaries...");

			DebugHotkey = KeybindLoader.RegisterKeybind(this, "Debugging", "J");

			TileUtils.tileToEntity = new Dictionary<int, MachineEntity>();
			TileUtils.tileToStructureName = new Dictionary<int, string>();

			ElectrolyzerEntity.liquidToGas = new Dictionary<MachineFluidID, (MachineFluidID, MachineFluidID)>(){
				[MachineFluidID.LiquidWater] = (MachineFluidID.HydrogenGas, MachineFluidID.OxygenGas),
				[MachineFluidID.LiquidSaltwater] = (MachineFluidID.HydrogenGas, MachineFluidID.ChlorineGas)
			};

			DatalessMachineInfo.recipes = new Dictionary<int, Action<Mod>>();
			DatalessMachineInfo.recipeIngredients = new Dictionary<int, RecipeIngredientSet>();

			Capsule.fluidContainerTypes = new Dictionary<int, MachineFluidID>();
			FakeCapsuleFluidItem.containerTypes = new Dictionary<int, MachineFluidID>();

			MachineMufflerTile.mufflers = new List<Point16>();

			PulverizerEntity.inputToOutputs = new Dictionary<int, WeightedRandom<(int type, int stack)>>();

			JunctionMergeable.InitMergeArray();

			NetworkCollection.networkTypeCtors = new Dictionary<Type, NetworkCollection.TypeCtor>(){
				[typeof(TFWire)] = (location, network) => new TFWire(location, network),
				[typeof(ItemPipe)] = (location, network) => new ItemPipe(location, network),
				[typeof(FluidPipe)] = (location, network) => new FluidPipe(location, network)
			};

			Logger.DebugFormat("Adding other content...");

            AddContent(new ShakerDust("PepperDust", "Pepper Dust", new Color() { PackedValue = 0xff2a2a2a }));
            AddContent(new ShakerDust("SaltDust", "Salt Dust", new Color() { PackedValue = 0xffd5d5d5 }));

            AddContent(new Shaker("Shaker_Pepper",
                "Pepper Shaker",
                "\"Time to spice up the competition a bit!\"",
                () => ModContent.ItemType<Capsaicin>(),
                item => {
                    item.damage = 28;
                    item.knockBack = 5.735f;
                    item.rare = ItemRarityID.Blue;
                    item.value = Item.sellPrice(silver: 4, copper: 20);
                    item.shoot = Find<ModProjectile>("PepperDust").Type;
                }));
            AddContent(new Shaker("Shaker_Salt",
                "Salt Shaker",
                "\"Enemy #1 in the Slug Kingdom\"",
                () => ModContent.ItemType<Salt>(),
                item => {
                    item.damage = 11;
                    item.knockBack = 5.735f;
                    item.rare = ItemRarityID.Blue;
                    item.value = Item.sellPrice(silver: 4, copper: 20);
                    item.shoot = Find<ModProjectile>("SaltDust").Type;
                }));

            MachineFluidID[] fluidIDs = (MachineFluidID[])Enum.GetValues(typeof(MachineFluidID));
			for(int i = 0; i < fluidIDs.Length; i++){
				AddContent(new Capsule("Capsule_" + fluidIDs[i].EnumName()));
				int id = ItemLoader.ItemCount - 1;
				Capsule.fluidContainerTypes.Add(id, fluidIDs[i]);

				Logger.Debug($"Capsule type registered.  ID: {id}, Fluid Type: {(ModContent.GetModItem(id) as Capsule).FluidType}");
			}

			for(int i = 1; i < fluidIDs.Length; i++){
				AddContent(new FakeCapsuleFluidItem("Fake" + fluidIDs[i].EnumName() + "FluidIngredient"));
				int id = ItemLoader.ItemCount - 1;
				FakeCapsuleFluidItem.containerTypes.Add(id, fluidIDs[i]);

				Logger.Debug($"Fake fluid ingredient type registered.  ID: {id}, Fluid Type: {(ModContent.GetModItem(id) as FakeCapsuleFluidItem).Fluid}");
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
                    AddContent((ModItem)Activator.CreateInstance(datalessType, new object[] { $"Dataless{name.Substring(0, name.LastIndexOf("Item"))}" }));
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

			UITextPrompt.Load();

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

			//MUST do this here instead of PostSetupContent
			//  ModItem.SetStaticDefaults is called in SetupContent, which expects the tile entities to be registered already in MachineItem
			Logger.Debug("Registering machine tile entities...");

			TileUtils.RegisterAllEntities();
		}

		public static int GetCapsuleType(MachineFluidID fluid){
			int type = Instance.Find<ModItem>("Capsule_" + fluid).Type;
			if(type == 0)
				throw new ArgumentException("Fluid ID requested was invalid: " + fluid.ToString());

			return type;
		}

		public static int GetFakeIngredientType(MachineFluidID id) => Instance.Find<ModItem>("Fake" + id.EnumName() + "FluidIngredient").Type;

		public override void AddRecipeGroups(){
			RegisterRecipeGroup(ScienceRecipeGroups.Sand, ItemID.SandBlock, new int[]{ ItemID.SandBlock, ItemID.CrimsandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock });

			RegisterRecipeGroup(ScienceRecipeGroups.EvilBars, ItemID.DemoniteBar, new int[]{ ItemID.DemoniteBar, ItemID.CrimtaneBar });

			RegisterRecipeGroup(ScienceRecipeGroups.PreHmBarsTier1, ItemID.CopperBar, new int[]{ ItemID.CopperBar, ItemID.TinBar });
			RegisterRecipeGroup(ScienceRecipeGroups.PreHmBarsTier3, ItemID.SilverBar, new int[]{ ItemID.SilverBar, ItemID.TungstenBar });
			RegisterRecipeGroup(ScienceRecipeGroups.PreHmBarsTier4, ItemID.GoldBar, new int[]{ ItemID.GoldBar, ItemID.PlatinumBar });

			RegisterRecipeGroup(ScienceRecipeGroups.Chest, ItemID.Chest, new int[]{
				ItemID.Chest,
				ItemID.GoldChest,
				ItemID.ShadowChest,
				ItemID.EbonwoodChest,
				ItemID.RichMahoganyChest,
				ItemID.PearlwoodChest,
				ItemID.IvyChest,
				ItemID.IceChest,
				ItemID.LivingWoodChest,
				ItemID.SkywareChest,
				ItemID.ShadewoodChest,
				ItemID.WebCoveredChest,
				ItemID.LihzahrdChest,
				ItemID.WaterChest,
				ItemID.JungleChest,
				ItemID.CorruptionChest,
				ItemID.CrimsonChest,
				ItemID.HallowedChest,
				ItemID.FrozenChest,
				ItemID.DynastyChest,
				ItemID.HoneyChest,
				ItemID.SteampunkChest,
				ItemID.PalmWoodChest,
				ItemID.MushroomChest,
				ItemID.BorealWoodChest,
				ItemID.SlimeChest,
				ItemID.GreenDungeonChest,
				ItemID.PinkDungeonChest,
				ItemID.BlueDungeonChest,
				ItemID.BoneChest,
				ItemID.CactusChest,
				ItemID.FleshChest,
				ItemID.ObsidianChest,
				ItemID.PumpkinChest,
				ItemID.SpookyChest,
				ItemID.GlassChest,
				ItemID.MartianChest,
				ItemID.GraniteChest,
				ItemID.MeteoriteChest,
				ItemID.MarbleChest
			});
		}

		private static void RegisterRecipeGroup(string groupName, int itemForAnyName, int[] validTypes)
			=> RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(itemForAnyName)}", validTypes));

		internal void SetNetworkTilesSolid(){
			Main.tileSolid[ModContent.TileType<TransportJunction>()] = true;
			foreach(var type in types){
				if(type.IsAbstract)
					continue;

				if(typeof(JunctionMergeable).IsAssignableFrom(type)){
					int tileType = ModContent.TileType<type>();

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
					int tileType = ModContent.TileType<type>();

					Main.tileSolid[tileType] = false;
				}
			}
		}

		//public override void PreUpdateEntities(){
		//	//ModHooks.PreUpdateEntities() is called before WorldGen.UpdateWorld, which updates the tile entities
		//	//So this is a good place to have the tile stuff update

		//	//Sanity check
		//	ResetNetworkTilesSolid();

		//	//Reset the wire network export counts
		//	NetworkCollection.ResetNetworkInfo();
		//}

		//public override void PreUpdateProjectileItem(){ 
		//	NetworkCollection.UpdateItemNetworks();
		//	NetworkCollection.UpdateFluidNetworks();

		//	if(MagicStorageHandler.GUIRefreshPending && Main.GameUpdateCount % 120 == 0){
		//		//A return of "true" means the GUIs were refreshed
		//		MagicStorageHandler.GUIRefreshPending = !MagicStorageHandler.RefreshGUIs();
		//	}
		//}

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
			DatalessMachineInfo.recipeIngredients = null;

			Capsule.fluidContainerTypes = null;
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

			TesseractNetwork.Unload();

			TileEntity._UpdateStart -= NetworkCollection.SendPowerToMachines;

			types = null;

			UITextPrompt.Unload();

			Logger.DebugFormat("Unloading Cross-Mod Capabilities...");

			MagicStorageHandler.handler?.Unload();
			MagicStorageHandler.handler = null;
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
							throw new Exception($"Invalid data passed to Mod.Call(\"{method}\", int, int, int, int)");
					case "Register Matter Energizer Recipe":
						if(args.Length != 5)
							throw new Exception($"Invalid arguments list for Mod.Call(\"{method}\")");

						if(args[1] is int ingredientType2 && args[2] is int ingredientStack2 && args[3] is int resultType2 && args[4] is int resultStack2){
							BlastFurnaceEntity.ingredientToResult.Add(ingredientType2, (ingredientStack2, resultType2, resultStack2));
							return true;
						}else
							throw new Exception($"Invalid data passed to Mod.Call(\"{method}\", int, int, int, int)");
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

		//Debugging stuff
		public void PrintAllItemsForNetwork(string tesseractNetwork){
			if(TesseractNetwork.TryGetEntry(tesseractNetwork, out var entry)){
				for(int i = 0; i < entry.items.Length; i++){
					Item item = entry.items[i];
					Main.NewText($"  Item #{i}: {item.Name ?? "None"} {(item.stack > 1 ? $"({item.stack})" : "")}");
				}
			}
		}
	}
}