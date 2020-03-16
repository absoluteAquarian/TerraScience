﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.API.Classes.ModLiquid;
using TerraScience.Content.Items;
using TerraScience.Content.TileEntities;
using TerraScience.Content.UI;
using TerraScience.Systems.TemperatureSystem;
using TerraScience.Utilities;

namespace TerraScience {
	public class TerraScience : Mod {
		public static readonly Action<ModRecipe> NoRecipe = r => { };
		public static readonly Action<ModRecipe> OnlyWorkBench = r => { r.AddTile(TileID.WorkBenches); };

		internal SaltExtractorUILoader saltExtracterLoader;

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
		/// The cached Actions for each ElementItem recipe.  Assumes that only one recipe was added.
		/// </summary>
		public static Dictionary<string, Action<ModRecipe, ElementItem>> CachedElementRecipes { get; private set; }

		/// <summary>
		/// The cached Actions for each CompoundItem defaults.
		/// </summary>
		public static Dictionary<string, Action<Item>> CachedCompoundDefaults { get; private set; }

		/// <summary>
		/// The cached Actions for each CompoundItem recipe.  Assumes that only one recipe was added.
		/// </summary>
		public static Dictionary<string, Action<ModRecipe, CompoundItem>> CachedCompoundRecipes { get; private set; }

		public override void Load() {
			LiquidLoader = new ModLiquidLoader();
			LiquidFactory = new ModLiquidFactory();
			saltExtracterLoader = new SaltExtractorUILoader();
			temperatureSystem = new TemperatureSystem();
			ModLiquidRenderer.Instance = new ModLiquidRenderer();

			CachedElementDefaults = new Dictionary<string, Action<Item>>();
			CachedElementRecipes = new Dictionary<string, Action<ModRecipe, ElementItem>>();
			CachedCompoundDefaults = new Dictionary<string, Action<Item>>();
			CachedCompoundRecipes = new Dictionary<string, Action<ModRecipe, CompoundItem>>();

			DebugHotkey = RegisterHotKey("Debuging", "J");

			RegisterElements();
			RegisterCompounds();

			ContentInstance.Register(new ModLiquidLoader());

			Main.OnTick += OnUpdate;
			On.Terraria.Main.DrawWater += ModLiquidManager.SwapDrawWater;
			//IL.Terraria.Main.drawWaters += ModLiquidManager.EditDrawWaters;

			LiquidLoader.LoadLiquids(LiquidFactory);
			saltExtracterLoader.Load();
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
			saltExtracterLoader?.Unload();
		}

		public override void PostSetupContent() {
			TileUtils.Structures.SetupStructures();
		}

		public override void UpdateUI(GameTime gameTime) {
			saltExtracterLoader.UpdateUI(gameTime);

			if (!Main.dedServ) {
				ModLiquidRenderer.Instance.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			saltExtracterLoader.ModifyInterfaceLayers(layers);
		}

		// -- Types --
		// Call("Salt Extractor UI Visible") - returns true if the ui is visible, false if its not.
		// Call("Salt Extractor UI") - returns the SaltExtractorUI object
		// Call("Salt Extractor Interface") - returns the UserInterface of the salt extractor
		// Call("Salt Extractor UI Loader") - returns the SaltExtractorUILoader object
		// Call("Salt Extractor Entity", new Point16(x, y)) - returns the SaltExtractorEntity TileEntity if there is one at the position provided
		public override object Call(params object[] args) {
			string callType = args[0].ToString().ToLower().Trim().Replace(" ", "");

			if (callType == "saltextractoruivisible")
				return saltExtracterLoader.saltExtractorInterface.CurrentState != null;
			else if (callType == "saltextractorui")
				return saltExtracterLoader.saltExtractorUI;
			else if (callType == "saltextractorinterface")
				return saltExtracterLoader.saltExtractorInterface;
			else if (callType == "saltextractoruiloader")
				return saltExtracterLoader;
			else if (callType == "saltextractorentity") {
				MiscUtils.TryGetTileEntity((Point16)args[1], out SaltExtractorEntity entity);
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
				Color.Orange,
				null //The Hydrogen modliquid when we add liquids
			);

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
				Color.Wheat
			);

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
				isPlaceableBar: true
			);

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
				isPlaceableBar: true
			);

			// TODO: register Boron
			// TODO: register Carbon
			// TODO: register Nitrogen

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
				Color.CornflowerBlue
			);
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
				"Be(OH)2\nAn unstable gel.\nSeems to dissolve easily in many things.",
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
				"Li2O\nA fine powder.\nUsed for many things.",
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
				"Li2O2\nA fine powder.\nUsed for many things.",
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
				"LiO2\nThis shouldn't exist...",
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
				"H2O\nGood ol' dihydrogen monoxide",
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
				},
				ElementState.Solid,
				CompoundClassification.Chloride,
				TemperatureSystem.CelsiusToKelvin(1465),
				TemperatureSystem.CelsiusToKelvin(801),
				c => {
					c.AddElement(Element.Sodium, 1);
					c.AddElement(Element.Chlorine, 1);
				});
		}

		/// <summary>
		/// Wraps the vanilla Item.NewItem() method to drop an ElementItem instance.
		/// </summary>
		/// <param name="x">The tile X-position to spawn the item at.</param>
		/// <param name="y">The tile Y-position to spawn the item at.</param>
		/// <param name="width">The rectangle width to spawn the item in.</param>
		/// <param name="height">The rectangle height to spawn the item in.</param>
		/// <param name="itemName">The name of the ElementItem to drop.</param>
		/// <param name="stack">How many of the item to drop.</param>
		/// <returns></returns>
		public static int SpawnScienceItem<T>(int x, int y, int width, int height, T enumName, int stack = 1, Vector2? initialVelocity = null) where T : Enum {
			if (typeof(T) != typeof(Element) && typeof(T) != typeof(Compound))
				throw new ArgumentException("Generic argument must either be a \"TerraScience.Element\" or \"TerraScience.Compound\".", "enumName");

			string itemName = Enum.GetName(typeof(T), enumName);
			int type = ModContent.GetInstance<TerraScience>().ItemType(itemName);

			if (type > 0) {
				int index = Item.NewItem(x, y, width, height, type, stack);
				Item item = Main.item[index];
				item.velocity = initialVelocity ?? Vector2.Zero;
				return index;
			}

			//Invalid type
			return Main.maxItems;
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
		SodiumChloride
	}

	public enum CompoundClassification {
		Oxide, Hydroxide, Peroxide, Superoxide, Hydride, Chloride
	}
}