using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraScience.Utilities{
	public static class DrawingUtils{
		//Yoinked and edited from https://github.com/Eternal-Team/BaseLibrary/blob/1.3/Utility/RenderingUtility.cs
		public static void DrawItemInWorld(this SpriteBatch spriteBatch, Item item, Vector2 position, Vector2 size, float rotation = 0f){
			if(!item.IsAir){
				Texture2D itemTexture = TextureAssets.Item[item.type].Value;
				Rectangle rect = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTexture) : itemTexture.Frame();
				Color newColor = Color.White;
				float pulseScale = 1f;
				ItemSlot.GetItemLight(ref newColor, ref pulseScale, item, outInTheWorld: true);

				float availableWidth = size.X;
				int width = rect.Width;
				int height = rect.Height;
				float drawScale = 1f;
				if(width > availableWidth || height > availableWidth){
					if(width > height)
						drawScale = availableWidth / width;
					else
						drawScale = availableWidth / height;
				}

				Vector2 origin = rect.Size() * 0.5f;

				float totalScale = pulseScale * drawScale;

				if(ItemLoader.PreDrawInWorld(item, spriteBatch, item.GetColor(Color.White), item.GetAlpha(newColor), ref rotation, ref totalScale, item.whoAmI)){
					spriteBatch.Draw(itemTexture, position, rect, item.GetAlpha(newColor), rotation, origin, totalScale, SpriteEffects.None, 0f);

					if(item.color != Color.Transparent)
						spriteBatch.Draw(itemTexture, position, rect, item.GetColor(Color.White), rotation, origin, totalScale, SpriteEffects.None, 0f);
				}

				ItemLoader.PostDrawInWorld(item, spriteBatch, item.GetColor(Color.White), item.GetAlpha(newColor), rotation, totalScale, item.whoAmI);

				if(ItemID.Sets.TrapSigned[item.type])
					spriteBatch.Draw(TextureAssets.Wire.Value, position + new Vector2(40f, 40f) * drawScale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), drawScale, SpriteEffects.None, 0f);
			}
		}
	}
}
