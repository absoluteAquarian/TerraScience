using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Dusts;
using TerraScience.Items.Elements;

namespace TerraScience{
	public class TerraScience : Mod{
		public readonly Action<ModRecipe> NoRecipe = r => { };

		public static Dictionary<string, Action<Item>> CachedDefaults { get; private set; }
		public static Dictionary<string, Action<ModRecipe, BaseElementItem>> CachedElementRecipes { get; private set; }

		public TerraScience(){ }

		public override void Load(){
			CachedDefaults = new Dictionary<string, Action<Item>>();
			CachedElementRecipes = new Dictionary<string, Action<ModRecipe, BaseElementItem>>();

			AddElements();
		}

		public override void Unload(){
			CachedDefaults = null;
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
				},
				ElementState.Gas,
				Color.Orange);
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
				},
				ElementState.Gas,
				Color.Wheat);
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
		private void RegisterElement(string internalName, string displayName, string description, Action<ModRecipe> recipe, int stackCrafted, Action<Item> defaults, ElementState state, Color? gasColor = null){
			BaseElementItem item = new BaseElementItem(internalName,
				displayName,
				description,
				state,
				gasColor ?? Color.White);
			AddItem(internalName, item);

			//Cache the defaults and recipe so we can use it anytime
			CachedDefaults.Add(internalName, defaults);
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
	}

	public enum ElementState{
		Solid,
		Liquid,
		Gas
	}
}