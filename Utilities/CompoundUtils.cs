using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using TerraScience.API;
using TerraScience.API.Classes.ModLiquid;
using TerraScience.Content.Items;
using TerraScience.Content.Tiles;

namespace TerraScience.Utilities {
	public static class CompoundUtils {
		private static Mod ModInstance => ModContent.GetInstance<TerraScience>();

		//Get the name, make it "proper case" then trim the starting space
		public static string CompoundName(Compound name, bool doMakeProper = true)
			=> doMakeProper
				? string.Concat(Enum.GetName(typeof(Compound), name)
					.Select(x => char.IsUpper(x) ? " " + x : x.ToString()))
					.TrimStart(' ')
				: Enum.GetName(typeof(Compound), name);

		/// <summary>
		/// Registers a new CompoundItem.
		/// </summary>
		/// <param name="compound">The enum Compound value for this item.</param>
		/// <param name="description">The tooltip for this item.</param>
		/// <param name="recipe">The recipe for this item.</param>
		/// <param name="stackCrafted">How much of this item is crafted per recipe.</param>
		/// <param name="defaults">The defaults set for this item's <seealso cref="Item"/> item field.</param>
		/// <param name="state">The default ElementState for this compound.</param>
		/// <param name="classification">The default CompoundClassification for this compound.</param>
		/// <param name="boilingPoint">The boiling point for this compound in Kelvin. Set to -1 for "unstable".</param>
		/// <param name="meltingPoint">The melting point for this compound in Kelvin. Set to -1 for "unstable".</param>
		/// <param name="elements">The Action for adding elements to this compound's recipe.</param>
		/// <param name="gasColor">Optional.  Determines the colour for the gas drawn for this element when in the world.</param>
		/// <param name="liquid">Optional.  The ModLiquid for this element.</param>
		/// <param name="isPlaceableBar">Optional.  Determines if this metal element is a placeable bar.</param>
		internal static void RegisterCompound(Compound compound, string description, Action<ModRecipe> recipe, int stackCrafted, Action<Item> defaults, ElementState state, CompoundClassification classification, float boilingPoint, float meltingPoint, Action<CompoundItem> elements, Color? gasColor = null, ModLiquid liquid = null, bool isPlaceableBar = false) {
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
			ModInstance.AddItem(internalName, item);

			//Add the corresponding bar tile if it should exist
			// TODO: make a ScienceBar for compounds
			if (isPlaceableBar)
				ModInstance.AddTile(internalName, new ScienceBar(), $"TerraScience/Content/Tiles/{internalName}");

			//Cache the defaults and recipe so we can use it anytime
			TerraScience.CachedCompoundDefaults.Add(internalName, defaults);
			TerraScience.CachedCompoundRecipes.Add(internalName,
				(r, e) => {
					if(recipe == TerraScience.NoRecipe)
						return;

					recipe(r);

					for (int i = 0; i < item.Elements.Count; i++) {
						Tuple<Element, int> pair = item.Elements[i];
						r.AddIngredient(ModInstance.ItemType(ElementUtils.ElementName(pair.Item1)), pair.Item2);
					}

					r.SetResult(e, stackCrafted);
					r.AddRecipe();
				});
		}
	}
}