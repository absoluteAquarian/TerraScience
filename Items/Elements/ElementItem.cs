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
		public ElementFamily Family{ get; private set; } = ElementFamily.None;
		public Element ElementName{ get; private set; } = Element.Hydrogen;
		public Color GasColor{ get; private set; } = Color.White;

		public ModLiquid LiquidForm { get; private set; } = null;

		public float BoilingPoint { get; private set; } = 0f;
		public float MeltingPoint { get; private set; } = 0f;

		/// <summary>
		/// Whether this ElementItem is a placeable bar.
		/// The internal name for the tile must match this item's internal name.
		/// </summary>
		public readonly bool IsPlaceableBar;

		/// <summary>
		/// A timer used for various tasks.
		/// </summary>
		public int ReactionTimer{ get; private set; } = 0;

		//Useless, but it's required for the mod to load.
		public ElementItem(){ }

		/// <summary>
		/// Creates a new element item.
		/// </summary>
		/// <param name="internalName">The internal name (class name) for the item.  Used for autoloading the texture.</param>
		/// <param name="displayName">The display name for the item.</param>
		/// <param name="description">The tooltip for the item.</param>
		public ElementItem(Element name, string description, ElementState defaultState, ElementFamily family, Color gasColor, bool isPlaceableBar, ModLiquid liquid, float boilingPoint, float meltingPoint){
			ElementName = name;
			this.displayName = TerraScience.ElementName(name);
			this.description = description;
			BaseState = defaultState;
			Family = family;
			GasColor = gasColor;
			IsPlaceableBar = isPlaceableBar;
			LiquidForm = liquid;
			BoilingPoint = boilingPoint;
			MeltingPoint = meltingPoint;
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
			if(CurrentState == ElementState.Gas && Main.rand.NextFloat() < 11f / 60f)
				TerraScience.NewElementGasDust(item.position, item.width, item.height, GasColor);

			//If the element is a gas, make it rise above water if it's submerged
			if(BaseState == ElementState.Gas){
				if(item.wet)
					item.velocity.Y = -3f * 16 / 60;	//3 tiles per second upwards
				else
					item.velocity.Y = 0;
			}

			//If the element is an AlkakiMetal or AlkalineEarthMetal and is in water, make it explode after some random amount of time
			if(Family == ElementFamily.AlkaliMetals || Family == ElementFamily.AlkalineEarthMetals){
				//Gotta be wet
				if(item.wet){
					ReactionTimer += Main.rand.Next(3);

					//Get the minimum time needed to explode
					//Remove "Element" from the name
					int threshold;
					switch(ElementName){
						//Alkali metals
						case Element.Lithium:
							threshold = 8 * 60;
							break;
						case Element.Sodium:
							threshold = 5 * 60;
							break;
						case Element.Potassium:
							threshold = 4 * 60;
							break;
						case Element.Rubidium:
							threshold = 3 * 60;
							break;
						case Element.Caesium:
							threshold = 2 * 60;
							break;
						case Element.Francium:
							threshold = 45;
							break;
						//Alkaline Earth metals
						case Element.Beryllium:
							threshold = 11 * 60;
							break;
						case Element.Magnesium:
							threshold = 9 * 60;
							break;
						case Element.Calcium:
							threshold = 6 * 60;
							break;
						case Element.Strontium:
							threshold = 5 * 60;
							break;
						case Element.Barium:
							threshold = 4 * 60;
							break;
						case Element.Radium:
							threshold = 3 * 60;
							break;
						default:
							throw new InvalidFamilyException(displayName, Family);
					}

					//Spawn some "gas" bubbles
					if(Main.rand.NextFloat() < (ReactionTimer / (float)threshold) * 0.75f)
						TerraScience.NewElementGasDust(item.position, item.width, item.height, Color.White, new Vector2(0, -3));

					//The threshold has been met.  Cause an explosion!
					if(ReactionTimer >= threshold){
						if(item.stack > 1){
							//If there's more than one item in this stack, reduce the stack and the timer
							int oldStack = item.stack;
							item.stack--;
							ReactionTimer = (int)(ReactionTimer * (float)item.stack / oldStack);
						}else{
							//Otherwise, make the item despawn (and make its stack to 0 for good measure)
							item.stack = 0;
							item.active = false;
						}

						Projectile p = Projectile.NewProjectileDirect(item.Center, Vector2.Zero, ProjectileID.Grenade, (int)(300 / (float)threshold * 120), 8f, Main.myPlayer);
						//Force the explosion from the grenade to happen NOW
						p.timeLeft = 3;
					}
				}else
					ReactionTimer = 0;
				// TODO:  make reaction happen slower if it's in the air?
			}

			UpdateStates();
		}

		public override bool OnPickup(Player player){
			//Reset the reaction timer
			ReactionTimer = 0;

			return true;
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

			// TODO:  fancy shit
			return true;
		}
	}
}
