using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Dusts;
using TerraScience.Items.Compounds;
using TerraScience.Items.Elements;
using TerraScience.Tiles;

namespace TerraScience{
	public class TerraScience : Mod{
		public readonly Action<ModRecipe> NoRecipe = r => { };
		public readonly Action<ModRecipe> OnlyWorkBench = r => { r.AddTile(TileID.WorkBenches); };

		public readonly string WarningWaterExplode = "[c/bb3300:WARNING:] exposure to water may cause spontaneous combustion!";

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
		public static Dictionary<string, Action<Item>> CachedCompoundDefaults{ get; private set; }

		/// <summary>
		/// The cached Actions for each CompoundItem recipe.  Assumes that only one recipe was added.
		/// </summary>
		public static Dictionary<string, Action<ModRecipe, CompoundItem>> CachedCompoundRecipes{ get; private set; }

		public override void Load(){
			CachedElementDefaults = new Dictionary<string, Action<Item>>();
			CachedElementRecipes = new Dictionary<string, Action<ModRecipe, ElementItem>>();
			CachedCompoundDefaults = new Dictionary<string, Action<Item>>();
			CachedCompoundRecipes = new Dictionary<string, Action<ModRecipe, CompoundItem>>();

			AddElements();
			AddCompounds();
		}

		public override void Unload(){
			CachedElementDefaults = null;
			CachedElementRecipes = null;
		}

		private void AddElements(){
			RegisterElement(Element.Hydrogen,
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
			RegisterElement(Element.Helium,
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
			RegisterElement(Element.Lithium,
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
			RegisterElement(Element.Beryllium,
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
			// TODO: register Boron
			// TODO: register Carbon
			// TODO: register Nitrogen
			RegisterElement(Element.Oxygen,
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
		}

		private void AddCompounds(){
			//HYDROXIDES
			RegisterCompound(Compound.LithiumHydroxide,
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
			RegisterCompound(Compound.BerylliumHydroxide,
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
				-1f,	//beryllium hydroxide isn't stable
				-1f,
				c => {
					c.AddElement(Element.Beryllium, 1);
					c.AddElement(Element.Oxygen, 2);
					c.AddElement(Element.Hydrogen, 2);
				});
			//OXIDES
			RegisterCompound(Compound.LithiumOxide,
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
			RegisterCompound(Compound.BerylliumOxide,
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
			RegisterCompound(Compound.LithiumPeroxide,
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
			RegisterCompound(Compound.LithiumSuperoxide,
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
		}

		/// <summary>
		/// Registers a new ElementItem.
		/// </summary>
		/// <param name="name">The enum Element value for this item.</param>
		/// <param name="description">The tooltip for this item.</param>
		/// <param name="recipe">The recipe for this item.</param>
		/// <param name="stackCrafted">How much of this item is crafted per recipe.</param>
		/// <param name="defaults">The defaults set for this item's <seealso cref="Item"/> item field.</param>
		/// <param name="state">The default ElementState for this element.</param>
		/// <param name="family">What periodic family this element belongs to.</param>
		/// <param name="boilingPoint">The boiling point for this element in Kelvin.</param>
		/// <param name="meltingPoint">The melting point for this element in Kelvin.</param>
		/// <param name="gasColor">Optional.  Determines the colour for the gas drawn for this element when in the world.</param>
		/// <param name="liquid">Optional.  The ModLiquid for this element.</param>
		/// <param name="isPlaceableBar">Optional.  Determines if this metal element is a placeable bar.</param>
		private void RegisterElement(Element name, string description, Action<ModRecipe> recipe, int stackCrafted, Action<Item> defaults, ElementState state, ElementFamily family, float boilingPoint, float meltingPoint, Color? gasColor = null, ModLiquid liquid = null, bool isPlaceableBar = false){
			string internalName = ElementName(name);
			ElementItem item = new ElementItem(name,
				description,
				state,
				family,
				gasColor ?? Color.White,
				isPlaceableBar,
				liquid,
				boilingPoint,
				meltingPoint
				);
			AddItem(internalName, item);

			//Add the corresponding bar tile if it should exist
			if(isPlaceableBar)
				AddTile(internalName, new ScienceBar(), $"TerraScience/Tiles/{internalName}");

			//Cache the defaults and recipe so we can use it anytime
			CachedElementDefaults.Add(internalName, defaults);
			CachedElementRecipes.Add(internalName,
				(r, e) => {
					recipe(r);
					r.SetResult(e, stackCrafted);
					r.AddRecipe();
				});
		}

		/// <summary>
		/// Registers a new CompoundItem.
		/// </summary>
		/// <param name="compound">The enum Compound value for this item.</param>
		/// <param name="description">The tooltip for this item.</param>
		/// <param name="recipe">The recipe for this item.</param>
		/// <param name="stackCrafted">How much of this item is crafted per recipe.</param>
		/// <param name="defaults">The defaults set for this item's <seealso cref="Item"/> item field.</param>
		/// <param name="state">The default ElementState for this compound.</param>
		/// <param name="boilingPoint">The boiling point for this compound in Kelvin. Set to -1 for "unstable".</param>
		/// <param name="meltingPoint">The melting point for this compound in Kelvin. Set to -1 for "unstable".</param>
		/// <param name="elements">The Action for adding elements to this compound's recipe.</param>
		/// <param name="gasColor">Optional.  Determines the colour for the gas drawn for this element when in the world.</param>
		/// <param name="liquid">Optional.  The ModLiquid for this element.</param>
		/// <param name="isPlaceableBar">Optional.  Determines if this metal element is a placeable bar.</param>
		private void RegisterCompound(Compound compound, string description, Action<ModRecipe> recipe, int stackCrafted, Action<Item> defaults, ElementState state, CompoundClassification classification, float boilingPoint, float meltingPoint, Action<CompoundItem> elements, Color? gasColor = null, ModLiquid liquid = null, bool isPlaceableBar = false){
			string internalName = CompoundName(compound, false);
			CompoundItem item = new CompoundItem(compound,
				description,
				state,
				classification,
				gasColor ?? Color.White,
				isPlaceableBar,
				liquid,
				boilingPoint,
				meltingPoint,
				elements);
			AddItem(internalName, item);

			//Add the corresponding bar tile if it should exist
			// TODO: make a ScienceBar for compounds
			if(isPlaceableBar)
				AddTile(internalName, new ScienceBar(), $"TerraScience/Tiles/{internalName}");

			//Cache the defaults and recipe so we can use it anytime
			CachedCompoundDefaults.Add(internalName, defaults);
			CachedCompoundRecipes.Add(internalName,
				(r, e) => {
					recipe(r);

					foreach(var pair in item.Elements)
						r.AddIngredient(ItemType(ElementName(pair.Item1)), pair.Item2);

					r.SetResult(e, stackCrafted);
					r.AddRecipe();
				});
		}

		/// <summary>
		/// Creates an <seealso cref="ElementGasDust"/> dust and returns it.
		/// </summary>
		public static Dust NewElementGasDust(Vector2 position, int width, int height, Color gasColor, Vector2? speed = null){
			Dust dust = Dust.NewDustDirect(position, width, height, ModContent.DustType<ElementGasDust>());
			dust.customData = gasColor;
			dust.velocity = speed ?? Vector2.Zero;
			return dust;
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
		public static int SpawnScienceItem(int x, int y, int width, int height, string itemName, int stack = 1, Vector2? initialVelocity = null){
			int index = Item.NewItem(x, y, width, height, ModContent.GetInstance<TerraScience>().ItemType(itemName), stack);
			Item item = Main.item[index];
			item.velocity = initialVelocity ?? Vector2.Zero;
			return index;
		}

		public static string ElementName(Element name, bool includeElement = true)
			=> $"{(includeElement ? "Element" : "")}{Enum.GetName(typeof(Element), name)}";

		//Get the name, make it "proper case" then trim the starting space
		public static string CompoundName(Compound name, bool doMakeProper = true)
			=> doMakeProper
				? string.Concat(Enum.GetName(typeof(Compound), name)
					.Select(x => char.IsUpper(x) ? " " + x : x.ToString()))
					.TrimStart(' ')
				: Enum.GetName(typeof(Compound), name);

		public static T ParseToEnum<T>(string name) where T : Enum
			=> (T)Enum.Parse(typeof(T), name);


		public static void DetermineAlkaliWaterReactionCompound(ElementItem element, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned){
			//Setting a variable here for future use (TODO)
			float rand = Main.rand.NextFloat();

			if((element.Family == ElementFamily.AlkaliMetals || element.Family == ElementFamily.AlkalineEarthMetals) && rand < 0.95f){
				//Hydroxide is viable
				hydrogensSpawned = 2;
				compoundsSpawned = element.Family == ElementFamily.AlkaliMetals ? 2 : 1;
				compound = ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Hydroxide");
			}else{
				compound = Compound.Water;
				hydrogensSpawned = 0;
				compoundsSpawned = 0;
			}
		}

		public static void DetermineAlikaliAirReactionCompound(ElementItem element, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned){
			float rand = Main.rand.NextFloat();

			if(rand < 0.65f || element.Family == ElementFamily.AlkalineEarthMetals){
				//Oxides
				compound = ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Oxide");
			}else if(rand < 0.65f + 0.22f){
				//Peroxides
				compound = ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Peroxide");
			}else{
				//Superoxides
				compound = ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Superoxide");
			}

			//All reactions produce 2 Hydrogens and 1 Compound
			hydrogensSpawned = 2;
			compoundsSpawned = 1;
		}

		/// <summary>
		/// Blends the two colours together with a 50% bias.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="otherColor"></param>
		public static Color Blend(Color color, Color otherColor)
			=> FadeBetween(color, otherColor, 0.5f);

		/// <summary>
		/// Blends the two colours with the given % bias towards "toColor".  Thanks direwolf420!
		/// </summary>
		/// <param name="fromColor">The original colour.</param>
		/// <param name="toColor">The colour being blended towards</param>
		/// <param name="fadePercent">The % bias towards "toColor".  Range: [0,1]</param>
		public static Color FadeBetween(Color fromColor, Color toColor, float fadePercent)
			=> fadePercent == 0f ? fromColor : new Color(fromColor.ToVector4() * (1f - fadePercent) + toColor.ToVector4() * fadePercent);

		public static void HandleWaterReaction(ElementItem elementItem){
			DetermineAlkaliWaterReactionCompound(elementItem, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned);
			SpawnItemsFromReaction(elementItem, compound, hydrogensSpawned, compoundsSpawned);
		}

		public static void HandleAirReaction(ElementItem elementItem){
			DetermineAlikaliAirReactionCompound(elementItem, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned);
			SpawnItemsFromReaction(elementItem, compound, hydrogensSpawned, compoundsSpawned);
		}

		public static void SpawnItemsFromReaction(ElementItem elementItem, Compound compound, int hydrogensSpawned, int compoundsSpawned){
			//Give a chance to spawn Hydrogen item(s) as well as the corresponding CompoundItem
			SpawnScienceItem((int)elementItem.item.position.X, (int)elementItem.item.position.Y, 16, 16, ElementName(Element.Hydrogen), hydrogensSpawned, new Vector2(Main.rand.NextFloat(-1, 1), -4.2f));
			SpawnScienceItem((int)elementItem.item.position.X, (int)elementItem.item.position.Y, 16, 16, CompoundName(compound, false), compoundsSpawned, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2.1f));

			if(elementItem.item.stack > compoundsSpawned){
				//If there's more than one item in this stack, reduce the stack and the timer
				int oldStack = elementItem.item.stack;
				elementItem.item.stack -= compoundsSpawned;
				elementItem.ReactionTimer = (int)(elementItem.ReactionTimer * (float)elementItem.item.stack / oldStack * Main.rand.NextFloat(0.7f, 0.95f));
			}else{
				//Otherwise, make the item despawn (and make its stack to 0 for good measure)
				elementItem.item.stack = 0;
				elementItem.item.active = false;
			}
		}
	}

	public enum ElementState{
		Solid,
		Liquid,
		Gas
	}

	public enum ElementFamily{
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

	public enum Element{
		Hydrogen = 1, Helium,
		Lithium, Beryllium, Boron, Carbon, Nitrogen, Oxygen, Fluorine, Neon,
		Sodium, Magnesium, //Add rest of period
		Potassium, Calcium, //Add rest of period
		Rubidium, Strontium, //Add rest of period
		Caesium, Barium, //Add rest of period
		Francium, Radium //Add rest of period
	}

	public enum Compound{
		//Hydrogen compounds
		Water, HydrogenPeroxide, Hydroxide,
		//Alkali/Earth metal hydroxides
		LithiumHydroxide, BerylliumHydroxide, SodiumHydroxide, MagnesiumHydroxide, PotassiumHydroxide, CalciumHydroxide, RubidiumHydroxide, StrontiumHydroxide, CaesiumHydroxide, BariumHydroxide, FranciumHydroxide, RadiumHydroxide,
		//Alkali/Earth metal oxides
		LithiumOxide, BerylliumOxide, SodiumOxide, MagnesiumOxide, PotassiumOxide, CalciumOxide, RubidiumOxide, StrontiumOxide, CaesiumOxide, BariumOxide, FranciumOxide, RadiumOxide,
		//Alikali metal peroxides
		LithiumPeroxide, SodiumPeroxide, PotassiumPeroxide, RubidiumPeroxide, CaesiumPeroxide, FranciumPeroxide,
		//Alkali metal superoxides
		LithiumSuperoxide, SodiumSuperoxide, PotassiumSuperoxide, RubidiumSuperoxide, CaesiumSuperoxide, FranciumSuperoxide
	}

	public enum CompoundClassification{
		Oxide, Hydroxide, Peroxide, Superoxide, Hydride
	}
}