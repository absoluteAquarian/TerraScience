using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.API;
using TerraScience.Utilities;

namespace TerraScience.Content.Items{
	public class CompoundItem : ScienceItem{
		public override string Texture => $"TerraScience/Content/Items/Compounds/{CompoundUtils.CompoundName(CompoundName, false)}";

		//Prevent CompoundItem from autoloading
		public override bool Autoload(ref string name) 
			=> false;

		private Action<ModRecipe, CompoundItem> ItemRecipe => TerraScience.CachedCompoundRecipes[Name];
		private Action<Item> ItemDefaults => TerraScience.CachedCompoundDefaults[Name];

		public Compound CompoundName{ get; private set; } = Compound.LithiumHydroxide;

		public CompoundClassification Classification{ get; private set; } = CompoundClassification.Hydroxide;

		/// <summary>
		/// The List of the ElementItems this CompoundItem is made of, as well as their subscripts.
		/// Tuple is (type, subscript)
		/// </summary>
		public List<Tuple<Element, int>> Elements = new List<Tuple<Element, int>>();

		//Useless, but it's required for the mod to load.
		public CompoundItem() : base(){ }

		public CompoundItem(Compound name, string description, ElementState defaultState, CompoundClassification classification, Color gasColor, bool isPlaceableBar, ModLiquid liquid, float boilingPoint, float meltingPoint, Action<CompoundItem> elementsToAdd) : base(description, defaultState, gasColor, isPlaceableBar, liquid, boilingPoint, meltingPoint){
			CompoundName = name;
			Classification = classification;
			displayName = CompoundUtils.CompoundName(name);
			elementsToAdd(this);
		}

		public void AddElement(Element element, int subscript)
			=> Elements.Add(new Tuple<Element, int>(element, subscript));

		public override void SetDefaults(){
			ItemDefaults(item);

			//If the item is a placeable bar, register the tile type (tile name is guaranteed to be the same)
			if(IsPlaceableBar){
				item.createTile = mod.TileType(Name);
				item.useStyle = 1;
				item.useTurn = true;
				item.useAnimation = 15;
				item.useTime = 10;
				item.autoReuse = true;
				item.consumable = true;
				item.placeStyle = 0;
			}
		}

		public override void AddRecipes(){
			ModRecipe r = new ModRecipe(mod);
			ItemRecipe(r, this);
		}

		public override string ToString() {
			return string.Concat(Enum.GetName(typeof(Compound), CompoundName)
					.Select(x => char.IsUpper(x) ? " " + x : x.ToString()))
					.TrimStart(' ');
		}
	}
}
