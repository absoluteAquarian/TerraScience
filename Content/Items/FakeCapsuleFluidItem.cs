using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Utilities;

namespace TerraScience.Content.Items{
    [Autoload(false)]

    public class FakeCapsuleFluidItem : BrazilOnTouchItem{
		internal static Dictionary<int, MachineFluidID> containerTypes;

		public MachineFluidID Fluid => containerTypes.TryGetValue(Item.type, out var type) ? type : MachineFluidID.None;

		public override string Texture => "TerraScience/Content/Items/FakeCapsuleFluidItem";

        protected override bool CloneNewInstances => true;

        public override string Name { get; }

        public FakeCapsuleFluidItem() { }

        public FakeCapsuleFluidItem(string nameOverride) {
            Name = nameOverride ?? base.Name;
        }

        public override void SetStaticDefaults(){
			DisplayName.SetDefault(Fluid.ProperEnumName());
		}

		public override void SetDefaults(){
			Item.width = 16;
			Item.height = 16;

			Item.maxStack = 999;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale){
			Draw(spriteBatch, position, drawColor, origin, 0f, scale);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			Draw(spriteBatch, Item.Center - Main.screenPosition, lightColor, Item.Size / 2f, rotation, scale);
			return false;
		}

		private void Draw(SpriteBatch spriteBatch, Vector2 position, Color lightColor, Vector2 origin, float rotation, float scale){
			Color color = Fluid != MachineFluidID.None
					? Capsule.GetBackColor(Fluid)
					: throw new Exception();

			color = MiscUtils.MixLightColors(lightColor, color);

			spriteBatch.Draw(TextureAssets.Item[Item.type].Value, position, null, color, rotation, origin, scale, SpriteEffects.None, 0);
		}
	}
}
