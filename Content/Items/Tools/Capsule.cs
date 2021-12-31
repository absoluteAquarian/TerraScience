using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Tools{
	public class Capsule : ModItem{
		internal static Dictionary<int, MachineFluidID> fluidContainerTypes;

		public MachineFluidID FluidType => fluidContainerTypes.TryGetValue(item.type, out var gas) ? gas : MachineFluidID.None;

		public override bool Autoload(ref string name) => false;

		public override string Texture => "TerraScience/Content/Items/Tools/Capsule";

		public override bool CloneNewInstances => true;

		public Capsule(){ }

		public override void SetStaticDefaults(){
			var fluid = FluidType;

			DisplayName.SetDefault(fluid != MachineFluidID.None
					? "Capsule: " + fluid.ProperEnumName()
					: "Empty Capsule");
			Tooltip.SetDefault("A basic capsule able to store gases or liquids");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 32;
			item.scale = 0.6f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 3);
			item.maxStack = 999;
		}

		public override void AddRecipes(){
			if(FluidType == MachineFluidID.None){
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.TinBar, 2);
				recipe.AddIngredient(ItemID.Glass, 1);
				recipe.SetResult(this, 5);
				recipe.AddRecipe();
			}
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
			DrawBack(spriteBatch, position, drawColor, origin, 0f, scale);
			return true;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			DrawBack(spriteBatch, item.Center - Main.screenPosition, lightColor, item.Size / 2f, rotation, scale);
			return true;
		}

		private void DrawBack(SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 origin, float rotation, float scale){
			var fluid = FluidType;

			if(fluid == MachineFluidID.None)
				return;

			var texture = ModContent.GetTexture("TerraScience/Content/Items/Tools/capsule back");

			Color gasColor = GetBackColor(fluid);

			spriteBatch.Draw(texture, position, null, MiscUtils.MixLightColors(color, gasColor), rotation, origin, scale, SpriteEffects.None, 0);
		}

		public static Color GetBackColor(MachineFluidID id){
			switch(id){
				case MachineFluidID.None:
					return Color.Transparent;
				case MachineFluidID.LiquidWater:
					return new Color(){ PackedValue = 0xffbf3d09 };
				case MachineFluidID.LiquidSaltwater:
					return new Color(){ PackedValue = 0xff8e9107 };
				case MachineFluidID.LiquidLava:
					return new Color(){ PackedValue = 0xff0320fd };
				case MachineFluidID.LiquidHoney:
					return new Color(){ PackedValue = 0xff0c9cff };
				case MachineFluidID.HydrogenGas:
					return Color.Orange;
				case MachineFluidID.OxygenGas:
					return Color.LightBlue;
				case MachineFluidID.ChlorineGas:
					return Color.LimeGreen;
				default:
					throw new Exception("Invalid Fluid ID requested: " + id.ToString());
			}
		}
	}
}
