using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Items{
	public class ElementItem : ScienceItem{
		private Action<ModRecipe, ElementItem> ItemRecipe => TerraScience.CachedElementRecipes[Name];
		private Action<Item> ItemDefaults => TerraScience.CachedElementDefaults[Name];

		public ElementFamily Family{ get; private set; } = ElementFamily.None;
		public Element ElementName{ get; private set; } = Element.Hydrogen;

		//Useless, but it's required for the mod to load.
		public ElementItem() : base(){ }

		/// <summary>
		/// Creates a new element item.
		/// </summary>
		public ElementItem(Element name, string description, ElementState defaultState, ElementFamily family, Color gasColor, bool isPlaceableBar, ModLiquid liquid, float boilingPoint, float meltingPoint) : base(description, defaultState, gasColor, isPlaceableBar, liquid, boilingPoint, meltingPoint){
			ElementName = name;
			displayName = TerraScience.ElementName(name, false);
			Family = family;
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
			if(CurrentState == ElementState.Gas){
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
					switch(ElementName){
						//Alkali metals
						case Element.Lithium:
							reactionTimerMax = 8 * 60;
							break;
						case Element.Sodium:
							reactionTimerMax = 5 * 60;
							break;
						case Element.Potassium:
							reactionTimerMax = 4 * 60;
							break;
						case Element.Rubidium:
							reactionTimerMax = 3 * 60;
							break;
						case Element.Caesium:
							reactionTimerMax = 2 * 60;
							break;
						case Element.Francium:
							reactionTimerMax = 45;
							break;
						//Alkaline Earth metals
						case Element.Beryllium:
							reactionTimerMax = 11 * 60;
							break;
						case Element.Magnesium:
							reactionTimerMax = 9 * 60;
							break;
						case Element.Calcium:
							reactionTimerMax = 6 * 60;
							break;
						case Element.Strontium:
							reactionTimerMax = 5 * 60;
							break;
						case Element.Barium:
							reactionTimerMax = 4 * 60;
							break;
						case Element.Radium:
							reactionTimerMax = 3 * 60;
							break;
						default:
							throw new InvalidFamilyException(displayName, Family);
					}

					//Spawn some "gas" bubbles
					if(Main.rand.NextFloat() < ReactionTimer / (float)reactionTimerMax * 0.75f)
						TerraScience.NewElementGasDust(item.position, item.width, item.height, Color.White, new Vector2(0, -3));

					//The threshold has been met.  Cause an explosion!
					if(ReactionTimer >= reactionTimerMax){
						TerraScience.HandleWaterReaction(this);

						Projectile p = Projectile.NewProjectileDirect(item.Center, Vector2.Zero, ProjectileID.Grenade, (int)(300 / (float)reactionTimerMax * 120), 8f, Main.myPlayer);
						//Force the explosion from the grenade to happen NOW
						p.timeLeft = 3;
					}
				}else{
					//We aren't moist.  Instead, make the oxides
					//Reaction takes longer and the effect is different
					ReactionTimer += Main.rand.Next(4);

					//Get the minimum time needed to react
					switch(ElementName){
						//Alkali metals
						case Element.Lithium:
							reactionTimerMax = 12 * 60;
							break;
						case Element.Sodium:
							reactionTimerMax = 11 * 60;
							break;
						case Element.Potassium:
							reactionTimerMax = 9 * 60;
							break;
						case Element.Rubidium:
							reactionTimerMax = 8 * 60;
							break;
						case Element.Caesium:
							reactionTimerMax = 7 * 60;
							break;
						case Element.Francium:
							reactionTimerMax = 5 * 60;
							break;
						//Alkaline Earth metals
						case Element.Beryllium:
							reactionTimerMax = 14 * 60;
							break;
						case Element.Magnesium:
							reactionTimerMax = 13 * 60;
							break;
						case Element.Calcium:
							reactionTimerMax = 11 * 60;
							break;
						case Element.Strontium:
							reactionTimerMax = 9 * 60;
							break;
						case Element.Barium:
							reactionTimerMax = 8 * 60;
							break;
						case Element.Radium:
							reactionTimerMax = 6 * 60;
							break;
						default:
							throw new InvalidFamilyException(displayName, Family);
					}

					//Spawn some "gas" bubbles
					if(Main.rand.NextFloat() < ReactionTimer / (float)reactionTimerMax * 0.75f)
						TerraScience.NewElementGasDust(item.position, item.width, item.height, Color.White);

					if(ReactionTimer >= reactionTimerMax){
						//Handle the air reaction
						TerraScience.HandleAirReaction(this);
					}
				}
			}

			base.UpdateStates();
		}
	}
}
