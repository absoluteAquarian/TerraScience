using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Utilities;

namespace TerraScience.Content.Items{
	public class FakeCapsuleFluidItem : BrazilOnTouchItem{
		internal static Dictionary<int, (MachineGasID?, MachineLiquidID?)> containerTypes;

		public MachineGasID? Gas => containerTypes[item.type].Item1;
		public MachineLiquidID? Liquid => containerTypes[item.type].Item2;

		public override bool Autoload(ref string name) => false;

		public override string Texture => "TerraScience/Content/Items/FakeCapsuleFluidItem";

		public override bool CloneNewInstances => true;

		public FakeCapsuleFluidItem(){ }

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(Gas?.ProperEnumName() ?? Liquid?.ProperEnumName() ?? throw new Exception("Item instance was not set to a valid liquid or gas ID"));
		}

		public override void SetDefaults(){
			item.width = 16;
			item.height = 16;

			item.maxStack = 999;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale){
			Draw(spriteBatch, position, drawColor, origin, 0f, scale);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			Draw(spriteBatch, item.Center - Main.screenPosition, lightColor, item.Size / 2f, rotation, scale);
			return false;
		}

		private void Draw(SpriteBatch spriteBatch, Vector2 position, Color lightColor, Vector2 origin, float rotation, float scale){
			Color color = Gas is MachineGasID gasID
				? Capsule.GetBackColor(gasID)
				: Liquid is MachineLiquidID liquidID
					? Capsule.GetBackColor(liquidID)
					: throw new Exception();

			color = MiscUtils.MixLightColors(lightColor, color);

			spriteBatch.Draw(Main.itemTexture[item.type], position, null, color, rotation, origin, scale, SpriteEffects.None, 0);
		}
	}
}
