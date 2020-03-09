using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.API;
using TerraScience.Systems;
using TerraScience.Utilities;

namespace TerraScience.Content.Items{
	/// <summary>
	/// The base class for ElementItem and CompoundItem.
	/// </summary>
	public class ScienceItem : ModItem{
		public override bool CloneNewInstances => true;

		//Currents
		public float CurrentTemp => TemperatureSystem.CurrentTemperature(item);
		public ElementState CurrentState { get; internal set; } = ElementState.Solid;

		//We don't want this item to be autoloaded, since it's just a template for the other Element items
		public override bool Autoload(ref string name) => false;

		//Stored data, used during other sections of loading/autoloading
		internal string displayName = null;
		internal string description = null;

		public ElementState BaseState{ get; internal set; } = ElementState.Solid;
		public Color GasColor{ get; internal set; } = Color.White;

		public ModLiquid LiquidForm { get; internal set; } = null;

		public float BoilingPoint { get; internal set; } = 0f;
		public float MeltingPoint { get; internal set; } = 0f;

		/// <summary>
		/// Whether this ElementItem is a placeable bar.
		/// The internal name for the tile must match this item's internal name.
		/// </summary>
		public readonly bool IsPlaceableBar = false;

		/// <summary>
		/// A timer used for various tasks.
		/// </summary>
		public int ReactionTimer{ get; internal set; } = 0;
		internal int reactionTimerMax = 1;

		//Useless, but it's required for the mod to load.
		public ScienceItem(){ }

		/// <summary>
		/// Creates a new Science item.
		/// </summary>
		public ScienceItem(string description, ElementState defaultState, Color gasColor, bool isPlaceableBar, ModLiquid liquid, float boilingPoint, float meltingPoint){
			this.description = description;
			BaseState = defaultState;
			CurrentState = BaseState;
			GasColor = gasColor;
			IsPlaceableBar = isPlaceableBar;
			LiquidForm = liquid;
			BoilingPoint = boilingPoint;
			MeltingPoint = meltingPoint;
		}

		public bool Equals(ScienceItem other)
			=> this != null && other != null && CurrentState == other.CurrentState && ((this is ElementItem thisElem && other is ElementItem otherElem && thisElem.ElementName == otherElem.ElementName) || (this is CompoundItem thisComp && other is CompoundItem otherComp && thisComp.CompoundName == otherComp.CompoundName));

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(displayName);
			Tooltip.SetDefault(description);
			
			//If this element is normally a Gas, make it float when dropped
			// TODO: move to PostUpdate
			if(BaseState == ElementState.Gas)
				ItemID.Sets.ItemNoGravity[item.type] = true;
		}

		public override void SetDefaults(){
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

		public override bool OnPickup(Player player){
			//Reset the reaction timer
			ReactionTimer = 0;

			return true;
		}

		public override void PostUpdate(){
			UpdateStates();

			//Merge any nearby Gas items as to not create lag
			for(int i = 0; i < Main.maxItems; i++){
				Item otherItem = Main.item[i];
				ScienceItem otherScience = otherItem.modItem as ScienceItem;
				if(i != item.whoAmI && CurrentState == ElementState.Gas && this.Equals(otherScience) && item.Hitbox.Intersects(otherItem.Hitbox) && otherItem.stack != otherItem.maxStack){
					if(item.stack + otherItem.stack < item.maxStack){
						item.stack += otherItem.stack;
						otherItem.stack = 0;
						otherItem.active = false;
					}else{
						int oldStack = item.stack;
						item.stack = item.maxStack;
						otherItem.stack -= item.stack - oldStack;
					}
				}
			}
		}

		internal void UpdateStates()
		{
			//Changing states whilest temperature isnt implimented could cause issues.

			/*
			if (CurrentTemp >= BoilingPoint)
				CurrentState = ElementState.Gas;
			else if (CurrentTemp >= MeltingPoint)
				CurrentState = ElementState.Liquid;
			else if (CurrentTemp <= MeltingPoint)
				CurrentState = ElementState.Solid;*/
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			//We don't want the texture to draw when in the world if it's a gas
			if(CurrentState == ElementState.Gas)
				return false;

			//Otherwise, if it's a metal, draw it
			//Oxide reaction:
			if(this is ElementItem eItem && (eItem.Family == ElementFamily.AlkaliMetals || eItem.Family == ElementFamily.AlkalineEarthMetals) && !item.wet){
				Color drawColor = MiscUtils.FadeBetween(lightColor, Color.Gray, (float)ReactionTimer / reactionTimerMax);
				Texture2D texture = Main.itemTexture[item.type];
				spriteBatch.Draw(texture, item.Center - Main.screenPosition, null, drawColor, rotation, texture.Size() / 2f, scale, SpriteEffects.None, 0);
				return false;
			}

			return true;
		}
	}
}
