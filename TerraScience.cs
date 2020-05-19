using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.API.Classes.ModLiquid;
using TerraScience.Content.Items;
using TerraScience.Content.Items.Icons;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Weapons;
using TerraScience.Content.Projectiles;
using TerraScience.Content.TileEntities;
using TerraScience.Content.UI;
using TerraScience.Systems.TemperatureSystem;
using TerraScience.Utilities;

namespace TerraScience {
	public class TerraScience : Mod {
		/// <summary>
		/// For fast and easy access to this mod's instance when one doesn't exist already
		/// </summary>
		public static TerraScience Instance => ModContent.GetInstance<TerraScience>();

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
		};

		internal MachineUILoader machineLoader;

		internal ModLiquidLoader LiquidLoader { get; private set; }

		internal ModLiquidFactory LiquidFactory { get; private set; }

		internal TemperatureSystem temperatureSystem;

		public static ModHotKey DebugHotkey;

		public const string WarningWaterExplode = "[c/bb3300:WARNING:] exposure to water may cause spontaneous combustion!";

		/// <summary>
		/// The cached Actions for each ElementItem defaults.
		/// </summary>
		public static Dictionary<string, Action<Item>> CachedElementDefaults { get; private set; }

		/// <summary>
		/// The cached Actions for each ElementItem recipe.
		/// </summary>
		public static Dictionary<string, Action<ScienceRecipe, ElementItem>> CachedElementRecipes { get; private set; }

		/// <summary>
		/// The cached Actions for each CompoundItem defaults.
		/// </summary>
		public static Dictionary<string, Action<Item>> CachedCompoundDefaults { get; private set; }

		/// <summary>
		/// The cached Actions for each CompoundItem recipe.
		/// </summary>
		public static Dictionary<string, Action<ScienceRecipe, CompoundItem>> CachedCompoundRecipes { get; private set; }

		public override void Load() {
			LiquidLoader = new ModLiquidLoader();
			LiquidFactory = new ModLiquidFactory();
			machineLoader = new MachineUILoader();
			temperatureSystem = new TemperatureSystem();

			CachedElementDefaults = new Dictionary<string, Action<Item>>();
			CachedElementRecipes = new Dictionary<string, Action<ScienceRecipe, ElementItem>>();
			CachedCompoundDefaults = new Dictionary<string, Action<Item>>();
			CachedCompoundRecipes = new Dictionary<string, Action<ScienceRecipe, CompoundItem>>();

			DebugHotkey = RegisterHotKey("Debuging", "J");

			RegisterElements();
			RegisterCompounds();

			AddProjectile("PepperDust", new ShakerDust("Pepper Dust", new Color(){ PackedValue = 0xff2a2a2a }));
			AddProjectile("SaltDust", new ShakerDust("Salt Dust", new Color(){ PackedValue = 0xffd5d5d5 }));

			AddItem("Shaker_Pepper", new Shaker("Pepper Shaker",
				"\"Time to spice up the competition a bit!\"",
				Compound.Capsaicin,
				item => {
					item.damage = 28;
					item.knockBack = 5.735f;
					item.rare = ItemRarityID.Blue;
					item.value = Item.sellPrice(silver: 4, copper: 20);
					item.shoot = ProjectileType("PepperDust");
				}));
			AddItem("Shaker_Salt", new Shaker("Salt Shaker",
				"\"Enemy #1 in the Slug Kingdom\"",
				Compound.SodiumChloride,
				item => {
					item.damage = 11;
					item.knockBack = 5.735f;
					item.rare = ItemRarityID.Blue;
					item.value = Item.sellPrice(silver: 4, copper: 20);
					item.shoot = ProjectileType("SaltDust");
				}));

			AddIcon("SaltExtractor");
			AddIcon("ScienceWorkbench");

			Main.OnTick += OnUpdate;

			LiquidLoader.LoadLiquids(LiquidFactory);
			machineLoader.Load();
		}

		private void OnUpdate() {
			if (LiquidFactory.Liquids != null) {
				foreach (var liquid in LiquidFactory.Liquids) {
					liquid.Value.Update();
				}
			}
		}

		public override void Unload() {
			CachedElementDefaults = null;
			CachedElementRecipes = null;
			DebugHotkey = null;

			Main.OnTick -= OnUpdate;

			TileUtils.Structures.Unload();
			machineLoader?.Unload();
		}

		public override void PostSetupContent() {
			TileUtils.Structures.SetupStructures();
		}

		public override void UpdateUI(GameTime gameTime) {
			machineLoader.UpdateUI(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			machineLoader.ModifyInterfaceLayers(layers);
		}

		public override void AddRecipes(){
			// 1 Water + 1 Empty Bucket = 1 Water Bucket
			ModRecipe recipe = new ModRecipe(this);
			recipe.AddIngredient(CompoundUtils.CompoundType(Compound.Water));
			recipe.AddIngredient(ItemID.EmptyBucket);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(ItemID.WaterBucket);
			recipe.AddRecipe();
		}

		// -- Types --
		// Call("Get Machine Entity", new Point16(x, y)) - gets the MachineEntity at the tile position, if one exists there
		public override object Call(params object[] args) {
			//People who don't use the exact call name are dumb.  We shouldn't have to make sure they typed the name correctly

			if ((string)args[0] == "Get Machine Entity") {
				MiscUtils.TryGetTileEntity((Point16)args[1], out MachineEntity entity);
				return entity;
			}

			return base.Call(args);
		}
		private void RegisterElements() {
			ElementUtils.RegisterElement(Element.Hydrogen,
				"Element #1\nVery flammable.",
				NoRecipe,
				1,
				item => {
					item.width = 32;
					item.height = 32;
					item.scale = 0.5f;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 1;
				},
				ElementState.Gas,
				ElementFamily.None,
				TemperatureSystem.CelsiusToKelvin(-252.9f),
				TemperatureSystem.CelsiusToKelvin(-259.14f),
				Color.Orange);

			ElementUtils.RegisterElement(Element.Helium,
				"Element #2\nFloaty, floaty!\nInert, not very reactive",
				NoRecipe,
				1,
				item => {
					item.width = 32;
					item.height = 32;
					item.scale = 0.5f;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 1;
				},
				ElementState.Gas,
				ElementFamily.NobleGases,
				TemperatureSystem.CelsiusToKelvin(-268.9f),
				TemperatureSystem.CelsiusToKelvin(-272.2f),
				Color.Wheat);

			ElementUtils.RegisterElement(Element.Lithium,
				$"Element #3\nVERY reactive!\n{WarningWaterExplode}",
				NoRecipe,
				1,
				item => {
					item.width = 30;
					item.height = 24;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 20;
				},
				ElementState.Solid,
				ElementFamily.AlkaliMetals,
				TemperatureSystem.CelsiusToKelvin(1330f),
				TemperatureSystem.CelsiusToKelvin(180.5f),
				isPlaceableBar: true);

			ElementUtils.RegisterElement(Element.Beryllium,
				$"Element #4\nFairly reactive\n{WarningWaterExplode}",
				NoRecipe,
				1,
				item => {
					item.width = 30;
					item.height = 24;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 20;
				},
				ElementState.Solid,
				ElementFamily.AlkalineEarthMetals,
				TemperatureSystem.CelsiusToKelvin(2970f),
				TemperatureSystem.CelsiusToKelvin(1287f),
				isPlaceableBar: true);

			ElementUtils.RegisterElement(Element.Boron,
				"Element #5\nOne of the few metalloids",
				NoRecipe,
				1,
				item => {
					item.width = 26;
					item.height = 26;
					item.rare = ItemRarityID.Green;
					item.maxStack = 999;
					item.value = 50;
				},
				ElementState.Solid,
				ElementFamily.Boron,
				4200,
				2349,
				isPlaceableBar: true);

			// TODO: Add EvaporationType.Sublimation ?
			ElementUtils.RegisterElement(Element.Carbon,
				"Element #6\nThe backbone of all organic compounds",
				NoRecipe,
				1,
				item => {
					item.width = 32;
					item.height = 32;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 10;
				},
				ElementState.Solid,
				ElementFamily.Carbon,
				3915,
				-1);
			
			ElementUtils.RegisterElement(Element.Nitrogen,
				"Element #7\nThe main component of air",
				NoRecipe,
				1,
				item => {
					item.width = 32;
					item.height = 32;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 5;
				},
				ElementState.Gas,
				ElementFamily.Nitrogen,
				77.355f,
				63.15f,
				Color.LimeGreen);

			ElementUtils.RegisterElement(Element.Oxygen,
				"Element #8\nA breath of fresh air...",
				NoRecipe,
				1,
				item => {
					item.width = 32;
					item.height = 32;
					item.scale = 0.5f;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 3;
				},
				ElementState.Gas,
				ElementFamily.Oxygen,
				TemperatureSystem.CelsiusToKelvin(-182.962f),
				TemperatureSystem.CelsiusToKelvin(-218.79f),
				Color.CornflowerBlue);

			// TODO: register Fluorine
		}

		private void RegisterCompounds() {
			//HYDROXIDES
			CompoundUtils.RegisterCompound(Compound.LithiumHydroxide,
				"LiOH\nA fine powder.",
				NoRecipe,
				1,
				item => {
					item.width = 30;
					item.height = 24;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 120;
				},
				ElementState.Solid,
				CompoundClassification.Hydroxide,
				TemperatureSystem.CelsiusToKelvin(924),
				TemperatureSystem.CelsiusToKelvin(462),
				c => {
					c.AddElement(Element.Lithium, 1);
					c.AddElement(Element.Oxygen, 1);
					c.AddElement(Element.Hydrogen, 1);
				});
			CompoundUtils.RegisterCompound(Compound.BerylliumHydroxide,
				"Be(OH)₂\nAn unstable gel.\nSeems to dissolve easily in many things.",
				NoRecipe,
				1,
				item => {
					item.width = 26;
					item.height = 26;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 120;
				},
				ElementState.Solid,
				CompoundClassification.Hydroxide,
				-1f,    //beryllium hydroxide isn't stable
				-1f,
				c => {
					c.AddElement(Element.Beryllium, 1);
					c.AddElement(Element.Oxygen, 2);
					c.AddElement(Element.Hydrogen, 2);
				});
			//OXIDES
			CompoundUtils.RegisterCompound(Compound.LithiumOxide,
				"Li₂O\nA fine powder.\nUsed for many things.",
				NoRecipe,
				1,
				item => {
					item.width = 30;
					item.height = 24;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 120;
				},
				ElementState.Solid,
				CompoundClassification.Oxide,
				TemperatureSystem.CelsiusToKelvin(2600),
				TemperatureSystem.CelsiusToKelvin(1438),
				c => {
					c.AddElement(Element.Lithium, 2);
					c.AddElement(Element.Oxygen, 1);
				});
			CompoundUtils.RegisterCompound(Compound.BerylliumOxide,
				"BeO\nA weird powder with a high melting and boiling point",
				NoRecipe,
				1,
				item => {
					item.width = 30;
					item.height = 24;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 120;
				},
				ElementState.Solid,
				CompoundClassification.Oxide,
				TemperatureSystem.CelsiusToKelvin(3900),
				TemperatureSystem.CelsiusToKelvin(2507),
				c => {
					c.AddElement(Element.Beryllium, 1);
					c.AddElement(Element.Oxygen, 1);
				});
			//PEROXIDES
			CompoundUtils.RegisterCompound(Compound.LithiumPeroxide,
				"Li₂O₂\nA fine powder.\nUsed for many things.",
				NoRecipe,
				1,
				item => {
					item.width = 30;
					item.height = 24;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 120;
				},
				ElementState.Solid,
				CompoundClassification.Peroxide,
				-1f,
				TemperatureSystem.CelsiusToKelvin(340),  //Decomposes to Li2O
				c => {
					c.AddElement(Element.Lithium, 2);
					c.AddElement(Element.Oxygen, 2);
				});
			//SUPEROXIDES
			CompoundUtils.RegisterCompound(Compound.LithiumSuperoxide,
				"LiO₂\nThis shouldn't exist...",
				NoRecipe,
				1,
				item => {
					item.width = 32;
					item.height = 32;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 240;
				},
				ElementState.Solid,
				CompoundClassification.Superoxide,
				-1f,
				-1f,
				c => {
					c.AddElement(Element.Lithium, 1);
					c.AddElement(Element.Oxygen, 2);
				});
			//OTHER
			CompoundUtils.RegisterCompound(Compound.Water,
				"H₂O\nGood ol' dihydrogen monoxide",
				NoRecipe,
				1,
				item => {
					item.width = 32;
					item.height = 32;
					item.scale = 0.5f;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 3;
				},
				ElementState.Gas,
				CompoundClassification.Hydroxide,
				TemperatureSystem.CelsiusToKelvin(100),
				TemperatureSystem.CelsiusToKelvin(0),
				c => {
					c.AddElement(Element.Hydrogen, 2);
					c.AddElement(Element.Oxygen, 1);
				},
				Color.White);
			CompoundUtils.RegisterCompound(Compound.SodiumChloride,
				"NaCl\nSaltier than your average sailor",
				NoRecipe,
				1,
				item => {
					item.width = 28;
					item.height = 22;
					item.rare = ItemRarityID.Blue;
					item.maxStack = 999;
					item.value = 15;
					item.ammo = item.type;
					item.consumable = true;
				},
				ElementState.Solid,
				CompoundClassification.Chloride,
				TemperatureSystem.CelsiusToKelvin(1465),
				TemperatureSystem.CelsiusToKelvin(801),
				c => {
					c.AddElement(Element.Sodium, 1);
					c.AddElement(Element.Chlorine, 1);
				});
			CompoundUtils.RegisterCompound(Compound.Capsaicin,
				"C₁₈H₂₇NO₃\nSpicy!",
				NoRecipe,
				1,
				item => {
					item.width = 26;
					item.height = 20;
					item.rare = ItemRarityID.Green;
					item.maxStack = 999;
					item.value = 128;
					item.ammo = item.type;
					item.consumable = true;
				},
				ElementState.Solid,
				CompoundClassification.Organic,
				TemperatureSystem.CelsiusToKelvin(210),
				TemperatureSystem.CelsiusToKelvin(62),
				c => {
					c.AddElement(Element.Carbon, 18);
					c.AddElement(Element.Hydrogen, 27);
					c.AddElement(Element.Nitrogen, 1);
					c.AddElement(Element.Oxygen, 3);
				});
		}

		private void AddIcon(string internalName){
			AddItem($"{internalName}Icon", new IconTemplate(internalName));
		}

		/// <summary>
		/// Wraps the vanilla Item.NewItem() method to drop a ScienceItem instance.
		/// </summary>
		/// <param name="x">The tile X-position to spawn the item at.</param>
		/// <param name="y">The tile Y-position to spawn the item at.</param>
		/// <param name="width">The rectangle width to spawn the item in.</param>
		/// <param name="height">The rectangle height to spawn the item in.</param>
		/// <param name="enumName">The enum value of the ScienceItem to drop.</param>
		/// <param name="stack">How many of the item to drop.</param>
		/// <returns></returns>
		public static int SpawnScienceItem<T>(int x, int y, int width, int height, T enumName, int stack = 1, Vector2? initialVelocity = null) where T : Enum {
			int type = 0;
			if(enumName is Element e)
				type = ElementUtils.ElementType(e);
			if(enumName is Compound c)
				type = CompoundUtils.CompoundType(c);

			//'type' is 0 here if 'enumName' wasn't an Element nor a Compound
			if(type == 0)
				throw new ArgumentException("Generic argument must either be a \"TerraScience.Element\" or \"TerraScience.Compound\".", "enumName");

			int index = Item.NewItem(x, y, width, height, type, stack);
			Item item = Main.item[index];
			item.velocity = initialVelocity ?? Vector2.Zero;
			return index;
		}
	}

	public enum ElementState {
		Solid,
		Liquid,
		Gas
	}

	public enum ElementFamily {
		None,
		AlkaliMetals,
		AlkalineEarthMetals,
		TransitionMetals,
		Boron,
		Carbon,
		Nitrogen,
		Oxygen,
		Halogens,
		NobleGases
	}

	public enum Element {
		Hydrogen = 1, Helium,
		Lithium, Beryllium, Boron, Carbon, Nitrogen, Oxygen, Fluorine, Neon,
		Sodium, Magnesium, Aluminum, Silicon, Phosphorus, Sulfur, Chlorine, Argon,
		Potassium, Calcium, //Add rest of period
		Rubidium, Strontium, //Add rest of period
		Caesium, Barium, //Add rest of period
		Francium, Radium //Add rest of period
	}

	public enum Compound {
		//Hydrogen compounds
		Water, HydrogenPeroxide, Hydroxide,
		//Alkali/Earth metal hydroxides
		LithiumHydroxide, BerylliumHydroxide, SodiumHydroxide, MagnesiumHydroxide, PotassiumHydroxide, CalciumHydroxide, RubidiumHydroxide, StrontiumHydroxide, CaesiumHydroxide, BariumHydroxide, FranciumHydroxide, RadiumHydroxide,
		//Alkali/Earth metal oxides
		LithiumOxide, BerylliumOxide, SodiumOxide, MagnesiumOxide, PotassiumOxide, CalciumOxide, RubidiumOxide, StrontiumOxide, CaesiumOxide, BariumOxide, FranciumOxide, RadiumOxide,
		//Alikali metal peroxides
		LithiumPeroxide, SodiumPeroxide, PotassiumPeroxide, RubidiumPeroxide, CaesiumPeroxide, FranciumPeroxide,
		//Alkali metal superoxides
		LithiumSuperoxide, SodiumSuperoxide, PotassiumSuperoxide, RubidiumSuperoxide, CaesiumSuperoxide, FranciumSuperoxide,
		//Chloride compounds
		SodiumChloride,
		//Organic compounds
		Capsaicin
	}

	public enum CompoundClassification {
		Oxide, Hydroxide, Peroxide, Superoxide, Hydride, Chloride,
		Organic
	}
}