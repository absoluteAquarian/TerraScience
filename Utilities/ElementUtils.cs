using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using TerraScience.API;
using TerraScience.Content.Dusts;
using TerraScience.Content.Items;
using TerraScience.Content.Tiles;

namespace TerraScience.Utilities {
	public static class ElementUtils {
		private static readonly Mod mod = ModContent.GetInstance<TerraScience>();

		public static string ElementName(Element name, bool includeElement = true)
			=> $"{(includeElement ? "Element" : "")}{Enum.GetName(typeof(Element), name)}";

		public static void HandleWaterReaction(ElementItem elementItem) {
			DetermineAlkaliWaterReactionCompound(elementItem, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned);
			SpawnItemsFromReaction(elementItem, compound, hydrogensSpawned, compoundsSpawned);
		}

		public static void HandleAirReaction(ElementItem elementItem) {
			DetermineAlikaliAirReactionCompound(elementItem, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned);
			SpawnItemsFromReaction(elementItem, compound, hydrogensSpawned, compoundsSpawned);
		}

		/// <summary>
		/// Creates an <seealso cref="ElementGasDust"/> dust and returns it.
		/// </summary>
		public static Dust NewElementGasDust(Vector2 position, int width, int height, Color gasColor, Vector2? speed = null) {
			Dust dust = Dust.NewDustDirect(position, width, height, ModContent.DustType<ElementGasDust>());
			dust.customData = gasColor;
			dust.velocity = speed ?? Vector2.Zero;
			return dust;
		}

		public static void SpawnItemsFromReaction(ElementItem elementItem, Compound compound, int hydrogensSpawned, int compoundsSpawned) {
			//Give a chance to spawn Hydrogen item(s) as well as the corresponding CompoundItem
			TerraScience.SpawnScienceItem((int)elementItem.item.position.X, (int)elementItem.item.position.Y, 16, 16, ElementName(Element.Hydrogen), hydrogensSpawned, new Vector2(Main.rand.NextFloat(-1, 1), -4.2f));
			TerraScience.SpawnScienceItem((int)elementItem.item.position.X, (int)elementItem.item.position.Y, 16, 16, CompoundUtils.CompoundName(compound, false), compoundsSpawned, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2.1f));

			if (elementItem.item.stack > compoundsSpawned) {
				//If there's more than one item in this stack, reduce the stack and the timer
				int oldStack = elementItem.item.stack;
				elementItem.item.stack -= compoundsSpawned;
				elementItem.ReactionTimer = (int)(elementItem.ReactionTimer * (float)elementItem.item.stack / oldStack * Main.rand.NextFloat(0.7f, 0.95f));
			}
			else {
				//Otherwise, make the item despawn (and make its stack to 0 for good measure)
				elementItem.item.stack = 0;
				elementItem.item.active = false;
			}
		}

		public static void DetermineAlikaliAirReactionCompound(ElementItem element, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned) {
			float rand = Main.rand.NextFloat();

			if (rand < 0.65f || element.Family == ElementFamily.AlkalineEarthMetals) {
				//Oxides
				compound = MiscUtils.ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Oxide");
			}
			else if (rand < 0.65f + 0.22f) {
				//Peroxides
				compound = MiscUtils.ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Peroxide");
			}
			else {
				//Superoxides
				compound = MiscUtils.ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Superoxide");
			}

			//All reactions produce 2 Hydrogens and 1 Compound
			hydrogensSpawned = 2;
			compoundsSpawned = 1;
		}

		public static void DetermineAlkaliWaterReactionCompound(ElementItem element, out Compound compound, out int hydrogensSpawned, out int compoundsSpawned) {
			//Setting a variable here for future use (TODO)
			float rand = Main.rand.NextFloat();

			if ((element.Family == ElementFamily.AlkaliMetals || element.Family == ElementFamily.AlkalineEarthMetals) && rand < 0.95f) {
				//Hydroxide is viable
				hydrogensSpawned = 2;
				compoundsSpawned = element.Family == ElementFamily.AlkaliMetals ? 2 : 1;
				compound = MiscUtils.ParseToEnum<Compound>(ElementName(element.ElementName, false) + "Hydroxide");
			}
			else {
				compound = Compound.Water;
				hydrogensSpawned = 0;
				compoundsSpawned = 0;
			}
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
		internal static void RegisterElement(Element name, string description, Action<ModRecipe> recipe, int stackCrafted, Action<Item> defaults, ElementState state, ElementFamily family, float boilingPoint, float meltingPoint, Color? gasColor = null, ModLiquid liquid = null, bool isPlaceableBar = false) {
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
			mod.AddItem(internalName, item);

			//Add the corresponding bar tile if it should exist
			if (isPlaceableBar)
				mod.AddTile(internalName, new ScienceBar(), $"TerraScience/Tiles/{internalName}");

			//Cache the defaults and recipe so we can use it anytime
			TerraScience.CachedElementDefaults.Add(internalName, defaults);
			TerraScience.CachedElementRecipes.Add(internalName,
				(r, e) => {
					recipe(r);
					r.SetResult(e, stackCrafted);
					r.AddRecipe();
				});
		}
	}
}