using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Items.Elements{
	public class BaseElementItem : ModItem{
		public override string Texture => $"TerraScience/Items/Elements/{internalName}";

		public override bool CloneNewInstances => true;

		//We don't want this item to be autoloaded, since it's just a template
		// for the other Element items
		public override bool Autoload(ref string name)
			=> false;

		//Stored data, used during other sections of loading/autoloading
		private readonly string internalName;
		public string InternalName => Name ?? internalName;
		private readonly string displayName = null;
		private readonly string description = null;
		private Action<ModRecipe, BaseElementItem> ItemRecipe => TerraScience.CachedElementRecipes[InternalName];
		private Action<Item> ItemDefaults => TerraScience.CachedDefaults[InternalName];
		public readonly ElementState BaseState = ElementState.Solid;
		public readonly Color GasColor = Color.White;

		//Useless, but it's required for the mod to load.
		public BaseElementItem(){ }

		/// <summary>
		/// Creates a new element item.
		/// </summary>
		/// <param name="internalName">The internal name (class name) for the item.  Used for autoloading the texture.</param>
		/// <param name="displayName">The display name for the item.</param>
		/// <param name="description">The tooltip for the item.</param>
		/// <param name="recipe">What happens during this item's AddRecipes() hook.</param>
		/// <param name="defaults">What happens during this item's SetDefaults() hook.</param>
		public BaseElementItem(string internalName, string displayName, string description, ElementState state, Color gasColor){
			this.displayName = displayName;
			this.description = description;
			BaseState = state;
			GasColor = gasColor;

			//This constructor gets called before Mod.SetupContent(), so we can set the texture name here
			this.internalName = internalName;
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(displayName);
			Tooltip.SetDefault(description);
			
			//If this element is normally a Gas, make it float when dropped
			if(BaseState == ElementState.Gas)
				ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override void SetDefaults(){
			ItemDefaults(item);
		}

		public override void AddRecipes(){
			ModRecipe r = new ModRecipe(mod);
			ItemRecipe(r, this);
		}

		public override void PostUpdate(){
			//If this element is a gas, occasionally spawn some of the custom dust
			if(BaseState == ElementState.Gas && Main.rand.NextFloat() < 11f / 60f)
				TerraScience.NewElementGasDust(item.position, item.width, item.height, GasColor);
		}

		//We don't want the texture to draw when in the world
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
			=> false;
	}
}
