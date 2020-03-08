using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;
using TerraScience.Dusts;
using TerraScience.Items.Elements;
using TerraScience.Tiles;

namespace TerraScience{
	public class TerraScience : Mod{
		public readonly Action<ModRecipe> NoRecipe = r => { };

		/// <summary>
		/// The cached Actions for each ElementItem defaults.
		/// </summary>
		public static Dictionary<string, Action<Item>> CachedElementDefaults { get; private set; }

		/// <summary>
		/// The cached Actions for each ElementItem recipe.  Assumes that only one recipe was added.
		/// </summary>
		public static Dictionary<string, Action<ModRecipe, ElementItem>> CachedElementRecipes { get; private set; }

		public override void Load(){
			CachedElementDefaults = new Dictionary<string, Action<Item>>();
			CachedElementRecipes = new Dictionary<string, Action<ModRecipe, ElementItem>>();

			AddElements();
		}

		public override void Unload(){
			CachedElementDefaults = null;
			CachedElementRecipes = null;
		}

		private void AddElements(){
			RegisterElement("ElementHydrogen",
				"Hydrogen",
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
				TemperatureSystem.CelsiusToKelvin(-252.9f),
				TemperatureSystem.CelsiusToKelvin(-259.14f),
				Color.Orange,
				null //The Hydrogen modliquid when we add liquids
			);

			RegisterElement("ElementHelium",
				"Helium",
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
				TemperatureSystem.CelsiusToKelvin(-268.9f),
				TemperatureSystem.CelsiusToKelvin(-272.2f),
				Color.Wheat,
				null
			);

			RegisterElement("ElementLithium",
				"Lithium",
				"Element #3\nVERY reactive!\n[c/bb3300:WARNING:] exposure to water may cause spontaneous combustion!",
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
				TemperatureSystem.CelsiusToKelvin(1330f),
				TemperatureSystem.CelsiusToKelvin(180.5f),
				isPlaceableBar: true
			);
		}

		/// <summary>
		/// Registers a new BaseElementItem.
		/// </summary>
		/// <param name="internalName">The internal name for this item.</param>
		/// <param name="displayName">The display name for this item.</param>
		/// <param name="description">The tooltip for this item.</param>
		/// <param name="recipe">The recipe for this item.</param>
		/// <param name="stackCrafted">How much of this item is crafted per recipe.</param>
		/// <param name="defaults">The defaults set for this item's <seealso cref="Item"/> item field.</param>
		/// <param name="state">The default ElementState for this element.</param>
		/// <param name="gasColor">Optional.  Determines the colour for the gas drawn for this element when in the world.</param>
		/// <param name="isPlaceableBar">Optional.  Determines if this metal element is a placeable bar.</param>
		private void RegisterElement(string internalName, string displayName, string description, Action<ModRecipe> recipe, int stackCrafted, Action<Item> defaults, ElementState state, float boilingPoint, float meltingPoint, Color? gasColor = null, ModLiquid liquid = null, bool isPlaceableBar = false){
			ElementItem item = new ElementItem(displayName,
				description,
				state,
				gasColor ?? Color.White,
				isPlaceableBar,
				liquid,
				boilingPoint,
				meltingPoint
				);
			AddItem(internalName, item);

			//Add the corresponding bar tile if it should exist
			if(isPlaceableBar)
				AddTile(internalName, new ElementBar(), $"TerraScience/Tiles/{internalName}");

			//Cache the defaults and recipe so we can use it anytime
			CachedElementDefaults.Add(internalName, defaults);
			CachedElementRecipes.Add(internalName,
				(r, e) => {
					recipe(r);
					r.SetResult(e, stackCrafted);
				});
		}

		/// <summary>
		/// Creates an <seealso cref="ElementGasDust"/> dust and returns it.
		/// </summary>
		public static Dust NewElementGasDust(Vector2 position, int width, int height, Color gasColor, Vector2? speed = null){
			Dust dust = Dust.NewDustDirect(position, width, height, ModContent.DustType<ElementGasDust>(), speed?.X ?? 0, speed?.Y ?? 0);
			dust.customData = gasColor;
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
		/// <param name="noBroadcast"></param>
		/// <param name="noGrabDelay"></param>
		/// <param name="reverseLookup"></param>
		/// <returns></returns>
		public static int SpawnElementItem(int x, int y, int width, int height, string itemName, int stack = 1, bool noBroadcast = false, bool noGrabDelay = false, bool reverseLookup = false){
			return Item.NewItem(x, y, width, height, ModContent.GetInstance<TerraScience>().ItemType(itemName), stack, noBroadcast, 0, noGrabDelay, reverseLookup);
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
		AlkaliEarthMetals,
		TransitionMetals,
		Boron,
		Carbon,
		Nitrogen,
		Oxygen,
		Halogens,
		NobleGases
	}
}