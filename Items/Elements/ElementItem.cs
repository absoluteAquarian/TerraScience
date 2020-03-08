using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Items.Elements{
	public class ElementItem : ModItem{
		public override string Texture => $"TerraScience/Items/Elements/{Name}";

		public override bool CloneNewInstances => true;

		//Currents
		public float CurrentTemp => TemperatureSystem.CurrentTemperature(item);
		public ElementState CurrentState { get; private set; } = ElementState.Solid;

		//We don't want this item to be autoloaded, since it's just a template for the other Element items
		public override bool Autoload(ref string name) => false;

		//Stored data, used during other sections of loading/autoloading
		private readonly string displayName = null;
		private readonly string description = null;
		private Action<ModRecipe, ElementItem> ItemRecipe => TerraScience.CachedElementRecipes[Name];
		private Action<Item> ItemDefaults => TerraScience.CachedElementDefaults[Name];
		public ElementState BaseState{ get; private set; } = ElementState.Solid;
		public Color GasColor{ get; private set; } = Color.White;
		public ModLiquid LiquidForm { get; private set; } = null;
		public float BoilingPoint { get; private set; } = 0f;
		public float FreezingPoint { get; private set; } = 0f;

		/// <summary>
		/// Whether this ElementItem is a placeable bar.
		/// The internal name for the tile must match this item's internal name.
		/// </summary>
		public readonly bool IsPlaceableBar;

		//Useless, but it's required for the mod to load.
		public ElementItem(){ }

		/// <summary>
		/// Creates a new element item.
		/// </summary>
		/// <param name="internalName">The internal name (class name) for the item.  Used for autoloading the texture.</param>
		/// <param name="displayName">The display name for the item.</param>
		/// <param name="description">The tooltip for the item.</param>
		public ElementItem(string displayName, string description, ElementState state, Color gasColor, bool isPlaceableBar, ModLiquid liquid, float boilingPoint, float freezingPoint){
			this.displayName = displayName;
			this.description = description;
			BaseState = state;
			GasColor = gasColor;
			IsPlaceableBar = isPlaceableBar;
			LiquidForm = liquid;
			BoilingPoint = boilingPoint;
			FreezingPoint = freezingPoint;
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(displayName);
			Tooltip.SetDefault(description);
			
			//1If this element is normally a Gas, make it float when dropped
			if(BaseState == ElementState.Gas)
				ItemID.Sets.ItemNoGravity[item.type] = true;
		}

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

			CurrentState = BaseState;
		}

		public override void AddRecipes(){
			ModRecipe r = new ModRecipe(mod);
			ItemRecipe(r, this);
		}

		public override void PostUpdate(){
			//If this element is a gas, occasionally spawn some of the custom dust
			if(BaseState == ElementState.Gas && Main.rand.NextFloat() < 11f / 60f)
				TerraScience.NewElementGasDust(item.position, item.width, item.height, GasColor);

			UpdateState();
		}

		internal void UpdateState()
		{
			if (CurrentTemp >= BoilingPoint)
				CurrentState = ElementState.Gas;
			else if (CurrentTemp >= FreezingPoint)
				CurrentState = ElementState.Liquid;
			else if (CurrentTemp <= FreezingPoint)
				CurrentState = ElementState.Solid;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			//We don't want the texture to draw when in the world if it's a gas
			if(BaseState == ElementState.Gas)
				return false;

			//Otherwise, if it's a metal, draw it
			// TODO:  fancy shit
			return true;
		}
	}
}
