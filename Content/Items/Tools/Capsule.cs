﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Tools{
	[Autoload(false)]
	public class Capsule : ModItem{
		internal static Dictionary<int, MachineGasID> containerTypes;

		public MachineGasID GasType => containerTypes[Item.type];

		public override string Texture => "TerraScience/Content/Items/Tools/Capsule";

		public override string Name{ get; }

		public Capsule(){ }

		public Capsule(string nameOverride){
			Name = nameOverride ?? base.Name;
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(GasType != MachineGasID.None ? "Capsule: " + GasType.ProperEnumName() : "Empty Capsule");
			Tooltip.SetDefault("A basic capsule able to store gases");
		}

		public override void SetDefaults(){
			Item.width = 20;
			Item.height = 32;
			Item.scale = 0.6f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 3);
			Item.maxStack = 999;
		}

		public override void AddRecipes(){
			if(GasType == MachineGasID.None){
				Recipe recipe = CreateRecipe(5);
				recipe.AddIngredient(ItemID.TinBar, 2);
				recipe.AddIngredient(ItemID.Glass, 1);
				recipe.Register();
			}
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
			DrawBack(spriteBatch, position, drawColor, origin, 0f, scale);
			return true;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			DrawBack(spriteBatch, Item.Center - Main.screenPosition, lightColor, Item.Size / 2f, rotation, scale);
			return true;
		}

		private void DrawBack(SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 origin, float rotation, float scale){
			if(GasType == MachineGasID.None)
				return;

			var texture = ModContent.Request<Texture2D>("TerraScience/Content/Items/Tools/capsule back").Value;

			Color gasColor = GetBackColor(GasType);

			spriteBatch.Draw(texture, position, null, MiscUtils.MixLightColors(color, gasColor), rotation, origin, scale, SpriteEffects.None, 0);
		}

		public static Color GetBackColor(MachineGasID id){
			switch(id){
				case MachineGasID.Hydrogen:
					return Color.Orange;
				case MachineGasID.Oxygen:
					return Color.LightBlue;
				case MachineGasID.Chlorine:
					return Color.LimeGreen;
				default:
					throw new Exception("Invalid Gas ID requested: " + id.ToString());
			}
		}

		public static Color GetBackColor(MachineLiquidID id){
			switch(id){
				case MachineLiquidID.Water:
					return new Color(){ PackedValue = 0xffbf3d09 };
				case MachineLiquidID.Saltwater:
					return new Color(){ PackedValue = 0xff8e9107 };
				case MachineLiquidID.Lava:
					return new Color(){ PackedValue = 0xff0320fd };
				case MachineLiquidID.Honey:
					return new Color(){ PackedValue = 0xff0c9cff };
				default:
					throw new Exception("Invalid Liquid ID requested: " + id.ToString());
			}
		}
	}
}
